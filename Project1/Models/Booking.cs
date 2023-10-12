using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? SeatRoomComputerClubId { get; set; }
        public int? SeatRoomId { get; set; }
        public int? SeatId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double PriceAmount { get; set; }
        public bool IsCombo { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public virtual Seat? Seat { get; set; }

    }
}
