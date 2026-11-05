using System;
using System.Collections.Generic;
using System.Linq;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using UnityEngine;

namespace MyChickenMod.Custom.Cosmetics
{
    // ---------------------------------------------------------------------------------
    // Kosmetik für Chicken & Rice.
    // Outfits = Outfits aus dem Spiel klonen und umfärben (wie der Schutzanzug in Breaking Plates).
    // Kopf-Teile (Kamm, Schnabel, Anstecker) = einfache Formen, die am Kopf-Knochen hängen.
    // Lagen/Größen sind Startwerte -> im Selbsttest-Bericht prüfen und hier nachjustieren.
    // ---------------------------------------------------------------------------------

    internal static class VanillaCosmetics
    {
        public const int ChefOutfit = -856773417;
        public const int TallChefHat = -545572929;
        public const int Bandana = -1859499757;
        public const int HandymanChef = -1776995498;
        public const int FrankensteinChef = -2013127355;
        public const int ButcherChef = -1627755214;
        public const int AstronautChef = 20631900;
        public const int DiverChef = -950136670;
        public const int MushroomChef = 1350673715;
        public const int SkeletonChef = -1058807536;
    }

    internal static class Palette
    {
        public static readonly Color FeatherWhite = new Color(0.97f, 0.96f, 0.92f);
        public static readonly Color CombRed = new Color(0.86f, 0.14f, 0.12f);
        public static readonly Color BeakYellow = new Color(0.98f, 0.72f, 0.12f);
        public static readonly Color SweatRed = new Color(0.85f, 0.15f, 0.18f);
        public static readonly Color GymGrey = new Color(0.32f, 0.34f, 0.38f);
        public static readonly Color GymBlack = new Color(0.1f, 0.1f, 0.11f);
        public static readonly Color BeltBrown = new Color(0.36f, 0.2f, 0.1f);
        public static readonly Color NeonPink = new Color(1f, 0.25f, 0.65f);
        public static readonly Color NeonTeal = new Color(0.1f, 0.85f, 0.8f);
        public static readonly Color ShirtWhite = new Color(0.96f, 0.96f, 0.94f);
        public static readonly Color BroccoliGreen = new Color(0.25f, 0.6f, 0.25f);
        public static readonly Color ShackRed = new Color(0.8f, 0.12f, 0.1f);
        public static readonly Color TankWhite = new Color(0.92f, 0.92f, 0.9f);
    }

    internal static class Mat
    {
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        // Gleiches Material-Setup wie die Spielobjekte ("Simple Flat").
        public static Material Flat(string name, Color colour, float shininess = 0.2f)
        {
            if (Cache.TryGetValue(name, out var m) && m != null) return m;
            m = new Material(Shader.Find("Simple Flat")) { name = "ChickenFood - " + name };
            colour.a = 0f;
            m.SetColor("_Color0", colour);
            m.SetFloat("_Shininess", shininess);
            m.SetFloat("_OverlayScale", 10f);
            Cache[name] = m;
            return m;
        }
    }

