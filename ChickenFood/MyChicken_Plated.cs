using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using System.Collections.Generic;
using IngredientLib.Ingredient.Items;
using KitchenLib.References;
using UnityEngine;

namespace MyChickenMod.Custom.Chicken
{

    public class MyChicken_Plated : CustomItemGroup
    {
        public override string UniqueNameID => nameof(MyChicken_Plated);

        public override GameObject Prefab => ((Item)GDOUtils.GetExistingGDO(ItemReferences.Apple)).Prefab;

        public override Item DirtiesTo => (Item)GDOUtils.GetExistingGDO(ItemReferences.PlateDirty);

        public override Item DisposesTo => (Item)GDOUtils.GetExistingGDO(ItemReferences.Plate);
        
        public override List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>()
        {
            new ItemGroup.ItemSet()
            {
                Max = 1,
                Min = 1,
                Items = new List<Item>()
                {
                    (Item) GDOUtils.GetExistingGDO(ItemReferences.Plate)
                },
                IsMandatory = true,
            },
            new ItemGroup.ItemSet()
            {
                Max = 1,
                Min = 1,
                Items = new List<Item>()
                {
                    (Item) GDOUtils.GetCustomGameDataObject<CookedChicken>().GameDataObject
                },
            }
        };

        public override ItemValue ItemValue => ItemValue.Small;

        public override void OnRegister(ItemGroup gameDataObject)
        {
            // MaterialUtils.ApplyMaterial(gameDataObject.Prefab, "Plate/Plate/Cylinder", new Material[2]);
        }
    }
}
