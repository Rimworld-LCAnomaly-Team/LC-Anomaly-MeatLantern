using MeatLantern.Def;
using Verse;
using Verse.AI;

namespace MeatLantern.Job
{
    public class JobGiver_MeatLanternAttack : ThinkNode_JobGiver
    {
        protected override Verse.AI.Job TryGiveJob(Pawn pawn)
        {
            return JobMaker.MakeJob(JobDefOf.MeatLanternAttack, pawn.mindState.enemyTarget);
        }
    }
}