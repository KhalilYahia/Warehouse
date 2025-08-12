using Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Model.Configuration
{
    public class BalanceConfiguration : IEntityTypeConfiguration<Balance>
    {
        public void Configure(EntityTypeBuilder<Balance> builder)
        {
            builder.ToTable("Balances");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.HasIndex(b => new { b.ResourceId, b.UnitId })
                .IsUnique();
        }
    }


}
