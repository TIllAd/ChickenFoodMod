using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Kitchen;
using KitchenData;
using KitchenMods;
using UnityEngine;

// ModSelfTest: prüft die Spieldaten eigener Mods und rendert alles, was sie hinzufügen.
// Läuft NUR, wenn die Datei <persistentDataPath>/ModSelfTest/run.flag existiert
// (legt run-selftest.ps1 an). Sonst macht der Mod gar nichts.
namespace ModSelfTest
{
    // Läuft als Spiel-System (tickt garantiert jeden Frame, auch im Hauptmenü).
    public class SelfTestSystem : GenericSystemBase, IModSystem
    {
        private bool _active;
        private bool _done;
        private float _dataSeenAt = -1f;
        private float _lastHeartbeat;

        protected override void Initialise()
        {
            base.Initialise();
            _active = File.Exists(SelfTest.FlagPath);
            if (_active) Debug.LogWarning("[ModSelfTest] Flag gefunden, Selbsttest startet sobald die Spieldaten geladen sind.");
        }

        protected override void OnUpdate()
        {
            if (!_active || _done) return;
            float now = UnityEngine.Time.realtimeSinceStartup;
            bool ready = GameData.Main != null && GameData.Main.ItemSetView != null;
            if (now - _lastHeartbeat > 5f)
            {
                _lastHeartbeat = now;
                Debug.LogWarning($"[ModSelfTest] warte ... Spieldaten bereit: {ready}, t={now:0}s");
            }
            if (!ready) return;
            if (_dataSeenAt < 0f) _dataSeenAt = now;
            // Ein paar Sekunden warten, bis Menü, Shader und Kamera-Setup stehen.
            if (now - _dataSeenAt < 8f) return;
            _done = true;

            bool quit = true;
            try
            {
                Debug.LogWarning("[ModSelfTest] Test läuft ...");
                var flag = File.ReadAllLines(SelfTest.FlagPath).Select(l => l.Trim().Trim('\uFEFF')).Where(l => l.Length > 0).ToList();
                quit = !flag.Any(l => l.Equals("noquit", StringComparison.OrdinalIgnoreCase));
                var prefixes = flag.Where(l => !l.Equals("noquit", StringComparison.OrdinalIgnoreCase)).ToList();
                if (prefixes.Count == 0) prefixes.Add("TillAd.PlateUp.ChickenAndRice");
                new SelfTest(prefixes).Run();
            }
            catch (Exception e)
            {
                Directory.CreateDirectory(SelfTest.OutDir);
                File.WriteAllText(Path.Combine(SelfTest.OutDir, "crash.txt"), e.ToString());
                Debug.LogError("[ModSelfTest] " + e);
            }
            finally
            {
                try { File.Delete(SelfTest.FlagPath); } catch { }
                Directory.CreateDirectory(SelfTest.OutDir);
                File.WriteAllText(Path.Combine(SelfTest.OutDir, "done.txt"), DateTime.Now.ToString("s"));
            }
            if (quit) Application.Quit();
        }
    }

    public class SelfTest
    {
        public static string BaseDir => Path.Combine(Application.persistentDataPath, "ModSelfTest");
        public static string FlagPath => Path.Combine(BaseDir, "run.flag");
        public static string OutDir => Path.Combine(BaseDir, "report");
        private static string ImgDir => Path.Combine(OutDir, "img");

        private const int MaxCombosPerGroup = 4000;
        private const int MaxRendersPerGroup = 128;
        private const int ImageSize = 256;

        private readonly List<string> _prefixes;
        private readonly GameData _data = GameData.Main;
        private readonly List<Entry> _entries = new List<Entry>();
        private HashSet<int> _targetIds;
        private int _imageCounter;
        private string _renderError;

        private class Entry
        {
            public string Mod, Section, Subject, Status, Message, Image;
        }

        public SelfTest(List<string> prefixes) { _prefixes = prefixes; }

