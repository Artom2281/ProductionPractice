using LibraryManagement.Application.Abstractions;
using LibraryManagement.Data.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Data;

// Регистрация инфраструктурных сервисов: DbContext + реализация ILibraryDbContext + хешер паролей
public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Строка подключения к БД пуста.", nameof(connectionString));
        }

        services.AddDbContext<LibraryDbContext>(options => options.UseSqlite(connectionString));

        // ILibraryDbContext проксирует уже зарегистрированный LibraryDbContext,
        // чтобы Application слой не зависел от конкретной реализации
        services.AddScoped<ILibraryDbContext>(sp => sp.GetRequiredService<LibraryDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
