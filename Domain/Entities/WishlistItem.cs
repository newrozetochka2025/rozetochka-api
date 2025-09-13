namespace rozetochka_api.Domain.Entities
{


    // User <-> Product  (M2M таблица)
    public class WishlistItem
    {
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }

        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }

}
