﻿using BlogSimple.Model.Models;
using BlogSimple.Model.ViewModels.AccountViewModels;
using BlogSimple.Model.ViewModels.BlogViewModels;
using BlogSimple.Web.BusinessManager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BlogSimple.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAccountBusinessManager _accountBusinessManager;

        public AccountController(
            UserManager<User> userManager,
            RoleManager<ApplicationRole> roleManager,
            SignInManager<User> signInManager,
            IAccountBusinessManager accountBusinessManager
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _accountBusinessManager = accountBusinessManager;
        }

        public async Task<IActionResult> AboutMe()
        {
            AboutMeViewModel aboutMeViewModel = await _accountBusinessManager.GetAboutMeViewModel(User);

            return View(aboutMeViewModel);
        }

        [Authorize]
        public async Task<ActionResult> UpdateAboutMe()
        {
            var aboutMeViewModel = await _accountBusinessManager.GetAboutMeViewModel(User);

            if (aboutMeViewModel is null)
                return new NotFoundResult();

            return View(aboutMeViewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAboutMe(AboutMeViewModel aboutMeViewModel)
        {
            await _accountBusinessManager.EditUser(aboutMeViewModel, User);
            return RedirectToAction("AboutMe");
        }

        [Authorize]
        public IActionResult MyAccount()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [Authorize]
        public IActionResult RegisterRole()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Required] string username, [Required] string password, string returnurl)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(user, password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(returnurl ?? "/Blog/Index");
                    }
                }
                ModelState.AddModelError(nameof(username), "Login Failed: Invalid Email or Password");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                User newUser = new User
                {
                    FirstName = user.FirstName, 
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Description = null,
                    Content = null,
                    PortfolioLink = null,
                    TwitterLink = null,
                    GitHubLink = null,
                    LinkedInLink = null 
                };

                IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                    ViewBag.Message = "User created successfully!";
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterRole(UserRole userRole)
        {
            if (ModelState.IsValid)
            {
                ApplicationRole newRole = new ApplicationRole()
                {
                    Name = userRole.RoleName,
                };

                IdentityResult result = await _roleManager.CreateAsync(newRole);

                if (result.Succeeded)
                    ViewBag.Message = "Role Created Successfully!";
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}