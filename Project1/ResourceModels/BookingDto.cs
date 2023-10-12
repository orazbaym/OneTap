namespace Project1.ResourceModels
{
    public class BookingDto
    {
        public string Username { get; set; } = string.Empty;
        public string ComputerClubName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string SeatName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedTime { get; set; }

    }
}
