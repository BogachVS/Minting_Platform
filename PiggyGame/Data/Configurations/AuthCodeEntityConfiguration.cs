using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PiggyGame.Data.Entities;

namespace PiggyGame.Data.Configurations;

public class AuthCodeEntityConfiguration : EntityTypeConfiguration<AuthCode>
{
    public override void Configure(EntityTypeBuilder<AuthCode> builder)
    {
        builder
            .HasOne(x => x.User)
            .WithOne(x => x.AuthCode)
            .OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
