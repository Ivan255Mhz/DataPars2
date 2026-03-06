using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class Frequency
{
    public int Id { get; set; }

    public string Frequency1 { get; set; } = null!;

    public virtual ICollection<ParametersGroup> ParametersGroups { get; set; } = new List<ParametersGroup>();
}
