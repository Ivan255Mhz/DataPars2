using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class ControlPoint
{
    public int Id { get; set; }

    public string? ControlPointName { get; set; }

    public virtual ICollection<ControlPointsInAsset> ControlPointsInAssets { get; set; } = new List<ControlPointsInAsset>();
}
