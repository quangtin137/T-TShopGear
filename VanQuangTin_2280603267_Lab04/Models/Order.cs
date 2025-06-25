using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace VanQuangTin_2280603267_Lab04.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; } 
        public decimal TotalPrice { get; set; }
        public decimal Discount { get; set; }      // ➕ mới
        public decimal FinalAmount { get; set; }   // ➕ mới
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
