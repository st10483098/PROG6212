-- =====================================================================
-- RaceDay System - Database Creation and Population Script
-- Target: Microsoft SQL Server (SSMS)
-- This script matches the ERD in /docs/erd.png exactly.
-- Run on a clean SQL Server instance. Safe to re-run (drops DB first).
-- =====================================================================

IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ---------------------------------------------------------------------
-- 1. Users
-- Stores both roles (Admin, Athlete) in a single table via RoleName.
-- ---------------------------------------------------------------------
CREATE TABLE Users (
    UserID          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100)   NOT NULL,
    Email           NVARCHAR(150)   NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(256)   NOT NULL,
    RoleName        VARCHAR(20)     NOT NULL
                        CONSTRAINT CK_Users_Role CHECK (RoleName IN ('Admin', 'Athlete')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSDATETIME()
);
GO

-- ---------------------------------------------------------------------
-- 2. Events
-- ---------------------------------------------------------------------
CREATE TABLE Events (
    EventID         INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(150)   NOT NULL,
    EventDate       DATE            NOT NULL,
    Location        NVARCHAR(150)   NOT NULL,
    Description     NVARCHAR(1000)  NULL,
    Status          VARCHAR(20)     NOT NULL DEFAULT 'Scheduled'
                        CONSTRAINT CK_Events_Status CHECK (Status IN ('Scheduled','Ongoing','Completed','Cancelled'))
);
GO

-- ---------------------------------------------------------------------
-- 3. Categories
-- ---------------------------------------------------------------------
CREATE TABLE Categories (
    CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(500)   NULL
);
GO

-- ---------------------------------------------------------------------
-- 4. EventCategories (resolves Event M:M Category)
-- ---------------------------------------------------------------------
CREATE TABLE EventCategories (
    EventCategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT NOT NULL,
    CategoryID      INT NOT NULL,
    CONSTRAINT FK_EventCategories_Event
        FOREIGN KEY (EventID) REFERENCES Events(EventID) ON DELETE CASCADE,
    CONSTRAINT FK_EventCategories_Category
        FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE CASCADE,
    CONSTRAINT UQ_EventCategory UNIQUE (EventID, CategoryID)
);
GO

-- ---------------------------------------------------------------------
-- 5. Enrolments (User M:1, EventCategory M:1)
-- ---------------------------------------------------------------------
CREATE TABLE Enrolments (
    EnrolmentID     INT IDENTITY(1,1) PRIMARY KEY,
    UserID          INT NOT NULL,
    EventCategoryID INT NOT NULL,
    EnrolmentDate   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Status          VARCHAR(20) NOT NULL DEFAULT 'Confirmed'
                        CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Confirmed','Cancelled','Waitlisted')),
    CONSTRAINT FK_Enrolments_User
        FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolments_EventCategory
        FOREIGN KEY (EventCategoryID) REFERENCES EventCategories(EventCategoryID) ON DELETE CASCADE,
    CONSTRAINT UQ_Enrolment UNIQUE (UserID, EventCategoryID)
);
GO

-- ---------------------------------------------------------------------
-- 6. Results (1:1 with Enrolment)
-- ---------------------------------------------------------------------
CREATE TABLE Results (
    ResultID        INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID     INT NOT NULL UNIQUE,
    FinishTime      TIME(0) NULL,
    Position        INT NULL,
    Status          VARCHAR(20) NOT NULL DEFAULT 'Pending'
                        CONSTRAINT CK_Results_Status CHECK (Status IN ('Pending','Finished','DNF','DSQ')),
    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID) ON DELETE CASCADE
);
GO

-- =====================================================================
-- Sample data
-- =====================================================================

INSERT INTO Users (FullName, Email, PasswordHash, RoleName) VALUES
('Nomsa Dlamini', 'nomsa.admin@raceday.com', 'HASHED_PW_1', 'Admin'),
('Sipho Ndlovu',  'sipho.athlete@raceday.com', 'HASHED_PW_2', 'Athlete'),
('Lerato Khumalo', 'lerato.athlete@raceday.com', 'HASHED_PW_3', 'Athlete');

INSERT INTO Events (Name, EventDate, Location, Description, Status) VALUES
('Durban Beachfront 10K', '2026-11-14', 'Durban Beachfront', 'Annual coastal road race', 'Scheduled'),
('KZN Trail Championship', '2026-12-05', 'Krantzkloof Nature Reserve', 'Off-road trail running event', 'Scheduled');

INSERT INTO Categories (Name, Description) VALUES
('Open 10K', 'Open category, no age restriction'),
('Under 20', 'Athletes under 20 years old'),
('Veteran 40+', 'Athletes aged 40 and above');

INSERT INTO EventCategories (EventID, CategoryID) VALUES
(1, 1), (1, 3), (2, 1), (2, 2);

INSERT INTO Enrolments (UserID, EventCategoryID, Status) VALUES
(2, 1, 'Confirmed'),
(3, 3, 'Confirmed');

INSERT INTO Results (EnrolmentID, FinishTime, Position, Status) VALUES
(1, '00:42:18', 5, 'Finished'),
(2, NULL, NULL, 'Pending');
GO

-- =====================================================================
-- Verification queries (optional - comment out before final submission
-- if your brief wants a clean DDL/DML-only script)
-- =====================================================================
-- SELECT * FROM Users;
-- SELECT * FROM Events;
-- SELECT e.Name AS EventName, c.Name AS CategoryName
-- FROM EventCategories ec
-- JOIN Events e ON e.EventID = ec.EventID
-- JOIN Categories c ON c.CategoryID = ec.CategoryID;
