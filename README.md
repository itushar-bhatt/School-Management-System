# School Management System - Authentication

A complete authentication system with role-based access control for managing schools.

## Features

- **Role-Based Authentication**: Four user roles (Admin, Teacher, Student, Parent)
- **Secure Login**: ASP.NET Core Identity with password hashing
- **Admin Dashboard**: Manage users, add teachers, students, and parents
- **Role-Specific Dashboards**: Each role has a dedicated dashboard
- **Automatic Redirection**: Users are redirected to their role-specific dashboard after login

## Default Admin Credentials

- **Username**: `admin`
- **Password**: `Admin@123`
- **Email**: `admin@school.com`

## User Roles

### Admin
- Can add new users (Teachers, Students, Parents)
- Can manage all users
- Can delete users
- Access to admin dashboard

### Teacher
- View class schedules
- Manage student information
- Mark attendance
- Enter grades and marks
- Create assignments and tests

### Student
- View class schedule
- Check assignments and homework
- View grades and report cards
- Check attendance records
- Access study materials

### Parent
- View children's information
- Check attendance records
- View grades and report cards
- Communicate with teachers
- View school announcements

## How to Run

1. Restore dependencies:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run --project SchoolManagementSystem.API
   ```

3. Open your browser and navigate to:
   ```
   https://localhost:5001
   ```
   or
   ```
   http://localhost:5000
   ```

4. Login with the default admin credentials:
   - Username: `admin`
   - Password: `Admin@123`

## Project Structure

```
SchoolManagementSystem.API/
├── Controllers/
│   ├── AccountController.cs      # Login, Register, Logout
│   ├── AdminController.cs        # Admin dashboard and user management
│   ├── TeacherController.cs      # Teacher dashboard
│   ├── StudentController.cs      # Student dashboard
│   └── ParentController.cs       # Parent dashboard
├── Views/
│   ├── Account/
│   │   └── Login.cshtml          # Login page
│   ├── Admin/
│   │   ├── Index.cshtml          # Admin dashboard
│   │   ├── AddUser.cshtml        # Add new user form
│   │   └── ManageUsers.cshtml    # Manage all users
│   ├── Teacher/
│   │   └── Index.cshtml          # Teacher dashboard
│   ├── Student/
│   │   └── Index.cshtml          # Student dashboard
│   ├── Parent/
│   │   └── Index.cshtml          # Parent dashboard
│   └── Shared/
│       ├── _Layout.cshtml        # Main layout
│       └── _ValidationScriptsPartial.cshtml
├── Program.cs                    # Application entry point
└── appsettings.json              # Configuration

SchoolManagementSystem.Infrastructure/
└── Identity/
    ├── ApplicationUser.cs        # Custom user class
    └── ApplicationDbContext.cs   # Database context
```

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Authentication**: ASP.NET Core Identity
- **Database**: SQLite (cross-platform)
- **ORM**: Entity Framework Core 8.0
- **UI**: Bootstrap 5.3, Font Awesome 4.7
- **Architecture**: MVC (Model-View-Controller)

## Database

The application uses SQLite database stored in `SchoolManagementDB.db`. The database is automatically created when the application runs for the first time, including:
- All Identity tables (Users, Roles, Claims, etc.)
- Seeded admin user
- Seeded roles (Admin, Teacher, Student, Parent)

## Security Features

- Password hashing (never stored in plain text)
- Password requirements (minimum 6 characters, uppercase, lowercase, digit)
- Cookie-based authentication
- Role-based authorization
- Anti-forgery tokens on forms
- Secure cookie configuration

## Adding New Users

1. Login as admin
2. Navigate to "Add New User" or "Manage Users"
3. Fill in the user details:
   - Full Name
   - Username
   - Email
   - Role (Teacher, Student, or Parent)
   - Password
4. Click "Add User"

The new user will be created and can login immediately with their credentials.

## Notes

- The application runs on a random port by default in development mode
- SQLite database file will be created in the application root directory
- All passwords must meet the minimum requirements
- Admin can only create Teacher, Student, and Parent roles (not other admins)