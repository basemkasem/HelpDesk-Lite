using HelpDeskLite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskLite.Infrastructure.Data.Configurations;

public class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.HasIndex(h => new { h.TicketId, h.ChangedAt });
        builder.Property(h => h.OldStatus).HasMaxLength(50).IsRequired();
        builder.Property(h => h.NewStatus).HasMaxLength(50).IsRequired();
        builder.HasOne(h => h.Ticket).WithMany(t => t.StatusHistory).HasForeignKey(h => h.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(h => h.ChangedByUser).WithMany().HasForeignKey(h => h.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
