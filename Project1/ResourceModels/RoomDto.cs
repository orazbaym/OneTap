namespace Project1.ResourceModels
{
    public class RoomDto
    {
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public double PricePerHour { get; set; }
        public int Capacity { get; set; }
    }
}
