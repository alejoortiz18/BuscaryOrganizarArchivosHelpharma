using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ArchivosNas.Models.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ArchivosIndexado> ArchivosIndexados { get; set; }

    public virtual DbSet<ArchivosIndexadosStaging> ArchivosIndexadosStagings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FilesNas;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivosIndexado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Archivos__3214EC0770EA66CB");

            entity.HasIndex(e => e.Extension, "IX_Archivos_Extension");

            entity.HasIndex(e => new { e.Prefijo, e.NumeroFactura }, "IX_Archivos_PrefijoNumero");

            entity.HasIndex(e => e.RutaHash, "IX_Archivos_RutaHash").IsUnique();

            entity.Property(e => e.Extension)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.NombreArchivo).HasMaxLength(500);
            entity.Property(e => e.NumeroFactura)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Prefijo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RutaCompleta).HasMaxLength(1000);
            entity.Property(e => e.RutaHash)
                .HasMaxLength(32)
                .HasComputedColumnSql("(CONVERT([varbinary](32),hashbytes('SHA2_256',[RutaCompleta])))", true);
        });

        modelBuilder.Entity<ArchivosIndexadosStaging>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ArchivosIndexados_Staging");

            entity.HasIndex(e => e.RutaCompleta, "IX_Staging_Ruta");

            entity.Property(e => e.Extension)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.NombreArchivo).HasMaxLength(500);
            entity.Property(e => e.NumeroFactura)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Prefijo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RutaCompleta).HasMaxLength(1000);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
