using AutoMapper;
using DashboardApi.Dtos.Category;
using DashboardApi.Models;

namespace DashboardApi.Profiles
{
   public class CategoryProfile : Profile
   {
      public CategoryProfile()
      {
         CreateMap<CategoryDto, Category>();
      }

   }
}
