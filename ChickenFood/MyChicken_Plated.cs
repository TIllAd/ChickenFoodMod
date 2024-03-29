using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MyChickenMod.Custom.Chicken
{

    public class MyChicken_Plated : CustomItemGroup
    {
        public override string UniqueNameID => nameof(MyChicken_Plated);

        public virtual GameObject prefab => Mod.Bundle.LoadAsset<GameObject>("Chicken - Plated");

        public virtual bool AutoCollapsing => false;

        public virtual Item DirtiesTo => (Item)GDOUtils.GetExistingGDO(1517992271);

        public virtual Item DisposesTo => (Item)GDOUtils.GetExistingGDO(793377380);

        public virtual bool CanContainSide => false;

        public virtual string ColourBlindTag => "C";
        
        public virtual List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>()
        {
            new ItemGroup.ItemSet()
            {
                Max = 2,
                Min = 2,
                Items = new List<Item>()
                {
                    (Item) GDOUtils.GetExistingGDO(793377380)
                },
                OrderingOnly = false,
                IsMandatory = true,
                RequiresUnlock = false
            }
        };

        public virtual ItemValue ItemValue => (ItemValue) 7;

        public virtual void OnRegister(GameDataObject gameDataObject)
        {
            ItemGroup itemGroup = (ItemGroup)gameDataObject;
            MaterialUtils.ApplyMaterial(((Item)itemGroup).Prefab, "Plate/Plate/Cylinder", new Material[2]);
        }
    }
}
