using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Controls;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Reports;

// Отчёты с графиками во всех вкладках:
//  - Сводка: цифры + pie chart "Доступно / На руках"
//  - Популярные книги: bar chart топ-10 + таблица
//  - Активные читатели: bar chart топ-10 + таблица
//  - Просроченные: bar chart по дням просрочки + таблица
public class ReportsForm : Form
{
    private readonly IReportService _reportService;

    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly Button _btnRefresh = new() { Text = "Обновить отчёты", Width = 160, Height = 30 };

    private readonly Label _lblStats = new()
    {
        Dock = DockStyle.Top,
        TextAlign = ContentAlignment.TopLeft,
        Padding = new Padding(20),
        Font = new Font("Segoe UI", 11F),
        AutoSize = false,
        Height = 200
    };
    private readonly SimplePieChart _pieAvailability = new() { Dock = DockStyle.Fill, Title = "Распределение экземпляров" };

    private readonly DataGridView _gridPopular = NewGrid();
    private readonly SimpleBarChart _chartPopular = new() { Dock = DockStyle.Top, Height = 280, Title = "Топ-10 популярных книг (по количеству выдач)" };

    private readonly DataGridView _gridReaders = NewGrid();
    private readonly SimpleBarChart _chartReaders = new() { Dock = DockStyle.Top, Height = 280, Title = "Топ-10 активных читателей (по количеству выдач)" };

    private readonly DataGridView _gridOverdue = NewGrid();
    private readonly SimpleBarChart _chartOverdue = new() { Dock = DockStyle.Top, Height = 280, Title = "Просроченные выдачи (по дням просрочки)" };

