// ============================================================
// ThreatDemo.cs  -  AutoElevate Threat Simulation
// Target: .NET Framework 4.x   Compiler: csc.exe
// Version 3 - all fixes applied
// ============================================================

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

[assembly: AssemblyTitle("AutoElevate Threat Simulation")]
[assembly: AssemblyProduct("AutoElevate Threat Simulation")]
[assembly: AssemblyDescription("Living-off-the-Land Attack Simulation - CyberFOX")]
[assembly: AssemblyCompany("CyberFOX")]
[assembly: AssemblyVersion("3.1.0.0")]
[assembly: AssemblyFileVersion("3.1.0.0")]

enum Stage
{
    LiveCheck        = 0,
    Phishing         = 1,
    Mshta            = 2,
    Bitsadmin        = 3,
    ScriptHost       = 4,
    PowerShellBlock  = 5,
    ExceptionAllowed = 6,
    Summary          = 7
}

static class Pal
{
    public static readonly Color Shell       = Color.FromArgb(18,  18,  24);
    public static readonly Color PanelDark   = Color.FromArgb(26,  26,  34);
    public static readonly Color PanelMid    = Color.FromArgb(36,  36,  46);
    public static readonly Color Danger      = Color.FromArgb(220,  50,  50);
    public static readonly Color DangerDark  = Color.FromArgb(140,  25,  25);
    public static readonly Color Warning     = Color.FromArgb(220, 160,   0);
    public static readonly Color WarningDark = Color.FromArgb(120,  90,   0);
    public static readonly Color Success     = Color.FromArgb( 30, 185, 100);
    public static readonly Color SuccessDark = Color.FromArgb( 20, 100,  55);
    public static readonly Color Info        = Color.FromArgb( 60, 130, 220);
    public static readonly Color InfoDark    = Color.FromArgb( 30,  70, 140);
    public static readonly Color TextBright  = Color.FromArgb(240, 240, 245);
    public static readonly Color TextMid     = Color.FromArgb(185, 185, 200);
    public static readonly Color TextDim     = Color.FromArgb(110, 110, 130);
    public static readonly Color ChainIdle   = Color.FromArgb( 50,  50,  65);
    public static readonly Color ChainActive = Color.FromArgb( 80,  80, 100);
}

// ======================================================================
//  CHAIN TRACKER ITEM
// ======================================================================
class ChainItem : Panel
{
    Label _numLbl, _lbl, _badge;

    public ChainItem(string num, string label)
    {
        Size      = new Size(144, 50);
        BackColor = Pal.ChainIdle;

        _numLbl = new Label {
            Text      = num,
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Pal.TextDim,
            Size      = new Size(34, 48),
            Location  = new Point(6, 1),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _lbl = new Label {
            Text      = label,
            Font      = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Pal.TextDim,
            Size      = new Size(100, 48),
            Location  = new Point(40, 1),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _badge = new Label {
            Text      = "",
            Font      = new Font("Segoe UI", 7, FontStyle.Bold),
            ForeColor = Color.White,
            Size      = new Size(52, 14),
            Location  = new Point(88, 18),
            TextAlign = ContentAlignment.MiddleCenter,
            Visible   = false
        };

        Controls.Add(_numLbl);
        Controls.Add(_lbl);
        Controls.Add(_badge);
    }

    public void SetActive()
    {
        BackColor       = Pal.ChainActive;
        _numLbl.ForeColor = Color.White;
        _lbl.ForeColor  = Color.White;
        _badge.Visible  = false;
    }

    public void SetBlocked()
    {
        BackColor         = Pal.DangerDark;
        _numLbl.ForeColor = Pal.Danger;
        _lbl.ForeColor    = Pal.TextMid;
        _badge.Text       = "BLOCKED";
        _badge.ForeColor  = Pal.Danger;
        _badge.Visible    = true;
    }

    public void SetAllowed()
    {
        BackColor         = Pal.SuccessDark;
        _numLbl.ForeColor = Pal.Success;
        _lbl.ForeColor    = Pal.TextMid;
        _badge.Text       = "ALLOWED";
        _badge.ForeColor  = Pal.Success;
        _badge.Visible    = true;
    }

    public void SetDone(bool allowed)
    {
        if (allowed) SetAllowed(); else SetBlocked();
    }

    public void SetIdle()
    {
        BackColor         = Pal.ChainIdle;
        _numLbl.ForeColor = Pal.TextDim;
        _lbl.ForeColor    = Pal.TextDim;
        _badge.Visible    = false;
    }
}

// ======================================================================
//  MAIN FORM
// ======================================================================
class ThreatDemo : Form
{
    Stage      _stage = Stage.LiveCheck;
    bool[]     _stageComplete;   // tracks which stages have been run
    bool[]     _stageAllowed;    // tracks outcome (blocked=false, allowed=true)

    // layout
    Panel      _headerBar;
    Panel      _leftRail;
    Panel      _contentArea;
    Panel      _footerBar;
    Panel      _summaryPanel;

    // header
    Label      _headerStage;
    Label      _headerTitle;
    Panel      _liveIndicator;
    Label      _liveLabel;

    // content boxes
    Panel      _attackerBox;
    Label      _attackerHead;
    Label      _attackerText;
    Panel      _resultBox;
    Label      _resultHead;
    Label      _resultText;
    Panel      _whyBox;
    Label      _whyHead;
    Label      _whyText;

    // footer
    Button     _actionBtn;
    Button     _backBtn;
    Button     _nextBtn;
    Label      _footerNote;

    ChainItem[] _chain;

    static readonly string[] ChainNums   = { "0", "1", "2", "3", "4", "5", "6", "7" };
    static readonly string[] ChainLabels = {
        "Pre-Flight",
        "Phishing",
        "mshta.exe",
        "bitsadmin",
        "Script Host",
        "PowerShell",
        "Exception",
        "Summary"
    };

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ThreatDemo());
    }

