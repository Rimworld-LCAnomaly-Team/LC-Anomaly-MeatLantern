using LCAnomalyCore.Comp;
using MeatLantern.Comp;

namespace MeatLantern.Things
{
    public class LC_MeatLanternPawn : LC_EntityBasePawn
    {
        public LC_MeatLanternPawn()
        {
        }

        protected override void Tick()
        {
            //收容状态下丢下就出逃
            if (Spawned && CarriedBy == null && kindDef == Def.PawnKindDefOf.MeatLanternContained && !EntityComp.Escaped)
            {
                GetComp<CompMeatLantern>()?.Notify_Escaped();
            }

            base.Tick();
        }
    }
}
