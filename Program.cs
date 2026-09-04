using System.Collections.Concurrent;
using DataPars;
using DataPars.Data;
using DataPars.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

// Подавляем спам TLS 1.0 предупреждений от SQL Server
AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.SuppressInsecureTLSWarning", true);

// Кодировка консоли — для корректного отображения эмодзи и кириллицы
Console.OutputEncoding = System.Text.Encoding.UTF8;

// ─────────────────────────────────────────────
//  КОНФИГУРАЦИЯ (из config.json рядом с exe)
// ─────────────────────────────────────────────
var config = Config.Load();
string server = config.Server;
string baseOutputPath = config.OutputPath;

// ─────────────────────────────────────────────
//  АВТО-ПОИСК БАЗ ДАННЫХ НА СЕРВЕРЕ
// ─────────────────────────────────────────────
const string dbPattern = "Metro_SKZ_Archive_%";

Console.WriteLine($"🔍 Поиск баз по шаблону: {dbPattern.Replace("%", "*")}");

List<string> databases = await DiscoverDatabasesAsync(config, dbPattern);

if (databases.Count == 0)
{
    Console.WriteLine("❌ Базы не найдены. Проверьте подключение и шаблон.");
    return;
}

Console.WriteLine($"✅ Найдено баз: {databases.Count}");
foreach (var db in databases)
    Console.WriteLine($"   • {db}");
Console.WriteLine();

// ─────────────────────────────────────────────
//  ИНТЕРАКТИВНЫЙ ВЫБОР БАЗ ДАННЫХ
// ─────────────────────────────────────────────
Console.WriteLine("Введите номера баз через пробел (или Enter = все):");
for (int i = 0; i < databases.Count; i++)
    Console.WriteLine($"  [{i + 1}] {databases[i]}");

string? input = Console.ReadLine()?.Trim();

if (!string.IsNullOrWhiteSpace(input))
{
    var indices = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(s => int.TryParse(s, out var n) ? n - 1 : -1)
        .Where(n => n >= 0 && n < databases.Count)
        .Distinct()
        .OrderBy(n => n)
        .ToList();

    databases = indices.Select(i => databases[i]).ToList();
    Console.WriteLine($"✅ Выбрано баз: {databases.Count}");
}
else
{
    Console.WriteLine($"✅ Обрабатываем все {databases.Count} баз");
}
Console.WriteLine();

// ─────────────────────────────────────────────
//  ВЫБОР ПАПКИ ЭКСПОРТА
// ─────────────────────────────────────────────
Console.WriteLine($"📁 Текущая папка вывода: {baseOutputPath}");
Console.WriteLine("Введите новый путь (или Enter = оставить):");
string? newPath = Console.ReadLine()?.Trim().Trim('"');

if (!string.IsNullOrWhiteSpace(newPath))
{
    if (!Directory.Exists(newPath))
    {
        Console.Write($"⚠️  Папка не существует. Создать? (y/Enter = да): ");
        string? confirm = Console.ReadLine()?.Trim().ToLower();
        if (confirm == "y" || confirm == "")
            Directory.CreateDirectory(newPath);
        else
        {
            Console.WriteLine("⛔ Используем прежний путь.");
            newPath = baseOutputPath;
        }
    }
    baseOutputPath = newPath;
}

Console.WriteLine($"✅ Экспорт в: {baseOutputPath}");
Console.WriteLine();

// ─────────────────────────────────────────────
//  НАСТРОЙКИ ПАРАЛЛЕЛИЗМА
//  При таймаутах — уменьшайте setupParallelism (4 → 2 → 1)
// ─────────────────────────────────────────────
int dbParallelism = config.DbParallelism;
int setupParallelism = config.SetupParallelism;

// ─────────────────────────────────────────────
//  СУММАРНАЯ СТАТИСТИКА
// ─────────────────────────────────────────────
int grandTotalFiles = 0;
int grandTotalRecords = 0;
int grandSkipped = 0;

