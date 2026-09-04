using DataPars.Data;
using Microsoft.EntityFrameworkCore;

namespace DataPars.Services;

public class BinaryExporter
{
    private readonly MetroSkzArchiveGorEs01Context _context;

    // 64К записей × 4 байта = 256 KB на чанк
    private const int ChunkSize = 64_000;

    public BinaryExporter(MetroSkzArchiveGorEs01Context context)
    {
        _context = context;
    }

    /// <summary>
    /// Метаданные всего setup-а: MIN(Time) + COUNT.
    /// </summary>
    public async Task<(DateTime startTime, int count)> GetMetaAsync(int measureSetupId)
    {
        var meta = await _context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == measureSetupId)
            .GroupBy(_ => 1)
            .Select(g => new { StartTime = g.Min(x => x.Time), Count = g.Count() })
            .FirstOrDefaultAsync();

        if (meta == null) return (DateTime.MinValue, 0);
        return (meta.StartTime, meta.Count);
    }

    /// <summary>
    /// Метаданные для конкретного режима работы.
    /// </summary>
    public async Task<(DateTime startTime, int count)> GetMetaByModeAsync(int measureSetupId, int modeWorkId)
    {
        var meta = await _context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == measureSetupId && x.ModeWorkId == modeWorkId)
            .GroupBy(_ => 1)
            .Select(g => new { StartTime = g.Min(x => x.Time), Count = g.Count() })
            .FirstOrDefaultAsync();

        if (meta == null) return (DateTime.MinValue, 0);
        return (meta.StartTime, meta.Count);
    }

    /// <summary>
    /// Возвращает список активных режимов для setup-а.
    /// Пропускает режимы 0 "Нет данных" и 1 "Эскалатор выключен".
    /// </summary>
    public async Task<List<(int modeId, string modeName, int count, DateTime startTime)>> GetModesAsync(int measureSetupId)
    {
        var modes = await _context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == measureSetupId
                     && x.ModeWorkId != null
                     && x.ModeWorkId > 1)           // пропускаем 0=Нет данных, 1=Выключен
            .GroupBy(x => new { x.ModeWorkId, x.ModeWork!.ModeworkName })
            .Select(g => new
            {
                ModeId    = g.Key.ModeWorkId!.Value,
                ModeName  = g.Key.ModeworkName ?? "Unknown",
                Count     = g.Count(),
                StartTime = g.Min(x => x.Time)
            })
            .OrderBy(m => m.ModeId)
            .ToListAsync();

        return modes.Select(m => (m.ModeId, m.ModeName, m.Count, m.StartTime)).ToList();
    }

    /// <summary>
    /// Стриминговый экспорт всех данных setup-а на диск.
    /// </summary>
    public async Task ExportMeasureSetupAsync(int measureSetupId, string outputPath, float scale = 1.0f)
    {
        var query = _context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == measureSetupId)
            .OrderBy(x => x.Time)
            .Select(x => x.Value)
            .AsAsyncEnumerable();

        await StreamToFileAsync(query, outputPath, scale);
    }

    /// <summary>
    /// Стриминговый экспорт данных только одного режима работы.
    /// </summary>
    public async Task ExportMeasureSetupByModeAsync(int measureSetupId, string outputPath, float scale, int modeWorkId)
    {
        var query = _context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == measureSetupId && x.ModeWorkId == modeWorkId)
            .OrderBy(x => x.Time)
            .Select(x => x.Value)
            .AsAsyncEnumerable();

        await StreamToFileAsync(query, outputPath, scale);
    }

    /// <summary>
    /// Экспортирует канал режима работы — значения ModeWorkId по времени.
    /// Каждое значение записывается как float (0=Нет данных, 1=Выключен, 2=Подъём, 3=Спуск).
    /// </summary>
    public async Task<(DateTime startTime, int count)> ExportModeChannelAsync(
        int measureSetupId, string outputPath)
    {
        // Пишем как float (Single) — значения 0,1,2,3 отображаются как есть
        // Units="Режим" — подпись оси Y
        var query = _context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == measureSetupId)
            .OrderBy(x => x.Time)
            .Select(x => new { x.Time, x.ModeWorkId })
            .AsAsyncEnumerable();

        await using var fs     = new FileStream(outputPath, FileMode.Create, FileAccess.Write,
                                                FileShare.None, bufferSize: 256 * 1024, useAsync: true);
        await using var writer = new BinaryWriter(fs);

        var buffer         = new float[ChunkSize];
        int bufPos         = 0;
        int count          = 0;
        DateTime startTime = DateTime.MinValue;

        await foreach (var row in query)
        {
            if (count == 0) startTime = row.Time;
            // ×1000 чтобы шкала была 0/1000/2000/3000 — иначе 0,1,2,3 не читаются на оси
            buffer[bufPos++] = row.ModeWorkId.HasValue ? (float)row.ModeWorkId.Value : 0f;
            count++;

            if (bufPos == ChunkSize)
            {
                WriteChunk(writer, buffer, bufPos);
                bufPos = 0;
            }
        }

        if (bufPos > 0)
            WriteChunk(writer, buffer, bufPos);

        return (startTime, count);
    }

    // ── Общая стриминговая запись ──────────────────────────────────────
    private static async Task StreamToFileAsync(IAsyncEnumerable<double> query, string outputPath, float scale)
    {
        await using var fs     = new FileStream(outputPath, FileMode.Create, FileAccess.Write,
                                                FileShare.None, bufferSize: 256 * 1024, useAsync: true);
        await using var writer = new BinaryWriter(fs);

        var buffer = new float[ChunkSize];
        int bufPos = 0;

        await foreach (var value in query)
        {
            buffer[bufPos++] = (float)value * scale;

            if (bufPos == ChunkSize)
            {
                WriteChunk(writer, buffer, bufPos);
                bufPos = 0;
            }
        }

        if (bufPos > 0)
            WriteChunk(writer, buffer, bufPos);
    }

    private static void WriteChunk(BinaryWriter writer, float[] buffer, int count)
    {
        var bytes = System.Runtime.InteropServices.MemoryMarshal
            .AsBytes(buffer.AsSpan(0, count));
        writer.Write(bytes);
    }
}
