using Bookings.Domain.Bookings;
using Bookings.Domain.Bookings.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookings.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => BookingId.From(value))
            .HasColumnName("id");

        builder.Property(x => x.ResourceId)
            .HasConversion(id => id.Value, value => ResourceId.From(value))
            .HasColumnName("resource_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Customer, customer =>
        {
            customer.Property(c => c.Name)
                .HasColumnName("customer_name")
                .HasMaxLength(100)
                .IsRequired();

            customer.Property(c => c.Email)
                .HasConversion(e => e.Value, v => Email.Create(v))
                .HasColumnName("customer_email")
                .HasMaxLength(256)
                .IsRequired();
        });

        builder.Navigation(x => x.Customer).IsRequired();

        builder.OwnsOne(x => x.Range, range =>
        {
            range.Property(r => r.Start).HasColumnName("start_utc").IsRequired();
            range.Property(r => r.End).HasColumnName("end_utc").IsRequired();
        });

        builder.Navigation(x => x.Range).IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.ConfirmedAtUtc).HasColumnName("confirmed_at_utc");
        builder.Property(x => x.CancelledAtUtc).HasColumnName("cancelled_at_utc");
        builder.Property(x => x.CancellationReason).HasColumnName("cancellation_reason").HasMaxLength(500);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => new { x.ResourceId, x.Status });
        builder.HasIndex(x => x.Status);
    }
}
