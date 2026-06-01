using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Readers;

public class ReaderEditForm : Form
{
    private readonly IReaderService _readerService;

    private readonly TextBox _txtCard = new() { Width = 280 };
    private readonly TextBox _txtLastName = new() { Width = 280 };
    private readonly TextBox _txtFirstName = new() { Width = 280 };
    private readonly TextBox _txtMiddleName = new() { Width = 280 };
    private readonly DateTimePicker _dtBirth = new()
    {
        Format = DateTimePickerFormat.Short,
        ShowCheckBox = true,
        Checked = false,
        Width = 280
    };
    private readonly MaskedTextBox _txtPhone = new()
    {
        Mask = "+7 (000) 000-00-00",
        Width = 280,
        PromptChar = '_',
        ResetOnPrompt = false,
        ResetOnSpace = false
    };
    private readonly TextBox _txtEmail = new() { Width = 280, PlaceholderText = "user@example.com" };
    private readonly TextBox _txtAddress = new() { Width = 280 };
    private readonly CheckBox _chkBlocked = new() { Text = "Заблокирован", AutoSize = true };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30, DialogResult = DialogResult.Cancel };

    private ReaderDto _reader = new();

    public ReaderEditForm(IReaderService readerService)
    {
        _readerService = readerService;

        Text = "Читатель";
        Size = new Size(480, 540);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetReader(ReaderDto dto)
    {
        _reader = dto;
        Text = dto.Id == 0 ? "Новый читатель" : "Редактирование читателя";
        _txtCard.Text = dto.CardNumber;
        _txtLastName.Text = dto.LastName;
        _txtFirstName.Text = dto.FirstName;
        _txtMiddleName.Text = dto.MiddleName ?? string.Empty;
        if (dto.BirthDate.HasValue)
        {
            _dtBirth.Value = dto.BirthDate.Value;
            _dtBirth.Checked = true;
        }
        else
        {
            _dtBirth.Checked = false;
        }
        // MaskedTextBox принимает текст только если он соответствует маске - иначе очищаем
        _txtPhone.Text = dto.Phone ?? string.Empty;
        _txtEmail.Text = dto.Email ?? string.Empty;
        _txtAddress.Text = dto.Address ?? string.Empty;
        _chkBlocked.Checked = dto.IsBlocked;
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(15),
            AutoSize = true,
            Height = 410
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("№ билета:"), 0, 0); layout.Controls.Add(_txtCard, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Фамилия:"), 0, 1); layout.Controls.Add(_txtLastName, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Имя:"), 0, 2); layout.Controls.Add(_txtFirstName, 1, 2);
        layout.Controls.Add(Ui.MakeLabel("Отчество:"), 0, 3); layout.Controls.Add(_txtMiddleName, 1, 3);
        layout.Controls.Add(Ui.MakeLabel("Дата рождения:"), 0, 4); layout.Controls.Add(_dtBirth, 1, 4);
        layout.Controls.Add(Ui.MakeLabel("Телефон:"), 0, 5); layout.Controls.Add(_txtPhone, 1, 5);
        layout.Controls.Add(Ui.MakeLabel("Email:"), 0, 6); layout.Controls.Add(_txtEmail, 1, 6);
        layout.Controls.Add(Ui.MakeLabel("Адрес:"), 0, 7); layout.Controls.Add(_txtAddress, 1, 7);
        layout.Controls.Add(Ui.MakeLabel(""), 0, 8); layout.Controls.Add(_chkBlocked, 1, 8);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(250, 10);
        _btnCancel.Location = new Point(360, 10);
        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        Controls.Add(buttons);
        Controls.Add(layout);
    }

    private async Task SaveAsync()
    {
        _btnSave.Enabled = false;
        try
        {
            _reader.CardNumber = _txtCard.Text;
            _reader.LastName = _txtLastName.Text;
            _reader.FirstName = _txtFirstName.Text;
            _reader.MiddleName = string.IsNullOrWhiteSpace(_txtMiddleName.Text) ? null : _txtMiddleName.Text;
            _reader.BirthDate = _dtBirth.Checked ? _dtBirth.Value.Date : null;

            // Из MaskedTextBox: если пользователь ничего не вводил - null;
            // если ввёл частично - тоже сбрасываем в null (валидатор отбракует частичный номер
            // с непонятной ошибкой про regex). MaskFull = всё поле заполнено.
            _reader.Phone = _txtPhone.MaskFull ? _txtPhone.Text : null;

            _reader.Email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text;
            _reader.Address = string.IsNullOrWhiteSpace(_txtAddress.Text) ? null : _txtAddress.Text;
            _reader.IsBlocked = _chkBlocked.Checked;

            var result = await _readerService.SaveAsync(_reader);
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
