using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;

namespace LibraryManagement.WinForms.Forms.Readers;

public class ReadersListForm : Form
{
    private readonly IReaderService _readerService;
    private readonly CrudToolbar _toolbar = new();
    private readonly CheckBox _chkIncludeBlocked = new() { Text = "Включая заблокированных", AutoSize = true, Checked = true };
    private readonly Button _btnToggleBlock = new() { Text = "Заблокировать / разблокировать", Width = 220, Height = 28 };

    private readonly ContextMenuStrip _ctxMenu = new();
    private readonly ToolStripMenuItem _ctxEdit = new("Изменить");
    private readonly ToolStripMenuItem _ctxToggleBlock = new("Заблокировать / разблокировать");
    private readonly ToolStripMenuItem _ctxDelete = new("Удалить");

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

    public ReadersListForm(IReaderService readerService)
    {
        _readerService = readerService;
        Text = "Читатели";
        ClientSize = new Size(1100, 650);

        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(ReaderDto.Id), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "№ билета", DataPropertyName = nameof(ReaderDto.CardNumber) },
            new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = nameof(ReaderDto.LastName) },
            new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = nameof(ReaderDto.FirstName) },
            new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = nameof(ReaderDto.MiddleName) },
            new DataGridViewTextBoxColumn { HeaderText = "Телефон", DataPropertyName = nameof(ReaderDto.Phone) },
            new DataGridViewTextBoxColumn { HeaderText = "Email", DataPropertyName = nameof(ReaderDto.Email) },
            new DataGridViewCheckBoxColumn { HeaderText = "Заблокирован", DataPropertyName = nameof(ReaderDto.IsBlocked) }
        );

        _chkIncludeBlocked.CheckedChanged += async (_, _) => await ReloadAsync();
        _btnToggleBlock.Click += async (_, _) => await ToggleBlockSelectedAsync();
        _toolbar.ExtraButtonsPanel.Controls.AddRange(new Control[] { _chkIncludeBlocked, _btnToggleBlock });

        _toolbar.AddClicked += async (_, _) => await OpenEditAsync(null);
        _toolbar.EditClicked += async (_, _) => await EditSelectedAsync();
        _toolbar.DeleteClicked += async (_, _) => await DeleteSelectedAsync();
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();

        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditSelectedAsync(); };

        // Контекстное меню по правому клику на строке - удобный способ заблокировать читателя
        // прямо из списка, не открывая форму редактирования
        _ctxEdit.Click += async (_, _) => await EditSelectedAsync();
        _ctxToggleBlock.Click += async (_, _) => await ToggleBlockSelectedAsync();
        _ctxDelete.Click += async (_, _) => await DeleteSelectedAsync();
        _ctxMenu.Items.AddRange(new ToolStripItem[] { _ctxEdit, _ctxToggleBlock, _ctxDelete });
        _grid.ContextMenuStrip = _ctxMenu;

        // При правом клике автоматически выделяем строку под курсором,
        // чтобы пункты меню работали с тем читателем, по которому кликнули
        _grid.CellMouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                _grid.ClearSelection();
                _grid.Rows[e.RowIndex].Selected = true;
                _grid.CurrentCell = _grid.Rows[e.RowIndex].Cells[Math.Max(0, e.ColumnIndex)];
            }
        };

        // Динамическая надпись пункта меню "Заблокировать" / "Разблокировать"
        _ctxMenu.Opening += (_, e) =>
        {
            var sel = GetSelected();
            if (sel is null) { e.Cancel = true; return; }
            _ctxToggleBlock.Text = sel.IsBlocked ? "Разблокировать" : "Заблокировать";
        };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        Load += async (_, _) => await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        try
        {
            var data = await _readerService.GetAllAsync(_toolbar.SearchBox.Text, _chkIncludeBlocked.Checked);
            _grid.DataSource = data.ToList();

            // Подсветка заблокированных
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is ReaderDto r && r.IsBlocked)
                {
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить читателей: " + ex.Message);
        }
    }

    private ReaderDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as ReaderDto;

    private async Task EditSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите читателя в списке."); return; }
        await OpenEditAsync(current.Id);
    }

    private async Task OpenEditAsync(int? id)
    {
        var dto = id.HasValue
            ? await _readerService.GetByIdAsync(id.Value) ?? new ReaderDto()
            : new ReaderDto { RegistrationDate = DateTime.UtcNow };

        var form = AppHost.ResolveScopedForm<ReaderEditForm>();
        form.SetReader(dto);
        if (form.ShowDialog(this) == DialogResult.OK) await ReloadAsync();
    }

    private async Task DeleteSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите читателя в списке."); return; }
        if (!Ui.Confirm(this, $"Удалить читателя «{current.FullName}»?")) return;

        var result = await _readerService.DeleteAsync(current.Id);
        if (Ui.ReportResult(this, result, "Читатель удалён."))
        {
            await ReloadAsync();
        }
    }

    private async Task ToggleBlockSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите читателя в списке."); return; }
        var newState = !current.IsBlocked;
        var verb = newState ? "Заблокировать" : "Разблокировать";
        if (!Ui.Confirm(this, $"{verb} читателя «{current.FullName}»?")) return;

        var result = await _readerService.SetBlockedAsync(current.Id, newState);
        if (Ui.ReportResult(this, result))
        {
            await ReloadAsync();
        }
    }
}
