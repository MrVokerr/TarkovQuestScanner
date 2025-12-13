using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TarkovQuestScanner
{
    static class Program
    {
        public static MainForm main = null;
        public static Dictionary<String, String> settings = new Dictionary<String, String>();
        public static readonly String BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly String setting_path = Path.Combine(BaseDir, "settings.json");
        public static readonly String appname = "EscapeFromTarkov";
        public static readonly char[] splitcur = new char[] { '₽', '$', '€' };
        public const string WorthPerSlotThresholdKey = "WorthPerSlotThreshold";
        public const int WorthPerSlotThresholdDefault = 7500;
        public static DateTime APILastUpdated = DateTime.Now.AddHours(-5);
        public static TarkovAPI.Data tarkovAPI;
        public static bool forceUpdateAPI = false;
        private static object lockObject = new object();
        
        public static readonly String languageLoading = "Wait Language Model Loading";
        public static readonly String waitingForTooltip = "Loading";
        public static bool finishloadingAPI = false; 

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(Path.Combine(BaseDir, "debug_log.txt"), $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
                if (main != null)
                {
                    main.LogToGui(message);
                }
            }
            catch {{ }}
        }

        [STAThread]
        static void Main()
        {
            Log("Application Starting...");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            foreach (Process process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
            {
                if (process.Id == Process.GetCurrentProcess().Id)
                {
                    continue;
                }
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Log("Error killing process: " + ex.Message);
                }
            }
            ThreadPool.SetMinThreads(10, 10);
            ThreadPool.SetMaxThreads(20, 20);

            LoadSettings();

            string resourcePath = Path.Combine(BaseDir, "Resources");
            DirectoryInfo di = new DirectoryInfo(resourcePath);
            if (di.Exists == false)
            {
                di.Create();
            }

            string apiJsonPath = Path.Combine(resourcePath, "TarkovAPI.json");
            if (File.Exists(apiJsonPath))
                APILastUpdated = File.GetLastWriteTime(apiJsonPath);

            Task.Factory.StartNew(() => UpdateItemListAPI());

            main = new MainForm();
            if (Convert.ToBoolean(settings["MinimizetoTrayWhenStartup"]))
            {
                Application.Run();
            }
            else
            {
                Application.Run(main);
            }
        }

        public static async void UpdateItemListAPI()
        {
            Log("UpdateItemListAPI Started.");
            if (forceUpdateAPI)
            {
                lock (lockObject)
                {
                    tarkovAPI = null;
                }
            }

            // Always try to load local first if available
            bool shouldUpdateNetwork = forceUpdateAPI || (DateTime.Now - APILastUpdated).TotalMinutes >= 15;
            
            if (!File.Exists(Path.Combine(BaseDir, "Resources", "TarkovAPI.json")))
                shouldUpdateNetwork = true;

            if (shouldUpdateNetwork)
            {
                forceUpdateAPI = false;
                try
                {
                    Debug.WriteLine("\n--> Updating API...");
                    Log("Fetching Quest DB from tarkov.dev...");

                    var queryStr = "{\r\n  tasks(lang: en, gameMode: regular) {\r\n    id\r\n    name\r\n    map {\r\n      name\r\n    }\r\n    wikiLink\r\n  }\r\n}";
                    var data = new Dictionary<string, string>()
                        {
                            {"query", queryStr}
                        };

                    using (var httpClient = new HttpClient())
                    {
                        var httpResponse = await httpClient.PostAsJsonAsync("https://api.tarkov.dev/graphql", data);
                        string responseContent = await httpResponse.Content.ReadAsStringAsync();
                        
                        Log("Quest DB Response received (Length: " + responseContent.Length + ")");

                        lock (lockObject)
                        {
                            tarkovAPI = JsonConvert.DeserializeObject<TarkovAPI.Data>(responseContent);
                            if (tarkovAPI.tasks == null)
                            {
                                ResponseShell temp = JsonConvert.DeserializeObject<ResponseShell>(responseContent);
                                tarkovAPI = temp.data;
                            }
                        }
                        if (tarkovAPI?.tasks != null)
                            Log($"Quest DB Parsed Successfully. Tasks count: {tarkovAPI.tasks.Count}");
                        else
                            Log("Quest DB Parsing Failed: tasks is null");

                        APILastUpdated = DateTime.Now;
                        finishloadingAPI = true;
                        File.WriteAllText(Path.Combine(BaseDir, "Resources", "TarkovAPI.json"), responseContent);
                    }
                }
                catch (Exception ex)
                {
                    Log("Quest DB Update Error: " + ex.Message);
                    if (tarkovAPI == null) LoadLocalAPI();
                }
            }
            else if (tarkovAPI == null)
            {
                LoadLocalAPI();
            }
        }

        private static void LoadLocalAPI()
        {
            try
            {
                Log("Loading Quest DB from local cache...");
                string path = Path.Combine(BaseDir, "Resources", "TarkovAPI.json");
                if (!File.Exists(path)) {{ 
                     Log("Local Quest DB not found. Retrying network update...");
                     forceUpdateAPI = true;
                     UpdateItemListAPI(); 
                     return;
                }}

                string responseContent = File.ReadAllText(path);
                lock (lockObject)
                {
                    tarkovAPI = JsonConvert.DeserializeObject<TarkovAPI.Data>(responseContent);
                    if (tarkovAPI.tasks == null)
                    {
                        ResponseShell temp = JsonConvert.DeserializeObject<ResponseShell>(responseContent);
                        tarkovAPI = temp.data;
                    }
                }
                if (tarkovAPI?.tasks != null)
                {
                    Log($"Local Quest DB Loaded. Tasks count: {tarkovAPI.tasks.Count}");
                    finishloadingAPI = true;
                }
            }
            catch (Exception ex)
            {
                Log("Local Load Error: " + ex.Message);
            }
        }

        public static string LastUpdated(DateTime time)
        {
            TimeSpan elapsed = DateTime.Now - time;
            if (elapsed.TotalMinutes < 60) return $"Updated: {(int)elapsed.TotalMinutes}m ago";
            return $"Updated: {(int)elapsed.TotalHours}h ago";
        }

        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists(setting_path)) File.Create(setting_path).Dispose();
                String text = File.ReadAllText(setting_path);
                try {{ settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<String, String>>(text); }} 
                catch {{ settings = new Dictionary<string, string>(); }} 

                // Default Settings
                settings["Version"] = "v1.0"; 
                if (!settings.ContainsKey("MinimizetoTrayWhenStartup")) settings["MinimizetoTrayWhenStartup"] = "false";
                if (!settings.ContainsKey("ShowOverlay_Key")) settings["ShowOverlay_Key"] = "120"; // F9
                if (!settings.ContainsKey("Mode")) settings["Mode"] = "PVP";
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error 12: " + e.Message);
            }
        }

        public static void SaveSettings()
        {
            try
            {
                string jsonString = System.Text.Json.JsonSerializer.Serialize<Dictionary<String, String>>(settings);
                File.WriteAllText(setting_path, jsonString.Replace(",", ",\n"));
            }
            catch {{ }}
        }
    }
}
