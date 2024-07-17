using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using ReminderWebApp.Services.ToDoThingService;
using System.Runtime.CompilerServices;

namespace ReminderWebApp.UnitTests
{
    public class ToDoThingTests
    {
        private DbContextOptions<ApplicationDbContext> InitData(SqliteConnection connection, DateTime now, bool isEmptyTable)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.Database.EnsureCreated();

                if (!isEmptyTable)
                {
                    context.ToDoThings.AddRange(
                    new ToDoThing { Id = 1, Title = "TestToDoThingTitle1", Description = "TestToDoThingDescription1", Date = now.Date.AddDays(1), RemindTime = 10, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 2, Title = "TestToDoThingTitle2", Description = "TestToDoThingDescription2", Date = now.Date.AddMonths(1), RemindTime = 15, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 3, Title = "TestToDoThingTitle3", Description = "TestToDoThingDescription3", Date = now.Date, RemindTime = 90, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 4, Title = "TestToDoThingTitle4", Description = "TestToDoThingDescription4", Date = now.Date, RemindTime = 5, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 5, Title = "TestToDoThingTitle5", Description = "TestToDoThingDescription5", Date = now.AddDays(1), RemindTime = 60, IsDeleted = false, UserId = "TestUserId2" },
                    new ToDoThing { Id = 6, Title = "TestToDoThingTitle6", Description = "TestToDoThingDescription6", Date = now, RemindTime = 15, IsDeleted = false, UserId = "TestUserId2" }
                    );
                }
                
                context.SaveChanges();
            }

