using LiveNetwork.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType;

namespace Persistence.Context.Implementation
{
    public static class ExperienceRoleTable
    {
        public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<ExperienceRole>().ToTable(Database.Tables.ExperienceRoles);
            modelBuilder.Entity<ExperienceRole>(entity =>
            {
                entity.HasKey(r => r.Id);

                // Map the FK column explicitly (use your column type)
                entity.Property(r => r.ExperienceId)
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();

                entity.Property(r => r.Id).HasColumnType(columnTypes.TypeVar);

                entity.HasOne(r => r.Experience)
                      .WithMany(e => e.Roles)
                      .HasForeignKey(r => r.ExperienceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(r => r.Title).HasColumnType(columnTypes.TypeVar).IsRequired();
                entity.Property(r => r.DateRange).HasColumnType(columnTypes.TypeVar).IsRequired();
                entity.Property(r => r.WorkArrangement).HasColumnType(columnTypes.TypeVar);
                entity.Property(r => r.Description).HasColumnType(columnTypes.TypeVar);
                entity.Property(r => r.ContextualSkills).HasColumnType(columnTypes.TypeVar);

                // ✅ Correct way to name the index
                entity.HasIndex(r => r.ExperienceId)
                      .HasDatabaseName(Database.Index.IndexRoleByExperience);
            });
        }
    }
}
