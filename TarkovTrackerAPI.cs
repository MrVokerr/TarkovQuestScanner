using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TarkovQuestScanner
{
    public class TarkovTrackerAPI
    {
        public static async Task UpdateTaskProgress(string taskId, string token, string state = "started")
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "TarkovQuestScanner/1.0");
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token); // Removed
                //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", token); // Removed
                client.DefaultRequestHeaders.Add("x-api-token", token);
                try
                {
                    // Construct JSON body with state. Example: { "state": "started" }
                    var jsonBody = $"{{\"state\": \"{state}\"}}";
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    await client.PostAsync($"https://tarkovtracker.io/api/v2/progress/task/{taskId}", content);
                }
                catch { }
            }
        }

        public class Data
        {
            public List<TasksProgress> tasksProgress { get; set; }
            public List<TaskObjectivesProgress> taskObjectivesProgress { get; set; }
            public List<HideoutModulesProgress> hideoutModulesProgress { get; set; }
            public List<HideoutPartsProgress> hideoutPartsProgress { get; set; }
            public string displayName { get; set; }
            public string userId { get; set; }
            public int? playerLevel { get; set; }
            public int? gameEdition { get; set; }
        }

        public class HideoutModulesProgress
        {
            public string id { get; set; }
            public bool? complete { get; set; }
        }

        public class HideoutPartsProgress
        {
            public string id { get; set; }
            public bool? complete { get; set; }
            public int? count { get; set; }
        }

        public class Meta
        {
            public string self { get; set; }
        }

        public class Root
        {
            public Data data { get; set; }
            public Meta meta { get; set; }
        }

        public class TaskObjectivesProgress
        {
            public string id { get; set; }
            public bool? complete { get; set; }
            public int? count { get; set; }
        }

        public class TasksProgress
        {
            public string id { get; set; }
            public bool? complete { get; set; }
            public bool failed { get; set; }
            public bool invalid { get; set; }
        }
    }
}