            return options;
        }

        [Fact]
        public async Task GetTodayToDoThingsByUsedId_HasToDoThings_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var todayToDoThings = await service.GetUserTodayToDoThings("TestUserId1");
                Assert.NotNull(todayToDoThings);
                Assert.NotEmpty(todayToDoThings);
                Assert.Equal(2, todayToDoThings.Count);
                Assert.Equal(3, todayToDoThings[0].Id);
                Assert.Equal(4, todayToDoThings[1].Id);
            }
        }

        [Fact]
        public async Task GetTodayToDoThingsByUsedId_NoToDoThings_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, true);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var todayToDoThings = await service.GetUserTodayToDoThings("TestUserId3");
                Assert.NotNull(todayToDoThings);
                Assert.Empty(todayToDoThings);
            }
        }

        [Fact]
        public async Task DeleteToDoThingById_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                await service.DeleteToDoThingByIdAsync(1);
                Assert.Equal(5, context.ToDoThings.Where(t => !t.IsDeleted).Count());
            }
        }

        [Fact]
        public async Task DeleteToDoThingById_CouldNotFindToDoThing_Error()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                //await Assert.ThrowsAsync<Exception>(async () => await service.DeleteToDoThingByIdAsync("TestUserId1", 3));
                var ex = await Record.ExceptionAsync(async () => await service.DeleteToDoThingByIdAsync(7));

                Assert.NotNull(ex);
                Assert.IsType<Exception>(ex);
                Assert.Equal("Такого события не существует", ex.Message);
            }
        }

        [Fact]
        public async Task GetToDoThingById_HasToDoThing_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var todayToDoThing = await service.GetToDoThingByIdAsync(2);
                Assert.NotNull(todayToDoThing);
                Assert.Equal(2, todayToDoThing.Id);
            }
        }

        [Fact]
        public async Task GetToDoThingById_CouldNotFindToDoThing_Error()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                //await Assert.ThrowsAsync<Exception>(async () => await service.GetToDoThingByIdAsync(4));
                var ex = await Record.ExceptionAsync(async () => await service.GetToDoThingByIdAsync(7));

                Assert.NotNull(ex);
                Assert.IsType<Exception>(ex);
                Assert.Equal("Такого события не существует", ex.Message);
            }
        }

        [Fact]
        public async Task GetUserToDoThingsByDateAsync_HasToDoThings_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var t = context.ToDoThings.ToList();
                //await Assert.ThrowsAsync<Exception>(async () => await service.GetToDoThingByIdAsync(4));
                var todayToDoThings = await service.GetUserToDoThingsByDateAsync("TestUserId1", now);
                Assert.NotNull(todayToDoThings);
                Assert.NotEmpty(todayToDoThings);
                Assert.Equal(2, todayToDoThings.Count);
                Assert.Equal(3, todayToDoThings[0].Id);
                Assert.Equal(4, todayToDoThings[1].Id);
            }
        }

        [Fact]
        public async Task GetUserToDoThingsByDateAsync_CouldNotFindToDoThings_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                //await Assert.ThrowsAsync<Exception>(async () => await service.GetToDoThingByIdAsync(4));
                var todayToDoThings = await service.GetUserToDoThingsByDateAsync("TestUserId3", now);
                Assert.NotNull(todayToDoThings);
                Assert.Empty(todayToDoThings);
            }
        }

        [Fact]
        public async Task GetUserToDoThingsByDateAsync_WithoutDate_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                //await Assert.ThrowsAsync<Exception>(async () => await service.GetToDoThingByIdAsync(4));
                var todayToDoThings = await service.GetUserToDoThingsByDateAsync("TestUserId1", null);
                Assert.NotNull(todayToDoThings);
                Assert.Equal(4, todayToDoThings.Count);
                Assert.Equal(1, todayToDoThings[0].Id);
                Assert.Equal(2, todayToDoThings[1].Id);
                Assert.Equal(3, todayToDoThings[2].Id);
                Assert.Equal(4, todayToDoThings[3].Id);
            }
        }

        [Fact]
        public async Task ChangeToDoThingAsync_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                //await Assert.ThrowsAsync<Exception>(async () => await service.GetToDoThingByIdAsync(4));
                await service.ChangeToDoThingAsync(1, "ChangedTestToDoThingTitle1", "ChangedTestToDoThingDescription1", now, 60);
                var toDoThing = context.ToDoThings.FirstOrDefault();
                Assert.Equal("TestUserId1", toDoThing.UserId);
                Assert.Equal(1, toDoThing.Id);
                Assert.Equal("ChangedTestToDoThingTitle1", toDoThing.Title);
                Assert.Equal("ChangedTestToDoThingDescription1", toDoThing.Description);
                Assert.Equal(now, toDoThing.Date);
                Assert.Equal(60, toDoThing.RemindTime);
            }
        }

        [Fact]
        public async Task ChangeToDoThingAsync_Error()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                //await Assert.ThrowsAsync<Exception>(async () => await service.GetToDoThingByIdAsync(4));
                
                var ex = await Record.ExceptionAsync(async () => await service.ChangeToDoThingAsync(7, "ChangedTestToDoThingTitle1", "ChangedTestToDoThingDescription1", now, 60));

                Assert.NotNull(ex);
                Assert.IsType<Exception>(ex);
                Assert.Equal("Такого события не существует", ex.Message);
            }
        }

        [Fact]
        public async Task AddNewToDoThingAsync_Success()
        {
            var now = DateTime.Now;
            var secondsStripped = now.Date.AddHours(now.Hour).AddMinutes(now.Minute);
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, true);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                //await Assert.ThrowsAsync<Exception>(async () => await service.GetToDoThingByIdAsync(4));
                await service.AddNewToDoThingAsync("TestUserId1", "TestToDoThingTitle1", "TestToDoThingDescription1", now.Date, now, 60);
                var toDoThings = context.ToDoThings.ToList();
                Assert.Single(toDoThings);
                Assert.Equal("TestUserId1", toDoThings[0].UserId);
                Assert.Equal(1, toDoThings[0].Id);
                Assert.Equal("TestToDoThingTitle1", toDoThings[0].Title);
                Assert.Equal("TestToDoThingDescription1", toDoThings[0].Description);
                Assert.Equal(secondsStripped, toDoThings[0].Date);
                Assert.Equal(60, toDoThings[0].RemindTime);
            }
        }

        [Fact]
        public async Task GetUserToDoThingsDaysForCurrentMonthAsync_HasToDoThings_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var t = context.ToDoThings.ToList();
                var todayToDoThings = await service.GetUserToDoThingsDaysForCurrentMonthAsync("TestUserId1");
                Assert.NotNull(todayToDoThings);
                Assert.NotEmpty(todayToDoThings);
                Assert.Equal(2, todayToDoThings.Count);
                Assert.Contains(DateTime.Now.Date.Day, todayToDoThings);
                Assert.Contains(DateTime.Now.Date.Day + 1, todayToDoThings);
            }
        }

        [Fact]
        public async Task GetUserToDoThingsDaysForCurrentMonthAsync_CouldNotFindToDoThings_Success()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var todayToDoThings = await service.GetUserToDoThingsDaysForCurrentMonthAsync("TestUserId3");
                Assert.NotNull(todayToDoThings);
                Assert.Empty(todayToDoThings);
            }
        }

        [Fact]
        public async Task GetUserToDoThingsDaysForCurrentMonthAsync_UserIsNull_Success()
        {
            var now = DateTime.Now;
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var todayToDoThings = await service.GetUserToDoThingsDaysForCurrentMonthAsync(null);
                Assert.Empty(todayToDoThings);
            }
        }

        [Fact]
        public async Task IsCurrentUserToDoThing_True()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var isSameUser = await service.IsCurrentUserToDoThingAsync(1, "TestUserId1");
                Assert.True(isSameUser);
            }
        }

        [Fact]
        public async Task IsCurrentUserToDoThing_UserNotNull_False()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var isSameUser = await service.IsCurrentUserToDoThingAsync(1, "TestUserId2");
                Assert.False(isSameUser);
            }
        }

        [Fact]
        public async Task IsCurrentUserToDoThing_UserNull_False()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);
                var isSameUser = await service.IsCurrentUserToDoThingAsync(1, null);
                Assert.False(isSameUser);
            }
        }

        [Fact]
        public async Task IsCurrentUserToDoThing_CouldNotFindToDoThing_Error()
        {
            var now = DateTime.Now;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = InitData(connection, now, false);

            using (var context = new ApplicationDbContext(options))
            {
                var service = new ToDoThingService(context);

                var ex = await Record.ExceptionAsync(async () => await service.IsCurrentUserToDoThingAsync(7, "TestUserId1"));

                Assert.NotNull(ex);
                Assert.IsType<Exception>(ex);
                Assert.Equal("Такого события не существует", ex.Message);
            }
        }
    }
}