using HarmonyLib;
using MeatLantern.Comp;
using MeatLantern.Def;
using Verse;

namespace MeatLantern.Patch
{
    [HarmonyPatch(typeof(DamageWorker_AddInjury), nameof(DamageWorker_AddInjury.Apply))]
    public class Patch_DamageWorker_AddInjury
    {
        static void Postfix(DamageInfo dinfo, Thing thing)
        {
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker != null)
            {
                Pawn victim = thing as Pawn;
                if(victim != null)
                {
                    Hediff hediff;
                    attacker.health.hediffSet.TryGetHediff(HediffDefOf.MeatLanternImplant, out hediff);

                    if (hediff != null)
                    {
                        hediff.TryGetComp<HediffComp_MeatLanternImplant>().Notify_OnSelfPawnAttackOther(dinfo.Amount);
                    }
                }
            }
        }
    }
}
