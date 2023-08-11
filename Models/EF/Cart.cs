using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PCShop.Models.EF
{
    public class Cart
    {
        public List<CartItems> listCartItem { get; set; }
        public Cart()
        {
            this.listCartItem = new List<CartItems>();
        }

        public void AddItem(CartItems item, int quantity)
        {
            var checkExits = listCartItem.FirstOrDefault(x => x.Id == item.Id);
            if (checkExits != null)
            {
                checkExits.quantity += quantity;
                checkExits.totalPrice += checkExits.price * checkExits.quantity;
            }
            else
            {
                listCartItem.Add(item);
            }
        }

        public void DeleteItem(int id)
        {
            var checkExits = listCartItem.SingleOrDefault(x => x.Id == id);
            if (checkExits != null)
            {
                listCartItem.Remove(checkExits);
            }
        }

        public void UpdateQuantity(int id, int new_quantity)
        {
            var checkExits = listCartItem.SingleOrDefault(x => x.Id == id);
            if (checkExits != null)
            {
                checkExits.quantity = new_quantity;
                checkExits.totalPrice = checkExits.price * checkExits.quantity;
            }
        }

        public int GetTotalPrice()
        {
            return listCartItem.Sum(x => x.totalPrice);
        }

        public int GetTotalQuantity()
        {
            return listCartItem.Sum(x => x.quantity);
        }

        public void ClearCart()
        {
            listCartItem.Clear();
        }
    }

    public class CartItems
    {
        public int Id { get; set; }
        public string productName { get; set; }
        public string Alias { get; set; }
        public string categoryName { get; set; }
        public string productImg { get; set; }
        public int quantity { get; set; }
        public int price { get; set; }
        public int totalPrice { get; set; }
    }
}