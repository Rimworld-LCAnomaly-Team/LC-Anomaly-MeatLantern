using LCAnomalyCore.Defs;
using RimWorld;
using Verse;

namespace MeatLantern.Def
{
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef DyingMeatLantern;
        public static ThingDef_AbnormalityEntity_Spawn EscapingMeatLantern;
        public static ThingDef EatingMeatLantern;

        public static ThingDef MeatLanternEgg;
        public static ThingDef EgoWeapon_MeatLantern;

        static ThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }
    }
}
