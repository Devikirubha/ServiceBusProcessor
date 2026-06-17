using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ServiceBusMessageConfiguration : IEntityTypeConfiguration<ServiceBusMessage>
{
    public void Configure(EntityTypeBuilder<ServiceBusMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MessageId)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.MessageId)
            .IsUnique()
            .HasDatabaseName("IX_ServiceBusMessages_MessageId");

        builder.Property(x => x.Body)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.CorrelationId)
            .HasMaxLength(256);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(256);

        builder.ToTable("ServiceBusMessages");
    }
}
