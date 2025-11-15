using CnabParser.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CnabParser.Infrastructure.Data;

public class CnabParserDbContext : DbContext
{
    public CnabParserDbContext(DbContextOptions<CnabParserDbContext> options) : base(options)
    {
    }

    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Owner).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.Name, e.Owner }).IsUnique();
            entity.HasMany(e => e.Transactions)
                .WithOne()
                .HasForeignKey("StoreId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.StoreName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoreOwner).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Cpf).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Card).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Time).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => new { e.StoreName, e.StoreOwner });
        });
    }
}
