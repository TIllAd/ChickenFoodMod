using System.Collections.Generic;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace MyChickenMod.Custom.Rice
{
    // Topf + Reis + Wasser (roh). Auf dem Herd 10 s kochen -> RiceCookedPot.
    // Aufgebaut wie der Brokkoli-Topf aus dem Spiel.
    public class RicePot : CustomItemGroup
    {
        public override string UniqueNameID => "Rice - Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item DisposesTo => Ref(ItemReferences.Pot);

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 2, Max = 2, Items = new List<Item> { Ref(ItemReferences.Rice), Ref(ItemReferences.Water) } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { Ref(ItemReferences.Pot) } },
        };

        public override List<Item.ItemProcess> Processes => new List<Item.ItemProcess>
        {
            new Item.ItemProcess
            {
                Process = (Process)GDOUtils.GetExistingGDO(ProcessReferences.Cook),
                Duration = 10f,
                Result = (Item)GDOUtils.GetCustomGameDataObject<RiceCookedPot>().GameDataObject
            }
        };

        private GameObject _prefab, _rice, _water;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Rice Pot");
                    var pot = Ref(ItemReferences.Pot).Prefab;
                    PrefabUtil.CloneVisual(pot.transform.Find("Pot").gameObject, _prefab.transform, "Pot", Vector3.zero);
                    _water = PrefabUtil.CloneVisual(pot.transform.Find("Water").gameObject, _prefab.transform, "Water",
                        new Vector3(0f, 0.1f, 0f));
                    _water.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
                    _rice = PrefabUtil.CloneVisual(Ref(ItemReferences.Rice).Prefab, _prefab.transform, "Rice",
                        new Vector3(0f, 0.12f, 0f), 0.9f);
                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            var view = Prefab.GetComponent<ItemGroupView>();
            view.ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.Rice), GameObject = _rice },
                new ItemGroupView.ComponentGroup { Item = Ref(ItemReferences.Water), GameObject = _water },
            };
        }

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }

    // Gekochter Reistopf: 5 Portionen. Portion = der Reis-Haufen aus dem Spiel ("Rice - Container Cooked").
    public class RiceCookedPot : CustomItem
    {
        public override string UniqueNameID => "Rice - Cooked Pot";
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override Item SplitSubItem => Ref(ItemReferences.RiceContainerCooked);
        public override int SplitCount => 5;
        public override List<Item> SplitDepletedItems => new List<Item> { Ref(ItemReferences.Pot) };
        public override Item DisposesTo => Ref(ItemReferences.Pot);

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot("Rice Cooked Pot");
                    var pot = Ref(ItemReferences.Pot).Prefab;
                    PrefabUtil.CloneVisual(pot.transform.Find("Pot").gameObject, _prefab.transform, "Pot", Vector3.zero);
                    PrefabUtil.CloneVisual(Ref(ItemReferences.RiceContainerCooked).Prefab, _prefab.transform, "Rice",
                        new Vector3(0f, 0.15f, 0f), 1.4f);
                }
                return _prefab;
            }
        }

        private static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);
    }
}
