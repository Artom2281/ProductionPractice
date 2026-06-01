using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Authors;

// Диалог создания/редактирования автора. Перед открытием вызови SetAuthor(dto).
// Возвращает DialogResult.OK при успешном сохранении.
public class AuthorEditForm : Form
{
    private readonly IAuthorService _authorService;

    private readonly TextBox _txtLastName = new() { Width = 320 };
    private readonly TextBox _txtFirstName = new() { Width = 320 };
    private readonly TextBox _txtMiddleName = new() { Width = 320 };
    private readonly DateTimePicker _dtBirth = new()
    {
        Format = DateTimePickerFormat.Short,
        ShowCheckBox = true,
        Checked = false,
        Width = 280
    };
    private readonly TextBox _txtBiography = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Width = 320,
        Height = 110
    };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30, DialogResult = DialogResult.Cancel };

    private AuthorDto _author = new();

    public AuthorEditForm(IAuthorService authorService)
    {
        _authorService = authorService;

        Text = "Автор";
        Size = new Size(450, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetAuthor(AuthorDto dto)
    {
        _author = dto;
        Text = dto.Id == 0 ? "Новый автор" : "Редактирование автора";
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
        _txtBiography.Text = dto.Biography ?? string.Empty;
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
            Height = 350
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Фамилия:"), 0, 0); layout.Controls.Add(_txtLastName, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Имя:"), 0, 1); layout.Controls.Add(_txtFirstName, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Отчество:"), 0, 2); layout.Controls.Add(_txtMiddleName, 1, 2);
        layout.Controls.Add(Ui.MakeLabel("Дата рождения:"), 0, 3); layout.Controls.Add(_dtBirth, 1, 3);
        layout.Controls.Add(Ui.MakeLabel("Биография:"), 0, 4); layout.Controls.Add(_txtBiography, 1, 4);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(220, 10);
        _btnCancel.Location = new Point(330, 10);
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
            _author.LastName = _txtLastName.Text;
            _author.FirstName = _txtFirstName.Text;
            _author.MiddleName = string.IsNullOrWhiteSpace(_txtMiddleName.Text) ? null : _txtMiddleName.Text;
            _author.BirthDate = _dtBirth.Checked ? _dtBirth.Value.Date : null;
            _author.Biography = string.IsNullOrWhiteSpace(_txtBiography.Text) ? null : _txtBiography.Text;

            var result = await _authorService.SaveAsync(_author);
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
