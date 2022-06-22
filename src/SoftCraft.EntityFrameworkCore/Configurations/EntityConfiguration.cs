using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoftCraft.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SoftCraft.Configurations;

public class EntityConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Entities");
        builder.HasKey(x => x.Id);
        builder.ConfigureByConvention();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(60);
        
        builder.HasMany(entity=>entity.Properties)
            .WithOne(property =>  property.Entity)
            .HasForeignKey(property => property.EntityId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}