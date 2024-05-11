using Verse.AI;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System;

namespace MeatLantern
{
    public enum MeatLanternState
    {
        Attack,
        Wait
    }

    public class JobGiver_MeatLanternWait : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            return JobMaker.MakeJob(JobDefOf.MeatLanternWait);
        }
    }

    public class JobGiver_MeatLanternAttack : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            return JobMaker.MakeJob(JobDefOf.MeatLanternAttack, pawn.mindState.enemyTarget);
        }
    }


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
            Log.Message($"进入等待行为树");
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.tickAction = (Action)Delegate.Combine(toil.tickAction, (Action)delegate
            {
                //TODO 这里的条件可以改成吞噬动画控制
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

    /// <summary>
    /// 攻击状态
    /// </summary>
    public class JobDriver_MeatLanternAttack : JobDriver
    {
        private CompMeatLantern Comp => pawn.TryGetComp<CompMeatLantern>();

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Log.Message($"进入攻击行为树");
            //如果这些条件满足就执行失败
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);

            if(base.pawn.mindState.enemyTarget != null)
            {
                List<Pawn> victims = MeatLanternUtility.ScanForTarget_NewTemp(base.pawn, 2);

                if(victims!= null && victims.Count > 1)
                {
                    Log.Message($"准备攻击，受害者为复数单位，数量为：{victims.Count}");
                    Comp.Eat(victims);
                }
                else
                {
                    Pawn victim = (Pawn)base.pawn.mindState.enemyTarget;

                    Log.Message($"准备攻击，受害者为：{victim.Name}");
                    Comp.Eat(victim);
                }
            }
            Comp.meatLanternState = MeatLanternState.Wait;
            EndJobWith(JobCondition.InterruptForced);

            yield break;
        }

        public override bool IsContinuation(Job j)
        {
            return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }

        public override void Notify_PatherFailed()
        {
            job.SetTarget(TargetIndex.A, RevenantUtility.GetClosestTargetInRadius(pawn, 20f));
            pawn.mindState.enemyTarget = job.GetTarget(TargetIndex.A).Pawn;
            //if (pawn.mindState.enemyTarget == null)
            //{
            //    Comp.meatLanternState = MeatLanternState.Wander;
            //}

            base.Notify_PatherFailed();
        }
    }
}
