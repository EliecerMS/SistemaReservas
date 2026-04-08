using Microsoft.AspNetCore.Mvc;
using SistemaReservas.WebMVC.Models.ApiDTOs;
using SistemaReservas.WebMVC.Models.ViewModels;
using SistemaReservas.WebMVC.Services;
using System.Text.Json;

namespace SistemaReservas.WebMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var dto = new LoginDto { Email = model.Email, Password = model.Password };
                await _authService.LoginAsync(dto);
                
                // Set success notification
                TempData["Notification"] = JsonSerializer.Serialize(new 
                { 
                    icon = "success", 
                    title = "¡Bienvenido!", 
                    text = "Inicio de sesión exitoso.", 
                    timer = 2000 
                });

                return RedirectToAction("Index", "Zones"); // Redirect to zones commonly as home
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas o error de conexión.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var dto = new RegisterDto 
                { 
                    FirstName = model.FirstName, 
                    LastName = model.LastName, 
                    Email = model.Email, 
                    Password = model.Password 
                };
                
                await _authService.RegisterAsync(dto);

                TempData["Notification"] = JsonSerializer.Serialize(new 
                { 
                    icon = "success", 
                    title = "¡Registro Exitoso!", 
                    text = "Ahora puedes iniciar sesión.", 
                    timer = 2000 
                });

                return RedirectToAction(nameof(Login));
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Error registrando usuario o el correo ya existe.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
