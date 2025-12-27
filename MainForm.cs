using HtmlAgilityPack;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Online;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static TarkovQuestScanner.TarkovAPI;

namespace TarkovQuestScanner
{
    public partial class MainForm : Form
    {

        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(int hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO Dummy);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;

        private static int repeatCount = 0;
        public static System.Timers.Timer timer = new System.Timers.Timer(250);

#pragma warning disable 0649
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public ShowWindowCommands showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }
#pragma warning restore 0649

        private enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            Minimized = 2,
            Maximized = 3,
        }

        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private static readonly int WH_KEYBOARD_LL = 13;
        private static readonly int WM_KEYUP = 0x101;
        private static readonly int WH_MOUSE_LL = 14;
        private static readonly int WM_LBUTTONUP = 0x0202;
        private static readonly int WM_RBUTTONUP = 0x0205;
        private static readonly int WM_MBUTTONUP = 0x0208;
        private static readonly int WM_XBUTTONUP = 0x020C;
        private const int MOUSE_LEFT = 1001;
        private const int MOUSE_RIGHT = 1002;
        private const int MOUSE_MIDDLE = 1003;
        private const int MOUSE_X1 = 1004;
        private const int MOUSE_X2 = 1005;
        private const int XBUTTON1 = 0x0001;
        private const int XBUTTON2 = 0x0002;
        private static LowLevelProc _proc_keyboard = null;
        private static LowLevelProc _proc_mouse = null;
        private static IntPtr hhook_keyboard = IntPtr.Zero;
        private static IntPtr hhook_mouse = IntPtr.Zero;
        private static IntPtr h_instance = LoadLibrary("User32");
        private static System.Drawing.Point point = new System.Drawing.Point(0, 0);
        private static int nFlags = 0x0;
        private static CancellationTokenSource cts_info = new CancellationTokenSource();
        private static Control press_key_control = null;
        private static long idle_time = 3600000;
        private static object lockObject = new object();
        public static FullOcrModel languageModel = null;
        private static PaddleOcrAll ocrRecognizer = null;
        private static readonly object ocrLock = new object();
        private static Task<FullOcrModel> modelDownloadTask = null;
        public static DateTime KeyPressedTime;
        private static DateTime presstime;
        public static bool WaitingForTooltip = false;
        private static bool reportOpened = false;
        public static bool GettingItemInfo = false;
        private static List<TaskData> sessionFoundTasks = new List<TaskData>();
        public static Overlay overlay_info = new Overlay(true);

        public MainForm()
        {
            int style = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, style | WS_EX_LAYERED);
            if (Environment.OSVersion.Version.Major >= 6)
            {
                nFlags = 0x2;
            }
            InitializeComponent();
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tpv.ico");
                if (File.Exists(iconPath))
                {
                    Icon customIcon = new Icon(iconPath);
                    this.Icon = customIcon;
                    if (TrayIcon != null) TrayIcon.Icon = customIcon;
                }
            }
            catch { }
            SettingUI();
            SetHook();
            StartHttpServer();

            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!GettingItemInfo)
            {
                if (repeatCount >= 0)
                {
                    timer.Stop(); WaitingForTooltip = false;
                    repeatCount = 0;
                }
                else
                {
                    repeatCount++;
                    point = Control.MousePosition;
                    LoadingQuestScan();
                }
            }
        }

        private void PaddleRecognizer(Action<PaddleOcrAll> action)
        {
            EnsureRecognizer();
            if (ocrRecognizer == null)
            {
                return;
            }

            lock (ocrLock)
            {
                action?.Invoke(ocrRecognizer);
            }
        }

        private void SettingUI()
        {
            MinimizeBox = true;
            MaximizeBox = true;
            Version.Text = Program.settings["Version"];
            MinimizetoTrayWhenStartup.Checked = Convert.ToBoolean(Program.settings["MinimizetoTrayWhenStartup"]);
            
            ShowOverlay_Button.Text = GetKeybindText(Program.settings["ShowOverlay_Key"]);

            if (Program.settings.ContainsKey("Mode"))
                modeSelector.Text = Program.settings["Mode"];
            else
                modeSelector.Text = "PVP";

            if (Program.settings.ContainsKey("TarkovTrackerAPIKey"))
            {
                textBoxTarkovTrackerAPI.Text = Program.settings["TarkovTrackerAPIKey"];
                // Trigger verification without blocking UI
                Task.Run(async () => 
                {
                    bool isValid = await TarkovTrackerAPI.VerifyToken(textBoxTarkovTrackerAPI.Text);
                    this.Invoke(new Action(() => 
                    {
                        if (isValid) textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(0, 64, 0);
                        else textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(64, 0, 0);
                    }));
                });
            }

            PaddleRecognizer(null);//init ocr

            TrayIcon.Visible = true;
            check_idle_time.Start();
        }

        private void modeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.settings["Mode"] = modeSelector.Text;
            Program.SaveSettings();
        }

        private void textBoxTarkovTrackerAPI_TextChanged(object sender, EventArgs e)
        {
            Program.settings["TarkovTrackerAPIKey"] = textBoxTarkovTrackerAPI.Text;
            Program.SaveSettings();
            textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(30, 30, 30); // Reset color on edit
        }

        private async void textBoxTarkovTrackerAPI_Leave(object sender, EventArgs e)
        {
            string token = textBoxTarkovTrackerAPI.Text.Trim();
            if (!string.IsNullOrEmpty(token))
            {
                bool isValid = await TarkovTrackerAPI.VerifyToken(token);
                if (isValid)
                    textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(0, 64, 0); // Dark Green
                else
                    textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(64, 0, 0); // Dark Red
            }
        }

        private async void btnTestToken_Click(object sender, EventArgs e)
        {
            string token = textBoxTarkovTrackerAPI.Text.Trim();
            if (string.IsNullOrEmpty(token))
            {
                Program.Log("Error: API Key is empty.");
                return;
            }

            Program.Log("Testing API Token...");
            var profile = await TarkovTrackerAPI.GetUserProfile(token);
            
            if (profile != null)
            {
                string faction = !string.IsNullOrEmpty(profile.pmcFaction) ? profile.pmcFaction : "Unknown";
                string level = profile.playerLevel.HasValue ? profile.playerLevel.ToString() : "Unknown";
                
                Program.Log($"SUCCESS: Connected to TarkovTracker!");
                Program.Log($"Profile: Level {level}, Faction: {faction}");
                
                // Also visually confirm by setting green
                textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(0, 64, 0); 
            }
            else
            {
                Program.Log("FAILED: Could not retrieve profile.");
                Program.Log("Please check your API Token and internet connection.");
                textBoxTarkovTrackerAPI.BackColor = System.Drawing.Color.FromArgb(64, 0, 0);
            }
        }

        private void SetHook()
        {
            SetHook(false);
        }

        private void SetHook(bool force)
        {
            try
            {
                if (force)
                {
                    Debug.WriteLine("force unhook.");
                    UnHook();
                }
                if (hhook_keyboard == IntPtr.Zero)
                {
                    _proc_keyboard = hookKeyboardProc;
                    hhook_keyboard = SetWindowsHookEx(WH_KEYBOARD_LL, _proc_keyboard, h_instance, 0);
                }
                setMouseHook();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void setMouseHook()
        {
            if (hhook_mouse == IntPtr.Zero)
            {
                _proc_mouse = hookMouseProc;
                hhook_mouse = SetWindowsHookEx(WH_MOUSE_LL, _proc_mouse, h_instance, 0);
            }
        }

        private void unsetMouseHook()
        {
            if (hhook_mouse != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hhook_mouse);
                hhook_mouse = IntPtr.Zero;
                _proc_mouse = null;
            }
        }

        private void UnHook()
        {
            try
            {
                if (hhook_keyboard != IntPtr.Zero)
                {
                    UnhookWindowsHookEx(hhook_keyboard);
                    hhook_keyboard = IntPtr.Zero;
                    _proc_keyboard = null;
                }
                unsetMouseHook();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private IntPtr hookKeyboardProc(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (code >= 0 && wParam == (IntPtr)WM_KEYUP)
                {
                    if (press_key_control == null)
                    {
                        int vkCode = Marshal.ReadInt32(lParam);
                        HandleGlobalKeyOrMouse(vkCode);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return CallNextHookEx(hhook_keyboard, code, (int)wParam, lParam);
        }

        private void HandleGlobalKeyOrMouse(int code)
        {
            try
            {
                int showKey = Int32.Parse(Program.settings["ShowOverlay_Key"]);

                if (code == showKey)
                {
                    KeyPressedTime = DateTime.Now;
                    Debug.WriteLine("\n\n----------------" + Program.settings["ShowOverlay_Key"] + " Key Pressed -----------------");
                    Program.Log($"Key Pressed: {code} (ShowOverlay)");
                    if ((!timer.Enabled || !WaitingForTooltip) && (KeyPressedTime - presstime).TotalMilliseconds >= 200)
                    {
                        point = Control.MousePosition;
                        WaitingForTooltip = true; timer.Start();
                        LoadingQuestScan();
                    }
                    else if ((KeyPressedTime - presstime).TotalMilliseconds < 200)
                    {
                        Debug.WriteLine("Key pressed in less than 200 milliseconds.");
                    }
                    presstime = KeyPressedTime;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Program.Log("HandleGlobalKeyOrMouse Error: " + e.Message);
            }
        }

        private IntPtr hookMouseProc(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (code >= 0)
                {
                    if (press_key_control == null)
                    {
                        int mouseCode = 0;

                        if (wParam == (IntPtr)WM_LBUTTONUP)
                        {
                            mouseCode = MOUSE_LEFT;
                        }
                        else if (wParam == (IntPtr)WM_RBUTTONUP)
                        {
                            mouseCode = MOUSE_RIGHT;
                        }
                        else if (wParam == (IntPtr)WM_MBUTTONUP)
                        {
                            mouseCode = MOUSE_MIDDLE;
                        }
                        else if (wParam == (IntPtr)WM_XBUTTONUP)
                        {
                            int mouseData = Marshal.ReadInt32(lParam, 8);
                            int buttonFlag = (mouseData >> 16) & 0xffff;
                            if (buttonFlag == XBUTTON1)
                            {
                                mouseCode = MOUSE_X1;
                            }
                            else if (buttonFlag == XBUTTON2)
                            {
                                mouseCode = MOUSE_X2;
                            }
                        }

                        if (mouseCode != 0)
                        {
                            HandleGlobalKeyOrMouse(mouseCode);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return CallNextHookEx(hhook_mouse, code, (int)wParam, lParam);
        }

        private string GetKeybindText(string value)
        {
            int code;
            if (!int.TryParse(value, out code))
            {
                return value;
            }

            switch (code)
            {
                case MOUSE_LEFT:
                    return "Mouse Left";
                case MOUSE_RIGHT:
                    return "Mouse Right";
                case MOUSE_MIDDLE:
                    return "Mouse Middle";
                case MOUSE_X1:
                    return "Mouse X1";
                case MOUSE_X2:
                    return "Mouse X2";
                default:
                    return ((Keys)code).ToString();
            }
        }

        private uint GetIdleTime()
        {
            LASTINPUTINFO LastUserAction = new LASTINPUTINFO();
            LastUserAction.cbSize = (uint)Marshal.SizeOf(LastUserAction);
            GetLastInputInfo(ref LastUserAction);
            return ((uint)Environment.TickCount - LastUserAction.dwTime);
        }

        public long GetTickCount()
        {
            return Environment.TickCount;
        }

        private void CloseApp()
        {
            UnHook();
            TrayIcon.Dispose();
            Program.SaveSettings();
            System.Windows.Forms.Application.Exit();
        }

        public void LoadingQuestScan()
        {
            cts_info.Cancel();
            cts_info = new CancellationTokenSource();

            Task task = Task.Factory.StartNew(() => ScanQuestListTask(cts_info.Token));
        }

        private int ScanQuestListTask(CancellationToken cts_one)
        {
            Program.Log("ScanQuestListTask Started.");
            Bitmap fullimage = CaptureScreen(CheckisTarkov());
            if (fullimage != null)
            {
                if (!cts_one.IsCancellationRequested)
                {
                    ScanAndSync(fullimage);
                }
            }
            else
            {
                Program.Log("ScanQuestListTask: Image null (Capture failed or wrong window).");
                Debug.WriteLine("image null");
            }
            return 0;
        }

        private void ScanAndSync(Bitmap fullimage)
        {
            Program.Log("ScanAndSync Started.");
            // Removed isUpdatingTracker check since we are not updating the tracker anymore

            if (Program.tarkovAPI == null || Program.tarkovAPI.tasks == null)
            {
                Program.Log("ScanAndSync: API data null.");
                Debug.WriteLine("error : no task list.");
                Program.Log("Quest data not loaded yet. Please wait a moment.");
                return;
            }

            // ROI: Task Name Column - Shifted left to capture names
            int x = (int)(fullimage.Width * 0.05);
            int y = (int)(fullimage.Height * 0.10);
            int w = (int)(fullimage.Width * 0.35); 
            int h = (int)(fullimage.Height * 0.88); // Increased height to capture bottom tasks
            
            // Save full image for debug
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string debugDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug_images");
            try {
                if (!Directory.Exists(debugDir)) Directory.CreateDirectory(debugDir);
                fullimage.Save(Path.Combine(debugDir, $"full_{timestamp}.png"));
            } catch (Exception ex) { Program.Log("Failed to save debug full image: " + ex.Message); }

            OpenCvSharp.Rect roi = new OpenCvSharp.Rect(x, y, w, h);
            List<TaskData> currentScanMatches = new List<TaskData>();
            List<string> notFoundNames = new List<string>();

            using (Mat ScreenMat = BitmapConverter.ToMat(fullimage))
            using (Mat roiMat = ScreenMat.SubMat(roi))
            using (Mat gray = roiMat.CvtColor(ColorConversionCodes.BGRA2GRAY))
            using (Mat binary = gray.Threshold(127, 255, ThresholdTypes.BinaryInv))
            using (Mat scaled = new Mat())
            using (Mat padded = new Mat())
            using (Mat finalInput = new Mat())
            {
                // 1. Upscale binary image by 2x
                Cv2.Resize(binary, scaled, new OpenCvSharp.Size(0, 0), 2.0, 2.0, InterpolationFlags.Nearest);

                // 2. Add White Padding (20px) to ensure text doesn't touch edges
                Cv2.CopyMakeBorder(scaled, padded, 20, 20, 20, 20, BorderTypes.Constant, new Scalar(255));

                // 3. Convert to BGR (3 channels) as models usually expect this format
                Cv2.CvtColor(padded, finalInput, ColorConversionCodes.GRAY2BGR);

                // Save images for debug
                try {
                    binary.SaveImage(Path.Combine(debugDir, $"binary_{timestamp}.png"));
                    finalInput.SaveImage(Path.Combine(debugDir, $"input_ocr_{timestamp}.png"));
                } catch (Exception ex) { Program.Log("Failed to save debug images: " + ex.Message); }

                String text = getPaddleOCR(finalInput);
                Program.Log($"OCR Raw Text Length: {text.Length}");
                if (!string.IsNullOrWhiteSpace(text))
                {
                    string[] lines = text.Split('\n');
                    
                    var ignoreList = new HashSet<string>(StringComparer.OrdinalIgnoreCase) 
                    { 
                        "Task", "Trader", "Type", "Map", "Progress", "der", "Typee", "Status", "Loc"
                    };

                    string mode = "PVP";
                    if (Program.settings.ContainsKey("Mode")) mode = Program.settings["Mode"];

                    foreach (string line in lines)
                    {
                        string cleanLine = line.Trim();
                        // Remove common OCR noise/punctuation from ends
                        cleanLine = cleanLine.Trim('.', ',', ':', ';', '-', ' ', '[', ']', '(', ')', '{', '}', '^', '*', '\'');
                        
                        bool isPve = cleanLine.IndexOf("[PVE ZONE]", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (mode == "PVP" && isPve) continue;

                        if (cleanLine.Length < 3 || ignoreList.Contains(cleanLine)) continue;

                        // Try matching with raw line first
                        TaskData bestMatch = FindBestMatch(cleanLine);

                        // If low confidence, try stripping tags (in case DB doesn't have them)
                        if (bestMatch == null)
                        {
                            string stripped = System.Text.RegularExpressions.Regex.Replace(cleanLine, @"\[.*?ZONE\]?", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                            stripped = stripped.Replace("ZONE]", "").Trim();
                            if (stripped.Length >= 3 && !ignoreList.Contains(stripped) && stripped != cleanLine)
                            {
                                bestMatch = FindBestMatch(stripped);
                            }
                        }

                        if (bestMatch != null)
                        {
                            if (!currentScanMatches.Contains(bestMatch))
                            {
                                currentScanMatches.Add(bestMatch);
                            }
                        }
                    }
                }
            }
            fullimage.Dispose();

            // Accumulate to session list
            sessionFoundTasks.AddRange(currentScanMatches);
            sessionFoundTasks = sessionFoundTasks.Distinct().ToList();

            // Update TarkovTracker if API key is present
            if (Program.settings.ContainsKey("TarkovTrackerAPIKey"))
            {
                string apiKey = Program.settings["TarkovTrackerAPIKey"];
                if (!string.IsNullOrEmpty(apiKey) && currentScanMatches.Count > 0)
                {
                    foreach (var task in currentScanMatches)
                    {
                        string tid = task.id;
                        Task.Run(() => TarkovTrackerAPI.UpdateTaskProgress(tid, apiKey));
                    }
                    Program.Log($"Sent {currentScanMatches.Count} quests to TarkovTracker.");
                }
            }

            // Generate HTML Report
            if (sessionFoundTasks.Count > 0 || notFoundNames.Count > 0)
            {
                // Filter duplicates
                notFoundNames = notFoundNames.Distinct().ToList();

                _reportVersion++;
                string htmlContent = HtmlGenerator.GenerateReport(sessionFoundTasks, notFoundNames, Program.tarkovAPI.tasks, _reportVersion);
                _reportHtml = htmlContent;
                
                if (!reportOpened)
                {
                    try { Process.Start("http://localhost:55050"); } catch {}
                    reportOpened = true;
                }
            }
            else
            {
                Program.Log("No quests found in the scanned area.");
            }

            timer.Stop(); WaitingForTooltip = false; repeatCount = 0;
        }

        private TaskData FindBestMatch(string search)
        {
            int bestScore = 99999;
            TaskData best = null;
            string mode = "PVP";
            if (Program.settings.ContainsKey("Mode")) mode = Program.settings["Mode"];

            string lowerSearch = search.ToLower();
            char[] searchChars = lowerSearch.ToCharArray();

            foreach (var task in Program.tarkovAPI.tasks)
            {
                 string normName = task.name;
                 bool isPvpTask = false;
                 bool isPveTask = false;

                 // Check and strip PVP tag
                 int pvpIndex = normName.IndexOf("[PVP ZONE]", StringComparison.OrdinalIgnoreCase);
                 if (pvpIndex >= 0) 
                 {
                     isPvpTask = true;
                     normName = normName.Remove(pvpIndex, 10).Trim();
                 }
                 
                 // Check and strip PVE tag
                 int pveIndex = normName.IndexOf("[PVE ZONE]", StringComparison.OrdinalIgnoreCase);
                 if (pveIndex >= 0) 
                 {
                     isPveTask = true;
                     normName = normName.Remove(pveIndex, 10).Trim();
                 }

                 int dist = levenshteinDistance(searchChars, normName.ToLower().ToCharArray());
                 // Scale distance to prioritize it heavily over mode preference (avoid matching wrong Part #)
                 int score = dist * 10;

                 if (mode == "PVP")
                 {
                     if (isPvpTask) score -= 2; // Prefer PVP
                     else if (isPveTask) score += 5; // Avoid PVE
                     else score += 1; // Generic: slight penalty to break tie with PVP
                 }
                 else // PVE
                 {
                     if (isPveTask) score -= 2; // Prefer PVE
                     else if (isPvpTask) score += 5; // Avoid PVP
                     else score -= 1; // Generic: slight bonus to break tie with PVP
                 }

                 if (score < bestScore)
                 {
                     bestScore = score;
                     best = task;
                 }
            }

            if (best != null)
            {
                string normalizedBestName = best.name;
                normalizedBestName = System.Text.RegularExpressions.Regex.Replace(normalizedBestName, @"\[.*?ZONE\]", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                
                double maxLen = Math.Max(search.Length, normalizedBestName.Length);
                if (maxLen == 0) maxLen = 1;

                int realDist = levenshteinDistance(searchChars, normalizedBestName.ToLower().ToCharArray());
                double confidence = 1.0 - ((double)realDist / maxLen);

                if (confidence >= 0.70)
                {
                    if (confidence < 1.0)
                    {
                        Program.Log($"Fuzzy match: '{search}' => '{best.name}' (Confidence: {confidence:P0})");
                    }
                    return best;
                }
            }
            return null;
        }

        public void LogToGui(string message)
        {
            if (this.logBox == null || this.logBox.IsDisposed) return;
            
            if (this.logBox.InvokeRequired)
            {
                this.logBox.Invoke(new Action<string>(LogToGui), message);
                return;
            }
            try
            {
                this.logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                this.logBox.ScrollToCaret();
            }
            catch { }
        }

        public void CloseItemInfo()
        {
            cts_info.Cancel();
            overlay_info.HideInfo();
        }

        private IntPtr CheckisTarkov()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd != IntPtr.Zero)
            {
                StringBuilder sbWinText = new StringBuilder(260);
                GetWindowText(hWnd, sbWinText, 260);
                string title = sbWinText.ToString();
                //Program.Log("Active Window: " + title);
                if (title == Program.appname || title.Contains("EscapeFromTarkov") || title.Contains("Escape from Tarkov"))
                {
                    return hWnd;
                }
            }
            Program.Log("CheckisTarkov failed. Active window not Tarkov.");
            Debug.WriteLine("error - no app");
            return IntPtr.Zero;
        }

        private Bitmap CaptureScreen(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                using (Graphics Graphicsdata = Graphics.FromHwnd(hWnd))
                {
                    Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds);
                    Bitmap bmp = new Bitmap(rect.Width, rect.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        IntPtr hdc = g.GetHdc();
                        PrintWindow(hWnd, hdc, nFlags);
                        g.ReleaseHdc(hdc);
                    }
                    return bmp;
                }
            }
            else
            {
#if DEBUG
                try
                {
                    return new Bitmap(@"img\test.png");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("no test img" + e.Message);
                }
#endif
                Debug.WriteLine("error - no window");
                return null;
            }
        }

        private void ShowtestImage(Mat mat)
        {
            ShowtestImage("test", mat);
        }

        private void ShowtestImage(String name, Mat mat)
        {
            Action show = delegate ()
            {
                Cv2.ImShow(name, mat);
            };
            Invoke(show);
        }

        private Task<FullOcrModel> getPaddleModel()
        {
            if (languageModel != null) return Task.FromResult(languageModel);
            if (modelDownloadTask != null) return modelDownloadTask;

            modelDownloadTask = Task.Run(async () => {
                try
                {
                    Debug.WriteLine("Download the paddle language model.");
                    Program.Log("Downloading OCR model...");
                    FullOcrModel model = await OnlineFullModels.EnglishV4.DownloadAsync();

                    lock (lockObject)
                    {
                        Debug.WriteLine("language model setted.");
                        Program.Log("OCR model loaded successfully.");
                        languageModel = model;
                    }
                    return model;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to download/load model: " + ex.Message);
                    Program.Log("Failed to download/load OCR model: " + ex.Message);
                    throw; // Re-throw to caller
                }
            });
            return modelDownloadTask;
        }

        private void EnsureRecognizer()
        {
            try
            {
                // Wait for model to download (Safe: running on background thread)
                var modelTask = getPaddleModel();
                if (!modelTask.IsCompleted) Program.Log("Waiting for OCR model download to complete...");
                modelTask.Wait();
                if (languageModel == null) return;
            }
            catch (Exception ex) 
            { 
                Program.Log("OCR Model Ensure failed: " + ex.Message);
                return; 
            }

            if (ocrRecognizer == null)
            {
                lock (ocrLock)
                {
                    if (ocrRecognizer == null)
                    {
                        try
                        {
                            ocrRecognizer = new PaddleOcrAll(languageModel, PaddleDevice.Gpu());
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Error creating PaddleOcrAll: " + e.Message);
                            try
                            {
                                ocrRecognizer = new PaddleOcrAll(languageModel, PaddleDevice.Mkldnn());
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error creating CPU PaddleOcrAll: " + ex.Message);
                                ocrRecognizer = null;
                            }
                        }
                    }
                }
            }
        }

        private String getPaddleOCR(Mat textmat)
        {
            GettingItemInfo = true;
            String text = "";
            try
            {
                EnsureRecognizer();
                if (ocrRecognizer == null)
                {
                    Program.Log("getPaddleOCR: ocrRecognizer is null (Model not loaded yet?)");
                    GettingItemInfo = false;
                    return text;
                }

                lock (ocrLock)
                {
                    PaddleOcrResult result = ocrRecognizer.Run(textmat);
                    text = result.Text;
                    Program.Log($"OCR Result: '{text}' (Regions: {result.Regions.Length})");
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                        text = text.Split(Program.splitcur)[0].Trim();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Paddle error: " + e.Message);
                Program.Log("Paddle error: " + e.Message);
                GettingItemInfo = false;
            }
            GettingItemInfo = false;
            return text;
        }



        private int getMinimum(int val1, int val2, int val3)
        {
            int minNumber = val1;
            if (minNumber > val2) minNumber = val2;
            if (minNumber > val3) minNumber = val3;
            return minNumber;
        }

        private int levenshteinDistance(char[] s, char[] t)
        {
            int m = s.Length;
            int n = t.Length;

            int[,] d = new int[m + 1, n + 1];

            for (int i = 1; i < m; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 1; j < n; j++)
            {
                d[0, j] = j;
            }

            for (int j = 1; j < n; j++)
            {
                for (int i = 1; i < m; i++)
                {
                    if (s[i] == t[j])
                    {
                        d[i, j] = d[i - 1, j - 1];
                    }
                    else
                    {
                        d[i, j] = getMinimum(d[i - 1, j], d[i, j - 1], d[i - 1, j - 1]) + 1;
                    }
                }
            }
            return d[m - 1, n - 1];
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseApp();
        }

        private void TrayExit_Click(object sender, EventArgs e)
        {
            CloseApp();
        }

        private void TrayShow_Click(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void TrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(ShowMainWindow)); } catch { }
                return;
            }

            try
            {
                this.ShowInTaskbar = true;
                this.Show();
                if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
                this.Activate();
                this.BringToFront();
            }
            catch
            {
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Fully close application on 'X' click, do not minimize to tray unless explicitly requested
            // Previously: e.Cancel = true; this.Hide();
            // Now: allow close.
        }

        private void MinimizetoTrayWhenStartup_CheckedChanged(object sender, EventArgs e)
        {
            Program.settings["MinimizetoTrayWhenStartup"] = (sender as CheckBox).Checked.ToString();
        }

        private void CheckUpdate_Click(object sender, EventArgs e)
        {
            Program.Log("Update check disabled (No Repo).");
        }

        private void check_idle_time_Tick(object sender, EventArgs e)
        {
            if (GetIdleTime() >= idle_time)
            {
                idle_time += 3600000;
                SetHook(true);
            }
            else
            {
                if (idle_time > 3600000)
                {
                    idle_time = 3600000;
                }
                SetHook();
            }
        }

        private void CloseOverlayWhenMouseMoved_CheckedChanged(object sender, EventArgs e)
        {
            Program.settings["CloseOverlayWhenMouseMoved"] = (sender as CheckBox).Checked.ToString();
        }

        public void ChangePressKeyData(Keys? keycode)
        {
            if (press_key_control != null)
            {
                if (keycode != null)
                {
                    press_key_control.Text = keycode.ToString();
                }
                else
                {
                    ShowOverlay_Button.Text = GetKeybindText(Program.settings["ShowOverlay_Key"]);
                }
                press_key_control = null;
            }
        }

        private void Overlay_Button_Click(object sender, EventArgs e)
        {
            press_key_control = (sender as Control);
            int selected = 0;
            if (press_key_control == ShowOverlay_Button)
            {
                selected = 1;
            }
            
            if (selected != 0)
            {
                KeyPressCheck kpc = new KeyPressCheck(selected);
                kpc.ShowDialog(this);
            }
        }
        private HttpListener _httpListener;
        private int _reportVersion = 0;
        private string _reportHtml = "<html><body><h1 style='color:white;background:#1e1e1e;font-family:sans-serif;padding:20px;'>Waiting for first scan... (Press F9)</h1></body></html>";

        private void StartHttpServer()
        {
            try
            {
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://localhost:55050/");
                _httpListener.Start();
                Task.Run(HandleHttpRequests);
            }
            catch (Exception ex)
            {
                Program.Log("Failed to start HTTP server: " + ex.Message);
            }
        }

        private async Task HandleHttpRequests()
        {
            while (_httpListener != null && _httpListener.IsListening)
            {
                try
                {
                    var ctx = await _httpListener.GetContextAsync();
                    var resp = ctx.Response;
                    
                    if (ctx.Request.Url.AbsolutePath == "/version")
                    {
                        byte[] buf = Encoding.UTF8.GetBytes(_reportVersion.ToString());
                        resp.ContentType = "text/plain";
                        resp.ContentLength64 = buf.Length;
                        resp.OutputStream.Write(buf, 0, buf.Length);
                    }
                    else if (ctx.Request.Url.AbsolutePath == "/reset")
                    {
                        sessionFoundTasks.Clear();
                        _reportVersion++;
                         // Regenerate empty report so immediate reload works
                        _reportHtml = HtmlGenerator.GenerateReport(sessionFoundTasks, new List<string>(), Program.tarkovAPI.tasks, _reportVersion);
                        
                        byte[] buf = Encoding.UTF8.GetBytes("OK");
                        resp.ContentType = "text/plain";
                        resp.ContentLength64 = buf.Length;
                        resp.OutputStream.Write(buf, 0, buf.Length);
                    }
                    else if (ctx.Request.Url.AbsolutePath == "/wiki-images")
                    {
                        string url = ctx.Request.QueryString["url"];
                        if (!string.IsNullOrEmpty(url))
                        {
                            var images = await GetWikiImages(url);
                            string json = JsonConvert.SerializeObject(images);
                            byte[] buf = Encoding.UTF8.GetBytes(json);
                            resp.ContentType = "application/json";
                            resp.ContentLength64 = buf.Length;
                            resp.OutputStream.Write(buf, 0, buf.Length);
                        }
                        else
                        {
                            resp.StatusCode = 400;
                        }
                    }
                    else
                    {
                        byte[] buf = Encoding.UTF8.GetBytes(_reportHtml);
                        resp.ContentType = "text/html";
                        resp.ContentLength64 = buf.Length;
                        resp.OutputStream.Write(buf, 0, buf.Length);
                    }
                    resp.Close();
                }
                catch { }
            }
        }

        private async Task<List<string>> GetWikiImages(string url)
        {
            List<string> imageUrls = new List<string>();
            try
            {
                string html = "";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                    html = await client.GetStringAsync(url);
                }

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                
                var contentNode = doc.DocumentNode.SelectSingleNode("//div[@class='mw-parser-output']");
                if (contentNode != null)
                {
                    // 1. Look for gallery images (most likely what we want)
                    var galleryImages = contentNode.SelectNodes(".//ul[contains(@class,'gallery')]//img");
                    if (galleryImages != null)
                    {
                        foreach(var img in galleryImages) ProcessImageNode(img, imageUrls);
                    }

                    // 2. Look for standard images in anchors
                    var anchorImages = contentNode.SelectNodes(".//a[@class='image']//img");
                    if (anchorImages != null)
                    {
                        foreach (var img in anchorImages) ProcessImageNode(img, imageUrls);
                    }
                    
                    // 3. Fallback: Find any image if we still have nothing
                    if (imageUrls.Count == 0)
                    {
                        var allImages = contentNode.SelectNodes(".//img");
                        if (allImages != null)
                        {
                            foreach(var img in allImages) ProcessImageNode(img, imageUrls);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.Log("Wiki fetch error: " + ex.Message);
            }
            return imageUrls.Distinct().ToList();
        }

        private void ProcessImageNode(HtmlAgilityPack.HtmlNode img, List<string> imageUrls)
        {
            try {
                string src = img.GetAttributeValue("src", "");
                int width = img.GetAttributeValue("width", 0);
                int height = img.GetAttributeValue("height", 0);
                
                string dataSrc = img.GetAttributeValue("data-src", "");
                if (!string.IsNullOrEmpty(dataSrc)) src = dataSrc;

                if (!string.IsNullOrEmpty(src))
                {
                    // Width/Height check removed because lazy-loaded images often have small placeholder dimensions
                    // but contain valid high-res URLs in data-src.
                    // if (width > 60 && height > 60) 
                    {
                        int scaleIndex = src.IndexOf("/scale-to-width-down/");
                        if (scaleIndex > 0)
                        {
                            src = src.Substring(0, scaleIndex);
                        }
                        imageUrls.Add(src);
                    }
                }
            } catch {}
        }

        private void OpenReport_Click(object sender, EventArgs e)
        {
            try { Process.Start("http://localhost:55050"); } catch { }
        }
    }
}
