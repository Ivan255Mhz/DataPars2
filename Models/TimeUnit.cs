using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class TimeUnit
{
    public int Id { get; set; }

    public string UnitName { get; set; } = null!;

    public virtual ICollection<ArchiveInfo> ArchiveInfoMeasureIntervalUnits { get; set; } = new List<ArchiveInfo>();

    public virtual ICollection<ArchiveInfo> ArchiveInfoStoragePeriodUnits { get; set; } = new List<ArchiveInfo>();
}
