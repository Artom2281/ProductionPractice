namespace LibraryManagement.Application.Dtos;

public class PublisherDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Description { get; set; }
}
