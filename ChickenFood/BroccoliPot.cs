using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace MyChickenMod.Custom.Broccoli
{
    // Eigener Brokkoli-Topf für das Topping: GESCHNITTENER Brokkoli + Wasser.
    // (Ganzer Brokkoli + Wasser bleibt der Topf aus dem Spiel -> Beilage.)
    public class ChoppedBroccoliPot : CustomItemGroup
    {
        public override string UniqueNameID => "Broccoli - Chopped Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Ref(ItemReferences.Pot);

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 2, Max = 2, Items = new List<Item> { Ref(ItemReferences.BroccoliChopped), Ref(ItemReferences.Water) } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { Ref(ItemReferences.Pot) } },
        };

        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
                Duration = 8f,
                Result = (Item)GDOUtils.GetCustomGameDataObject<SteamedBroccoliPot>().GameDataObject
            }
        };

        private GameObject _prefab, _broccoli, _water;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Chopped Broccoli Pot");
                    var pot = Ref(ItemReferences.Pot).Prefab;
                    PrefabUtil.CloneVisual(pot.transform.Find("Pot").gameObject, _prefab.transform, "Pot", Vector3.zero);
                    _water = PrefabUtil.CloneVisual(pot.transform.Find("Water").gameObject, _prefab.transform, "Water", new Vector3(0f, 0.1f, 0f));
                    // Wasser etwas schmaler und flacher als im Original, damit es nicht unten aus dem Topf schaut.
                    _water.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                    _water.transform.localScale = new Vector3(0.52f, 0.05f, 0.52f);
                    _broccoli = PrefabUtil.CloneVisual(Ref(ItemReferences.BroccoliChopped).Prefab, _prefab.transform, "Broccoli",
                        new Vector3(0.05f, 0.02f, 0f), 1f);
                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.BroccoliChopped), GameObject = _broccoli },
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.Water), GameObject = _water },
            };
        }

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    // Gekochter Topf: 5 Portionen gegarter Brokkoli, wird beim Entnehmen leerer.
    public class SteamedBroccoliPot : CustomItem
    {
        public override string UniqueNameID => "Broccoli - Steamed Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        // Portion = gegarter geschnittener Brokkoli aus dem Spiel (wie beim Wok-Gericht), keine Beilage.
        public override Item SplitSubItem => Ref(ItemReferences.BroccoliChoppedContainerCooked);
        public override int SplitCount => 5;
        public override List<Item> SplitDepletedItems => new List<Item> { Ref(ItemReferences.Pot) };
        public override Item DisposesTo => Ref(ItemReferences.Pot);
        public override string ColourBlindTag => "Br";

        private static readonly Vector3 Full = new Vector3(0.05f, 0.08f, 0f);
        private static readonly Vector3 Empty = new Vector3(0.05f, -0.1f, 0f);

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Steamed Broccoli Pot");
                    var pot = Ref(ItemReferences.Pot).Prefab;
                    PrefabUtil.CloneVisual(pot.transform.Find("Pot").gameObject, _prefab.transform, "Pot", Vector3.zero);
                    var broccoli = PrefabUtil.CloneVisual(Ref(ItemReferences.BroccoliChopped).Prefab, _prefab.transform, "Broccoli", Full, 1.3f);
                    var view = _prefab.AddComponent<PositionSplittableView>();
                    ReflectionUtils.GetField<PositionSplittableView>("FullPosition").SetValue(view, Full);
                    ReflectionUtils.GetField<PositionSplittableView>("EmptyPosition").SetValue(view, Empty);
                    ReflectionUtils.GetField<PositionSplittableView>("Objects").SetValue(view, new List<GameObject> { broccoli });
                }
                return _prefab;
            }
        }

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }
}
