using KitchenLib;
using KitchenLib.Logging;
using KitchenMods;
using System.Reflection;
using UnityEngine;
using MyChickenMod.Custom.Chicken;

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

            AddGameDataObject<MyChicken_Plated>();
            AddGameDataObject<MyChicken_Plated_Dish>();
        }


        public static void LogInfo(string _log) => Debug.Log((object)("ChickenMod" + _log));

        public static void LogWarning(string _log) => Debug.LogWarning((object)("ChickenMod " + _log));

        public static void LogError(string _log) => Debug.LogError((object)("ChickenMod " + _log));

        public static void LogInfo(object _log) => Mod.LogInfo(_log.ToString());

        public static void LogWarning(object _log) => Mod.LogWarning(_log.ToString());

        public static void LogError(object _log) => Mod.LogError(_log.ToString());
    }


}

