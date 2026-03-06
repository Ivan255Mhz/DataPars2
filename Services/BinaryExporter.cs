using DataPars.Data;
using Microsoft.EntityFrameworkCore;

namespace DataPars.Services;

public class BinaryExporter
{
    private readonly MetroSkzArchiveGorEs01Context _context;

    public BinaryExporter(MetroSkzArchiveGorEs01Context context)
    {
        _context = context;
    }

    public async Task ExportMeasureSetupAsync(int measureSetupId, string outputPath)
    {
        var records = await _context.ArchiveLevel0s
            .Where(x => x.MeasureSetupId == measureSetupId)
            .OrderBy(x => x.Time)
            .Select(x => x.Value)
            .ToListAsync();

        if (!records.Any())
            return;

        // Конвертируем int в float
        float[] floatValues = records.Select(v => (float)v).ToArray();

        // Конвертируем в байты
        byte[] bytes = new byte[floatValues.Length * 4];
        Buffer.BlockCopy(floatValues, 0, bytes, 0, bytes.Length);

        // Сохраняем
        await File.WriteAllBytesAsync(outputPath, bytes);
    }
}