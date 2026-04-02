using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PiggyGame.Data.Entities;

namespace PiggyGame.Data.Configurations;

public class InternalCallbackDataConfiguration : EntityTypeConfiguration<InternalCallbackData>
{
    public override void Configure(EntityTypeBuilder<InternalCallbackData> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Data).HasColumnType("jsonb");
    }
}