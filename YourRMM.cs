// ============================================================
// YourRMM.cs  -  AutoElevate Exception Rule Demo Agent
// Target: .NET Framework 4.x   Compiler: csc.exe
//
// COMPILE COMMAND (run in PowerShell ISE):
//   $src = "C:\Demo\YourRMM.cs"
//   $out = "C:\Demo\YourRMM.exe"
//   $refs = "System.Windows.Forms", "System.Drawing"
//   Add-Type -TypeDefinition (Get-Content $src -Raw) `
//       -ReferencedAssemblies $refs `
//       -OutputAssembly $out `
//       -OutputType WindowsApplication
//
// PURPOSE:
//   This is the "trusted parent process" for the AutoElevate
//   exception rule demo. When this exe launches powershell.exe,
//   AutoElevate sees the approved parent and ALLOWS it through -
//   demonstrating that the same binary blocked for an attacker
//   is permitted for legitimate IT tooling.
// ============================================================

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

// ---- Assembly metadata - this is what AutoElevate / PAM logs --------
[assembly: AssemblyTitle("Your RMM Tool")]
[assembly: AssemblyProduct("Your RMM Tool")]
[assembly: AssemblyDescription("AutoElevate Exception Rule Demo")]
[assembly: AssemblyCompany("CyberFOX")]
[assembly: AssemblyVersion("2.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
// ----------------------------------------------------------------------

class YourRMM : Form
{
    // colours - match ThreatDemo palette
    static readonly Color Shell      = Color.FromArgb(18,  18,  24);
    static readonly Color PanelDark  = Color.FromArgb(26,  26,  34);
    static readonly Color PanelMid   = Color.FromArgb(36,  36,  46);
    static readonly Color Success    = Color.FromArgb(30,  185, 100);
    static readonly Color SuccessDk  = Color.FromArgb(20,  100,  55);
    static readonly Color InfoBlue   = Color.FromArgb(60,  130, 220);
    static readonly Color InfoDark   = Color.FromArgb(30,   70, 140);
    static readonly Color TextBright = Color.FromArgb(240, 240, 245);
    static readonly Color TextMid    = Color.FromArgb(185, 185, 200);
    static readonly Color TextDim    = Color.FromArgb(110, 110, 130);

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new YourRMM());
    }

    public YourRMM()
    {
        // ---- form -------------------------------------------------------
        Text            = "Your RMM Tool  |  AutoElevate Exception Demo";
        Size            = new Size(480, 400);
        StartPosition   = FormStartPosition.Manual;
        Location        = PositionBesideTaskbar();
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        BackColor       = Shell;

        // ---- top banner -------------------------------------------------
        Panel banner = new Panel {
            Dock      = DockStyle.Top,
            Height    = 60,
            BackColor = InfoDark
        };

        Label bannerIcon = new Label {
            Text      = "-",
            Font      = new Font("Segoe UI", 20),
            ForeColor = InfoBlue,
            Location  = new Point(14, 10),
            Size      = new Size(40, 40),
            TextAlign = ContentAlignment.MiddleCenter
        };

        Label bannerTitle = new Label {
            Text      = "RMM AGENT - AUTHORIZED PROCESS",
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            Location  = new Point(58, 10),
            Size      = new Size(400, 22),
            TextAlign = ContentAlignment.MiddleLeft
        };

        Label bannerSub = new Label {
            Text      = "CyberFOX  |  AutoElevate Exception Rule Demonstration",
            Font      = new Font("Segoe UI", 8),
            ForeColor = Color.FromArgb(140, 170, 220),
            Location  = new Point(58, 32),
            Size      = new Size(400, 18),
            TextAlign = ContentAlignment.MiddleLeft
        };

        banner.Controls.Add(bannerIcon);
        banner.Controls.Add(bannerTitle);
        banner.Controls.Add(bannerSub);
        Controls.Add(banner);

        // ---- context box ------------------------------------------------
        Panel ctxBox = new Panel {
            Location  = new Point(16, 78),
            Size      = new Size(440, 130),
            BackColor = PanelDark
        };
        ctxBox.Paint += (s, e) => {
            e.Graphics.FillRectangle(new SolidBrush(InfoBlue), 0, 0, 4, ctxBox.Height);
        };

        Label ctxHead = new Label {
            Text      = "WHAT IS DIFFERENT HERE",
            Font      = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = InfoBlue,
            Location  = new Point(16, 10),
            Size      = new Size(410, 16)
        };

        Label ctxText = new Label {
            Text      = "In the previous stage, an attacker called powershell.exe directly.\n" +
                         "AutoElevate blocked it immediately.\n\n" +
                         "Now the exact same binary is being called from this process -\n" +
                         "an approved RMM parent. AutoElevate checks the parent chain,\n" +
                         "recognises it as trusted, and allows execution.",
            Font      = new Font("Segoe UI", 9),
            ForeColor = TextBright,
            Location  = new Point(16, 30),
            Size      = new Size(410, 90),
            AutoSize  = false
        };

        ctxBox.Controls.Add(ctxHead);
        ctxBox.Controls.Add(ctxText);
        Controls.Add(ctxBox);

        // ---- rule display -----------------------------------------------
        Panel ruleBox = new Panel {
            Location  = new Point(16, 222),
            Size      = new Size(440, 64),
            BackColor = PanelDark
        };
        ruleBox.Paint += (s, e) => {
            e.Graphics.FillRectangle(new SolidBrush(Success), 0, 0, 4, ruleBox.Height);
        };

        Label ruleText = new Label {
            Text      = "Attacker  -  powershell.exe   =   BLOCKED\n" +
                         "YourRMM   -  powershell.exe   =   ALLOWED",
            Font      = new Font("Consolas", 10, FontStyle.Bold),
            ForeColor = TextBright,
            Location  = new Point(16, 12),
            Size      = new Size(410, 40),
            AutoSize  = false
        };
        ruleBox.Controls.Add(ruleText);
        Controls.Add(ruleBox);

        // ---- status label -----------------------------------------------
        Label statusLbl = new Label {
            Name      = "statusLbl",
            Text      = "Click the button below to launch PowerShell via this approved agent.",
            Font      = new Font("Segoe UI", 8),
            ForeColor = TextDim,
            Location  = new Point(16, 296),
            Size      = new Size(440, 18),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Controls.Add(statusLbl);

        // ---- buttons ----------------------------------------------------
        Button psBtn = new Button {
            Text      = "-  Launch PowerShell (via RMM)",
            Size      = new Size(220, 32),
            Location  = new Point(16, 320),
            FlatStyle = FlatStyle.Flat,
            BackColor = InfoDark,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        psBtn.FlatAppearance.BorderColor = InfoBlue;
        psBtn.Click += (s, e) => LaunchPS(psBtn, statusLbl);
        Controls.Add(psBtn);

        Button closeBtn = new Button {
            Text      = "Close",
            Size      = new Size(80, 32),
            Location  = new Point(372, 320),
            FlatStyle = FlatStyle.Flat,
            BackColor = PanelMid,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9),
            Cursor    = Cursors.Hand
        };
        closeBtn.FlatAppearance.BorderColor = TextDim;
        closeBtn.Click += (s, e) => Close();
        Controls.Add(closeBtn);
    }

    // ======================================================================
    //  LAUNCH POWERSHELL
    // ======================================================================
    void LaunchPS(Button psBtn, Label statusLbl)
    {
        psBtn.Enabled = false;

        try
        {
            Process.Start(new ProcessStartInfo {
                FileName        = "powershell.exe",
                Arguments       = "-NoProfile -NoExit -Command \"" +
                                  "Write-Host '' -ForegroundColor Green; " +
                                  "Write-Host '  AutoElevate Exception Rule - PowerShell Allowed' -ForegroundColor Green; " +
                                  "Write-Host '  Parent Process: YourRMM.exe (Approved)' -ForegroundColor Cyan; " +
                                  "Write-Host '  Same binary, trusted parent = allowed through.' -ForegroundColor White; " +
                                  "Write-Host ''\"",
                UseShellExecute = true,
                CreateNoWindow  = false
            });

            statusLbl.Text        = "V  PowerShell launched - AutoElevate allowed it via exception rule.";
            statusLbl.ForeColor   = Success;
            psBtn.Text            = "-  PowerShell Launched";
            psBtn.BackColor       = SuccessDk;
        }
        catch (Exception ex)
        {
            // Even from a trusted parent, AE blocked it - or something else went wrong
            statusLbl.Text      = "-  PowerShell was blocked even from RMM - check exception rule config.";
            statusLbl.ForeColor = Color.FromArgb(220, 160, 0);
            psBtn.Enabled       = true;

            ShowError("PowerShell Launch Failed",
                "PowerShell was not allowed even from this approved parent process.\n\n" +
                "Possible causes:\n" +
                "  - The AutoElevate exception rule has not been configured yet\n" +
                "  - The rule is tied to a specific parent EXE name/path that\n" +
                "    doesn't match this demo executable\n\n" +
                "Error detail:\n" + ex.Message);
        }
    }

    // ======================================================================
    //  ERROR DIALOG
    // ======================================================================
    void ShowError(string title, string message)
    {
        Form d = new Form {
            Text            = title,
            Size            = new Size(420, 280),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            BackColor       = Color.FromArgb(26, 26, 34),
            StartPosition   = FormStartPosition.CenterParent
        };

        Panel topBar = new Panel {
            Dock      = DockStyle.Top,
            Height    = 44,
            BackColor = Color.FromArgb(100, 80, 0)
        };
        Label tl = new Label {
            Text      = "-  " + title,
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(220, 160, 0),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        topBar.Controls.Add(tl);
        d.Controls.Add(topBar);

        Label ml = new Label {
            Text      = message,
            Font      = new Font("Segoe UI", 9),
            ForeColor = TextBright,
            Location  = new Point(14, 54),
            Size      = new Size(388, 160),
            AutoSize  = false
        };

        Button ok = new Button {
            Text      = "OK",
            Size      = new Size(80, 28),
            Location  = new Point((d.ClientSize.Width - 80) / 2, 222),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(36, 36, 46),
            ForeColor = Color.White
        };
        ok.FlatAppearance.BorderColor = TextDim;
        ok.Click += (s, e) => d.Close();

        d.Controls.Add(ml);
        d.Controls.Add(ok);
        d.ShowDialog(this);
    }

    // ======================================================================
    //  POSITION: put window to the right of centre screen,
    //  so it's clearly a separate process from ThreatDemo
    // ======================================================================
    Point PositionBesideTaskbar()
    {
        Rectangle sc = Screen.PrimaryScreen.WorkingArea;
        return new Point(
            sc.Right - Width - 30,
            sc.Top   + 100
        );
    }
}
