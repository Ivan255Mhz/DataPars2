using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class ArchiveLevel0
{
    public int Id { get; set; }

    public int MeasureSetupId { get; set; }

    public DateTime Time { get; set; }

    public double Value { get; set; }

    public int? ModeWorkId { get; set; }

    public virtual MeasureSetup MeasureSetup { get; set; } = null!;

    public virtual ModeWork? ModeWork { get; set; }
}
