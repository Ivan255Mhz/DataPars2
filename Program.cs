using DataPars.Data;
using DataPars.Services;

using Microsoft.EntityFrameworkCore;
using System.Text;

// Конфигурация
string server = "GYRDYMOV-NEW\\DREAM";
string userId = "sa";
string password = "Basepwd#0000";
string database = "Metro_SKZ_Archive_Gorniy_ES01_30_01_2026";
string baseOutputPath = @"C:\ADC_Exports";

string connectionString = $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;";

var optionsBuilder = new DbContextOptionsBuilder<MetroSkzArchiveGorEs01Context>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new MetroSkzArchiveGorEs01Context(optionsBuilder.Options);

Console.WriteLine(" Загрузка конфигураций измерений...");

var setups = await context.MeasureSetups
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
    .ToListAsync();

Console.WriteLine($" Найдено конфигураций: {setups.Count}");
Console.WriteLine();

int totalFiles = 0;
int totalRecords = 0;
var exporter = new BinaryExporter(context);

foreach (var setup in setups)
{
    try
    {
        var asset = setup.MonitoringPoint.ControlPointInAssets.Asset;
        var point = setup.MonitoringPoint.ControlPointInAssets.ControlPoint;
        var param = setup.ParamGroup.Parameter;
        var freq = setup.ParamGroup.Frequency;
        var device = setup.MonitoringPoint.Channel.Device;
        var channelNum = setup.MonitoringPoint.Channel.Channel1;

        // Получаем статистику по данным
        var dataInfo = await context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == setup.Id)
            .Select(x => new { x.Time, x.Value })
            .ToListAsync();

        if (!dataInfo.Any())
        {
            Console.WriteLine($" Пропуск {setup.Id}: нет данных");
            continue;
        }

        var startTime = dataInfo.Min(x => x.Time);
        var endTime = dataInfo.Max(x => x.Time);
        int count = dataInfo.Count;
        int dataLength = count * 4;

        // Формируем имя файла по новому формату
        string pointNumber = ExtractNumber(point.ControlPointName);
        string paramName = SanitizeForFileName(param.ParameterName);
        string freqName = freq != null ? SanitizeForFileName(freq.Frequency1) : "NoFreq";

        string fileName = $"ESC{asset.Number}_Point{pointNumber}_{paramName}_{freqName}";
        string safeFileName = SanitizeForFileName(fileName);

        // Создаем структуру папок: Станция → Тип эскалатора → Эскалатор
        string stationFolder = Path.Combine(baseOutputPath, "Горный"); // Из конфигурации
        string typeFolder = Path.Combine(stationFolder, device.Type?.TypeName ?? "Unknown");
        string escalatorFolder = Path.Combine(typeFolder, $"Эскалатор №{asset.Number}");

        Directory.CreateDirectory(escalatorFolder);

        string binPath = Path.Combine(escalatorFolder, safeFileName + ".bin");
        string tsxmlPath = Path.Combine(escalatorFolder, safeFileName + ".tsxml");

        Console.Write($"📤 {fileName}... ");

        // Экспортируем бинарные данные
        await exporter.ExportMeasureSetupAsync(setup.Id, binPath);

        // Конвертируем единицы измерения в HTML мнемонику
        string units = ConvertToHtmlMnemonic(param.Unit?.UnitName ?? "");

        // Формируем комментарий
        string comment = $"{asset.NameAsset} {point.ControlPointName}".Trim();

        // Создаем TSXML файл
        TsxmlGenerator.Create(
            tsxmlPath,
            Path.GetFileName(binPath),
            startTime,
            dataLength,
            $"Device_{device.Number}_Ch{channelNum}",  // ChannelName
            units,
            comment,
            frequency: 2,  // По умолчанию 2 Гц
            durationSeconds: count / 2
        );

        totalFiles++;
        totalRecords += count;
        Console.WriteLine($" ({count:N0} записей)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Ошибка: {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine(" ЭКСПОРТ ЗАВЕРШЕН");
Console.WriteLine($"   Файлов создано: {totalFiles}");
Console.WriteLine($"   Всего записей: {totalRecords:N0}");

// Вспомогательные методы
static string ExtractNumber(string pointName)
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

static string ConvertToHtmlMnemonic(string unit)
{
    if (string.IsNullOrEmpty(unit)) return "ед";
    return unit
        .Replace("²", "&#178;")
        .Replace("2", "&#178;")
        .Replace("°", "&#176;")
        .Replace("м/с", "м/с");
}