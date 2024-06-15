using MeatLantern.Utility;
using RimWorld;
using Verse;

namespace MeatLantern.Comp
{
    internal class CompAbilityEffect_Whirlwind_OmNomNom : CompAbilityEffect
    {
        public new CompProperties_AbilityWhirlwind_OmNomNom Props => (CompProperties_AbilityWhirlwind_OmNomNom)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            if (pawn != null)
            {
                MeatLanternUtility.DoBiteOnPawn(pawn, parent.pawn, Props.damageRange, Props.armorPenetration, Props.suckPercent);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                return false;
            }

            if (pawn.Faction != null && !pawn.IsSlaveOfColony && !pawn.IsPrisonerOfColony)
            {
                if (pawn.Faction.HostileTo(parent.pawn.Faction))
                {
                    if (!pawn.Downed)
                    {
                        if (throwMessages)
                        {
                            Messages.Message("MessageCantUseOnResistingPerson".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                        }

                        return false;
                    }
                }
                else if (pawn.IsQuestLodger() || pawn.Faction != parent.pawn.Faction)
                {
                    if (throwMessages)
                    {
                        Messages.Message("MessageCannotUseOnOtherFactions".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                    }

                    return false;
                }
            }

            if (ModsConfig.AnomalyActive && pawn.IsMutant && !pawn.mutant.Def.canBleed)
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCannotUseOnNonBleeder".Translate(parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, historical: false);
                }

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
