using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class MeasureUnit
{
    public int Id { get; set; }

    public string? UnitName { get; set; }

    public virtual ICollection<Parameter> Parameters { get; set; } = new List<Parameter>();
}
