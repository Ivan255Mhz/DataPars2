using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class Asset
{
    public int Id { get; set; }

    public int Number { get; set; }

    public string NameAsset { get; set; } = null!;

    public virtual ICollection<ControlPointsInAsset> ControlPointsInAssets { get; set; } = new List<ControlPointsInAsset>();
}