    internal static class Shapes
    {
        // Rotationskörper aus einem Profil (x = Radius, y = Höhe).
        public static Mesh Lathe(Vector2[] profile, int segments = 16)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float a = s / (float)segments * Mathf.PI * 2f;
                float c = Mathf.Cos(a), sn = Mathf.Sin(a);
                foreach (var p in profile) verts.Add(new Vector3(p.x * c, p.y, p.x * sn));
            }
            int n = profile.Length;
            for (int s = 0; s < segments; s++)
                for (int i = 0; i < n - 1; i++)
                {
                    int a0 = s * n + i, a1 = a0 + 1, b0 = (s + 1) * n + i, b1 = b0 + 1;
                    tris.AddRange(new[] { a0, b0, a1, a1, b0, b1 });
                    tris.AddRange(new[] { a0, a1, b0, a1, b1, b0 }); // beidseitig
                }
            var mesh = new Mesh { name = "ChickenFood Lathe" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh Sphere(float r, int rings = 8)
        {
            var p = new Vector2[rings + 1];
            for (int i = 0; i <= rings; i++)
            {
                float t = i / (float)rings * Mathf.PI;
                p[i] = new Vector2(Mathf.Sin(t) * r, -Mathf.Cos(t) * r);
            }
            return Lathe(p);
        }

        // Offener Ring (Stirnband), Achse = y.
        public static Mesh Ring(float inner, float outer, float height) =>
            Lathe(new[] { new Vector2(inner, 0f), new Vector2(outer, 0f), new Vector2(outer, height),
                          new Vector2(inner, height), new Vector2(inner, 0f) }, 24);

        // Kegel, Spitze zeigt nach +y.
        public static Mesh Cone(float r, float h) =>
            Lathe(new[] { new Vector2(0f, 0f), new Vector2(r, 0f), new Vector2(0f, h) });

        public static GameObject Create(string name, Transform parent, Mesh mesh, Material mat,
            Vector3 pos, Vector3? euler = null, Vector3? scale = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
            go.transform.localScale = scale ?? Vector3.one;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }
    }

    internal static class CosmeticUtil
    {
        // Spiel-Kosmetik klonen (inaktiv geparkt, wird nie selbst gerendert).
        public static GameObject Clone(int vanillaId, string name)
        {
            var source = ((PlayerCosmetic)GDOUtils.GetExistingGDO(vanillaId)).Visual;
            var clone = UnityEngine.Object.Instantiate(source, PrefabUtil.Holder, false);
            clone.name = name;
            return clone;
        }

        // Teile nach Namen umfärben oder entfernen (null = unverändert, Remove = weg).
        public static readonly Material Remove = null;
        public static void Restyle(GameObject root, Func<string, (bool remove, Material mat)> rule)
        {
            var comp = root.GetComponent<PlayerOutfitComponent>();
            foreach (var r in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (r == null) continue; // schon mit einem übergeordneten Teil gelöscht (z. B. Stern am Pilzhut)
                string n = r.gameObject.name;
                if (n == "Body") continue; // Spielerkörper nie anfassen
                var (remove, mat) = rule(n);
                if (remove)
                {
                    comp?.Renderers?.Remove(r);
                    comp?.Hats?.Remove(r);
                    UnityEngine.Object.DestroyImmediate(r.gameObject);
                    continue;
                }
                if (mat == null) continue;
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.sharedMaterials = mats;
            }
        }

