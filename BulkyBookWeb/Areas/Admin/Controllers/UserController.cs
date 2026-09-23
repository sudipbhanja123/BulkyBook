using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IApplicationUserService _userService;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IApplicationUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RoleManagement(string userId)
        {
            var users = await _userService.GetUserByIdAsync(userId);
            if (users == null)
            {
                return Json(new { success = false, message = "User not found" });
            }
            RoleManagmentVM RoleVm = new()
            {
                ApplicationUser = users,
                RoleList = _roleManager.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                })
            };
            RoleVm.ApplicationUser.Role = (await _userManager.GetRolesAsync(users)).FirstOrDefault() ?? "No Role";

            return View(RoleVm);
        }
        [HttpPost]
        public async Task<IActionResult> RoleManagement(RoleManagmentVM roleManagmentVM)
        {
            var users = await _userService.GetUserByIdAsync(roleManagmentVM.ApplicationUser.Id);
            if (users == null)
            {
                return Json(new { success = false, message = "User not found" });
            }
            string oldRole = (await _userManager.GetRolesAsync(users)).FirstOrDefault() ?? "No Role";
            if (roleManagmentVM.ApplicationUser.Role != oldRole)
            {
                if (oldRole != "No Role")
                {
                    await _userManager.RemoveFromRoleAsync(users, oldRole);
                    await _userManager.AddToRoleAsync(users, roleManagmentVM.ApplicationUser.Role);
                }

            }
            TempData["success"] = "Role has been updated";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ChangePassword(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }
            AdminChangePasswordVM adminChangePasswordVM = new()
            {
                UserEmail = user.Email,
                UserId = user.Id,
            };


            return View(adminChangePasswordVM);
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordVM adminChangePasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View(adminChangePasswordVM);
            }
            var users = await _userService.GetUserByIdAsync(adminChangePasswordVM.UserId);
            if (users == null)
            {
                return NotFound();
            }
            var result = await _userManager.ResetPasswordAsync(users, await _userManager.GeneratePasswordResetTokenAsync(users), adminChangePasswordVM.NewPassword);
            if (result.Succeeded)
            {
                TempData["success"] = "Password has been updated";
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            adminChangePasswordVM.UserEmail = users.Email;
            return View(adminChangePasswordVM);

        }

        #region API CALLS
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            foreach (var user in users)
            {
                var role = await _userManager.GetRolesAsync(user);
                user.Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "No Role";
            }
            return Json(new { data = users });
        }
        [HttpPost]
        public async Task<IActionResult> LockUnlock([FromBody] string userId)
        {
            var users = await _userService.GetUserByIdAsync(userId);
            if (users == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking" });
            }
            if (await _userManager.IsLockedOutAsync(users))
            {
                await _userManager.SetLockoutEndDateAsync(users, DateTimeOffset.UtcNow);
                return Json(new { success = true, message = "User unlocked successfully" });
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(users, DateTimeOffset.UtcNow.AddYears(100));
                return Json(new { success = true, message = "User locked successfully" });
            }

        }


        #endregion
    }
}
