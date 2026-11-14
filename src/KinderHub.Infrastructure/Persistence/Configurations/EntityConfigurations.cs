using KinderHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KinderHub.Infrastructure.Persistence.Configurations;

// Database rules: required fields, max lengths, unique values, relationships.

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> b)
    {
        b.Property(g => g.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(g => g.Name).IsUnique();
        b.Property(g => g.AgeRange).HasMaxLength(50);
        b.Property(g => g.Room).HasMaxLength(50);
        b.Property(g => g.TeacherName).HasMaxLength(100);
    }
}

public class ChildConfiguration : IEntityTypeConfiguration<Child>
{
    public void Configure(EntityTypeBuilder<Child> b)
    {
        b.Property(c => c.FirstName).IsRequired().HasMaxLength(50);
        b.Property(c => c.LastName).IsRequired().HasMaxLength(50);
        b.Property(c => c.ParentName).IsRequired().HasMaxLength(100);
        b.Property(c => c.ParentEmail).IsRequired().HasMaxLength(150);
        b.Property(c => c.ParentPhone).HasMaxLength(30);
        b.Property(c => c.Notes).HasMaxLength(500);
        b.HasIndex(c => c.ParentEmail);

        // A group with children cannot be deleted by accident
        b.HasOne(c => c.Group)
         .WithMany(g => g.Children)
         .HasForeignKey(c => c.GroupId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> b)
    {
        b.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(a => a.PickedUpBy).HasMaxLength(100);
        b.Property(a => a.Note).HasMaxLength(300);

        // Only ONE attendance record per child per day
        b.HasIndex(a => new { a.ChildId, a.Date }).IsUnique();

        b.HasOne(a => a.Child)
         .WithMany(c => c.AttendanceRecords)
         .HasForeignKey(a => a.ChildId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DailyReportConfiguration : IEntityTypeConfiguration<DailyReport>
{
    public void Configure(EntityTypeBuilder<DailyReport> b)
    {
        b.Property(r => r.Mood).HasConversion<string>().HasMaxLength(20);
        b.Property(r => r.Meals).HasMaxLength(300);
        b.Property(r => r.Activities).HasMaxLength(500);
        b.Property(r => r.TeacherComment).HasMaxLength(500);
        b.Property(r => r.AuthorName).HasMaxLength(100);

        // Only ONE daily report per child per day
        b.HasIndex(r => new { r.ChildId, r.Date }).IsUnique();

        b.HasOne(r => r.Child)
         .WithMany(c => c.DailyReports)
         .HasForeignKey(r => r.ChildId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> b)
    {
        b.Property(a => a.Title).IsRequired().HasMaxLength(150);
        b.Property(a => a.Message).IsRequired().HasMaxLength(2000);
        b.Property(a => a.AuthorName).HasMaxLength(100);
        b.HasIndex(a => a.CreatedAt);

        b.HasOne(a => a.Group)
         .WithMany()
         .HasForeignKey(a => a.GroupId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
