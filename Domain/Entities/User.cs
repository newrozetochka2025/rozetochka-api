namespace rozetochka_api.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "user";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<WishlistItem> Wishlist { get; set; } = new List<WishlistItem>();
    }
}