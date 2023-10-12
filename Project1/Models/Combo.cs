using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project1.Models
{
    public class Combo
    {
        public int Id { get; set; }
        public int ComputerClubId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public int ComboHours { get; set; }
        public int AmountOfHours { get; set; }

        [Required]
        public virtual ComputerClub? ComputerClub { get; set; }
    }
}
