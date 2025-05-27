using MongoDB.Driver;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly IMongoCollection<Product> _productCollection;

        public CartService(MongoDbService dbService)
        {
            _cartCollection = dbService.GetCartCollection();
            _productCollection = dbService.GetProductCollection();
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<Cart> GetOrCreateCart(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };
                await _cartCollection.InsertOneAsync(cart);
            }
            return cart;
        }

        public async Task AddToCartAsync(string userId, string productId, int quantity)
        {
            var cart = await GetOrCreateCart(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
            }
            await SaveCart(cart);
        }

        public async Task RemoveItemAsync(string userId, string productId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return;

            cart.Items.RemoveAll(i => i.ProductId == productId);
            await SaveCart(cart);
        }

        public async Task ChangeQuantityAsync(string userId, string productId, int delta)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null) return;

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity += delta;
                if (item.Quantity <= 0)
                    cart.Items.Remove(item);

                await SaveCart(cart);
            }
        }

        // ✅ Hàm bạn đang thiếu – để Controller dùng được
        public async Task IncreaseQuantityAsync(string userId, string productId)
        {
            await ChangeQuantityAsync(userId, productId, 1);
        }

        public async Task DecreaseQuantityAsync(string userId, string productId)
        {
            await ChangeQuantityAsync(userId, productId, -1);
        }

        private async Task SaveCart(Cart cart)
        {
            await _cartCollection.ReplaceOneAsync(c => c.Id == cart.Id, cart, new ReplaceOptions { IsUpsert = true });
        }
    }
}
