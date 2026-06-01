using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;

namespace LibraryManagement.WinForms.Forms.Publishers;

public class PublishersListForm : Form
{
    private readonly IPublisherService _publisherService;
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

    public PublishersListForm(IPublisherService publisherService)
    {
        _publisherService = publisherService;
        Text = "Издательства";
        ClientSize = new Size(1100, 650);

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(PublisherDto.Id), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = nameof(PublisherDto.Name) },
            new DataGridViewTextBoxColumn { HeaderText = "Город", DataPropertyName = nameof(PublisherDto.City) },
            new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = nameof(PublisherDto.Description) }
        );

        _toolbar.AddClicked += async (_, _) => await OpenEditAsync(null);
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();

        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync(); };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        Load += async (_, _) => await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        try
        {
            var data = await _publisherService.GetAllAsync(_toolbar.SearchBox.Text);
            _grid.DataSource = data.ToList();
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить издательства: " + ex.Message);
        }
    }

    private PublisherDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as PublisherDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите издательство в списке."); return; }
        await OpenEditAsync(current.Id);
    }

    private async Task OpenEditAsync(int? id)
    {
        var dto = id.HasValue
            ? await _publisherService.GetByIdAsync(id.Value) ?? new PublisherDto()
            : new PublisherDto();

        var form = AppHost.ResolveScopedForm<PublisherEditForm>();
        form.SetPublisher(dto);
        if (form.ShowDialog(this) == DialogResult.OK) await ReloadAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите издательство в списке."); return; }
        if (!Ui.Confirm(this, $"Удалить издательство «{current.Name}»? Связанные книги останутся, но потеряют ссылку на издательство.")) return;

        var result = await _publisherService.DeleteAsync(current.Id);
        if (Ui.ReportResult(this, result, "Издательство удалено."))
        {
            await ReloadAsync();
        }
    }
}
