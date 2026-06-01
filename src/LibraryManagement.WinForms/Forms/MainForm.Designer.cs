namespace LibraryManagement.WinForms.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private System.Windows.Forms.MenuStrip menuStrip;
    private System.Windows.Forms.ToolStripMenuItem mnuFile;
    private System.Windows.Forms.ToolStripMenuItem mnuLogout;
    private System.Windows.Forms.ToolStripMenuItem mnuExit;
    private System.Windows.Forms.ToolStripMenuItem mnuWindow;
    private System.Windows.Forms.ToolStripMenuItem mnuCascade;
    private System.Windows.Forms.ToolStripMenuItem mnuTileH;
    private System.Windows.Forms.ToolStripMenuItem mnuTileV;
    private System.Windows.Forms.ToolStripMenuItem mnuCloseAll;
    private System.Windows.Forms.ToolStripMenuItem mnuHelp;
    private System.Windows.Forms.ToolStripMenuItem mnuAbout;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel lblUser;
    private System.Windows.Forms.ToolStripStatusLabel lblSpacer;
    private System.Windows.Forms.ToolStripStatusLabel lblDatabase;

    private void InitializeComponent()
    {
        this.menuStrip = new System.Windows.Forms.MenuStrip();
        this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuLogout = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuWindow = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuCascade = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuTileH = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuTileV = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuCloseAll = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
        this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.lblUser = new System.Windows.Forms.ToolStripStatusLabel();
        this.lblSpacer = new System.Windows.Forms.ToolStripStatusLabel();
        this.lblDatabase = new System.Windows.Forms.ToolStripStatusLabel();
        this.menuStrip.SuspendLayout();
        this.statusStrip.SuspendLayout();
        this.SuspendLayout();

        // menuStrip
        this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this.mnuFile, this.mnuWindow, this.mnuHelp
        });
        this.menuStrip.MdiWindowListItem = this.mnuWindow;
        this.menuStrip.Location = new System.Drawing.Point(0, 0);

        // mnuFile
        this.mnuFile.Text = "Файл";
        this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this.mnuLogout, this.mnuExit
        });

        this.mnuLogout.Text = "Выход из учётной записи";
        this.mnuLogout.Click += new System.EventHandler(this.OnLogoutClick);

        this.mnuExit.Text = "Закрыть программу";
        this.mnuExit.ShortcutKeys = System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4;
        this.mnuExit.Click += new System.EventHandler(this.OnExitClick);

        // mnuWindow
        this.mnuWindow.Text = "Окна";
        this.mnuWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this.mnuCascade, this.mnuTileH, this.mnuTileV, this.mnuCloseAll
        });
        this.mnuCascade.Text = "Каскадом";
        this.mnuCascade.Click += new System.EventHandler(this.OnCascadeClick);
        this.mnuTileH.Text = "Горизонтально";
        this.mnuTileH.Click += new System.EventHandler(this.OnTileHClick);
        this.mnuTileV.Text = "Вертикально";
        this.mnuTileV.Click += new System.EventHandler(this.OnTileVClick);
        this.mnuCloseAll.Text = "Закрыть все";
        this.mnuCloseAll.Click += new System.EventHandler(this.OnCloseAllClick);

        // mnuHelp
        this.mnuHelp.Text = "Справка";
        this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.mnuAbout });
        this.mnuAbout.Text = "О программе";
        this.mnuAbout.Click += new System.EventHandler(this.OnAboutClick);

        // statusStrip
        this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
        {
            this.lblUser, this.lblSpacer, this.lblDatabase
        });
        this.lblUser.Text = "Пользователь:";
        this.lblSpacer.Spring = true;
        this.lblSpacer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
        this.lblDatabase.Text = "БД:";

        // MainForm
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1100, 700);
        this.Controls.Add(this.statusStrip);
        this.Controls.Add(this.menuStrip);
        this.IsMdiContainer = true;
        this.MainMenuStrip = this.menuStrip;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Система управления библиотекой";
        this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.menuStrip.ResumeLayout(false);
        this.menuStrip.PerformLayout();
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
