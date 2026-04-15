// ============================================================
// AttackerConsole.cs  -  AI Attacker Terminal Simulation
// Target: .NET Framework 4.x   Compiler: csc.exe
//
// DROP THIS FILE in the same folder as ThreatDemo.cs
// Add to build.ps1:  Compile 'AttackerConsole'  (not needed -
// this class is compiled INTO ThreatDemo.cs via the same csc
// call - just add AttackerConsole.cs to the compile line)
//
// In build.ps1 change the ThreatDemo compile line to:
//   & $csc /nologo /target:winexe /out:$outFile `
//     /r:System.Windows.Forms.dll /r:System.Drawing.dll `
//     $srcFile (Join-Path $src 'AttackerConsole.cs')
// ============================================================

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;

// ======================================================================
//  MATRIX COLOUR PALETTE
// ======================================================================
static class MatrixPal
{
    public static readonly Color Black       = Color.FromArgb(  0,   0,   0);
    public static readonly Color MatrixGreen = Color.FromArgb(  0, 255,  70);
    public static readonly Color DimGreen    = Color.FromArgb(  0, 140,  40);
    public static readonly Color HotGreen    = Color.FromArgb(180, 255, 100);
    public static readonly Color BrightWhite = Color.FromArgb(255, 255, 255);
    public static readonly Color RedAlert    = Color.FromArgb(220,  50,  50);
    public static readonly Color DimRed      = Color.FromArgb(140,  25,  25);
    public static readonly Color Amber       = Color.FromArgb(255, 180,   0);
}

// ======================================================================
//  TERMINAL LINE  - one line of output with its own color
// ======================================================================
class TermLine
{
    public string Text;
    public Color  Col;
    public int    DelayBefore; // ms to wait before printing this line

    public TermLine(string text, Color col, int delayBefore = 0)
    {
        Text        = text;
        Col         = col;
        DelayBefore = delayBefore;
    }
}

// ======================================================================
//  ATTACKER CONSOLE FORM
// ======================================================================
class AttackerConsole : Form
{
    // ---- speed adjuster -----------------------------------------------
    // Lower = faster scroll, Higher = slower
    // Recommended range: 20 (fast/intimidating) to 80 (readable)
    const int SCROLL_DELAY_MS = 40;

    // ---- controls ------------------------------------------------------
    RichTextBox _terminal;
    Panel       _summaryPanel;
    Panel       _statsPanel;
    Button      _closeBtn;
    System.Windows.Forms.Timer _cursorTimer;
    System.Windows.Forms.Timer _scrollTimer;

    // ---- state ---------------------------------------------------------
    List<TermLine> _lines;
    int            _lineIndex   = 0;
    bool           _cursorOn    = true;
    bool           _scrollDone  = false;
    string         _lastLine    = "";
    Action         _onClose;

    // ======================================================================
    public AttackerConsole(Action onCloseCallback)
    {
        _onClose = onCloseCallback;

        Text            = "REMOTE SESSION  -  ACME-CORP\\JSmith  -  [ACTIVE]";
        Size            = new Size(760, 560);
        MinimumSize     = new Size(760, 560);
        MaximumSize     = new Size(760, 560);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = MatrixPal.Black;

        BuildTerminal();
        BuildSummaryPanel();
        BuildCloseButton();
        BuildLines();
        StartScroll();
    }

    // ======================================================================
    //  BUILD TERMINAL
    // ======================================================================
    void BuildTerminal()
    {
        _terminal = new RichTextBox {
            Location        = new Point(0, 0),
            Size            = new Size(744, 460),
            BackColor       = MatrixPal.Black,
            ForeColor       = MatrixPal.MatrixGreen,
            Font            = new Font("Consolas", 11),
            ReadOnly        = true,
            BorderStyle     = BorderStyle.None,
            ScrollBars      = RichTextBoxScrollBars.Vertical,
            WordWrap        = true
        };
        Controls.Add(_terminal);
    }

