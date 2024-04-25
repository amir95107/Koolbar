using Datalayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Datalayer.Data
{
    public class ApplicationDBContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<Sponser> Sponsers { get; set; }
        public virtual DbSet<SponserStatistics> SponserStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Config(modelBuilder);
        }

        private void Config(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Request>()
                    .HasOne(x => x.User)
                    .WithMany(x => x.Requests)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Users_Requests");

            modelBuilder.Entity<City>()
                    .HasOne(x => x.State)
                    .WithMany(x => x.Cities)
                    .HasForeignKey(m => m.StateId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_State_City");

            modelBuilder.Entity<State>()
                    .HasOne(x => x.Country)
                    .WithMany(x => x.States)
                    .HasForeignKey(m => m.ContryId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Country_States");

            modelBuilder.Entity<WalletHistory>()
                    .HasOne(x => x.Wallet)
                    .WithMany(x => x.WalletHistories)
                    .HasForeignKey(m => m.WalletId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Wallet_WalletHistories");

            modelBuilder.Entity<Wallet>()
                    .HasOne(x => x.User)
                    .WithOne(x => x.Wallet)
                    .HasForeignKey<Wallet>(m => m.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_User_Wallet");
        }
    }
}
