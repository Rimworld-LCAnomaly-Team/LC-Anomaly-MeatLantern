using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MeatLantern
{
    public class CompMeatLanternEgg : CompInteractable
    {
        protected override void OnInteracted(Pawn caster)
        {
            CompUsable comp = parent.GetComp<CompUsable>();
            comp.TryStartUseJob(caster, comp.GetExtraTarget(caster));
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            yield break;
        }
    }
}
