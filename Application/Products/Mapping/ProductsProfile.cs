using System.Collections.Generic;
using AutoMapper;
using rozetochka_api.Application.Products.DTOs;
using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Products.Mapping
{
    public class ProductsProfile : Profile
    {
        public ProductsProfile()
        {
            // Product -> DTO
            CreateMap<Product, ProductResponseDto>()
                .ForMember(d => d.Href, m => m.MapFrom(s => string.IsNullOrWhiteSpace(s.Slug) ? s.Id.ToString() : s.Slug))
                .ForMember(d => d.Img, m => m.MapFrom(s => s.ImgUrl))
                //.ForMember(d => d.IsInWishlist, m => m.Ignore());     // пока заглушка?

                // вычисляем IsInWishlist через Items["wishlist"] = HashSet<Guid>
                .AfterMap((product, dto, ctx) =>
                {
                    if (ctx.TryGetItems(out var items) &&
                        items is IDictionary<string, object> dict &&
                        dict.TryGetValue("wishlist", out var obj) &&
                        obj is ISet<Guid> ids)
                    {
                        dto.IsInWishlist = ids.Contains(product.Id);
                    }
                    else
                    {
                        dto.IsInWishlist = false;
                    }
                });
        }
    }
}
