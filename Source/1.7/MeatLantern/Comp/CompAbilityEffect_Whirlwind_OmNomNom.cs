using MeatLantern.Utility;
using RimWorld;
using Verse;

namespace MeatLantern.Comp
{
    public class CompAbilityEffect_WhirlwindOmNomNom : CompAbilityEffect
    {
        public new CompProperties_AbilityWhirlwindOmNomNom Props => (CompProperties_AbilityWhirlwindOmNomNom)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn != null)
            {
                MeatLanternUtility.DoBiteOnPawn(pawn, parent.pawn, Props.damageRange, Props.armorPenetration, Props.suckPercent, Props.stunPercent);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            var pawn = target.Pawn;

            if (pawn == null)
            {
                return false;
            }

            return true;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return true;
        }
    }
}