    public ReportsForm(IReportService reportService)
    {
        _reportService = reportService;
        Text = "Отчёты";
        ClientSize = new Size(1100, 700);

        BuildGridColumns();
        BuildTabs();

        var topPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8) };
        _btnRefresh.Click += async (_, _) => await ReloadAllAsync();
        _btnRefresh.Dock = DockStyle.Left;
        topPanel.Controls.Add(_btnRefresh);

        Controls.Add(_tabs);
        Controls.Add(topPanel);

        Load += async (_, _) => await ReloadAllAsync();
    }

    private static DataGridView NewGrid() => new()
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

    private void BuildGridColumns()
    {
        _gridPopular.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = nameof(PopularBookRow.Title) },
            new DataGridViewTextBoxColumn { HeaderText = "Автор", DataPropertyName = nameof(PopularBookRow.AuthorFullName) },
            new DataGridViewTextBoxColumn { HeaderText = "Выдач", DataPropertyName = nameof(PopularBookRow.LoanCount) }
        );

        _gridReaders.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Билет", DataPropertyName = nameof(ActiveReaderRow.CardNumber) },
            new DataGridViewTextBoxColumn { HeaderText = "ФИО", DataPropertyName = nameof(ActiveReaderRow.FullName) },
            new DataGridViewTextBoxColumn { HeaderText = "Выдач", DataPropertyName = nameof(ActiveReaderRow.LoanCount) }
        );

        _gridOverdue.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "Книга", DataPropertyName = nameof(LoanDto.BookTitle) },
            new DataGridViewTextBoxColumn { HeaderText = "Читатель", DataPropertyName = nameof(LoanDto.ReaderFullName) },
            new DataGridViewTextBoxColumn { HeaderText = "Билет", DataPropertyName = nameof(LoanDto.ReaderCardNumber) },
            new DataGridViewTextBoxColumn { HeaderText = "Срок был", DataPropertyName = nameof(LoanDto.DueDate), DefaultCellStyle = { Format = "dd.MM.yyyy" } },
            new DataGridViewTextBoxColumn { HeaderText = "Дней просрочки", DataPropertyName = nameof(LoanDto.OverdueDays) },
            new DataGridViewTextBoxColumn { HeaderText = "Штраф", DataPropertyName = nameof(LoanDto.FineAmount), DefaultCellStyle = { Format = "0.00 ₽" } }
        );
    }

    private void BuildTabs()
    {
        // Сводка: текстовая статистика сверху + круговая диаграмма снизу
        var tabStats = new TabPage("Сводка");
        var statsContainer = new Panel { Dock = DockStyle.Fill };
        statsContainer.Controls.Add(_pieAvailability);
        statsContainer.Controls.Add(_lblStats);
        tabStats.Controls.Add(statsContainer);

        var tabPopular = new TabPage("Популярные книги");
        var popularContainer = new Panel { Dock = DockStyle.Fill };
        popularContainer.Controls.Add(_gridPopular);
        popularContainer.Controls.Add(_chartPopular);
        tabPopular.Controls.Add(popularContainer);

        var tabReaders = new TabPage("Активные читатели");
        var readersContainer = new Panel { Dock = DockStyle.Fill };
        readersContainer.Controls.Add(_gridReaders);
        readersContainer.Controls.Add(_chartReaders);
        tabReaders.Controls.Add(readersContainer);

        var tabOverdue = new TabPage("Просроченные выдачи");
        var overdueContainer = new Panel { Dock = DockStyle.Fill };
        overdueContainer.Controls.Add(_gridOverdue);
        overdueContainer.Controls.Add(_chartOverdue);
        tabOverdue.Controls.Add(overdueContainer);

        _tabs.TabPages.AddRange(new[] { tabStats, tabPopular, tabReaders, tabOverdue });
    }

    private async Task ReloadAllAsync()
    {
        try
        {
            var stats = await _reportService.GetStatsAsync();
            _lblStats.Text =
                $"Всего наименований книг: {stats.TotalBooks}\n" +
                $"Всего экземпляров: {stats.TotalCopies}\n" +
                $"Доступно к выдаче: {stats.AvailableCopies}\n\n" +
                $"Зарегистрированных читателей: {stats.TotalReaders}\n" +
                $"Активных выдач: {stats.ActiveLoans}\n" +
                $"Просроченных выдач: {stats.OverdueLoans}";

            // Pie: доступные vs на руках. На руках = TotalCopies - AvailableCopies.
            var inHands = Math.Max(0, stats.TotalCopies - stats.AvailableCopies);
            _pieAvailability.SetData(new[]
            {
                new SimplePieChart.PieSlice("Доступно", stats.AvailableCopies, Color.FromArgb(80, 170, 110)),
                new SimplePieChart.PieSlice("На руках", inHands, Color.FromArgb(220, 130, 60)),
            });

            var popular = await _reportService.GetPopularBooksAsync();
            _gridPopular.DataSource = popular.ToList();
            _chartPopular.SetData(popular.Select(p => new SimpleBarChart.BarItem(
                $"{p.Title} ({p.AuthorFullName})", p.LoanCount, Color.FromArgb(70, 130, 200))));

            var readers = await _reportService.GetActiveReadersAsync();
            _gridReaders.DataSource = readers.ToList();
            _chartReaders.SetData(readers.Select(r => new SimpleBarChart.BarItem(
                $"{r.FullName} (билет {r.CardNumber})", r.LoanCount, Color.FromArgb(120, 100, 200))));

            var overdueLoans = await _reportService.GetOverdueLoansAsync();
            // Колонка "Дней просрочки" привязана к computed-полю OverdueDays - всегда положительное.
            // Никакой ручной перезаписи Cells[].Value здесь больше нет.
            _gridOverdue.DataSource = overdueLoans.ToList();
            foreach (DataGridViewRow row in _gridOverdue.Rows)
            {
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }
            _chartOverdue.SetData(overdueLoans.Select(l => new SimpleBarChart.BarItem(
                $"{l.ReaderFullName} → «{l.BookTitle}»", l.OverdueDays, Color.FromArgb(200, 80, 80))));
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, "Не удалось обновить отчёты: " + ex.Message);
        }
    }
}
