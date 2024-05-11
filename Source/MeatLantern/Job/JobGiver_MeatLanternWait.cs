using Verse.AI;
using Verse;
using MeatLantern.Def;

namespace MeatLantern.Job
{
    public class JobGiver_MeatLanternWait : ThinkNode_JobGiver
    {
        protected override Verse.AI.Job TryGiveJob(Pawn pawn)
        {
            return JobMaker.MakeJob(JobDefOf.MeatLanternWait);
        }
    }
}
