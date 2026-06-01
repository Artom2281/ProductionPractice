namespace LibraryManagement.WinForms.Controls;

// Минимальный pie chart - круговая диаграмма для нескольких именованных секторов.
public class SimplePieChart : Control
{
    public record PieSlice(string Label, double Value, Color Color);

    private List<PieSlice> _slices = new();
    private string _title = string.Empty;

    public SimplePieChart()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9F);
    }

    [System.ComponentModel.DefaultValue("")]
    public string Title
    {
        get => _title;
        set { _title = value ?? string.Empty; Invalidate(); }
    }

    public void SetData(IEnumerable<PieSlice> slices)
    {
        _slices = slices?.ToList() ?? new List<PieSlice>();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        if (Width <= 0 || Height <= 0) return;

        var titleFont = new Font(Font.FontFamily, 11F, FontStyle.Bold);
        int titleHeight = 0;
        if (!string.IsNullOrEmpty(_title))
        {
            titleHeight = (int)g.MeasureString(_title, titleFont).Height + 8;
            g.DrawString(_title, titleFont, Brushes.Black, new PointF(10, 6));
        }

        var total = _slices.Sum(s => Math.Max(0, s.Value));
        if (total <= 0)
        {
            var msg = "Нет данных для отображения.";
            var sz = g.MeasureString(msg, Font);
            g.DrawString(msg, Font, Brushes.Gray, new PointF((Width - sz.Width) / 2, (Height - sz.Height) / 2));
            titleFont.Dispose();
            return;
        }

        // Слева - круг, справа - легенда
        int chartArea = Math.Min(Width / 2, Height - titleHeight - 20);
        int diameter = Math.Min(chartArea, 260);
        int chartX = 16;
        int chartY = titleHeight + 16;
        var pieRect = new Rectangle(chartX, chartY, diameter, diameter);

        float startAngle = -90; // 12 часов
        foreach (var slice in _slices)
        {
            if (slice.Value <= 0) continue;
            float sweep = (float)(360.0 * slice.Value / total);
            using var brush = new SolidBrush(slice.Color);
            g.FillPie(brush, pieRect, startAngle, sweep);
            using var pen = new Pen(Color.White, 2);
            g.DrawPie(pen, pieRect, startAngle, sweep);
            startAngle += sweep;
        }

        // Легенда справа от круга
        int legendLeft = chartX + diameter + 24;
        int legendTop = titleHeight + 16;
        int rowH = 22;
        int i = 0;
        foreach (var slice in _slices)
        {
            int y = legendTop + i * rowH;
            if (y + rowH > Height) break;
            using (var brush = new SolidBrush(slice.Color))
            {
                g.FillRectangle(brush, legendLeft, y + 4, 14, 14);
            }
            g.DrawRectangle(Pens.Gray, legendLeft, y + 4, 14, 14);

            var pct = 100.0 * slice.Value / total;
            var label = $"{slice.Label}: {slice.Value:0.##} ({pct:0.#}%)";
            g.DrawString(label, Font, Brushes.Black, new PointF(legendLeft + 22, y + 3));
            i++;
        }

        titleFont.Dispose();
    }
}
