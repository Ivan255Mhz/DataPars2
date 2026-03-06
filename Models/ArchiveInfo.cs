using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class ArchiveInfo
{
    public int Id { get; set; }

    public string ArchiveName { get; set; } = null!;

    public int? MeasureIntervalValue { get; set; }

    public int? MeasureIntervalUnitId { get; set; }

    public int StoragePeriodValue { get; set; }

    public int StoragePeriodUnitId { get; set; }

    public virtual TimeUnit? MeasureIntervalUnit { get; set; }

    public virtual TimeUnit StoragePeriodUnit { get; set; } = null!;
}
