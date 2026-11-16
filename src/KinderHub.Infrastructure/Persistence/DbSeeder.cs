using KinderHub.Domain.Entities;
using KinderHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace KinderHub.Infrastructure.Persistence;

// Fills an empty database with demo data so the app is not empty.
public static class DbSeeder
{
    public static async Task SeedAsync(KinderHubDbContext db)
    {
        if (await db.Groups.AnyAsync()) return; // already has data

        var sunflowers = new Group { Name = "Sunflowers", AgeRange = "2-3 years", Room = "101", TeacherName = "Aigerim Sadykova", Capacity = 12 };
        var rainbows = new Group { Name = "Rainbows", AgeRange = "3-4 years", Room = "102", TeacherName = "Dana Akhmetova", Capacity = 15 };
        var stars = new Group { Name = "Little Stars", AgeRange = "5-6 years", Room = "201", TeacherName = "Madina Bekova", Capacity = 18 };
        db.Groups.AddRange(sunflowers, rainbows, stars);

        var children = new List<Child>
        {
            new() { FirstName = "Aruzhan",  LastName = "Serikova",    DateOfBirth = new DateOnly(2023, 3, 14),  Group = sunflowers, ParentName = "Serik Nurlanov",      ParentEmail = "parent@kinderhub.kz",  ParentPhone = "+7 701 111 2233" },
            new() { FirstName = "Alikhan",  LastName = "Serikov",     DateOfBirth = new DateOnly(2021, 6, 2),   Group = stars,      ParentName = "Serik Nurlanov",      ParentEmail = "parent@kinderhub.kz",  ParentPhone = "+7 701 111 2233" },
            new() { FirstName = "Amina",    LastName = "Zhumabekova", DateOfBirth = new DateOnly(2023, 9, 21),  Group = sunflowers, ParentName = "Gulnara Zhumabekova", ParentEmail = "gulnara@example.kz",   ParentPhone = "+7 702 222 3344" },
            new() { FirstName = "Nurislam", LastName = "Abenov",      DateOfBirth = new DateOnly(2023, 1, 5),   Group = sunflowers, ParentName = "Yerlan Abenov",       ParentEmail = "yerlan@example.kz",    ParentPhone = "+7 705 333 4455" },
            new() { FirstName = "Sofia",    LastName = "Ivanova",     DateOfBirth = new DateOnly(2022, 11, 30), Group = rainbows,   ParentName = "Elena Ivanova",       ParentEmail = "elena@example.kz",     ParentPhone = "+7 707 444 5566" },
            new() { FirstName = "Daniyar",  LastName = "Kassymov",    DateOfBirth = new DateOnly(2022, 7, 18),  Group = rainbows,   ParentName = "Asel Kassymova",      ParentEmail = "asel@example.kz",      ParentPhone = "+7 708 555 6677" },
            new() { FirstName = "Mariam",   LastName = "Tulegenova",  DateOfBirth = new DateOnly(2022, 4, 9),   Group = rainbows,   ParentName = "Bauyrzhan Tulegenov", ParentEmail = "bauyrzhan@example.kz", ParentPhone = "+7 747 666 7788" },
            new() { FirstName = "Timur",    LastName = "Petrov",      DateOfBirth = new DateOnly(2022, 2, 25),  Group = rainbows,   ParentName = "Olga Petrova",        ParentEmail = "olga@example.kz",      ParentPhone = "+7 771 777 8899" },
            new() { FirstName = "Ayaulym",  LastName = "Nurlanova",   DateOfBirth = new DateOnly(2020, 12, 12), Group = stars,      ParentName = "Nurlan Ospanov",      ParentEmail = "nurlan@example.kz",    ParentPhone = "+7 775 888 9900" },
            new() { FirstName = "Yerassyl", LastName = "Bekmuratov",  DateOfBirth = new DateOnly(2021, 3, 3),   Group = stars,      ParentName = "Zhanna Bekmuratova",  ParentEmail = "zhanna@example.kz",    ParentPhone = "+7 776 999 0011" },
            new() { FirstName = "Eva",      LastName = "Smirnova",    DateOfBirth = new DateOnly(2021, 8, 16),  Group = stars,      ParentName = "Irina Smirnova",      ParentEmail = "irina@example.kz",     ParentPhone = "+7 777 000 1122" },
            new() { FirstName = "Arnur",    LastName = "Dauletov",    DateOfBirth = new DateOnly(2020, 10, 7),  Group = stars,      ParentName = "Dauren Dauletov",     ParentEmail = "dauren@example.kz",    ParentPhone = "+7 778 123 4567" },
        };
        db.Children.AddRange(children);

        // Attendance for the last 2 weeks (weekdays only)
        var random = new Random(42);
        var today = DateOnly.FromDateTime(DateTime.Today);
        for (int d = 1; d <= 14; d++)
        {
            var date = today.AddDays(-d);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;

            foreach (var child in children)
            {
                var roll = random.Next(100);
                var status = roll < 80 ? AttendanceStatus.Present
                           : roll < 88 ? AttendanceStatus.Late
                           : roll < 95 ? AttendanceStatus.Sick
                           : AttendanceStatus.Absent;
                var cameIn = status is AttendanceStatus.Present or AttendanceStatus.Late;

                db.AttendanceRecords.Add(new AttendanceRecord
                {
                    Child = child,
                    Date = date,
                    Status = status,
                    CheckInTime = cameIn ? new TimeOnly(status == AttendanceStatus.Late ? 9 : 8, random.Next(0, 30)) : null,
                    CheckOutTime = cameIn ? new TimeOnly(17, random.Next(0, 59)) : null,
                    PickedUpBy = cameIn ? child.ParentName : null
                });
            }
        }

        db.Announcements.AddRange(
            new Announcement { Title = "Welcome to KinderHub!", Message = "Parents can now follow attendance and daily reports online.", AuthorName = "Administration", IsImportant = true },
            new Announcement { Title = "Autumn concert", Message = "Our autumn concert is on Friday at 16:00 in the main hall. Everyone is welcome!", AuthorName = "Administration" },
            new Announcement { Title = "Bring warm clothes", Message = "Please bring a warm jacket and hat for outdoor walks.", AuthorName = "Dana Akhmetova", Group = rainbows }
        );

        await db.SaveChangesAsync();
    }
}
