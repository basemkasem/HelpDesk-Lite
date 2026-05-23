using HelpDeskLite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDeskLite.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.TicketNumber).IsUnique();
        builder.Property(t => t.TicketNumber).HasMaxLength(32).IsRequired();
        builder.Property(t => t.Title).HasMaxLength(300).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(8000).IsRequired();
        builder.Property(t => t.Status).HasMaxLength(50).IsRequired();
        builder.HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.CreatedByUser)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
