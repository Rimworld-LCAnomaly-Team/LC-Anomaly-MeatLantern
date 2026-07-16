using RimWorld;
using Verse;

namespace MeatLantern.Def
{
    [DefOf]
    public static class JobDefOf
    {
        public static JobDef MeatLanternAttack;

        public static JobDef MeatLanternWait;

        static JobDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
        }
    }
}
