using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Publishers;

public class PublisherEditForm : Form
{
    private readonly IPublisherService _publisherService;

    private readonly TextBox _txtName = new() { Width = 320 };
    private readonly TextBox _txtCity = new() { Width = 320 };
    private readonly TextBox _txtDescription = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Width = 320,
        Height = 110
    };
    private readonly Button _btnSave = new() { Text = "Сохранить", Width = 100, Height = 30 };
    private readonly Button _btnCancel = new() { Text = "Отмена", Width = 100, Height = 30, DialogResult = DialogResult.Cancel };

    private PublisherDto _publisher = new();

    public PublisherEditForm(IPublisherService publisherService)
    {
        _publisherService = publisherService;

        Text = "Издательство";
        Size = new Size(450, 380);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        AcceptButton = _btnSave;
        CancelButton = _btnCancel;

        BuildLayout();
        _btnSave.Click += async (_, _) => await SaveAsync();
    }

    public void SetPublisher(PublisherDto dto)
    {
        _publisher = dto;
        Text = dto.Id == 0 ? "Новое издательство" : "Редактирование издательства";
        _txtName.Text = dto.Name;
        _txtCity.Text = dto.City ?? string.Empty;
        _txtDescription.Text = dto.Description ?? string.Empty;
    }

    private void BuildLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(15),
            AutoSize = false
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(Ui.MakeLabel("Название:"), 0, 0); layout.Controls.Add(_txtName, 1, 0);
        layout.Controls.Add(Ui.MakeLabel("Город:"), 0, 1); layout.Controls.Add(_txtCity, 1, 1);
        layout.Controls.Add(Ui.MakeLabel("Описание:"), 0, 2); layout.Controls.Add(_txtDescription, 1, 2);

        var buttons = new Panel { Dock = DockStyle.Bottom, Height = 50 };
        _btnSave.Location = new Point(220, 10);
        _btnCancel.Location = new Point(330, 10);
        buttons.Controls.Add(_btnSave);
        buttons.Controls.Add(_btnCancel);

        Controls.Add(layout);
        Controls.Add(buttons);
    }

    private async Task SaveAsync()
    {
        _btnSave.Enabled = false;
        try
        {
            _publisher.Name = _txtName.Text;
            _publisher.City = string.IsNullOrWhiteSpace(_txtCity.Text) ? null : _txtCity.Text;
            _publisher.Description = string.IsNullOrWhiteSpace(_txtDescription.Text) ? null : _txtDescription.Text;

            var result = await _publisherService.SaveAsync(_publisher);
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
