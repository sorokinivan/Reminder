using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;
using ReminderWebApp.Pages;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ReminderWebApp.IntegrationTests
{
    public class ToDoThingTests : IClassFixture<CustomApplicationFactory>
    {
        //private readonly WebApplicationFactory<Program> _applicationFactory;
        private readonly ApplicationDbContext _context;
        private readonly IServiceScope _scope;
        private readonly HttpClient _client;
        private readonly ToDoThingService _toDoThingService;
        private readonly PageContext _pageContext;
        public ToDoThingTests(CustomApplicationFactory applicationFactory)
        {
            //_applicationFactory = applicationFactory;

            _client = applicationFactory.CreateDefaultClient();
            _scope = (applicationFactory.Services.GetRequiredService<IServiceScopeFactory>()).CreateScope();
            _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _context.Database.EnsureCreated();
            InitData();
            _toDoThingService = new ToDoThingService(_context);
            _pageContext = GetPageContext();
        }

        private void InitData()
        {
            if (!_context.ToDoThings.Any())
            {
                var now = DateTime.Now;
                var testData = new List<ToDoThing>
                {
                    new ToDoThing { Id = 1, Title = "TestToDoThingTitle1", Description = "TestToDoThingDescription1", Date = now.Date.AddDays(1), RemindTime = 10, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 2, Title = "TestToDoThingTitle2", Description = "TestToDoThingDescription2", Date = now.Date.AddMonths(1), RemindTime = 15, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 3, Title = "TestToDoThingTitle3", Description = "TestToDoThingDescription3", Date = now.Date, RemindTime = 90, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 4, Title = "TestToDoThingTitle4", Description = "TestToDoThingDescription4", Date = now.Date, RemindTime = 5, IsDeleted = false, UserId = "TestUserId1" },
                    new ToDoThing { Id = 5, Title = "TestToDoThingTitle5", Description = "TestToDoThingDescription5", Date = now.AddDays(1), RemindTime = 60, IsDeleted = false, UserId = "TestUserId2" },
                    new ToDoThing { Id = 6, Title = "TestToDoThingTitle6", Description = "TestToDoThingDescription6", Date = now, RemindTime = 15, IsDeleted = false, UserId = "TestUserId2" }
                };
                _context.AddRange(testData);
                _context.SaveChanges();
            }
        }

        private PageContext GetPageContext()
        {
            var user = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new Claim[] { new Claim(ClaimTypes.Name, "test@email.com"), new Claim(ClaimTypes.NameIdentifier, "TestUserId1") },
                            "Basic")
                        );
            var httpContext = new DefaultHttpContext()
            {
                User = user
            };
            var modelState = new ModelStateDictionary();
            var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            var pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };
            return pageContext;
        }

        private UserService GetUserService()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            store.Setup(x => x.FindByIdAsync("TestUserId1", CancellationToken.None))
                .ReturnsAsync(new IdentityUser()
                {
                    UserName = "test@email.com",
                    Id = "TestUserId1"
                });
            var mgr = new UserManager<IdentityUser>(store.Object, null, null, null, null, null, null, null, null);
            var user = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new Claim[] { new Claim(ClaimTypes.Name, "test@email.com"), new Claim(ClaimTypes.NameIdentifier, "TestUserId1") },
                            "Basic")
                        );
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            httpContextAccessorMock.Setup(h => h.HttpContext.User).Returns(user);
            return new UserService(httpContextAccessorMock.Object, mgr);
        }

        [Fact]
        public async Task HealthzRequest_ReturnHealthy()
        {
            var response = await _client.GetAsync("/healthz");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Healthy", content);
        }

        [Fact]
        public async Task AddNewToDoThing_Post_Success()
        {
            var now = DateTime.Now;

            var pageModel = new IndexModel(_toDoThingService) { PageContext = _pageContext };
            pageModel.Input = new NewToDoThingModel { Title = "TestToDoThingTitle7", Description = "TestToDoThingDescription7", Date = now.Date.AddDays(1), RemindTime = now.TimeOfDay};
            await pageModel.OnPostAsync();
            var toDoThings = _context.ToDoThings.ToList();
            Assert.Equal(7, toDoThings.Count);
            Assert.Equal(7, toDoThings.Last().Id);
            Assert.Equal("TestToDoThingTitle7", toDoThings.Last().Title);
            Assert.Equal("TestToDoThingDescription7", toDoThings.Last().Description);
            Assert.Equal(now.Date.AddDays(1), toDoThings.Last().Date);
            Assert.Equal(now.TimeOfDay.TotalMinutes, toDoThings.Last().RemindTime);
        }

        [Fact]
        public async Task AddNewToDoThing_Post_ModelIsInvalid_NotSuccess()
        {
            var now = DateTime.Now;

            var pageModel = new IndexModel(_toDoThingService);
            pageModel.Input = new NewToDoThingModel { Title = "TestToDoThingTitle8", Description = "TestToDoThingDescription8", Date = now.Date.AddDays(1), RemindTime = now.TimeOfDay };
            pageModel.ModelState.AddModelError("Title", "Необходимо ввести название события");
            await pageModel.OnPostAsync();
            var toDoThing = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Title == "TestToDoThingTitle8");
            Assert.Null(toDoThing);
        }

        [Fact]
        public async Task GetDaysWithToDothings_Get_Success()
        {
            var now = DateTime.Now;

            var pageModel = new IndexModel(_toDoThingService) { PageContext = _pageContext };

            await pageModel.OnGetAsync();

            Assert.NotNull(pageModel.DaysWithToDoThings);
            Assert.NotEmpty(pageModel.DaysWithToDoThings);
        }

        [Fact]
        public async Task EditToDoThing_Post_Success()
        {
            var now = DateTime.Now;

            var pageModel = new EditToDoThingModel(_toDoThingService);
            pageModel.ToDoThing = new ToDoThingModelForEdit{Id = 1, Title = "TestToDoThingTitle7", Description = "TestToDoThingDescription7", Date = now.Date.AddDays(1), RemindTime = now.TimeOfDay };
            
            await pageModel.OnPostAsync(1);
            var toDoThing = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == 1);
            Assert.NotNull(toDoThing);
            Assert.Equal("TestToDoThingTitle7", toDoThing.Title);
            Assert.Equal("TestToDoThingDescription7", toDoThing.Description);
            Assert.Equal(now.Date.AddDays(1), toDoThing.Date);
            Assert.Equal(now.TimeOfDay.TotalMinutes, toDoThing.RemindTime);
        }

        [Fact]
        public async Task EditToDoThing_Post_ModelIsInvalid_NotSuccess()
        {
            var now = DateTime.Now;
            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<EditToDoThingModel>>();

            var pageModel = new EditToDoThingModel(_toDoThingService);
            pageModel.ToDoThing = new ToDoThingModelForEdit { Id = 1, Title = "TestToDoThingTitle7", Description = "TestToDoThingDescription7", Date = now.Date.AddDays(1), RemindTime = now.TimeOfDay };
            pageModel.ModelState.AddModelError("Title", "Необходимо ввести название события");
            await pageModel.OnPostAsync(4);
            var toDoThing = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == 4);
            Assert.NotNull(toDoThing);
            Assert.Equal("TestToDoThingTitle4", toDoThing.Title);
            Assert.Equal("TestToDoThingDescription4", toDoThing.Description);
            Assert.Equal(5, toDoThing.RemindTime);
        }

        [Fact]
        public async Task GetToDoThingById_Get_Success()
        {
            var now = DateTime.Now;

            var pageModel = new EditToDoThingModel(_toDoThingService);

            await pageModel.OnGetAsync(3);

            Assert.NotNull(pageModel.ToDoThing);
            Assert.Equal("TestToDoThingTitle3", pageModel.ToDoThing.Title);
            Assert.Equal("TestToDoThingDescription3", pageModel.ToDoThing.Description);
            Assert.Equal(90, pageModel.ToDoThing.RemindTime.TotalMinutes);
        }

        [Fact]
        public async Task GetToDoThingById_Get_CouldNotFindToDoThing_Error()
        {
            var now = DateTime.Now;

            var pageModel = new EditToDoThingModel(_toDoThingService);

            var ex = await Record.ExceptionAsync(async () => await pageModel.OnGetAsync(10));

            Assert.NotNull(ex);
            Assert.IsType<Exception>(ex);
            Assert.Equal("Такого события не существует", ex.Message);
        }

        [Fact]
        public async Task DeleteToDoThing_Post_Success()
        {
            var authorizeService = new Mock<IAuthorizationService>();
            authorizeService.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success);

            var pageModel = new ToDoThingModel(_toDoThingService, authorizeService.Object);

            await pageModel.OnPostDeleteAsync(2);
            var toDoThing = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == 2);
            Assert.NotNull(toDoThing);
            Assert.True(toDoThing.IsDeleted);
        }

        [Fact]
        public async Task DeleteToDoThing_Post_WrongUser_NotSuccess()
        {
            var authorizeService = new Mock<IAuthorizationService>();
            authorizeService.Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success);

            var pageModel = new ToDoThingModel(_toDoThingService, authorizeService.Object);

            await pageModel.OnPostDeleteAsync(5);

            var toDoThing = await _context.ToDoThings.FirstOrDefaultAsync(t => t.Id == 5);

            Assert.NotNull(toDoThing);
        }

        [Fact]
        public async Task GetUserAllToDoThings_Get_Success()
        {
            //var user = new ClaimsPrincipal(
            //            new ClaimsIdentity(
            //                new Claim[] { new Claim(ClaimTypes.Name, "test@email.com"), new Claim(ClaimTypes.NameIdentifier, "TestUserId1") },
            //                "Basic")
            //            );
            //var httpContext = new DefaultHttpContext()
            //{
            //    User = user
            //};
            //var modelState = new ModelStateDictionary();
            //var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            //var modelMetadataProvider = new EmptyModelMetadataProvider();
            //var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            //var pageContext = new PageContext(actionContext)
            //{
            //    ViewData = viewData
            //};
            var pageModel = new AllToDoThingsModel(_toDoThingService) { PageContext = _pageContext };

            await pageModel.OnGet(null);

            Assert.NotNull(pageModel.ToDoThings);
            Assert.NotEmpty(pageModel.ToDoThings);
        }

        [Fact]
        public async Task GetUserToDoThingsByDate_Get_Success()
        {
            var now = DateTime.Now;

            var pageModel = new AllToDoThingsModel(_toDoThingService) { PageContext = _pageContext };

            await pageModel.OnGet(now);

            Assert.NotNull(pageModel.ToDoThings);
            Assert.NotEmpty(pageModel.ToDoThings);
            Assert.Equal(2, pageModel.ToDoThings.Count);
        }
        //[Fact]
        //public async Task Test()
        //{
        //    HttpClient client = _applicationFactory.CreateClient();
        //    var response = await client.GetAsync("/healthz");

        //    response.EnsureSuccessStatusCode();
        //    var content = await response.Content.ReadAsStringAsync();
        //    Assert.Equal("Healthy", content);
        //}
    }
}
