using Microsoft.EntityFrameworkCore;
using SpendWise.Models;

namespace SpendWise.Data;

public class SpendWiseDbContext : DbContext
{
    public SpendWiseDbContext(DbContextOptions<SpendWiseDbContext> options) : base(options) { }

    // This represents Table in SQL
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)"); // <--- Add this line

        modelBuilder.Entity<Transaction>()
            .HasDiscriminator<string>("TransactionType")
            .HasValue<OneTimeExpense>("Expense")
            .HasValue<Subscription>("Subscription");
    }
}