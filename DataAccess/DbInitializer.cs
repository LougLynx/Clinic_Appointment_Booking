using BussinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class DbInitializer
    {
        public static void Initialize(ClinicDbContext context)
        {
            // Apply pending migrations to update database schema
            context.Database.Migrate();

            // Check if data already exists
            if (context.Users.Any())
            {
                return; // Database has been seeded
            }

            SeedSpecialties(context);
            SeedUsers(context);
            SeedDoctors(context);

            SeedSchedules(context);
            SeedAppointments(context);
            SeedMedicalRecords(context);
            SeedPayments(context);
            SeedReviews(context);
            SeedNotifications(context);
            SeedContactMessages(context);

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
                    Name = "Pediatrics",
                    Description = "Infant, child, and adolescent care",
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
                    Name = "Neurology",
                    Description = "Brain and nervous system",
                    IconName = "neurology",
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
                    Name = "Dentistry",
                    Description = "Oral and dental health",
                    IconName = "dentistry",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Psychiatry",
                    Description = "Mental health and behavioral care",
                    IconName = "psychology",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "General Practice",
                    Description = "Comprehensive primary care for all ages",
                    IconName = "medical_services",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Endocrinology",
                    Description = "Hormone and metabolic disorders",
                    IconName = "biotech",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Gastroenterology",
                    Description = "Digestive system and liver care",
                    IconName = "science",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Specialty
                {
                    Name = "Oncology",
                    Description = "Cancer screening and treatment",
                    IconName = "health_and_safety",
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
                new User
                {
                    FullName = "Dr. Olivia Carter",
                    Email = "olivia.carter@mediclinic.com",
                    PhoneNumber = "0123456705",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1987, 7, 19),
                    Gender = "Female",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Daniel Lee",
                    Email = "daniel.lee@mediclinic.com",
                    PhoneNumber = "0123456706",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1984, 2, 2),
                    Gender = "Male",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Aisha Khan",
                    Email = "aisha.khan@mediclinic.com",
                    PhoneNumber = "0123456707",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1986, 9, 12),
                    Gender = "Female",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Michael Rivera",
                    Email = "michael.rivera@mediclinic.com",
                    PhoneNumber = "0123456708",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1981, 4, 27),
                    Gender = "Male",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Hannah Brooks",
                    Email = "hannah.brooks@mediclinic.com",
                    PhoneNumber = "0123456709",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1991, 1, 9),
                    Gender = "Female",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. James Stewart",
                    Email = "james.stewart@mediclinic.com",
                    PhoneNumber = "0123456710",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1979, 12, 3),
                    Gender = "Male",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Priya Nair",
                    Email = "priya.nair@mediclinic.com",
                    PhoneNumber = "0123456711",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1988, 6, 21),
                    Gender = "Female",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Dr. Sophia Nguyen",
                    Email = "sophia.nguyen@mediclinic.com",
                    PhoneNumber = "0123456712",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"),
                    Role = "Doctor",
                    DateOfBirth = new DateTime(1992, 10, 14),
                    Gender = "Female",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                // Sample Patients
                new User
                {
                    FullName = "John Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "0987654321",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    DateOfBirth = new DateTime(1990, 6, 15),
                    Gender = "Male",
                    Address = "123 Main Street, Springfield",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Emma Davis",
                    Email = "emma.davis@example.com",
                    PhoneNumber = "0987654322",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    DateOfBirth = new DateTime(1994, 4, 8),
                    Gender = "Female",
                    Address = "456 Oak Avenue, Riverview",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Liam Brown",
                    Email = "liam.brown@example.com",
                    PhoneNumber = "0987654323",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    DateOfBirth = new DateTime(1989, 9, 30),
                    Gender = "Male",
                    Address = "78 Lake Road, Brookfield",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Ava Johnson",
                    Email = "ava.johnson@example.com",
                    PhoneNumber = "0987654324",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    DateOfBirth = new DateTime(1996, 11, 22),
                    Gender = "Female",
                    Address = "92 Pine Street, Lakeview",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Noah Miller",
                    Email = "noah.miller@example.com",
                    PhoneNumber = "0987654325",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    DateOfBirth = new DateTime(1987, 3, 5),
                    Gender = "Male",
                    Address = "15 Maple Drive, Hillcrest",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FullName = "Sophia Garcia",
                    Email = "sophia.garcia@example.com",
                    PhoneNumber = "0987654326",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123"),
                    Role = "Patient",
                    DateOfBirth = new DateTime(1992, 8, 17),
                    Gender = "Female",
                    Address = "210 Cedar Lane, Fairview",
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
                    Location = "Heart Center, Main Campus",
                    Languages = "English, Mandarin",
                    Rating = 4.9m,
                    ReviewCount = 210,
                    ConsultationFee = 180.00m,
                    Bio = "Cardiologist focused on preventive care, heart failure management, and long-term cardiovascular health.",
                    ProfileImageUrl = "https://images.pexels.com/photos/35260793/pexels-photo-35260793.jpeg?auto=compress&cs=tinysrgb&w=800",
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
                    ReviewCount = 165,
                    ConsultationFee = 200.00m,
                    Bio = "Neurologist specializing in headache medicine, epilepsy, and neurodiagnostic care.",
                    ProfileImageUrl = "https://images.pexels.com/photos/32254662/pexels-photo-32254662.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "sarah.patel@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Dermatology").SpecialtyId,
                    Qualifications = "MD, FAAD",
                    YearsOfExperience = 9,
                    Location = "Main Campus, Wing B",
                    Languages = "English, Hindi",
                    Rating = 4.9m,
                    ReviewCount = 190,
                    ConsultationFee = 140.00m,
                    Bio = "Dermatologist providing medical, surgical, and cosmetic skin care for all ages.",
                    ProfileImageUrl = "https://images.pexels.com/photos/7904406/pexels-photo-7904406.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "david.wilson@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Pediatrics").SpecialtyId,
                    Qualifications = "MD, Pediatrics",
                    YearsOfExperience = 18,
                    Location = "Children's Center, East Wing",
                    Languages = "English",
                    Rating = 4.7m,
                    ReviewCount = 240,
                    ConsultationFee = 110.00m,
                    Bio = "Pediatrician dedicated to preventative care, childhood wellness, and family education.",
                    ProfileImageUrl = "https://images.pexels.com/photos/27298085/pexels-photo-27298085.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "olivia.carter@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Orthopedics").SpecialtyId,
                    Qualifications = "MD, Orthopedic Surgery",
                    YearsOfExperience = 11,
                    Location = "Orthopedic Clinic, West Wing",
                    Languages = "English, French",
                    Rating = 4.8m,
                    ReviewCount = 130,
                    ConsultationFee = 190.00m,
                    Bio = "Orthopedic surgeon focused on sports injuries, joint preservation, and minimally invasive procedures.",
                    ProfileImageUrl = "https://images.pexels.com/photos/7904416/pexels-photo-7904416.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "daniel.lee@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Ophthalmology").SpecialtyId,
                    Qualifications = "MD, Ophthalmology",
                    YearsOfExperience = 10,
                    Location = "Vision Center, Floor 2",
                    Languages = "English, Korean",
                    Rating = 4.8m,
                    ReviewCount = 120,
                    ConsultationFee = 160.00m,
                    Bio = "Ophthalmologist specializing in comprehensive eye exams, cataracts, and glaucoma management.",
                    ProfileImageUrl = "https://images.pexels.com/photos/31842729/pexels-photo-31842729.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "aisha.khan@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Psychiatry").SpecialtyId,
                    Qualifications = "MD, Psychiatry",
                    YearsOfExperience = 13,
                    Location = "Behavioral Health, Suite 210",
                    Languages = "English, Urdu",
                    Rating = 4.9m,
                    ReviewCount = 155,
                    ConsultationFee = 170.00m,
                    Bio = "Psychiatrist focused on mood disorders, anxiety care, and evidence-based therapy plans.",
                    ProfileImageUrl = "https://images.pexels.com/photos/12889997/pexels-photo-12889997.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "michael.rivera@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Dentistry").SpecialtyId,
                    Qualifications = "DDS, Cosmetic Dentistry",
                    YearsOfExperience = 14,
                    Location = "Dental Clinic, Floor 1",
                    Languages = "English, Spanish",
                    Rating = 4.7m,
                    ReviewCount = 180,
                    ConsultationFee = 130.00m,
                    Bio = "Dentist providing preventive care, restorative treatments, and cosmetic smile design.",
                    ProfileImageUrl = "https://images.pexels.com/photos/28755708/pexels-photo-28755708.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "hannah.brooks@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Endocrinology").SpecialtyId,
                    Qualifications = "MD, Endocrinology",
                    YearsOfExperience = 9,
                    Location = "Metabolic Clinic, Suite 405",
                    Languages = "English",
                    Rating = 4.8m,
                    ReviewCount = 98,
                    ConsultationFee = 175.00m,
                    Bio = "Endocrinologist specializing in diabetes management, thyroid care, and metabolic health.",
                    ProfileImageUrl = "https://images.pexels.com/photos/32254665/pexels-photo-32254665.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "james.stewart@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Gastroenterology").SpecialtyId,
                    Qualifications = "MD, Gastroenterology",
                    YearsOfExperience = 16,
                    Location = "Digestive Health Center",
                    Languages = "English",
                    Rating = 4.8m,
                    ReviewCount = 140,
                    ConsultationFee = 190.00m,
                    Bio = "Gastroenterologist providing comprehensive care for digestive and liver conditions.",
                    ProfileImageUrl = "https://images.pexels.com/photos/32254658/pexels-photo-32254658.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "priya.nair@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "Oncology").SpecialtyId,
                    Qualifications = "MD, Medical Oncology",
                    YearsOfExperience = 12,
                    Location = "Cancer Care Center",
                    Languages = "English, Malayalam",
                    Rating = 4.9m,
                    ReviewCount = 160,
                    ConsultationFee = 220.00m,
                    Bio = "Oncologist focused on personalized treatment plans, survivorship care, and patient support.",
                    ProfileImageUrl = "https://images.pexels.com/photos/7904455/pexels-photo-7904455.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                },
                new Doctor
                {
                    UserId = context.Users.First(u => u.Email == "sophia.nguyen@mediclinic.com").UserId,
                    SpecialtyId = context.Specialties.First(s => s.Name == "General Practice").SpecialtyId,
                    Qualifications = "MD, Family Medicine",
                    YearsOfExperience = 8,
                    Location = "Primary Care Clinic",
                    Languages = "English, Vietnamese",
                    Rating = 4.7m,
                    ReviewCount = 110,
                    ConsultationFee = 120.00m,
                    Bio = "Family physician delivering comprehensive primary care, preventive screenings, and chronic disease management.",
                    ProfileImageUrl = "https://images.pexels.com/photos/8376309/pexels-photo-8376309.jpeg?auto=compress&cs=tinysrgb&w=800",
                    IsAvailable = true,
                    CreatedAt = DateTime.Now
                }
            };

            context.Doctors.AddRange(doctors);
            context.SaveChanges();
        }

        private static void SeedSchedules(ClinicDbContext context)
        {
            var doctors = context.Doctors.Take(5).ToList();
            var schedules = new List<DoctorSchedule>();

            foreach (var doc in doctors)
            {
                // Thêm lịch Thứ 2, 4, 6
                int[] days = { 1, 3, 5 };
                foreach (var day in days)
                {
                    schedules.Add(new DoctorSchedule
                    {
                        DoctorId = doc.DoctorId,
                        DayOfWeek = (DayOfWeek)day,
                        StartTime = new TimeSpan(8, 0, 0),
                        EndTime = new TimeSpan(12, 0, 0),
                        SlotDurationMinutes = 30,
                        IsAvailable = true
                    });
                }
            }
            context.DoctorSchedules.AddRange(schedules);
            context.SaveChanges();
        }

        private static void SeedAppointments(ClinicDbContext context)
        {
            var docs = context.Doctors.Take(3).ToList();
            var patients = context.Users.Where(u => u.Role == "Patient").Take(3).ToList();

            var appointments = new List<Appointment>
    {
        new Appointment { PatientId = patients[0].UserId, DoctorId = docs[0].DoctorId, AppointmentDate = DateTime.Now.AddDays(1).Date, AppointmentTime = new TimeSpan(9, 0, 0), Status = "Confirmed", ReasonForVisit = "General Checkup", ConsultationFee = 150, CreatedAt = DateTime.Now },
        new Appointment { PatientId = patients[1].UserId, DoctorId = docs[1].DoctorId, AppointmentDate = DateTime.Now.AddDays(2).Date, AppointmentTime = new TimeSpan(10, 30, 0), Status = "Pending", ReasonForVisit = "Skin Rash", ConsultationFee = 200, CreatedAt = DateTime.Now },
        new Appointment { PatientId = patients[2].UserId, DoctorId = docs[2].DoctorId, AppointmentDate = DateTime.Now.AddDays(-2).Date, AppointmentTime = new TimeSpan(14, 0, 0), Status = "Completed", ReasonForVisit = "Follow up", ConsultationFee = 120, CreatedAt = DateTime.Now.AddDays(-5) },
        new Appointment { PatientId = patients[0].UserId, DoctorId = docs[1].DoctorId, AppointmentDate = DateTime.Now.AddDays(-5).Date, AppointmentTime = new TimeSpan(15, 30, 0), Status = "Completed", ReasonForVisit = "Headache", ConsultationFee = 180, CreatedAt = DateTime.Now.AddDays(-10) },
        new Appointment { PatientId = patients[1].UserId, DoctorId = docs[0].DoctorId, AppointmentDate = DateTime.Now.AddDays(3).Date, AppointmentTime = new TimeSpan(8, 0, 0), Status = "Cancelled", ReasonForVisit = "Consultation", CancellationReason = "Patient busy", ConsultationFee = 150, CreatedAt = DateTime.Now }
    };
            context.Appointments.AddRange(appointments);
            context.SaveChanges();
        }

        private static void SeedMedicalRecords(ClinicDbContext context)
        {
            var completedAppts = context.Appointments.Where(a => a.Status == "Completed").ToList();
            var records = new List<MedicalRecord>();

            foreach (var appt in completedAppts)
            {
                records.Add(new MedicalRecord
                {
                    AppointmentId = appt.AppointmentId,
                    PatientId = appt.PatientId,
                    DoctorId = appt.DoctorId,
                    Symptoms = "Common symptoms related to " + appt.ReasonForVisit,
                    Diagnosis = "Diagnosed condition for " + appt.ReasonForVisit,
                    Prescription = "Medicine A 500mg, Medicine B",
                    TreatmentPlan = "Follow up in 2 weeks",
                    CreatedAt = DateTime.Now
                });
            }
            context.MedicalRecords.AddRange(records);
        }

        private static void SeedPayments(ClinicDbContext context)
        {
            var appts = context.Appointments.Take(4).ToList();
            var payments = new List<Payment>();

            for (int i = 0; i < appts.Count; i++)
            {
                payments.Add(new Payment
                {
                    AppointmentId = appts[i].AppointmentId,
                    PatientId = appts[i].PatientId,
                    Amount = appts[i].ConsultationFee,
                    PaymentMethod = i % 2 == 0 ? "Credit Card" : "Cash",
                    PaymentStatus = i == 3 ? "Pending" : "Completed",
                    TransactionId = "TXN" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    PaymentDate = DateTime.Now.AddDays(-i),
                    CreatedAt = DateTime.Now
                });
            }
            context.Payments.AddRange(payments);
        }

        private static void SeedReviews(ClinicDbContext context)
        {
            var completedAppts = context.Appointments.Where(a => a.Status == "Completed").ToList();
            var reviews = new List<Review>();

            int rating = 5;
            foreach (var appt in completedAppts)
            {
                reviews.Add(new Review
                {
                    DoctorId = appt.DoctorId,
                    PatientId = appt.PatientId,
                    AppointmentId = appt.AppointmentId,
                    Rating = rating--,
                    Comment = "Feedback for doctor " + appt.DoctorId,
                    IsVerified = true,
                    CreatedAt = DateTime.Now
                });
                if (rating < 3)
                {
                    rating = 5;
                }
            }
            context.Reviews.AddRange(reviews);
        }

        private static void SeedNotifications(ClinicDbContext context)
        {
            var patients = context.Users.Where(u => u.Role == "Patient").Take(3).ToList();
            var notifications = new List<Notification>();

            foreach (var p in patients)
            {
                notifications.Add(new Notification { UserId = p.UserId, Title = "Health Tip", Message = "Remember to drink 2L of water daily.", Type = "Info", CreatedAt = DateTime.Now });
                notifications.Add(new Notification { UserId = p.UserId, Title = "Update", Message = "Your profile has been updated.", Type = "Success", CreatedAt = DateTime.Now });
            }
            context.Notifications.AddRange(notifications);
        }

        private static void SeedContactMessages(ClinicDbContext context)
        {
            var messages = new List<ContactMessage>();
            for (int i = 1; i <= 5; i++)
            {
                messages.Add(new ContactMessage
                {
                    FirstName = "User" + i,
                    LastName = "Test",
                    Email = $"user{i}@test.com",
                    Subject = "Inquiry " + i,
                    Message = "This is a test message number " + i,
                    Status = i % 2 == 0 ? "Read" : "New",
                    CreatedAt = DateTime.Now
                });
            }
            context.ContactMessages.AddRange(messages);
        }
    }
}
