namespace Project1.Services
{
    public interface IServiceManagement
    {
        void SendEmail();
        void UpdateDatabase();
        void GenerateMerchandise();
        void SyncData();
        void UpdateExpiredBookings();
    }
}
