# MSP Capstone Backend

<div align="center">

**Meeting Support Platform - RESTful API Backend Service**

[![. NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-99.9%25-239120?style=flat&logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Database-336791?style=flat&logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=flat&logo=docker)](https://www.docker.com/)

</div>

## 📋 Overview

The **Meeting Support Platform Backend** is a robust and scalable RESTful API service built with ASP.NET Core, designed to assist users in organizing, managing, and retrieving online meeting content efficiently. The platform provides comprehensive features for meeting management, document handling, task tracking, video conferencing integration, and AI-powered summarization.

### ✨ Key Features

- 🔐 **Authentication & Authorization**: JWT-based authentication with ASP.NET Core Identity, Google Sign-In integration
- 👥 **User Management**: Multi-role support (Admin, Business Owner, Member) with approval workflows
- 🏢 **Organization Management**: Organization invitations, member management, hierarchical structures
- 📅 **Project & Task Management**: Project creation, task assignment, milestone tracking, todo lists
- 📄 **Document Management**: Document CRUD operations, project-based organization, owner tracking
- 📹 **Video Conference Integration**: Stream Video SDK integration for high-quality video calls
- 🤖 **AI-Powered Features**: Text summarization, todo list generation, video transcription analysis
- 🔔 **Real-time Notifications**: SignalR for live updates, Firebase Cloud Messaging for push notifications
- 💳 **Payment Integration**: PayOS payment gateway with webhook support
- 📊 **Subscription Management**: Package-based subscription system with usage tracking
- 🔄 **Background Jobs**: Hangfire for scheduled tasks and background processing

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

```
MeetingSupportPlatform/
├── MSP.Domain/                 # Domain Layer
│   ├── Entities/              # Domain entities
│   └── Enums/                 # Domain enumerations
├── MSP.Application/            # Application Layer
│   ├── Services/              # Business logic services
│   │   ├── Interfaces/       # Service contracts
│   │   └── Implementations/  # Service implementations
│   ├── Models/               # DTOs (Requests/Responses)
│   ├── Repositories/         # Repository interfaces
│   └── Extensions/           # Application extensions
├── MSP.Infrastructure/         # Infrastructure Layer
│   ├── Data/                 # Database context & migrations
│   ├── Repositories/         # Repository implementations
│   ├── Configurations/       # Entity configurations
│   └── Extensions/           # Infrastructure extensions
├── MSP. Shared/                # Shared Layer
│   ├── Common/               # Common utilities
│   ├── Enums/                # Shared enumerations
│   └── Specifications/       # Specification pattern
├── MSP.WebAPI/                # Presentation Layer
│   ├── Controllers/          # API endpoints
│   ├── Hubs/                 # SignalR hubs
│   ├── Filters/              # Action filters
│   └── Program.cs            # Application entry point
└── MSP.Tests/                 # Test Layer
    └── Services/             # Unit tests
```

## 🛠️ Technology Stack

### Core Framework
- **.NET 8.0**:  Latest LTS version of .NET
- **ASP.NET Core**: Web API framework
- **C#**: 99.9% of codebase

### Database & ORM
- **PostgreSQL**: Primary database
- **Entity Framework Core**:  ORM for data access
- **Hangfire**: Background job processing with PostgreSQL storage

### Authentication & Security
- **ASP.NET Core Identity**: User management
- **JWT (JSON Web Tokens)**: Token-based authentication
- **Google Sign-In**: OAuth 2.0 integration

### Real-time Communication
- **SignalR**: Real-time notifications and updates
- **Stream Video SDK**: Video conferencing capabilities
- **Firebase Cloud Messaging**:  Push notifications

### Integration Services
- **PayOS**: Payment gateway integration
- **AI/ML Services**: Text summarization and analysis
- **Cloudinary**: Media file storage (inferred from attachments)

### Background Processing
- **Hangfire**: Job scheduling and processing
- **Hangfire Dashboard**: Job monitoring UI

### Containerization
- **Docker**: Container support with Dockerfile

## 🚀 Getting Started

### Prerequisites

- **.NET SDK 8.0** or later
- **PostgreSQL 14+**
- **Visual Studio 2022** or **VS Code** with C# extension
- **Docker** (optional, for containerized deployment)

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/LongLQ25/msp-capstone-be.git
cd msp-capstone-be/MeetingSupportPlatform
```

#### 2. Configure Database Connection

Update `appsettings.json` with your PostgreSQL connection string: 

```json
{
  "ConnectionStrings": {
    "DbConnectionString": "Host=localhost;Port=5432;Database=msp_db;Username=your_username;Password=your_password"
  }
}
```

#### 3. Configure Application Settings

Set up the required configuration in `appsettings.json`:

```json
{
  "AppSettings": {
    "ClientUrl": "http://localhost:3000"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "MSP",
    "Audience":  "MSP-Client",
    "ExpirationMinutes": 60
  }
}
```

#### 4. Set Up Firebase (for Push Notifications)

1. Download your Firebase service account JSON file
2. Place it in the project root or configure the path
3. Initialize Firebase Admin SDK in your environment

#### 5. Apply Database Migrations

```bash
dotnet ef database update --project MSP.Infrastructure --startup-project MSP.WebAPI
```

#### 6. Run the Application

```bash
dotnet run --project MSP.WebAPI
```

The API will be available at: 
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`
- **Hangfire Dashboard**: `https://localhost:5001/hangfire`

### Docker Deployment

#### Build Docker Image

```bash
docker build -t msp-backend:latest -f MeetingSupportPlatform/Dockerfile .
```

#### Run with Docker Compose

```bash
docker-compose up -d
```

## 📚 API Documentation

### Base URL

```
https://api.example.com/api/v1
```

### Main API Endpoints

#### Authentication (`/auth`)
- `POST /auth/register` - User registration
- `POST /auth/login` - User login
- `POST /auth/google-login` - Google Sign-In
- `POST /auth/refresh-token` - Refresh access token
- `POST /auth/logout` - User logout
- `POST /auth/forgot-password` - Password reset request
- `POST /auth/reset-password` - Reset password
- `GET /auth/check-phone` - Check phone number availability

#### Users (`/users`)
- `GET /users/{id}` - Get user details
- `PUT /users/{id}` - Update user profile
- `GET /users/business-owners` - Get all business owners
- `GET /users/pending-business-owners` - Get pending approvals
- `POST /users/approve/{id}` - Approve business owner
- `POST /users/reject/{id}` - Reject business owner

#### Organizations (`/organizations`)
- `POST /organizations/invite` - Send organization invitation
- `POST /organizations/accept-invite` - Accept invitation
- `POST /organizations/reject-invite` - Reject invitation
- `GET /organizations/members` - Get organization members

#### Projects (`/projects`)
- `POST /projects` - Create project
- `GET /projects` - Get all projects
- `GET /projects/{id}` - Get project by ID
- `PUT /projects/{id}` - Update project
- `DELETE /projects/{id}` - Delete project
- `GET /projects/by-owner/{ownerId}` - Get projects by owner

#### Tasks (`/tasks`)
- `POST /tasks` - Create task
- `GET /tasks/{id}` - Get task details
- `PUT /tasks/{id}` - Update task
- `DELETE /tasks/{id}` - Delete task
- `GET /tasks/by-project/{projectId}` - Get tasks by project
- `GET /tasks/by-user-and-project/{userId}/{projectId}` - Get user tasks in project
- `GET /tasks/by-milestone/{milestoneId}` - Get tasks by milestone

#### Documents (`/documents`)
- `POST /documents` - Create document
- `GET /documents/{id}` - Get document by ID
- `PUT /documents` - Update document
- `DELETE /documents/{id}` - Delete document
- `GET /documents/by-project/{projectId}` - Get documents by project
- `GET /documents/by-owner/{ownerId}` - Get documents by owner

#### Video Streaming (`/stream`)
- `POST /stream/register` - Register user for video streaming
- `POST /stream/call/{type}/{id}/delete` - Delete call
- `GET /stream/call/{type}/{id}/transcriptions` - Get call transcriptions

#### AI Summarization (`/summarize`)
- `POST /summarize/summary` - Summarize text
- `POST /summarize/create-todolist` - Generate todo list from text
- `POST /summarize/video-text-analysis` - Analyze video with transcription

#### Subscriptions (`/subscriptions`)
- `POST /subscriptions` - Purchase package
- `GET /subscriptions/active/{userId}` - Get active subscription
- `GET /subscriptions/active/{userId}/usage` - Get subscription usage
- `GET /subscriptions/{userId}` - Get user subscriptions

#### Payments (`/payments`)
- `POST /payments/webhook` - Payment webhook endpoint (PayOS)
- `POST /payments/confirm-test` - Test webhook configuration

#### Notifications
- **SignalR Hub**: `/notificationHub` or `/api/v1/notificationHub`
  - Real-time notifications
  - Connection management
  - Broadcast messages

### Authentication

All protected endpoints require JWT token in the Authorization header: 

```
Authorization: Bearer <your_jwt_token>
```

### Swagger Documentation

Access interactive API documentation at `/swagger` when running in development mode.

## 🧪 Testing

### Run Unit Tests

```bash
dotnet test MSP.Tests/MSP.Tests.csproj
```

### Test Coverage

The project includes comprehensive unit tests for:
- User services (registration, profile updates, approval workflows)
- Authentication services (login, password reset, token management)
- Organization services (invitations, member management)
- Account services

## 📦 Database Schema

### Main Entities

- **Users**: User accounts with role-based access
- **Organizations**: Business organizations and teams
- **OrganizationInvitations**: Invitation management
- **Projects**: Project management entities
- **ProjectMembers**: Project membership
- **ProjectTasks**: Task tracking
- **Documents**: Document management
- **Subscriptions**: Subscription plans and tracking
- **Packages**: Available subscription packages
- **Notifications**: In-app notifications
- **TaskAttachments**: File attachments for tasks

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Password Hashing**: ASP.NET Core Identity password hashing
- **Role-Based Authorization**: Admin, Business Owner, Member roles
- **HTTPS Enforcement**: Secure communication
- **CORS Configuration**:  Controlled cross-origin requests
- **Rate Limiting**: API rate limiting (via MemoryCache)
- **SQL Injection Prevention**: Entity Framework parameterized queries

## 🌐 Environment Configuration

### Development

```json
{
  "Environment": "Development",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Production

- Use environment variables for sensitive configuration
- Enable HTTPS redirection
- Configure proper CORS policies
- Set up application insights/logging
- Configure database connection pooling

## 📊 Monitoring & Logging

- **Hangfire Dashboard**: Job monitoring at `/hangfire`
- **Application Logging**: Structured logging with ILogger
- **Request/Response Tracing**: Custom middleware for request tracing

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Write unit tests for new features
- Update API documentation
- Use meaningful commit messages

## 🐛 Bug Reports & Issues

If you find a bug or have suggestions, please [create an issue](https://github.com/LongLQ25/msp-capstone-be/issues/new).

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Authors & Contributor

**LongLQ25**
- GitHub: [@LongLQ25](https://github.com/LongLQ25)
  
**DatHuynh2003**
- GitHub: [@dathuynh2003](https://github.com/dathuynh2003)
  
**PMKTien3101**
- GitHub: [@pmktien3101](https://github.com/pmktien3101)
  
**PhuocLoc3012**
- GitHub: [PhuocLoc3012](https://github.com/PhuocLoc3012)

## 🙏 Acknowledgments

- [ASP.NET Core Team](https://dotnet.microsoft.com/) - Excellent framework
- [Entity Framework Core](https://docs.microsoft.com/ef/core/) - Powerful ORM
- [Hangfire](https://www.hangfire.io/) - Background job processing
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) - Real-time communication
- [Stream](https://getstream.io/) - Video infrastructure
- [Firebase](https://firebase.google.com/) - Cloud services

---

<div align="center">

**⭐ If you find this project useful, please give it a star! ⭐**

Made with ❤️ using ASP.NET Core

</div>
