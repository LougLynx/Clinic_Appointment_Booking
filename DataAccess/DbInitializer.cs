using BussinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace DataAccess
{
    public class DbInitializer
    {
        public static void Initialize(ClinicDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if data already exists
            if (context.Users.Any())
            {
                return; // Database has been seeded
            }

            SeedSpecialties(context);
            SeedUsers(context);
            SeedDoctors(context);

            context.SaveChanges();
        }

        private static void SeedSpecialties(ClinicDbContext context)
        {
            var specialties = new[]
            {
                new Specialty
                {
                    Name = "Cardiology",
                    Description = "Heart and cardiovascular system",
                    IconName = "ecg_heart",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Dermatology",
                    Description = "Skin, hair, and nails",
                    IconName = "dermatology",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Neurology",
                    Description = "Brain and nervous system",
                    IconName = "neurology",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Pediatrics",
                    Description = "Children's health",
                    IconName = "pediatrics",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Orthopedics",
                    Description = "Bones, joints, and muscles",
                    IconName = "orthopedics",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Ophthalmology",
                    Description = "Eyes and vision",
                    IconName = "visibility",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "General Practice",
                    Description = "General medical care",
                    IconName = "medical_services",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };

            context.Specialties.AddRange(specialties);
            context.SaveChanges();
        }

        private static void SeedUsers(ClinicDbContext context)
        {
            var users = new[]
            {
                // Admin User
                new User
                {
                    FullName = "Admin User",
                    Email = "admin@mediclinic.com",
                    PhoneNumber = "0123456789",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Gender = "Male",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                // Doctor Users
                new User
                {
                    FullName = "Dr. Emily Chen",
                    Email = "emily.chen@mediclinic.com",
                    PhoneNumber = "0123456701",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Gender = "Female",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Marcus Johnson",
                    Email = "marcus.johnson@mediclinic.com",
                    PhoneNumber = "0123456702",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1982, 8, 22),
                    Gender = "Male",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Sarah Patel",
                    Email = "sarah.patel@mediclinic.com",
                    PhoneNumber = "0123456703",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1990, 3, 10),
                    Gender = "Female",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. David Wilson",
                    Email = "david.wilson@mediclinic.com",
                    PhoneNumber = "0123456704",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1978, 11, 5),
                    Gender = "Male",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                // Sample Patient
                new User
                {
                    FullName = "John Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "0987654321",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    DateOfBirth = new DateTime(1990, 6, 15),
                    Gender = "Male",
                    Address = "123 Main Street, City",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static void SeedDoctors(ClinicDbContext context)
        {
            var doctors = new[]
            {
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "emily.chen@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Cardiology").SpecialtyId,
                    Qualifications = "MD, FACC",
                    YearsOfExperience = 15,
                    Location = "Main Campus, Wing A",
                    Languages = "English, Mandarin",
                    Rating = 4.9m,
                    ReviewCount = 120,
                    ConsultationFee = 150.00m,
                    Bio = "Experienced cardiologist specializing in preventive cardiology and heart disease management.",
                    ProfileImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuA6wrTt341E3yA-wcu7p1aezUUzdiU1W6-T8VcCSRJDUz30gdlsrDhEkuQ81u3wwgHfpxjPy8qPB4Vs67aLJOmkK9fxlE09vNYcwPv82Z0wAPTVv3FU43V2XurmRI90S5d_WpTxuti53EFIOo1TVob_7CzaY8-egib308iyVTAfuKpsfCH_zq1rxrZpmZAuA8qXs0teRnfvMPOYnJleqCFK6gPIeS9EJQ42w4zNrXgEEKuRKrqSSNwUwWmWtTSEf2_nuTPWKE0Q2CSB",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "marcus.johnson@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Neurology").SpecialtyId,
                    Qualifications = "MD, PhD in Neurology",
                    YearsOfExperience = 12,
                    Location = "North Clinic, Suite 302",
                    Languages = "English, Spanish",
                    Rating = 4.8m,
                    ReviewCount = 95,
                    ConsultationFee = 180.00m,
                    Bio = "Neurologist with expertise in treating neurological disorders and brain health.",
                    ProfileImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuCqwIgMbGcY96gevDFCIo-w_AO6bicyVlqwdSnci0dC8pTAVaPeeloMbZBWe1Ze5rtptK6u1O0BM0N7fmQv5yh_JZBxKOrgW-DzgFeRXi6_eoCfYDCYbwhN2UcnBJni1deqsP2E2uL3b7jDYlpxdeJavx0eGme7N_f20sxJyi3cjubpzelNX8sLUn2vBEW_nPQ-YomdymMzghv3tkVO86k8ctFsoSZpnXSvgWNY4xO4g6n7Y2lkjXgk-I3VdyYt1uLJFc_eQr0hLTmV",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "sarah.patel@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Dermatology").SpecialtyId,
                    Qualifications = "MD, Board Certified Dermatologist",
                    YearsOfExperience = 8,
                    Location = "Main Campus, Wing B",
                    Languages = "English, Hindi",
                    Rating = 5.0m,
                    ReviewCount = 150,
                    ConsultationFee = 120.00m,
                    Bio = "Dermatologist specializing in medical and cosmetic dermatology.",
                    ProfileImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuCxFkEg4MvatjF5Jt30_e1V_1uHOAljZ6PumLEN75ruIXZ3uvcyX1Xxiw82_fIvn9Nqyq5rijetXlpEM6jFDy12ZyWxs1wv1MKGvobPLxkg3EpJeJ4SqkPPcUIGL14zeoDjds8TKwLTPvP3AdQ9EVOQl0NKK3Qy12r7yhJpOUUN2fyetyVvmCC8Tj7fh8OWIq5SagzulPwLAVBJnC3i4vHSVq126IwJnLy-TAZ49IdIZNR-baA5Gl6hTB392w2sa7dgnv0-cEg5Gz8V",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "david.wilson@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Pediatrics").SpecialtyId,
                    Qualifications = "MD, Pediatrics Specialist",
                    YearsOfExperience = 20,
                    Location = "East Wing Children's Center",
                    Languages = "English",
                    Rating = 4.7m,
                    ReviewCount = 200,
                    ConsultationFee = 100.00m,
                    Bio = "Experienced pediatrician dedicated to children's health and wellness.",
                    ProfileImageUrl = "https://lh3.googleusercontent.com/aida-public/AB6AXuDXQZna2h6VMsuzhgfuErgB60Vw3raDH9krDGMNUMh0P6uBLUTDqKnoj_7x0V4eUDnf9SmHwzrT15jo1xk922e4VOXTZDG-uf1nAU24cn6YjXlzvOUB88rLchU-S_yrQ97kl1JNtwWHucgUcCw0fsGtp-j1z6zY8eBaGjc0dgfPIAzJfdtlaUqetaOJDbBmc2iGCKtK9X-TikTW4wzu28gdlkQJPXJP4-BqKN_4v2SjJLCzgYFAOaGGq5Bcb2Q-IMghQqvQrioeD2-T",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                }
            };

            context.Doctors.AddRange(doctors);
            context.SaveChanges();
        }
    }
}
