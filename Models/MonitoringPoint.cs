using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class MonitoringPoint
{
    public int Id { get; set; }

    public int ControlPointInAssetsId { get; set; }

    public int ChannelId { get; set; }

    public virtual Channel Channel { get; set; } = null!;

    public virtual ControlPointsInAsset ControlPointInAssets { get; set; } = null!;

    public virtual ICollection<MeasureSetup> MeasureSetups { get; set; } = new List<MeasureSetup>();
}
