using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Project1.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly DbContextOptions _options;

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {
            _options = options;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Combo>()
                .HasKey(c => new { c.ComputerClubId, c.Id });
            modelBuilder.Entity<Room>()
                .HasKey(r => new { r.ComputerClubId, r.Id });
            modelBuilder.Entity<Seat>()
                .HasKey(s => new { s.RoomComputerClubId, s.RoomId, s.Id });
        }
        public DbSet<ComputerClub> ComputerClubs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Seat> Seats { get; set; }
    }
}
