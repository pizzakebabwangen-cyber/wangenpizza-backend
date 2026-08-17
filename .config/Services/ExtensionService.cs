using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class ExtensionService: IExtensionService
    {
        private readonly ApplicationDbContext context;
        public ExtensionService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Extension> Create(Extension Extension)
        {
            await context.Extension.AddAsync(Extension);
            context.SaveChanges();
            return Extension;
        }



        public void Delete(Extension Extension)
        {
            context.Entry(Extension).State = EntityState.Deleted;

            context.SaveChanges();

        }



        public async Task<IEnumerable<Extension>> Get()
        {
            return await context.Extension.Where(e=>e.Kind== "MainExtension").Include(a=>a.Category).ToListAsync();
        }
        public async Task<IEnumerable<Extension>> GetByCategoryId(int categoryId)
        {
            return await context.Extension.Where(a=>a.CategoryId==categoryId).ToListAsync();
        }

        public async Task<Extension> GetById(int id)
        {
            return await context.Extension.SingleOrDefaultAsync(a => a.Id == id);
        }
        public async Task<Extension> GetByName(string name)
        {
            return await context.Extension.SingleOrDefaultAsync(a => a.Name == name);
        }



        public void Update(Extension Extension)
        {
            context.Entry(Extension).State = EntityState.Modified;

            context.SaveChanges();

        }
    }
}
