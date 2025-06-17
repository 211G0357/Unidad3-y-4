using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace RestauranteApi.Models.Entities;

public partial class HamburguesaContext : DbContext
{
    public HamburguesaContext()
    {
    }

    public HamburguesaContext(DbContextOptions<HamburguesaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Hamburguesa> Hamburguesa { get; set; }

    public virtual DbSet<Papas> Papas { get; set; }

    public virtual DbSet<Pedido> Pedido { get; set; }

    public virtual DbSet<Pedidococina> Pedidococina { get; set; }

    public virtual DbSet<Pedidodetalle> Pedidodetalle { get; set; }

    public virtual DbSet<Refrescoprecio> Refrescoprecio { get; set; }

    public virtual DbSet<Saboresrefresco> Saboresrefresco { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=root;database=Hamburguesa", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.34-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Hamburguesa>(entity =>
        {
            entity.HasKey(e => e.IdHamburguesa).HasName("PRIMARY");

            entity.ToTable("hamburguesa");

            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Precio).HasPrecision(8, 2);
        });

        modelBuilder.Entity<Papas>(entity =>
        {
            entity.HasKey(e => e.IdPapas).HasName("PRIMARY");

            entity.ToTable("papas");

            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Precio).HasPrecision(8, 2);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PRIMARY");

            entity.ToTable("pedido");

            entity.HasIndex(e => e.IdUsuario, "IdUsuario");

            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Pendiente'")
                .HasColumnType("enum('Pendiente','Preparación','Terminado')");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pedido)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("pedido_ibfk_1");
        });

        modelBuilder.Entity<Pedidococina>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PRIMARY");

            entity.ToTable("pedidococina");

            entity.HasIndex(e => e.IdDetalle, "IdDetalle");

            entity.Property(e => e.Estado)
                .HasDefaultValueSql("'Pendiente'")
                .HasColumnType("enum('Pendiente','En preparación','Terminado')");

            entity.HasOne(d => d.IdDetalleNavigation).WithMany(p => p.Pedidococina)
                .HasForeignKey(d => d.IdDetalle)
                .HasConstraintName("pedidococina_ibfk_1");
        });

        modelBuilder.Entity<Pedidodetalle>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PRIMARY");

            entity.ToTable("pedidodetalle");

            entity.HasIndex(e => e.IdPedido, "IdPedido");

            entity.Property(e => e.PrecioUnitario).HasPrecision(8, 2);
            entity.Property(e => e.TipoProducto).HasColumnType("enum('Hamburguesa','Papas','Refresco')");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.Pedidodetalle)
                .HasForeignKey(d => d.IdPedido)
                .HasConstraintName("pedidodetalle_ibfk_1");
        });

        modelBuilder.Entity<Refrescoprecio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refrescoprecio");

            entity.Property(e => e.IdSaboresRefresco).HasColumnName("idSaboresRefresco");
            entity.Property(e => e.Precio).HasPrecision(8, 2);
            entity.Property(e => e.Tamaño).HasMaxLength(50);
        });

        modelBuilder.Entity<Saboresrefresco>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("saboresrefresco");

            entity.Property(e => e.Sabor).HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

            entity.ToTable("usuario");

            entity.Property(e => e.Contraseña).HasMaxLength(255);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Rol).HasColumnType("enum('Mesero','Cocinero')");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
