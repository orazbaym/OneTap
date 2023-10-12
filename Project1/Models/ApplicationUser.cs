namespace Project1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Firstname { get; set; }
        public string? ImageUrl { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}
