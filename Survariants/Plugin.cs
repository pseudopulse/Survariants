using BepInEx;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using Survariants;

namespace Survariants {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class Survariants : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "Survariants";
        public const string PluginVersion = "1.0.0";

        public static BepInEx.Logging.ManualLogSource ModLogger;

        public void Awake() {
            // set logger
            ModLogger = Logger;

            SurvivorVariantManager.Initialize();
        }
    }
}