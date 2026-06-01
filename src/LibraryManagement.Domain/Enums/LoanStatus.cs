namespace LibraryManagement.Domain.Enums;

// Статус выдачи книги читателю
public enum LoanStatus
{
    Active = 1, // Книга на руках, срок возврата ещё не наступил
    Returned = 2, // Книга возвращена в фонд
    Overdue = 3 // Книга на руках, срок возврата уже прошёл
}