        // ------------------------------------------------------------------ Ablauf
        public void Run()
        {
            if (Directory.Exists(OutDir)) Directory.Delete(OutDir, true);
            Directory.CreateDirectory(ImgDir);

            var targets = _data.Get<GameDataObject>().Where(IsTarget).ToList();
            _targetIds = new HashSet<int>(targets.Select(t => t.ID));
            Debug.LogWarning($"[ModSelfTest] {targets.Count} Objekte aus {string.Join(", ", _prefixes)}");

            foreach (var g in targets.OfType<ItemGroup>()) TestGroupCombinations(g);
            foreach (var d in targets.OfType<Dish>()) TestOrders(d);
            foreach (var i in targets.OfType<Item>()) TestItem(i);
            foreach (var a in targets.OfType<Appliance>()) TestAppliance(a);
            foreach (var d in targets.OfType<Dish>()) TestDishCard(d);
            foreach (var c in targets.OfType<UnlockCard>()) TestUnlockCard(c);
            foreach (var p in targets.OfType<Process>())
                Add(p, "Prozesse", Short(p.name), "INFO", "eigener Prozess", null);

            WriteReport();
        }

        private bool IsTarget(GameDataObject o) => o != null && o.name != null && _prefixes.Any(p => o.name.StartsWith(p));

        // ------------------------------------------------------------------ 1. Kombinationen
        // Spielt jede Teilmenge der Zutaten eines Gerichts durch und fragt das Spiel,
        // welches Gericht daraus wird (dieselbe Funktion, die das Spiel beim Kombinieren nutzt).
        private void TestGroupCombinations(ItemGroup g)
        {
            var sets = g.DerivedSets.Where(s => !s.OrderingOnly).ToList();
            var perSet = new List<List<List<int>>>();
            foreach (var s in sets)
            {
                var ids = s.Items.Where(i => i != null).Select(i => i.ID).ToList();
                int min = s.IsMandatory ? s.Min : 0;
                perSet.Add(Subsets(ids, min, Math.Min(s.Max, ids.Count)));
            }

            int fails = 0, warns = 0, passes = 0, rendered = 0;
            var seen = new HashSet<string>();
            foreach (var combo in Product(perSet, MaxCombosPerGroup))
            {
                var flat = combo.SelectMany(c => c).ToList();
                if (flat.Count < 2) continue;
                var key = string.Join(",", flat.OrderBy(x => x));
                if (!seen.Add(key)) continue;

                bool complete = true;
                for (int i = 0; i < sets.Count; i++)
                    if (combo[i].Count < sets[i].Min) complete = false;

                var list = new ItemList(flat);
                var sat = _data.ItemSetView.IsValidGroup(list, out int gid);
                string label = Label(flat);

                if (gid != g.ID && sat == Satisfaction.Correct)
                {
                    fails++;
                    Add(g, "Kombinationen", Short(g.name), "FAIL",
                        $"{label} wird zum fremden Gericht '{Name(gid)}' – danach passt nichts mehr dazu", RenderGroup(gid, list, false));
                }
                else if (gid != g.ID && sat == Satisfaction.Partial)
                {
                    // Vorläufig einem anderen Gericht zugeordnet. Unkritisch, solange unser Gericht den
                    // Teilstand auch akzeptiert: mit der nächsten Zutat wird neu zugeordnet.
                    var own = _data.ItemSetView.IsGroupSatisfied(_data.ItemSetView.GetGroupData(g.ID), list);
                    if (own == Satisfaction.Impossible)
                    {
                        warns++;
                        Add(g, "Kombinationen", Short(g.name), "WARN",
                            $"{label} wird als '{Name(gid)}' erkannt und passt nicht mehr zu diesem Gericht", RenderGroup(gid, list, false));
                    }
                    else
                    {
                        passes++;
                        if (rendered < MaxRendersPerGroup)
                        {
                            rendered++;
                            Add(g, "Kombinationen (Bilder)", Short(g.name), "PASS",
                                $"{label}<br><small>Teilstand, vorläufig als '{Name(gid)}' geführt (unkritisch)</small>", RenderGroup(gid, list, false));
                        }
                    }
                }
                else if (gid != g.ID)
                {
                    fails++;
                    Add(g, "Kombinationen", Short(g.name), "FAIL", $"{label} lässt sich gar nicht kombinieren", null);
                }
                else if (complete && sat != Satisfaction.Correct)
                {
                    fails++;
                    Add(g, "Kombinationen", Short(g.name), "FAIL", $"{label} sollte fertig sein, Spiel sagt {sat}", null);
                }
                else
                {
                    passes++;
                    if (rendered < MaxRendersPerGroup)
                    {
                        rendered++;
                        Add(g, "Kombinationen (Bilder)", Short(g.name), "PASS",
                            complete ? $"<b>fertig</b>: {label}" : $"{label}<br><small>Teilstand</small>", RenderGroup(g.ID, list, false));
                    }
                }
            }
            Add(g, "Kombinationen", Short(g.name), fails > 0 ? "FAIL" : warns > 0 ? "WARN" : "PASS",
                $"{seen.Count} Zutaten-Kombinationen geprüft: {passes} ok, {warns} Warnungen, {fails} Fehler", null);
        }

