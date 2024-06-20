using MeatLantern.Comp;
using MeatLantern.Job;
using Verse;
using Verse.AI;

namespace MeatLantern.ThinkNode
{
    public class ThinkNode_ConditionalMeatLanternState : ThinkNode_Conditional
    {
        public MeatLanternState state;

        protected override bool Satisfied(Pawn pawn)
        {
            CompMeatLantern compMeatLantern = pawn.TryGetComp<CompMeatLantern>();
            if (compMeatLantern != null)
            {
                return compMeatLantern.meatLanternState == state;
            }

            return false;
        }
    }
}