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
        protected static Item SteamedBroccoliItem => Ref(ItemReferences.BroccoliChoppedContainerCooked);

        public override Item DirtiesTo => Ref(ItemReferences.PlateDirty);
        public override Item DisposesTo => Ref(ItemReferences.Plate);
        public override ItemValue ItemValue => ItemValue.Medium;
        public override bool CanContainSide => true;

        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>
        {
            new ItemGroup.ItemSet { Min = 1, Max = 1, IsMandatory = true, Items = new List<Item> { Ref(ItemReferences.Plate) } },
            new ItemGroup.ItemSet { Min = 1, Max = 1, Items = new List<Item> { ChickenItem } },
            // Bulking Season: zweite Portion Chicken, nur mit Karte (dann ~50 % der Bestellungen).
            new ItemGroup.ItemSet { Min = 0, Max = 1, RequiresUnlock = true, Items = new List<Item> { ChickenItem } },
            // Beilage: Reis (vom Grundgericht freigeschaltet) oder - mit Karte "Low Carb" - Salat statt Reis.
            // Gleiches Muster wie die Soßen bei FryThat: Pflichtauswahl aus den freigeschalteten Zutaten.
            new ItemGroup.ItemSet { Min = 1, Max = 1, RequiresUnlock = true,
                Items = new List<Item> { Ref(ItemReferences.RiceContainerCooked), Ref(ItemReferences.LettuceChopped) } },
            // Toppings: nur mit freigeschalteter Karte. Getrennte Sets -> jedes Topping
            // wird unabhängig mit 50 % bestellt (0 oder 1, gleich verteilt).
            new ItemGroup.ItemSet { Min = 0, Max = 1, RequiresUnlock = true,
                Items = new List<Item> { SteamedBroccoliItem } },
            new ItemGroup.ItemSet { Min = 0, Max = 1, RequiresUnlock = true,
                Items = new List<Item> { Ref(ItemReferences.LemonSliced) } },
        };

        // Farbenblind-Modus: Buchstaben für alles, was sich nur über die Farbe unterscheidet.
        public override List<ItemGroupView.ColourBlindLabel> Labels => new List<ItemGroupView.ColourBlindLabel>
        {
            new ItemGroupView.ColourBlindLabel { Item = SteamedBroccoliItem, Text = "B" },
            new ItemGroupView.ColourBlindLabel { Item = Ref(ItemReferences.LemonSliced), Text = "Le" },
            new ItemGroupView.ColourBlindLabel { Item = Ref(ItemReferences.LettuceChopped), Text = "Sa" },
        };

        public override List<ItemGroup.ItemReward> Rewards => new List<ItemGroup.ItemReward>
        {
            new ItemGroup.ItemReward { Item = SteamedBroccoliItem, RewardAmount = 1 },
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
            // Zweites Stück für Bulking Season (wird nur bei 2 Portionen angezeigt).
            var chicken2GO = PrefabUtil.AddChicken(Prefab.transform, ChickenCustom);
            chicken2GO.name = "Chicken 2";
            chicken2GO.transform.localPosition = PrefabUtil.Chicken2Pos;
            var rice = Ref(ItemReferences.RiceContainerCooked);
            var riceGO = PrefabUtil.CloneVisual(rice.Prefab, Prefab.transform, "Rice", PrefabUtil.RicePos, 1.1f);
            // Topping = Portion aus unserem Brokkoli-Topf; Optik: die geschnittenen Röschen.
            var broccoli = SteamedBroccoliItem;
            var broccoliGO = PrefabUtil.CloneVisual(Ref(ItemReferences.BroccoliChopped).Prefab, Prefab.transform, "Broccoli", new Vector3(-0.1f, 0.02f, -0.12f), 0.8f);
            var lemon = Ref(ItemReferences.LemonSliced);
            var lettuce = Ref(ItemReferences.LettuceChopped);
            var lettuceGO = PrefabUtil.CloneVisual(lettuce.Prefab, Prefab.transform, "Lettuce", PrefabUtil.RicePos, 1f);
            var lemonGO = PrefabUtil.CloneVisual(lemon.Prefab, Prefab.transform, "Lemon", new Vector3(0.14f, 0.06f, 0.12f), 0.7f);

            Prefab.GetComponent<ItemGroupView>().ComponentGroups = new List<ItemGroupView.ComponentGroup>
            {
                // Objects ohne DrawAll: 1 Portion -> erstes Stück, 2 Portionen -> beide.
                new ItemGroupView.ComponentGroup { Item = ChickenItem, Objects = new List<GameObject> { chickenGO, chicken2GO } },
                new ItemGroupView.ComponentGroup { Item = rice, GameObject = riceGO },
                new ItemGroupView.ComponentGroup { Item = broccoli, GameObject = broccoliGO },
                new ItemGroupView.ComponentGroup { Item = lemon, GameObject = lemonGO },
                new ItemGroupView.ComponentGroup { Item = lettuce, GameObject = lettuceGO },
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

        public override List<ItemGroupView.ColourBlindLabel> Labels
        {
            get
            {
                var labels = base.Labels;
                labels.Insert(0, new ItemGroupView.ColourBlindLabel { Item = ChickenItem, Text = "Sh" });
                return labels;
            }
        }
        protected override CustomGameDataObject ChickenCustom => GDOUtils.GetCustomGameDataObject<ShreddedChicken>();
    }
}
