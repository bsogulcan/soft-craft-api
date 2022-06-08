using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftCraft.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SoftCraft.Configurations;

public class EnumerateValueConfiguration:IEntityTypeConfiguration<EnumerateValue>
{
    public void Configure(EntityTypeBuilder<EnumerateValue> builder)
    {
        builder.ToTable("EnumerateValues");
        builder.HasKey(x => x.Id);
        builder.ConfigureByConvention();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(60);

    }
}