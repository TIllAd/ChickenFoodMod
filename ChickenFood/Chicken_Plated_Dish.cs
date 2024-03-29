using KitchenData;
using KitchenLib.Utils;
using System.Collections.Generic;
using KitchenLib.Customs;
using KitchenLib.References;

namespace MyChickenMod.Custom.Chicken
{
    public class MyChicken_Plated_Dish : CustomDish
    {

        public override string UniqueNameID => nameof(MyChicken_Plated_Dish);

        public override CardType CardType => CardType.Default;

        public override DishType Type => DishType.Main;

        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;

        public override int Difficulty => 1;

        public override DishCustomerChange CustomerMultiplier => (DishCustomerChange)3;
        
        public override List<string> StartingNameSet=> new List<string>()
        {
            "Chicken"
        };

        public override HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            (Item) GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.Chicken>().GameDataObject
        };

        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.RequireOven)
        };

        public override List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>()
        {
            new Dish.MenuItem()
            {
                Item = (Item) GDOUtils.GetCustomGameDataObject<MyChicken_Plated>().GameDataObject,
                Phase = MenuPhase.Main,
                Weight = 1,
                DynamicMenuType =  DynamicMenuType.Static,
                DynamicMenuIngredient = null
            }
        };
       
        public override bool IsAvailableAsLobbyOption => true;
        public override Dictionary<Locale, string> Recipe => new Dictionary<Locale, string>
        {
            { Locale.English, "Cook Chicken, put it on a plate and serve" }
        };

        public override List<(Locale, UnlockInfo)> InfoList => new List<(Locale, UnlockInfo)>
        {
            (Locale.English, new UnlockInfo
            {
                Name = "Chicken",
                Description = "Adds Chicken as a Main",
                FlavourText = ""
            })
        };
    }
}







