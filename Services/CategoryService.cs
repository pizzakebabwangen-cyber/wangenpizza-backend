using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WangenPizza.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly ApplicationDbContext context;
        public CategoryService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Category> Create(Category category)
        {
            if (category.DisplayOrder == 0)
            {
                var maxOrder = await context.Category.MaxAsync(c => (int?)c.DisplayOrder) ?? 0;
                category.DisplayOrder = maxOrder + 1;
            }

            await context.Category.AddAsync(category);
            context.SaveChanges();
            return category;
        }



        public void Delete(Category category)
        {
            context.Entry(category).State = EntityState.Deleted;

            context.SaveChanges();

        }



        public async Task<IEnumerable<Category>> Get()
        {
            var allCategories = await context.Category.Include("SubCategory").ToListAsync();
            return allCategories
                .Where(c => c.Id != 31 && c.Id != 30)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id);
        }

        public async Task<Category> GetById(int id)
        {
            return await context.Category.FirstOrDefaultAsync(a => a.Id == id);
        }



        public void Update(Category category)
        {
            context.Entry(category).State = EntityState.Modified;

            context.SaveChanges();

        }

    }
}
