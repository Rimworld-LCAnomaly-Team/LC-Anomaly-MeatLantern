using LCAnomalyLibrary.Comp;
using MeatLantern.Comp;

namespace MeatLantern.Things
{
    public class LC_MeatLanternPawn : LC_EntityBasePawn
    {
        public LC_MeatLanternPawn()
        {
        }

        public override void Tick()
        {
            //收容状态下丢下就出逃
            if(CarriedBy == null && kindDef == Def.PawnKindDefOf.MeatLanternContained)
            {
                GetComp<CompMeatLantern>()?.Notify_Escaped();
            }
        }
    }
}