var globalStopwatch = System.Diagnostics.Stopwatch.StartNew();

Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║        ЭКСПОРТ АРХИВНЫХ ДАННЫХ       ║");
Console.WriteLine($"║  Баз данных:       {databases.Count,-19}║");
Console.WriteLine($"║  Баз одновременно: {dbParallelism,-19}║");
Console.WriteLine($"║  Потоков на БД:    {setupParallelism,-19}║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.WriteLine();

var consoleLock = new object();
int dbsDone = 0;

// ─────────────────────────────────────────────
//  ПАРАЛЛЕЛЬНЫЙ ЦИКЛ ПО БАЗАМ
// ─────────────────────────────────────────────
await Parallel.ForEachAsync(databases,
    new ParallelOptions { MaxDegreeOfParallelism = dbParallelism },
    async (database, outerCt) =>
    {
        int myIndex = Interlocked.Increment(ref dbsDone);
        (string stationName, string escalatorType) = ParseDatabaseName(database);

        string connectionString = config.BuildConnectionString(database);

        lock (consoleLock)
        {
            Console.WriteLine($"┌─ [{myIndex}/{databases.Count}] {database}");
            Console.WriteLine($"│  📍 {stationName} / {escalatorType}");
        }

        var dbOptions = new DbContextOptionsBuilder<MetroSkzArchiveGorEs01Context>()
            .UseSqlServer(connectionString, sqlOpts =>
            {
                sqlOpts.CommandTimeout(600);
                sqlOpts.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            })
            .Options;

        // ── Проверка соединения ──
        try
        {
            await using var testCtx = new MetroSkzArchiveGorEs01Context(dbOptions);
            await testCtx.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            lock (consoleLock)
            {
                Console.WriteLine($"│  ❌ Не удалось подключиться: {ex.Message}");
                Console.WriteLine("└──────────────────────────────────────");
                Console.WriteLine();
            }
            return;
        }

        // ── Загрузка конфигураций ──
        List<DataPars.Models.MeasureSetup> setups;
        try
        {
            await using var mainCtx = new MetroSkzArchiveGorEs01Context(dbOptions);
            setups = await mainCtx.MeasureSetups
                .Include(ms => ms.ParamGroup)
                    .ThenInclude(pg => pg.Parameter)
                        .ThenInclude(p => p.Unit)
                .Include(ms => ms.ParamGroup)
                    .ThenInclude(pg => pg.Frequency)
                .Include(ms => ms.MonitoringPoint)
                    .ThenInclude(mp => mp.Channel)
                        .ThenInclude(ch => ch.Device)
                            .ThenInclude(d => d.Type)
                .Include(ms => ms.MonitoringPoint)
                    .ThenInclude(mp => mp.ControlPointInAssets)
                        .ThenInclude(cpa => cpa.Asset)
                .Include(ms => ms.MonitoringPoint)
                    .ThenInclude(mp => mp.ControlPointInAssets)
                        .ThenInclude(cpa => cpa.ControlPoint)
                .Where(ms => ms.MonitoringPoint.ControlPointInAssets != null)
                .AsSplitQuery()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            lock (consoleLock)
            {
                Console.WriteLine($"│  ❌ Ошибка загрузки данных: {ex.Message}");
                for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
                    Console.WriteLine($"│  └─ {inner.Message}");
                Console.WriteLine("└──────────────────────────────────────");
                Console.WriteLine();
            }
            return;
        }

        lock (consoleLock)
            Console.WriteLine($"│  ⏳ Конфигураций: {setups.Count}");

        // ── Сборщик каналов для ALL.tsxml (корень эскалатора) ──
        var allChannels = new ConcurrentDictionary<string, ConcurrentBag<ChannelInfo>>();

        // ── Сборщик каналов для ALL.tsxml (папки режимов) ──
        var allModeChannels = new ConcurrentDictionary<string, ConcurrentBag<ChannelInfo>>();

        // ── Параллельный экспорт setup-ов ──
        int dbFiles = 0;
        int dbRecords = 0;
        int dbProcessed = 0;
        var dbStopwatch = System.Diagnostics.Stopwatch.StartNew();

        await Parallel.ForEachAsync(setups,
            new ParallelOptions { MaxDegreeOfParallelism = setupParallelism },
            async (setup, ct) =>
            {
                await using var ctx = new MetroSkzArchiveGorEs01Context(dbOptions);
                var exporter = new BinaryExporter(ctx);

                try
                {
                    var asset = setup.MonitoringPoint.ControlPointInAssets.Asset;
                    var point = setup.MonitoringPoint.ControlPointInAssets.ControlPoint;
                    var param = setup.ParamGroup.Parameter;
                    var freq = setup.ParamGroup.Frequency;
                    var device = setup.MonitoringPoint.Channel.Device;
                    var channelNum = setup.MonitoringPoint.Channel.Channel1;

                    var (startTime, count) = await exporter.GetMetaAsync(setup.Id);

                    if (count == 0)
                    {
                        Interlocked.Increment(ref dbProcessed);
                        Interlocked.Increment(ref grandSkipped);
                        return;
                    }

                    int dataLength = count * 4;

                    string pointNumber = ExtractNumber(point.ControlPointName);
                    string paramName = SanitizeForFileName(param.ParameterName);
                    string freqName = freq != null ? SanitizeForFileName(freq.Frequency1) : "NoFreq";
                    string channelName = $"ESC{asset.Number}_{point.ControlPointName}_{param.ParameterName}";
                    string fileName = $"ESC{asset.Number}_Point{pointNumber}_{paramName}_{freqName}_ID{setup.Id}";
                    string safeFileName = SanitizeForFileName(fileName);

                    bool isTacho = IsTachometer(param.Unit?.UnitName);
                    float scale = isTacho ? (1f / 60f) : 1.0f;

                    if (isTacho)
                        lock (consoleLock)
                            Console.WriteLine($"│  🔄 Тахометр: [{param.Unit?.UnitName}] → Гц (÷60), Setup {setup.Id}");

                    string escalatorFolder = Path.Combine(
                        baseOutputPath, stationName, escalatorType,
                        $"Эскалатор №{asset.Number}");

                    Directory.CreateDirectory(escalatorFolder);

                    string binPath = Path.Combine(escalatorFolder, safeFileName + ".bin");
                    string tsxmlPath = Path.Combine(escalatorFolder, safeFileName + ".tsxml");

                    string rawUnit = param.Unit?.UnitName ?? "";
                    string units = isTacho ? "Гц" : ConvertToHtmlMnemonic(rawUnit);
                    string comment = $"{asset.NameAsset} {point.ControlPointName}".Trim();
                    int freqHz = 2048;

                    // ── Основной экспорт (все данные) ──────────────────
                    await exporter.ExportMeasureSetupAsync(setup.Id, binPath, scale);

                    int actualDataLength = (int)new FileInfo(binPath).Length;
                    TsxmlGenerator.Create(tsxmlPath, Path.GetFileName(binPath),
                        startTime, actualDataLength, channelName, units, comment,
                        frequency: freqHz);

                    // ── Регистрируем канал данных для ALL.tsxml ────────
                    allChannels
                        .GetOrAdd(escalatorFolder, _ => new ConcurrentBag<ChannelInfo>())
                        .Add(new ChannelInfo
                        {
                            ChannelName = channelName,
                            DataFileName = safeFileName + ".bin",
                            Comment = comment,
                            DataLength = actualDataLength,
                            Units = units,
                            StartTime = startTime
                        });

                    // ── Экспорт канала режима работы ────────────────────
                    string modeChannelKey = $"{escalatorFolder}|mode";
                    string modeBinName = $"ESC{asset.Number}_ModeWork.bin";
                    string modeBinFile = Path.Combine(escalatorFolder, modeBinName);
                    string modeTsxmlFile = Path.Combine(escalatorFolder, $"ESC{asset.Number}_ModeWork.tsxml");

                    if (allChannels.TryAdd(modeChannelKey, new ConcurrentBag<ChannelInfo>()))
                    {
                        var (modeStart, modeCount) = await exporter.ExportModeChannelAsync(setup.Id, modeBinFile);

                        if (modeCount > 0)
                        {
                            int modeDataLength = (int)new FileInfo(modeBinFile).Length;

                            TsxmlGenerator.Create(modeTsxmlFile, modeBinName,
                                modeStart, modeDataLength,
                                $"ESC{asset.Number}_ModeWork",
                                units: "Режим",
                                comment: $"{asset.NameAsset} Режим работы: 0=Нет данных 1=Выключен 2=Подъём 3=Спуск",
                                frequency: freqHz,
                                sensorScale: "1,0",
                                sensorSensitivity: "1,0");

                            allChannels
                                .GetOrAdd(escalatorFolder, _ => new ConcurrentBag<ChannelInfo>())
                                .Add(new ChannelInfo
                                {
                                    ChannelName = $"ESC{asset.Number}_ModeWork",
                                    DataFileName = modeBinName,
                                    Comment = $"{asset.NameAsset} Режим работы",
                                    DataLength = modeCount * 4,
                                    Units = "Режим",
                                    StartTime = modeStart
                                });

                            lock (consoleLock)
                                Console.WriteLine($"│  🎛️ Режим работы: {modeCount:N0} зап. → {modeBinName}");
                        }
                    }

                    // ── Feature 1: экспорт по режимам ──────────────────
                    var modes = await exporter.GetModesAsync(setup.Id);

                    foreach (var (modeId, modeName, modeCount, modeStart) in modes)
                    {
                        string modeFolder = Path.Combine(escalatorFolder, SanitizeForFolderName(modeName));
                        Directory.CreateDirectory(modeFolder);

                        string modeBinPath = Path.Combine(modeFolder, safeFileName + ".bin");
                        string modeTsxmlPath = Path.Combine(modeFolder, safeFileName + ".tsxml");

                        await exporter.ExportMeasureSetupByModeAsync(setup.Id, modeBinPath, scale, modeId);

                        int actualModeBinLength = (int)new FileInfo(modeBinPath).Length;
                        TsxmlGenerator.Create(modeTsxmlPath, Path.GetFileName(modeBinPath),
                            modeStart, actualModeBinLength, channelName, units, comment,
                            frequency: freqHz);

                        allModeChannels
                            .GetOrAdd(modeFolder, _ => new ConcurrentBag<ChannelInfo>())
                            .Add(new ChannelInfo
                            {
                                ChannelName = channelName,
                                DataFileName = safeFileName + ".bin",
                                Comment = comment,
                                DataLength = modeCount * 4,
                                Units = units,
                                StartTime = modeStart
                            });
                    }

                    Interlocked.Add(ref dbRecords, count);
                    Interlocked.Increment(ref dbFiles);
                    int done = Interlocked.Increment(ref dbProcessed);

                    lock (consoleLock)
                        Console.WriteLine($"│  📤 [{done}/{setups.Count}] {fileName} ({count:N0} зап." +
                            (modes.Count > 0 ? $", {modes.Count} реж." : "") + ")");
                }
                catch (Exception ex)
                {
                    int done = Interlocked.Increment(ref dbProcessed);
                    lock (consoleLock)
                        Console.WriteLine($"│  ❌ [{done}/{setups.Count}] Setup {setup.Id}: {ex.Message}");
                }
            });

        // ── Feature 2: генерируем ALL.tsxml для каждого эскалатора ─────
        foreach (var (folder, channelBag) in allChannels)
        {
            try
            {
                var chList = channelBag
                    .OrderBy(c => c.ChannelName)
                    .ToList();

                if (chList.Count == 0) continue;

                DateTime overallStart = chList.Min(c => c.StartTime);
                string allTsxmlPath = Path.Combine(folder, "ALL.tsxml");

                TsxmlGenerator.CreateMultiChannel(allTsxmlPath, overallStart, chList, frequency: 2046);

                lock (consoleLock)
                    Console.WriteLine($"│  📋 ALL.tsxml: {chList.Count} кан. → {Path.GetRelativePath(baseOutputPath, allTsxmlPath)}");
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                    Console.WriteLine($"│  ❌ ALL.tsxml ошибка ({folder}): {ex.Message}");
            }
        }

        // ── Генерируем ALL.tsxml для каждой папки режима ──────────
        foreach (var (folder, channelBag) in allModeChannels)
        {
            try
            {
                var chList = channelBag
                    .OrderBy(c => c.ChannelName)
                    .ToList();

                if (chList.Count == 0) continue;

                DateTime overallStart = chList.Min(c => c.StartTime);
                string allTsxmlPath = Path.Combine(folder, "ALL.tsxml");

                TsxmlGenerator.CreateMultiChannel(allTsxmlPath, overallStart, chList, frequency: 2046);

                lock (consoleLock)
                    Console.WriteLine($"│  📋 ALL.tsxml режима: {chList.Count} кан. → {Path.GetRelativePath(baseOutputPath, allTsxmlPath)}");
            }
            catch (Exception ex)
            {
                lock (consoleLock)
                    Console.WriteLine($"│  ❌ ALL.tsxml режима ошибка ({folder}): {ex.Message}");
            }
        }

        dbStopwatch.Stop();
        Interlocked.Add(ref grandTotalFiles, dbFiles);
        Interlocked.Add(ref grandTotalRecords, dbRecords);

        lock (consoleLock)
        {
            Console.WriteLine($"│  ✅ Файлов: {dbFiles}  |  Записей: {dbRecords:N0}  |  {dbStopwatch.Elapsed.ToString(@"mm\:ss")}");
            Console.WriteLine("└──────────────────────────────────────");
            Console.WriteLine();
        }
    });

globalStopwatch.Stop();

// ─────────────────────────────────────────────
//  ИТОГ
// ─────────────────────────────────────────────
Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║          ИТОГОВАЯ СТАТИСТИКА         ║");
Console.WriteLine($"║  Всего файлов:   {grandTotalFiles,-20}║");
Console.WriteLine($"║  Всего записей:  {grandTotalRecords,-20:N0}║");
Console.WriteLine($"║  Пропущено:      {grandSkipped,-20}║");
Console.WriteLine($"║  Общее время:    {globalStopwatch.Elapsed.ToString(@"mm\:ss\.ff"),-20}║");
Console.WriteLine("╚══════════════════════════════════════╝");

// ─────────────────────────────────────────────
//  ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
// ─────────────────────────────────────────────
static string ExtractNumber(string? pointName)
{
    if (string.IsNullOrEmpty(pointName)) return "0";
    var match = System.Text.RegularExpressions.Regex.Match(pointName, @"\d+");
    return match.Success ? match.Value : "0";
}

static string SanitizeForFileName(string name)
{
    if (string.IsNullOrEmpty(name)) return "Unknown";
    foreach (char c in Path.GetInvalidFileNameChars())
        name = name.Replace(c, '_');
    return name;
}

static string SanitizeForFolderName(string name)
{
    if (string.IsNullOrEmpty(name)) return "Unknown";
    foreach (char c in Path.GetInvalidPathChars())
        name = name.Replace(c, '_');
    return name;
}

static string ConvertToHtmlMnemonic(string unit)
{
    if (string.IsNullOrEmpty(unit)) return "ед";
    return unit
        .Replace("²", "&#178;")
        .Replace("°", "&#176;");
}

static (string station, string type) ParseDatabaseName(string dbName)
{
    var parts = dbName.Split('_');
    string station = parts.Length > 3 ? TransliterateStation(parts[3]) : "Unknown";

    string type = "Unknown";
    if (parts.Length > 4)
    {
        bool part5isSuffix = parts.Length > 5 && !int.TryParse(parts[5], out _);
        type = part5isSuffix ? $"{parts[4]}_{parts[5]}" : parts[4];
    }

    return (station, type);
}

static string TransliterateStation(string name) => name switch
{
    "Gor" => "Горный",
    "Kaz" => "Казаковская",
    "Put" => "Путиловская",
    _ => name
};

static bool IsTachometer(string? unitName)
{
    if (string.IsNullOrEmpty(unitName)) return false;
    var u = unitName.ToLowerInvariant();
    return u.Contains("обр\\мин")
        || u.Contains("об\\мин")
        || u.Contains("об/мин")
        || u.Contains("обр/мин")
        || u.Contains("об.мин")
        || u.Contains("rpm")
        || u.Contains("r/min")
        || u.Contains("r\\min");
}

static async Task<List<string>> DiscoverDatabasesAsync(Config config, string pattern)
{
    string masterCs = config.BuildConnectionString("master");
    var result = new List<string>();

    try
    {
        await using var conn = new SqlConnection(masterCs);
        await conn.OpenAsync();

        const string sql = """
            SELECT name
            FROM   sys.databases
            WHERE  name LIKE @pattern
              AND  state_desc = 'ONLINE'
            ORDER BY name
            """;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@pattern", pattern);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(reader.GetString(0));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при поиске БД: {ex.Message}");
    }

    result.Sort();
    return result;
}