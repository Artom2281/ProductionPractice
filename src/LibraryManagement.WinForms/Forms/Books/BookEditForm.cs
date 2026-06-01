using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Books;

public class BookEditForm : Form
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly IGenreService _genreService;
    private readonly IPublisherService _publisherService;

    private readonly TextBox _txtTitle = new() { Width = 320 };
    private readonly TextBox _txtIsbn = new() { Width = 320 };
    private readonly DateTimePicker _dtPublication = new()
    {
        Format = DateTimePickerFormat.Short,
        ShowCheckBox = true,
        Checked = false,
        Width = 200
    };
    private readonly ComboBox _cmbPublisher = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
    private readonly NumericUpDown _numCopies = new() { Width = 100, Minimum = 1, Maximum = 10000, Value = 1 };
    private readonly ComboBox _cmbAuthor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
    private readonly ComboBox _cmbGenre = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 320 };
    private readonly TextBox _txtDescription = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Width = 320,
        Height = 80
    };
    private readonly Label _lblAvailable = new() { AutoSize = true, ForeColor = Color.Gray };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30, DialogResult = DialogResult.Cancel };

    private BookDto _book = new();

    public BookEditForm(IBookService bookService, IAuthorService authorService, IGenreService genreService, IPublisherService publisherService)
    {
        _bookService = bookService;
        _authorService = authorService;
        _genreService = genreService;
        _publisherService = publisherService;

        Text = "Книга";
        // Высоту считает AutoSize TableLayoutPanel внутри. Стартовый размер - "ровно по контенту".
        Size = new Size(500, 560);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        Load += async (_, _) => await LoadDictionariesAsync();
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetBook(BookDto dto)
    {
        _book = dto;
        Text = dto.Id == 0 ? "Новая книга" : "Редактирование книги";
    }

    private void BuildLayout()
    {
        // Кнопки внизу - сначала Dock=Bottom, чтобы layout оставшееся место отдал TableLayoutPanel
        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(280, 10);
        _btnCancel.Location = new Point(390, 10);
        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(15),
            AutoSize = false
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        for (int i = 0; i < 9; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Название:"), 0, 0); layout.Controls.Add(_txtTitle, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("ISBN:"), 0, 1); layout.Controls.Add(_txtIsbn, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Дата издания:"), 0, 2); layout.Controls.Add(_dtPublication, 1, 2);
        layout.Controls.Add(Ui.MakeLabel("Издательство:"), 0, 3); layout.Controls.Add(_cmbPublisher, 1, 3);
        layout.Controls.Add(Ui.MakeLabel("Автор:"), 0, 4); layout.Controls.Add(_cmbAuthor, 1, 4);
        layout.Controls.Add(Ui.MakeLabel("Жанр:"), 0, 5); layout.Controls.Add(_cmbGenre, 1, 5);
        layout.Controls.Add(Ui.MakeLabel("Экземпляров:"), 0, 6); layout.Controls.Add(_numCopies, 1, 6);
        layout.Controls.Add(Ui.MakeLabel(""), 0, 7); layout.Controls.Add(_lblAvailable, 1, 7);
        layout.Controls.Add(Ui.MakeLabel("Описание:"), 0, 8); layout.Controls.Add(_txtDescription, 1, 8);

        // Порядок добавления важен: layout идёт ПОСЛЕ buttons, иначе DockStyle.Fill съест место кнопок
        Controls.Add(layout);
        Controls.Add(buttons);
    }

    private async Task LoadDictionariesAsync()
    {
        try
        {
            var authors = await _authorService.GetAllAsync();
            _cmbAuthor.DisplayMember = nameof(AuthorDto.FullName);
            _cmbAuthor.ValueMember = nameof(AuthorDto.Id);
            _cmbAuthor.DataSource = authors.ToList();

            var genres = await _genreService.GetAllAsync();
            _cmbGenre.DisplayMember = nameof(GenreDto.Name);
            _cmbGenre.ValueMember = nameof(GenreDto.Id);
            _cmbGenre.DataSource = genres.ToList();

            // Издательство опциональное - первой опцией добавляем "не указано" с Id=0
            var publishersFromDb = await _publisherService.GetAllAsync();
            var publishers = new List<PublisherDto> { new() { Id = 0, Name = "— не указано —" } };
            publishers.AddRange(publishersFromDb);
            _cmbPublisher.DisplayMember = nameof(PublisherDto.Name);
            _cmbPublisher.ValueMember = nameof(PublisherDto.Id);
            _cmbPublisher.DataSource = publishers;

            // Применяем значения формы только после того, как заполнены справочники,
            // иначе SelectedValue не успеет связаться с DataSource
            _txtTitle.Text = _book.Title;
            _txtIsbn.Text = _book.Isbn ?? string.Empty;
            if (_book.PublicationDate.HasValue)
            {
                _dtPublication.Value = _book.PublicationDate.Value;
                _dtPublication.Checked = true;
            }
            else
            {
                _dtPublication.Checked = false;
            }
            _numCopies.Value = Math.Max(1, _book.TotalCopies);
            _txtDescription.Text = _book.Description ?? string.Empty;

            if (_book.AuthorId > 0) _cmbAuthor.SelectedValue = _book.AuthorId;
            if (_book.GenreId > 0) _cmbGenre.SelectedValue = _book.GenreId;
            _cmbPublisher.SelectedValue = _book.PublisherId ?? 0;

            _lblAvailable.Text = _book.Id == 0
                ? "При создании все экземпляры считаются доступными."
                : $"Сейчас доступно: {_book.AvailableCopies} из {_book.TotalCopies}.";
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить справочники: " + ex.Message);
        }
    }

    private async Task SaveAsync()
    {
        _btnSave.Enabled = false;
        try
        {
            _book.Title = _txtTitle.Text;
            _book.Isbn = string.IsNullOrWhiteSpace(_txtIsbn.Text) ? null : _txtIsbn.Text;
            _book.PublicationDate = _dtPublication.Checked ? _dtPublication.Value.Date : null;
            _book.TotalCopies = (int)_numCopies.Value;
            _book.AuthorId = _cmbAuthor.SelectedValue is int aid ? aid : 0;
            _book.GenreId = _cmbGenre.SelectedValue is int gid ? gid : 0;
            // 0 = "не указано" - в БД хранится как null
            _book.PublisherId = _cmbPublisher.SelectedValue is int pid && pid > 0 ? pid : null;
            _book.Description = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text;

            var result = await _bookService.SaveAsync(_book);
            if (Ui.ReportResult(this, result))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        finally
        {
            _btnSave.Enabled = true;
        }
    }
}
