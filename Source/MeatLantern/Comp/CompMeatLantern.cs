using LCAnomalyCore.Comp;
using LCAnomalyCore.Comp.Pawns;
using LCAnomalyCore.Util;
using MeatLantern.Job;
using MeatLantern.Utility;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MeatLantern.Comp
{
    public class CompMeatLantern : LC_CompEntity
    {
        #region 变量

        public MeatLanternState meatLanternState;

        /// <summary>
        /// 下一次吞噬的时间
        /// </summary>
        public int nextEat = -99999;

        private int tempStudyTick = 0;

        public new CompProperties_MeatLantern Props => (CompProperties_MeatLantern)props;

        #endregion 变量

        #region 生命周期

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref nextEat, "nextEat", -99999);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            CheckSpawnHostile();
        }

        #endregion 生命周期

        #region 触发事件

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            MeatLanternUtility.OnMeatLanternDeath((Pawn)parent, prevMap);
        }

        public override void Notify_Escaped()
        {
            base.Notify_Escaped();

            MeatLanternUtility.OnMeatLanternEscape((Pawn)parent, parent.MapHeld);
        }

        /// <summary>
        /// 绑到收容平台上后的操作
        /// </summary>
        public override void Notify_Holded()
        {
            CheckIsDiscovered();
        }

        public override void Notify_Studying(Pawn studier)
        {
            tempStudyTick++;
        }

        public override void Notify_Studied(Pawn studier, bool interrupted = false)
        {
            base.Notify_Studied(studier, interrupted);

            int time = tempStudyTick / 60;
            tempStudyTick = 0;

            //如果肉食提灯研究时间少于40s，逆卡巴拉计数器会立即减少
            if (time < 40)
                QliphothCountCurrent--;
        }

        #endregion 触发事件

        #region 行为逻辑

        /// <summary>
        /// 吞噬攻击单位
        /// </summary>
        /// <param name="victims">受害者Lsit</param>
        public void Eat(List<Pawn> victims)
        {
            MeatLanternUtility.OnMeatLanternEat((Pawn)parent, victims);

            nextEat = Find.TickManager.TicksGame + Props.eatCooldownTick;
            ((Pawn)parent).mindState.enemyTarget = null;
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="state">状态</param>

        public void SetState(MeatLanternState state)
        {
            meatLanternState = state;
            ((Pawn)parent).jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        /// <summary>
        /// 判断是否应该在生成时改变敌对情况（是逃跑状态则派系变空，否则变实体阵营）
        /// </summary>
        private void CheckSpawnHostile()
        {
            PawnKindDef def = ((Pawn)parent).kindDef;

            if (def == Def.PawnKindDefOf.MeatLanternEscaped)
            {
                if (parent.Faction != null)
                    parent.SetFaction(null);
            }
            else if (def == Def.PawnKindDefOf.MeatLanternContained)
            {
                if (parent.Faction != Faction.OfEntities)
                    parent.SetFaction(Faction.OfEntities);
            }
        }

        #endregion 行为逻辑

        #region 研究与图鉴

        protected override float StudySuccessRateCalculate(CompPawnStatus studier, EAnomalyWorkType workType)
        {
            float baseRate = base.StudySuccessRateCalculate(studier, workType);
            float workTypeRate = 0;
            float finalRate = 0;

            switch (workType)
            {
                case EAnomalyWorkType.Instinct:
                    //本能：I和II级45%，III级50%，别的55%
                    switch (studier.GetPawnStatusELevel(EPawnStatus.Fortitude))
                    {
                        case EPawnLevel.I:
                        case EPawnLevel.II:
                            workTypeRate = 0.45f;
                            break;

                        case EPawnLevel.III:
                            workTypeRate = 0.5f;
                            break;

                        default:
                            workTypeRate = 0.55f;
                            break;
                    }
                    break;

                case EAnomalyWorkType.Insight:
                    //洞察：60%
                    switch (studier.GetPawnStatusELevel(EPawnStatus.Prudence))
                    {
                        default:
                            workTypeRate = 0.6f;
                            break;
                    }
                    break;

                case EAnomalyWorkType.Attachment:
                    //沟通：45%
                    switch (studier.GetPawnStatusELevel(EPawnStatus.Temperance))
                    {
                        default:
                            workTypeRate = 0.45f;
                            break;
                    }
                    break;

                case EAnomalyWorkType.Repression:
                    //压迫：30%
                    switch (studier.GetPawnStatusELevel(EPawnStatus.Justice))
                    {
                        default:
                            workTypeRate = 0.3f;
                            break;
                    }
                    break;
            }

            finalRate = baseRate + workTypeRate;

            //成功率不能超过95%
            if (finalRate > 0.95f)
                finalRate = 0.95f;

            return finalRate;
        }

        /// <summary>
        /// 检查是否已在图鉴中被解锁
        /// </summary>
        private void CheckIsDiscovered()
        {
            //Log.Message($"检查图鉴解锁情况，我是 {SelfPawn.def.defName}");

            if (AnomalyUtility.ShouldNotifyCodex((Pawn)parent, EntityDiscoveryType.Unfog, out var entries))
            {
                Find.EntityCodex.SetDiscovered(entries, Def.PawnKindDefOf.MeatLanternContained.race, (Pawn)parent);
                Find.EntityCodex.SetDiscovered(entries, Def.PawnKindDefOf.MeatLanternEscaped.race, (Pawn)parent);
            }
        }

        #endregion 研究与图鉴

        #region UI

        public override string CompInspectStringExtra()
        {
            TaggedString taggedString = "Biosignature".Translate() + ": " + BiosignatureName;
            if (DebugSettings.showHiddenInfo)
            {
                taggedString += "\nState: " + meatLanternState;
                taggedString += "\nEatDuration：" + Props.eatCooldownTick;
                taggedString += "\nReadyToEat：" + (Find.TickManager.TicksGame >= nextEat);
            }

            return taggedString;
        }

        #endregion UI
    }
}