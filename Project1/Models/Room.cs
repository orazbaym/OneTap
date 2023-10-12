using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1.Models
{
    public class Room
    {
        public int Id { get; set; }
        public int ComputerClubId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double PricePerHour { get; set; }
        public int Capacity { get; set; }

        [Required]
        public virtual ComputerClub? ComputerClub { get; set; }
        public virtual ICollection<Seat>? Seats { get; set; }

    }
}
