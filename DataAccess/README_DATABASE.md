# Database Configuration

## Setup Connection String

1. Copy `appsettings.json` to `appsettings.Development.json`
2. Update the password in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local);Database=ClinicAppointmentBookingDB;Uid=sa;Pwd=YOUR_ACTUAL_PASSWORD;TrustServerCertificate=True"
  }
}
```

**Note**: `appsettings.Development.json` is already in `.gitignore` and will not be committed to Git.

## Migration Commands

```bash
# Navigate to DataAccess folder
cd DataAccess

# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Seed data
dotnet run
```

## Default Accounts

After seeding, you can login with:

- **Admin**: admin@mediclinic.com / Admin@123
- **Doctor 1**: emily.chen@mediclinic.com / Doctor@123
- **Doctor 2**: marcus.johnson@mediclinic.com / Doctor@123
- **Doctor 3**: sarah.patel@mediclinic.com / Doctor@123
- **Doctor 4**: david.wilson@mediclinic.com / Doctor@123
- **Patient**: john.doe@example.com / Patient@123
