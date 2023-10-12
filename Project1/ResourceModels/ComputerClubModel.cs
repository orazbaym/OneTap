namespace Project1.ResourceModels
{
    public class ComputerClubModel
    {
        public string Name { get; set; } = string.Empty;
        public double Rating { get; set; }
        public int ReviewNumbers { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Imageurl { get; set; } = string.Empty;
        public bool Is24x7 { get; set; } // isOpen? 
        public double PricePerHour { get; set; }
        public int OverallCapacity { get; set; }
    }
}
