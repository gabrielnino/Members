using LiveNetwork.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType;

namespace Persistence.Context.Implementation
{
    public static class ProfileTable
    {
        public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<Profile>().ToTable(Database.Tables.Profiles);

            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).HasColumnType(columnTypes.TypeVar);

                entity.Property(p => p.FullName).HasColumnType(columnTypes.TypeVar).IsRequired();
                entity.Property(p => p.Headline).HasColumnType(columnTypes.TypeVar);
                entity.Property(p => p.Location).HasColumnType(columnTypes.TypeVar);
                entity.Property(p => p.CurrentCompany).HasColumnType(columnTypes.TypeVar);

                entity.Property(p => p.ProfileImageUrl).HasColumnType(columnTypes.TypeVar);
                entity.Property(p => p.BackgroundImageUrl).HasColumnType(columnTypes.TypeVar);

                entity.Property(p => p.ConnectionDegree).HasColumnType(columnTypes.TypeVar);
                entity.Property(p => p.Connections).HasColumnType(columnTypes.TypeVar);
                entity.Property(p => p.Followers).HasColumnType(columnTypes.TypeVar);

                entity.Property(p => p.AboutText).HasColumnType(columnTypes.TypeVar);

                entity.Property(p => p.Url).HasConversion(
                    v => v.ToString(),
                    v => new Uri(v))
                    .HasColumnType(columnTypes.TypeVar)
                    .IsRequired();

                // Relationships (shadow FKs)
                entity.HasMany(p => p.Experiences)
                      .WithOne()
                      .HasForeignKey("ProfileId")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Educations)
                      .WithOne()
                      .HasForeignKey("ProfileId")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Communications)
                      .WithOne()
                      .HasForeignKey("ProfileId")
                      .OnDelete(DeleteBehavior.Cascade);



                // Optional helpful indexes
                entity.HasIndex(p => p.FullName, Database.Index.IndexProfileFullName);
                entity.HasIndex(p => p.Url, Database.Index.IndexProfileUrl).IsUnique();
            });
        }
    }
}
