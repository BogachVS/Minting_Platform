namespace PiggyGame.Common.DTOs;

public class PagedListDto<T>
{
    public required ICollection<T> Items { get; set; }
    public required int TotalItems { get; set; }
}