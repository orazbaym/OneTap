namespace Project1.Params
{
    public class PaginationParams
    {
        private int itemsPerPage = 3;

        public int Page { get; set; } = 1;
        public int ItemsPerPage
        {
            get => itemsPerPage;
            set => itemsPerPage = value;
        }
    }
}
