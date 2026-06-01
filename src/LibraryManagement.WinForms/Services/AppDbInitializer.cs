using LibraryManagement.Application.Abstractions;
using LibraryManagement.Data;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Services;

// Применяет миграции при старте приложения и создаёт администратора по умолчанию,
// если в системе ещё нет ни одного пользователя.
// Это даёт работающий первый запуск без ручного SQL.
public static class AppDbInitializer
{
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminPassword = "admin";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync(cancellationToken);

        var hasAnyUser = await db.Users.AnyAsync(cancellationToken);
        if (!hasAnyUser)
        {
            db.Users.Add(new User
            {
                Username = DefaultAdminUsername,
                FullName = "Администратор системы",
                PasswordHash = hasher.Hash(DefaultAdminPassword),
                Role = UserRole.Administrator,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
