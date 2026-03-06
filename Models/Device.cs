using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class Device
{
    public int Id { get; set; }

    public int Number { get; set; }

    public int TypeId { get; set; }

    public string? IpAddress { get; set; }

    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();

    public virtual TypeOfDevice Type { get; set; } = null!;
}
