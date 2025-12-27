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
                objectives = t.objectives?.Select(o => new { type = o.type, description = o.description, maps = o.maps?.Select(m => m.name).ToList() }).ToList()
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
            sb.AppendLine(".quest-icons { margin-right: 5px; font-size: 1.1em; }");
            sb.AppendLine(".map-header { color: #ffa726; font-weight: bold; margin-top: 15px; margin-bottom: 5px; display: flex; justify-content: space-between; }");
            sb.AppendLine(".quest-count { background-color: #333; padding: 2px 6px; border-radius: 10px; font-size: 0.8em; }");
            sb.AppendLine("input[type=checkbox] { cursor: pointer; margin-right: 5px; }");
            sb.AppendLine(".error-item { color: #ef5350; font-style: italic; padding: 4px 0; border-bottom: 1px solid #2a2a2a; }");
            sb.AppendLine(".input-group { margin-bottom: 15px; display: flex; gap: 5px; }");
            sb.AppendLine("input[type=text] { flex: 1; padding: 8px; background: #2c2c2c; border: 1px solid #444; color: white; border-radius: 4px; }");
            sb.AppendLine("button { padding: 8px 15px; background: #2e7d32; color: white; border: none; border-radius: 4px; cursor: pointer; }");
            sb.AppendLine("button:hover { background: #1b5e20; }");
            sb.AppendLine(".danger-btn { background: #c62828; margin-top: 10px; width: 100%; }");
            sb.AppendLine(".danger-btn:hover { background: #b71c1c; }");
            sb.AppendLine("#completed-list { opacity: 0.7; }");
            sb.AppendLine(".delete-btn { margin-left: 10px; color: #555; cursor: pointer; font-weight: bold; padding: 0 8px; font-size: 1.1em; transition: color 0.2s; }");
            sb.AppendLine(".delete-btn:hover { color: #e53935; }");
            sb.AppendLine("#image-popup { display:none; position:fixed; z-index:9999; border:2px solid #555; background:#1e1e1e; max-width:500px; max-height:500px; overflow:hidden; pointer-events:none; box-shadow: 0 0 10px #000; border-radius:4px; }");
            sb.AppendLine("#image-popup img { width:100%; height:auto; display:block; }");
            sb.AppendLine("#image-popup .loading { padding:10px; color:#aaa; text-align:center; }");
            sb.AppendLine("#image-popup .counter { position:absolute; bottom:5px; right:5px; background:rgba(0,0,0,0.7); color:white; padding:2px 5px; font-size:12px; border-radius:4px; }");
            sb.AppendLine("</style>");
            
            sb.AppendLine("<script>");
            sb.AppendLine($"const ALL_TASKS = {allTasksJson};");
            sb.AppendLine($"const SCANNED_IDS = {foundIdsJson};");
            sb.AppendLine($"const ERRORS = {errorsJson};");
            sb.AppendLine("const COMPLETED_KEY = 'tarkov_completed_quests';");
            sb.AppendLine("const MANUAL_KEY = 'tarkov_manual_quests';");
            sb.AppendLine("const HIDDEN_KEY = 'tarkov_hidden_quests';");
            sb.AppendLine($"const CURRENT_VERSION = {version};");
            sb.AppendLine("let popup = null; let imgCache = {}; let currentImages = []; let imgIndex = 0; let activeUrl = '';");

            sb.AppendLine("setInterval(async () => { try { let v = await fetch('/version').then(r=>r.text()); if(parseInt(v) > CURRENT_VERSION) location.reload(); } catch {} }, 1000);");

            sb.AppendLine("function getStorage(key) { return JSON.parse(localStorage.getItem(key) || '[]'); }");
            sb.AppendLine("function setStorage(key, val) { localStorage.setItem(key, JSON.stringify(val)); }");

            sb.AppendLine("function initPopup() { popup = document.getElementById('image-popup'); }");

            sb.AppendLine("async function showPopup(e, url) {");
            sb.AppendLine("  if(!popup) initPopup();");
            sb.AppendLine("  activeUrl = url;");
            sb.AppendLine("  popup.style.display = 'block';");
            sb.AppendLine("  movePopup(e);");
            sb.AppendLine("  if(imgCache[url]) {");
            sb.AppendLine("    currentImages = imgCache[url]; imgIndex = 0; displayImage();");
            sb.AppendLine("  } else {");
            sb.AppendLine("    popup.innerHTML = '<div class=\"loading\">Loading images...</div>';");
            sb.AppendLine("    currentImages = [];");
            sb.AppendLine("    try {");
            sb.AppendLine("      let res = await fetch('/wiki-images?url=' + encodeURIComponent(url));");
            sb.AppendLine("      let imgs = await res.json();");
            sb.AppendLine("      imgCache[url] = imgs;");
            sb.AppendLine("      if(activeUrl === url) {");
            sb.AppendLine("        if(imgs.length > 0) { currentImages = imgs; imgIndex = 0; displayImage(); }");
            sb.AppendLine("        else popup.innerHTML = '<div class=\"loading\">No images found</div>';");
            sb.AppendLine("      }");
            sb.AppendLine("    } catch { if(activeUrl === url) popup.innerHTML = '<div class=\"loading\">Error loading</div>'; }");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            sb.AppendLine("function displayImage() {");
            sb.AppendLine("  if(!popup || currentImages.length === 0) return;");
            sb.AppendLine("  popup.innerHTML = `<img src=\"${currentImages[imgIndex]}\"><div class=\"counter\">${imgIndex+1}/${currentImages.length}</div>`;");
            sb.AppendLine("}");

            sb.AppendLine("function hidePopup() { if(popup) popup.style.display = 'none'; activeUrl = ''; }");

            sb.AppendLine("function movePopup(e) {");
            sb.AppendLine("  if(!popup) return;");
            sb.AppendLine("  let x = e.clientX + 20; let y = e.clientY + 20;");
            sb.AppendLine("  if(x + 520 > window.innerWidth) x = e.clientX - 540;");
            sb.AppendLine("  if(y + 520 > window.innerHeight) y = e.clientY - 540;");
            sb.AppendLine("  popup.style.left = x + 'px'; popup.style.top = y + 'px';");
            sb.AppendLine("}");

            sb.AppendLine("function cycleImages(e) {");
            sb.AppendLine("  if(currentImages.length > 1) {");
            sb.AppendLine("    e.preventDefault();");
            sb.AppendLine("    if(e.deltaY > 0) imgIndex = (imgIndex + 1) % currentImages.length;");
            sb.AppendLine("    else imgIndex = (imgIndex - 1 + currentImages.length) % currentImages.length;");
            sb.AppendLine("    displayImage();");
            sb.AppendLine("  }");
            sb.AppendLine("}");

            sb.AppendLine("function toggleComplete(id) {");
            sb.AppendLine("  let list = getStorage(COMPLETED_KEY);");
            sb.AppendLine("  if(list.includes(id)) list = list.filter(x => x !== id); else list.push(id);");
            sb.AppendLine("  setStorage(COMPLETED_KEY, list);");
            sb.AppendLine("  render();");
            sb.AppendLine("}");

            sb.AppendLine("function hideQuest(id) {");
            sb.AppendLine("  if(!confirm('Remove this quest from the list?')) return;");
            sb.AppendLine("  let list = getStorage(HIDDEN_KEY);");
            sb.AppendLine("  if(!list.includes(id)) list.push(id);");
            sb.AppendLine("  setStorage(HIDDEN_KEY, list);");
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

            sb.AppendLine("async function clearAllTasks() {");
            sb.AppendLine("  if(!confirm('Are you sure you want to RESET the view? This will unhide all quests and clear the current scan.')) return;");
            sb.AppendLine("  setStorage(MANUAL_KEY, []);");
            sb.AppendLine("  setStorage(HIDDEN_KEY, []);");
            sb.AppendLine("  try {");
            sb.AppendLine("    await fetch('/reset');");
            sb.AppendLine("    location.reload();");
            sb.AppendLine("  } catch(e) { alert('Failed to reset: ' + e); }");
            sb.AppendLine("}");

            sb.AppendLine("function getTaskIcons(t) {");
            sb.AppendLine("  if(!t.objectives) return '';");
            sb.AppendLine("  const icons = new Set();");
            sb.AppendLine("  t.objectives.forEach(o => {");
            sb.AppendLine("    const type = o.type ? o.type.toLowerCase() : '';");
            sb.AppendLine("    const desc = o.description ? o.description.toLowerCase() : '';");
            
            sb.AppendLine("    if(type.includes('survive') || type.includes('extract') || desc.includes('survive') || desc.includes('extract')) icons.add('🏃');");
            sb.AppendLine("    else if(type === 'elimination' || type.includes('kill') || desc.includes('kill') || desc.includes('eliminate') || desc.includes('shoot') || desc.includes('hit')) icons.add('☠️');");
            sb.AppendLine("    else if(type.includes('find') || type.includes('handover') || type.includes('give')) icons.add('📦');");
            sb.AppendLine("    else if(type.includes('mark') || type.includes('place')) icons.add('📍');");
            sb.AppendLine("    else if(type.includes('visit') || type.includes('scout') || type.includes('locate')) icons.add('🔭');");
            sb.AppendLine("    else if(type.includes('skill')) icons.add('💪');");
            sb.AppendLine("    else icons.add('📝');");
            sb.AppendLine("  });");
            sb.AppendLine("  return Array.from(icons).join(' ');");
            sb.AppendLine("}");

            sb.AppendLine("function render() {");
            sb.AppendLine("  const completedIds = getStorage(COMPLETED_KEY);");
            sb.AppendLine("  const manualIds = getStorage(MANUAL_KEY);");
            sb.AppendLine("  const hiddenIds = getStorage(HIDDEN_KEY);");
            sb.AppendLine("  const activeIds = [...new Set([...SCANNED_IDS, ...manualIds])].filter(id => !completedIds.includes(id) && !hiddenIds.includes(id));");
            
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

            sb.AppendLine("    // Fallback: Check objective descriptions for known map names if no explicit maps found");
            sb.AppendLine("    if (maps.size === 0 && t.objectives) {");
            sb.AppendLine("        const KNOWN_MAPS = ['Customs', 'Factory', 'Woods', 'Shoreline', 'Interchange', 'Reserve', 'Lighthouse', 'Streets of Tarkov', 'Ground Zero', 'Labs'];");
            sb.AppendLine("        t.objectives.forEach(obj => {");
            sb.AppendLine("            if (obj.description) {");
            sb.AppendLine("                const desc = obj.description;");
            sb.AppendLine("                KNOWN_MAPS.forEach(km => {");
            sb.AppendLine("                    if (desc.includes(km)) maps.add(km);");
            sb.AppendLine("                });");
            sb.AppendLine("            }");
            sb.AppendLine("        });");
            sb.AppendLine("    }");
            
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
            sb.AppendLine("    if(hiddenIds.includes(id)) return;");
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
            
            sb.AppendLine("  const icons = getTaskIcons(t);");
            sb.AppendLine("  if(icons) {");
            sb.AppendLine("    const span = document.createElement('span');");
            sb.AppendLine("    span.className = 'quest-icons';");
            sb.AppendLine("    span.textContent = icons;");
            sb.AppendLine("    a.appendChild(span);");
            sb.AppendLine("  }");
            sb.AppendLine("  a.appendChild(document.createTextNode(t.name));");

            sb.AppendLine("  a.href = t.wikiLink || `https://escapefromtarkov.fandom.com/wiki/${t.name.replace(/ /g, '_')}`;");
            sb.AppendLine("  a.target = '_blank';");
            sb.AppendLine("  a.onmouseenter = (e) => showPopup(e, a.href);");
            sb.AppendLine("  a.onmouseleave = () => hidePopup();");
            sb.AppendLine("  a.onmousemove = (e) => movePopup(e);");
            sb.AppendLine("  a.onwheel = (e) => cycleImages(e);");
            sb.AppendLine("  li.appendChild(chk); li.appendChild(a);");

            sb.AppendLine("  const del = document.createElement('span');");
            sb.AppendLine("  del.className = 'delete-btn';");
            sb.AppendLine("  del.innerHTML = '✕';");
            sb.AppendLine("  del.title = 'Remove quest';");
            sb.AppendLine("  del.onclick = (e) => { e.preventDefault(); hideQuest(t.id); };");
            sb.AppendLine("  li.appendChild(del);");

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

            sb.AppendLine("<button onclick='clearAllTasks()' class='danger-btn'>Clear All Active Quests</button>");

            sb.AppendLine("<h2>Completed</h2>");
            sb.AppendLine("<ul id='completed-list' class='quest-list'></ul>");
            
            sb.AppendLine("<h2>Errors / Notes</h2>");
            sb.AppendLine("<ul id='error-list' class='quest-list'></ul>");
            sb.AppendLine("</div>");

            sb.AppendLine("</div>"); // grid
            sb.AppendLine("</div>"); // container
            sb.AppendLine("<div id='image-popup'></div>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }
}
