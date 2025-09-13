namespace rozetochka_api.Application.Wishlist.Repository
{
    public interface IWishlistRepository
    {
        Task<bool> IsInWishlistAsync(Guid userId, Guid productId);
        Task AddAsync(Guid userId, Guid productId);
        Task RemoveAsync(Guid userId, Guid productId);
        Task<bool> ToggleAsync(Guid userId, Guid productId); // true = добавили, false = убрали
        Task<List<Guid>> GetProductIdsForUserAsync(Guid userId);
    }
}
