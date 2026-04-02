namespace PiggyGame.Common.DTOs;

public class PagedListRequest
{
    public int Limit { get; set; }
    public int Offset { get; set; }
}