namespace WebApp.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string ProductName { get; set; }
        public string CountryName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
    }
} 