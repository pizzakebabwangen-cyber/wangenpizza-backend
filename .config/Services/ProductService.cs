using Microsoft.EntityFrameworkCore;
using WangenPizza.Context;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext context;
        public ProductService(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>Extensions lookup requires SubCategory; orphan products must not crash the admin dashboard.</summary>
        private static List<Extension> ExtensionsForProductSubCategory(Product product, List<Extension> extensions)
        {
            if (product.SubCategory == null)
                return new List<Extension>();
            var categoryId = product.SubCategory.CategoryId;
            return extensions.Where(e => e.CategoryId == categoryId).ToList();
        }

        public async Task<Product> Create(Product Product)
        {
            await context.Product.AddAsync(Product);
            context.SaveChanges();
            return Product;
        }



        public void Delete(int id)
        {
            var product = new Product { Id = id }; // ما بنجيبش الكائن كله من DB
            context.Product.Attach(product);       // Attach للكائن بس عشان EF يعرفه
            context.Product.Remove(product);       // شيله
            context.SaveChanges();                  // نفذ
        }



        public async Task<IEnumerable<Product>> Get()
        {
            var products = await context.Product.Include("SubCategory").Where(a=>a.ProductType != "TodayBonus" && a.ProductType != "Offer").OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();

            // Get all extensions that are not associated with any CartItem
            var extensions = await context.Extension.Where(e => e.Kind== "MainExtension").ToListAsync();

            var productWithExtensions = products.Select(product => new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PhotoName = product.PhotoName,
                Price = product.Price,
                Pickup_Price = product.Pickup_Price,
                DisplayOrder = product.DisplayOrder,
                AddToHome = product.AddToHome,
                SubCategoryId = product.SubCategoryId,
                SubCategory = product.SubCategory,
                Extensions = ExtensionsForProductSubCategory(product, extensions)
            }).ToList();

            return productWithExtensions;
        }
        public async Task<IEnumerable<Product>> GetByCatgoryId(int categoryId)
        {
            var products = await context.Product.Include("SubCategory").Where(a => a.SubCategoryId == categoryId).OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
            var extensions = await context.Extension.Where(e => e.Kind == "MainExtension").ToListAsync();

            var productWithExtensions = products.Select(product => new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PhotoName = product.PhotoName,
                Price = product.Price,
                Pickup_Price = product.Pickup_Price,
                DisplayOrder = product.DisplayOrder,
                AddToHome = product.AddToHome,
                SubCategoryId = product.SubCategoryId,
                SubCategory = product.SubCategory,
                Extensions = ExtensionsForProductSubCategory(product, extensions)
            }).ToList();
            return productWithExtensions;
        }

        public async Task<IEnumerable<Product>> GetBySubCatgoryId(int subCategoryId)
        {
            var products = await context.Product.Include("SubCategory").Where(a=>a.SubCategoryId== subCategoryId).OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
            var extensions = await context.Extension.Where(e => e.Kind == "MainExtension").ToListAsync();

            var productWithExtensions = products.Select(product => new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PhotoName = product.PhotoName,
                Price = product.Price,
                Pickup_Price = product.Pickup_Price,
                DisplayOrder = product.DisplayOrder,
                AddToHome = product.AddToHome,
                SubCategoryId = product.SubCategoryId,
                SubCategory = product.SubCategory,
                Extensions = ExtensionsForProductSubCategory(product, extensions)
            }).ToList();
            return productWithExtensions;
        }

        public async Task<Product> GetById(int id)
        {
            var product = await context.Product.Include("SubCategory").SingleOrDefaultAsync(a => a.Id == id);
            if (product == null)
                return null!;
            var extensions = await context.Extension.Where(e => e.Kind == "MainExtension").ToListAsync();

            var productWithExtensions = new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PhotoName = product.PhotoName,
                Price = product.Price,
                Pickup_Price = product.Pickup_Price,
                DisplayOrder = product.DisplayOrder,
                AddToHome = product.AddToHome,
                SubCategoryId = product.SubCategoryId,
                SubCategory = product.SubCategory,
                Extensions = ExtensionsForProductSubCategory(product, extensions)
            };
            return productWithExtensions;
        }



        public void Update(Product Product)
        {
            context.Entry(Product).State = EntityState.Modified;


            context.SaveChanges();

        }

        public async Task<IEnumerable<Product>> GetProductsToHome()
        {
            var products = await context.Product.Include("SubCategory").Where(a => a.AddToHome == true).OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
            var extensions = await context.Extension.Where(e => e.Kind == "MainExtension").ToListAsync();

            var productWithExtensions = products.Select(product => new Product
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                PhotoName = product.PhotoName,
                Price = product.Price,
                Pickup_Price = product.Pickup_Price,
                DisplayOrder = product.DisplayOrder,
                AddToHome = product.AddToHome,
                SubCategoryId = product.SubCategoryId,
                SubCategory = product.SubCategory,
                Extensions = ExtensionsForProductSubCategory(product, extensions)
            }).ToList();
            return productWithExtensions;
        }

        public async Task<IEnumerable<Product>> GetOffers()
        {
            return await context.Product.Include("SubCategory").Where(a => a.ProductType == "Offer").OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetTodayBonus()
        {
            return await context.Product.Include("SubCategory").Where(a => a.ProductType == "TodayBonus").OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id).ToListAsync();
        }
    }
}
