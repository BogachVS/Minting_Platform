using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PiggyGame.Data.Entities;

namespace PiggyGame.Data.Configurations;

public class UserConfiguration : EntityTypeConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);
        
        builder.HasIndex(x => x.TelegramId).IsUnique();
        builder.HasIndex(x => x.ChatId).IsUnique();

        builder.OwnsOne(
            x => x.LastInteractionSequence,
            x =>
            {
                x.Property(y => y.Data).HasColumnType("jsonb");
            }
        );

        builder
            .HasOne(x => x.Referrer)
            .WithMany(x => x.Referrals)
            .OnDelete(DeleteBehavior.SetNull);
    }
}