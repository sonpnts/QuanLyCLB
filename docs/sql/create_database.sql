-- Script khởi tạo cơ sở dữ liệu SQL Server cho ứng dụng Quản Lý CLB
-- Có thể chạy trong SQL Server Management Studio hoặc sqlcmd

IF DB_ID(N'QuanLyCLB') IS NULL
BEGIN
    PRINT 'Tạo mới database QuanLyCLB...';
    CREATE DATABASE QuanLyCLB;
END
GO

USE QuanLyCLB;
GO

-- Bảng lưu thông tin người dùng chung
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Username NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        PhoneNumber NVARCHAR(50) NOT NULL,
        AvatarUrl NVARCHAR(1024) NULL,
        GoogleSubject NVARCHAR(200) NULL,
        PasswordHash NVARCHAR(512) NULL,
        PasswordSalt NVARCHAR(256) NULL,
        SkillLevel NVARCHAR(200) NOT NULL DEFAULT '',
        Certification NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
    CREATE UNIQUE INDEX IX_Users_Username ON dbo.Users(Username);
END
GO

-- Bảng lưu log đăng nhập của người dùng
IF OBJECT_ID(N'dbo.LoginAuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LoginAuditLogs
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserAccountId UNIQUEIDENTIFIER NULL,
        Username NVARCHAR(200) NOT NULL,
        Provider NVARCHAR(100) NOT NULL,
        IsSuccess BIT NOT NULL,
        ApiEndpoint NVARCHAR(400) NOT NULL,
        LocationAddress NVARCHAR(500) NULL,
        DeviceInfo NVARCHAR(500) NULL,
        IpAddress NVARCHAR(100) NULL,
        Message NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL
    );

    ALTER TABLE dbo.LoginAuditLogs
        ADD CONSTRAINT FK_LoginAuditLogs_Users
            FOREIGN KEY (UserAccountId) REFERENCES dbo.Users(Id)
            ON DELETE SET NULL;
END
GO