        // Ein Teil (z. B. Gürtel) aus einem anderen Spiel-Outfit dazunehmen. Alle Outfits hängen am
        // selben Skelett, die Knochen werden beim Anziehen ohnehin neu zugewiesen.
        public static SkinnedMeshRenderer Borrow(GameObject root, int vanillaId, string partName, Material mat)
        {
            var source = ((PlayerCosmetic)GDOUtils.GetExistingGDO(vanillaId)).Visual;
            SkinnedMeshRenderer part = null;
            foreach (var r in source.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (r.gameObject.name == partName) { part = r; break; }
            if (part == null) { Mod.LogWarning($"[Cosmetics] Teil '{partName}' nicht gefunden"); return null; }
            var target = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var anchor = target.Length > 0 ? target[0].transform.parent : root.transform;
            var copy = UnityEngine.Object.Instantiate(part.gameObject, anchor, false);
            copy.name = partName;
            foreach (Transform child in copy.transform) UnityEngine.Object.DestroyImmediate(child.gameObject);
            var smr = copy.GetComponent<SkinnedMeshRenderer>();
            if (target.Length > 0) { smr.rootBone = target[0].rootBone; smr.bones = target[0].bones; }
            if (mat != null)
            {
                var mats = smr.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                smr.sharedMaterials = mats;
            }
            root.GetComponent<PlayerOutfitComponent>()?.Renderers?.Add(smr);
            return smr;
        }

        // Einzelne Materialien tauschen (z. B. nur das Weiß, das Band bleibt).
        public static void ReplaceMaterials(GameObject root, Func<Material, Material> rule)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null) continue;
                    var m = rule(mats[i]);
                    if (m != null) { mats[i] = m; changed = true; }
                }
                if (changed) r.sharedMaterials = mats;
            }
        }

        // Ärmel entfernen: alle Dreiecke weg, die überwiegend an Oberarm/Unterarm/Hand hängen.
        public static void CutSleeves(SkinnedMeshRenderer r)
        {
            if (r == null || r.sharedMesh == null) return;
            if (r.bones == null || r.bones.Length == 0)
            {
                Mod.LogWarning($"[Cosmetics] CutSleeves {r.name}: keine Knochen am Prefab -> nichts geschnitten");
                return;
            }
            var mesh = UnityEngine.Object.Instantiate(r.sharedMesh);
            mesh.name = r.sharedMesh.name + " (sleeveless)";
            var bones = r.bones;
            bool IsArm(int i) => i >= 0 && i < bones.Length && bones[i] != null &&
                (bones[i].name.StartsWith("Shoulder") || bones[i].name.StartsWith("UpperArm") || bones[i].name.StartsWith("LowerArm") || bones[i].name.StartsWith("Hand"));
            var w = mesh.boneWeights;
            var arm = new bool[w.Length];
            for (int i = 0; i < w.Length; i++)
            {
                float armWeight = (IsArm(w[i].boneIndex0) ? w[i].weight0 : 0f) + (IsArm(w[i].boneIndex1) ? w[i].weight1 : 0f)
                                + (IsArm(w[i].boneIndex2) ? w[i].weight2 : 0f) + (IsArm(w[i].boneIndex3) ? w[i].weight3 : 0f);
                arm[i] = armWeight > 0.5f;
            }
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                var tris = mesh.GetTriangles(sub);
                var keep = new List<int>(tris.Length);
                for (int t = 0; t < tris.Length; t += 3)
                {
                    int armCount = (arm[tris[t]] ? 1 : 0) + (arm[tris[t + 1]] ? 1 : 0) + (arm[tris[t + 2]] ? 1 : 0);
                    if (armCount >= 2) continue;
                    keep.Add(tris[t]); keep.Add(tris[t + 1]); keep.Add(tris[t + 2]);
                }
                mesh.SetTriangles(keep, sub);
            }
            int armVerts = arm.Count(a => a);
            Mod.LogWarning($"[Cosmetics] CutSleeves {r.name}: {bones.Length} Knochen ({string.Join(",", bones.Where(b => b != null).Select(b => b.name))}), " +
                           $"{armVerts}/{arm.Length} Arm-Vertices, Dreiecke {r.sharedMesh.triangles.Length / 3} -> {mesh.triangles.Length / 3}");
            r.sharedMesh = mesh;
        }

        // Container, der beim Anziehen an den Kopf-Knochen gehängt wird.
        public static Transform HeadAttachment(GameObject root)
        {
            var comp = root.GetComponent<PlayerOutfitComponent>() ?? root.AddComponent<PlayerOutfitComponent>();
            if (comp.AttachToHeadBone == null)
            {
                var holder = new GameObject("ChickenFood Head");
                holder.transform.SetParent(root.transform, false);
                comp.AttachToHeadBone = holder;
            }
            return comp.AttachToHeadBone.transform;
        }

        // Hühnerkopf: Kamm oben, Schnabel und Kehllappen vorn (Kopf schaut nach +z).
        public static void AddChickenHead(Transform head, float scale = 1f)
        {
            var g = new GameObject("Chicken Head");
            g.transform.SetParent(head, false);
            g.transform.localScale = Vector3.one * scale;
            var red = Mat.Flat("Comb Red", Palette.CombRed);
            var yellow = Mat.Flat("Beak Yellow", Palette.BeakYellow, 0.3f);
            var flat = new Vector3(0.45f, 1f, 1f);
            Shapes.Create("Comb Back", g.transform, Shapes.Sphere(0.045f), red, new Vector3(0f, 0.29f, -0.06f), null, flat);
            Shapes.Create("Comb Mid", g.transform, Shapes.Sphere(0.06f), red, new Vector3(0f, 0.32f, 0f), null, flat);
            Shapes.Create("Comb Front", g.transform, Shapes.Sphere(0.045f), red, new Vector3(0f, 0.29f, 0.06f), null, flat);
            Shapes.Create("Beak", g.transform, Shapes.Cone(0.05f, 0.1f), yellow, new Vector3(0f, 0.13f, 0.2f), new Vector3(90f, 0f, 0f));
            Shapes.Create("Wattle", g.transform, Shapes.Sphere(0.035f), red, new Vector3(0f, 0.06f, 0.2f), null, new Vector3(0.6f, 1.3f, 0.6f));
        }
    }

    // ================================================================== Hüte
    public class SweatbandHat : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Sweatband";
        public override CosmeticType CosmeticType => CosmeticType.Hat;
        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Sweatband", Description = "No pain, no grain." })
        };

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        // Offener Ring um die Stirn (Startwerte: Höhe/Radius am Kopf).
        private static GameObject Make()
        {
            var root = PrefabUtil.CreateRoot("ChickenFood Sweatband");
            var comp = root.AddComponent<PlayerOutfitComponent>();
            comp.Renderers = new List<SkinnedMeshRenderer>();
            comp.Hats = new List<Renderer>();
            var head = CosmeticUtil.HeadAttachment(root);
            var band = Shapes.Create("Band", head, Shapes.Ring(0.225f, 0.25f, 0.07f), Mat.Flat("Sweat Red", Palette.SweatRed, 0.05f),
                new Vector3(0f, 0.15f, 0.01f), new Vector3(-8f, 0f, 0f));
            Shapes.Create("Stripe", band.transform, Shapes.Ring(0.249f, 0.253f, 0.02f), Mat.Flat("Shirt White", Palette.ShirtWhite),
                new Vector3(0f, 0.025f, 0f));
            return root;
        }
    }

    public class ChickenChefHat : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Chicken Chef Hat";
        public override CosmeticType CosmeticType => CosmeticType.Hat;
        public override bool BlockHats => true;
        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Chicken Chef Hat", Description = "Head chef of the coop." })
        };

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            var root = CosmeticUtil.Clone(VanillaCosmetics.TallChefHat, "ChickenFood Chicken Chef Hat");
            // Original ist eher grau ("Clothing White") -> reines Weiß, damit er nicht wie ein Eimer wirkt.
            CosmeticUtil.ReplaceMaterials(root, m => m.name.Contains("Clothing White") ? Mat.Flat("Hat White", new Color(1f, 1f, 1f), 0.1f) : null);
            // Kleiner Hühnchen-Anstecker vorn an der Mütze.
            var pin = new GameObject("Chicken Pin");
            pin.transform.SetParent(CosmeticUtil.HeadAttachment(root), false);
            pin.transform.localPosition = new Vector3(0f, 0.36f, 0.16f);
            var white = Mat.Flat("Feather White", Palette.FeatherWhite);
            Shapes.Create("Pin Body", pin.transform, Shapes.Sphere(0.04f), white, Vector3.zero, null, new Vector3(1f, 0.9f, 0.6f));
            Shapes.Create("Pin Head", pin.transform, Shapes.Sphere(0.025f), white, new Vector3(0.03f, 0.035f, 0f));
            Shapes.Create("Pin Comb", pin.transform, Shapes.Sphere(0.012f), Mat.Flat("Comb Red", Palette.CombRed), new Vector3(0.03f, 0.062f, 0f));
            Shapes.Create("Pin Beak", pin.transform, Shapes.Cone(0.01f, 0.02f), Mat.Flat("Beak Yellow", Palette.BeakYellow, 0.3f),
                new Vector3(0.053f, 0.035f, 0f), new Vector3(0f, 0f, -90f));
            return root;
        }
    }

    // ================================================================== Outfits
    public class ChickenCostume : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Chicken Costume";
        public override CosmeticType CosmeticType => CosmeticType.Outfit;
        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Chicken Costume", Description = "Best worn with the rooster hat." })
        };

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            // Ganzkörperanzug: Astronauten-Outfit ohne Helm, weiß, gelbe Stiefel = Hühnerfüße.
            var root = CosmeticUtil.Clone(VanillaCosmetics.AstronautChef, "ChickenFood Chicken Costume");
            var white = Mat.Flat("Feather White", Palette.FeatherWhite);
            var yellow = Mat.Flat("Beak Yellow", Palette.BeakYellow, 0.3f);
            CosmeticUtil.Restyle(root, n =>
                n.Contains("Helmet") || n.Contains("Tank") || n.Contains("Pack") ? (true, null) :
                n.Contains("Boots") ? (false, yellow) :
                (false, white));
            return root;
        }
    }

    public class GymOutfit : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Gym Outfit";
        public override CosmeticType CosmeticType => CosmeticType.Outfit;
        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Gym Outfit", Description = "Tank top and lifting belt. Never skip leg day." })
        };

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            // Vorlage: Pilz-Kostüm = ärmelloses Oberteil + kurze Hose. Umhang/Hut weg, Gürtel vom Handwerker dazu.
            var root = CosmeticUtil.Clone(VanillaCosmetics.MushroomChef, "ChickenFood Gym Outfit");
            CosmeticUtil.Restyle(root, n =>
                n.Contains("Cape") || n.Contains("Hat") ? (true, null) :
                n.Contains("Trousers") ? (false, Mat.Flat("Gym Black", Palette.GymBlack)) :
                (false, Mat.Flat("Tank White", Palette.TankWhite)));
            CosmeticUtil.Borrow(root, VanillaCosmetics.HandymanChef, "Handyman Chef Belt", Mat.Flat("Belt Brown", Palette.BeltBrown, 0.3f));
            return root;
        }
    }

    public class RetroTracksuit : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Retro Tracksuit";
        public override CosmeticType CosmeticType => CosmeticType.Outfit;
        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Retro Tracksuit", Description = "Totally radical cardio." })
        };

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            // Vorlage: Skelett-Kostüm = Ganzkörperanzug; Knochen sind nur Textur und verschwinden beim Umfärben.
            var root = CosmeticUtil.Clone(VanillaCosmetics.SkeletonChef, "ChickenFood Retro Tracksuit");
            CosmeticUtil.Restyle(root, n =>
                n.Contains("Hat") || n.Contains("Skull") ? (true, null) :
                (false, Mat.Flat("Neon Pink", Palette.NeonPink, 0.4f)));
            return root;
        }
    }

    public class ChickenShackApron : CustomPlayerCosmetic
    {
        public override string UniqueNameID => "Chicken Shack Apron";
        public override CosmeticType CosmeticType => CosmeticType.Outfit;
        public override List<(Locale, CosmeticInfo)> InfoList => new List<(Locale, CosmeticInfo)>
        {
            (Locale.English, new CosmeticInfo { Name = "Chicken Shack Apron", Description = "Golden, crispy, served hot." })
        };

        private GameObject _visual;
        public override GameObject Visual => _visual ??= Make();

        private static GameObject Make()
        {
            var root = CosmeticUtil.Clone(VanillaCosmetics.ButcherChef, "ChickenFood Chicken Shack Apron");
            CosmeticUtil.Restyle(root, n =>
                n.Contains("Hat") ? (true, null) :
                n.Contains("Apron") ? (false, Mat.Flat("Shack Red", Palette.ShackRed)) :
                (false, Mat.Flat("Shirt White", Palette.ShirtWhite)));
            return root;
        }
    }
}
