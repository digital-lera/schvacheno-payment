using Microsoft.EntityFrameworkCore;
using Payments.Models;

namespace Payments.Data;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Currency).HasMaxLength(3);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(
            "Host=localhost;Database=schvacheno;Username=postgres;Password=pgpass123;Trust Server Certificate=true",
            x => x.MigrationsHistoryTable("__MyMigrationsHistory", "mySchema"));

}
