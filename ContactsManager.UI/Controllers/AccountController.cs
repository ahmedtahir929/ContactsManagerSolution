using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers
{
  [Route("[controller]/[action]")]
  [AllowAnonymous]
  public class AccountController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
      _userManager = userManager;
      _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDTO registerDTO)
    {
      //Check for validaiton errors
      if (!ModelState.IsValid)
      {
        ViewBag.Errors = ModelState
          .Values
          .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage)
          .ToList();

        return View(registerDTO);
      }

      ApplicationUser user = new ApplicationUser
      {
        UserName = registerDTO.Email,
        Email = registerDTO.Email,
        PhoneNumber = registerDTO.Phone,
        PersonName = registerDTO.PersonName
      };

      IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password!);

      if (!result.Succeeded)
      {
        foreach (var error in result.Errors)
        {
          ModelState.AddModelError("Register", error.Description);
        }

        return View(registerDTO);
      }

      //Sign in
      await _signInManager.SignInAsync(user, isPersistent: false);

      return RedirectToAction(nameof(PersonsController.Index), "Persons");
    }

    [HttpGet]
    public IActionResult Login()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO loginDTO)
    {
      if (!ModelState.IsValid)
      {
        ViewBag.Errors = ModelState
          .Values
          .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage)
          .ToList();

        return View(loginDTO);
      }

      var result = await _signInManager.PasswordSignInAsync(loginDTO.Email!, loginDTO.Password!, isPersistent: false, lockoutOnFailure: false);

      if (!result.Succeeded)
      {
        ModelState.AddModelError("Login", "Invalid email or password");
      }

      return RedirectToAction(nameof(PersonsController.Index), "Persons");
    }

    public async Task<IActionResult> Logout()
    {
      await _signInManager.SignOutAsync();

      return RedirectToAction(nameof(PersonsController.Index), "Persons");
    }
  }
}
