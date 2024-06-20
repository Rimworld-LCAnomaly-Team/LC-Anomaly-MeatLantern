using LCAnomalyLibrary.Defs;
using RimWorld;
using Verse;

namespace MeatLantern.Def
{
    [DefOf]
    public static class ThingDefOf
    {
        [MayRequireAnomaly]
        public static ThingDef_AnomalyEgg MeatLanternEgg;

        [MayRequireAnomaly]
        public static ThingDef DyingMeatLantern;

        [MayRequireAnomaly]
        public static ThingDef_AnomalyEntity_Spawn EscapingMeatLantern;

        [MayRequireAnomaly]
        public static ThingDef EatingMeatLantern;

        [MayRequireAnomaly]
        public static ThingDef EgoWeapon_MeatLantern;
    }
}