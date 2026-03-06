using System;
using System.Collections.Generic;
using DataPars.Models;
using Microsoft.EntityFrameworkCore;

namespace DataPars.Data;

public partial class MetroSkzArchiveGorEs01Context : DbContext
{
    public MetroSkzArchiveGorEs01Context()
    {
    }

    public MetroSkzArchiveGorEs01Context(DbContextOptions<MetroSkzArchiveGorEs01Context> options)
        : base(options)
    {
    }

    public virtual DbSet<ArchiveInfo> ArchiveInfos { get; set; }

    public virtual DbSet<ArchiveLevel0> ArchiveLevel0s { get; set; }

    public virtual DbSet<ArchiveLevel1> ArchiveLevel1s { get; set; }

    public virtual DbSet<ArchiveLevel2> ArchiveLevel2s { get; set; }

    public virtual DbSet<ArchiveLevel3> ArchiveLevel3s { get; set; }

    public virtual DbSet<ArchiveLevel4> ArchiveLevel4s { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<ControlPoint> ControlPoints { get; set; }

    public virtual DbSet<ControlPointsInAsset> ControlPointsInAssets { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Endian> Endians { get; set; }

    public virtual DbSet<Frequency> Frequencies { get; set; }

    public virtual DbSet<MeasureSetup> MeasureSetups { get; set; }

    public virtual DbSet<MeasureUnit> MeasureUnits { get; set; }

    public virtual DbSet<ModeWork> ModeWorks { get; set; }

    public virtual DbSet<MonitoringPoint> MonitoringPoints { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<ParametersGroup> ParametersGroups { get; set; }

    public virtual DbSet<RegisterAddress> RegisterAddresses { get; set; }

    public virtual DbSet<TimeUnit> TimeUnits { get; set; }

    public virtual DbSet<TypeOfDevice> TypeOfDevices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchiveInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ArchiveI__3213E83FC3613EDA");

            entity.ToTable("ArchiveInfo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArchiveName)
                .HasMaxLength(70)
                .HasColumnName("archiveName");
            entity.Property(e => e.MeasureIntervalUnitId).HasColumnName("measureIntervalUnit_Id");
            entity.Property(e => e.MeasureIntervalValue).HasColumnName("measureIntervalValue");
            entity.Property(e => e.StoragePeriodUnitId).HasColumnName("storagePeriodUnit_Id");
            entity.Property(e => e.StoragePeriodValue).HasColumnName("storagePeriodValue");

            entity.HasOne(d => d.MeasureIntervalUnit).WithMany(p => p.ArchiveInfoMeasureIntervalUnits)
                .HasForeignKey(d => d.MeasureIntervalUnitId)
                .HasConstraintName("FK_ArchiveInfo_TimeUnit_Measure");

            entity.HasOne(d => d.StoragePeriodUnit).WithMany(p => p.ArchiveInfoStoragePeriodUnits)
                .HasForeignKey(d => d.StoragePeriodUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchiveInfo_TimeUnit_Storage");
        });

        modelBuilder.Entity<ArchiveLevel0>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ArchiveL__3213E83F12D24D87");

            entity.ToTable("ArchiveLevel0");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MeasureSetupId).HasColumnName("measureSetup_id");
            entity.Property(e => e.ModeWorkId).HasColumnName("modeWork_Id");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("time");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.MeasureSetup).WithMany(p => p.ArchiveLevel0s)
                .HasForeignKey(d => d.MeasureSetupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchiveLevel0_MeasureSetup");

            entity.HasOne(d => d.ModeWork).WithMany(p => p.ArchiveLevel0s)
                .HasForeignKey(d => d.ModeWorkId)
                .HasConstraintName("FK_ArchiveLevel0_ModeWork");
        });

        modelBuilder.Entity<ArchiveLevel1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ArchiveL__3213E83F833AC9EE");

