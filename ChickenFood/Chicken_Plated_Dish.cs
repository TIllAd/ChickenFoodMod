using System.Collections.Generic;
using IngredientLib.Ingredient.Items;
using KitchenData;
using KitchenLib.Customs;
using KitchenLib.References;
using KitchenLib.Utils;
using UnityEngine;

namespace MyChickenMod.Custom.Chicken
{
    internal static class Refs
    {
        public static Item Item(int id) => (Item)GDOUtils.GetExistingGDO(id);
        public static Process Process(int id) => (Process)GDOUtils.GetExistingGDO(id);
        public static Item Custom<T>() => (Item)GDOUtils.GetCustomGameDataObject<T>().GameDataObject;
        public static Dish BaseDish => (Dish)GDOUtils.GetCustomGameDataObject<MyChicken_Plated_Dish>().GameDataObject;

        public static (Locale, UnlockInfo) Info(string name, string desc) =>
            (Locale.English, new UnlockInfo { Name = name, Description = desc, FlavourText = "" });
    }

    // ---------- Grundgericht: Hähnchen mit Reis ----------
    public class MyChicken_Plated_Dish : CustomDish
    {
        public override string UniqueNameID => nameof(MyChicken_Plated_Dish);
        public override DishType Type => DishType.Base;
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override int Difficulty => 2;
        public override bool IsAvailableAsLobbyOption => true;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;
        public override List<string> StartingNameSet => new List<string>
        {
            "Chicken & Rice", "Rice Rice Baby", "Wing It", "Cluck & Cook", "The Rice Is Right", "Fowl Play"
        };

        public override HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            Refs.Custom<IngredientLib.Ingredient.Items.Chicken>(),
            Refs.Item(ItemReferences.Plate),
            Refs.Item(ItemReferences.Rice),
            Refs.Item(ItemReferences.Pot),
            Refs.Item(ItemReferences.Water)
        };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process> { Refs.Process(ProcessReferences.Cook) };

        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>
        {
            new Dish.MenuItem { Item = Refs.Custom<MyChicken_Plated>(), Phase = MenuPhase.Main, Weight = 1, DynamicMenuType = DynamicMenuType.Static }
        };

        private GameObject _icon, _display;
        public override GameObject IconPrefab => _icon ??= PrefabUtil.BuildPlateWithChicken("Chicken - Icon");
        public override GameObject DisplayPrefab => _display ??= PrefabUtil.BuildPlateWithChicken("Chicken - Display");

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Cook the chicken. Put rice and water in a pot and cook it, then portion the rice onto a plate with the chicken." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            Refs.Info("Chicken & Rice", "Adds Chicken with Rice as a Main")
        };
    }

    // ---------- Variante: Geschnetzeltes (wie dünnes/dickes Steak) ----------
    public class ShreddedChicken_Dish : CustomDish
    {
        public override string UniqueNameID => "ShreddedChicken_Dish";
        public override DishType Type => DishType.Main;
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override int Difficulty => 2;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;
        public override List<Unlock> HardcodedRequirements => new List<Unlock> { Refs.BaseDish };

        public override HashSet<Process> RequiredProcesses => new HashSet<Process> { Refs.Process(ProcessReferences.Chop) };
        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>
        {
            new Dish.MenuItem { Item = Refs.Custom<ShreddedChicken_Plated>(), Phase = MenuPhase.Main, Weight = 0.75f, DynamicMenuType = DynamicMenuType.Static }
        };

        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= PrefabUtil.BuildPlate("Shredded Chicken - Icon", GDOUtils.GetCustomGameDataObject<ShreddedChicken>());

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Chop the cooked chicken before putting it on the plate." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            Refs.Info("Shredded Chicken", "Customers may order shredded chicken with rice")
        };
    }

    // ---------- Extras ----------
    public abstract class ChickenExtraDish : CustomDish
    {
        public override DishType Type => DishType.Extra;
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;
        public override List<Unlock> HardcodedRequirements => new List<Unlock> { Refs.BaseDish };

        protected static IEnumerable<Item> PlatedItems => new[] { Refs.Custom<MyChicken_Plated>(), Refs.Custom<ShreddedChicken_Plated>() };

        protected static HashSet<Dish.IngredientUnlock> UnlockOnBoth(Item ingredient)
        {
            var set = new HashSet<Dish.IngredientUnlock>();
            foreach (var plated in PlatedItems)
                set.Add(new Dish.IngredientUnlock { MenuItem = (ItemGroup)plated, Ingredient = ingredient });
            return set;
        }
    }

    public class ChickenBroccoli_Dish : ChickenExtraDish
    {
        public override string UniqueNameID => "ChickenBroccoli_Dish";
        public override int Difficulty => 1;
        // Brokkoli schneiden, mit Wasser im Topf kochen, portionieren (eigener Topf, keine Beilage).
        public override HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            Refs.Item(ItemReferences.BroccoliRaw), Refs.Item(ItemReferences.Pot), Refs.Item(ItemReferences.Water)
        };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            Refs.Process(ProcessReferences.Chop), Refs.Process(ProcessReferences.Cook)
        };
        public override HashSet<Dish.IngredientUnlock> IngredientsUnlocks => UnlockOnBoth(Refs.Custom<MyChickenMod.Custom.Broccoli.SteamedBroccoli>());

        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= PrefabUtil.BuildItemIcon("Chicken Broccoli - Icon", ItemReferences.BroccoliChopped);

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Chop broccoli, cook it with water in a pot, then portion it onto the chicken plate." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            Refs.Info("Chicken - Broccoli", "Customers may request broccoli with their chicken")
        };
    }

    public class ChickenLemon_Dish : ChickenExtraDish
    {
        public override string UniqueNameID => "ChickenLemon_Dish";
        public override int Difficulty => 1;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { Refs.Item(ItemReferences.Lemon) };
        public override HashSet<Process> RequiredProcesses => new HashSet<Process> { Refs.Process(ProcessReferences.Chop) };
        public override HashSet<Dish.IngredientUnlock> IngredientsUnlocks => UnlockOnBoth(Refs.Item(ItemReferences.LemonSliced));

        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= PrefabUtil.BuildItemIcon("Chicken Lemon - Icon", ItemReferences.LemonSliced);

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Slice a lemon and add it to the chicken plate." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            Refs.Info("Lemon Chicken", "Customers may request a lemon slice with their chicken")
        };
    }

    // Wie Sojasauce beim Wok-Gericht: Kunde will die Flasche zusätzlich am Tisch.
    public class ChickenSoySauce_Dish : ChickenExtraDish
    {
        public override string UniqueNameID => "ChickenSoySauce_Dish";
        public override int Difficulty => 1;
        public override HashSet<Item> MinimumIngredients => new HashSet<Item> { Refs.Item(ItemReferences.CondimentSoySauce) };
        public override HashSet<Dish.IngredientUnlock> ExtraOrderUnlocks => UnlockOnBoth(Refs.Item(ItemReferences.CondimentSoySauce));

        private GameObject _icon;
        public override GameObject IconPrefab => _icon ??= PrefabUtil.BuildItemIcon("Chicken Soy - Icon", ItemReferences.CondimentSoySauce);

        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Some customers want soy sauce with their chicken. Bring the bottle to their table." }
        };
        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            Refs.Info("Chicken - Soy Sauce", "Customers may request soy sauce with their chicken")
        };
    }
}
