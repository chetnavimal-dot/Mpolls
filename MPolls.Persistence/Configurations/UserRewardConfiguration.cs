using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MPolls.Domain.Entities;

namespace MPolls.Persistence.Configurations;

public class UserRewardConfiguration : IEntityTypeConfiguration<UserReward>
{
    public void Configure(EntityTypeBuilder<UserReward> builder)
    {
        builder.ToTable("UserRewards");
        builder.HasKey(x => x.RewardId);

        builder.Property(x => x.RewardId)
            .HasDefaultValueSql("NEWID()");

        builder.Property(x => x.PanelistUlid)
            .HasMaxLength(26)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired(false);

        builder.Property(x => x.Points)
            .IsRequired();

        builder.Property(x => x.TransactionType)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(x => x.CreatedOn)
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(x => x.Panelist)
            .WithMany()
            .HasPrincipalKey(p => p.Ulid)
            .HasForeignKey(x => x.PanelistUlid)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PanelistUlid);
    }
}