    public ThreatDemo()
    {
        _stageComplete = new bool[8];
        _stageAllowed  = new bool[8];

        Text            = "AutoElevate Threat Simulation  |  CyberFOX";
        Size            = new Size(960, 650);
        MinimumSize     = new Size(960, 650);
        MaximumSize     = new Size(960, 650);
        StartPosition   = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = Pal.Shell;

        BuildHeader();
        BuildLeftRail();
        BuildContentArea();
        BuildFooter();
        BuildSummaryPanel();

        LoadStage(Stage.LiveCheck);

        // auto-run preflight as soon as the form is visible
        this.Shown += (snd, ev) => DoLiveCheck();
    }

    // ======================================================================
    //  BUILD HEADER
    // ======================================================================
    void BuildHeader()
    {
        _headerBar = new Panel {
            Dock      = DockStyle.Top,
            Height    = 68,
            BackColor = Pal.Warning
        };
        Controls.Add(_headerBar);

        _headerStage = new Label {
            Font      = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Color.Black,
            Location  = new Point(12, 8),
            Size      = new Size(600, 18),
            Text      = "PRE-FLIGHT CHECK"
        };

        _headerTitle = new Label {
            Font      = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.Black,
            Location  = new Point(12, 26),
            Size      = new Size(680, 32),
            Text      = "Checking AutoElevate Status..."
        };

        // live indicator top-right
        _liveIndicator = new Panel {
            Size      = new Size(130, 40),
            Location  = new Point(_headerBar.Width - 148, 14),
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.FromArgb(60, 0, 0)
        };
        _liveLabel = new Label {
            Text      = "NOT LIVE",
            Font      = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 80, 80),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        _liveIndicator.Controls.Add(_liveLabel);

        _headerBar.Controls.Add(_headerStage);
        _headerBar.Controls.Add(_headerTitle);
        _headerBar.Controls.Add(_liveIndicator);
    }

    // ======================================================================
    //  BUILD LEFT RAIL
    // ======================================================================
    void BuildLeftRail()
    {
        _leftRail = new Panel {
            Location  = new Point(0, 68),
            Size      = new Size(154, 510),
            BackColor = Pal.Shell
        };
        Controls.Add(_leftRail);

        Label railTitle = new Label {
            Text      = "ATTACK CHAIN",
            Font      = new Font("Segoe UI", 7, FontStyle.Bold),
            ForeColor = Pal.TextDim,
            Location  = new Point(0, 10),
            Size      = new Size(154, 18),
            TextAlign = ContentAlignment.MiddleCenter
        };
        _leftRail.Controls.Add(railTitle);

        _chain = new ChainItem[8];
        for (int i = 0; i < 8; i++)
        {
            _chain[i] = new ChainItem(ChainNums[i], ChainLabels[i]);
            _chain[i].Location = new Point(5, 34 + i * 57);
            _leftRail.Controls.Add(_chain[i]);
        }
    }

    // ======================================================================
    //  BUILD CONTENT AREA
    // ======================================================================
    void BuildContentArea()
    {
        _contentArea = new Panel {
            Location  = new Point(154, 68),
            Size      = new Size(790, 510),
            BackColor = Pal.Shell
        };
        Controls.Add(_contentArea);

        // attacker box - full width, taller
        _attackerBox = new Panel {
            Location  = new Point(14, 14),
            Size      = new Size(762, 148),
            BackColor = Pal.PanelDark
        };
        _attackerBox.Paint += (s, e) =>
            e.Graphics.FillRectangle(new SolidBrush(Pal.Danger), 0, 0, 4, _attackerBox.Height);

        _attackerHead = new Label {
            Text      = "THE ATTACKER'S MOVE",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Pal.Danger,
            Location  = new Point(16, 10),
            Size      = new Size(730, 18)
        };
        _attackerText = new Label {
            Font      = new Font("Segoe UI", 11),
            ForeColor = Pal.TextBright,
            Location  = new Point(16, 32),
            Size      = new Size(730, 108),
            AutoSize  = false
        };
        _attackerBox.Controls.Add(_attackerHead);
        _attackerBox.Controls.Add(_attackerText);
        _contentArea.Controls.Add(_attackerBox);

        // result box - left half, taller
        _resultBox = new Panel {
            Location  = new Point(14, 174),
            Size      = new Size(372, 180),
            BackColor = Pal.PanelDark
        };
        _resultBox.Paint += (s, e) =>
            e.Graphics.FillRectangle(new SolidBrush(Pal.Success), 0, 0, 4, _resultBox.Height);

        _resultHead = new Label {
            Text      = "WHAT HAPPENED",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Pal.Success,
            Location  = new Point(16, 10),
            Size      = new Size(340, 18)
        };
        _resultText = new Label {
            Font      = new Font("Segoe UI", 11),
            ForeColor = Pal.TextBright,
            Location  = new Point(16, 34),
            Size      = new Size(340, 138),
            AutoSize  = false
        };
        _resultBox.Controls.Add(_resultHead);
        _resultBox.Controls.Add(_resultText);
        _contentArea.Controls.Add(_resultBox);

        // why box - right half, taller
        _whyBox = new Panel {
            Location  = new Point(400, 174),
            Size      = new Size(376, 180),
            BackColor = Pal.PanelDark
        };
        _whyBox.Paint += (s, e) =>
            e.Graphics.FillRectangle(new SolidBrush(Pal.Warning), 0, 0, 4, _whyBox.Height);

        _whyHead = new Label {
            Text      = "WHY THIS MATTERS",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Pal.Warning,
            Location  = new Point(16, 10),
            Size      = new Size(344, 18)
        };
        _whyText = new Label {
            Font      = new Font("Segoe UI", 11),
            ForeColor = Pal.TextBright,
            Location  = new Point(16, 34),
            Size      = new Size(344, 138),
            AutoSize  = false
        };
        _whyBox.Controls.Add(_whyHead);
        _whyBox.Controls.Add(_whyText);
        _contentArea.Controls.Add(_whyBox);
    }

