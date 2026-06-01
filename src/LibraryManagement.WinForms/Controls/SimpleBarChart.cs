namespace LibraryManagement.WinForms.Controls;

// Минимальный bar chart без сторонних зависимостей. Рисуется через GDI+ Graphics.
// Подходит для отображения top-N значений с лейблами.
public class SimpleBarChart : Control
{
    public record BarItem(string Label, double Value, Color? Color = null);

    private List<BarItem> _items = new();
    private string _title = string.Empty;

    public SimpleBarChart()
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

    public void SetData(IEnumerable<BarItem> items)
    {
        _items = items?.ToList() ?? new List<BarItem>();
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

        if (_items.Count == 0)
        {
            var msg = "Нет данных для отображения.";
            var sz = g.MeasureString(msg, Font);
            g.DrawString(msg, Font, Brushes.Gray,
                new PointF((Width - sz.Width) / 2, (Height - sz.Height) / 2));
            return;
        }

        // Layout - горизонтальные бары: слева лейбл, справа цветная полоса с числом в конце
        int top = 8 + titleHeight;
        int rowHeight = 26;
        int rowSpacing = 4;
        int labelWidth = 220;
        int valueTextWidth = 80;
        int chartLeft = 14 + labelWidth;
        int chartRight = Width - 14 - valueTextWidth;
        int chartWidth = Math.Max(50, chartRight - chartLeft);

        double max = _items.Max(i => Math.Abs(i.Value));
        if (max <= 0) max = 1;

        var defaultBarColor = Color.FromArgb(70, 130, 200);

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            int y = top + i * (rowHeight + rowSpacing);
            if (y > Height) break;

            // Label
            var labelRect = new RectangleF(8, y, labelWidth, rowHeight);
            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            {
                g.DrawString(item.Label, Font, Brushes.Black, labelRect, sf);
            }

            // Bar
            int barWidth = (int)(chartWidth * (Math.Abs(item.Value) / max));
            var barRect = new Rectangle(chartLeft, y + 4, barWidth, rowHeight - 8);
            using var brush = new SolidBrush(item.Color ?? defaultBarColor);
            g.FillRectangle(brush, barRect);

            // Value text
            var valueText = item.Value.ToString("0.##");
            var valRect = new RectangleF(chartLeft + barWidth + 6, y, valueTextWidth, rowHeight);
            using (var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(valueText, Font, Brushes.Black, valRect, sf);
            }
        }

        titleFont.Dispose();
    }
}
