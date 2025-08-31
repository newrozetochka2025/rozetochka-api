using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace rozetochka_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {

        [HttpGet("")]
        public object? GetAll()
        {
            return JsonSerializer.Deserialize<dynamic>("""
                                {
                    "categories": [
                        {
                            "id": "1",
                            "title": "Ноутбуки та комп’ютери",
                            "svg": "https://rozetka.com.ua/assets/sprite/sprite.555e6ed8.svg#icon-fat-2416",
                            "href": "/computers-notebooks",
                            "imageUrl": "https://video.rozetka.com.ua/img_superportal/kompyutery_i_noutbuki/noutbuki.png"
                        },
                        {
                            "id": "2",
                            "title": "Смартфони, ТВ і Електроніка",
                            "svg": "https://rozetka.com.ua/assets/sprite/sprite.555e6ed8.svg#icon-fat-3361",
                            "href": "/telefony-tv-i-ehlektronika",
                            "imageUrl": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg"
                        }
                    ],
                    "banners": [
                        {
                            "id": "3",
                            "img": "https://site.my/slide/1.jpg",
                            "href": "(ссылка на рекламируемый товар или группу товаров)"
                        },
                        {
                            "id": "4",
                            "img": "https://site.my/slide/2.jpg",
                            "href": "(ссылка на рекламируемый товар или группу товаров)"
                        },
                        {
                            "id": "5",
                            "img": "https://site.my/slide/2.jpg",
                            "href": "(ссылка на рекламируемый товар или группу товаров)"
                        }
                    ],
                    "recommendProducts": [],
                    "bestProducts": [
                        {
                            "id": "6",
                            "title": "Ніж універсальний Satori 27см",
                            "href": "/449065067",
                            "img": "картинка",
                            "price": 1000,
                            "discountPrice": 499,
                            "isInWishlist": true
                        }
                    ]
                }
                """);
        }


        [HttpGet("{id}")]
        public object? Get(string id)
        {
            return JsonSerializer.Deserialize<dynamic>("""
                    {
                        "pagination": {
                          "currentPage": 1,
                          "lastPage": 10,
                          "perPage": 20
                        },
                        "breadcrumbs": [
                            {
                                "title": "Товары для дома",
                                "href": "/tovari"
                            },
                            {
                                "title": "Домашний текстиль",
                                "href": null
                            }
                        ],
                        "title": "Домашний текстиль",
                        "subCategories": [
                                {
                                "id": "1",
                                "title": "Ноутбуки та комп’ютери",
                                "svg": "https://rozetka.com.ua/assets/sprite/sprite.555e6ed8.svg#icon-fat-2416",
                                "href": "/computers-notebooks",
                                "imageUrl": "https://video.rozetka.com.ua/img_superportal/kompyutery_i_noutbuki/noutbuki.png"
                            },
                            {
                                "id": "2",
                                "title": "Смартфони, ТВ і Електроніка",
                                "svg": "https://rozetka.com.ua/assets/sprite/sprite.555e6ed8.svg#icon-fat-3361",
                                "href": "/telefony-tv-i-ehlektronika",
                                "imageUrl": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg"
                            }
                        ],
                        "total": 100500,
                        "products": [
                            {
                                "id": "6",
                                "title": "Ніж універсальний Satori 27см",
                                "href": "/449065067",
                                "img": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg",
                                "price": 1000,
                                "discountPrice": 499,
                                "isInWishlist": true
                            },
                            {
                                "id": "6",
                                "title": "Ніж універсальний Satori 27см",
                                "href": "/449065067",
                                "img": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg",
                                "price": 1000,
                                "discountPrice": 499,
                                "isInWishlist": true
                            },
                            {
                                "id": "6",
                                "title": "Ніж універсальний Satori 27см",
                                "href": "/449065067",
                                "img": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg",
                                "price": 1000,
                                "discountPrice": 499,
                                "isInWishlist": true
                            }
                        ],
                        "seeAlso": [
                            {
                                "id": "6",
                                "title": "Ніж універсальний Satori 27см",
                                "href": "/449065067",
                                "img": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg",
                                "price": 1000,
                                "discountPrice": 499,
                                "isInWishlist": true
                            },
                            {
                                "id": "6",
                                "title": "Ніж універсальний Satori 27см",
                                "href": "/449065067",
                                "img": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg",
                                "price": 1000,
                                "discountPrice": 499,
                                "isInWishlist": true
                            },
                            {
                                "id": "6",
                                "title": "Ніж універсальний Satori 27см",
                                "href": "/449065067",
                                "img": "https://content2.rozetka.com.ua/constructor/images_site/original/586131701.jpg",
                                "price": 1000,
                                "discountPrice": 499,
                                "isInWishlist": true
                            }
                        ],
                        "filters": {
                            "brands": [
                                {
                                    "id": "100",
                                    "title": "Ardesto",
                                    "count": 13957,
                                    "slug": "ardesto"
                                },
                                {
                                    "id": "101",
                                    "title": "dwaf fes",
                                    "count": 13957,
                                    "slug": "dwaf-fes"
                                }
                            ],
                            "price": {
                                "min": 1,
                                "max": 3592179
                            },
                            "groups": [
                                {
                                    "title": "Доставка",
                                    "variants": [
                                        {
                                            "title": "Готов к отправке",
                                            "count": 1000
                                        },
                                        {
                                            "title": "Доставка в магазины ROZETKA",
                                            "count": 418070
                                        },
                                        {
                                            "title": "Бесплатно из Новой Почты",
                                            "count": 432620
                                        }
                                    ]
                                },
                                {
                                    "title": "Использование",
                                    "variants": [
                                        {
                                            "title": "Текстиль для ванной",
                                            "count": 3450
                                        },
                                        {
                                            "title": "Текстиль для декора",
                                            "count": 17465
                                        },
                                        {
                                            "title": "Текстиль для кухни",
                                            "count": 9921
                                        },
                                        {
                                            "title": "Текстиль для сна",
                                            "count": 126986
                                        }
                                    ]
                                }
                            ]
                        }
                }
                """);
        }
    }
}