    // ======================================================================
    //  BUILD FOOTER
    // ======================================================================
    void BuildFooter()
    {
        _footerBar = new Panel {
            Location  = new Point(154, 573),
            Size      = new Size(790, 58),
            BackColor = Pal.Shell
        };
        Controls.Add(_footerBar);

        _footerNote = new Label {
            Text      = "",
            Font      = new Font("Segoe UI", 10),
            ForeColor = Pal.TextDim,
            Location  = new Point(110, 0),
            Size      = new Size(220, 58),
            TextAlign = ContentAlignment.MiddleCenter,
        };

        _backBtn = new Button {
            Text      = "< Back",
            Size      = new Size(90, 32),
            Location  = new Point(10, 13),
            FlatStyle = FlatStyle.Flat,
            BackColor = Pal.PanelMid,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            Enabled   = false
        };
        _backBtn.FlatAppearance.BorderColor = Pal.TextDim;
        _backBtn.Click += OnBackClick;

        _actionBtn = new Button {
            Text      = "Run Check",
            Size      = new Size(200, 32),
            Location  = new Point(424, 13),
            FlatStyle = FlatStyle.Flat,
            BackColor = Pal.PanelMid,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        _actionBtn.FlatAppearance.BorderColor = Pal.TextDim;
        _actionBtn.Click += OnActionClick;

        _nextBtn = new Button {
            Text      = "Next  >",
            Size      = new Size(130, 32),
            Location  = new Point(632, 13),
            FlatStyle = FlatStyle.Flat,
            BackColor = Pal.Info,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        _nextBtn.FlatAppearance.BorderColor = Pal.InfoDark;
        _nextBtn.Click += OnNextClick;

        _footerBar.Controls.Add(_footerNote);
        _footerBar.Controls.Add(_backBtn);
        _footerBar.Controls.Add(_actionBtn);
        _footerBar.Controls.Add(_nextBtn);
    }

    // ======================================================================
    //  BUILD SUMMARY PANEL
    // ======================================================================
    void BuildSummaryPanel()
    {
        _summaryPanel = new Panel {
            Location  = new Point(154, 68),
            Size      = new Size(790, 510),
            BackColor = Pal.Shell,
            Visible   = false
        };
        Controls.Add(_summaryPanel);

        Label title = new Label {
            Text      = "AUTOELEVATE SCORECARD  -  ATTACK CHAIN COMPLETE",
            Font      = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Pal.Success,
            Location  = new Point(16, 18),
            Size      = new Size(758, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };
        _summaryPanel.Controls.Add(title);

        string[] blockedTools = {
            "mshta.exe",
            "bitsadmin.exe",
            "wscript.exe",
            "cscript.exe",
            "powershell.exe (external)"
        };
        string[] blockedReasons = {
            "Remote script foothold - never executed",
            "Silent payload download - never completed",
            "VBScript and JS engine - fully disabled",
            "VBScript and JS engine - fully disabled",
            "Direct attacker shell - dead on arrival"
        };

        int yPos = 60;
        Label blockedTitle = new Label {
            Text      = "BLOCKED  ( 5 techniques stopped cold )",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Pal.Danger,
            Location  = new Point(24, yPos),
            Size      = new Size(720, 20)
        };
        _summaryPanel.Controls.Add(blockedTitle);
        yPos += 26;

        for (int i = 0; i < blockedTools.Length; i++)
        {
            int idx = i;
            Panel row = new Panel {
                Location  = new Point(24, yPos),
                Size      = new Size(720, 36),
                BackColor = Pal.PanelDark
            };
            row.Paint += (s, e) =>
                e.Graphics.FillRectangle(new SolidBrush(Pal.Danger), 0, 0, 3, 36);

            Label xLbl = new Label {
                Text      = "X",
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Pal.Danger,
                Location  = new Point(10, 6),
                Size      = new Size(22, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Label toolLbl = new Label {
                Text      = blockedTools[idx],
                Font      = new Font("Consolas", 10, FontStyle.Bold),
                ForeColor = Pal.TextBright,
                Location  = new Point(36, 8),
                Size      = new Size(240, 20)
            };
            Label reasonLbl = new Label {
                Text      = blockedReasons[idx],
                Font      = new Font("Segoe UI", 9),
                ForeColor = Pal.TextMid,
                Location  = new Point(284, 10),
                Size      = new Size(426, 16)
            };

            row.Controls.Add(xLbl);
            row.Controls.Add(toolLbl);
            row.Controls.Add(reasonLbl);
            _summaryPanel.Controls.Add(row);
            yPos += 40;
        }

        yPos += 10;

        Label allowedTitle = new Label {
            Text      = "ALLOWED  ( legitimate IT tools unaffected )",
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Pal.Success,
            Location  = new Point(24, yPos),
            Size      = new Size(720, 20)
        };
        _summaryPanel.Controls.Add(allowedTitle);
        yPos += 26;

        Panel allowRow = new Panel {
            Location  = new Point(24, yPos),
            Size      = new Size(720, 36),
            BackColor = Pal.PanelDark
        };
        allowRow.Paint += (s, e) =>
            e.Graphics.FillRectangle(new SolidBrush(Pal.Success), 0, 0, 3, 36);
        Label okLbl = new Label {
            Text      = "OK",
            Font      = new Font("Segoe UI", 8, FontStyle.Bold),
            ForeColor = Pal.Success,
            Location  = new Point(6, 8),
            Size      = new Size(26, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };
        Label aTool = new Label {
            Text      = "powershell.exe  (Internal via YourRMM)",
            Font      = new Font("Consolas", 10, FontStyle.Bold),
            ForeColor = Pal.TextBright,
            Location  = new Point(36, 8),
            Size      = new Size(400, 20)
        };
        Label aReason = new Label {
            Text      = "Exception rule: trusted parent verified, allowed",
            Font      = new Font("Segoe UI", 9),
            ForeColor = Pal.TextMid,
            Location  = new Point(444, 10),
            Size      = new Size(266, 16)
        };
        allowRow.Controls.Add(okLbl);
        allowRow.Controls.Add(aTool);
        allowRow.Controls.Add(aReason);
        _summaryPanel.Controls.Add(allowRow);
        yPos += 50;

        Panel banner = new Panel {
            Location  = new Point(16, yPos),
            Size      = new Size(752, 52),
            BackColor = Pal.SuccessDark
        };
        Label bannerLbl = new Label {
            Text      = "Attack chain broken at every stage.  5 techniques blocked.  0 breaches.  IT wins.",
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.White,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        banner.Controls.Add(bannerLbl);
        _summaryPanel.Controls.Add(banner);
    }

    // ======================================================================
    //  LOAD STAGE
    // ======================================================================
    void LoadStage(Stage s)
    {
        _stage = s;

        bool isSummary = (s == Stage.Summary);
        _contentArea.Visible  = !isSummary;
        _summaryPanel.Visible =  isSummary;

        // reset action button - clear ALL previous click handlers by replacing the button
        // (this is the fix for the button only working once)
        _footerBar.Controls.Remove(_actionBtn);
        _actionBtn = new Button {
            Text      = "Run Check",
            Size      = new Size(200, 32),
            Location  = new Point(424, 13),
            FlatStyle = FlatStyle.Flat,
            BackColor = Pal.PanelMid,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            Cursor    = Cursors.Hand,
            Visible   = true,
            Enabled   = true
        };
        _actionBtn.FlatAppearance.BorderColor = Pal.TextDim;
        _actionBtn.Click += OnActionClick;
        _footerBar.Controls.Add(_actionBtn);

        _footerNote.Text     = "";
        _footerNote.ForeColor = Pal.TextDim;
        _backBtn.Enabled     = (s > Stage.Phishing);
        _nextBtn.Text        = "Next  >";
        _nextBtn.BackColor   = Pal.Info;

        UpdateChain(s);

        // v2 default hint for auto-trigger stages - overridden per case below
        bool isAutoStage = (s == Stage.Mshta || s == Stage.Bitsadmin ||
                            s == Stage.ScriptHost || s == Stage.PowerShellBlock);
        if (isAutoStage)
        {
            _footerNote.Text      = "Read the stage, then click Next > to trigger the attack.";
            _footerNote.ForeColor = Pal.TextDim;
        }

        switch (s)
        {
            case Stage.LiveCheck:
                SetHeader(Pal.Warning, "PRE-FLIGHT CHECK", "Checking AutoElevate Status...", true);
                SetContent(
                    "RUNNING AUTOMATIC CHECK",
                    "Checking whether AutoElevate is active and running in Live blocking mode.\n\n" +
                    "This check silently attempts to launch mshta.exe with a harmless argument.\n" +
                    "If AutoElevate is live it will be blocked immediately and you will see GREEN.\n" +
                    "If it runs, AutoElevate is not blocking and you will see RED.",
                    "WHAT WE CHECK",
                    Pal.Info,
                    "AutoElevate agent is running\n" +
                    "Policy is set to Live mode\n" +
                    "Technician Mode is OFF\n\n" +
                    "Result will appear in the top-right indicator.",
                    "IF THE CHECK FAILS",
                    Pal.Warning,
                    "Switch AutoElevate to Live mode.\n" +
                    "Turn off Technician Mode.\n" +
                    "Click the Re-Run Check button that appears."
                );
                _actionBtn.Visible   = false;
                _nextBtn.Text        = "Checking...";
                _nextBtn.BackColor   = Pal.PanelMid;
                _nextBtn.Enabled     = false;
                break;

            case Stage.Phishing:
                SetHeader(Pal.Danger, "STAGE 1 OF 6  |  INITIAL INFECTION", "The Phishing Email - It Only Takes One Click", false);
                SetContent(
                    "THE ATTACKER'S MOVE",
                    "A phishing email lands in your end user's inbox.\n" +
                    "It looks legitimate - a shipping notice, an invoice, an HR update.\n" +
                    "Against everything IT has told them, the user clicks the link.\n" +
                    "What happens next occurs entirely in the background.\n" +
                    "The user sees nothing. The attacker sees everything.",
                    "WHAT HAPPENS IN THE BACKGROUND",
                    Pal.Danger,
                    "Without AutoElevate blocking, the full attack chain executes automatically in seconds\n\n" +
                    "Click the button below to see exactly what that looks like.",
                    "THE SIMULATION",
                    Pal.Warning,
                    "the Attacker Console will show you a real-time view\n" +
                    "of what an AI-assisted attack does the moment that\n" +
                    "phishing link is clicked. Watch closely."
);
                _actionBtn.Text      = "Click Me Please!";
                _actionBtn.BackColor = Pal.DangerDark;
                break;

            case Stage.Mshta:
                SetHeader(Pal.Danger, "STAGE 2 OF 6  |  FOOTHOLD ATTEMPT", "The Attacker's First Tool - mshta.exe", false);
                SetContent(
                    "THE ATTACKER'S MOVE",
                    "From the phishing page, a script silently calls mshta.exe - a legitimate\n" +
                    "built-in Windows binary. The attacker uses it to execute a remote script\n" +
                    "without ever dropping a file to disk. No AV alert. No user prompt.",
                    "WHAT HAPPENED",
                    Pal.Success,
                    "AutoElevate blocked mshta.exe the instant it was called.\n" +
                    "Remember Phase 2 in the console - Foothold Attempt\n" +
                    "Foothold Blocked. Door Closed.",
                    "WHY THIS MATTERS",
                    Pal.Warning,
                    "mshta.exe is a favourite LOLBin because it is a signed\n" +
                    "Microsoft binary - legacy AV tools trust it completely.\n" +
                    "AutoElevate does not. Blocked before it could do anything."
                );
                _actionBtn.Visible   = false;  // v2: auto-triggered by Next
                break;

            case Stage.Bitsadmin:
                SetHeader(Pal.Danger, "STAGE 3 OF 6  |  PAYLOAD DOWNLOAD", "The Silent Downloader - bitsadmin.exe", false);
                SetContent(
                    "THE ATTACKER'S MOVE",
                    "Foothold established - now the attacker needs their toolkit.\n" +
                    "They use bitsadmin.exe, another built-in Windows tool, to silently\n" +
                    "download ransomware, a C2 agent, or a credential harvester.",
                    "WHAT HAPPENED",
                    Pal.Success,
                    "AutoElevate blocked bitsadmin.exe before the transfer started.\n" +
                    "Phase 3 - Payload Download\n" +
                    "The tools never landed in C:\\Windows\\Temp.\n",
                    "WHY THIS MATTERS",
                    Pal.Warning,
                    "BITS is the same service Windows uses for Windows Update\n" +
                    "So it is almost never blocked by default.\n" +
                    "Attackers rely on this trust. AutoElevate removes it."
                );
                _actionBtn.Visible   = false;  // v2: auto-triggered by Next
                break;

            case Stage.ScriptHost:
                SetHeader(Pal.Danger, "STAGE 4 OF 6  |  SCRIPT EXECUTION", "The Script Engines - wscript and cscript", false);
                SetContent(
                    "THE ATTACKER'S MOVE",
                    "The attacker attempts to run malicious .vbs and .js script files\n" +
                    "using Windows Script Host. These scripts enumerate the network,\n" +
                    "disable security tools, and prep the machine for the final payload.",
                    "WHAT HAPPENED",
                    Pal.Success,
                    "Both wscript.exe and cscript.exe were blocked.\n" +
                    "Phase 4 - Network Enumeration\n" +
                    "Never happened: All Shares are safe.\n"+
                    "All .js and .vbs script runs disabled.",
                    "WHY THIS MATTERS",
                    Pal.Warning,
                    "Script-based attacks leave almost no forensic trace compared\n" +
                    "to compiled malware. Blocking the engine itself - not just\n" +
                    "the script - is the only reliable defence."
                );
                _actionBtn.Visible   = false;  // v2: auto-triggered by Next
                break;

            case Stage.PowerShellBlock:
                SetHeader(Pal.Danger, "STAGE 5 OF 6  |  POWERSHELL BLOCKED", "The Attacker's Swiss Army Knife - powershell.exe", false);
                SetContent(
                    "THE ATTACKER'S MOVE",
                    "PowerShell is the most weaponised tool in modern ransomware attacks.\n" +
                    "The attacker calls it to download the final payload, disable Defender,\n" +
                    "encrypt file shares, and begin lateral movement. This is the kill shot.",
                    "WHAT HAPPENED",
                    Pal.Success,
                    "AutoElevate blocked PowerShell Immediately.\n" +
                    "Phase 5 - Lateral Movement,\n" +
                    "Blocked: Credentials were never harvested,\n" +
                    "Blocked: No domain admin gained.\n"+
                    "Blocked: Disabling Defender.",
                    "THE PROBLEM THIS CREATES",
                    Pal.Warning,
                    "Your IT team uses PowerShell every day legitimately.\n" +
                    "Blocking it entirely would break real operations.\n" +
                    "Watch the next stage - same binary, different outcome."
                );
                _actionBtn.Visible   = false;  // v2: auto-triggered by Next
                break;

            case Stage.ExceptionAllowed:
                SetHeader(Pal.Info, "STAGE 6 OF 6  |  EXCEPTION RULE", "Same Binary. Different Context. AutoElevate Knows.", false);
                SetContent(
                    "THE EXCEPTION RULE",
                    "In the simulation PowerShell was the kill shot.\n" +
                    "Defender disabled, credentials harvested, DC hit.\n" +
                    "That was the attacker calling it directly.\n" +
                    "Your RMM calls the same binary - trusted parent,\n" +
                    "AutoElevate lets it through.",
                    "WHAT HAPPENED",
                    Pal.Success,
                    "PowerShell was launched by YourRMM.exe - an approved parent.\n" +
                    "AutoElevate verified the chain and allowed execution.\n" +
                    "Your IT team's tools work without interruption.",
                    "THE BOTTOM LINE",
                    Pal.Info,
                    "Attacker calls PowerShell  =  BLOCKED\n" +
                    "Your RMM calls PowerShell  =  ALLOWED\n\n" +
                    "Same binary. Same machine. AutoElevate knows the difference."
                );
                _actionBtn.Text      = "Launch Approved RMM Agent";
                _actionBtn.BackColor = Pal.InfoDark;
                _nextBtn.Text        = "View Summary  >";
                _nextBtn.BackColor   = Pal.Success;
                break;

            case Stage.Summary:
                SetHeader(Pal.Success, "DEMO COMPLETE", "AutoElevate - Attack Chain Blocked at Every Stage", false);
                _actionBtn.Visible = false;
                _nextBtn.Text      = "Close Demo";
                _nextBtn.BackColor = Pal.SuccessDark;
                break;
        }
    }

    // ======================================================================
    //  SET HEADER
    // ======================================================================
    void SetHeader(Color c, string stageText, string titleText, bool darkText)
    {
        _headerBar.BackColor   = c;
        _headerStage.Text      = stageText;
        _headerTitle.Text      = titleText;
        Color textCol          = darkText ? Color.Black : Color.White;
        _headerStage.ForeColor = textCol;
        _headerTitle.ForeColor = textCol;
    }

    // ======================================================================
    //  SET CONTENT
    // ======================================================================
    void SetContent(
        string attackHead, string attackText,
        string resultHead, Color resultAccent, string resultText,
        string whyHead,    Color whyAccent,    string whyText)
    {
        _attackerHead.Text    = attackHead;
        _attackerText.Text    = attackText;

        _resultHead.Text      = resultHead;
        _resultHead.ForeColor = resultAccent;
        _resultText.Text      = resultText;
        _resultBox.Invalidate();

        _whyHead.Text         = whyHead;
        _whyHead.ForeColor    = whyAccent;
        _whyText.Text         = whyText;
        _whyBox.Invalidate();
    }

    // ======================================================================
    //  UPDATE CHAIN
    // ======================================================================
    void UpdateChain(Stage active)
    {
        for (int i = 0; i < _chain.Length; i++)
        {
            Stage itemStage = (Stage)i;
            if (itemStage == active)
            {
                _chain[i].SetActive();
            }
            else if (itemStage < active)
            {
                if (_stageComplete[i])
                    _chain[i].SetDone(_stageAllowed[i]);
                else if (itemStage == Stage.LiveCheck || itemStage == Stage.Phishing)
                    _chain[i].SetAllowed();
                else
                    _chain[i].SetBlocked();
            }
            else
            {
                _chain[i].SetIdle();
            }
        }
    }

    // ======================================================================
    //  BACK BUTTON
    // ======================================================================
    void OnBackClick(object sender, EventArgs e)
    {
        if (_stage > Stage.LiveCheck)
            LoadStage((Stage)((int)_stage - 1));
    }

    // ======================================================================
    //  NEXT BUTTON
    // ======================================================================
    void OnNextClick(object sender, EventArgs e)
    {
        if (_stage == Stage.Summary)
        {
            Application.Exit();
            return;
        }

        Stage next = (_stage == Stage.LiveCheck) ? Stage.Phishing : (Stage)((int)_stage + 1);

        // For attack stages, load the stage content first then
        // auto-trigger the action after a 2-second pause so it
        // feels like the attack is happening in real time
        bool isAttackStage = (next == Stage.Mshta       ||
                              next == Stage.Bitsadmin   ||
                              next == Stage.ScriptHost  ||
                              next == Stage.PowerShellBlock);

        LoadStage(next);

        if (isAttackStage)
        {
            // disable Next while attack is running
            _nextBtn.Enabled = false;
            _nextBtn.Text    = "Running...";

            // countdown in footer note
	    _footerNote.Text      = "Attack incoming in 3...";
	    _footerNote.ForeColor = Color.White;
            Application.DoEvents();
            System.Threading.Thread.Sleep(1000);
            
            _footerNote.Text      = "Attack incoming in 2...";
            _footerNote.ForeColor = Color.White;
            Application.DoEvents();
            System.Threading.Thread.Sleep(1000);

            _footerNote.Text = "Attack incoming in 1...";
            _footerNote.ForeColor = Color.White;
            Application.DoEvents();
            System.Threading.Thread.Sleep(1000);

            _footerNote.Text = "Triggering...";
            _footerNote.ForeColor = Color.White;
            Application.DoEvents();

            // fire the action for this stage
            switch (next)
            {
                case Stage.Mshta:           DoMshta();       break;
                case Stage.Bitsadmin:       DoBitsadmin();   break;
                case Stage.ScriptHost:      DoScriptHost();  break;
                case Stage.PowerShellBlock: DoPowerShell();  break;
            }

            // re-enable Next after modal is closed
            _nextBtn.Enabled = true;
            _nextBtn.Text    = "Next  >";
        }
    }

    // ======================================================================
    //  ACTION BUTTON  - dispatches to stage handler
    // ======================================================================
    void OnActionClick(object sender, EventArgs e)
    {
        _actionBtn.Enabled = false;
        switch (_stage)
        {
            case Stage.LiveCheck:       DoLiveCheck();   break;
            case Stage.Phishing:        DoPhishing();    break;
            case Stage.Mshta:           DoMshta();       break;
            case Stage.Bitsadmin:       DoBitsadmin();   break;
            case Stage.ScriptHost:      DoScriptHost();  break;
            case Stage.PowerShellBlock: DoPowerShell();  break;
            case Stage.ExceptionAllowed:DoRMM();         break;
        }
    }

    // ======================================================================
    //  STAGE ACTIONS
    // ======================================================================

    void DoLiveCheck()
    {
        // update header to show checking state
        SetHeader(Pal.Warning, "PRE-FLIGHT CHECK", "Checking AutoElevate Status...", true);
        _footerNote.Text      = "Testing mshta.exe...";
        _footerNote.ForeColor = Pal.Warning;
        _nextBtn.Enabled      = false;
        _nextBtn.Text         = "Checking...";
        Application.DoEvents();

        bool blocked = false;
        try
        {
            Process p = new Process();
            p.StartInfo.FileName        = "mshta.exe";
            p.StartInfo.Arguments       = "about:blank";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow  = true;
            p.Start();
            System.Threading.Thread.Sleep(600);
            try { p.Kill(); } catch { }
            blocked = false;
        }
        catch { blocked = true; }

        _stageComplete[0] = true;
        _stageAllowed[0]  = blocked;
        _nextBtn.Enabled  = true;

        if (blocked)
        {
            // GREEN - good to go
            SetHeader(Pal.Success, "PRE-FLIGHT CHECK  -  PASSED", "AutoElevate is Active and Blocking", false);
            _liveIndicator.BackColor = Pal.SuccessDark;
            _liveLabel.ForeColor     = Color.White;
            _liveLabel.Text          = "LIVE MODE  ON";
            _footerNote.Text         = "AutoElevate is LIVE - click Start Demo to begin.";
            _footerNote.ForeColor    = Pal.Success;
            _nextBtn.Text            = "Start Demo  >";
            _nextBtn.BackColor       = Pal.Success;
            _chain[0].SetAllowed();

            // update the content boxes to show green result
            SetContent(
                "CHECK PASSED",
                "AutoElevate is running and blocking in Live mode.\n\n" +
                "All PRE-FLIGHT checks have passed.\n" +
                "Demo is GOOD TO GO. Click Start Demo to begin.",
                "STATUS",
                Pal.Success,
                "AutoElevate agent: RUNNING\n" +
                "Blocking mode: LIVE\n" +
                "Technician Mode: OFF\n\n" +
                "All systems go.",
                "WHAT HAPPENS NEXT",
                Pal.Info,
                "Click Start Demo to begin the simulation.\n" +
                "The live indicator in the top-right will stay\n" +
                "green throughout the presentation."
            );
        }
        else
        {
            // RED - not ready
            SetHeader(Pal.Danger, "PRE-FLIGHT CHECK  -  FAILED", "AutoElevate Is NOT Blocking", false);
            _liveIndicator.BackColor = Color.FromArgb(100, 20, 20);
            _liveLabel.ForeColor     = Pal.Danger;
            _liveLabel.Text          = "NOT LIVE";
            _footerNote.Text         = "Fix AutoElevate mode then click Re-Run Check.";
            _footerNote.ForeColor    = Pal.Danger;
            _nextBtn.Text            = "Proceed Anyway  >";
            _nextBtn.BackColor       = Pal.WarningDark;
            _chain[0].SetBlocked();

            // update content boxes to show red result and instructions
            SetContent(
                "CHECK FAILED",
                "PRE-FLIGHT CHECK  -  FAILED. AutoElevate is not in Live mode.\n\n" +
                "If you proceed with the demo, the LOLBin processes will actually\n" +
                "execute on screen instead of being blocked - not a good look.",
                "WHAT TO FIX",
                Pal.Danger,
                "1. Open the AutoElevate admin console\n" +
                "2. Set the policy to LIVE mode\n" +
                "3. Confirm Technician Mode is OFF\n" +
                "4. Click Re-Run Check below",
                "WHY THIS MATTERS",
                Pal.Warning,
                "Running the demo without Live mode active means\n" +
                "the audience will see processes launching instead\n" +
                "of being blocked. Fix this before proceeding."
            );

            // show a re-run button in place of the action button
            _footerBar.Controls.Remove(_actionBtn);
            _actionBtn = new Button {
                Text      = "Re-Run Check",
                Size      = new Size(200, 32),
                Location  = new Point(424, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Pal.DangerDark,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Visible   = true,
                Enabled   = true
            };
            _actionBtn.FlatAppearance.BorderColor = Pal.Danger;
            _actionBtn.Click += (snd, ev) => {
                _actionBtn.Enabled = false;
                DoLiveCheck();
            };
            _footerBar.Controls.Add(_actionBtn);
        }
    }

    void DoPhishing()
{
    _stageComplete[1] = true;
    _stageAllowed[1]  = true;
    _chain[1].SetAllowed();
    _actionBtn.Visible    = false;
    _footerNote.Text      = "Watch the attacker console - close it when ready then click Next.";
    _footerNote.ForeColor = Pal.TextMid;

    AttackerConsole console = new AttackerConsole(() => {
        this.Activate();
        this.BringToFront();
    });
    console.Show(this);
}

    void DoMshta()
    {
        bool ok = TryBlocked("mshta.exe", "about:blank",
            "mshta.exe was blocked by AutoElevate.\nThe remote script foothold was never established.",
            "mshta.exe ran - AutoElevate did NOT block it.\nCheck Live mode and Technician Mode settings.");
        _stageComplete[2] = true;
        _stageAllowed[2]  = false;
        _chain[2].SetDone(!ok);
    }

    void DoBitsadmin()
    {
        bool ok = TryBlocked("bitsadmin.exe", "/list",
            "bitsadmin.exe was blocked by AutoElevate.\nThe silent download never started.",
            "bitsadmin.exe ran - AutoElevate did NOT block it.\nCheck Live mode and Technician Mode settings.");
        _stageComplete[3] = true;
        _stageAllowed[3]  = false;
        _chain[3].SetDone(!ok);
    }

    void DoScriptHost()
    {
        bool ok1 = TryBlocked("wscript.exe", "//B",
            "wscript.exe was blocked by AutoElevate.",
            "wscript.exe ran - AutoElevate did NOT block it.");
        System.Threading.Thread.Sleep(300);
        bool ok2 = TryBlocked("cscript.exe", "//B",
            "cscript.exe was also blocked by AutoElevate.\nBoth script engines are offline for the attacker.",
            "cscript.exe ran - AutoElevate did NOT block it.");
        _stageComplete[4] = true;
        _stageAllowed[4]  = false;
        _chain[4].SetDone(!(ok1 || ok2));
    }

    void DoPowerShell()
    {
        bool ok = TryBlocked("powershell.exe", "-NoProfile -WindowStyle Hidden -Command exit",
            "powershell.exe was blocked by AutoElevate.\nThe attacker's shell never opened.",
            "powershell.exe ran - AutoElevate did NOT block it.\nCheck Live mode and Technician Mode settings.");
        _stageComplete[5] = true;
        _stageAllowed[5]  = false;
        _chain[5].SetDone(!ok);
    }

    void DoRMM()
    {
        string rmmPath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Application.ExecutablePath),
            "YourRMM.exe");

        if (!System.IO.File.Exists(rmmPath))
        {
            ShowWarningDialog("YourRMM.exe Not Found",
                "YourRMM.exe was not found in the same folder as ThreatDemo.exe.\n\n" +
                "Please compile YourRMM.cs and place YourRMM.exe alongside ThreatDemo.exe,\n" +
                "then try again.");
            _actionBtn.Enabled = true;
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo {
                FileName        = rmmPath,
                UseShellExecute = true
            });
            _stageComplete[6]     = true;
            _stageAllowed[6]      = true;
            _chain[6].SetAllowed();
            _footerNote.Text      = "Show PowerShell allowed via RMM exception rule.";
            _footerNote.ForeColor = Pal.Success;
        }
        catch (Exception ex)
        {
            ShowWarningDialog("Launch Failed", "Could not launch YourRMM.exe:\n\n" + ex.Message);
            _actionBtn.Enabled = true;
        }
    }

    // ======================================================================
    //  TryBlocked  - returns true if the process WAS blocked
    // ======================================================================
    bool TryBlocked(string exe, string args, string blockedMsg, string notBlockedMsg)
    {
        bool wasBlocked = false;
        string detail   = "";

        try
        {
            Process p = new Process();
            p.StartInfo.FileName        = exe;
            p.StartInfo.Arguments       = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow  = true;
            p.Start();
            System.Threading.Thread.Sleep(500);
            try { p.Kill(); } catch { }
            wasBlocked = false;
            detail     = "Process started (not blocked).";
        }
        catch (Exception ex)
        {
            wasBlocked = true;
            detail     = ex.Message;
        }

        ShowBlockedDialog(exe, wasBlocked ? blockedMsg : notBlockedMsg, detail, wasBlocked);

        _footerNote.Text      = wasBlocked
            ? (exe + " blocked - click Next to continue.")
            : (exe + " was NOT blocked - check AutoElevate mode.");
        _footerNote.ForeColor = Pal.Warning;

        return wasBlocked;
    }

    // ======================================================================
    //  MODAL: BLOCKED / NOT BLOCKED DIALOG
    // ======================================================================
    void ShowBlockedDialog(string toolName, string message, string detail, bool wasBlocked)
    {
        Form d = new Form {
            Text            = wasBlocked ? "Blocked by AutoElevate" : "Warning - Not Blocked",
            Size            = new Size(440, 260),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            MinimizeBox     = false,
            BackColor       = Pal.PanelDark,
            StartPosition   = FormStartPosition.Manual
        };

        Rectangle wb = Bounds;
        Rectangle sc = Screen.FromControl(this).WorkingArea;
        int x = wb.Right + 10;
        if (x + d.Width > sc.Right) x = wb.Left - d.Width - 10;
        d.Location = new Point(x, wb.Top + 60);

        Color accent = wasBlocked ? Pal.Warning : Pal.Warning;

        Panel topBar = new Panel {
            Dock      = DockStyle.Top,
            Height    = 54,
            BackColor = wasBlocked ? Pal.DangerDark : Pal.WarningDark
        };
        Label headLbl = new Label {
            Text      = wasBlocked ? (toolName + "  --  BLOCKED") : (toolName + "  --  NOT BLOCKED"),
            Font      = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = accent,
            Location  = new Point(16, 12),
            Size      = new Size(400, 28)
        };
        topBar.Controls.Add(headLbl);
        d.Controls.Add(topBar);

        Label msgLbl = new Label {
            Text      = message,
            Font      = new Font("Segoe UI", 10),
            ForeColor = Pal.TextBright,
            Location  = new Point(16, 64),
            Size      = new Size(408, 72),
            AutoSize  = false
        };

        Label detailLbl = new Label {
            Text      = detail,
            Font      = new Font("Consolas", 8),
            ForeColor = Pal.TextDim,
            Location  = new Point(16, 140),
            Size      = new Size(408, 36),
            AutoSize  = false
        };

        Button closeBtn = new Button {
            Text      = "Close",
            Size      = new Size(90, 28),
            Location  = new Point((d.ClientSize.Width - 90) / 2, 186),
            FlatStyle = FlatStyle.Flat,
            BackColor = Pal.PanelMid,
            ForeColor = Color.White,
            Cursor    = Cursors.Hand
        };
        closeBtn.FlatAppearance.BorderColor = Pal.TextDim;
        closeBtn.Click += (s, e) => d.Close();

        d.Controls.Add(msgLbl);
        d.Controls.Add(detailLbl);
        d.Controls.Add(closeBtn);
        d.ShowDialog(this);
    }

    // ======================================================================
    //  MODAL: WARNING DIALOG
    // ======================================================================
    void ShowWarningDialog(string title, string message)
    {
        Form d = new Form {
            Text            = title,
            Size            = new Size(460, 320),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false,
            MinimizeBox     = false,
            BackColor       = Pal.PanelDark,
            StartPosition   = FormStartPosition.CenterParent
        };

        Panel topBar = new Panel {
            Dock      = DockStyle.Top,
            Height    = 48,
            BackColor = Pal.WarningDark
        };
        Label titleLbl = new Label {
            Text      = "WARNING  --  " + title,
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Pal.Warning,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        topBar.Controls.Add(titleLbl);
        d.Controls.Add(topBar);

        Label msgLbl = new Label {
            Text      = message,
            Font      = new Font("Segoe UI", 9),
            ForeColor = Pal.TextBright,
            Location  = new Point(16, 58),
            Size      = new Size(424, 196),
            AutoSize  = false
        };

        Button okBtn = new Button {
            Text      = "Understood",
            Size      = new Size(110, 28),
            Location  = new Point((d.ClientSize.Width - 110) / 2, 262),
            FlatStyle = FlatStyle.Flat,
            BackColor = Pal.PanelMid,
            ForeColor = Color.White,
            Cursor    = Cursors.Hand
        };
        okBtn.FlatAppearance.BorderColor = Pal.TextDim;
        okBtn.Click += (s, e) => d.Close();

        d.Controls.Add(msgLbl);
        d.Controls.Add(okBtn);
        d.ShowDialog(this);
    }
}
