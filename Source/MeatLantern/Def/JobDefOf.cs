using RimWorld;
using Verse;

namespace MeatLantern.Def
{
    [DefOf]
    public static class JobDefOf
    {
        [MayRequireAnomaly]
        public static JobDef MeatLanternAttack;

        [MayRequireAnomaly]
        public static JobDef MeatLanternWait;
    }
}
