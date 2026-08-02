using Microsoft.EntityFrameworkCore;
using OpinionesData.Models;

namespace OpinionesData.Context;

public sealed class OpinionesDbContext : DbContext
{
    public OpinionesDbContext(DbContextOptions<OpinionesDbContext> options)
        : base(options)
    {
    }

    public DbSet<OpinionStaging> OpinionesStaging => Set<OpinionStaging>();

    public DbSet<ResenaWebOrigen> ResenasWebOrigen => Set<ResenaWebOrigen>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpinionStaging>(entity =>
        {
            entity.ToTable("StagingOpiniones");
            entity.HasKey(opinion => opinion.IdStaging);

            entity.Property(opinion => opinion.IdStaging)
                .HasColumnName("IdStaging")
                .ValueGeneratedOnAdd();
            entity.Property(opinion => opinion.LoteId)
                .HasColumnName("LoteId");
            entity.Property(opinion => opinion.Fuente)
                .HasColumnName("Fuente")
                .HasMaxLength(50);
            entity.Property(opinion => opinion.OrigenId)
                .HasColumnName("OrigenId")
                .HasMaxLength(100);
            entity.Property(opinion => opinion.IdCliente)
                .HasColumnName("IdCliente")
                .HasMaxLength(50);
            entity.Property(opinion => opinion.IdProducto)
                .HasColumnName("IdProducto")
                .HasMaxLength(50);
            entity.Property(opinion => opinion.Fecha)
                .HasColumnName("Fecha")
                .HasPrecision(3);
            entity.Property(opinion => opinion.Comentario)
                .HasColumnName("Comentario");
            entity.Property(opinion => opinion.ClasificacionOrigen)
                .HasColumnName("ClasificacionOrigen")
                .HasMaxLength(50);
            entity.Property(opinion => opinion.PuntajeOrigen)
                .HasColumnName("PuntajeOrigen");
            entity.Property(opinion => opinion.FechaExtraccionUtc)
                .HasColumnName("FechaExtraccionUtc")
                .HasPrecision(3);
            entity.Property(opinion => opinion.Estado)
                .HasColumnName("Estado")
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");

            entity.HasIndex(opinion => opinion.LoteId)
                .HasDatabaseName("IX_StagingOpiniones_LoteId");
        });

        modelBuilder.Entity<ResenaWebOrigen>(entity =>
        {
            entity.HasNoKey();
            entity.Property(review => review.IdReview)
                .HasColumnName("IdReview");
            entity.Property(review => review.IdCliente)
                .HasColumnName("IdCliente");
            entity.Property(review => review.IdProducto)
                .HasColumnName("IdProducto");
            entity.Property(review => review.Fecha)
                .HasColumnName("Fecha");
            entity.Property(review => review.Comentario)
                .HasColumnName("Comentario");
        });
    }
}
