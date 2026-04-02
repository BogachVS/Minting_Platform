using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PiggyGame.Data.Entities;

namespace PiggyGame.Data.Configurations;

public class GameEntityConfiguration : EntityTypeConfiguration<Game>
{
    public override void Configure(EntityTypeBuilder<Game> builder)
    {
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Games)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.Configure(builder);
    }
}