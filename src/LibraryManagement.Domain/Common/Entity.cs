namespace LibraryManagement.Domain.Common;

// Базовый класс для всех доменных сущностей с целочисленным идентификатором и аудит-полями
public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
