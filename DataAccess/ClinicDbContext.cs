using BussinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext()
        {
        }

        public ClinicDbContext(DbContextOptions<ClinicDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DoctorEarning> DoctorEarnings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber);
            });

            // Doctor configuration
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithOne(u => u.Doctor)
                    .HasForeignKey<Doctor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Specialty)
                    .WithMany(s => s.Doctors)
                    .HasForeignKey(d => d.SpecialtyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DoctorSchedule configuration
            modelBuilder.Entity<DoctorSchedule>(entity =>
            {
                entity.HasOne(ds => ds.Doctor)
                    .WithMany(d => d.DoctorSchedules)
                    .HasForeignKey(ds => ds.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Appointment configuration
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Patient)
                    .WithMany(u => u.Appointments)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                    .WithMany(d => d.Appointments)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.DoctorId, e.AppointmentDate, e.AppointmentTime });
            });

            // MedicalRecord configuration
            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.HasOne(mr => mr.Appointment)
                    .WithOne(a => a.MedicalRecord)
                    .HasForeignKey<MedicalRecord>(mr => mr.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(mr => mr.Patient)
                    .WithMany()
                    .HasForeignKey(mr => mr.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(mr => mr.Doctor)
                    .WithMany(d => d.MedicalRecords)
                    .HasForeignKey(mr => mr.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasOne(p => p.Appointment)
                    .WithOne(a => a.Payment)
                    .HasForeignKey<Payment>(p => p.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Patient)
                    .WithMany()
                    .HasForeignKey(p => p.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DoctorEarning configuration
            modelBuilder.Entity<DoctorEarning>(entity =>
            {
                entity.HasOne(de => de.Doctor)
                    .WithMany(d => d.DoctorEarnings)
                    .HasForeignKey(de => de.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(de => de.Appointment)
                    .WithMany()
                    .HasForeignKey(de => de.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Review configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Doctor)
                    .WithMany()
                    .HasForeignKey(r => r.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Patient)
                    .WithMany()
                    .HasForeignKey(r => r.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Appointment)
                    .WithMany()
                    .HasForeignKey(r => r.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.IsRead });
            });
        }
    }
}
