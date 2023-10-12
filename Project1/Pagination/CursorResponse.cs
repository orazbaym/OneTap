namespace Project1.Pagination
{
    public sealed record CursorResponse<T>(
        long Cursor,
        T Data);
}
