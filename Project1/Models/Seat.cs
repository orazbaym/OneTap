using System.ComponentModel.DataAnnotations;

namespace Project1.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int RoomComputerClubId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AvailabilityStatus { get; set;} = string.Empty;
        public DateTime BookingTimestamp { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
        [Required]
        public virtual Room? Room { get; set; }

    }
}
