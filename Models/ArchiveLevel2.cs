using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class ArchiveLevel2
{
    public int Id { get; set; }

    public int MeasureSetupId { get; set; }

    public DateTime Time { get; set; }

    public double ValueAvg { get; set; }

    public double ValueMax { get; set; }

    public double Deviation { get; set; }

    public int Counts { get; set; }

    public int? ModeWorkId { get; set; }

    public virtual MeasureSetup MeasureSetup { get; set; } = null!;

    public virtual ModeWork? ModeWork { get; set; }
}
