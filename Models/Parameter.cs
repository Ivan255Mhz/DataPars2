using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class Parameter
{
    public int Id { get; set; }

    public string ParameterName { get; set; } = null!;

    public int? UnitId { get; set; }

    public virtual ICollection<ParametersGroup> ParametersGroups { get; set; } = new List<ParametersGroup>();

    public virtual MeasureUnit? Unit { get; set; }
}
