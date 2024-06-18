using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MeatLantern.Comp
{
    public class CompProperties_AbilityWhirlwindOmNomNom : CompProperties_AbilityEffect
    {
        public FloatRange damageRange;
        public float armorPenetration;
        public float suckPercent;
        public float stunPercent;


        public CompProperties_AbilityWhirlwindOmNomNom()
        {
            compClass = typeof(CompAbilityEffect_WhirlwindOmNomNom);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return "SuckAmount".Translate() + ": " + $"{damageRange.min * suckPercent} ~ {damageRange.max * suckPercent}";
            yield return "StunTime".Translate() + ": " + $"{damageRange.min * stunPercent * 0.5f} ~ {damageRange.max * stunPercent * 0.5f}s";
        }
    }
}
