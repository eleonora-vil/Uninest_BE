using Microsoft.EntityFrameworkCore;

namespace BE_EXE201.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        #region Dbset
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Utilities> Utilities { get; set; }
        public DbSet<Home> Homes { get; set; }
        public DbSet<HomeImage> HomeImages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<HomePostingTransaction> HomePostingTransactions { get; set; }


        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Home>()
            .HasOne(h => h.User)
            .WithMany() // Assuming User has many Homes
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.NoAction); // No cascading delete*/
        }
    }
}