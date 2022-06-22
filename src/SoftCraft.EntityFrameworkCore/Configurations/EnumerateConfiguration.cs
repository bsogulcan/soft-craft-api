using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftCraft.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SoftCraft.Configurations;

public class EnumerateConfiguration:IEntityTypeConfiguration<Enumerate>
{
    public void Configure(EntityTypeBuilder<Enumerate> builder)
    {
        builder.ToTable("Enumerates");
        builder.HasKey(x => x.Id);
        builder.ConfigureByConvention();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(60);
    }
}