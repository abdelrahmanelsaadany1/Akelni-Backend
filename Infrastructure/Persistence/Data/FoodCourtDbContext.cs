global using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public class FoodCourtDbContext:DbContext
    {
        public FoodCourtDbContext(DbContextOptions<FoodCourtDbContext> options) : base(options) { }
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        #region DbSets
        public DbSet<Category> Categories { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<AddOn> AddOns { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ItemAddOn> ItemAddOns { get; set; }
        public DbSet<ItemCombo> ItemCombos { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemAddOn> OrderItemAddOns { get; set; }
        public DbSet<OrderItemCombo> OrderItemCombos { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<DeliveryRate> DeliveryRates { get; set; }

        #endregion
    }
}
//test saadany