-- Bảng quy định tiền lương theo vai trò và trình độ
IF OBJECT_ID(N'dbo.PayrollRules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayrollRules
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        RoleName NVARCHAR(100) NOT NULL,
        SkillLevel NVARCHAR(200) NOT NULL,
        HourlyRate DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Notes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE UNIQUE INDEX IX_PayrollRules_Role_Skill
        ON dbo.PayrollRules(RoleName, SkillLevel);
END
GO

-- Bảng lớp đào tạo
IF OBJECT_ID(N'dbo.TrainingClasses', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingClasses
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        MaxStudents INT NOT NULL,
        CoachId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    ALTER TABLE dbo.TrainingClasses
        ADD CONSTRAINT FK_TrainingClasses_Users
            FOREIGN KEY (CoachId) REFERENCES dbo.Users(Id)
            ON DELETE NO ACTION;

    CREATE UNIQUE INDEX IX_TrainingClasses_Code ON dbo.TrainingClasses(Code);
END
GO

-- Bảng chi nhánh lưu thông tin địa điểm điểm danh
IF OBJECT_ID(N'dbo.Branches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Branches
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Address NVARCHAR(500) NOT NULL,
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        AllowedRadiusMeters FLOAT NOT NULL,
        GooglePlaceId NVARCHAR(200) NULL,
        GoogleMapsEmbedUrl NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE UNIQUE INDEX IX_Branches_Name ON dbo.Branches(Name);
END
GO

-- Bảng lịch học theo tuần của lớp
IF OBJECT_ID(N'dbo.ClassSchedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassSchedules
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TrainingClassId UNIQUEIDENTIFIER NOT NULL,
        DayOfWeek INT NOT NULL,
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        BranchId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    ALTER TABLE dbo.ClassSchedules
        ADD CONSTRAINT FK_ClassSchedules_TrainingClasses
            FOREIGN KEY (TrainingClassId) REFERENCES dbo.TrainingClasses(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.ClassSchedules
        ADD CONSTRAINT FK_ClassSchedules_Branches
            FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
            ON DELETE NO ACTION;

    CREATE UNIQUE INDEX IX_ClassSchedules_TrainingClass_Day
        ON dbo.ClassSchedules(TrainingClassId, DayOfWeek);

    CREATE UNIQUE INDEX IX_ClassSchedules_TrainingClass_Day_Time
        ON dbo.ClassSchedules(TrainingClassId, DayOfWeek, StartTime, EndTime);
END
GO

-- Bảng phân công trợ giảng cho lớp và lịch học
IF OBJECT_ID(N'dbo.ClassAssistantAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassAssistantAssignments
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TrainingClassId UNIQUEIDENTIFIER NOT NULL,
        ClassScheduleId UNIQUEIDENTIFIER NULL,
        AssistantId UNIQUEIDENTIFIER NOT NULL,
        RoleName NVARCHAR(100) NOT NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        Notes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    ALTER TABLE dbo.ClassAssistantAssignments
        ADD CONSTRAINT FK_ClassAssistantAssignments_TrainingClasses
            FOREIGN KEY (TrainingClassId) REFERENCES dbo.TrainingClasses(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.ClassAssistantAssignments
        ADD CONSTRAINT FK_ClassAssistantAssignments_ClassSchedules
            FOREIGN KEY (ClassScheduleId) REFERENCES dbo.ClassSchedules(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.ClassAssistantAssignments
        ADD CONSTRAINT FK_ClassAssistantAssignments_Users
            FOREIGN KEY (AssistantId) REFERENCES dbo.Users(Id)
            ON DELETE NO ACTION;

    CREATE INDEX IX_ClassAssistantAssignments_AssistantId
        ON dbo.ClassAssistantAssignments(AssistantId);

    CREATE INDEX IX_ClassAssistantAssignments_ClassScheduleId
        ON dbo.ClassAssistantAssignments(ClassScheduleId);

    CREATE INDEX IX_ClassAssistantAssignments_TrainingClassId
        ON dbo.ClassAssistantAssignments(TrainingClassId);
END
GO

-- Bảng phiếu hỗ trợ/điều chỉnh điểm danh
IF OBJECT_ID(N'dbo.AttendanceTickets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceTickets
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
        CoachId UNIQUEIDENTIFIER NOT NULL,
        Reason NVARCHAR(500) NOT NULL,
        CreatedBy NVARCHAR(200) NOT NULL,
        IsApproved BIT NOT NULL DEFAULT 0,
        ApprovedBy NVARCHAR(200) NULL,
        ApprovedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    ALTER TABLE dbo.AttendanceTickets
        ADD CONSTRAINT FK_AttendanceTickets_ClassSchedules
            FOREIGN KEY (ClassScheduleId) REFERENCES dbo.ClassSchedules(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.AttendanceTickets
        ADD CONSTRAINT FK_AttendanceTickets_Users
            FOREIGN KEY (CoachId) REFERENCES dbo.Users(Id)
            ON DELETE NO ACTION;

    CREATE INDEX IX_AttendanceTickets_CoachId
        ON dbo.AttendanceTickets(CoachId);
END
GO

-- Bảng ghi nhận điểm danh
IF OBJECT_ID(N'dbo.AttendanceRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceRecords
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
        CoachId UNIQUEIDENTIFIER NOT NULL,
        CheckedInAt DATETIME2 NOT NULL,
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        Status INT NOT NULL DEFAULT 0,
        Notes NVARCHAR(500) NULL,
        TicketId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    ALTER TABLE dbo.AttendanceRecords
        ADD CONSTRAINT FK_AttendanceRecords_ClassSchedules
            FOREIGN KEY (ClassScheduleId) REFERENCES dbo.ClassSchedules(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.AttendanceRecords
        ADD CONSTRAINT FK_AttendanceRecords_Users
            FOREIGN KEY (CoachId) REFERENCES dbo.Users(Id)
            ON DELETE NO ACTION;

    ALTER TABLE dbo.AttendanceRecords
        ADD CONSTRAINT FK_AttendanceRecords_Tickets
            FOREIGN KEY (TicketId) REFERENCES dbo.AttendanceTickets(Id)
            ON DELETE SET NULL;

    CREATE UNIQUE INDEX IX_AttendanceRecords_TicketId
        ON dbo.AttendanceRecords(TicketId)
        WHERE TicketId IS NOT NULL;

    CREATE INDEX IX_AttendanceRecords_CoachId
        ON dbo.AttendanceRecords(CoachId);
END
GO

-- Bảng kỳ lương và chi tiết lương
IF OBJECT_ID(N'dbo.PayrollPeriods', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayrollPeriods
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        CoachId UNIQUEIDENTIFIER NOT NULL,
        [Year] INT NOT NULL,
        [Month] INT NOT NULL,
        TotalHours DECIMAL(18, 2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
        GeneratedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    ALTER TABLE dbo.PayrollPeriods
        ADD CONSTRAINT FK_PayrollPeriods_Users
            FOREIGN KEY (CoachId) REFERENCES dbo.Users(Id)
            ON DELETE CASCADE;

    CREATE UNIQUE INDEX IX_PayrollPeriods_PerCoach
        ON dbo.PayrollPeriods(CoachId, [Year], [Month]);
END
GO

IF OBJECT_ID(N'dbo.PayrollDetails', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayrollDetails
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        PayrollPeriodId UNIQUEIDENTIFIER NOT NULL,
        AttendanceRecordId UNIQUEIDENTIFIER NOT NULL,
        Hours DECIMAL(18, 2) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CreatedByUserId UNIQUEIDENTIFIER NULL,
        UpdatedByUserId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    ALTER TABLE dbo.PayrollDetails
        ADD CONSTRAINT FK_PayrollDetails_PayrollPeriods
            FOREIGN KEY (PayrollPeriodId) REFERENCES dbo.PayrollPeriods(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.PayrollDetails
        ADD CONSTRAINT FK_PayrollDetails_AttendanceRecords
            FOREIGN KEY (AttendanceRecordId) REFERENCES dbo.AttendanceRecords(Id)
            ON DELETE NO ACTION;
END
GO
