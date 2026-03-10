using DataAccess;

namespace DataAccess
{
    public class SeedDataProgram
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Clinic Appointment Booking - Database Seeder ===");
            Console.WriteLine();

            try
            {
                using (var context = new ClinicDbContext())
                {
                    Console.WriteLine("Connecting to database...");
                    
                    // Check if database exists
                    if (context.Database.CanConnect())
                    {
                        Console.WriteLine("✓ Connected to database successfully!");
                        Console.WriteLine();
                        
                        // Initialize seed data
                        Console.WriteLine("Starting data seeding process...");
                        DbInitializer.Initialize(context);
                        
                        Console.WriteLine();
                        Console.WriteLine("✓ Database seeded successfully!");
                        Console.WriteLine();
                        Console.WriteLine("Default accounts created:");
                        Console.WriteLine("  Admin: admin@mediclinic.com / Admin@123");
                        Console.WriteLine("  Doctor 1: emily.chen@mediclinic.com / Doctor@123");
                        Console.WriteLine("  Doctor 2: marcus.johnson@mediclinic.com / Doctor@123");
                        Console.WriteLine("  Doctor 3: sarah.patel@mediclinic.com / Doctor@123");
                        Console.WriteLine("  Doctor 4: david.wilson@mediclinic.com / Doctor@123");
                        Console.WriteLine("  Patient: john.doe@example.com / Patient@123");
                    }
                    else
                    {
                        Console.WriteLine("✗ Cannot connect to database!");
                        Console.WriteLine("Please check your connection string in appsettings.json");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("✗ Error occurred:");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
