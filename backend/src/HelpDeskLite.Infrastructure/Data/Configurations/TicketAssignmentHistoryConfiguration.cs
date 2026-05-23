using HelpDeskLite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskLite.Infrastructure.Data.Configurations;

public class TicketAssignmentHistoryConfiguration : IEntityTypeConfiguration<TicketAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<TicketAssignmentHistory> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.TicketId, a.AssignedAt });
        builder.HasOne(a => a.Ticket).WithMany(t => t.AssignmentHistory).HasForeignKey(a => a.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.PreviousAssignee).WithMany().HasForeignKey(a => a.PreviousAssigneeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.NewAssignee).WithMany().HasForeignKey(a => a.NewAssigneeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.AssignedByUser).WithMany().HasForeignKey(a => a.AssignedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
