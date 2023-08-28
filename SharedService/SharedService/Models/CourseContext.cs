using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SharedService.Models;

namespace Coursesvc.Models;

public partial class CourseContext : DbContext
{
    public CourseContext()
    {
    }

    public CourseContext(DbContextOptions<CourseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }
    public virtual DbSet<ImageFile> ImageFiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;database=course;user=root;password=01Infodation", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.1.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("courses");

            entity.Property(e => e.Code).HasMaxLength(45);
            entity.Property(e => e.Decription).HasMaxLength(255);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("enrollments");

            entity.HasIndex(e => e.CouresId, "FK_course");

            entity.Property(e => e.EnrolledDate).HasColumnType("datetime");

            /*entity.HasOne(d => d.Coures).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CouresId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_course");*/
        });
        modelBuilder.Entity<ImageFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.ImageLink);
            entity.Property(e => e.ImageStorageName);



        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
