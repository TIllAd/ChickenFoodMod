using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KitchenData;
using KitchenLib.References;
using UnityEngine;

namespace MyChickenMod
{
    // Debug: schreibt die Spieldaten zu Reis, Brokkoli, Soßen usw. in eine Textdatei,
    // damit wir die echten Verarbeitungsschritte kennen statt zu raten.
    public static class DataDump
    {
        private static readonly string[] ItemKeywords = { "Rice", "Broccoli", "Sauce", "StirFry", "Wok", "Plate", "Pot", "Lemon", "Condiment" };
        private static readonly string[] DishKeywords = { "StirFry", "Steak", "Broccoli" };

        public static void Write(GameData gameData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ChickenFood data dump " + DateTime.Now);

            DumpRefs(sb, gameData, typeof(ItemReferences), ItemKeywords);
            DumpRefs(sb, gameData, typeof(DishReferences), DishKeywords);

            sb.AppendLine("\n##### Eigene GDOs");
            foreach (var g in gameData.Get<GameDataObject>().Where(o => o.name != null && o.name.StartsWith("com.till")))
                DumpObject(sb, g);

            sb.AppendLine("\n##### Items mit 'chicken' im Namen");
            foreach (var item in gameData.Get<Item>().Where(i => i.name.ToLower().Contains("chicken")))
                DumpObject(sb, item);

            var path = Path.Combine(Application.dataPath, "..", "Mods", "ChickenFood", "chickenfood_dump.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, sb.ToString());
            Mod.LogWarning("Data dump geschrieben: " + path);
        }

        private static void DumpRefs(StringBuilder sb, GameData gameData, Type refs, string[] keywords)
        {
            sb.AppendLine($"\n##### {refs.Name}");
            foreach (var prop in refs.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (!keywords.Any(k => prop.Name.Contains(k))) continue;
                int id = (int)prop.GetValue(null);
                if (!gameData.TryGet<GameDataObject>(id, out var gdo, true)) { sb.AppendLine($"\n== {prop.Name} ({id}): nicht gefunden"); continue; }
                sb.Append($"\n== {prop.Name} ");
                DumpObject(sb, gdo);
            }
        }

        private static void DumpObject(StringBuilder sb, GameDataObject gdo)
        {
            sb.AppendLine($"[{gdo.GetType().Name}] '{gdo.name}' id={gdo.ID}");
            foreach (var f in gdo.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object v;
                try { v = f.GetValue(gdo); } catch { continue; }
                var s = Fmt(v, 0);
                if (s == null) continue;
                sb.AppendLine($"   {f.Name} = {s}");
            }
            if (gdo is Item item && item.Prefab != null)
                sb.AppendLine("   Prefab-Hierarchie:\n" + Hierarchy(item.Prefab.transform, "      "));
        }

        private static string Fmt(object v, int depth)
        {
            if (v == null) return null;
            if (v is GameDataObject g) return $"'{g.name}'";
            if (v is UnityEngine.Object uo) return $"<{uo.GetType().Name} {uo.name}>";
            if (v is string str) return string.IsNullOrEmpty(str) ? null : $"\"{str}\"";
            if (v is IEnumerable en)
            {
                var parts = en.Cast<object>().Select(o => Fmt(o, depth + 1) ?? "null").ToList();
                return parts.Count == 0 ? null : "[" + string.Join(", ", parts) + "]";
            }
            var t = v.GetType();
            if (t.IsValueType && !t.IsPrimitive && !t.IsEnum && depth < 3)
            {
                var fs = t.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Select(f => { var s = Fmt(f.GetValue(v), depth + 1); return s == null ? null : $"{f.Name}: {s}"; })
                    .Where(s => s != null);
                return "{" + string.Join("; ", fs) + "}";
            }
            if (v is bool b && !b) return null;
            if (v is int i && i == 0) return null;
            if (v is float fl && fl == 0f) return null;
            return v.ToString();
        }

        private static string Hierarchy(Transform t, string indent)
        {
            var sb = new StringBuilder();
            var comps = string.Join(",", t.GetComponents<Component>().Where(c => c != null && !(c is Transform)).Select(c => c.GetType().Name));
            var r = t.GetComponent<MeshRenderer>();
            var mf = t.GetComponent<MeshFilter>();
            if (r != null)
                comps += " mats=" + string.Join("/", r.sharedMaterials.Select(m => m == null ? "null" : m.name))
                       + (mf != null && mf.sharedMesh != null ? $" mesh={mf.sharedMesh.name} size={mf.sharedMesh.bounds.size}" : "");
            sb.AppendLine($"{indent}{t.name} pos={t.localPosition} scale={t.localScale} [{comps}]");
            foreach (Transform c in t)
                sb.Append(Hierarchy(c, indent + "  "));
            return sb.ToString();
        }
    }
}
