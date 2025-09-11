using LiveNetwork.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType;

namespace Persistence.Context.Implementation
{
    public static class EducationTable
    {
        public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<Education>().ToTable(Database.Tables.Educations);

            modelBuilder.Entity<Education>(entity =>
            {
                entity.HasKey(ed => ed.Id);
                entity.Property(ed => ed.Id).HasColumnType(columnTypes.TypeVar);

                entity.Property(ed => ed.School).HasColumnType(columnTypes.TypeVar).IsRequired();
                entity.Property(ed => ed.SchoolUrl).HasColumnType(columnTypes.TypeVar);
                entity.Property(ed => ed.LogoUrl).HasColumnType(columnTypes.TypeVar);
                entity.Property(ed => ed.LogoAlt).HasColumnType(columnTypes.TypeVar);
                entity.Property(ed => ed.Degree).HasColumnType(columnTypes.TypeVar);
                entity.Property(ed => ed.Field).HasColumnType(columnTypes.TypeVar);
                entity.Property(ed => ed.DateRange).HasColumnType(columnTypes.TypeVar);
                entity.Property(ed => ed.Description).HasColumnType(columnTypes.TypeVar);


                entity.Property(e => e.ProfileId)
                        .HasColumnType(columnTypes.TypeVar)
                        .IsRequired();

                entity.HasOne(e => e.Profile)
                      .WithMany(p => p.Educations)
                      .HasForeignKey(e => e.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ✅ define el índice sobre la propiedad y luego nómbralo
                entity.HasIndex(e => e.ProfileId)
                      .HasDatabaseName(Database.Index.IndexEducationByProfile);
            });
        }
    }
}
