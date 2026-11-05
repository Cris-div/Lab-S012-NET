using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Veterinaria.Api.Models;

public partial class VeterinariaContext : DbContext
{
    public VeterinariaContext(DbContextOptions<VeterinariaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Mascota> Mascotas { get; set; }

    public virtual DbSet<Propietario> Propietarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.ToTable("Mascotas");
            entity.HasKey(e => e.Id).HasName("Mascotas_pkey");

            entity.HasIndex(e => e.PropietarioId, "IX_Mascotas_PropietarioId");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Especie).HasMaxLength(40);
            entity.Property(e => e.Nombre).HasMaxLength(80);
            entity.Property(e => e.Peso).HasPrecision(7, 2);

            entity.HasOne(d => d.Propietario).WithMany(p => p.Mascotas)
                .HasForeignKey(d => d.PropietarioId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Mascotas_Propietarios");
        });

        modelBuilder.Entity<Propietario>(entity =>
        {
            entity.ToTable("Propietarios");
            entity.HasKey(e => e.Id).HasName("Propietarios_pkey");

            entity.HasIndex(e => e.Documento, "UQ_Propietarios_Documento").IsUnique();

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Documento).HasMaxLength(8);
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(15);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
