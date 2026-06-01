namespace LibraryManagement.WinForms.Forms.Auth;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private System.Windows.Forms.Label lblTitle;
    private System.Windows.Forms.Label lblUsername;
    private System.Windows.Forms.TextBox txtUsername;
    private System.Windows.Forms.Label lblPassword;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Button btnLogin;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Label lblHint;

    private void InitializeComponent()
    {
        this.lblTitle = new System.Windows.Forms.Label();
        this.lblUsername = new System.Windows.Forms.Label();
        this.txtUsername = new System.Windows.Forms.TextBox();
        this.lblPassword = new System.Windows.Forms.Label();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.btnLogin = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.lblHint = new System.Windows.Forms.Label();
        this.SuspendLayout();

        // lblTitle
        this.lblTitle.AutoSize = false;
        this.lblTitle.Location = new System.Drawing.Point(30, 25);
        this.lblTitle.Size = new System.Drawing.Size(360, 32);
        this.lblTitle.Text = "Система управления библиотекой";
        this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F);
        this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // lblUsername
        this.lblUsername.AutoSize = true;
        this.lblUsername.Location = new System.Drawing.Point(30, 80);
        this.lblUsername.Text = "Логин:";

        // txtUsername
        this.txtUsername.Location = new System.Drawing.Point(30, 100);
        this.txtUsername.Size = new System.Drawing.Size(360, 23);

        // lblPassword
        this.lblPassword.AutoSize = true;
        this.lblPassword.Location = new System.Drawing.Point(30, 135);
        this.lblPassword.Text = "Пароль:";

        // txtPassword
        this.txtPassword.Location = new System.Drawing.Point(30, 155);
        this.txtPassword.Size = new System.Drawing.Size(360, 23);
        this.txtPassword.UseSystemPasswordChar = true;

        // btnLogin
        this.btnLogin.Location = new System.Drawing.Point(220, 200);
        this.btnLogin.Size = new System.Drawing.Size(85, 30);
        this.btnLogin.Text = "Войти";
        this.btnLogin.Click += new System.EventHandler(this.OnLoginClick);

        // btnCancel
        this.btnCancel.Location = new System.Drawing.Point(310, 200);
        this.btnCancel.Size = new System.Drawing.Size(80, 30);
        this.btnCancel.Text = "Отмена";
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

        // lblHint
        this.lblHint.AutoSize = false;
        this.lblHint.Location = new System.Drawing.Point(30, 245);
        this.lblHint.Size = new System.Drawing.Size(360, 18);
        this.lblHint.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Italic);
        this.lblHint.ForeColor = System.Drawing.Color.Gray;
        this.lblHint.Text = "По умолчанию: admin / admin";
        this.lblHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

        // LoginForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.AcceptButton = this.btnLogin;
        this.CancelButton = this.btnCancel;
        this.ClientSize = new System.Drawing.Size(420, 280);
        this.Controls.Add(this.lblTitle);
        this.Controls.Add(this.lblUsername);
        this.Controls.Add(this.txtUsername);
        this.Controls.Add(this.lblPassword);
        this.Controls.Add(this.txtPassword);
        this.Controls.Add(this.btnLogin);
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.lblHint);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "LoginForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Вход в систему";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
