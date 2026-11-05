using KitchenLib;
using KitchenLib.Logging;
using KitchenMods;
using System.Reflection;
using UnityEngine;
using MyChickenMod.Custom.Chicken;
using MyChickenMod.Custom.Rice;
using MyChickenMod.Custom.Broccoli;
using KitchenLib.Event;

namespace MyChickenMod
{
    public class Mod : BaseMod, IModSystem
    {
        public const string MOD_GUID = "TillAd.PlateUp.ChickenAndRice";
        public const string MOD_NAME = "Chicken & Rice";
        public const string MOD_VERSION = "1.1.0";
        public const string MOD_AUTHOR = "Till";
        public const string MOD_GAMEVERSION = ">=1.1.4";

        public static AssetBundle Bundle;
        public static KitchenLogger Logger;

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
            // Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).FirstOrDefault() ?? throw new MissingAssetBundleException(MOD_GUID);
            Logger = InitLogger();

            AddGameDataObject<MyChickenMod.Custom.Portions.RicePortion>();
            AddGameDataObject<MyChickenMod.Custom.Portions.SteamedBroccoliPortion>();
            AddGameDataObject<RiceCoolDownProcess>();
            AddGameDataObject<RiceBurntPot>();
            AddGameDataObject<RiceColdPot>();
            AddGameDataObject<RiceCookedPot>();
            AddGameDataObject<RiceCookedPotBurning>();
            AddGameDataObject<RiceCookedPotCooling>();
            AddGameDataObject<RiceCookedPotBurningCooling>();
            AddGameDataObject<RicePot>();
            AddGameDataObject<SteamedBroccoliPot>();
            AddGameDataObject<ChoppedBroccoliPot>();
            AddGameDataObject<MyChicken_Plated>();
            AddGameDataObject<ShreddedChicken_Plated>();
            AddGameDataObject<MyChicken_Plated_Dish>();
            AddGameDataObject<ShreddedChicken_Dish>();
            AddGameDataObject<ChickenBroccoli_Dish>();
            AddGameDataObject<ChickenLemon_Dish>();
            AddGameDataObject<ChickenSoySauce_Dish>();
            AddGameDataObject<LowCarb_Dish>();
            AddGameDataObject<BulkingSeason_Dish>();
            AddGameDataObject<BurntRiceCard>();
            AddGameDataObject<ColdRiceCard>();

            Events.BuildGameDataPreSetupEvent += (s, args) => PrefabUtil.FixMaterials();

#if DEBUG
            // Nur in Debug-Builds: Spieldaten zum Nachschauen in eine Datei schreiben.
            Events.BuildGameDataEvent += (s, args) =>
            {
                if (!args.firstBuild) return;
                try { DataDump.Write(args.gamedata); }
                catch (System.Exception e) { LogError("Dump fehlgeschlagen: " + e); }
            };
#endif
        }


        public static void LogInfo(string _log) => Debug.Log((object)("ChickenMod" + _log));

        public static void LogWarning(string _log) => Debug.LogWarning((object)("ChickenMod " + _log));

        public static void LogError(string _log) => Debug.LogError((object)("ChickenMod " + _log));

        public static void LogInfo(object _log) => Mod.LogInfo(_log.ToString());

        public static void LogWarning(object _log) => Mod.LogWarning(_log.ToString());

        public static void LogError(object _log) => Mod.LogError(_log.ToString());
    }


}

