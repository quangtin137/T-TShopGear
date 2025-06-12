using System.ComponentModel.DataAnnotations.Schema;

namespace VanQuangTin_2280603267_Lab04.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
