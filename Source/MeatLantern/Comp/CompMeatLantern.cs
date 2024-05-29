using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using LCAnomalyLibrary.Comp;
using System.IO;
using MeatLantern.Job;
using MeatLantern.Utility;

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

        /// <summary>
        /// 吞噬冷却时间
        /// </summary>
        public int EatCooldownTick = 500;

        [Unsaved(false)]
        private LC_HediffComp_FakeInvisibility invisibility;
        public LC_HediffComp_FakeInvisibility Invisibility
        {
            get
            {
                if (invisibility != null)
                {
                    return invisibility;
                }

                Hediff hediff = SelfPawn.health.hediffSet.GetFirstHediffOfDef(LCAnomalyLibrary.Defs.HediffDefOf.FakeInvisibility);
                if (hediff == null)
                {
                    Log.Warning("Hediff is null, prepate to add hediff");
                    hediff = SelfPawn.health.AddHediff(LCAnomalyLibrary.Defs.HediffDefOf.FakeInvisibility);
                }
                else
                {
                    Log.Warning("Hediff is not null");
                }
                invisibility = hediff?.TryGetComp<LC_HediffComp_FakeInvisibility>();

                return invisibility;
            }
        }

        #endregion

        #region 生命周期

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref nextEat, "nextEat", -99999);
            Scribe_Values.Look(ref EatCooldownTick, "EatCooldownTick", 500);
        }

        public override void PostPostMake()
        {
            biosignature = Rand.Int;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            SelfPawn.health.GetOrAddHediff(LCAnomalyLibrary.Defs.HediffDefOf.FakeInvisibility);
            CheckSpawnVisible();
        }

        #endregion

        #region 触发事件

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            //调用死后操作
            MeatLanternUtility.OnMeatLanternDeath(SelfPawn, prevMap);
        }

        public override void Notify_Escaped()
        {
            //生成逃脱的肉食提灯
            MeatLanternUtility.OnMeatLanternEscape(SelfPawn, SelfPawn.Map);
        }

        /// <summary>
        /// 绑到收容平台上后的操作
        /// </summary>
        public override void Notify_Holded()
        {
            base.Notify_Holded();

            CheckIsDiscovered();
        }

        /// <summary>
        /// 被研究后执行的操作
        /// </summary>
        public override void Notify_Studied(Pawn studier)
        {
            if (studier == null)
                return;

            CheckIfStudySuccess(studier);
        }

        #endregion

        #region 行为逻辑

        /// <summary>
        /// 吞噬攻击单位
        /// </summary>
        /// <param name="victims">受害者Lsit</param>
        public void Eat(List<Pawn> victims)
        {
            MeatLanternUtility.OnMeatLanternEat(SelfPawn, victims, SelfPawn.Map);

            nextEat = Find.TickManager.TicksGame + EatCooldownTick;
            SelfPawn.mindState.enemyTarget = null;
        }

        public void SetState(MeatLanternState state)
        {
            meatLanternState = state;
            SelfPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        /// <summary>
        /// 判断是否应该在生成时隐身
        /// </summary>
        /// <returns>该隐身就返回true</returns>
        /// <exception cref="InvalidDataException">不接受的PawnKindDef</exception>
        private bool CheckSpawnVisible()
        {
            PawnKindDef def = SelfPawn.kindDef;

            if (def == Def.PawnKindDefOf.MeatLanternEscaped)
            {
                //Log.Warning($"Pawn：{SelfPawn.ThingID} 应该隐身");
                Invisibility.BecomeInvisible();
                return true;
            }
            else if (def == Def.PawnKindDefOf.MeatLanternContained)
            {
                //Log.Warning("应该显形");
                Invisibility.BecomeVisible();
                return false;
            }
            else
            {
                throw new InvalidDataException($"Invalid PawnKindDef:{def.defName} Found");
            }
        }

        #endregion

        #region 研究与图鉴

        protected override bool CheckIfFinalStudySuccess(Pawn studier)
        {
            //每级智力提供5%成功率，10级智力提供50%成功率
            float successRate_Intellectual = studier.skills.GetSkill(SkillDefOf.Intellectual).Level * 0.05f;
            //叠加基础成功率，此处是50%，叠加完应是100%
            float finalSuccessRate = successRate_Intellectual + Props.studySucessRateBase;

            return Rand.Chance(finalSuccessRate);
        }

        protected override bool CheckStudierSkillRequire(Pawn studier)
        {
            if (studier.skills.GetSkill(SkillDefOf.Intellectual).Level < 2)
            {
                Log.Message($"研究者{studier.Name}的技能{SkillDefOf.Intellectual.defName.Translate()}不足2，研究固定无法成功");
                return false;
            }

            return true;
        }

        protected override void StudyEvent_Failure(Pawn studier)
        {
            base.StudyEvent_Failure(studier);
        }

        protected override void StudyEvent_Success(Pawn studier)
        {
            base.StudyEvent_Success(studier);

            CheckGiveAccessory(studier, Def.HediffDefOf.Accessory_MeatLantern, "LC_Accessory_Mouth");
        }

        /// <summary>
        /// 检查是否已在图鉴中被解锁
        /// </summary>
        private void CheckIsDiscovered()
        {
            Log.Message($"检查图鉴解锁情况，我是 {SelfPawn.def.defName}");

            if (Invisibility.PsychologicallyVisible && AnomalyUtility.ShouldNotifyCodex(SelfPawn, EntityDiscoveryType.Unfog, out var entries))
            {
                Find.EntityCodex.SetDiscovered(entries, Def.PawnKindDefOf.MeatLanternContained.race, SelfPawn);
                Find.EntityCodex.SetDiscovered(entries, Def.PawnKindDefOf.MeatLanternEscaped.race, SelfPawn);
            }
        }

        #endregion

        #region UI

        public override string CompInspectStringExtra()
        {
            TaggedString taggedString = "Biosignature".Translate() + ": " + BiosignatureName;
            if (DebugSettings.showHiddenInfo)
            {
                taggedString += "\nState: " + meatLanternState;
            }

            return taggedString;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {

                    defaultLabel = "kill",
                    action = delegate
                    {
                        SelfPawn.Kill(null);
                    }
                };

                yield return new Command_Action
                {

                    defaultLabel = "Force Meltdown",
                    action = delegate
                    {
                        Log.Warning($"Dev：{SelfPawn.def.defName} 的收容单元发生了强制熔毁");
                        ForceQliphothMeltdown();
                    }
                };

                yield return new Command_Action
                {

                    defaultLabel = "QliphothCount +1",
                    action = delegate
                    {
                        Log.Warning($"Dev：{SelfPawn.def.defName} 的逆卡巴拉计数器上升了1点");
                        QliphothCountCurrent++;
                    }
                };

                yield return new Command_Action
                {

                    defaultLabel = "QliphothCount -1",
                    action = delegate
                    {
                        Log.Warning($"Dev：{SelfPawn.def.defName} 的逆卡巴拉计数器下降了1点");
                        QliphothCountCurrent--;
                    }
                };
            }

            yield return new Command_Action
            {

                defaultLabel = "Suppress Entity",
                action = delegate
                {
                    //Log.Warning("supress entity");

                    if (Invisibility.PsychologicallyVisible)
                        return;

                    if (!Invisibility.PsychologicallyVisible)
                        Find.LetterStack.ReceiveLetter("LetterMeatLanternSupressLabel".Translate(), "LetterMeatLanternSupress".Translate(), LetterDefOf.ThreatBig, SelfPawn);

                    Invisibility.BecomeVisible();
                }
            };
        }

        #endregion
    }
}
