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
                    // Wasser etwas schmaler und flacher als im Original, damit es nicht unten aus dem Topf schaut.
                    _water.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                    _water.transform.localScale = new Vector3(0.52f, 0.05f, 0.52f);
                    _rice = PrefabUtil.CloneVisual(Ref(ItemReferences.Rice).Prefab, _prefab.transform, "Rice",
                        new Vector3(0f, 0.12f, 0f), 0.9f);
                    RemoveBowl(_rice);
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

        // Das Reis-Modell des Spiels bringt eine Holzschüssel mit (Kind "Cylinder", Material "Wood 1").
        // Im Topf ergibt die keinen Sinn -> entfernen.
        internal static void RemoveBowl(GameObject rice)
        {
            var bowl = rice.transform.Find("Cylinder");
            if (bowl != null) Object.DestroyImmediate(bowl.gameObject);
        }
    }

    // Gekochter Reistopf: 5 Portionen. Portion = der Reis-Haufen aus dem Spiel ("Rice - Container Cooked").
    // Ohne Reis-Karten bleibt es bei diesem Item; mit "Burnt Rice"/"Cold Rice" tauscht RicePotStateSystem
    // den Topf gegen eine Variante, die anbrennt bzw. abkühlt (siehe RiceStates.cs).
    public class RiceCookedPot : RiceCookedPotBase
    {
        public override string UniqueNameID => "Rice - Cooked Pot";
    }
}
