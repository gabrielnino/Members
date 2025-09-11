using LiveNetwork.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants;

namespace Persistence.Context.Implementation
{
    public static class CommunicationTable
    {
        public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            // Base abstract type
            modelBuilder.Entity<Interaction>().ToTable(Database.Tables.Communications);

            modelBuilder.Entity<Interaction>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).HasColumnType(columnTypes.TypeVar);

                entity.Property(c => c.Content).HasColumnType(columnTypes.TypeVar);
                entity.Property(c => c.Experiment).HasColumnType(columnTypes.TypeVar);
                entity.Property(c => c.FeedbackNotes).HasColumnType(columnTypes.TypeVar);

                entity.Property(c => c.CreatedAt).HasColumnType(columnTypes.TypeDateTime);
                entity.Property(c => c.UpdatedAt).HasColumnType(columnTypes.TypeDateTime);


                entity.Property(e => e.ProfileId)
                        .HasColumnType(columnTypes.TypeVar)
                        .IsRequired();

                entity.HasOne(e => e.Profile)
                      .WithMany(p => p.Communications)
                      .HasForeignKey(e => e.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);


            });

            // Invite specifics
            modelBuilder.Entity<ConnectionInvite>(entity =>
            {
                entity.Property(i => i.Status)
                      .HasConversion<string>()
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();

                entity.Property(i => i.SentAt)
                      .HasColumnType(columnTypes.TypeDateTime);

                entity.Property(i => i.CompletedAt)
                      .HasColumnType(columnTypes.TypeDateTime);
            });

            // Message specifics
            modelBuilder.Entity<MessageInteraction>(entity =>
            {
                entity.Property(m => m.Status)
                      .HasConversion<string>()
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();

                // If you later add DeliveredAt/ReadAt timestamps to Message, map them here:
                entity.Property<DateTimeOffset?>("DeliveredAt").HasColumnType(columnTypes.TypeDateTime);
                entity.Property<DateTimeOffset?>("ReadAt").HasColumnType(columnTypes.TypeDateTime);
            });
        }
    }
}
