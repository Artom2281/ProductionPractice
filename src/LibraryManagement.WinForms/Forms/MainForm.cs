using LibraryManagement.Application.Abstractions;
using LibraryManagement.Domain.Enums;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Forms.Admin;
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

namespace LibraryManagement.WinForms.Forms;

// Главное MDI-окно. Слева - NavigationPanel (DockStyle.Left), MdiClient заполняет остальное.
// Поддерживает один открытый child-form каждого типа: повторное открытие активирует существующий.
public partial class MainForm : Form
{
    private readonly ICurrentUserService _currentUser;
    private readonly IConfiguration _configuration;
    private NavigationPanel _navigation = null!;

    public MainForm(ICurrentUserService currentUser, IConfiguration configuration)
    {
        _currentUser = currentUser;
        _configuration = configuration;
        InitializeComponent();
        BuildSidebar();
        UpdateStatusBar();
        // Первое окно открываем после Show - на этот момент handle MainForm уже создан,
        // и MDI child'у есть к чему привязаться через MdiParent.
        Shown += OnFirstShown;
    }

    private void OnFirstShown(object? sender, EventArgs e)
    {
        Shown -= OnFirstShown; // только при первом показе
        // Включаем обработку SectionSelected ПОСЛЕ первого показа MainForm.
        // К этому моменту все отложенные TreeView сообщения (TVM_SELECTITEM от
        // программной установки SelectedNode в Populate) уже обработаны - ранние
        // AfterSelect глушились флагом, и handle MainForm существует.
        _navigation.EnableSelection();
        OpenChild<BooksListForm>();
    }

    private void BuildSidebar()
    {
        _navigation = new NavigationPanel();
        _navigation.SectionSelected += OnSectionSelected;
        Controls.Add(_navigation);
        // Sidebar добавляется поверх MdiClient - переносим её на передний план,
        // чтобы дочерние MDI-формы не перекрывали панель навигации
        _navigation.BringToFront();
        _navigation.Populate(_currentUser.Role ?? UserRole.Librarian);
    }

    private void UpdateStatusBar()
    {
        var roleText = _currentUser.Role switch
        {
            UserRole.Administrator => "Администратор",
            UserRole.Director => "Директор",
            UserRole.Librarian => "Библиотекарь",
            _ => "Пользователь"
        };
        lblUser.Text = $"Пользователь: {_currentUser.Username} ({roleText})";
        var conn = _configuration.GetConnectionString("Library") ?? "не задана";
        lblDatabase.Text = $"БД: {conn}";
    }

    private void OnSectionSelected(object? sender, string sectionKey)
    {
        switch (sectionKey)
        {
            case "books": OpenChild<BooksListForm>(); break;
            case "readers": OpenChild<ReadersListForm>(); break;
            case "loans": OpenChild<LoansListForm>(); break;
            case "authors": OpenChild<AuthorsListForm>(); break;
            case "genres": OpenChild<GenresListForm>(); break;
            case "publishers": OpenChild<PublishersListForm>(); break;
            case "reports": OpenChild<ReportsForm>(); break;
            case "users":
                if (_currentUser.Role == UserRole.Administrator || _currentUser.Role == UserRole.Director)
                    OpenChild<UsersListForm>();
                else Ui.ShowError(this, "Раздел доступен только администратору или директору.");
                break;
        }
    }

    // Открывает MDI-child указанного типа. Если уже открыт - активирует существующий.
    // Несколько разных разделов могут быть открыты одновременно, пользователь применяет
    // меню "Окна -> Каскадом / Горизонтально / Вертикально" для удобной раскладки.
    private void OpenChild<TForm>() where TForm : Form
    {
        var existing = MdiChildren.OfType<TForm>().FirstOrDefault();
        if (existing != null)
        {
            existing.Activate();
            return;
        }

        var form = AppHost.ResolveScopedForm<TForm>();
        form.MdiParent = this;
        form.Show();
    }

    private void OnLogoutClick(object? sender, EventArgs e)
    {
        if (!Ui.Confirm(this, "Выйти из учётной записи и закрыть приложение?")) return;
        _currentUser.SignOut();
        Close();
    }

    private void OnExitClick(object? sender, EventArgs e) => Close();

    private void OnCascadeClick(object? sender, EventArgs e) => LayoutMdi(MdiLayout.Cascade);
    private void OnTileHClick(object? sender, EventArgs e) => LayoutMdi(MdiLayout.TileHorizontal);
    private void OnTileVClick(object? sender, EventArgs e) => LayoutMdi(MdiLayout.TileVertical);

    private void OnCloseAllClick(object? sender, EventArgs e)
    {
        foreach (var child in MdiChildren.ToArray()) child.Close();
    }

    private void OnAboutClick(object? sender, EventArgs e)
    {
        Ui.ShowInfo(this,
            "Система управления библиотекой\n.NET 10 / WinForms / Разработчик: Сидоров Артём Сергеевич из группы ИСс-32\n\nУчебный проект.",
            "О программе");
    }
}
