using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.Infrastructure.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<StudentParent> StudentParents { get; set; }
        public DbSet<TeacherClass> TeacherClasses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Student configuration
            builder.Entity<Student>()
                .HasIndex(s => s.AdmissionNo)
                .IsUnique();

            builder.Entity<Student>()
                .Property(s => s.UserId)
                .IsRequired();

            // Parent configuration
            builder.Entity<Parent>()
                .HasIndex(p => p.Phone)
                .IsUnique();

            builder.Entity<Parent>()
                .Property(p => p.UserId)
                .IsRequired();

            // StudentParent configuration
            builder.Entity<StudentParent>()
                .HasIndex(sp => new { sp.StudentId, sp.ParentId })
                .IsUnique();

            builder.Entity<StudentParent>()
                .Property(sp => sp.StudentId)
                .IsRequired();

            builder.Entity<StudentParent>()
                .Property(sp => sp.ParentId)
                .IsRequired();

            // TeacherClass configuration
            builder.Entity<TeacherClass>()
                .HasIndex(tc => new { tc.TeacherId, tc.Class, tc.Section })
                .IsUnique();

            builder.Entity<TeacherClass>()
                .Property(tc => tc.TeacherId)
                .IsRequired();

            builder.Entity<TeacherClass>()
                .Property(tc => tc.Class)
                .IsRequired();
        }
    }
}