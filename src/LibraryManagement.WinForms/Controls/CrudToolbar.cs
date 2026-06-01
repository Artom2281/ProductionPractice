namespace LibraryManagement.WinForms.Controls;

// Общая верхняя панель для списочных форм: поле поиска + кнопки действий.
// Каждое нажатие/изменение поднимает событие; реализация решает что делать.
public class CrudToolbar : Panel
{
    public TextBox SearchBox { get; }
    public Button AddButton { get; }
    public Button EditButton { get; }
    public Button DeleteButton { get; }
    public Button RefreshButton { get; }
    public FlowLayoutPanel ExtraButtonsPanel { get; }

    public event EventHandler? SearchTextChanged;
    public event EventHandler? AddClicked;
    public event EventHandler? EditClicked;
    public event EventHandler? DeleteClicked;
    public event EventHandler? RefreshClicked;

    public CrudToolbar()
    {
        Dock = DockStyle.Top;
        Height = 50;
        Padding = new Padding(8);
        BackColor = Color.White;

        SearchBox = new TextBox
        {
            PlaceholderText = "Поиск…",
            Width = 280,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Location = new Point(8, 14)
        };
        SearchBox.TextChanged += (s, e) => SearchTextChanged?.Invoke(this, EventArgs.Empty);

        AddButton = MakeButton("Добавить", 300, click: () => AddClicked?.Invoke(this, EventArgs.Empty));
        EditButton = MakeButton("Изменить", 392, click: () => EditClicked?.Invoke(this, EventArgs.Empty));
        DeleteButton = MakeButton("Удалить", 484, click: () => DeleteClicked?.Invoke(this, EventArgs.Empty));
        RefreshButton = MakeButton("Обновить", 576, click: () => RefreshClicked?.Invoke(this, EventArgs.Empty));

        ExtraButtonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Location = new Point(670, 10),
            Margin = new Padding(0)
        };

        Controls.Add(SearchBox);
        Controls.Add(AddButton);
        Controls.Add(EditButton);
        Controls.Add(DeleteButton);
        Controls.Add(RefreshButton);
        Controls.Add(ExtraButtonsPanel);
    }

    private Button MakeButton(string text, int x, Action click)
    {
        var btn = new Button
        {
            Text = text,
            Width = 88,
            Height = 28,
            Location = new Point(x, 11),
            FlatStyle = FlatStyle.System
        };
        btn.Click += (s, e) => click();
        return btn;
    }
}
