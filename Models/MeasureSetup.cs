using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class MeasureSetup
{
    public int Id { get; set; }

    public int MonitoringPointId { get; set; }

    public int ParamGroupId { get; set; }

    public int RegisterAddressId { get; set; }

    public int EndiansId { get; set; }

    public virtual ICollection<ArchiveLevel0> ArchiveLevel0s { get; set; } = new List<ArchiveLevel0>();

    public virtual ICollection<ArchiveLevel1> ArchiveLevel1s { get; set; } = new List<ArchiveLevel1>();

    public virtual ICollection<ArchiveLevel2> ArchiveLevel2s { get; set; } = new List<ArchiveLevel2>();

    public virtual ICollection<ArchiveLevel3> ArchiveLevel3s { get; set; } = new List<ArchiveLevel3>();

    public virtual ICollection<ArchiveLevel4> ArchiveLevel4s { get; set; } = new List<ArchiveLevel4>();

    public virtual Endian Endians { get; set; } = null!;

    public virtual MonitoringPoint MonitoringPoint { get; set; } = null!;

    public virtual ParametersGroup ParamGroup { get; set; } = null!;

    public virtual RegisterAddress RegisterAddress { get; set; } = null!;
}
