# QuanLyCLB - Club Management System

A comprehensive club management system built with ASP.NET Core 8.0 Web API, designed for managing students, classes, attendance, payments, and schedules in sports clubs, training centers, or educational institutions.

## Features

### Core Functionality
- **User Management**: Admin, Trainer, and Assistant roles with role-based access control
- **Student Management**: Complete student profiles with parent information and status tracking
- **Class Management**: Create, manage, and copy classes with trainer/assistant assignments
- **Enrollment System**: Student enrollment with transfer capabilities between classes
- **Attendance Tracking**: Digital attendance with photo capture and bulk recording
- **Payment Management**: Fee collection, payment tracking, and automated monthly fee generation
- **Scheduling**: Trainer and assistant scheduling with conflict detection
- **Comprehensive Reporting**: Dashboard, financial reports, attendance reports, and student lists

### Authentication & Security
- **JWT Authentication**: Secure token-based authentication
- **Google OAuth Integration**: Login with Google accounts
- **Role-based Authorization**: Different access levels for Admins, Trainers, and Assistants
- **Secure File Upload**: Photo upload for attendance tracking

### API Features
- **RESTful API Design**: Clean, consistent API endpoints
- **Swagger Documentation**: Interactive API documentation
- **CORS Support**: Cross-origin resource sharing enabled
- **Entity Framework Core**: Database operations with SQL Server support
- **Input Validation**: Comprehensive data validation and error handling

## Technology Stack

- **Backend**: ASP.NET Core 8.0 Web API
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT + Google OAuth 2.0
- **Documentation**: Swagger/OpenAPI
- **File Storage**: Local file system for photo uploads

## API Endpoints

### Authentication
- `POST /api/auth/login` - Email-based login
- `POST /api/auth/google-login` - Google OAuth login
- `POST /api/auth/register` - User registration

### User Management
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user (Admin only)
- `PUT /api/users/{id}` - Update user (Admin only)
- `DELETE /api/users/{id}` - Delete user (Admin only)
- `GET /api/users/trainers` - Get all trainers
- `GET /api/users/assistants` - Get all assistants

### Student Management
- `GET /api/students` - Get all students
- `GET /api/students/{id}` - Get student by ID
- `POST /api/students` - Create new student
- `PUT /api/students/{id}` - Update student
- `DELETE /api/students/{id}` - Delete student (Admin only)
- `GET /api/students/active` - Get active students

### Class Management
- `GET /api/classes` - Get all classes
- `GET /api/classes/{id}` - Get class by ID
- `POST /api/classes` - Create new class
- `PUT /api/classes/{id}` - Update class
- `DELETE /api/classes/{id}` - Delete class (Admin only)
- `POST /api/classes/{id}/copy` - Copy existing class

### Enrollment Management
- `GET /api/enrollments` - Get all enrollments
- `GET /api/enrollments/class/{classId}` - Get enrollments by class
- `POST /api/enrollments` - Create new enrollment
- `POST /api/enrollments/transfer` - Transfer student between classes
- `PUT /api/enrollments/{id}/complete` - Complete enrollment
- `DELETE /api/enrollments/{id}` - Delete enrollment (Admin only)

### Attendance Tracking
- `GET /api/attendance/class/{classId}` - Get attendance by class
- `GET /api/attendance/student/{studentId}` - Get attendance by student
- `POST /api/attendance` - Record single attendance
- `POST /api/attendance/bulk` - Record bulk attendance
- `POST /api/attendance/{id}/upload-photo` - Upload attendance photo
- `PUT /api/attendance/{id}` - Update attendance
- `DELETE /api/attendance/{id}` - Delete attendance (Admin only)

### Payment Management
- `GET /api/payments` - Get all payments
- `GET /api/payments/student/{studentId}` - Get payments by student
- `GET /api/payments/overdue` - Get overdue payments
- `POST /api/payments` - Create new payment
- `POST /api/payments/process` - Process payment
- `POST /api/payments/generate-monthly` - Generate monthly fees (Admin only)
- `GET /api/payments/report` - Get payment report
- `DELETE /api/payments/{id}` - Delete payment (Admin only)

### Schedule Management
- `GET /api/schedules` - Get all schedules
- `GET /api/schedules/class/{classId}` - Get schedules by class
- `GET /api/schedules/user/{userId}` - Get schedules by user
- `GET /api/schedules/weekly` - Get weekly schedule view
- `POST /api/schedules` - Create new schedule
- `PUT /api/schedules/{id}` - Update schedule
- `PUT /api/schedules/{id}/deactivate` - Deactivate schedule
- `DELETE /api/schedules/{id}` - Delete schedule (Admin only)

