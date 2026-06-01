using LibraryManagement.Domain.Enums;

namespace LibraryManagement.WinForms.Controls;

// Боковая панель навигации MDI. Узлы дерева - разделы приложения.
// При выборе узла поднимает событие SectionSelected с ключом раздела.
public class NavigationPanel : UserControl
{
    public event EventHandler<string>? SectionSelected;

    private readonly Label _header;
    private readonly TreeView _tree;

    // События selection заглушаются до тех пор, пока хост (MainForm) не вызовет EnableSelection().
    // Так пресекаем отложенный AfterSelect от TVM_SELECTITEM, который Windows доставляет из
    // message pump УЖЕ ПОСЛЕ того как Populate отработал. Если бы события сразу шли, hostForm
    // ловил бы их в конструкторе MainForm - до того как родительский handle создан, и WinForms
    // падал с "Дескриптор окна уже существует" при попытке Show() MDI child.
    private bool _selectionEventsEnabled;

    public NavigationPanel()
    {
        Dock = DockStyle.Left;
        Width = 220;
        BackColor = Color.FromArgb(245, 247, 250);
        Padding = new Padding(0);

        _header = new Label
        {
            Text = "Навигация",
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0),
            Font = new Font("Segoe UI Semibold", 11F),
            BackColor = Color.FromArgb(40, 60, 90),
            ForeColor = Color.White
        };

        _tree = new TreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10F),
            FullRowSelect = true,
            HideSelection = false,
            ShowLines = false,
            ShowPlusMinus = false,
            ShowRootLines = false,
            ItemHeight = 28,
            BackColor = Color.FromArgb(245, 247, 250)
        };
        _tree.AfterSelect += OnAfterSelect;

        Controls.Add(_tree);
        Controls.Add(_header);
    }

    // Заполняет дерево разделами в зависимости от роли пользователя.
    // Раздел "Пользователи системы" виден только Администратору и Директору -
    // обычные библиотекари его вообще не видят и не могут зайти.
    public void Populate(UserRole role)
    {
        _tree.BeginUpdate();
        _tree.Nodes.Clear();

        AddNode("books", "Книги");
        AddNode("readers", "Читатели");
        AddNode("loans", "Выдачи и возвраты");
        AddNode("authors", "Авторы");
        AddNode("genres", "Жанры");
        AddNode("publishers", "Издательства");
        AddNode("reports", "Отчёты");

        if (role == UserRole.Administrator || role == UserRole.Director)
        {
            AddNode("users", "Пользователи системы");
        }

        _tree.EndUpdate();
        if (_tree.Nodes.Count > 0) _tree.SelectedNode = _tree.Nodes[0];
    }

    // Включает обработку SectionSelected. Должен вызываться хост-формой только
    // после того как её handle создан (например из Form.Shown).
    public void EnableSelection() => _selectionEventsEnabled = true;

    private void AddNode(string key, string title)
    {
        _tree.Nodes.Add(new TreeNode(title) { Tag = key });
    }

    private void OnAfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (!_selectionEventsEnabled) return;
        if (e.Node?.Tag is string key)
        {
            SectionSelected?.Invoke(this, key);
        }
    }
}
