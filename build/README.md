# Survariants

a system for adding survivor variants.

to use, just:

```cs
using Survariants;

SurvivorVariantDef variant = ScriptableObject.CreateInstance<SurvivorVariantDef>(); // create a new variant
(variant as ScriptableObject).name = "The Name Of Your Variant";
variant.DisplayName = "The Name Of Your Variant'
variant.VariantSurvivor = Survivor; // the SurvivorDef of your variant
variant.TargetSurvivor = TargetSurvivorDef;  // the survivor the variant is for
variant.RequiredUnlock = RequiredUnlock; // optional: unlock requirement
variant.Description = Description; // the flavor text of your variant in the variants tab

Survivor.hidden = true; // make your survivor not appear in the css bar

SurvivorVariantCatalog.AddSurvivorVariant(variant); // add your variant!
```