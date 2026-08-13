using Microsoft.EntityFrameworkCore;
using OpinionesData.Models;

namespace OpinionesData.Context;

public sealed class OpinionesDbContext(DbContextOptions<OpinionesDbContext> options) : DbContext(options)
{
    public DbSet<OpinionStaging> OpinionesStaging => Set<OpinionStaging>();
    public DbSet<ResenaWebOrigen> ResenasWebOrigen => Set<ResenaWebOrigen>();
    public DbSet<Opinion> Opiniones => Set<Opinion>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<FuenteDato> FuentesDatos => Set<FuenteDato>();

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

        modelBuilder.Entity<Opinion>(entity =>
        {
            entity.ToTable("Opiniones");

            entity.HasKey(opinion => opinion.IdOpinion);

            entity.Property(opinion => opinion.IdOpinion)
                .HasColumnName("IdOpinion")
                .ValueGeneratedOnAdd();

            entity.Property(opinion => opinion.IdCliente)
                .HasColumnName("IdCliente")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(opinion => opinion.IdProducto)
                .HasColumnName("IdProducto")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(opinion => opinion.IdFuente)
                .HasColumnName("IdFuente");

            entity.Property(opinion => opinion.Fecha)
                .HasColumnName("Fecha")
                .HasColumnType("datetime");

            entity.Property(opinion => opinion.Comentario)
                .HasColumnName("Comentario")
                .HasColumnType("varchar(max)");

            entity.Property(opinion => opinion.Clasificacion)
                .HasColumnName("Clasificacion")
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(opinion => opinion.PuntajeSatisfaccion)
                .HasColumnName("PuntajeSatisfaccion");

            entity.Property(opinion => opinion.OrigenId)
                .HasColumnName("OrigenId")
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasIndex(opinion => new
            {
                opinion.OrigenId,
                opinion.IdFuente
            })
                .IsUnique()
                .HasFilter("[OrigenId] IS NOT NULL")
                .HasDatabaseName("UQ_Opiniones_OrigenId_IdFuente");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(cliente => cliente.IdCliente);
            entity.Property(cliente => cliente.IdCliente)
                .HasColumnName("IdCliente")
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(cliente => cliente.Nombre)
                .HasColumnName("Nombre")
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(cliente => cliente.Email)
                .HasColumnName("Email")
                .HasMaxLength(150)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos");
            entity.HasKey(producto => producto.IdProducto);
            entity.Property(producto => producto.IdProducto)
                .HasColumnName("IdProducto")
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(producto => producto.Nombre)
                .HasColumnName("Nombre")
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(producto => producto.Categoria)
                .HasColumnName("Categoria")
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FuenteDato>(entity =>
        {
            entity.ToTable("FuenteDatos");
            entity.HasKey(fuente => fuente.IdFuente);
            entity.Property(fuente => fuente.IdFuente)
                .HasColumnName("IdFuente")
                .ValueGeneratedOnAdd();
            entity.Property(fuente => fuente.TipoFuente)
                .HasColumnName("TipoFuente")
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(fuente => fuente.FechaCarga)
                .HasColumnName("FechaCarga")
                .HasColumnType("datetime");
        });
    }
}
