using KitchenData;
using KitchenLib.Customs;
using KitchenLib.Utils;
using System.Collections.Generic;
using KitchenLib.References;
using UnityEngine;

namespace MyChickenMod.Custom.Chicken
{

    public class MyChicken_Plated : CustomItemGroup
    {
        public override string UniqueNameID => nameof(MyChicken_Plated);

        public virtual GameObject Prefab => Mod.Bundle.LoadAsset<GameObject>("Chicken - Plated");

        public virtual Item DirtiesTo => (Item)GDOUtils.GetExistingGDO(ItemReferences.PlateDirty);

        public virtual Item DisposesTo => (Item)GDOUtils.GetExistingGDO(ItemReferences.Plate);
        
        public virtual List<ItemGroup.ItemSet> Sets => new List<ItemGroup.ItemSet>()
        {
            new ItemGroup.ItemSet()
            {
                Max = 2,
                Min = 2,
                Items = new List<Item>()
                {
                    (Item) GDOUtils.GetExistingGDO(ItemReferences.Plate)
                },
                IsMandatory = true,
            }
        };

        public virtual ItemValue ItemValue => ItemValue.Small;

        public virtual void OnRegister(ItemGroup gameDataObject)
        {
            MaterialUtils.ApplyMaterial(gameDataObject.Prefab, "Plate/Plate/Cylinder", new Material[2]);
        }
    }
}
