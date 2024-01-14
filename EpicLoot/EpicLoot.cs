using System;
using BepInEx;
using BepInEx.Logging;
using EpicLoot.BaseEL;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace EpicLoot
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class EpicLoot : BaseUnityPlugin
    {
        public const string PluginGUID = "com.lootgoblinsheim.epicloot";
        public const string PluginName = "LGH.EpicLoot";
        public const string PluginVersion = "0.0.1";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private EpicLootBase _epicLootBase;

        private void Awake()
        {
            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo("EpicLoot has landed");
            _epicLootBase = new EpicLootBase();
            _epicLootBase.Awake(Config, Logger);
            
        }

        private void Start()
        {
            _epicLootBase.Start();
        }

        private void OnDestroy()
        {
            _epicLootBase.OnDestroy();
        }
    }
}