        // ------------------------------------------------------------------ 2. Bestellungen
        // Erzeugt jede Bestellung, die Kunden mit beliebigen Kombinationen eigener Karten aufgeben können,
        // und prüft, dass sie sich bauen lässt.
        private void TestOrders(Dish dish)
        {
            if (dish.UnlocksMenuItems == null) return;
            var allDishes = _data.Get<Dish>().Where(d => _targetIds.Contains(d.ID)).ToList();

            foreach (var mi in dish.UnlocksMenuItems)
            {
                if (!(mi.Item is ItemGroup group))
                {
                    if (mi.Item != null)
                        Add(dish, "Bestellungen", Short(dish.name), "PASS", $"bestellt '{Short(mi.Item.name)}' ({mi.Phase})", RenderItem(mi.Item));
                    continue;
                }

                // Karten, die diese Karte voraussetzt (z. B. Grundgericht für eine Variante), sind immer dabei.
                var required = new List<Dish>();
                CollectRequired(dish, required, 0);
                var contributors = allDishes.Where(d => d != dish && !required.Contains(d) && d.UnlocksIngredients != null &&
                    d.UnlocksIngredients.Any(u => u.MenuItem != null && u.MenuItem.ID == group.ID)).ToList();
                if (contributors.Count > 10) contributors = contributors.Take(10).ToList();

                var configs = new Dictionary<string, (List<int> items, List<string> cards)>();
                var reported = new HashSet<string>();
                int subsetCount = 1 << contributors.Count;
                for (int mask = 0; mask < subsetCount; mask++)
                {
                    var cards = new List<Dish> { dish };
                    cards.AddRange(required);
                    for (int b = 0; b < contributors.Count; b++)
                        if ((mask & (1 << b)) != 0) cards.Add(contributors[b]);

                    var available = new HashSet<int>(cards.Where(c => c.UnlocksIngredients != null)
                        .SelectMany(c => c.UnlocksIngredients)
                        .Where(u => u.MenuItem != null && u.MenuItem.ID == group.ID && u.Ingredient != null)
                        .Select(u => u.Ingredient.ID));

                    string cardLabel = string.Join(" + ", cards.Select(c => SafeName(c)));
                    foreach (var cfg in OrderConfigurations(group, available, out var warning))
                    {
                        if (warning != null && reported.Add(warning + "|" + cardLabel))
                            Add(dish, "Bestellungen", Short(group.name), "FAIL", $"[{cardLabel}] {warning}", null);
                        var key = string.Join(",", cfg.OrderBy(x => x));
                        if (!configs.TryGetValue(key, out var entry))
                            configs[key] = entry = (cfg, new List<string>());
                        if (entry.cards.Count < 4) entry.cards.Add(cardLabel);
                    }
                }

                foreach (var kv in configs)
                {
                    var list = new ItemList(kv.Value.items);
                    var sat = _data.ItemSetView.IsValidGroup(list, out int gid);
                    bool ok = sat == Satisfaction.Correct && gid == group.ID;
                    string msg = $"{Label(kv.Value.items)}<br><small>mit: {string.Join(" | ", kv.Value.cards)}</small>";
                    if (!ok) msg += $"<br><b>Spiel erkennt: {Name(gid)} ({sat})</b>";
                    Add(dish, "Bestellungen", Short(group.name), ok ? "PASS" : "FAIL", msg, RenderGroup(group.ID, list, true));
                }
            }

            if (dish.ExtraOrderUnlocks != null)
                foreach (var u in dish.ExtraOrderUnlocks)
                    if (u.Ingredient != null)
                        Add(dish, "Bestellungen", Short(dish.name), "PASS",
                            $"Extra-Wunsch am Tisch: '{Short(u.Ingredient.name)}' zu '{(u.MenuItem != null ? Short(u.MenuItem.name) : "?")}'", RenderItem(u.Ingredient));
        }

