using DataPars.Data;
using DataPars.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

// Конфигурация
string server = "BELYAEV\\DREAM";
string userId = "sa";
string password = "Basepwd#0000";
string database = "Metro_SKZ_Archive_Put_ES01";
string BaseOutputPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
    "ADC_Exports"
);

string connectionString = $"Server={server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;";

var optionsBuilder = new DbContextOptionsBuilder<MetroSkzArchiveGorEs01Context>();
optionsBuilder.UseSqlServer(connectionString);

using var context = new MetroSkzArchiveGorEs01Context(optionsBuilder.Options);

Console.WriteLine($" Загрузка конфигураций измерений из базы: {database}");

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

// Используем имя базы данных как название папки первого уровня
string dbFolderName = SanitizeForFileName(database);
string dbFolderPath = Path.Combine(BaseOutputPath, dbFolderName);

Console.WriteLine($" Папка для экспорта: {dbFolderPath}");
Console.WriteLine();

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
        int count = dataInfo.Count;
        int dataLength = count * 4;

        // Формируем имя файла по новому формату
        string pointNumber = ExtractNumber(point.ControlPointName);
        string paramName = SanitizeForFileName(param.ParameterName);
        string freqName = freq != null ? SanitizeForFileName(freq.Frequency1) : "NoFreq";

        string fileName = $"ESC{asset.Number}_Point{pointNumber}_{paramName}_{freqName}";
        string safeFileName = SanitizeForFileName(fileName);

        // Создаем структуру папок: База данных → Тип эскалатора → Эскалатор
        string typeFolder = Path.Combine(dbFolderPath, device.Type?.TypeName ?? "Unknown");
        string escalatorFolder = Path.Combine(typeFolder, $"Эскалатор_№{asset.Number}");

        Directory.CreateDirectory(escalatorFolder);

        string binPath = Path.Combine(escalatorFolder, safeFileName + ".bin");
        string tsxmlPath = Path.Combine(escalatorFolder, safeFileName + ".tsxml");

        // Проверяем, не существует ли уже файл
        if (File.Exists(binPath) || File.Exists(tsxmlPath))
        {
            Console.WriteLine($" Файл уже существует: {fileName}, пропускаем...");
            continue;
        }

        Console.Write($" {fileName}... ");

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
            frequency: 2046,  
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
Console.WriteLine("   ЭКСПОРТ ЗАВЕРШЕН");
Console.WriteLine($"   База данных: {database}");
Console.WriteLine($"   Папка: {dbFolderPath}");
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

    
    string unitLower = unit.ToLower();

    // Тахометр (обороты в минуту)
    if (unitLower.Contains("гц") || unitLower.Contains("гц") ||
        unitLower.Contains("об") || unitLower.Contains("оборотов"))
    {
        return "об/мин";
    }

    // Частота (герцы)
    if (unitLower.Contains("гц") || unitLower.Contains("hz"))
    {
        return "Гц";
    }

    // Вибрация (м/с²)
    if (unitLower.Contains("м/с") || unitLower.Contains("м/с2") ||
        unitLower.Contains("м/с²") || unitLower.Contains("m/s"))
    {
        return "м/с&#178;";  // HTML мнемоника для квадрата
    }

    // Температура
    if (unitLower.Contains("°c") || unitLower.Contains("°с") ||
        unitLower.Contains("c°") || unitLower.Contains("с°"))
    {
        return "&#176;C";  
    }

    // Если ничего не подошло, возвращаем оригинал с заменой спецсимволов
    return unit
        .Replace("²", "&#178;")
        .Replace("2", "&#178;")
        .Replace("°", "&#176;");
}