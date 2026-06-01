using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraryManagement.Data;

// Используется только утилитой dotnet ef для создания миграций.
// На рантайме не подключается - там DbContext конфигурируется через DI из WinForms.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    public LibraryDbContext CreateDbContext(string[] args)
    {
        // База в системном Temp - чтобы не оставлять library.design.db в исходниках после
        // dotnet ef migrations add. Файл служит только для генерации миграций.
        var path = Path.Combine(Path.GetTempPath(), "library.design.db");
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite($"Data Source={path}")
            .Options;
        return new LibraryDbContext(options);
    }
}
