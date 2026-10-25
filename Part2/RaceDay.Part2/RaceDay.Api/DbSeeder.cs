using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(RaceDayDbContext db)
        {
            if (db.Users.Any()) return;

            const string testPassword = "Password123!";

            // Users (RoleID 1 = Organiser, 2 = Participant, seeded in the DbContext)
            var thandeka = new User { RoleID = 1, FullName = "Thandeka Mokoena", Email = "thandeka.mokoena@raceday.co.za", PasswordHash = PasswordHasher.Hash(testPassword), PhoneNumber = "0821234567" };
            var sipho = new User { RoleID = 1, FullName = "Sipho Naidoo", Email = "sipho.naidoo@raceday.co.za", PasswordHash = PasswordHasher.Hash(testPassword), PhoneNumber = "0837654321" };
            var lindiwe = new User { RoleID = 2, FullName = "Lindiwe Dlamini", Email = "lindiwe.dlamini@example.com", PasswordHash = PasswordHasher.Hash(testPassword), PhoneNumber = "0731122334" };
            var johan = new User { RoleID = 2, FullName = "Johan van der Merwe", Email = "johan.vdm@example.com", PasswordHash = PasswordHasher.Hash(testPassword), PhoneNumber = "0715566778" };
            db.Users.AddRange(thandeka, sipho, lindiwe, johan);
            db.SaveChanges();

            // Events
            var marathon = new Event { OrganiserID = thandeka.UserID, Name = "Joburg City Marathon", Description = "Annual road marathon through the Johannesburg CBD.", EventDate = new DateTime(2026, 11, 14), Location = "Johannesburg, Gauteng", DistanceKm = 42.20m, EventType = EventType.Run };
            var walk = new Event { OrganiserID = thandeka.UserID, Name = "Vaal Family Fun Walk", Description = "Community fun walk along the Vaal riverbank.", EventDate = new DateTime(2026, 10, 4), Location = "Vanderbijlpark, Gauteng", DistanceKm = 5.00m, EventType = EventType.Walk };
            var cycle = new Event { OrganiserID = sipho.UserID, Name = "Highveld Cycle Classic", Description = "Road cycling event through the Highveld countryside.", EventDate = new DateTime(2026, 12, 6), Location = "Bronkhorstspruit, Gauteng", DistanceKm = 94.70m, EventType = EventType.Cycle };
            db.Events.AddRange(marathon, walk, cycle);
            db.SaveChanges();

            // Categories
            var open42 = new Category { EventID = marathon.EventID, Name = "42km Open", Description = "Full marathon, open to all ages." };
            var senior42 = new Category { EventID = marathon.EventID, Name = "Senior 42km (50+)", Description = "Full marathon, senior category." };
            var fun5 = new Category { EventID = walk.EventID, Name = "5km Fun Walk", Description = "Non-competitive community walk." };
            var open94 = new Category { EventID = cycle.EventID, Name = "94km Open", Description = "Full distance road cycling." };
            var half47 = new Category { EventID = cycle.EventID, Name = "47km Half Distance", Description = "Half-distance road cycling." };
            db.Categories.AddRange(open42, senior42, fun5, open94, half47);
            db.SaveChanges();

            // Enrolments
            var e1 = new Enrolment { ParticipantID = lindiwe.UserID, EventID = marathon.EventID, CategoryID = open42.CategoryID, Status = EnrolmentStatus.Confirmed };
            var e2 = new Enrolment { ParticipantID = johan.UserID, EventID = marathon.EventID, CategoryID = senior42.CategoryID, Status = EnrolmentStatus.Confirmed };
            var e3 = new Enrolment { ParticipantID = lindiwe.UserID, EventID = walk.EventID, CategoryID = fun5.CategoryID, Status = EnrolmentStatus.Pending };
            var e4 = new Enrolment { ParticipantID = johan.UserID, EventID = cycle.EventID, CategoryID = open94.CategoryID, Status = EnrolmentStatus.Confirmed };
            db.Enrolments.AddRange(e1, e2, e3, e4);
            db.SaveChanges();

            // Results
            db.Results.AddRange(
                new Result { EnrolmentID = e1.EnrolmentID, FinishTime = new TimeSpan(3, 45, 12), FinishPosition = 47 },
                new Result { EnrolmentID = e2.EnrolmentID, FinishTime = new TimeSpan(4, 2, 55), FinishPosition = 112 });
            db.SaveChanges();
        }
    }
}