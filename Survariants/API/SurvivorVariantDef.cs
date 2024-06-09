using System;

namespace Survariants {
    public class SurvivorVariantDef : ScriptableObject {
        public SurvivorDef TargetSurvivor;
        public string Description;
        public SurvivorDef VariantSurvivor;
        public UnlockableDef RequiredUnlock;
        public string DisplayName;
        public Sprite Icon;
        public Color Color;
    }
}