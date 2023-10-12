using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

namespace Project1.Services
{
    public class ServiceManagement : IServiceManagement
    {
        private readonly AppDbContext _dbContext;

        public ServiceManagement(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void GenerateMerchandise()
        {
            Console.WriteLine($"Generate Merchandise: Long running task {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        public void SendEmail()
        {
            Console.WriteLine($"Send Email: Short running task {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        public void SyncData()
        {
            Console.WriteLine($"Sync Data: Short running task {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        public void UpdateDatabase()
        {
            Console.WriteLine($"Update Database: Long running task {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        public void UpdateExpiredBookings()
        {
            Console.WriteLine($"Started Update Expired Booking Local: Long running task {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Asia Standard Time");
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, targetTimeZone);
            var expiredBookings = _dbContext.Seats
                .Where(s => s.AvailabilityStatus == "Booked" && s.BookingTimestamp < dateTime)
                .ToList();

            foreach (var seat in expiredBookings)
            {
                seat.AvailabilityStatus = "Available";
            }
            List<Booking> bookings = _dbContext.Bookings.Where(s => s.EndTime < dateTime).ToList();
            foreach(var booking in bookings)
            {
                _dbContext.Bookings.Remove(booking);
            }

            _dbContext.SaveChanges();

            Console.WriteLine($"Ended Update Expired Booking Local: Long running task {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
        }
    }
}
