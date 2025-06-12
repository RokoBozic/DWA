using WebApp.Models;

namespace WebApp.ViewModels
{
    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; }
        public List<Country> Countries { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
        public int? CountryId { get; set; }
    }
}