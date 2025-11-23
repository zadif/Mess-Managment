using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EAD.Models;

public partial class EadProjectContext : DbContext
{
    public EadProjectContext()
    {
    }

    public EadProjectContext(DbContextOptions<EadProjectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bill> Bills { get; set; }

    public virtual DbSet<BillRecheckRequest> BillRecheckRequests { get; set; }

    public virtual DbSet<DailyConsumption> DailyConsumptions { get; set; }

    public virtual DbSet<DailyMenu> DailyMenus { get; set; }

    public virtual DbSet<MealItem> MealItems { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=eadProject;Trusted_Connection=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bills__3214EC0785021DE2");

            entity.HasIndex(e => new { e.UserId, e.MonthYear }, "UQ_Bill").IsUnique();

            entity.Property(e => e.GeneratedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.MonthYear)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Bills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Bill_User");
        });

        modelBuilder.Entity<BillRecheckRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BillRech__3214EC0775322CE4");

            entity.Property(e => e.RequestMessage).HasMaxLength(500);
            entity.Property(e => e.RequestedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Bill).WithMany(p => p.BillRecheckRequests)
                .HasForeignKey(d => d.BillId)
                .HasConstraintName("FK_RecheckRequest_Bill");

            entity.HasOne(d => d.User).WithMany(p => p.BillRecheckRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RecheckRequest_User");
        });

        modelBuilder.Entity<DailyConsumption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DailyCon__3214EC074D020E3D");

            entity.HasIndex(e => new { e.UserId, e.MealItemId, e.ConsumptionDate }, "UQ_DailyConsumption").IsUnique();

            entity.Property(e => e.ConsumptionDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.MealItem).WithMany(p => p.DailyConsumptions)
                .HasForeignKey(d => d.MealItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DailyConsumption_MealItem");

            entity.HasOne(d => d.User).WithMany(p => p.DailyConsumptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_DailyConsumption_User");
        });

        modelBuilder.Entity<DailyMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DailyMen__3214EC0785AD9854");

            entity.ToTable("DailyMenu");

            entity.Property(e => e.DayOfWeek).HasMaxLength(20);
            entity.Property(e => e.MealType).HasMaxLength(20);

            entity.HasOne(d => d.MealItem).WithMany(p => p.DailyMenus)
                .HasForeignKey(d => d.MealItemId)
                .HasConstraintName("FK_DailyMenu_MealItem");
        });

        modelBuilder.Entity<MealItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC077CB09D50");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC07B1AEAFBA");

            entity.HasIndex(e => e.Email, "UQ__tmp_ms_x__A9D10534D3A90284").IsUnique();

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
