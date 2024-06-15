using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MeatLantern.Comp
{
    public class CompProperties_AbilityWhirlwind_OmNomNom : CompProperties_AbilityEffect
    {
        public FloatRange damageRange;
        public float suckPercent;
        public float armorPenetration;

        public CompProperties_AbilityWhirlwind_OmNomNom()
        {
            compClass = typeof(CompAbilityEffect_Whirlwind_OmNomNom);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return "AbilityHealthGain".Translate() + ": " + $"({damageRange.min} ~ {damageRange.max}) * {suckPercent}";
        }
    }
}
