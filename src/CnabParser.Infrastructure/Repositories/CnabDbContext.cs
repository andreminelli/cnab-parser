using CnabParser.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CnabParser.Infrastructure.Repositories;

public class CnabDbContext : DbContext
{
    public CnabDbContext(DbContextOptions<CnabDbContext> options) : base(options)
    {
    }

    public DbSet<Store> Stores { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Store>(entity =>
        {
            entity.ToTable("Stores");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Name).IsRequired().HasMaxLength(20);
            entity.Property(e => e.OwnerName).IsRequired().HasMaxLength(15);

            //entity.HasMany(e => e.Transactions)
            //    .WithOne(t => t.Store)
            //    .HasForeignKey("StoreId")
            //    .IsRequired()
            //    .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.Name, e.OwnerName }).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Store)
                .WithMany(s => s.Transactions)
                .HasForeignKey("StoreId")
                .IsRequired();

            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);
            entity.Property(e => e.Card).IsRequired().HasMaxLength(12);
        });
    }
}