### Reports & Analytics
- `GET /api/reports/dashboard` - Get dashboard summary
- `GET /api/reports/students` - Get student report
- `GET /api/reports/classes` - Get class report
- `GET /api/reports/attendance/{classId}` - Get attendance report
- `GET /api/reports/financial` - Get financial report
- `GET /api/reports/class-students/{classId}` - Get class student list
- `GET /api/reports/student-fees/{studentId}` - Get student fee report

## Database Models

### Core Entities
- **User**: System users (Admin, Trainer, Assistant)
- **Student**: Student profiles with parent information
- **Class**: Training classes with fee structure
- **Enrollment**: Student-class relationships
- **Attendance**: Attendance records with photo support
- **Payment**: Payment tracking and fee management
- **Schedule**: Class schedules for trainers and assistants

### Key Relationships
- Users can be assigned as trainers or assistants to classes
- Students can be enrolled in multiple classes
- Attendance is tracked per student per class per session
- Payments are linked to students and optionally to specific classes
- Schedules define when classes occur and who conducts them

## Setup and Installation

### Prerequisites
- .NET 8.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio or VS Code (optional)

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd QuanLyCLB/QuanLyCLB.API
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure database connection**
   Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuanLyCLBDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Configure authentication**
   Update JWT and Google OAuth settings in `appsettings.json`:
   ```json
   {
     "Jwt": {
       "SecretKey": "your-secret-key-here",
       "Issuer": "QuanLyCLB",
       "Audience": "QuanLyCLB"
     },
     "Google": {
       "ClientId": "your-google-client-id",
       "ClientSecret": "your-google-client-secret"
     }
   }
   ```

5. **Create database**
   The database will be created automatically when the application starts.

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access Swagger documentation**
   Navigate to `https://localhost:5001/swagger` or `http://localhost:5000/swagger`

## Configuration

### Environment Variables
You can also configure the application using environment variables:
- `ConnectionStrings__DefaultConnection`: Database connection string
- `Jwt__SecretKey`: JWT secret key
- `Google__ClientId`: Google OAuth client ID
- `Google__ClientSecret`: Google OAuth client secret

### File Upload Configuration
Photos are stored in the `wwwroot/uploads/attendance` directory. Ensure the application has write permissions to this location.

## Security Considerations

1. **Change default JWT secret**: Update the JWT secret key in production
2. **Use HTTPS**: Enable HTTPS in production environments
3. **Configure CORS**: Restrict CORS origins in production
4. **File upload security**: Validate file types and sizes for photo uploads
5. **Database security**: Use proper SQL Server authentication in production
6. **Google OAuth**: Configure proper redirect URIs and restrict API keys

## Usage Examples

### Authentication
```bash
# Login with email
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@example.com"}'

# Register new user
curl -X POST "http://localhost:5000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"fullName": "John Doe", "email": "john@example.com", "phoneNumber": "123456789"}'
```

### Student Management
```bash
# Create student
curl -X POST "http://localhost:5000/api/students" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"fullName": "Jane Smith", "email": "jane@example.com", "dateOfBirth": "2010-01-01", "parentName": "John Smith", "parentPhone": "987654321"}'
```

### Class Management
```bash
# Create class
curl -X POST "http://localhost:5000/api/classes" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Beginner Soccer", "maxStudents": 20, "feePerMonth": 100.00, "startDate": "2024-01-01", "trainerId": 1}'
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions, please create an issue in the repository or contact the development team.

## Roadmap

### Future Enhancements
- [ ] Mobile app integration
- [ ] Email notifications for payments and attendance
- [ ] SMS reminders
- [ ] Advanced reporting with charts
- [ ] Multi-language support
- [ ] Integration with payment gateways
- [ ] Automated backup system
- [ ] Advanced user permissions
- [ ] Class capacity management alerts
- [ ] Parent portal access

## Architecture

### Project Structure
```
QuanLyCLB.API/
├── Controllers/          # API controllers
├── Models/              # Entity models
├── DTOs/                # Data Transfer Objects
├── Data/                # Entity Framework context
├── Services/            # Business logic services
├── wwwroot/             # Static files and uploads
├── Properties/          # Launch settings
├── appsettings.json     # Configuration
└── Program.cs           # Application entry point
```

### Design Patterns
- **Repository Pattern**: Implemented through Entity Framework
- **DTO Pattern**: Separate DTOs for API responses
- **Dependency Injection**: Built-in .NET DI container
- **MVC Pattern**: Controller-based API design
- **Unit of Work**: Entity Framework context