using LibraryManagement.Application.Abstractions;
using LibraryManagement.Application.Dtos;
using LibraryManagement.Application.Services;
using LibraryManagement.WinForms.Helpers;

namespace LibraryManagement.WinForms.Forms.Auth;

// Форма входа. При успешной аутентификации устанавливает CurrentUserService и закрывается с DialogResult.OK.
public partial class LoginForm : Form
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public LoginForm(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
        InitializeComponent();
    }

    private async void OnLoginClick(object? sender, EventArgs e)
    {
        btnLogin.Enabled = false;
        try
        {
            var request = new LoginRequest
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text
            };

            var result = await _authService.LoginAsync(request);

            if (!Ui.ReportResult(this, result)) return;

            var user = result.Value!;
            _currentUser.SignIn(user.Id, user.Username, user.Role);
            DialogResult = DialogResult.OK;
            Close();
        }
        finally
        {
            btnLogin.Enabled = true;
        }
    }
}
