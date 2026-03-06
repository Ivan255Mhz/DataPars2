using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class RegisterAddress
{
    public int Id { get; set; }

    public int? Address { get; set; }

    public virtual ICollection<MeasureSetup> MeasureSetups { get; set; } = new List<MeasureSetup>();
}
