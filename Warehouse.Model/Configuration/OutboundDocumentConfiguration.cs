using Warehouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Model.Configuration
{
    public class OutboundDocumentConfiguration : IEntityTypeConfiguration<OutboundDocument>
    {
        public void Configure(EntityTypeBuilder<OutboundDocument> builder)
        {
            builder.ToTable("OutboundDocuments");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Number)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(d => d.Number)
                .IsUnique();

            builder.Property(d => d.Date)
                .IsRequired();

            builder.Property(d => d.IsSigned)
                .IsRequired();

            builder.HasOne(d => d.Client)
                .WithMany(c => c.OutboundDocuments)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }


}
