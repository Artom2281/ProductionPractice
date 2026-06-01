using LibraryManagement.Application;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Data;
using LibraryManagement.WinForms.Forms;
using LibraryManagement.WinForms.Forms.Admin;
using LibraryManagement.WinForms.Forms.Auth;
using LibraryManagement.WinForms.Forms.Authors;
using LibraryManagement.WinForms.Forms.Books;
using LibraryManagement.WinForms.Forms.Genres;
using LibraryManagement.WinForms.Forms.Loans;
using LibraryManagement.WinForms.Forms.Publishers;
using LibraryManagement.WinForms.Forms.Readers;
using LibraryManagement.WinForms.Forms.Reports;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Любая необработанная ошибка - в crashlog.txt рядом с .exe для диагностики
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                        $"[{DateTime.Now:O}] AppDomain unhandled\n{ex}\n\n");
            }
            catch { /* never fail in crash handler */ }
        };

        // Async void event handler'ы (а их у нас много в формах) глотают исключения
        // из задач до выхода в синхронную часть. UnobservedTaskException ловит их когда
        // GC соберёт незаобсервленную Task. Без этого ошибки тонут молча.
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] UnobservedTaskException\n{e.Exception}\n\n");
            }
            catch { }
            e.SetObserved();
        };

        try
        {
            RunApp();
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] Main caught\n{ex}");
            }
            catch { }
            throw;
        }
    }

    private static void RunApp()
    {
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Library")
            ?? throw new InvalidOperationException("Не задана строка подключения 'Library' в appsettings.json.");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        ConfigureServices(services, connectionString);
        var provider = services.BuildServiceProvider();
        AppHost.SetServices(provider);

        // Глобальный перехват необработанных исключений в UI-потоке
        System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        System.Windows.Forms.Application.ThreadException += (_, e) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] ThreadException\n{e.Exception}");
            }
            catch { }
            Ui.ShowError(null, "Необработанная ошибка:\n" + e.Exception.Message, "Сбой");
        };

        try
        {
            AppDbInitializer.InitializeAsync(provider).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] DB init failed\n{ex}");
            }
            catch { }
            Ui.ShowError(null, "Не удалось инициализировать базу данных:\n" + ex.Message, "Ошибка запуска");
            return;
        }

        using var loginScope = provider.CreateScope();
        var loginForm = loginScope.ServiceProvider.GetRequiredService<LoginForm>();

        if (loginForm.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var mainScope = provider.CreateScope();
        var mainForm = mainScope.ServiceProvider.GetRequiredService<MainForm>();
        mainForm.FormClosed += (_, _) => mainScope.Dispose();

        System.Windows.Forms.Application.Run(mainForm);
    }

    private static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services.AddData(connectionString);
        services.AddApplication();

        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        // Все формы регистрируются как Transient - каждый Resolve даёт новый экземпляр
        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();

        services.AddTransient<BooksListForm>();
        services.AddTransient<BookEditForm>();

        services.AddTransient<ReadersListForm>();
        services.AddTransient<ReaderEditForm>();

        services.AddTransient<LoansListForm>();
        services.AddTransient<IssueLoanForm>();
        services.AddTransient<ReturnLoanForm>();

        services.AddTransient<AuthorsListForm>();
        services.AddTransient<AuthorEditForm>();

        services.AddTransient<GenresListForm>();
        services.AddTransient<GenreEditForm>();

        services.AddTransient<PublishersListForm>();
        services.AddTransient<PublisherEditForm>();

        services.AddTransient<ReportsForm>();

        services.AddTransient<UsersListForm>();
        services.AddTransient<UserEditForm>();
    }
}
