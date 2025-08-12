using Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Model.Configuration
{
    public class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.ToTable("Units");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnType("int")
                .UseIdentityColumn();

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(u => u.Name)
                .IsUnique();

            builder.Property(u => u.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.HasMany(u => u.Balances)
                .WithOne(b => b.Unit)
                .HasForeignKey(b => b.UnitId);
        }
    }


}
