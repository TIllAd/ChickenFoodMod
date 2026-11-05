using System.Collections.Generic;
using IngredientLib.Ingredient.Items;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace MyChickenMod
{
    // Baut Prefabs zur Laufzeit aus vorhandenen Spielmodellen zusammen,
    // damit wir für einfache Gerichte gar kein Unity/Blender brauchen.
    public static class PrefabUtil
    {
        // Lage auf dem Teller (überall gleich, damit Icon = Spiel).
        public static readonly Vector3 ChickenPos = new Vector3(0.1f, 0.09f, -0.06f);
        public static readonly Quaternion ChickenRot = Quaternion.Euler(0f, 60f, 0f);
        public const float ChickenScale = 0.75f;
        // Zweite Portion (Bulking Season): vorne auf dem Teller, neben der ersten.
        public static readonly Vector3 Chicken2Pos = new Vector3(0.04f, 0.12f, 0.0f); // Startwert, im Spiel nachjustieren
        public static readonly Vector3 RicePos = new Vector3(-0.1f, 0f, 0.08f);

        private static GameObject _holder;

        // Inaktiver Container: Prefabs darin werden nie selbst gerendert.
        public static Transform Holder
        {
            get
            {
                if (_holder == null)
                {
                    _holder = new GameObject("ChickenFood Prefabs");
                    _holder.SetActive(false);
                    Object.DontDestroyOnLoad(_holder);
                }
                return _holder.transform;
            }
        }

        public static GameObject CreateRoot(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Holder, false);
            return go;
        }

        // Kopiert nur die Optik (Meshes/Materialien) eines Prefabs, ohne dessen Logik-Komponenten.
        public static GameObject CloneVisual(GameObject source, Transform parent, string name,
            Vector3 localPosition, float scale = 1f)
        {
            var clone = Object.Instantiate(source, parent, false);
            clone.name = name;
            foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true))
                Object.DestroyImmediate(mb);
            foreach (var col in clone.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(col);
            clone.transform.localPosition = localPosition;
            // Bei Teilstücken eines Prefabs (z.B. Topf, Wasser) Drehung und Größe des Originals behalten,
            // sonst liegen sie schief. Ganze Item-Prefabs (Wurzel) werden wie bisher neutral platziert.
            bool isChildPart = source.transform.parent != null;
            clone.transform.localRotation = isChildPart ? source.transform.localRotation : Quaternion.identity;
            clone.transform.localScale = (isChildPart ? source.transform.localScale : Vector3.one) * scale;
            clone.SetActive(true);
            return clone;
        }

        // IngredientLib setzt seine Materialien erst bei der Registrierung.
        // Kopien, die vorher entstehen, wären sonst pink -> später echtes Material übernehmen.
        private static readonly List<(GameObject clone, CustomGameDataObject source)> MaterialFixups =
            new List<(GameObject, CustomGameDataObject)>();

        public static GameObject AddChicken(Transform parent, CustomGameDataObject chicken)
        {
            var prefab = ((CustomItem<Item>)chicken).Prefab;
            var go = CloneVisual(prefab, parent, "Chicken", ChickenPos, ChickenScale);
            go.transform.localRotation = ChickenRot;
            MaterialFixups.Add((go, chicken));
            return go;
        }

        // Nach der Registrierung aller Mods aufrufen.
        public static void FixMaterials()
        {
            foreach (var (clone, source) in MaterialFixups)
            {
                var item = source.GameDataObject as Item;
                if (item == null || item.Prefab == null || clone == null) continue;
                var src = item.Prefab.GetComponentsInChildren<MeshRenderer>(true);
                var dst = clone.GetComponentsInChildren<MeshRenderer>(true);
                for (int i = 0; i < Mathf.Min(src.Length, dst.Length); i++)
                    dst[i].sharedMaterials = src[i].sharedMaterials;
            }
        }

        // Teller mit Hähnchen und Reis: für Icons und Anzeige im Hauptquartier.
        public static GameObject BuildPlate(string name, CustomGameDataObject chicken,
            bool lettuceInsteadOfRice = false, bool doubleChicken = false)
        {
            var root = CreateRoot(name);
            CloneVisual(((Item)GDOUtils.GetExistingGDO(ItemReferences.Plate)).Prefab, root.transform, "Plate", Vector3.zero);
            AddChicken(root.transform, chicken);
            if (doubleChicken)
                AddChicken(root.transform, chicken).transform.localPosition = Chicken2Pos;
            if (lettuceInsteadOfRice)
                CloneVisual(((Item)GDOUtils.GetExistingGDO(ItemReferences.LettuceChopped)).Prefab, root.transform, "Lettuce", RicePos, 1f);
            else
                CloneVisual(((Item)GDOUtils.GetExistingGDO(ItemReferences.RiceContainerCooked)).Prefab, root.transform, "Rice", RicePos, 1.1f);
            return root;
        }

        public static GameObject BuildPlateWithChicken(string name) =>
            BuildPlate(name, GDOUtils.GetCustomGameDataObject<CookedChicken>());

        // Icon für Topping-Karten: einfach das Item selbst.
        public static GameObject BuildItemIcon(string name, int itemId)
        {
            var root = CreateRoot(name);
            CloneVisual(((Item)GDOUtils.GetExistingGDO(itemId)).Prefab, root.transform, "Item", Vector3.zero);
            return root;
        }
    }
}
