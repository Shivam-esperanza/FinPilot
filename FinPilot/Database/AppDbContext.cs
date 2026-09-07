using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using FinPilot.Models;

namespace FinPilot.Database
{
    public class AppDbContext : DbContext
    {
        // Core Table Sets mapping your Domain Models to SQLite Tables
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Bank> Banks { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<CreditCard> CreditCards { get; set; } = null!;
        public DbSet<Offer> Offers { get; set; } = null!;

        // ─── ADDED LINES BELOW: Registers missing tables to clear CS1061 ───
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Recommendation> Recommendations { get; set; } = null!;

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "finpilot.db");
                optionsBuilder.UseSqlite($"Filename={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Email Unique Index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Account Relationships
            modelBuilder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Bank)
                .WithMany(b => b.Accounts)
                .HasForeignKey(a => a.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            // Loan Relationships
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Bank)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            // CreditCard Relationships
            modelBuilder.Entity<CreditCard>()
                .HasOne(c => c.User)
                .WithMany(u => u.CreditCards)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CreditCard>()
                .HasOne(c => c.Bank)
                .WithMany(b => b.CreditCards)
                .HasForeignKey(c => c.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transaction Relationships
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Loan)
                .WithMany()
                .HasForeignKey(t => t.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.CreditCard)
                .WithMany()
                .HasForeignKey(t => t.CreditCardId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification Relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Offer Relationships
            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Bank)
                .WithMany()
                .HasForeignKey(o => o.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recommendation Relationships
            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.Bank)
                .WithMany()
                .HasForeignKey(r => r.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
