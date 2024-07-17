using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ReminderWebApp.Data;
using ReminderWebApp.Data.Models;

namespace ReminderWebApp.Helpers.AuthorizationHelpers
{
    public class IsToDoThingOwnerHandler : AuthorizationHandler<IsToDoThingOwnerRequirement, ToDoThing>
    {
        private readonly UserManager<IdentityUser> _userManager;

        public IsToDoThingOwnerHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsToDoThingOwnerRequirement requirement, ToDoThing toDoThing)
        {
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return;
            }
            if (toDoThing.UserId == user.Id)
            {
                context.Succeed(requirement);
            }
        }
    }
}
