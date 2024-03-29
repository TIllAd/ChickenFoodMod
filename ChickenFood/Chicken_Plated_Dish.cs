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

        public virtual bool IsSpecificFranchiseTier => false;

        public virtual UnlockGroup UnlockGroup => UnlockGroup.Dish;

        public virtual int MinimumFranchiseTier => 0;

        public override int Difficulty => 1;

        public virtual DishCustomerChange CustomerMultiplier => (DishCustomerChange)3;


        public virtual List<string> StartingNameSet
        {
            get => new List<string>() { "Chicken" };

        }

        public virtual HashSet<Item> MinimumIngredients => new HashSet<Item>
        {
            (Item) GDOUtils.GetCustomGameDataObject<IngredientLib.Ingredient.Items.Chicken>().GameDataObject
        };

        public override HashSet<Process> RequiredProcesses => new HashSet<Process>
        {
            (Process)GDOUtils.GetExistingGDO(ProcessReferences.RequireOven)
        };

        public virtual List<Dish.MenuItem> ResultingMenuItems => new List<Dish.MenuItem>()
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







