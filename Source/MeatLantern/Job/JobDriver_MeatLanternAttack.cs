using MeatLantern.Comp;
using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using MeatLantern.Utility;

namespace MeatLantern.Job
{
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
            //Log.Message($"进入攻击行为树");

            //如果这些条件满足就执行失败
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.B);

            if (base.pawn.mindState.enemyTarget != null)
            {
                List<Pawn> victims = MeatLanternUtility.ScanForTarget_NewTemp(base.pawn, 2);

                if (victims != null && victims.Count > 0)
                {
                    Log.Message($"准备攻击，受害者数量为：{victims.Count}");
                    Comp.Eat(victims);
                }
            }
            Comp.meatLanternState = MeatLanternState.Wait;
            EndJobWith(JobCondition.InterruptForced);

            yield break;
        }

        public override bool IsContinuation(Verse.AI.Job j)
        {
            return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }

        public override void Notify_PatherFailed()
        {
            job.SetTarget(TargetIndex.A, RevenantUtility.GetClosestTargetInRadius(pawn, 20f));
            pawn.mindState.enemyTarget = job.GetTarget(TargetIndex.A).Pawn;

            base.Notify_PatherFailed();
        }
    }
}
