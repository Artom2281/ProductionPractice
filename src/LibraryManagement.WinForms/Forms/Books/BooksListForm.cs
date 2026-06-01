using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Forms.Authors;
using LibraryManagement.WinForms.Forms.Genres;
using LibraryManagement.WinForms.Forms.Publishers;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;

namespace LibraryManagement.WinForms.Forms.Books;

// Главный раздел библиотеки: список книг с фильтрами по автору, жанру и наличию.
public class BooksListForm : Form
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly IGenreService _genreService;
    private readonly IPublisherService _publisherService;

    private readonly CrudToolbar _toolbar = new();
    private readonly ComboBox _cmbAuthorFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
    private readonly ComboBox _cmbGenreFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly CheckBox _chkAvailableOnly = new() { Text = "Только доступные", AutoSize = true };

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

    public BooksListForm(IBookService bookService, IAuthorService authorService, IGenreService genreService, IPublisherService publisherService)
    {
        _bookService = bookService;
        _authorService = authorService;
        _genreService = genreService;
        _publisherService = publisherService;

        Text = "Книги";
        ClientSize = new Size(1100, 650);

        BuildGridColumns();
        BuildExtraFilters();

        _toolbar.AddClicked += async (_, _) => await OpenEditAsync(null);
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();

        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync(); };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        Load += async (_, _) => { await LoadFiltersAsync(); await ReloadAsync(); };
    }

    private void BuildGridColumns()
    {
        // Колонка "Доступно / всего" привязана к computed-свойству BookDto.AvailabilityText.
        // Раньше я перезаписывал Cells[].Value у int-колонки строкой "5 / 5" -
        // это вызывало ArgumentException при попытке DataGridView записать значение обратно.
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(BookDto.Id), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = nameof(BookDto.Title) },
            new DataGridViewTextBoxColumn { HeaderText = "Автор", DataPropertyName = nameof(BookDto.AuthorFullName) },
            new DataGridViewTextBoxColumn { HeaderText = "Жанр", DataPropertyName = nameof(BookDto.GenreName) },
            new DataGridViewTextBoxColumn { HeaderText = "Издательство", DataPropertyName = nameof(BookDto.PublisherName) },
            new DataGridViewTextBoxColumn { HeaderText = "Дата издания", DataPropertyName = nameof(BookDto.PublicationDate), DefaultCellStyle = { Format = "dd.MM.yyyy" } },
            new DataGridViewTextBoxColumn { HeaderText = "ISBN", DataPropertyName = nameof(BookDto.Isbn) },
            new DataGridViewTextBoxColumn { HeaderText = "Доступно / всего", DataPropertyName = nameof(BookDto.AvailabilityText) }
        );
    }

    // Добавляет фильтры (автор, жанр, "только доступные") в правую часть тулбара
    private void BuildExtraFilters()
    {
        var lblAuthor = new Label { Text = "Автор:", AutoSize = true, Margin = new Padding(8, 9, 4, 0) };
        var lblGenre = new Label { Text = "Жанр:", AutoSize = true, Margin = new Padding(8, 9, 4, 0) };

        _cmbAuthorFilter.SelectedIndexChanged += async (_, _) => await ReloadAsync();
        _cmbGenreFilter.SelectedIndexChanged += async (_, _) => await ReloadAsync();
        _chkAvailableOnly.CheckedChanged += async (_, _) => await ReloadAsync();

        _toolbar.ExtraButtonsPanel.Controls.AddRange(new Control[]
        {
            lblAuthor, _cmbAuthorFilter, lblGenre, _cmbGenreFilter, _chkAvailableOnly
        });
    }

    private async Task LoadFiltersAsync()
    {
        var authors = await _authorService.GetAllAsync();
        var authorOptions = new List<AuthorDto> { new() { Id = 0, LastName = "— все —" } };
        authorOptions.AddRange(authors);
        _cmbAuthorFilter.DisplayMember = nameof(AuthorDto.FullName);
        _cmbAuthorFilter.ValueMember = nameof(AuthorDto.Id);
        _cmbAuthorFilter.DataSource = authorOptions;

        var genres = await _genreService.GetAllAsync();
        var genreOptions = new List<GenreDto> { new() { Id = 0, Name = "— все —" } };
        genreOptions.AddRange(genres);
        _cmbGenreFilter.DisplayMember = nameof(GenreDto.Name);
        _cmbGenreFilter.ValueMember = nameof(GenreDto.Id);
        _cmbGenreFilter.DataSource = genreOptions;
    }

    private async Task ReloadAsync()
    {
        try
        {
            int? authorId = _cmbAuthorFilter.SelectedValue is int a && a > 0 ? a : null;
            int? genreId = _cmbGenreFilter.SelectedValue is int g && g > 0 ? g : null;
            var data = await _bookService.GetAllAsync(_toolbar.SearchBox.Text, authorId, genreId, _chkAvailableOnly.Checked);
            _grid.DataSource = data.ToList();

            // Подсветка - книги без свободных экземпляров. Без записи в Value (она bound к computed-свойству).
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is BookDto b && b.AvailableCopies == 0)
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить книги: " + ex.Message);
        }
    }

    private BookDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as BookDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите книгу в списке."); return; }
        await OpenEditAsync(current.Id);
    }

    private async Task OpenEditAsync(int? id)
    {
        // Для новой книги нужны и автор и жанр - предлагаем создать прямо отсюда,
        // без открытия пустой формы книги. Для редактирования существующей книги -
        // справочники уже непустые по построению.
        if (!id.HasValue)
        {
            if (!await EnsureAuthorAvailableAsync()) return;
            if (!await EnsureGenreAvailableAsync()) return;
        }

        var dto = id.HasValue
            ? await _bookService.GetByIdAsync(id.Value) ?? new BookDto()
            : new BookDto();

        var form = AppHost.ResolveScopedForm<BookEditForm>();
        form.SetBook(dto);
        if (form.ShowDialog(this) == DialogResult.OK) await ReloadAsync();
    }

    private async Task<bool> EnsureAuthorAvailableAsync()
    {
        var authors = await _authorService.GetAllAsync();
        if (authors.Count > 0) return true;

        if (!Ui.Confirm(this, "В базе нет авторов. Создать автора сейчас?")) return false;

        var form = AppHost.ResolveScopedForm<AuthorEditForm>();
        form.SetAuthor(new AuthorDto());
        return form.ShowDialog(this) == DialogResult.OK;
    }

    private async Task<bool> EnsureGenreAvailableAsync()
    {
        var genres = await _genreService.GetAllAsync();
        if (genres.Count > 0) return true;

        if (!Ui.Confirm(this, "В базе нет жанров. Создать жанр сейчас?")) return false;

        var form = AppHost.ResolveScopedForm<GenreEditForm>();
        form.SetGenre(new GenreDto());
        return form.ShowDialog(this) == DialogResult.OK;
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите книгу в списке."); return; }
        if (!Ui.Confirm(this, $"Удалить книгу «{current.Title}»?")) return;

        var result = await _bookService.DeleteAsync(current.Id);
        if (Ui.ReportResult(this, result, "Книга удалена."))
        {
            await ReloadAsync();
        }
    }
}