            entity.ToTable("ArchiveLevel1");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Counts).HasColumnName("counts");
            entity.Property(e => e.Deviation).HasColumnName("deviation");
            entity.Property(e => e.MeasureSetupId).HasColumnName("measureSetup_id");
            entity.Property(e => e.ModeWorkId).HasColumnName("modeWork_Id");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("time");
            entity.Property(e => e.ValueAvg).HasColumnName("value_avg");
            entity.Property(e => e.ValueMax).HasColumnName("value_max");

            entity.HasOne(d => d.MeasureSetup).WithMany(p => p.ArchiveLevel1s)
                .HasForeignKey(d => d.MeasureSetupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchiveLevel1_MeasureSetup");

            entity.HasOne(d => d.ModeWork).WithMany(p => p.ArchiveLevel1s)
                .HasForeignKey(d => d.ModeWorkId)
                .HasConstraintName("FK_ArchiveLevel1_ModeWork");
        });

        modelBuilder.Entity<ArchiveLevel2>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ArchiveL__3213E83F1CA193C2");

            entity.ToTable("ArchiveLevel2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Counts).HasColumnName("counts");
            entity.Property(e => e.Deviation).HasColumnName("deviation");
            entity.Property(e => e.MeasureSetupId).HasColumnName("measureSetup_id");
            entity.Property(e => e.ModeWorkId).HasColumnName("modeWork_Id");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("time");
            entity.Property(e => e.ValueAvg).HasColumnName("value_avg");
            entity.Property(e => e.ValueMax).HasColumnName("value_max");

            entity.HasOne(d => d.MeasureSetup).WithMany(p => p.ArchiveLevel2s)
                .HasForeignKey(d => d.MeasureSetupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchiveLevel2_MeasureSetup");

            entity.HasOne(d => d.ModeWork).WithMany(p => p.ArchiveLevel2s)
                .HasForeignKey(d => d.ModeWorkId)
                .HasConstraintName("FK_ArchiveLevel2_ModeWork");
        });

        modelBuilder.Entity<ArchiveLevel3>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ArchiveL__3213E83F631E9C0A");

            entity.ToTable("ArchiveLevel3");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Counts).HasColumnName("counts");
            entity.Property(e => e.Deviation).HasColumnName("deviation");
            entity.Property(e => e.MeasureSetupId).HasColumnName("measureSetup_id");
            entity.Property(e => e.ModeWorkId).HasColumnName("modeWork_Id");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("time");
            entity.Property(e => e.ValueAvg).HasColumnName("value_avg");
            entity.Property(e => e.ValueMax).HasColumnName("value_max");

            entity.HasOne(d => d.MeasureSetup).WithMany(p => p.ArchiveLevel3s)
                .HasForeignKey(d => d.MeasureSetupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchiveLevel3_MeasureSetup");

            entity.HasOne(d => d.ModeWork).WithMany(p => p.ArchiveLevel3s)
                .HasForeignKey(d => d.ModeWorkId)
                .HasConstraintName("FK_ArchiveLevel3_ModeWork");
        });

        modelBuilder.Entity<ArchiveLevel4>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ArchiveL__3213E83FA529C810");

            entity.ToTable("ArchiveLevel4");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Counts).HasColumnName("counts");
            entity.Property(e => e.Deviation).HasColumnName("deviation");
            entity.Property(e => e.MeasureSetupId).HasColumnName("measureSetup_id");
            entity.Property(e => e.ModeWorkId).HasColumnName("modeWork_Id");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("time");
            entity.Property(e => e.ValueAvg).HasColumnName("value_avg");
            entity.Property(e => e.ValueMax).HasColumnName("value_max");

            entity.HasOne(d => d.MeasureSetup).WithMany(p => p.ArchiveLevel4s)
                .HasForeignKey(d => d.MeasureSetupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ArchiveLevel4_MeasureSetup");

            entity.HasOne(d => d.ModeWork).WithMany(p => p.ArchiveLevel4s)
                .HasForeignKey(d => d.ModeWorkId)
                .HasConstraintName("FK_ArchiveLevel4_ModeWork");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asset__3213E83FB56D6EFB");

            entity.ToTable("Asset");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NameAsset)
                .HasMaxLength(100)
                .HasColumnName("nameAsset");
            entity.Property(e => e.Number).HasColumnName("number");
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Channels__3213E83F068E6454");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Channel1).HasColumnName("channel");
            entity.Property(e => e.DeviceId).HasColumnName("device_Id");

            entity.HasOne(d => d.Device).WithMany(p => p.Channels)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Channels_Device");
        });

        modelBuilder.Entity<ControlPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ControlP__3213E83FE99B45C3");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ControlPointName)
                .HasMaxLength(25)
                .HasColumnName("controlPointName");
        });

        modelBuilder.Entity<ControlPointsInAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ControlP__3213E83FDCFD3F94");

            entity.ToTable("ControlPointsInAsset");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssetId).HasColumnName("asset_Id");
            entity.Property(e => e.ControlPointId).HasColumnName("controlPoint_Id");

            entity.HasOne(d => d.Asset).WithMany(p => p.ControlPointsInAssets)
                .HasForeignKey(d => d.AssetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlPointsInAsset_Asset");

            entity.HasOne(d => d.ControlPoint).WithMany(p => p.ControlPointsInAssets)
                .HasForeignKey(d => d.ControlPointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlPointsInAsset_ControlPoints");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Device__3213E83F9B965A45");

            entity.ToTable("Device");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .HasColumnName("ip_address");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.TypeId).HasColumnName("type_Id");

            entity.HasOne(d => d.Type).WithMany(p => p.Devices)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Device_TypeOfDevice");
        });

        modelBuilder.Entity<Endian>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Endians__3213E83FEFCD679C");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Endians)
                .HasMaxLength(4)
                .HasColumnName("endians");
        });

        modelBuilder.Entity<Frequency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Frequenc__3213E83F0FA3EFE2");

            entity.ToTable("Frequency");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Frequency1)
                .HasMaxLength(100)
                .HasColumnName("frequency");
        });

        modelBuilder.Entity<MeasureSetup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MeasureS__3213E83FED39C986");

            entity.ToTable("MeasureSetup");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndiansId).HasColumnName("endians_id");
            entity.Property(e => e.MonitoringPointId).HasColumnName("monitoringPoint_id");
            entity.Property(e => e.ParamGroupId).HasColumnName("paramGroup_id");
            entity.Property(e => e.RegisterAddressId).HasColumnName("registerAddress_id");

            entity.HasOne(d => d.Endians).WithMany(p => p.MeasureSetups)
                .HasForeignKey(d => d.EndiansId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeasureSetup_Endians");

            entity.HasOne(d => d.MonitoringPoint).WithMany(p => p.MeasureSetups)
                .HasForeignKey(d => d.MonitoringPointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeasureSetup_MonitoringPoint");

            entity.HasOne(d => d.ParamGroup).WithMany(p => p.MeasureSetups)
                .HasForeignKey(d => d.ParamGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeasureSetup_ParametersGroup");

            entity.HasOne(d => d.RegisterAddress).WithMany(p => p.MeasureSetups)
                .HasForeignKey(d => d.RegisterAddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeasureSetup_RegisterAddress");
        });

        modelBuilder.Entity<MeasureUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MeasureU__3213E83FF5895934");

            entity.ToTable("MeasureUnit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UnitName)
                .HasMaxLength(15)
                .HasColumnName("unitName");
        });

        modelBuilder.Entity<ModeWork>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ModeWork__3213E83F31CE114B");

            entity.ToTable("ModeWork");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ModeworkName)
                .HasMaxLength(100)
                .HasColumnName("modeworkName");
        });

        modelBuilder.Entity<MonitoringPoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Monitori__3213E83F21ABA0E3");

            entity.ToTable("MonitoringPoint");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChannelId).HasColumnName("channel_Id");
            entity.Property(e => e.ControlPointInAssetsId).HasColumnName("controlPointInAssets_Id");

            entity.HasOne(d => d.Channel).WithMany(p => p.MonitoringPoints)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonitoringPoint_Channels");

            entity.HasOne(d => d.ControlPointInAssets).WithMany(p => p.MonitoringPoints)
                .HasForeignKey(d => d.ControlPointInAssetsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MonitoringPoint_ControlPoint");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paramete__3213E83F60A93AA9");

            entity.ToTable("Parameter");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ParameterName)
                .HasMaxLength(100)
                .HasColumnName("parameterName");
            entity.Property(e => e.UnitId).HasColumnName("unit_Id");

            entity.HasOne(d => d.Unit).WithMany(p => p.Parameters)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("FK_Parameter_MeasureUnit");
        });

        modelBuilder.Entity<ParametersGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paramete__3213E83F929FF247");

            entity.ToTable("ParametersGroup");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FrequencyId).HasColumnName("frequency_Id");
            entity.Property(e => e.ParameterId).HasColumnName("parameter_Id");

            entity.HasOne(d => d.Frequency).WithMany(p => p.ParametersGroups)
                .HasForeignKey(d => d.FrequencyId)
                .HasConstraintName("FK_ParametersGroup_Frequency");

            entity.HasOne(d => d.Parameter).WithMany(p => p.ParametersGroups)
                .HasForeignKey(d => d.ParameterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParametersGroup_Parameter");
        });

        modelBuilder.Entity<RegisterAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Register__3213E83F65D9B877");

            entity.ToTable("RegisterAddress");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
        });

        modelBuilder.Entity<TimeUnit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TimeUnit__3213E83F99672AFE");

            entity.ToTable("TimeUnit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UnitName)
                .HasMaxLength(25)
                .HasColumnName("unitName");
        });

        modelBuilder.Entity<TypeOfDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TypeOfDe__3213E83FB0A580A6");

            entity.ToTable("TypeOfDevice");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(50)
                .HasColumnName("typeName");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
