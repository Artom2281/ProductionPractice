using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Enums;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;

namespace LibraryManagement.WinForms.Forms.Loans;

// Список выдач с фильтрами "только активные" / "только просроченные".
// Кнопки в этом разделе нестандартные: вместо Add/Edit - "Выдать книгу" / "Принять возврат" / Удалить (записи).
public class LoansListForm : Form
{
    private readonly ILoanService _loanService;

    private readonly CrudToolbar _toolbar = new();
    private readonly CheckBox _chkOnlyActive = new() { Text = "Только активные", AutoSize = true };
    private readonly CheckBox _chkOnlyOverdue = new() { Text = "Только просроченные", AutoSize = true };
    private readonly Button _btnIssue = new() { Text = "Выдать книгу", Width = 130, Height = 28 };
    private readonly Button _btnReturn = new() { Text = "Принять возврат", Width = 140, Height = 28 };

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

    public LoansListForm(ILoanService loanService)
    {
        _loanService = loanService;
        Text = "Выдачи и возвраты";
        ClientSize = new Size(1100, 650);

        BuildGridColumns();
        BuildExtras();

        _toolbar.AddButton.Visible = false;
        _toolbar.EditButton.Visible = false;
        _toolbar.DeleteButton.Visible = false;
        _toolbar.RefreshClicked += async (_, _) => await ReloadAsync();
        _toolbar.SearchTextChanged += async (_, _) => await ReloadAsync();

        _grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await OpenReturnForSelectedAsync(); };

        Controls.Add(_grid);
        Controls.Add(_toolbar);
        Load += async (_, _) =>
        {
            await _loanService.RefreshOverdueStatusesAsync();
            await ReloadAsync();
        };
    }

    private void BuildGridColumns()
    {
        _grid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "ID", DataPropertyName = nameof(LoanDto.Id), Width = 60 },
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = nameof(LoanDto.BookTitle) },
            new DataGridViewTextBoxColumn { HeaderText = "Читатель", DataPropertyName = nameof(LoanDto.ReaderFullName) },
            new DataGridViewTextBoxColumn { HeaderText = "Билет", DataPropertyName = nameof(LoanDto.ReaderCardNumber) },
            new DataGridViewTextBoxColumn { HeaderText = "Выдана", DataPropertyName = nameof(LoanDto.LoanDate), DefaultCellStyle = { Format = "dd.MM.yyyy" } },
            new DataGridViewTextBoxColumn { HeaderText = "Срок", DataPropertyName = nameof(LoanDto.DueDate), DefaultCellStyle = { Format = "dd.MM.yyyy" } },
            new DataGridViewTextBoxColumn { HeaderText = "Возвращена", DataPropertyName = nameof(LoanDto.ReturnedAt), DefaultCellStyle = { Format = "dd.MM.yyyy" } },
            new DataGridViewTextBoxColumn { HeaderText = "Статус", DataPropertyName = nameof(LoanDto.StatusDisplay) },
            new DataGridViewTextBoxColumn { HeaderText = "Штраф", DataPropertyName = nameof(LoanDto.FineAmount), DefaultCellStyle = { Format = "0.00" } },
            new DataGridViewTextBoxColumn { HeaderText = "Кто выдал", DataPropertyName = nameof(LoanDto.IssuedByUserName) }
        );
    }

    private void BuildExtras()
    {
        _btnIssue.Click += async (_, _) => await OpenIssueAsync();
        _btnReturn.Click += async (_, _) => await OpenReturnForSelectedAsync();
        _chkOnlyActive.CheckedChanged += async (_, _) =>
        {
            if (_chkOnlyActive.Checked) _chkOnlyOverdue.Checked = false;
            await ReloadAsync();
        };
        _chkOnlyOverdue.CheckedChanged += async (_, _) =>
        {
            if (_chkOnlyOverdue.Checked) _chkOnlyActive.Checked = false;
            await ReloadAsync();
        };

        _toolbar.ExtraButtonsPanel.Controls.AddRange(new Control[]
        {
            _btnIssue, _btnReturn, _chkOnlyActive, _chkOnlyOverdue
        });
    }

    private async Task ReloadAsync()
    {
        try
        {
            var data = await _loanService.GetAllAsync(
                search: _toolbar.SearchBox.Text,
                onlyActive: _chkOnlyActive.Checked,
                onlyOverdue: _chkOnlyOverdue.Checked);
            _grid.DataSource = data.ToList();

            // Подсветка статусов
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is LoanDto l)
                {
                    if (l.Status == LoanStatus.Overdue) row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    else if (l.Status == LoanStatus.Returned) row.DefaultCellStyle.ForeColor = Color.Gray;
                }
            }
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось загрузить выдачи: " + ex.Message);
        }
    }

    private LoanDto? GetSelected() => _grid.CurrentRow?.DataBoundItem as LoanDto;

    private async Task OpenIssueAsync()
    {
        var form = AppHost.ResolveScopedForm<IssueLoanForm>();
        if (form.ShowDialog(this) == DialogResult.OK) await ReloadAsync();
    }

    private async Task OpenReturnForSelectedAsync()
    {
        var current = GetSelected();
        if (current is null) { Ui.ShowInfo(this, "Выберите выдачу в списке."); return; }
        if (current.ReturnedAt != null) { Ui.ShowInfo(this, "Эта книга уже возвращена."); return; }

        var form = AppHost.ResolveScopedForm<ReturnLoanForm>();
        form.SetLoan(current);
        if (form.ShowDialog(this) == DialogResult.OK) await ReloadAsync();
    }
}
