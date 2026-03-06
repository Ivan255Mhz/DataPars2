using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class ParametersGroup
{
    public int Id { get; set; }

    public int? FrequencyId { get; set; }

    public int ParameterId { get; set; }

    public virtual Frequency? Frequency { get; set; }

    public virtual ICollection<MeasureSetup> MeasureSetups { get; set; } = new List<MeasureSetup>();

    public virtual Parameter Parameter { get; set; } = null!;
}