    // ======================================================================
    //  BUILD SUMMARY PANEL  (hidden until scroll complete)
    // ======================================================================
    void BuildSummaryPanel()
    {
        _summaryPanel = new Panel {
            Location  = new Point(0, 0),
            Size      = new Size(744, 500),
            BackColor = MatrixPal.Black,
            Visible   = false
        };

        // title bar
        Panel titleBar = new Panel {
            Location  = new Point(0, 0),
            Size      = new Size(744, 44),
            BackColor = MatrixPal.DimRed
        };
        Label titleLbl = new Label {
            Text      = "ATTACK SIMULATION COMPLETE  -  ACME-CORP\\JSmith",
            Font      = new Font("Consolas", 11, FontStyle.Bold),
            ForeColor = MatrixPal.BrightWhite,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        titleBar.Controls.Add(titleLbl);
        _summaryPanel.Controls.Add(titleBar);

        // time line
        Label timeLbl = new Label {
            Text      = "Time to full compromise:  4.8 seconds",
            Font      = new Font("Consolas", 11, FontStyle.Bold),
            ForeColor = MatrixPal.RedAlert,
            Location  = new Point(20, 54),
            Size      = new Size(700, 22)
        };
        _summaryPanel.Controls.Add(timeLbl);

        Label subLbl = new Label {
            Text      = "The following occurred silently in the background while the user watched a webpage:",
            Font      = new Font("Consolas", 9),
            ForeColor = MatrixPal.DimGreen,
            Location  = new Point(20, 80),
            Size      = new Size(700, 18)
        };
        _summaryPanel.Controls.Add(subLbl);

        // stats panel
        _statsPanel = new Panel {
            Location  = new Point(14, 106),
            Size      = new Size(716, 270),
            BackColor = MatrixPal.Black
        };
        _summaryPanel.Controls.Add(_statsPanel);

        string[] labels = {
            "CREDENTIALS STOLEN",
            "SENSITIVE FILES EXPOSED",
            "NETWORK SHARES BREACHED",
            "DOMAIN CONTROLLER HIT",
            "RANSOMWARE DEPLOYED",
            "ESTIMATED RECOVERY TIME",
            "AVERAGE RANSOM DEMAND",
            "DATA BREACH NOTIFICATION",
            "CYBER INSURANCE STATUS"
        };
        string[] values = {
            "4 cached credential entries exfiltrated",
            "847 documents indexed and staged for exfiltration",
            "4 shares breached  ( Finance, HR, Executive, IT )",
            "ACME-DC01 fully compromised - domain admin obtained",
            "All local and mapped drives encrypted",
            "30 days average - business operations halted",
            "$2.3M average SMB ransom demand ( 2025 )",
            "Required in 47 states - legal exposure immediate",
            "LIKELY VOIDED - endpoint running as Full Admin\n                              No least-privilege policy in place"
        };
        Color[] valueColors = {
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite,
            MatrixPal.BrightWhite
        };

        int yPos = 4;
        for (int i = 0; i < labels.Length; i++)
        {
            int idx = i;
            Panel row = new Panel {
                Location  = new Point(0, yPos),
                Size      = new Size(716, i == 8 ? 42 : 26),
                BackColor = MatrixPal.Black
            };

            Label lbl = new Label {
                Text      = labels[idx],
                Font      = new Font("Consolas", 9, FontStyle.Bold),
                ForeColor = MatrixPal.DimGreen,
                Location  = new Point(0, 4),
                Size      = new Size(230, 18)
            };
            Label val = new Label {
                Text      = values[idx],
                Font      = new Font("Consolas", 9),
                ForeColor = valueColors[idx],
                Location  = new Point(238, 4),
                Size      = new Size(470, i == 8 ? 34 : 18)
            };

            row.Controls.Add(lbl);
            row.Controls.Add(val);
            _statsPanel.Controls.Add(row);
            yPos += (i == 8 ? 46 : 28);
        }

        // bottom banner
        Panel banner = new Panel {
            Location  = new Point(14, 384),
            Size      = new Size(716, 38),
            BackColor = MatrixPal.DimRed
        };
        Label bannerLbl = new Label {
            Text      = "THIS ALL STARTED WITH ONE CLICK.",
            Font      = new Font("Consolas", 12, FontStyle.Bold),
            ForeColor = MatrixPal.BrightWhite,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        banner.Controls.Add(bannerLbl);
        _summaryPanel.Controls.Add(banner);

        // close button
        Button closeBtn = new Button {
            Text      = "Show AutoElevate With Blocker Enabled",
            Size      = new Size(340, 32),
            Location  = new Point(199, 428),
            FlatStyle = FlatStyle.Flat,
            BackColor = MatrixPal.DimRed,
            ForeColor = MatrixPal.BrightWhite,
            Font      = new Font("Consolas", 10, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        closeBtn.FlatAppearance.BorderColor = MatrixPal.RedAlert;
        closeBtn.Click += (s, e) => {
            System.Threading.Thread.Sleep(1200);
            if (_onClose != null) _onClose();
            Close();
        };
        _summaryPanel.Controls.Add(closeBtn);

        Controls.Add(_summaryPanel);
    }

    // ======================================================================
    //  BUILD CLOSE BUTTON (visible during scroll for presenter bail-out)
    // ======================================================================
    void BuildCloseButton()
    {
        _closeBtn = new Button {
            Text      = "Skip",
            Size      = new Size(60, 22),
            Location  = new Point(678, 466),
            FlatStyle = FlatStyle.Flat,
            BackColor = MatrixPal.Black,
            ForeColor = MatrixPal.DimGreen,
            Font      = new Font("Consolas", 8),
            Cursor    = Cursors.Hand
        };
        _closeBtn.FlatAppearance.BorderColor = MatrixPal.DimGreen;
        _closeBtn.Click += (s, e) => ShowSummary();
        Controls.Add(_closeBtn);
    }

    // ======================================================================
    //  BUILD TERMINAL LINES
    // ======================================================================
    void BuildLines()
    {
        _lines = new List<TermLine>();

        // session header
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  ######  #######  #    #  ######  #######  #######",       MatrixPal.DimGreen,    0);
        Add("  #    #  #        ##  ##  #    #     #    #      ",       MatrixPal.DimGreen,    0);
        Add("  ######  #####    # ## #  #    #     #    #####  ",       MatrixPal.MatrixGreen, 0);
        Add("  #  #    #        #    #  #    #     #    #      ",       MatrixPal.DimGreen,    0);
        Add("  #   #   #######  #    #  ######     #    #######",       MatrixPal.DimGreen,    0);
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  AI-ASSISTED ATTACK FRAMEWORK  v4.2.1",                        MatrixPal.HotGreen,    0);
        Add("  TARGET: ACME-CORP\\JSmith  |  SESSION: ESTABLISHED",          MatrixPal.MatrixGreen, 0);
        Add("  HOST: ACME-WORKSTATION-04  |  OS: Windows 11 Pro 22631",      MatrixPal.DimGreen,    0);
        Add("  PRIVILEGE: User  ->  escalating...",                          MatrixPal.DimGreen,    0);
        Add("",                                                              MatrixPal.MatrixGreen, 200);

        // recon phase
        Add("[ PHASE 1 ] RECONNAISSANCE",                                    MatrixPal.HotGreen,    300);
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  [>] Enumerating user profile...",                             MatrixPal.DimGreen,    0);
        Add("  [>] Scanning Documents folder...",                            MatrixPal.DimGreen,    0);
        Add("  [+] C:\\Users\\JSmith\\Documents\\Q1-Sales-Pipeline.xlsx",       MatrixPal.MatrixGreen, 0);
        Add("  [+] C:\\Users\\JSmith\\Documents\\Customer-Contracts-2026.pdf",  MatrixPal.MatrixGreen, 0);
        Add("  [+] C:\\Users\\JSmith\\Documents\\HR-Salaries-2026.xlsx",        MatrixPal.BrightWhite, 0);
        Add("  [+] C:\\Users\\JSmith\\Documents\\Executive-Comp-2026.xlsx",     MatrixPal.BrightWhite, 0);
        Add("  [+] C:\\Users\\JSmith\\Downloads\\CyberInsurance-Policy-2026.pdf",MatrixPal.BrightWhite,0);
        Add("  [+] C:\\Users\\JSmith\\AppData\\Roaming\\Microsoft\\Credentials\\",MatrixPal.RedAlert,  0);
        Add("  [!] Credential cache located - harvesting...",                MatrixPal.RedAlert,    0);
        Add("  [+] 4 credential entries extracted",                          MatrixPal.RedAlert,    0);
        Add("  [+] ACME-CORP\\Administrator hash obtained",                   MatrixPal.RedAlert,    200);
        Add("",                                                              MatrixPal.MatrixGreen, 0);

        // mshta phase
        Add("[ PHASE 2 ] FOOTHOLD - mshta.exe",                             MatrixPal.HotGreen,    300);
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  [>] Calling mshta.exe with remote payload URL...",            MatrixPal.DimGreen,    0);
        Add("  [>] Remote script executing in memory - no file written...",  MatrixPal.DimGreen,    0);
        Add("  [+] Backdoor established - C2 channel open",                  MatrixPal.RedAlert,    0);
        Add("  [+] Registry persistence key written:",                       MatrixPal.MatrixGreen, 0);
        Add("      HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run",    MatrixPal.DimGreen,    0);
        Add("  [+] Survives reboot - persistence confirmed",                 MatrixPal.RedAlert,    200);
        Add("",                                                              MatrixPal.MatrixGreen, 0);

        // bitsadmin phase
        Add("[ PHASE 3 ] PAYLOAD DOWNLOAD - bitsadmin.exe",                 MatrixPal.HotGreen,    300);
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  [>] Creating BITS transfer job...",                           MatrixPal.DimGreen,    0);
        Add("  [>] Downloading via Windows Update channel - AV blind spot",  MatrixPal.DimGreen,    0);
        Add("  [+] ransomware.exe -> C:\\Windows\\Temp\\svchost32.exe  100%",  MatrixPal.RedAlert,    0);
        Add("  [+] credential_harvest.exe -> C:\\Windows\\Temp\\wmi32.exe",    MatrixPal.RedAlert,    0);
        Add("  [+] lateral_move.exe -> C:\\Windows\\Temp\\lsass_helper.exe",   MatrixPal.RedAlert,    200);
        Add("",                                                              MatrixPal.MatrixGreen, 0);

        // script host phase
        Add("[ PHASE 4 ] NETWORK ENUMERATION - wscript.exe",                MatrixPal.HotGreen,    300);
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  [>] Running network enumeration script...",                   MatrixPal.DimGreen,    0);
        Add("  [+] Network share discovered: \\\\ACME-DC01\\Finance\\",        MatrixPal.MatrixGreen, 0);
        Add("  [+] Network share discovered: \\\\ACME-DC01\\HR-Confidential\\",MatrixPal.BrightWhite, 0);
        Add("  [+] Network share discovered: \\\\ACME-DC01\\Executive-Shared\\",MatrixPal.BrightWhite,0);
        Add("  [+] Network share discovered: \\\\ACME-DC01\\IT-Infrastructure\\",MatrixPal.BrightWhite,0);
        Add("  [+] All shares accessible - Full Admin token confirmed",       MatrixPal.RedAlert,    0);
        Add("  [!] Full Admin privileges detected - no least privilege",      MatrixPal.RedAlert,    200);
        Add("",                                                              MatrixPal.MatrixGreen, 0);

        // powershell phase
        Add("[ PHASE 5 ] LATERAL MOVEMENT - powershell.exe",                MatrixPal.HotGreen,    300);
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  [>] Disabling Windows Defender real-time protection...",      MatrixPal.DimGreen,    0);
        Add("  [+] Defender disabled",                                       MatrixPal.RedAlert,    0);
        Add("  [>] Harvesting cached credentials via PowerShell...",         MatrixPal.DimGreen,    0);
        Add("  [+] 4 entries extracted from Windows Credential Manager",     MatrixPal.RedAlert,    0);
        Add("  [>] Scanning subnet 192.168.1.0/24...",                       MatrixPal.DimGreen,    0);
        Add("  [+] ACME-WS01      192.168.1.101  COMPROMISED",               MatrixPal.RedAlert,    0);
        Add("  [+] ACME-WS02      192.168.1.102  COMPROMISED",               MatrixPal.RedAlert,    0);
        Add("  [+] ACME-DC01      192.168.1.10   COMPROMISED - DOMAIN CTRL", MatrixPal.RedAlert,    0);
        Add("  [!] Domain Admin credentials obtained via DC",                MatrixPal.RedAlert,    0);
        Add("  [!] Full domain compromise achieved",                         MatrixPal.RedAlert,    200);
        Add("",                                                              MatrixPal.MatrixGreen, 0);

        // ransomware phase
        Add("[ PHASE 6 ] RANSOMWARE DEPLOYMENT",                            MatrixPal.RedAlert,    400);
        Add("",                                                              MatrixPal.MatrixGreen, 0);
        Add("  [>] Executing ransomware payload on all compromised hosts...", MatrixPal.DimGreen,   0);
        Add("  [ENCRYPT] C:\\Users\\JSmith\\Documents\\  ...encrypting",       MatrixPal.RedAlert,    0);
        Add("  [ENCRYPT] C:\\Users\\JSmith\\Desktop\\    ...encrypting",       MatrixPal.RedAlert,    0);
        Add("  [ENCRYPT] \\\\ACME-DC01\\Finance\\         ...encrypting",       MatrixPal.RedAlert,    0);
        Add("  [ENCRYPT] \\\\ACME-DC01\\HR-Confidential\\ ...encrypting",       MatrixPal.RedAlert,    0);
        Add("  [ENCRYPT] \\\\ACME-DC01\\Executive-Shared\\ ...encrypting",      MatrixPal.RedAlert,    0);
        Add("  [ENCRYPT] \\\\ACME-DC01\\IT-Infrastructure\\ ...encrypting",     MatrixPal.RedAlert,    0);
        Add("  [+] 847 files encrypted across 6 locations",                  MatrixPal.RedAlert,    0);
        Add("  [+] Shadow copies deleted - recovery blocked",                MatrixPal.RedAlert,    0);
        Add("  [+] Backups enumerated and encrypted",                        MatrixPal.RedAlert,    0);
        Add("",                                                              MatrixPal.MatrixGreen, 200);
        Add("  [COMPLETE] Attack chain finished.  Elapsed: 4.8 seconds.",    MatrixPal.BrightWhite, 400);
        Add("  [COMPLETE] Ransom note deploying to all desktops...",         MatrixPal.BrightWhite, 0);
        Add("",                                                              MatrixPal.MatrixGreen, 600);
    }

    void Add(string text, Color col, int delay)
    {
        _lines.Add(new TermLine(text, col, delay));
    }

    // ======================================================================
    //  START SCROLL TIMER
    // ======================================================================
    void StartScroll()
    {
        // cursor blink timer
        _cursorTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _cursorTimer.Tick += OnCursorBlink;
        _cursorTimer.Start();

        // scroll timer
        _scrollTimer = new System.Windows.Forms.Timer { Interval = SCROLL_DELAY_MS };
        _scrollTimer.Tick += OnScrollTick;
        _scrollTimer.Start();
    }

    // ======================================================================
    //  SCROLL TICK  - prints one line at a time
    // ======================================================================
    void OnScrollTick(object sender, EventArgs e)
    {
        if (_lineIndex >= _lines.Count)
        {
            _scrollTimer.Stop();
            _scrollDone = true;

            // brief pause then show summary
            System.Windows.Forms.Timer pauseTimer = new System.Windows.Forms.Timer { Interval = 800 };
            pauseTimer.Tick += (s2, e2) => {
                ((System.Windows.Forms.Timer)s2).Stop();
                ShowSummary();
            };
            pauseTimer.Start();
            return;
        }

        TermLine line = _lines[_lineIndex];

        // handle delay by temporarily slowing the timer
        if (line.DelayBefore > 0)
        {
            _scrollTimer.Interval = line.DelayBefore;
            line.DelayBefore = 0;
            return;
        }
        _scrollTimer.Interval = SCROLL_DELAY_MS;

        // remove cursor from previous line
        if (_terminal.Text.EndsWith(" |"))
        {
            _terminal.SelectionStart  = _terminal.Text.Length - 2;
            _terminal.SelectionLength = 2;
            _terminal.SelectedText    = "";
        }

        // append new line
        _terminal.SelectionStart  = _terminal.TextLength;
        _terminal.SelectionLength = 0;
        _terminal.SelectionColor  = line.Col;
        _terminal.AppendText(line.Text + "\n");
        _lastLine = line.Text;

        // scroll to bottom
        _terminal.ScrollToCaret();

        _lineIndex++;
    }

    // ======================================================================
    //  CURSOR BLINK
    // ======================================================================
    void OnCursorBlink(object sender, EventArgs e)
    {
        if (_scrollDone) return;

        if (_cursorOn)
        {
            _terminal.SelectionStart  = _terminal.TextLength;
            _terminal.SelectionColor  = MatrixPal.MatrixGreen;
            _terminal.AppendText(" |");
        }
        else
        {
            if (_terminal.Text.EndsWith(" |"))
            {
                _terminal.SelectionStart  = _terminal.Text.Length - 2;
                _terminal.SelectionLength = 2;
                _terminal.SelectedText    = "";
            }
        }
        _cursorOn = !_cursorOn;
    }

    // ======================================================================
    //  SHOW SUMMARY
    // ======================================================================
    void ShowSummary()
    {
        _cursorTimer.Stop();
        _closeBtn.Visible     = false;
        _terminal.Visible     = false;
        _summaryPanel.Visible = true;
        LaunchPhishingPage();
    }

    // ======================================================================
    //  LAUNCH PHISHING PAGE  (fires when close button clicked)
    // ======================================================================
    void LaunchPhishingPage()
    {
        try
        {
            Process.Start(new ProcessStartInfo {
                FileName        = "msedge.exe",
                Arguments       = "--new-window https://geekprank.shi4home.com/fake-virus/index.htm",
                UseShellExecute = true
            });
        }
        catch
        {
            try { Process.Start("https://geekprank.shi4home.com/fake-virus/index.htm"); } catch { }
        }
    }
}
