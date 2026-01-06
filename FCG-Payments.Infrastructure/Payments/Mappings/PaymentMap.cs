using FCG_Payments.Domain.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCG_Payments.Infrastructure.Payments.Mappings
{
    public class PaymentMap : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("PaymentId")
                .IsRequired(true);

            builder.Property(p => p.ItemId)
                .HasColumnName("ItemId")
                .IsRequired(true);

            builder.Property(p => p.UserId)
                .HasColumnName("UserId")
                .IsRequired(true);

            builder.Property(p => p.GameId)
                .HasColumnName("GameId")
                .IsRequired(true);

            builder.Property(p => p.Price)
                .HasColumnName("Price")
                .IsRequired(true);

            builder.Property(p => p.PaymentType)
                .HasColumnName("PaymentType")
                .HasColumnType("TINYINT")
                .IsRequired(true);

            builder.Property(p => p.Status)
                .HasColumnName("Status")
                .HasColumnType("TINYINT")
                .IsRequired(true);
        }
    }
}
