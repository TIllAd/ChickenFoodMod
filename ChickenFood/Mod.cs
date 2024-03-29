using IngredientLib.Ingredient.Items;
using IngredientLib.Ingredient.Providers;
using KitchenLib;
using KitchenLib.Logging;
using KitchenLib.Logging.Exceptions;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KitchenLib.Customs;
using MyChickenMod.Custom;
using KitchenLib.Event;
using MyChickenMod.Custom.Chicken;
using KitchenData;
using KitchenLib.Utils;
using System;
using System.Text;
using IngredientLib.Ingredient.Items;
using IngredientLib.Ingredient.Providers;
using KitchenLib;
using KitchenLib.Logging;
using KitchenLib.Logging.Exceptions;
using KitchenMods;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using KitchenLib.Customs;
using MyChickenMod.Custom;
using KitchenLib.Event;
using System.Web.UI.WebControls;
using KitchenLib.Registry;
using Unity.Entities;
using KitchenLib.References;

using ApplianceLib.Api.References;

using System;




namespace MyChickenMod
{
    public class Mod : BaseMod, IModSystem
    {
        public const string MOD_GUID = "com.till.mychickenmod";
        public const string MOD_NAME = "MyChickenMod";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Till";
        public const string MOD_GAMEVERSION = ">=1.1.4";

        public static AssetBundle Bundle;
        public static KitchenLogger Logger;


        public static CustomDish Chicken;

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            Logger.LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).FirstOrDefault() ?? throw new MissingAssetBundleException(MOD_GUID);
            Logger = InitLogger();

            List<AssetBundleModPack> packs = mod.GetPacks<AssetBundleModPack>();
            //Mod.bundle = Mod.bundle = packs.SelectMany<AssetBundleModPack, AssetBundle>((Func<AssetBundleModPack, IEnumerable<AssetBundle>>)(e => (IEnumerable<AssetBundle>)e.AssetBundles)).ToList<AssetBundle>()[0];
            this.AddGameDataObject<MyChicken_Plated>();
            
            Mod.Chicken = (CustomDish)this.AddGameDataObject<MyChicken_Plated_Dish>();
            //Events.BuildGameDataEvent += (EventHandler<BuildGameDataEventArgs>)((s, args) => ModRegistry.HandleBuildGameDataEvent(args));
            //this.AddGameDataObject<MyChickenMod.Custom.MyChicken_Plated>();
            // Events.BuildGameDataEvent += (EventHandler<BuildGameDataEventArgs>)((s, args) => ModRegistry.HandleBuildGameDataEvent(args));




            // Mod.Chicken = (CustomDish) this.AddGameDataObject<Chicken> 
        }
    }
}