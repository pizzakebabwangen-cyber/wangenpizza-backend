using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class SubCategoryService: ISubCategoryService
    {
        private readonly ApplicationDbContext context;
        public SubCategoryService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<SubCategory> Create(SubCategory SubCategory)
        {
            await context.SubCategory.AddAsync(SubCategory);
            context.SaveChanges();
            return SubCategory;
        }



        public void Delete(SubCategory SubCategory)
        {

            context.Entry(SubCategory).State = EntityState.Deleted;
            context.SaveChanges();

        }



        public async Task<IEnumerable<SubCategory>> Get()
        {
            var subdata= await context.SubCategory.Include("Category").Include("Products").ToListAsync();
            var filteredSubCategories = subdata.Where(c => c.Id != 41 && c.Id != 43);
            return filteredSubCategories;
        }
        public async Task<IEnumerable<SubCategory>> GetByCategoryId(int id)
        {
            return await context.SubCategory.Include("Category").Include("Products").Where(a=>a.CategoryId==id).ToListAsync();
        }

        public async Task<SubCategory> GetById(int id)
        {
            return await context.SubCategory.Include("Category").Include("Products").SingleOrDefaultAsync(a => a.Id == id);
        }



        public void Update(SubCategory SubCategory)
        {
            context.Entry(SubCategory).State = EntityState.Modified;


            context.SaveChanges();

        }

    }
}
