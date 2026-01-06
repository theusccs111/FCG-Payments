using FCG_Payments.Domain.Payments.Entities;
using FCG_Payments.Infrastructure.Payments.Mappings;
using Microsoft.EntityFrameworkCore;

namespace FCG_Payments.Infrastructure.Shared.Context
{
    public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
    {
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentMap());
        }
    }    
}
