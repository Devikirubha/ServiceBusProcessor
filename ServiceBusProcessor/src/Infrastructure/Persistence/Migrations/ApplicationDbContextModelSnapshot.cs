using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
partial class ApplicationDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.2")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("Domain.Entities.ServiceBusMessage", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd().HasColumnType("uniqueidentifier");
            b.Property<string>("Body").IsRequired().HasColumnType("nvarchar(max)");
            b.Property<string>("CorrelationId").HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime2");
            b.Property<string>("CreatedBy").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<string>("ErrorMessage").HasMaxLength(2000).HasColumnType("nvarchar(2000)");
            b.Property<string>("MessageId").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
            b.Property<DateTime?>("ProcessedAt").HasColumnType("datetime2");
            b.Property<int>("RetryCount").HasColumnType("int");
            b.Property<int>("Status").HasColumnType("int");
            b.Property<string>("Subject").IsRequired().HasMaxLength(512).HasColumnType("nvarchar(512)");
            b.Property<DateTime?>("UpdatedAt").HasColumnType("datetime2");
            b.HasKey("Id");
            b.HasIndex("MessageId").IsUnique().HasDatabaseName("IX_ServiceBusMessages_MessageId");
            b.ToTable("ServiceBusMessages");
        });
#pragma warning restore 612, 618
    }
}
