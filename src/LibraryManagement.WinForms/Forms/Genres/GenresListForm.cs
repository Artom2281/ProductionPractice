using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;

namespace LibraryManagement.WinForms.Forms.Genres;

public class GenresListForm : Form
{
    private readonly IGenreService _genreService;
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
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = Color.White
    };

    public GenresListForm(IGenreService genreService)
    {
        _genreService = genreService;
        Text = "Жанры";
        ClientSize = new Size(1100, 650);

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(GenreDto.Id), Width = 60, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
            new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = nameof(GenreDto.Name), AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells },
            new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = nameof(GenreDto.Description), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
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
            var data = await _genreService.GetAllAsync(_toolbar.SearchBox.Text);
            _grid.DataSource = data.ToList();
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить жанры: " + ex.Message);
        }
    }

    private GenreDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as GenreDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите жанр в списке."); return; }
        await OpenEditAsync(current.Id);
    }

    private async Task OpenEditAsync(int? id)
    {
        var dto = id.HasValue
            ? await _genreService.GetByIdAsync(id.Value) ?? new GenreDto()
            : new GenreDto();

        var form = AppHost.ResolveScopedForm<GenreEditForm>();
        form.SetGenre(dto);
        if (form.ShowDialog(this) == DialogResult.OK) await ReloadAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите жанр в списке."); return; }
        if (!Ui.Confirm(this, $"Удалить жанр «{current.Name}»?")) return;

        var result = await _genreService.DeleteAsync(current.Id);
        if (Ui.ReportResult(this, result, "Жанр удалён."))
        {
            await ReloadAsync();
        }
    }
}