        private static void CollectRequired(Unlock u, List<Dish> into, int depth)
        {
            if (u?.Requires == null || depth > 5) return;
            foreach (var r in u.Requires)
                if (r is Dish d && !into.Contains(d)) { into.Add(d); CollectRequired(d, into, depth + 1); }
        }

        // Bestellungen so erzeugen, wie das Spiel es tut: die echte Funktion (inkl. Patches von Mods)
        // oft aufrufen und alle verschiedenen Ergebnisse sammeln.
        private const int OrderSamples = 400;

        private List<List<int>> OrderConfigurations(ItemGroup group, HashSet<int> available, out string warning)
        {
            warning = null;
            var seen = new Dictionary<string, List<int>>();
            var state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(12345);
            try
            {
                for (int i = 0; i < OrderSamples; i++)
                {
                    var cfg = _data.ItemSetView.GetRandomConfiguration(group.ID, available);
                    var ids = new List<int>();
                    foreach (int id in cfg) ids.Add(id);
                    var key = string.Join(",", ids.OrderBy(x => x));
                    if (!seen.ContainsKey(key)) seen[key] = ids;
                }
            }
            finally { UnityEngine.Random.state = state; }

            foreach (var s in group.DerivedSets.Where(s => s.Min > 0 && s.RequiresUnlock))
                if (!s.Items.Any(i => i != null && available.Contains(i.ID)))
                    warning = $"Set mit {string.Join("/", s.Items.Where(i => i != null).Select(i => Short(i.name)))} braucht {s.Min}, freigeschaltet ist nichts davon";
            return seen.Values.ToList();
        }

        // ------------------------------------------------------------------ 3. Items / Geräte / Karten
        private void TestItem(Item item)
        {
            string subject = Short(item.name);
            if (item.Prefab == null) Add(item, "Items", subject, "FAIL", "kein Prefab", null);

            if (item.DerivedProcesses != null)
                foreach (var p in item.DerivedProcesses)
                {
                    if (p.Process == null) Add(item, "Items", subject, "FAIL", "Prozess ohne Prozess-Typ", null);
                    else if (p.Result == null) Add(item, "Items", subject, "FAIL", $"Prozess '{Short(p.Process.name)}' ohne Ergebnis", null);
                    else
                    {
                        Add(item, "Items", subject, "INFO",
                            $"{Short(p.Process.name)} {p.Duration:0.#} s{(p.IsBad ? " (schlecht)" : "")} → {Short(p.Result.name)}", null);
                        if (!_targetIds.Contains(p.Result.ID) && !p.IsBad)
                            Add(item, "Items", subject, "WARN",
                                $"Ergebnis '{Short(p.Result.name)}' ist ein Spiel-Item – kann in Spiel-Gerichten landen", null);
                    }
                }

            var auto = item.AutomaticItemProcess;
            if (auto.Process != null)
            {
                if (auto.Result == null) Add(item, "Items", subject, "FAIL", "automatischer Prozess ohne Ergebnis", null);
                else Add(item, "Items", subject, "INFO",
                    $"automatisch: {Short(auto.Process.name)} {auto.Duration:0.#} s → {Short(auto.Result.name)}" +
                    (auto.Process.IsCounteractedBy != null ? $" (gestoppt durch {Short(auto.Process.IsCounteractedBy.name)})" : ""), null);
            }

            if (item.SplitCount > 0)
            {
                if (item.SplitSubItem == null) Add(item, "Items", subject, "FAIL", $"{item.SplitCount} Portionen, aber kein Portions-Item", null);
                else if (!_targetIds.Contains(item.SplitSubItem.ID))
                    Add(item, "Items", subject, "WARN",
                        $"Portion ist das Spiel-Item '{Short(item.SplitSubItem.name)}' – kann in Spiel-Gerichten landen (vgl. Stir-Fry-Bug)", null);
                else Add(item, "Items", subject, "INFO", $"{item.SplitCount}× {Short(item.SplitSubItem.name)}", null);
            }

            // Vanilla-Gerichte, die unsere Items enthalten (sollte nie passieren).
            foreach (var foreign in _data.Get<ItemGroup>().Where(gr => !_targetIds.Contains(gr.ID)))
                if (foreign.DerivedSets.Any(s => s.Items.Any(i => i != null && i.ID == item.ID)))
                    Add(item, "Items", subject, "WARN", $"steckt im fremden Gericht '{foreign.name}'", null);

            if (!(item is ItemGroup) && item.Prefab != null)
                Add(item, "Items (Bilder)", subject, "PASS", subject, RenderItem(item));
        }

