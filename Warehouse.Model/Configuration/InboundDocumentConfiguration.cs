using Warehouse.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Warehouse.Model.Configuration
{
    public class InboundDocumentConfiguration : IEntityTypeConfiguration<InboundDocument>
    {
        public void Configure(EntityTypeBuilder<InboundDocument> builder)
        {
            builder.ToTable("InboundDocuments");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Number)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(d => d.Number)
                .IsUnique();

            builder.Property(d => d.Date)
                .IsRequired();
        }
    }


}
