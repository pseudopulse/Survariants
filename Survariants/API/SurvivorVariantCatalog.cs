using System;

namespace Survariants {
    public class SurvivorVariantCatalog {
        public static List<SurvivorVariantDef> SurvivorVariantDefs = new();
        public static Dictionary<SurvivorDef, List<SurvivorVariantDef>> SurvivorVariantMap = new();
        public static Dictionary<SurvivorDef, SurvivorDef> SurvivorVariantReverseMap = new();

        public static void AddSurvivorVariant(SurvivorVariantDef SurvivorVariantDef) {
            if (SurvivorVariantDefs.Contains(SurvivorVariantDef)) return;

            if (!SurvivorVariantMap.ContainsKey(SurvivorVariantDef.TargetSurvivor)) {
                SurvivorVariantMap.Add(SurvivorVariantDef.TargetSurvivor, new List<SurvivorVariantDef>());
            }

            SurvivorVariantDefs.Add(SurvivorVariantDef);
            SurvivorVariantMap[SurvivorVariantDef.TargetSurvivor].Add(SurvivorVariantDef);
            SurvivorVariantReverseMap.Add(SurvivorVariantDef.VariantSurvivor, SurvivorVariantDef.TargetSurvivor);
        }
    }
}