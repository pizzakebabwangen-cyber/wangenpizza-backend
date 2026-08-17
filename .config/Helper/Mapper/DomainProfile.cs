using AutoMapper;
using WangenPizza.Dtos;
using WangenPizza.Models;

namespace WangenPizza.Helper.Mapper
{
    public class DomainProfile:Profile
    {
        public DomainProfile()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();
            //---------------
            CreateMap<SubCategory, SubCategoryDto>();
            CreateMap<SubCategoryDto, SubCategory>();
            //---------------
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<DiscountCode, DiscountCodeDto>()
            .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate.ToString("dd.MM.yyyy")));

            // Map from DiscountCodeDto to DiscountCode and ignore ExpiryDate
            CreateMap<DiscountCodeDto, DiscountCode>()
                .ForMember(dest => dest.ExpiryDate, opt => opt.Ignore());
            //---------------
            CreateMap<Contact, ContactDto>();
            CreateMap<ContactDto, Contact>();
            //---------------
            CreateMap<Offer, OfferDto>();
            CreateMap<OfferDto, Offer>();
            //---------------
            CreateMap<TodayBonus, TodayBonusDto>();
            CreateMap<TodayBonusDto, TodayBonus>();
            //---------------
            CreateMap<CompanyData, CompanyDataDto>();
            CreateMap<CompanyDataDto, CompanyData>();
            //---------------
            CreateMap<Delivery, DeliveryDto>();
            CreateMap<DeliveryDto, Delivery>();
            //---------------
            CreateMap<EmailText, EmailTextDto>();
            CreateMap<EmailTextDto, EmailText>();
            //---------------
            CreateMap<CartItem, CartItemDto>();
            CreateMap<CartItemDto, CartItem>();
            //---------------
            CreateMap<Order, OrderDto>();
            CreateMap<OrderDto, Order>();
            //---------------
            CreateMap<Reservation, ReservationDto>();
            CreateMap<ReservationDto, Reservation>();
            //---------------
            CreateMap<Extension, ExtensionDto>();
            CreateMap<ExtensionDto, Extension>();
            //---------------
        }
    }
}
