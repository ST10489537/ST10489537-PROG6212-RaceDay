/* =========================================================
   RaceDay Database Schema
   Module: PROG6212/w - Part 1 (Section C)
   Student: Nkosana | ST10489537
   Run this script on a clean SQL Server instance in SSMS.
   ========================================================= */

CREATE DATABASE RaceDayDB;
GO
USE RaceDayDB;
GO

/* ---------- ROLES ---------- */
CREATE TABLE Roles (
    RoleID      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(20) NOT NULL UNIQUE
);
GO

/* ---------- USERS ---------- */
CREATE TABLE Users (
    UserID              INT IDENTITY(1,1) PRIMARY KEY,
    RoleID              INT NOT NULL,
    FullName            NVARCHAR(100) NOT NULL,
    Email               NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash        NVARCHAR(255) NOT NULL,
    PhoneNumber         NVARCHAR(20) NULL,
    ProfilePictureUrl   NVARCHAR(500) NULL,
    CreatedAt           DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

/* ---------- EVENTS ---------- */
CREATE TABLE Events (
    EventID         INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID     INT NOT NULL,
    Name            NVARCHAR(150) NOT NULL,
    Description     NVARCHAR(1000) NULL,
    EventDate       DATE NOT NULL,
    Location        NVARCHAR(200) NOT NULL,
    DistanceKm      DECIMAL(5,2) NOT NULL,
    EventType       NVARCHAR(10) NOT NULL CHECK (EventType IN ('Run','Walk','Cycle')),
    BannerImageUrl  NVARCHAR(500) NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID) REFERENCES Users(UserID)
);
GO

/* ---------- CATEGORIES ---------- */
CREATE TABLE Categories (
    CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT NOT NULL,
    Name            NVARCHAR(50) NOT NULL,
    Description     NVARCHAR(300) NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID)
);
GO

/* ---------- ENROLMENTS ---------- */
CREATE TABLE Enrolments (
    EnrolmentID     INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID   INT NOT NULL,
    EventID         INT NOT NULL,
    CategoryID      INT NOT NULL,
    EnrolmentDate   DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status          NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending','Confirmed','Cancelled')),
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT UQ_Enrolments_ParticipantEvent UNIQUE (ParticipantID, EventID)
);
GO

/* ---------- RESULTS ---------- */
CREATE TABLE Results (
    ResultID        INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID     INT NOT NULL UNIQUE,
    FinishTime      TIME NOT NULL,
    FinishPosition  INT NOT NULL,
    CapturedAt      DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);
GO

/* =========================================================
   SEED DATA
   ========================================================= */

INSERT INTO Roles (RoleName) VALUES ('Organiser'), ('Participant');
GO

-- Passwords below are placeholder BCrypt-style hashes for seed purposes only.
INSERT INTO Users (RoleID, FullName, Email, PasswordHash, PhoneNumber) VALUES
(1, 'Thandeka Mokoena', 'thandeka.mokoena@raceday.co.za', '$2a$11$examplehash0001', '0821234567'),
(1, 'Sipho Naidoo',      'sipho.naidoo@raceday.co.za',     '$2a$11$examplehash0002', '0837654321'),
(2, 'Lindiwe Dlamini',   'lindiwe.dlamini@example.com',    '$2a$11$examplehash0003', '0731122334'),
(2, 'Johan van der Merwe','johan.vdm@example.com',         '$2a$11$examplehash0004', '0715566778');
GO

INSERT INTO Events (OrganiserID, Name, Description, EventDate, Location, DistanceKm, EventType, BannerImageUrl) VALUES
(1, 'Joburg City Marathon', 'Annual road marathon through the Johannesburg CBD.', '2026-11-14', 'Johannesburg, Gauteng', 42.20, 'Run', NULL),
(1, 'Vaal Family Fun Walk', 'Community fun walk along the Vaal riverbank.', '2026-10-04', 'Vanderbijlpark, Gauteng', 5.00, 'Walk', NULL),
(2, 'Highveld Cycle Classic', 'Road cycling event through the Highveld countryside.', '2026-12-06', 'Bronkhorstspruit, Gauteng', 94.70, 'Cycle', NULL);
GO

INSERT INTO Categories (EventID, Name, Description) VALUES
(1, '42km Open', 'Full marathon, open to all ages.'),
(1, 'Senior 42km (50+)', 'Full marathon, senior category.'),
(2, '5km Fun Walk', 'Non-competitive community walk.'),
(3, '94km Open', 'Full distance road cycling.'),
(3, '47km Half Distance', 'Half-distance road cycling.');
GO

INSERT INTO Enrolments (ParticipantID, EventID, CategoryID, Status) VALUES
(3, 1, 1, 'Confirmed'),
(4, 1, 2, 'Confirmed'),
(3, 2, 3, 'Pending'),
(4, 3, 4, 'Confirmed');
GO

INSERT INTO Results (EnrolmentID, FinishTime, FinishPosition) VALUES
(1, '03:45:12', 47),
(2, '04:02:55', 112);
GO
