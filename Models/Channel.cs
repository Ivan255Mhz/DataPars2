using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class Channel
{
    public int Id { get; set; }

    public int Channel1 { get; set; }

    public int DeviceId { get; set; }

    public virtual Device Device { get; set; } = null!;

    public virtual ICollection<MonitoringPoint> MonitoringPoints { get; set; } = new List<MonitoringPoint>();
}
