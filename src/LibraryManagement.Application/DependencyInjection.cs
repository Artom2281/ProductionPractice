using FluentValidation;
using LibraryManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LibraryManagement.Application;

// Регистрация сервисов и валидаторов Application слоя в контейнере DI
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IGenreService, GenreService>();
        services.AddScoped<IPublisherService, PublisherService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IReaderService, ReaderService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
