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

-- Bảng lưu thông tin giảng viên
IF OBJECT_ID(N'dbo.Instructors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Instructors
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        PhoneNumber NVARCHAR(50) NULL,
        HourlyRate DECIMAL(18, 2) NOT NULL DEFAULT 0,
        GoogleSubject NVARCHAR(200) NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );

    CREATE UNIQUE INDEX IX_Instructors_Email ON dbo.Instructors(Email);
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
        InstructorId UNIQUEIDENTIFIER NOT NULL
    );

    ALTER TABLE dbo.TrainingClasses
        ADD CONSTRAINT FK_TrainingClasses_Instructors
            FOREIGN KEY (InstructorId) REFERENCES dbo.Instructors(Id)
            ON DELETE NO ACTION;

    CREATE UNIQUE INDEX IX_TrainingClasses_Code ON dbo.TrainingClasses(Code);
END
GO

-- Bảng lịch học chi tiết theo từng buổi
IF OBJECT_ID(N'dbo.ClassSchedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ClassSchedules
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TrainingClassId UNIQUEIDENTIFIER NOT NULL,
        StudyDate DATE NOT NULL,
        StartTime TIME NOT NULL,
        EndTime TIME NOT NULL,
        DayOfWeek INT NOT NULL,
        LocationName NVARCHAR(200) NOT NULL,
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        AllowedRadiusMeters DECIMAL(18, 2) NOT NULL
    );

    ALTER TABLE dbo.ClassSchedules
        ADD CONSTRAINT FK_ClassSchedules_TrainingClasses
            FOREIGN KEY (TrainingClassId) REFERENCES dbo.TrainingClasses(Id)
            ON DELETE CASCADE;

    CREATE UNIQUE INDEX IX_ClassSchedules_ClassDate
        ON dbo.ClassSchedules(TrainingClassId, StudyDate);
END
GO

-- Bảng phiếu hỗ trợ/điều chỉnh điểm danh
IF OBJECT_ID(N'dbo.AttendanceTickets', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceTickets
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
        InstructorId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Reason NVARCHAR(500) NOT NULL,
        CreatedBy NVARCHAR(200) NOT NULL,
        IsApproved BIT NOT NULL DEFAULT 0,
        ApprovedBy NVARCHAR(200) NULL,
        ApprovedAt DATETIME2 NULL
    );

    ALTER TABLE dbo.AttendanceTickets
        ADD CONSTRAINT FK_AttendanceTickets_ClassSchedules
            FOREIGN KEY (ClassScheduleId) REFERENCES dbo.ClassSchedules(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.AttendanceTickets
        ADD CONSTRAINT FK_AttendanceTickets_Instructors
            FOREIGN KEY (InstructorId) REFERENCES dbo.Instructors(Id)
            ON DELETE NO ACTION;
END
GO

-- Bảng ghi nhận điểm danh
IF OBJECT_ID(N'dbo.AttendanceRecords', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttendanceRecords
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ClassScheduleId UNIQUEIDENTIFIER NOT NULL,
        InstructorId UNIQUEIDENTIFIER NOT NULL,
        CheckedInAt DATETIME2 NOT NULL,
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        Status INT NOT NULL DEFAULT 0,
        Notes NVARCHAR(500) NULL,
        TicketId UNIQUEIDENTIFIER NULL
    );

    ALTER TABLE dbo.AttendanceRecords
        ADD CONSTRAINT FK_AttendanceRecords_ClassSchedules
            FOREIGN KEY (ClassScheduleId) REFERENCES dbo.ClassSchedules(Id)
            ON DELETE CASCADE;

    ALTER TABLE dbo.AttendanceRecords
        ADD CONSTRAINT FK_AttendanceRecords_Instructors
            FOREIGN KEY (InstructorId) REFERENCES dbo.Instructors(Id)
            ON DELETE NO ACTION;

    ALTER TABLE dbo.AttendanceRecords
        ADD CONSTRAINT FK_AttendanceRecords_Tickets
            FOREIGN KEY (TicketId) REFERENCES dbo.AttendanceTickets(Id)
            ON DELETE SET NULL;

    CREATE UNIQUE INDEX IX_AttendanceRecords_TicketId
        ON dbo.AttendanceRecords(TicketId)
        WHERE TicketId IS NOT NULL;
END
GO

-- Bảng kỳ lương và chi tiết lương
IF OBJECT_ID(N'dbo.PayrollPeriods', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PayrollPeriods
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        InstructorId UNIQUEIDENTIFIER NOT NULL,
        [Year] INT NOT NULL,
        [Month] INT NOT NULL,
        TotalHours DECIMAL(18, 2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
        GeneratedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    ALTER TABLE dbo.PayrollPeriods
        ADD CONSTRAINT FK_PayrollPeriods_Instructors
            FOREIGN KEY (InstructorId) REFERENCES dbo.Instructors(Id)
            ON DELETE CASCADE;

    CREATE UNIQUE INDEX IX_PayrollPeriods_PerInstructor
        ON dbo.PayrollPeriods(InstructorId, [Year], [Month]);
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
        Amount DECIMAL(18, 2) NOT NULL
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
