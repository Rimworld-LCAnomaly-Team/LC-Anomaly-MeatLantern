using MeatLantern.Comp;
using System;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using MeatLantern.Utility;

namespace MeatLantern.Job
{
    /// <summary>
    /// 等待/诱捕状态
    /// </summary>
    public class JobDriver_MeatLanternWait : JobDriver
    {
        private CompMeatLantern Comp => pawn.TryGetComp<CompMeatLantern>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Log.Message($"进入等待行为树");
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
            {
                if (Find.TickManager.TicksGame >= Comp.nextEat && Rand.MTBEventOccurs(10f, 1f, 1f))
                {
                    Pawn pawn = MeatLanternUtility.GetClosestTargetInRadius(base.pawn, 1f);
                    if (pawn != null)
                    {
                        base.pawn.mindState.enemyTarget = pawn;
                        Comp.meatLanternState = MeatLanternState.Attack;
                        EndJobWith(JobCondition.InterruptForced);
                    }
                }
            });
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}
