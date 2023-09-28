using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProducerApi.Application.Abstractions;
using ProducerApi.Domain.Entities;

namespace ProducerApi.Infrastructure.Persistence;

public partial class KafkapubsubContext : DbContext, IPortfolioRepository, IProductRepository
{
    public KafkapubsubContext()
    {
    }

    public KafkapubsubContext(DbContextOptions<KafkapubsubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Portfolio> Portfolios { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public async Task<int> SavePortfoliosAsync(List<Portfolio> portfolios)
    {
        Portfolios.AddRange(portfolios);
        return await SaveChangesAsync();
    }

    public async Task<int> SaveProductAsync(List<Portfolio> portfolios)
    {
        Products.AddRange(Products);
        return await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.HasKey(e => e.PortfolioId).HasName("portfolios_pkey");

            entity.ToTable("portfolios");

            entity.HasIndex(e => e.PortfolioId, "idx_portfolio_uuid");

            entity.Property(e => e.PortfolioId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("portfolio_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PortfolioUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("portfolio_uuid");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("products_pkey");

            entity.ToTable("products");

            entity.HasIndex(e => e.ProductUuid, "idx_product_uuid");

            entity.Property(e => e.ProductId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("product_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_on");
            entity.Property(e => e.Payload)
                .HasColumnType("json")
                .HasColumnName("payload");
            entity.Property(e => e.PortfolioId).HasColumnName("portfolio_id");
            entity.Property(e => e.ProductUuid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("product_uuid");

            entity.HasOne(d => d.Portfolio).WithMany(p => p.Products)
                .HasForeignKey(d => d.PortfolioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("products_portfolio_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
