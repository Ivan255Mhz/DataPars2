using System;
using System.Collections.Generic;

namespace DataPars.Models;

public partial class ModeWork
{
    public int Id { get; set; }

    public string? ModeworkName { get; set; }

    public virtual ICollection<ArchiveLevel0> ArchiveLevel0s { get; set; } = new List<ArchiveLevel0>();

    public virtual ICollection<ArchiveLevel1> ArchiveLevel1s { get; set; } = new List<ArchiveLevel1>();

    public virtual ICollection<ArchiveLevel2> ArchiveLevel2s { get; set; } = new List<ArchiveLevel2>();

    public virtual ICollection<ArchiveLevel3> ArchiveLevel3s { get; set; } = new List<ArchiveLevel3>();

    public virtual ICollection<ArchiveLevel4> ArchiveLevel4s { get; set; } = new List<ArchiveLevel4>();
}
