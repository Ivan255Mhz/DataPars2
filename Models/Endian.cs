using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class Endian
{
    public int Id { get; set; }

    public string? Endians { get; set; }

    public virtual ICollection<MeasureSetup> MeasureSetups { get; set; } = new List<MeasureSetup>();
}
