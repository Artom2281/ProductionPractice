using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Loans;

public class IssueLoanForm : Form
{
    private readonly ILoanService _loanService;
    private readonly IBookService _bookService;
    private readonly IReaderService _readerService;
    private readonly ICurrentUserService _currentUser;

    private readonly ComboBox _cmbBook = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 350 };
    private readonly ComboBox _cmbReader = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 350 };
    private readonly DateTimePicker _dtLoan = new() { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 200 };
    private readonly DateTimePicker _dtDue = new() { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(14), Width = 200 };
    private readonly TextBox _txtNotes = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Width = 350, Height = 70 };
    private readonly Button _btnSave = new() { Text = "Выдать", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30, DialogResult = DialogResult.Cancel };

    public IssueLoanForm(ILoanService loanService, IBookService bookService, IReaderService readerService, ICurrentUserService currentUser)
    {
        _loanService = loanService;
        _bookService = bookService;
        _readerService = readerService;
        _currentUser = currentUser;

        Text = "Выдача книги";
        Size = new Size(550, 460);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        Load += async (_, _) => await LoadDictionariesAsync();
        _btnSave.Click += async (_, _) => await IssueAsync();
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(15),
            AutoSize = true,
            Height = 340
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Книга:"), 0, 0); layout.Controls.Add(_cmbBook, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Читатель:"), 0, 1); layout.Controls.Add(_cmbReader, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Дата выдачи:"), 0, 2); layout.Controls.Add(_dtLoan, 1, 2);
        layout.Controls.Add(Ui.MakeLabel("Срок возврата:"), 0, 3); layout.Controls.Add(_dtDue, 1, 3);
        layout.Controls.Add(Ui.MakeLabel("Заметки:"), 0, 4); layout.Controls.Add(_txtNotes, 1, 4);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(320, 10);
        _btnCancel.Location = new Point(430, 10);
        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        Controls.Add(buttons);
        Controls.Add(layout);
    }

    private async Task LoadDictionariesAsync()
    {
        // Только доступные книги (с AvailableCopies > 0) - на руки нечего давать,
        // если экземпляров не осталось
        var availableBooks = await _bookService.GetAllAsync(onlyAvailable: true);
        _cmbBook.DisplayMember = nameof(BookDto.Title);
        _cmbBook.ValueMember = nameof(BookDto.Id);
        _cmbBook.DataSource = availableBooks.ToList();

        // Только не заблокированные читатели
        var readers = await _readerService.GetAllAsync(includeBlocked: false);
        _cmbReader.DisplayMember = nameof(ReaderDto.FullName);
        _cmbReader.ValueMember = nameof(ReaderDto.Id);
        _cmbReader.DataSource = readers.ToList();

        if (availableBooks.Count == 0)
        {
            Ui.ShowInfo(this, "Нет доступных книг для выдачи.");
        }
        if (readers.Count == 0)
        {
            Ui.ShowInfo(this, "Нет активных читателей для выдачи.");
        }
    }

    private async Task IssueAsync()
    {
        if (_currentUser.UserId is not int userId)
        {
            Ui.ShowError(this, "Сессия истекла. Перезайдите в систему.");
            return;
        }

        if (_cmbBook.SelectedValue is not int bookId || bookId <= 0)
        {
            Ui.ShowError(this, "Выберите книгу.");
            return;
        }
        if (_cmbReader.SelectedValue is not int readerId || readerId <= 0)
        {
            Ui.ShowError(this, "Выберите читателя.");
            return;
        }

        _btnSave.Enabled = false;
        try
        {
            var request = new IssueLoanRequest
            {
                BookId = bookId,
                ReaderId = readerId,
                LoanDate = _dtLoan.Value.Date,
                DueDate = _dtDue.Value.Date,
                Notes = string.IsNullOrWhiteSpace(_txtNotes.Text) ? null : _txtNotes.Text
            };

            var result = await _loanService.IssueAsync(request, userId);
            if (Ui.ReportResult(this, result, "Книга выдана."))
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
