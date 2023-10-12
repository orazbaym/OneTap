namespace Project1.Params;

public class FilterParams
{
    public double MyLatitude { get; set; } = 51.096244;
    public double MyLongitude { get; set; } = 71.40726;
    public double MaxDistance { get; set; } = 100;
    public bool By24x7 { get; set; }
    public bool ByNearest { get; set; }
    public bool ByRating { get; set; }

}
