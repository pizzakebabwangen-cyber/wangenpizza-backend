using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WangenPizza.Models;

namespace WangenPizza.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<SubCategory> SubCategory { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<DiscountCode> DiscountCode { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<Offer> Offer { get; set; }
        public DbSet<TodayBonus> TodayBonus { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CompanyData> CompanyData { get; set; }
        public DbSet<Delivery> Delivery { get; set; }
        public DbSet<EmailText> EmailText { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<Extension> Extension { get; set; }
        public DbSet<ExtensionOrderItem> ExtensionOrderItem { get; set; }
        public DbSet<RestaurantSettings> RestaurantSettings { get; set; }
        public DbSet<RestaurantImage> RestaurantImages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure cascade delete for SubCategories when a Category is deleted
            modelBuilder.Entity<SubCategory>()
                .HasOne(sc => sc.Category)
                .WithMany(c => c.SubCategory)
                .HasForeignKey(sc => sc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure cascade delete for Products when a SubCategory is deleted
            modelBuilder.Entity<Product>()
                .HasOne(p => p.SubCategory)
                .WithMany(sc => sc.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.Cascade);


            // The rest of your configurations
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasMany(oi => oi.ExtensionOrderItem)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShoppingCart>()
                .HasMany(sc => sc.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShoppingCart>()
                .HasMany(sc => sc.OrderItems)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasMany(ci => ci.Extensions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);

            base.OnModelCreating(modelBuilder);

       
        }

    }
}
