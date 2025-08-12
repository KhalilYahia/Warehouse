using Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Model.Configuration
{
    public class OutboundItemConfiguration : IEntityTypeConfiguration<OutboundItem>
    {
        public void Configure(EntityTypeBuilder<OutboundItem> builder)
        {
            builder.ToTable("OutboundItems");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Quantity)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            builder.HasOne(i => i.OutboundDocument)
                .WithMany(d => d.Items)
                .HasForeignKey(i => i.OutboundDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }


}
