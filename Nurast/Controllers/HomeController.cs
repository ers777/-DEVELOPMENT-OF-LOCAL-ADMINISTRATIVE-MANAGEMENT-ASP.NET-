using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using Nurast.Models;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nurast.Controllers
{
    public class HomeController : Controller
    {
        private IFirebaseClient client =new FirebaseService().GetClient();

        public HomeController()
        {
          
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



        public async Task<IActionResult> Privacy(string key)
        {
            try
            {
                // Запрос к Firebase для получения VacancyItem по ключу, например "vac1"
                var response = await client.GetAsync($"Value/Vacancy/{key}");
               

                var vacancyItem = response.ResultAs<VacancyItem>();  // Конвертация ответа в объект VacancyItem
                if (vacancyItem == null)
                {
                    ViewBag.ErrorMessage = "Vacancy not found.";
                    return View("Error");  // Вернуть страницу ошибки, если вакансия не найдена
                }

                return View(vacancyItem);  // Отправка объекта в представление
            }
            catch (Exception ex)
            {
                // Обработка ошибок при выполнении запроса
                ViewBag.ErrorMessage = $"Error: {ex.Message}";
                return View("Error");
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            string safeEmail = email.Replace('.', ',');  // Замена точки на запятую для безопасного использования в URL
            try
            {
                FirebaseResponse response = await client.SetAsync($"Users/{safeEmail}", new { Email = email, Password = password });

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("Index");  // Перенаправление на главную страницу после успешной регистрации
                }
                else
                {
                    // Логирование подробностей ошибки
                    ViewBag.ErrorMessage = "Произошла ошибка при регистрации. Пожалуйста, попробуйте снова.";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                // Логирование необработанного исключения
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
            string safeEmail = email.Replace('.', ',');  // Замена точки на запятую для безопасного использования в URL
            FirebaseResponse response = await client.GetAsync($"Users/{safeEmail}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var user = response.ResultAs<Dictionary<string, string>>();
                if (user != null && user["Password"] == password && user["Email"]==email)
                {
                    return RedirectToAction("Index");  // Перенаправить пользователя на главную страницу после успешного входа
                }
                else
                {
                    ViewBag.Error = "Invalid credentials";  // Отображение сообщения об ошибке
                    return View("Login");  // Вернуться на страницу входа
                }
            }
            ViewBag.Error = "User not found";  // Отображение сообщения об ошибке
            return View("Login"); // В случае неудачи показать страницу с ошибкой
        }

    }
}
