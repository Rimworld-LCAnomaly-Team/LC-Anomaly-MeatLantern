using RimWorld;
using Verse;
using LCAnomalyLibrary.Defs;

namespace MeatLantern
{

    [DefOf]
    public static class HediffDefOf
    {
        [MayRequireAnomaly]
        public static HediffDef MeatLanternInvisibility;
    }

    [DefOf]
    public static class KindDefOf
    {
        public static PawnKindDef MeatLanternEscaped;
        public static PawnKindDef MeatLanternContained;
    }

    [DefOf]
    public static class JobDefOf
    {
        [MayRequireAnomaly]
        public static JobDef MeatLanternAttack;

        [MayRequireAnomaly]
        public static JobDef MeatLanternWait;
    }

    [DefOf]
    public static class FleckDefOf 
    {
        public static FleckDef MeatLantern_Escaped_Mouth_Eat;
    }

    [DefOf]
    public static class SoundDefOf
    {
        public static SoundDef MeatLantern_Eat;
        public static SoundDef MeatLantern_Defeated;
        public static SoundDef MeatLantern_Escape;

    }

    [DefOf]
    public static class ThingDefOf
    {
        [MayRequireAnomaly]
        public static ThingDef MeatLanternEgg;

        [MayRequireAnomaly]
        public static ThingDef DyingMeatLantern;

        [MayRequireAnomaly]
        public static ThingDef EscapingMeatLantern;
    }
}
