namespace VanQuangTin_2280603267_Lab04.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                Items.Add(item);
        }

        public void RemoveItem(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
                Items.Remove(item);
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.Price * i.Quantity * 1000);
        }

        public decimal GetDiscount()
        {
            var total = GetTotalPrice();
            return total >= 5000000 ? total * 0.1m : 0;
        }

        public decimal GetFinalTotal()
        {
            return GetTotalPrice() - GetDiscount();
        }
    }
}
