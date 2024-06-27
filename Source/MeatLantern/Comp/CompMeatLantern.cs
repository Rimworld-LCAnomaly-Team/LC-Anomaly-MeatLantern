using LCAnomalyLibrary.Comp;
using LCAnomalyLibrary.Util;
using MeatLantern.Job;
using MeatLantern.Utility;
using RimWorld;
using System.Collections.Generic;
using System.IO;
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

                Hediff hediff = ((Pawn)parent).health.hediffSet.GetFirstHediffOfDef(LCAnomalyLibrary.Defs.HediffDefOf.FakeInvisibility);
                if (hediff == null)
                {
                    Log.Message("肉食提灯：隐形hediff为空，准备添加");
                    hediff = ((Pawn)parent).health.AddHediff(LCAnomalyLibrary.Defs.HediffDefOf.FakeInvisibility);
                }
                else
                {
                    Log.Message("肉食提灯：隐形hediff不为空");
                }
                invisibility = hediff?.TryGetComp<LC_HediffComp_FakeInvisibility>();

                return invisibility;
            }
        }

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

            ((Pawn)parent).health.GetOrAddHediff(LCAnomalyLibrary.Defs.HediffDefOf.FakeInvisibility);
            CheckSpawnVisible();
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

        /// <summary>
        /// 被研究后执行的操作
        /// </summary>
        public override void Notify_Studied(Pawn studier)
        {
            if (studier == null)
                return;

            CheckIfStudySuccess(studier);
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
        /// 判断是否应该在生成时隐身
        /// </summary>
        /// <returns>该隐身就返回true</returns>
        /// <exception cref="InvalidDataException">不接受的PawnKindDef</exception>
        private void CheckSpawnVisible()
        {
            PawnKindDef def = ((Pawn)parent).kindDef;

            if (def == Def.PawnKindDefOf.MeatLanternEscaped)
            {
                //Log.Warning($"Pawn：{SelfPawn.ThingID} 应该隐身");
                Invisibility.BecomeInvisible();
            }
            else if (def == Def.PawnKindDefOf.MeatLanternContained)
            {
                //Log.Warning("应该显形");
                Invisibility.BecomeVisible();
            }
            else
            {
                throw new InvalidDataException($"Invalid PawnKindDef:{def.defName} Found");
            }
        }

        #endregion 行为逻辑

        #region 研究与图鉴

        protected override LC_StudyResult CheckFinalStudyQuality(Pawn studier)
        {
            //每级智力提供5%成功率，10级智力提供50%成功率
            float successRate_Intellectual = studier.skills.GetSkill(SkillDefOf.Intellectual).Level * 0.05f;
            //叠加基础成功率，此处是50%，叠加完应是100%
            float finalSuccessRate = successRate_Intellectual + Props.studySucessRateBase;
            //成功率不能超过90%
            if (finalSuccessRate >= 1f)
                finalSuccessRate = 0.9f;

            return Rand.Chance(finalSuccessRate) ? LC_StudyResult.Good : LC_StudyResult.Normal;
        }

        protected override bool CheckStudierSkillRequire(Pawn studier)
        {
            if (studier.skills.GetSkill(SkillDefOf.Intellectual).Level < 4)
            {
                Log.Message($"工作：{studier.Name}的技能{SkillDefOf.Intellectual.defName.Translate()}等级不足4，工作固定无法成功");
                return false;
            }

            return true;
        }

        protected override void StudyEvent_NotBad(Pawn studier, LC_StudyResult result)
        {
            switch (result)
            {
                case LC_StudyResult.Good:
                    QliphothCountCurrent++;
                    CheckGiveAccessory(studier, Def.HediffDefOf.AccessoryMeatLantern, "LC_Accessory_Mouth");
                    break;

                case LC_StudyResult.Normal:
                    break;
            }

            if(PeboxComp != null)
                PeboxComp.CheckSpawnPeBox(studier, result);

            StudyUtil.DoStudyResultEffect(studier, (Pawn)parent, result);
        }

        protected override void StudyEvent_Bad(Pawn studier)
        {
            base.StudyEvent_Bad(studier);

            if (PeboxComp != null)
                PeboxComp.CheckSpawnPeBox(studier, LC_StudyResult.Bad);
        }

        /// <summary>
        /// 检查是否已在图鉴中被解锁
        /// </summary>
        private void CheckIsDiscovered()
        {
            //Log.Message($"检查图鉴解锁情况，我是 {SelfPawn.def.defName}");

            if (Invisibility.PsychologicallyVisible && AnomalyUtility.ShouldNotifyCodex((Pawn)parent, EntityDiscoveryType.Unfog, out var entries))
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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = "Suppress Entity",
                action = delegate
                {
                    //Log.Warning("supress entity");

                    if (Invisibility.PsychologicallyVisible)
                        return;
                    else
                        Find.LetterStack.ReceiveLetter(
                            "LetterMeatLanternSupressLabel".Translate()
                            , "LetterMeatLanternSupress".Translate()
                            , LetterDefOf.ThreatBig, (Pawn)parent);

                    Invisibility.BecomeVisible();
                }
            };
        }

        #endregion UI
    }
}