        private void TestAppliance(Appliance a)
        {
            string subject = Short(a.name);
            if (a.Prefab == null) { Add(a, "Geräte", subject, "FAIL", "kein Prefab", null); return; }
            string procs = a.Processes == null ? "" : string.Join(", ", a.Processes.Where(p => p.Process != null).Select(p => Short(p.Process.name)));
            Add(a, "Geräte", subject, "PASS", $"{subject}{(procs.Length > 0 ? " – " + procs : "")}, Preis {a.PurchaseCost}",
                Render(() => PrefabSnapshot.GetApplianceSnapshot(a.Prefab, true)));
        }

        private void TestDishCard(Dish d)
        {
            string text = $"<b>{Enc(SafeName(d))}</b> ({d.Type}, Schwierigkeit {d.Difficulty}, Kunden {d.CustomerMultiplier})<br>{Enc(SafeDesc(d))}" +
                          Requires(d);
            Add(d, "Karten", Short(d.name), "PASS", text, d.IconPrefab != null ? RenderPrefab(d.IconPrefab) : null);
            if (d.IconPrefab == null) Add(d, "Karten", Short(d.name), "WARN", "kein Icon", null);
            if (d.DisplayPrefab != null)
                Add(d, "Karten", Short(d.name), "PASS", "Anzeige (Hauptquartier)", RenderPrefab(d.DisplayPrefab));
        }

        private void TestUnlockCard(UnlockCard c)
        {
            string effects = c.Effects == null || c.Effects.Count == 0 ? "keine Effekte" : string.Join(", ", c.Effects.Select(e => e?.GetType().Name ?? "null"));
            Add(c, "Karten", Short(c.name), "PASS",
                $"<b>{Enc(SafeName(c))}</b> ({c.CardType}, {c.UnlockGroup})<br>{Enc(SafeDesc(c))}<br><small>{effects}</small>{Requires(c)}", null);
        }

        private string Requires(Unlock u) =>
            u.Requires != null && u.Requires.Count > 0
                ? $"<br><small>benötigt: {Enc(string.Join(", ", u.Requires.Where(r => r != null).Select(r => SafeName(r))))}</small>"
                : "";

        // ------------------------------------------------------------------ Rendern
        private string RenderGroup(int groupId, ItemList components, bool asOrder)
        {
            if (!_data.TryGet<Item>(groupId, out var item) || item.Prefab == null) return null;
            return Render(() =>
            {
                var go = UnityEngine.Object.Instantiate(item.Prefab);
                try
                {
                    go.SetActive(true);
                    go.transform.position = new Vector3(0f, -200f, 0f);
                    go.transform.rotation = Quaternion.LookRotation(new Vector3(1f, -1f, 1f), new Vector3(0f, 1f, 1f));
                    var view = go.GetComponentInChildren<IItemSpecificView>(true);
                    view?.PerformUpdate(groupId, components, asOrder);
                    return Snapshot.RenderToTexture(ImageSize, ImageSize, go, 0.5f, 0.5f).Snapshot;
                }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            });
        }

        private string RenderItem(Item item) => item?.Prefab == null ? null : RenderPrefab(item.Prefab);

