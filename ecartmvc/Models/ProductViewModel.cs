namespace ecartmvc.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        //bind reviews in Details.cshtml
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
