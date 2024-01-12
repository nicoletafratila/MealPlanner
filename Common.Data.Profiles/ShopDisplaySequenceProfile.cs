﻿using AutoMapper;
using Common.Data.Entities;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace Common.Data.Profiles
{
    public class ShopDisplaySequenceProfile : Profile
    {
        public ShopDisplaySequenceProfile()
        {
            CreateMap<ShopDisplaySequence, ShopDisplaySequenceModel>()
               .ReverseMap()
               .ForMember(data => data.Shop, opt => opt.Ignore())
               .ForMember(data => data.ProductCategory, opt => opt.Ignore());
        }
    }
}