using AutoMapper;
using EShopCart.DTOs;
using EShopCart.Models;
using System.Linq;

namespace EShopCart.Mappings
{
    public class MappingProfile : Profile
    {
        
        public MappingProfile()
        {
            // User Mappings
            CreateMap<User, UserDto>(); // Map User entity to UserDto
            CreateMap<UserRegisterDto, User>(); // Map UserRegisterDto to User entity

            // Product Mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName)) // Map CategoryName from Category
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl)) // Map ImageUrl
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // Map UserId
                // .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username)) // Map Username from User
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category)) // Map Category details (CategoryDto)
                .ReverseMap();

            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());
                 // ImageUrl will be set during file upload
                // .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)); // Map UserId from the DTO

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // Allow updates for all fields except ImageUrl
                .ForMember(dest => dest.UserId, opt => opt.Ignore()); // Ignore UserId updates to preserve the original value

            // Cart and CartItem Mappings
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.CartItems.Sum(ci => ci.Price * ci.Quantity))); // Calculate TotalPrice for the cart

            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price * src.Quantity)); // Calculate TotalPrice for CartItemDto

            CreateMap<CartItemCreateDto, CartItem>(); // Map CartItemCreateDto to CartItem entity

            // Order and OrderItem Mappings
            CreateMap<Order, OrderDto>().ReverseMap(); // Map Order to OrderDto and vice versa
            CreateMap<OrderItem, OrderItemDto>().ReverseMap(); // Map OrderItem to OrderItemDto and vice versa
            CreateMap<OrderCreateDto, Order>(); // Map OrderCreateDto to Order entity

            // Payment Mappings
            CreateMap<Payment, PaymentDto>().ReverseMap(); // Map Payment to PaymentDto and vice versa
            CreateMap<PaymentCreateDto, Payment>(); // Map PaymentCreateDto to Payment entity

            // Category Mappings
            CreateMap<Categories, CategoryDto>().ReverseMap(); // Map Categories to CategoryDto and vice versa
        }
    }
}
