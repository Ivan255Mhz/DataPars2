using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class ControlPointsInAsset
{
    public int Id { get; set; }

    public int AssetId { get; set; }

    public int ControlPointId { get; set; }

    public virtual Asset Asset { get; set; } = null!;

    public virtual ControlPoint ControlPoint { get; set; } = null!;

    public virtual ICollection<MonitoringPoint> MonitoringPoints { get; set; } = new List<MonitoringPoint>();
}
