using System.Collections.Generic;
using IngredientLib.Ingredient.Items;
using Kitchen;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace MyChickenMod.Custom.Chicken
{
    // Gemeinsame Basis für "Hähnchen mit Reis" (ganzes Stück oder geschnetzelt).
    public abstract class ChickenPlatedBase : CustomItemGroup
    {
        protected abstract CustomGameDataObject ChickenCustom { get; }
        protected Item ChickenItem => (Item)ChickenCustom.GameDataObject;
        protected abstract string PrefabName { get; }

        protected static Item Ref(int id) => (Item)GDOUtils.GetExistingGDO(id);

        public override Item DirtiesTo => Ref(ItemReferences.PlateDirty);
        public override Item DisposesTo => Ref(ItemReferences.Plate);
        public override ItemValue ItemValue => ItemValue.Medium;
        public override bool CanContainSide => true;

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { Ref(ItemReferences.Plate) } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, Items = new List<Item> { ChickenItem } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, Items = new List<Item> { Ref(ItemReferences.RiceContainerCooked) } },
            // Toppings: nur mit freigeschalteter Karte, bis zu zwei pro Teller.
            new ItemGroup.ItemSet { Min = 0, Max = 2, RequiresUnlock = true,
                Items = new List<Item> { Ref(ItemReferences.BroccoliServing), Ref(ItemReferences.LemonSliced) } },
        };

        public override List<ItemGroup.ItemReward> Rewards => new List<ItemGroup.ItemReward>
        {
            new ItemGroup.ItemReward { Item = Ref(ItemReferences.BroccoliServing), RewardAmount = 1 },
            new ItemGroup.ItemReward { Item = Ref(ItemReferences.LemonSliced), RewardAmount = 1 },
        };

        private GameObject _prefab;
        public override GameObject Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = PrefabUtil.CreateRoot(PrefabName);
                    PrefabUtil.CloneVisual(Ref(ItemReferences.Plate).Prefab, _prefab.transform, "Plate", Vector3.zero);
                    // KitchenLib hängt Beilagen an ein Kind mit genau diesem Namen (wie beim Steak).
                    var side = new GameObject("Side Container");
                    side.transform.SetParent(_prefab.transform, false);
                    side.transform.localPosition = new Vector3(0f, 0f, -0.2f);
                }
                return _prefab;
            }
        }

        public override void OnRegister(ItemGroup gdo)
        {
            var chickenGO = PrefabUtil.AddChicken(Prefab.transform, ChickenCustom);
            var rice = Ref(ItemReferences.RiceContainerCooked);
            var riceGO = PrefabUtil.CloneVisual(rice.Prefab, Prefab.transform, "Rice", PrefabUtil.RicePos, 1.1f);
            // Topping = gekochte Brokkoli-Portion aus dem Topf; Optik: die geschnittenen Röschen.
            var broccoli = Ref(ItemReferences.BroccoliServing);
            var broccoliGO = PrefabUtil.CloneVisual(Ref(ItemReferences.BroccoliChopped).Prefab, Prefab.transform, "Broccoli", new Vector3(-0.1f, 0.02f, -0.12f), 0.8f);
            var lemon = Ref(ItemReferences.LemonSliced);
            var lemonGO = PrefabUtil.CloneVisual(lemon.Prefab, Prefab.transform, "Lemon", new Vector3(0.14f, 0.06f, 0.12f), 0.7f);

            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                new ItemGroupView.ComponentGroup { Item = ChickenItem, GameObject = chickenGO },
                new ItemGroupView.ComponentGroup { Item = rice, GameObject = riceGO },
                new ItemGroupView.ComponentGroup { Item = broccoli, GameObject = broccoliGO },
                new ItemGroupView.ComponentGroup { Item = lemon, GameObject = lemonGO },
            };
        }
    }

    // Name bleibt "MyChicken_Plated", damit alte Spielstände nicht kaputtgehen.
    public class MyChicken_Plated : ChickenPlatedBase
    {
        public override string UniqueNameID => nameof(MyChicken_Plated);
        protected override string PrefabName => "Chicken Plated";
        protected override CustomGameDataObject ChickenCustom => GDOUtils.GetCustomGameDataObject<CookedChicken>();
    }

    public class ShreddedChicken_Plated : ChickenPlatedBase
    {
        public override string UniqueNameID => "ShreddedChicken_Plated";
        protected override string PrefabName => "Shredded Chicken Plated";
        protected override CustomGameDataObject ChickenCustom => GDOUtils.GetCustomGameDataObject<ShreddedChicken>();
    }
}
