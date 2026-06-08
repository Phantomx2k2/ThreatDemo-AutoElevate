// ============================================================
// DemoConsole.cs  -  CyberFOX AutoElevate Demo Console
// Target: .NET Framework 4.x   Compiler: csc.exe
//
// FOLDER LAYOUT (bin folder):
//   DemoConsole.exe
//   ThreatDemo.exe
//   YourRMM.exe
//   Support Files\
//       CF_Logo_AutoElevate_White_081425.png
//       Video\
//           JIT Demo.mp4
//           Mac Demo.mp4
// ============================================================

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

[assembly: AssemblyTitle("Demo Console")]
[assembly: AssemblyProduct("Demo Console")]
[assembly: AssemblyDescription("AutoElevate Demo Launcher - CyberFOX")]
[assembly: AssemblyCompany("CyberFOX")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// ======================================================================
//  CYBERFOX BRAND PALETTE
// ======================================================================
static class CF
{
    public static readonly Color Blue       = Color.FromArgb(  0, 174, 239);  // #00AEEF
    public static readonly Color DarkBlue   = Color.FromArgb(  0,  99, 190);  // #0063BE
    public static readonly Color LightBlue  = Color.FromArgb(143, 202, 231);  // #8FCAE7
    public static readonly Color Orange     = Color.FromArgb(236, 100,  36);  // #EC6424
    public static readonly Color Black      = Color.FromArgb(  0,   0,   0);
    public static readonly Color Charcoal   = Color.FromArgb( 32,  32,  32);  // #202020
    public static readonly Color Gray       = Color.FromArgb(102, 102, 102);  // #666666
    public static readonly Color White      = Color.FromArgb(255, 255, 255);
    public static readonly Color PanelDark  = Color.FromArgb( 26,  26,  26);
    public static readonly Color PanelMid   = Color.FromArgb( 38,  38,  38);
    public static readonly Color Border     = Color.FromArgb( 50,  50,  50);
    public static readonly Color TextDim    = Color.FromArgb( 90,  90,  90);
    public static readonly Color LiveGreen  = Color.FromArgb(  0, 220,  80);
}

// ======================================================================
//  MAIN FORM
// ======================================================================
class DemoConsole : Form
{
    string _appDir;
    string _desktop;
    string _supportFiles;
    string _videoDir;
    string _threatDemoExe;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new DemoConsole());
    }

    public DemoConsole()
    {
        // ---- resolve paths -----------------------------------------------
        _appDir       = AppDomain.CurrentDomain.BaseDirectory;
        _desktop      = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        _supportFiles = Path.Combine(_appDir, "Support Files");
        _videoDir     = Path.Combine(_supportFiles, "Video");
        _threatDemoExe = Path.Combine(_appDir, "ThreatDemo.exe");

        // ---- form setup --------------------------------------------------
        Text            = "AE Demo";
        Size            = new Size(260, 620);
        MinimumSize     = new Size(260, 620);
        MaximumSize     = new Size(260, 620);
        StartPosition   = FormStartPosition.Manual;
        Location        = PositionRight();
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = CF.Charcoal;
        TopMost         = true;

        BuildUI();
    }

    // ======================================================================
    //  POSITION  - right side of screen, near top
    // ======================================================================
    Point PositionRight()
    {
        Rectangle sc = Screen.PrimaryScreen.WorkingArea;
        return new Point(sc.Right - Width - 10, sc.Top + 40);
    }

    // ======================================================================
    //  BUILD UI
    // ======================================================================
    void BuildUI()
    {
        int y = 0;

        // ---- HEADER ------------------------------------------------------
        Panel header = new Panel {
            Location  = new Point(0, 0),
            Size      = new Size(254, 72),
            BackColor = CF.Blue
        };

        // logo image
        string logoPath = Path.Combine(_supportFiles, "CF_Logo_AutoElevate_White_081425.png");
        if (File.Exists(logoPath))
        {
            try
            {
                PictureBox logo = new PictureBox {
                    Image    = Image.FromFile(logoPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(12, 8),
                    Size     = new Size(200, 38),
                    BackColor = Color.Transparent
                };
                header.Controls.Add(logo);
            }
            catch { AddLogoFallback(header); }
        }
        else
        {
            AddLogoFallback(header);
        }

        // sub label
        Label subLbl = new Label {
            Text      = "AutoElevate Demo Console",
            Font      = new Font("Segoe UI", 7, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 235, 255),
            Location  = new Point(12, 52),
            Size      = new Size(260, 16),
            TextAlign = ContentAlignment.MiddleLeft
        };
        header.Controls.Add(subLbl);
        Controls.Add(header);
        y = 72;

        // ---- DEMO VIDEOS -------------------------------------------------
        y = AddSection(y, "DEMO VIDEOS");
        y = AddButton(y, "JIT Login Demo",      CF.DarkBlue,  CF.LightBlue, "J", () => LaunchVideo("JIT Demo.mp4"));
        y = AddButton(y, "Mac Agent Demo",      CF.DarkBlue,  CF.LightBlue, "M", () => LaunchVideo("Mac Demo.mp4"));
        y = AddButtonDisabled(y, "JIT Domain Login", "SOON");

        // ---- SIMULATIONS -------------------------------------------------
        y = AddDivider(y);
        y = AddSection(y, "SIMULATIONS");
        y = AddButton(y, "Launch Malware Sim",  CF.Orange,    CF.White,     "!", () => LaunchExe(_threatDemoExe));

        // ---- AUTOELEVATE CONTROLS ----------------------------------------
        y = AddDivider(y);
        y = AddSection(y, "AUTOELEVATE");
        y = AddButton(y, "Technician Mode",     CF.Blue,      CF.White,     "AE", () => LaunchExe(
            @"C:\Program Files (x86)\AutoElevate\AETechnicianModeLauncher.exe"));

        // ---- UAC TRIGGERS ------------------------------------------------
        y = AddDivider(y);
        y = AddSection(y, "UAC TRIGGERS");
        y = AddButton(y, "TextEdit (Deny)",  CF.LightBlue,      CF.White,     "TX", () => LaunchDesktop("TESetup.exe"));
        y = AddButton(y, "VLC Player (Approve)",CF.LightBlue,      CF.White,     "RD", () => LaunchDesktop("VLC Install.exe"));
        y = AddButton(y, "Java Setup (Rule)",        CF.LightBlue,      CF.White,     "XL", () => LaunchDesktop("Java.exe"));
        y = AddButton(y, "PowerShell as Admin", CF.Orange,  CF.LightBlue, "PS", () => LaunchAsAdmin("powershell.exe", ""));

        // ---- EXIT --------------------------------------------------------
        y = AddDivider(y);
        y = AddExitButton(y);

        // ---- FOOTER ------------------------------------------------------
        Panel footer = new Panel {
            Location  = new Point(0, y + 4),
            Size      = new Size(254, 22),
            BackColor = CF.Black
        };
        Label footerLbl = new Label {
            Text      = "CyberFOX  |  AutoElevate Demo System",
            Font      = new Font("Segoe UI", 7),
            ForeColor = CF.TextDim,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        footer.Controls.Add(footerLbl);
        Controls.Add(footer);
    }

    // ======================================================================
    //  LOGO FALLBACK  (if image file not found)
    // ======================================================================
    void AddLogoFallback(Panel header)
    {
        Label fallback = new Label {
            Text      = "CyberFOX",
            Font      = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = CF.White,
            Location  = new Point(10, 14),
            Size      = new Size(200, 34),
            TextAlign = ContentAlignment.MiddleLeft
        };
        header.Controls.Add(fallback);
    }

    // ======================================================================
    //  SECTION LABEL
    // ======================================================================
    int AddSection(int y, string text)
    {
        Label lbl = new Label {
            Text      = text,
            Font      = new Font("Segoe UI", 7, FontStyle.Bold),
            ForeColor = CF.TextDim,
            Location  = new Point(12, y + 6),
            Size      = new Size(260, 16),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(lbl);
        return y + 24;
    }

    // ======================================================================
    //  STANDARD BUTTON
    // ======================================================================
    int AddButton(int y, string label, Color iconBg, Color textCol, string iconText, Action onClick)
    {
        Panel row = new Panel {
            Location  = new Point(10, y),
            Size      = new Size(234, 32),
            BackColor = CF.PanelMid,
            Cursor    = Cursors.Hand
        };
        row.Paint += (s, e) => {
            e.Graphics.FillRectangle(new SolidBrush(iconBg), 0, 0, 3, 32);
        };

        Panel iconBox = new Panel {
            Location  = new Point(8, 5),
            Size      = new Size(22, 22),
            BackColor = iconBg
        };
        Label iconLbl = new Label {
            Text      = iconText,
            Font      = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = CF.White,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        iconBox.Controls.Add(iconLbl);

        Label textLbl = new Label {
            Text      = label,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = textCol,
            Location  = new Point(36, 6),
            Size      = new Size(220, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        row.Controls.Add(iconBox);
        row.Controls.Add(textLbl);

        // click on any part of the row
        Action fire = onClick;
        row.Click      += (s, e) => fire();
        iconBox.Click  += (s, e) => fire();
        iconLbl.Click  += (s, e) => fire();
        textLbl.Click  += (s, e) => fire();

        // hover effect
        row.MouseEnter  += (s, e) => row.BackColor = CF.PanelDark;
        row.MouseLeave  += (s, e) => row.BackColor = CF.PanelMid;
        textLbl.MouseEnter += (s, e) => row.BackColor = CF.PanelDark;
        textLbl.MouseLeave += (s, e) => row.BackColor = CF.PanelMid;

        Controls.Add(row);
        return y + 36;
    }

    // ======================================================================
    //  DISABLED BUTTON  (coming soon)
    // ======================================================================
    int AddButtonDisabled(int y, string label, string badge)
    {
        Panel row = new Panel {
            Location  = new Point(10, y),
            Size      = new Size(234, 32),
            BackColor = CF.PanelDark
        };

        Label textLbl = new Label {
            Text      = label,
            Font      = new Font("Segoe UI", 9),
            ForeColor = CF.TextDim,
            Location  = new Point(36, 6),
            Size      = new Size(130, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        Label badgeLbl = new Label {
            Text      = badge,
            Font      = new Font("Segoe UI", 7, FontStyle.Bold),
            ForeColor = CF.TextDim,
            Location  = new Point(160, 9),
            Size      = new Size(80, 14),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = CF.Border
        };

        row.Controls.Add(textLbl);
        row.Controls.Add(badgeLbl);
        Controls.Add(row);
        return y + 36;
    }

    // ======================================================================
    //  EXIT BUTTON
    // ======================================================================
    int AddExitButton(int y)
    {
        Button exitBtn = new Button {
            Text      = "Close Console",
            Size      = new Size(234, 32),
            Location  = new Point(10, y),
            FlatStyle = FlatStyle.Flat,
            BackColor = CF.Orange,
            ForeColor = CF.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        exitBtn.FlatAppearance.BorderColor = CF.Border;
        exitBtn.Click += (s, e) => Application.Exit();
        Controls.Add(exitBtn);
        return y + 36;
    }

    // ======================================================================
    //  DIVIDER
    // ======================================================================
    int AddDivider(int y)
    {
        Panel div = new Panel {
            Location  = new Point(10, y + 4),
            Size      = new Size(234, 1),
            BackColor = CF.Border
        };
        Controls.Add(div);
        return y + 10;
    }

    // ======================================================================
    //  LAUNCH ACTIONS
    // ======================================================================
     
     void LaunchVideo(string filename)
    {
        string path = Path.Combine(_videoDir, filename);
        if (!File.Exists(path))
        {
            ShowError("Video Not Found",
                "Could not find:\n" + path +
                "\n\nMake sure the video file is in:\nbin\\Support Files\\Video\\");
            return;
        }
        try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
        catch { }
    }

    void LaunchExe(string path)
    {
        if (!File.Exists(path))
        {
            ShowError("File Not Found", "Could not find:\n" + path);
            return;
        }
        try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
        catch { }
    }

    void LaunchDesktop(string filename)
    {
        string path = Path.Combine(_desktop, filename);
        if (!File.Exists(path))
        {
            ShowError("File Not Found",
                "Could not find:\n" + filename +
                "\n\nMake sure the file is on the Desktop:\n" + _desktop);
            return;
        }
        try { Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true }); }
        catch { }
    }

    void LaunchAsAdmin(string exe, string args)
    {
        try
        {
            Process.Start(new ProcessStartInfo {
                FileName        = exe,
                Arguments       = args,
                UseShellExecute = true,
                Verb            = "runas"
            });
        }
        catch { }
    }

    // ======================================================================
    //  ERROR DIALOG
    // ======================================================================
    void ShowError(string title, string message)
    {
        Form d = new Form {
            Text            = title,
            Size            = new Size(380, 220),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            MinimizeBox     = false,
            BackColor       = CF.Charcoal,
            StartPosition   = FormStartPosition.CenterParent
        };

        Panel topBar = new Panel {
            Dock      = DockStyle.Top,
            Height    = 40,
            BackColor = CF.Orange
        };
        Label titleLbl = new Label {
            Text      = "  " + title,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = CF.White,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        topBar.Controls.Add(titleLbl);
        d.Controls.Add(topBar);

        Label msgLbl = new Label {
            Text      = message,
            Font      = new Font("Segoe UI", 9),
            ForeColor = CF.White,
            Location  = new Point(14, 50),
            Size      = new Size(348, 110),
            AutoSize  = false
        };

        Button okBtn = new Button {
            Text      = "OK",
            Size      = new Size(80, 28),
            Location  = new Point((d.ClientSize.Width - 80) / 2, 168),
            FlatStyle = FlatStyle.Flat,
            BackColor = CF.PanelMid,
            ForeColor = CF.White,
            Cursor    = Cursors.Hand
        };
        okBtn.FlatAppearance.BorderColor = CF.Border;
        okBtn.Click += (s, e) => d.Close();

        d.Controls.Add(msgLbl);
        d.Controls.Add(okBtn);
        d.ShowDialog(this);
    }
}
