using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class TypeOfDevice
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
