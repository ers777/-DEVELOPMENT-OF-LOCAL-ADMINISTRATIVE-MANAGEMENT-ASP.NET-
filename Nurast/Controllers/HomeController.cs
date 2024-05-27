using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nurast.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nurast.Controllers
{
    public class HomeController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private IFirebaseClient client;

        public HomeController()
        {
            _firebaseService = new FirebaseService();
            client = _firebaseService.GetClient();
        }

        public async Task<IActionResult> Index()
        {
            var response_news = await client.GetAsync("Value/News");
            var response_vac = await client.GetAsync("Value/Vacancy");

            var newsData = response_news.ResultAs<Dictionary<string, NewsItem>>();
            var vacData = response_vac.ResultAs<Dictionary<string, VacancyItem>>();
            var viewModel = new HomeViewModel
            {
                News = newsData,
                Vacancies = vacData
            };
            return View(viewModel);
        }
        public async Task<IActionResult> Profile(string key)
        {
            if (Request.Cookies["category"] =="Әкім") { return RedirectToAction("ProfileAkim"); }
            return View();
        }
        public async Task<IActionResult> ProfileAkim()
        {
            try
            {
                var response = await client.GetAsync($"Users/Akim/Hattar");
                var newsData = response.ResultAs<Dictionary<string, HatModelcs>>();

                if (newsData == null)
                {
                    ViewBag.ErrorMessage = "Data not found.";
                    return View("Error");
                }

                return View(newsData.Values.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost]

        public async Task<IActionResult> AkimHat(string hat, string title)
        {
            try
            {
                // Ensure the title is not null or empty to avoid issues with Firebase
                if (string.IsNullOrWhiteSpace(title))
                {
                    ViewBag.ErrorMessage = "Title cannot be empty.";
                    return View("Error");
                }

                // Get the current date and time
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var email = Request.Cookies["email"];
                var fullName = Request.Cookies["fullName"];
                // Create the data object to be saved
                var data = new
                {
                    Text = hat,
                    Title = title,
                    Date = currentDate,
                    Email= email,
                    FullName = fullName
                };

                FirebaseResponse response = await client.SetAsync($"Users/Akim/Hattar/{title}", data);

                // Set a success message in ViewBag
                ViewBag.SuccessMessage = "Your request has been successfully submitted.";
                return View("Profile");
            }
            catch (Exception ex)
            {
                // Log the exception (you can use any logging mechanism you prefer)
                // For example: _logger.LogError(ex, "Error submitting AkimHat");

                ViewBag.ErrorMessage = "An error occurred while processing your request. Please contact support.";
                return View("Error");
            }
        }



        public async Task<IActionResult> Back()
        {
            // Set the IsAuthenticated cookie to "false"
            Response.Cookies.Append("IsAuthenticated", "false", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddHours(1) // Adjust expiration as needed
            });

            // Redirect to the Index view
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Privacy(string key)
        {
            try
            {
                var response = await client.GetAsync($"Value/Vacancy/{key}");
                var vacancyItem = response.ResultAs<VacancyItem>();

                if (vacancyItem == null)
                {
                    ViewBag.ErrorMessage = "Vacancy not found.";
                    return View("Error");
                }

                return View(vacancyItem);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Privacy(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var downloadUrl = await _firebaseService.UploadFileAsync(stream, fileName);
                    ViewBag.Message = $"Файл загружен. Доступ по ссылке: {downloadUrl}";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Файл не выбран.";
            }

            return View();
        }
        public IActionResult Basharushi()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string fullName, string phoneNumber, string dateOfBirth, string gender, string category)
        {
            string safeEmail = email.Replace('.', ',');
            try
            {
                FirebaseResponse response = await client.SetAsync($"Users/{safeEmail}",
                    new { Email = email, Password = password, FIO = fullName, PhoneNumber = phoneNumber, Date = dateOfBirth, Famele = gender, Category = category });


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
              
                    Response.Cookies.Append("IsAuthenticated", "true");
                    Response.Cookies.Append("fullName", fullName);
                    Response.Cookies.Append("email", email);
                    Response.Cookies.Append("phoneNumber", phoneNumber);
                    Response.Cookies.Append("dateOfBirth", dateOfBirth);
                    Response.Cookies.Append("gender", gender);
                    Response.Cookies.Append("category", category);
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ErrorMessage = "Произошла ошибка при регистрации. Пожалуйста, попробуйте снова.";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Произошла системная ошибка. Пожалуйста, свяжитесь с поддержкой.";
                return View("Error");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            string safeEmail = email.Replace('.', ',');
            FirebaseResponse response = await client.GetAsync($"Users/{safeEmail}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var user = response.ResultAs<Dictionary<string, string>>();
                if (user != null && user["Password"] == password && user["Email"] == email)
                {
                    var vacancyItem = response.ResultAs<VacancyItem>();

                    Response.Cookies.Append("IsAuthenticated", "true");
                    Response.Cookies.Append("fullName", user["FIO"]);
                    Response.Cookies.Append("email", email);
                    Response.Cookies.Append("phoneNumber", user["PhoneNumber"]);
                    Response.Cookies.Append("dateOfBirth", user["Date"]);
                    Response.Cookies.Append("gender", user["Famele"]);
                    Response.Cookies.Append("category", user["Category"]);


                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Invalid credentials";
                    return View("Login");
                }
            }
            ViewBag.Error = "User not found";
            return View("Login");
        }
    }
}
