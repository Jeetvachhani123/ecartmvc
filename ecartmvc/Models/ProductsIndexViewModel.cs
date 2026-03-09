namespace ecartmvc.Models
{
    public class ProductsIndexViewModel
    {
        public List<ProductViewModel> Products { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedCategory { get; set; }
        public List<string> Brands { get; set; }
        public string SelectedBrand { get; set; }
        public string Search { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal SelectedPriceRange { get; set; }
        public int? SelectedRating { get; set; }
    }
}
