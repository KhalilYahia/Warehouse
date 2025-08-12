using Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Model.Configuration
{
    public class InboundItemConfiguration : IEntityTypeConfiguration<InboundItem>
    {
        public void Configure(EntityTypeBuilder<InboundItem> builder)
        {
            builder.ToTable("InboundItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.HasOne(i => i.InboundDocument)
                .WithMany(d => d.Items)
                .HasForeignKey(i => i.InboundDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }


}
