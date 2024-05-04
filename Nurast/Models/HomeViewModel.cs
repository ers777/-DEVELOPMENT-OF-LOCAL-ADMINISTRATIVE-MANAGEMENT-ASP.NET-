namespace Nurast.Models
{
    public class HomeViewModel
    {
        public Dictionary<string, NewsItem> News { get; set; }
        public Dictionary<string, VacancyItem> Vacancies { get; set; }
    }
}
