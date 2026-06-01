using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;

namespace LibraryManagement.WinForms.Forms.Authors;

// Список авторов с поиском и CRUD-операциями. Загрузка - на Form_Load и при каждом действии.
public class AuthorsListForm : Form
{
    private readonly IAuthorService _authorService;
    private readonly CrudToolbar _toolbar = new();
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AutoGenerateColumns = false,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
        BackgroundColor = Color.White
    };

    public AuthorsListForm(IAuthorService authorService)
    {
        _authorService = authorService;
        Text = "Авторы";
        ClientSize = new Size(1100, 650);

        BuildGridColumns();

        _toolbar.AddClicked += async (_, _) => await OpenEditAsync(null);
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();

        _grid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0) await EditSelectedAsync();
        };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        Load += async (_, _) => await ReloadAsync();
    }

    private void BuildGridColumns()
    {
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(AuthorDto.Id), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = nameof(AuthorDto.LastName) },
            new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = nameof(AuthorDto.FirstName) },
            new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = nameof(AuthorDto.MiddleName) },
            new DataGridViewTextBoxColumn { HeaderText = "Дата рождения", DataPropertyName = nameof(AuthorDto.BirthDate), DefaultCellStyle = { Format = "dd.MM.yyyy" } }
        );
    }

    private async Task ReloadAsync()
    {
        try
        {
            var data = await _authorService.GetAllAsync(_toolbar.SearchBox.Text);
            _grid.DataSource = data.ToList();
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить авторов: " + ex.Message);
        }
    }

    private AuthorDto? GetSelected()
    {
        return _grid.CurrentRow?.DataBoundItem as AuthorDto;
    }

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null)
        {
            Ui.ShowInfo(this, "Выберите автора в списке.");
            return;
        }
        await OpenEditAsync(current.Id);
    }

    private async Task OpenEditAsync(int? id)
    {
        var dto = id.HasValue
            ? await _authorService.GetByIdAsync(id.Value) ?? new AuthorDto()
            : new AuthorDto();

        var form = AppHost.ResolveScopedForm<AuthorEditForm>();
        form.SetAuthor(dto);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            await ReloadAsync();
        }
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null)
        {
            Ui.ShowInfo(this, "Выберите автора в списке.");
            return;
        }

        if (!Ui.Confirm(this, $"Удалить автора «{current.FullName}»?")) return;

        var result = await _authorService.DeleteAsync(current.Id);
        if (Ui.ReportResult(this, result, "Автор удалён."))
        {
            await ReloadAsync();
        }
    }
}
