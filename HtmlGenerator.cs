using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using static TarkovQuestScanner.TarkovAPI;

namespace TarkovQuestScanner
{
    public static class HtmlGenerator
    {
        public static string GenerateReport(List<TaskData> foundTasks, List<string> notFoundNames, List<TaskData> allTasks, int version)
        {
            // Prepare data for JS
            // Ensure IDs exist
            var cleanAllTasks = allTasks.Select(t => new {  
                id = t.id ?? Guid.NewGuid().ToString(), 
                name = t.name, 
                map = t.map?.name ?? "Global/Multi-Map",
                wikiLink = t.wikiLink,
                objectives = t.objectives?.Select(o => new { maps = o.maps?.Select(m => m.name).ToList() }).ToList()
            }).ToList();

            var foundIds = foundTasks.Select(t => t.id).Where(id => id != null).ToList();

            string allTasksJson = JsonConvert.SerializeObject(cleanAllTasks);
            string foundIdsJson = JsonConvert.SerializeObject(foundIds);
            string errorsJson = JsonConvert.SerializeObject(notFoundNames);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='en'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.AppendLine("<title>Tarkov Quest Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #121212; color: #e0e0e0; margin: 0; padding: 20px; }");
            sb.AppendLine("h1 { color: #f0f0f0; border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 20px; }");
            sb.AppendLine("h2 { color: #81c784; font-size: 1.2em; border-bottom: 1px solid #444; padding-bottom: 5px; margin-top: 0; }");
            sb.AppendLine(".container { max-width: 1400px; margin: 0 auto; }");
            sb.AppendLine(".grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 20px; }");
            sb.AppendLine(".col { background-color: #1e1e1e; padding: 15px; border-radius: 8px; border: 1px solid #333; height: fit-content; }");
            sb.AppendLine(".quest-list { list-style-type: none; padding: 0; }");
            sb.AppendLine(".quest-item { padding: 6px 0; border-bottom: 1px solid #2a2a2a; display: flex; align-items: center; font-size: 0.9em; }");
            sb.AppendLine(".quest-item:last-child { border-bottom: none; }");
            sb.AppendLine("a { color: #4fc3f7; text-decoration: none; transition: color 0.2s; margin-left: 8px; flex: 1; }");
            sb.AppendLine("a:hover { color: #29b6f6; text-decoration: underline; }");
            sb.AppendLine(".map-header { color: #ffa726; font-weight: bold; margin-top: 15px; margin-bottom: 5px; display: flex; justify-content: space-between; }");
            sb.AppendLine(".quest-count { background-color: #333; padding: 2px 6px; border-radius: 10px; font-size: 0.8em; }");
            sb.AppendLine("input[type=checkbox] { cursor: pointer; margin-right: 5px; }");
            sb.AppendLine(".error-item { color: #ef5350; font-style: italic; padding: 4px 0; border-bottom: 1px solid #2a2a2a; }");
            sb.AppendLine(".input-group { margin-bottom: 15px; display: flex; gap: 5px; }");
            sb.AppendLine("input[type=text] { flex: 1; padding: 8px; background: #2c2c2c; border: 1px solid #444; color: white; border-radius: 4px; }");
            sb.AppendLine("button { padding: 8px 15px; background: #2e7d32; color: white; border: none; border-radius: 4px; cursor: pointer; }");
            sb.AppendLine("button:hover { background: #1b5e20; }");
            sb.AppendLine("#completed-list { opacity: 0.7; }");
            sb.AppendLine("</style>");
            
            sb.AppendLine("<script>");
            sb.AppendLine($"const ALL_TASKS = {allTasksJson};");
            sb.AppendLine($"const SCANNED_IDS = {foundIdsJson};");
            sb.AppendLine($"const ERRORS = {errorsJson};");
            sb.AppendLine("const COMPLETED_KEY = 'tarkov_completed_quests';");
            sb.AppendLine("const MANUAL_KEY = 'tarkov_manual_quests';");
            sb.AppendLine($"const CURRENT_VERSION = {version};");
            sb.AppendLine("setInterval(async () => { try { let v = await fetch('/version').then(r=>r.text()); if(parseInt(v) > CURRENT_VERSION) location.reload(); } catch {} }, 1000);");

            sb.AppendLine("function getStorage(key) { return JSON.parse(localStorage.getItem(key) || '[]'); }");
            sb.AppendLine("function setStorage(key, val) { localStorage.setItem(key, JSON.stringify(val)); }");

            sb.AppendLine("function toggleComplete(id) {");
            sb.AppendLine("  let list = getStorage(COMPLETED_KEY);");
            sb.AppendLine("  if(list.includes(id)) list = list.filter(x => x !== id); else list.push(id);");
            sb.AppendLine("  setStorage(COMPLETED_KEY, list);");
            sb.AppendLine("  render();");
            sb.AppendLine("}");

            sb.AppendLine("function addManual() {");
            sb.AppendLine("  const input = document.getElementById('manual-input');");
            sb.AppendLine("  const val = input.value.trim();");
            sb.AppendLine("  if(!val) return;");
            sb.AppendLine("  const task = ALL_TASKS.find(t => t.name.toLowerCase() === val.toLowerCase());");
            sb.AppendLine("  if(!task) { alert('Task not found in database!'); return; }");
            sb.AppendLine("  let list = getStorage(MANUAL_KEY);");
            sb.AppendLine("  if(!list.includes(task.id)) { list.push(task.id); setStorage(MANUAL_KEY, list); }");
            sb.AppendLine("  input.value = '';");
            sb.AppendLine("  render();");
            sb.AppendLine("}");

            sb.AppendLine("function render() {");
            sb.AppendLine("  const completedIds = getStorage(COMPLETED_KEY);");
            sb.AppendLine("  const manualIds = getStorage(MANUAL_KEY);");
            sb.AppendLine("  const activeIds = [...new Set([...SCANNED_IDS, ...manualIds])].filter(id => !completedIds.includes(id));");
            
            sb.AppendLine("  // Buckets");
            sb.AppendLine("  const globalTasks = [];");
            sb.AppendLine("  const mapTasks = {};");
            sb.AppendLine("  const completedTasks = [];");

            sb.AppendLine("  // Process Active");
            sb.AppendLine("  activeIds.forEach(id => {");
            sb.AppendLine("    const t = ALL_TASKS.find(x => x.id === id);");
            sb.AppendLine("    if(!t) return;");
            sb.AppendLine("    let maps = new Set();");
            sb.AppendLine("    if (t.objectives) {");
            sb.AppendLine("      t.objectives.forEach(obj => {");
            sb.AppendLine("        if (obj.maps) obj.maps.forEach(m => maps.add(m));");
            sb.AppendLine("      });");
            sb.AppendLine("    }");
            sb.AppendLine("    if (maps.size === 0 && t.map && t.map !== 'Global/Multi-Map') maps.add(t.map);");
            
            sb.AppendLine("    if(maps.size === 0) {");
            sb.AppendLine("      globalTasks.push(t);");
            sb.AppendLine("    } else {");
            sb.AppendLine("      maps.forEach(map => {");
            sb.AppendLine("        if(!mapTasks[map]) mapTasks[map] = [];");
            sb.AppendLine("        // Check if already added to avoid dupes in same list (unlikely but safe)");
            sb.AppendLine("        if(!mapTasks[map].find(x => x.id === t.id)) mapTasks[map].push(t);");
            sb.AppendLine("      });");
            sb.AppendLine("    }");
            sb.AppendLine("  });");

            sb.AppendLine("  // Process Completed");
            sb.AppendLine("  completedIds.forEach(id => {");
            sb.AppendLine("    const t = ALL_TASKS.find(x => x.id === id);");
            sb.AppendLine("    if(t) completedTasks.push(t);");
            sb.AppendLine("  });");

            sb.AppendLine("  // Sort lists");
            sb.AppendLine("  const nameSort = (a,b) => a.name.localeCompare(b.name);");
            sb.AppendLine("  globalTasks.sort(nameSort);");
            sb.AppendLine("  completedTasks.sort(nameSort);");

            sb.AppendLine("  // Render Col 1: Global");
            sb.AppendLine("  renderList('col-global', globalTasks, false);");

            sb.AppendLine("  // Render Col 2: Maps");
            sb.AppendLine("  const mapContainer = document.getElementById('col-maps');");
            sb.AppendLine("  mapContainer.innerHTML = '<h2>Map Quests</h2>';");
            sb.AppendLine("  const sortedMaps = Object.keys(mapTasks).sort((a,b) => mapTasks[b].length - mapTasks[a].length);");
            sb.AppendLine("  sortedMaps.forEach(map => {");
            sb.AppendLine("     const div = document.createElement('div');");
            sb.AppendLine("     div.className = 'map-header';");
            sb.AppendLine("     div.innerHTML = `<span>${map}</span><span class='quest-count'>${mapTasks[map].length}</span>`;");
            sb.AppendLine("     mapContainer.appendChild(div);");
            sb.AppendLine("     const ul = document.createElement('ul');");
            sb.AppendLine("     ul.className = 'quest-list';");
            sb.AppendLine("     mapTasks[map].sort(nameSort).forEach(t => ul.appendChild(createItem(t, false)));");
            sb.AppendLine("     mapContainer.appendChild(ul);");
            sb.AppendLine("  });");

            sb.AppendLine("  // Render Col 3: Completed & Errors");
            sb.AppendLine("  renderList('completed-list', completedTasks, true);");
            sb.AppendLine("  const errContainer = document.getElementById('error-list');");
            sb.AppendLine("  errContainer.innerHTML = '';");
            sb.AppendLine("  ERRORS.forEach(e => {");
            sb.AppendLine("    const li = document.createElement('li'); li.className = 'error-item'; li.textContent = 'Unrecognized: ' + e;");
            sb.AppendLine("    errContainer.appendChild(li);");
            sb.AppendLine("  });");
            sb.AppendLine("}");

            sb.AppendLine("function renderList(id, tasks, isCompleted) {");
            sb.AppendLine("  const el = document.getElementById(id);");
            sb.AppendLine("  el.innerHTML = '';");
            sb.AppendLine("  tasks.forEach(t => el.appendChild(createItem(t, isCompleted)));");
            sb.AppendLine("}");

            sb.AppendLine("function createItem(t, checked) {");
            sb.AppendLine("  const li = document.createElement('li'); li.className = 'quest-item';");
            sb.AppendLine("  const chk = document.createElement('input'); chk.type='checkbox'; chk.checked = checked;");
            sb.AppendLine("  chk.onclick = () => toggleComplete(t.id);");
            sb.AppendLine("  const a = document.createElement('a');");
            sb.AppendLine("  a.href = t.wikiLink || `https://escapefromtarkov.fandom.com/wiki/${t.name.replace(/ /g, '_')}`;");
            sb.AppendLine("  a.target = '_blank'; a.textContent = t.name;");
            sb.AppendLine("  li.appendChild(chk); li.appendChild(a);");
            sb.AppendLine("  return li;");
            sb.AppendLine("}");

            sb.AppendLine("  window.onload = function() { render(); };");
            sb.AppendLine("</script>");

            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine("<h1>Active Quest Report</h1>");
            
            sb.AppendLine("<div class='grid'>");
            
            // Col 1
            sb.AppendLine("<div class='col'>");
            sb.AppendLine("<h2>Global Quests</h2>");
            sb.AppendLine("<ul id='col-global' class='quest-list'></ul>");
            sb.AppendLine("</div>");

            // Col 2
            sb.AppendLine("<div class='col' id='col-maps'>");
            sb.AppendLine("<h2>Map Quests</h2>");
            sb.AppendLine("</div>");

            // Col 3
            sb.AppendLine("<div class='col'>");
            sb.AppendLine("<h2>Manual Add</h2>");
            sb.AppendLine("<div class='input-group'>");
            sb.AppendLine("<input type='text' id='manual-input' list='task-list' placeholder='Type quest name...'>");
            sb.AppendLine("<datalist id='task-list'>");
            // Inject datalist options
            foreach(var t in cleanAllTasks)
            {
                sb.AppendLine($"<option value=\"{t.name}\">");
            }
            sb.AppendLine("</datalist>");
            sb.AppendLine("<button onclick='addManual()'>Add</button>");
            sb.AppendLine("</div>");

            sb.AppendLine("<h2>Completed</h2>");
            sb.AppendLine("<ul id='completed-list' class='quest-list'></ul>");
            
            sb.AppendLine("<h2>Errors / Notes</h2>");
            sb.AppendLine("<ul id='error-list' class='quest-list'></ul>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>"); // grid
            sb.AppendLine("</div>"); // container
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }
}
