using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftCraft.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SoftCraft.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(x => x.Id);
        builder.ConfigureByConvention();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(60);
    }
}