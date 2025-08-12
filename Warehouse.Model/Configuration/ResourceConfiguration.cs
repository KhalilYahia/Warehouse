using Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Model.Configuration
{
    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.ToTable("Resources");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .HasColumnType("int")
                .IsRequired()
                .UseIdentityColumn();

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(r => r.Name)
                .IsUnique();

            builder.Property(r => r.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.HasMany(r => r.Balances)
                .WithOne(b => b.Resource)
                .HasForeignKey(b => b.ResourceId);

            builder.HasMany(r => r.InboundItems)
                .WithOne(i => i.Resource)
                .HasForeignKey(i => i.ResourceId);

            builder.HasMany(r => r.OutboundItems)
                .WithOne(o => o.Resource)
                .HasForeignKey(o => o.ResourceId);
        }
    }

}
