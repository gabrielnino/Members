using LiveNetwork.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStructure.Constants;
using Persistence.CreateStructure.Constants.ColumnType;


namespace Persistence.Context.Implementation
{
    public static class ExperienceTable
    {
        public static void Create(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<Experience>().ToTable(Database.Tables.Experiences);

            modelBuilder.Entity<Experience>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Company).HasColumnType(columnTypes.TypeVar).IsRequired();
                entity.Property(e => e.CompanyUrl).HasColumnType(columnTypes.TypeVar);
                entity.Property(e => e.CompanyLogoUrl).HasColumnType(columnTypes.TypeVar);
                entity.Property(e => e.CompanyLogoAlt).HasColumnType(columnTypes.TypeVar);
                entity.Property(e => e.EmploymentSummary).HasColumnType(columnTypes.TypeVar);
                entity.Property(e => e.Location).HasColumnType(columnTypes.TypeVar);


                entity.Property(e => e.ProfileId)
                      .HasColumnType(columnTypes.TypeVar)
                      .IsRequired();

                entity.HasOne(e => e.Profile)
                      .WithMany(p => p.Experiences)
                      .HasForeignKey(e => e.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Experience -> Roles
                entity.HasMany(e => e.Roles)
                .WithOne(r => r.Experience)
                .HasForeignKey(r => r.ExperienceId)
                .OnDelete(DeleteBehavior.Cascade);


                // Optional index:
                entity.HasIndex(e => e.ProfileId)
                .HasDatabaseName(Database.Index.IndexExperienceByProfile);
            });
        }
    }
}