        private string RenderPrefab(GameObject prefab) => Render(() =>
        {
            var go = UnityEngine.Object.Instantiate(prefab);
            try
            {
                go.SetActive(true);
                go.transform.position = new Vector3(0f, -200f, 0f);
                go.transform.rotation = Quaternion.LookRotation(new Vector3(1f, -1f, 1f), new Vector3(0f, 1f, 1f));
                return Snapshot.RenderToTexture(ImageSize, ImageSize, go, 0.5f, 0.5f).Snapshot;
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        });

        private string Render(Func<Texture2D> make)
        {
            float fade = Shader.GetGlobalFloat("_NightFade");
            try
            {
                Shader.SetGlobalFloat("_NightFade", 0f);
                var tex = make();
                if (tex == null) return null;
                string file = $"{++_imageCounter:0000}.png";
                File.WriteAllBytes(Path.Combine(ImgDir, file), tex.EncodeToPNG());
                UnityEngine.Object.Destroy(tex);
                return "img/" + file;
            }
            catch (Exception e)
            {
                if (_renderError == null) _renderError = e.ToString();
                return null;
            }
            finally { Shader.SetGlobalFloat("_NightFade", fade); }
        }

        // ------------------------------------------------------------------ Bericht
        private void Add(GameDataObject owner, string section, string subject, string status, string message, string image)
        {
            _entries.Add(new Entry { Mod = ModOf(owner), Section = section, Subject = subject, Status = status, Message = message, Image = image });
        }

        private void WriteReport()
        {
            int fail = _entries.Count(e => e.Status == "FAIL"), warn = _entries.Count(e => e.Status == "WARN");
            int pass = _entries.Count(e => e.Status == "PASS");

            var txt = new StringBuilder();
            txt.AppendLine($"ModSelfTest {DateTime.Now:yyyy-MM-dd HH:mm}  FAIL {fail}  WARN {warn}  PASS {pass}");
            foreach (var e in _entries.Where(e => e.Status == "FAIL" || e.Status == "WARN"))
                txt.AppendLine($"{e.Status} | {e.Mod} | {e.Section} | {e.Subject} | {StripTags(e.Message)}");
            if (_renderError != null) txt.AppendLine("RENDER-FEHLER: " + _renderError);
            File.WriteAllText(Path.Combine(OutDir, "summary.txt"), txt.ToString());

            var h = new StringBuilder();
            h.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>ModSelfTest</title><style>")
             .Append("body{font-family:Segoe UI,Arial,sans-serif;background:#1e1e1e;color:#ddd;margin:24px}")
             .Append("h1{margin:0 0 4px}h2{border-bottom:2px solid #444;padding-bottom:4px;margin-top:36px}h3{color:#aaa;margin:20px 0 8px}")
             .Append(".grid{display:flex;flex-wrap:wrap;gap:10px}.card{background:#2a2a2a;border-radius:8px;padding:8px;width:220px;font-size:13px;border-left:5px solid #555}")
             .Append(".card img{width:204px;height:204px;background:#3a3a3a;border-radius:4px;display:block;margin-bottom:6px}")
             .Append(".PASS{border-color:#3c9}.WARN{border-color:#fb3}.FAIL{border-color:#f45}.INFO{border-color:#58f}")
             .Append(".pill{display:inline-block;padding:2px 8px;border-radius:9px;margin-right:6px;font-weight:600}")
             .Append(".list .card{width:auto;max-width:900px}small{color:#999}")
             .Append("</style></head><body>");
            h.Append($"<h1>ModSelfTest</h1><div>{DateTime.Now:dd.MM.yyyy HH:mm} · {Enc(string.Join(", ", _prefixes))}</div><p>")
             .Append($"<span class='pill' style='background:#f45'>FAIL {fail}</span><span class='pill' style='background:#b80;'>WARN {warn}</span>")
             .Append($"<span class='pill' style='background:#286'>PASS {pass}</span></p>");
            if (_renderError != null) h.Append($"<p style='color:#f88'>Rendern fehlgeschlagen: <pre>{Enc(_renderError)}</pre></p>");

            if (fail + warn > 0)
            {
                h.Append("<h2>Probleme</h2><div class='list'>");
                foreach (var e in _entries.Where(e => e.Status == "FAIL" || e.Status == "WARN"))
                    h.Append($"<div class='card {e.Status}'><b>{e.Status}</b> · {Enc(e.Mod)} · {Enc(e.Section)} · <b>{Enc(e.Subject)}</b><br>{e.Message}</div>");
                h.Append("</div>");
            }

            string[] order = { "Karten", "Bestellungen", "Kombinationen (Bilder)", "Kombinationen", "Items (Bilder)", "Items", "Geräte", "Prozesse" };
            foreach (var mod in _entries.Select(e => e.Mod).Distinct().OrderBy(m => m))
            {
                h.Append($"<h2>{Enc(mod)}</h2>");
                var modEntries = _entries.Where(e => e.Mod == mod).ToList();
                foreach (var section in modEntries.Select(e => e.Section).Distinct().OrderBy(s => Array.IndexOf(order, s) < 0 ? 99 : Array.IndexOf(order, s)))
                {
                    var secEntries = modEntries.Where(e => e.Section == section).ToList();
                    bool withImages = secEntries.Any(e => e.Image != null);
                    h.Append($"<h3>{Enc(section)}</h3><div class='{(withImages ? "grid" : "grid list")}'>");
                    foreach (var e in secEntries)
                    {
                        h.Append($"<div class='card {e.Status}'>");
                        if (e.Image != null) h.Append($"<img src='{e.Image}' loading='lazy'>");
                        h.Append($"<small>{Enc(e.Subject)}</small><br>{e.Message}</div>");
                    }
                    h.Append("</div>");
                }
            }
            h.Append("</body></html>");
            File.WriteAllText(Path.Combine(OutDir, "report.html"), h.ToString(), Encoding.UTF8);
            Debug.LogWarning($"[ModSelfTest] Bericht: {OutDir} (FAIL {fail}, WARN {warn}, PASS {pass})");
        }

        // ------------------------------------------------------------------ Helfer
        private string ModOf(GameDataObject o)
        {
            if (o?.name == null) return "?";
            string n = o.name;
            foreach (var p in _prefixes) if (n.StartsWith(p)) n = n.Substring(p.Length);
            int dash = n.IndexOf(" - ", StringComparison.Ordinal);
            string mod = dash >= 0 ? n.Substring(0, dash) : n;
            if (mod.Length == 0) mod = o.name.Substring(0, Math.Max(0, o.name.IndexOf(" - ", StringComparison.Ordinal)));
            return mod.Length == 0 ? o.name : mod;
        }

        private string Short(string name)
        {
            if (name == null) return "?";
            int dash = name.IndexOf(" - ", StringComparison.Ordinal);
            return _prefixes.Any(p => name.StartsWith(p)) && dash > 0 ? name.Substring(dash + 3) : name;
        }

        private string Name(int id) => id == 0 ? "nichts" : _data.TryGet<GameDataObject>(id, out var o) ? Short(o.name) : id.ToString();
        private string Label(IEnumerable<int> ids) => Enc(string.Join(" + ", ids.Select(Name)));

        private static string SafeName(Unlock u) { try { return string.IsNullOrEmpty(u.Name) ? u.name : u.Name; } catch { return u.name; } }
        private static string SafeDesc(Unlock u) { try { return u.Description ?? ""; } catch { return ""; } }
        private static string Enc(string s) => WebUtility.HtmlEncode(s ?? "");
        private static string StripTags(string s) => System.Text.RegularExpressions.Regex.Replace(s ?? "", "<[^>]+>", " ");

        // Alle Teilmengen (nach Position, Duplikate erlaubt) mit Größe min..max.
        private static List<List<int>> Subsets(List<int> items, int min, int max)
        {
            var result = new List<List<int>>();
            int n = items.Count;
            if (n > 16) n = 16;
            for (int mask = 0; mask < (1 << n); mask++)
            {
                int bits = 0;
                for (int i = 0; i < n; i++) if ((mask & (1 << i)) != 0) bits++;
                if (bits < min || bits > max) continue;
                var s = new List<int>();
                for (int i = 0; i < n; i++) if ((mask & (1 << i)) != 0) s.Add(items[i]);
                result.Add(s);
            }
            return result;
        }

        // Kreuzprodukt (Zählwerk), höchstens cap Ergebnisse.
        private static IEnumerable<List<List<int>>> Product(List<List<List<int>>> options, int cap)
        {
            if (options.Any(o => o.Count == 0)) yield break;
            var idx = new int[options.Count];
            for (int produced = 0; produced < cap; produced++)
            {
                yield return options.Select((o, i) => o[idx[i]]).ToList();
                int d = options.Count - 1;
                while (d >= 0)
                {
                    if (++idx[d] < options[d].Count) break;
                    idx[d] = 0;
                    d--;
                }
                if (d < 0) yield break;
            }
        }
    }
}
