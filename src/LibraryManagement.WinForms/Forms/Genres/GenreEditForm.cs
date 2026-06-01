using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Genres;

public class GenreEditForm : Form
{
    private readonly IGenreService _genreService;

    private readonly TextBox _txtName = new() { Width = 320 };
    private readonly TextBox _txtDescription = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Width = 320,
        Height = 130
    };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30, DialogResult = DialogResult.Cancel };

    private GenreDto _genre = new();

    public GenreEditForm(IGenreService genreService)
    {
        _genreService = genreService;

        Text = "Жанр";
        Size = new Size(450, 340);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetGenre(GenreDto dto)
    {
        _genre = dto;
        Text = dto.Id == 0 ? "Новый жанр" : "Редактирование жанра";
        _txtName.Text = dto.Name;
        _txtDescription.Text = dto.Description ?? string.Empty;
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(15),
            AutoSize = true,
            Height = 220
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Название:"), 0, 0); layout.Controls.Add(_txtName, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Описание:"), 0, 1); layout.Controls.Add(_txtDescription, 1, 1);

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
            _genre.Name = _txtName.Text;
            _genre.Description = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text;

            var result = await _genreService.SaveAsync(_genre);
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
