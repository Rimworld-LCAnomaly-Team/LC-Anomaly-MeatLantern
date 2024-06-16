using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MeatLantern.Comp
{
    public class CompProperties_AbilityWhirlwindOmNomNom : CompProperties_AbilityEffect
    {
        public FloatRange damageRange;
        public float suckPercent;
        public float armorPenetration;
        public float foodConsumePercent;

        public CompProperties_AbilityWhirlwindOmNomNom()
        {
            compClass = typeof(CompAbilityEffect_WhirlwindOmNomNom);
        }

        public override IEnumerable<string> ExtraStatSummary()
        {
            yield return "AbilityHealthGain".Translate() + ": " + $"({damageRange.min} ~ {damageRange.max}) * {suckPercent}";
            yield return "ConsumeFood".Translate() + ": " + $"({foodConsumePercent * 100}%";
        }
    }
}
