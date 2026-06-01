using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms.Services;

// Глобальный доступ к сервисам приложения и хелперы для управления DI-scope формы.
// Каждая форма, открываемая через ResolveScopedForm, получает собственный IServiceScope,
// который автоматически освобождается при закрытии формы. Это нужно EF Core DbContext'у,
// зарегистрированному как Scoped: его жизненный цикл должен совпадать со временем жизни формы.
public static class AppHost
{
    public static IServiceProvider Services { get; private set; } = null!;

    public static void SetServices(IServiceProvider provider)
    {
        if (Services is not null)
        {
            throw new InvalidOperationException("AppHost.Services уже инициализирован. Повторная инициализация недопустима.");
        }
        Services = provider;
    }

    // Создаёт новый scope, разрешает в нём форму TForm и связывает scope.Dispose с FormClosed
    public static TForm ResolveScopedForm<TForm>() where TForm : Form
    {
        var scope = Services.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<TForm>();
        form.FormClosed += (_, _) => scope.Dispose();
        return form;
    }
}
