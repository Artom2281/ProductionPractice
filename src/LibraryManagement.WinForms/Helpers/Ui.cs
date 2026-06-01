using LibraryManagement.Application.Common;

namespace LibraryManagement.WinForms.Helpers;

// Утилитарные методы для типовых UI-сценариев: вывод ошибок, подтверждения, обработка Result.
public static class Ui
{
    // Стандартный label для левой колонки edit-форм. Раньше дублировался в 8 формах.
    public static Label MakeLabel(string text) => new()
    {
        Text = text,
        Anchor = AnchorStyles.Left,
        AutoSize = true,
        Padding = new Padding(0, 6, 0, 0)
    };

    public static void ShowError(IWin32Window? owner, string message, string caption = "Ошибка")
    {
        MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void ShowInfo(IWin32Window? owner, string message, string caption = "Сообщение")
    {
        MessageBox.Show(owner, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static bool Confirm(IWin32Window? owner, string message, string caption = "Подтверждение")
    {
        return MessageBox.Show(owner, message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    // Преобразует Result бизнес-операции в UI-фидбек: ошибки валидации показываются списком,
    // обычные бизнес-ошибки - короткой строкой, успех молчит.
    public static bool ReportResult(IWin32Window? owner, Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                ShowInfo(owner, successMessage);
            }
            return true;
        }

        if (result.ValidationErrors.Count > 0)
        {
            var text = string.Join(Environment.NewLine, result.ValidationErrors.Select(e => "• " + e));
            ShowError(owner, text, "Ошибка валидации");
            return false;
        }

        ShowError(owner, result.Error ?? "Неизвестная ошибка.");
        return false;
    }
}
