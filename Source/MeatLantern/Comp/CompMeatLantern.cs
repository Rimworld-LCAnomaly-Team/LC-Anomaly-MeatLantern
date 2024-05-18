using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using LCAnomalyLibrary.Comp;
using LCAnomalyLibrary.Defs;
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

                Hediff hediff = SelfPawn.health.hediffSet.GetFirstHediffOfDef(LC_HediffDefOf.FakeInvisibility);
                if (hediff == null)
                {
                    Log.Warning("Hediff is null, prepate to add hediff");
                    hediff = SelfPawn.health.AddHediff(LC_HediffDefOf.FakeInvisibility);
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

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public void SetState(MeatLanternState state)
        {
            meatLanternState = state;
            SelfPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        public override void PostPostMake()
        {
            biosignature = Rand.Int;
        }

        public override void CompTickLong()
        {
            base.CompTickLong();

            CheckIsDiscovered();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            SelfPawn.health.GetOrAddHediff(LC_HediffDefOf.FakeInvisibility);
            CheckSpawnVisible();
        }

        protected override void CheckIfSeen(){}

        /// <summary>
        /// 在应用伤害之前的方法
        /// </summary>
        /// <param name="dinfo">伤害信息</param>
        /// <param name="totalDamageDealt">伤害量</param>
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            //如果实体没有死亡
            if (!SelfPawn.Dead)
            {
                //且处于进攻状态，就更新进攻时受到伤害的状态
                if (meatLanternState == MeatLanternState.Attack)
                {
                    injuredWhileAttacking = true;
                }
            }
        }
        
        /// <summary>
        /// 在砂了别的pawn之后调用的方法
        /// </summary>
        /// <param name="pawn">受害者</param>
        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
        }

        /// <summary>
        /// 被杀之后触发
        /// </summary>
        /// <param name="prevMap"></param>
        /// <param name="dinfo"></param>
        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            //调用死后操作
            MeatLanternUtility.OnMeatLanternDeath(SelfPawn, prevMap);
        }

        /// <summary>
        /// 逃脱时执行的操作
        /// </summary>
        public override void Escape()
        {
            //生成逃脱的肉食提灯
            MeatLanternUtility.OnMeatLanternEscape(SelfPawn,SelfPawn.Map);
        }

        /// <summary>
        /// 绑到收容平台上后的操作
        /// </summary>
        public override void AfterHoldToPlatform()
        {
            CheckIsDiscovered();
        }

        /// <summary>
        /// 被研究后执行的操作
        /// </summary>
        public override void AfterStudy(Pawn studier)
        {
            if (studier == null)
                return;

            CheckIfStudySuccess(studier);

            //Log.Message($"我是：{SelfPawn.def.defName}，我被研究完了。");
        }

        /// <summary>
        /// 吃小人咯
        /// </summary>
        /// <param name="victims">受害者Lsit</param>
        public void Eat(List<Pawn> victims)
        {
            MeatLanternUtility.OnMeatLanternEat(SelfPawn, victims, SelfPawn.Map);

            nextEat = Find.TickManager.TicksGame + EatCooldownTick;
            SelfPawn.mindState.enemyTarget = null;
        }

        protected override bool CheckIfStudySuccess(Pawn studier)
        {
            if(CheckStudierSkillRequire(studier))
            {
                if (CheckIfFinalStudySuccess(studier))
                {
                    StudyEvent_Success(studier);
                    Log.Warning($"{SelfPawn.def.defName} 研究成功");
                    return true;
                }
                else
                {
                    StudyEvent_Failure(studier);
                    Log.Warning($"{SelfPawn.def.defName} 研究失败");
                    return false;
                }
            }
            else
            {
                StudyEvent_Failure(studier);
                Log.Warning($"{SelfPawn.def.defName} 研究失败");
                return false;
            }
        }

        protected override bool CheckIfFinalStudySuccess(Pawn studier)
        {
            //每级智力提供5%成功率，10级智力提供50%成功率
            float successRate_Intellectual = studier.skills.GetSkill(SkillDefOf.Intellectual).Level * 0.05f;
            //叠加基础成功率，此处是50%，叠加完应是100%
            float finalSuccessRate = successRate_Intellectual + Props.studySucessRateBase;

            return Rand.Chance(finalSuccessRate);
        }

        //TODO 以后有机会搞一个数据结构类，外部允许输入需要的skilldef+等级，而不是在里面写
        private bool CheckStudierSkillRequire(Pawn studier)
        {
            if(studier.skills.GetSkill(SkillDefOf.Intellectual).Level < 2)
            {
                Log.Message($"研究者{studier.Name}的技能{SkillDefOf.Intellectual.defName.Translate()}不足2，研究固定无法成功");
                return false;
            }

            return true;
        }

        protected override void StudyEvent_Failure(Pawn studier)
        {
            QliphothCountCurrent--;
            Log.Message($"{SelfPawn.def.defName} 的逆卡巴拉计数器减少，变为：{QliphothCountCurrent}");
            CheckSpawnPeBox(studier, Props.amountPeBoxStudyFail);
        }

        protected override void StudyEvent_Success(Pawn studier)
        {
            QliphothCountCurrent++;
            Log.Message($"{SelfPawn.def.defName} 的逆卡巴拉计数器增加，变为：{QliphothCountCurrent}");
            CheckSpawnPeBox(studier, Props.amountPeBoxStudySuccess);
            CheckGiveAccessory(studier);
        }

        protected override void QliphothMeltdown()
        {
            Log.Message($"{SelfPawn.def.defName} 的收容单元发生了熔毁");

            CompHoldingPlatformTarget comp = SelfPawn.TryGetComp<CompHoldingPlatformTarget>();
            if(comp != null)
            {
                Log.Message($"{SelfPawn.def.defName} 因收容单元熔毁而出逃");
                comp.Escape(initiator: true);
            }
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
            else if(def == Def.PawnKindDefOf.MeatLanternContained)
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

        /// <summary>
        /// 检查是否需要给予饰品
        /// </summary>
        private void CheckGiveAccessory(Pawn studier)
        {
            //概率排前面是为了减少计算量，避免下面的foreach每次都要触发
            if (!Rand.Chance(Props.accessoryChance))
            {
                Log.Message($"{studier.Name} 获取饰品失败，概率判定失败");
                return;
            }

            if (CheckIfAccessoryConflict(studier))
            {
                var bodypart = studier.RaceProps.body.corePart;
                if (bodypart != null)
                {
                    studier.health.AddHediff(Def.HediffDefOf.Accessory_MeatLantern, bodypart);
                    Log.Message($"{studier.Name} 获取饰品成功");
                }
                else
                {
                    Log.Message($"{studier.Name} 获取饰品失败，身体核心部位为空");
                }
            }
            else
            {
                Log.Message($"{studier.Name} 获取饰品失败，已经拥有相同饰品");
            }
        }

        /// <summary>
        /// 检查是否生成Pebox
        /// </summary>
        /// <param name="studier">研究者</param>
        /// <param name="amount">生成数量</param>
        private void CheckSpawnPeBox(Pawn studier, int amount)
        {
            if (amount <= 0)
                return;

            if(studier!= null)
            {
                if(Props.peBoxDef != null) 
                {
                    Thing thing = ThingMaker.MakeThing(Props.peBoxDef);
                    thing.stackCount = amount;
                    GenSpawn.Spawn(thing, studier.Position, studier.Map);
                    Log.Message($"{SelfPawn.def.defName}生成了{amount}单位的{Props.peBoxDef.defName}");
                }
            }
        }
        
        /// <summary>
        /// 检查是否可以添加饰品
        /// </summary>
        /// <param name="studier">研究者</param>
        /// <returns>true：可以添加 false：不可以添加</returns>
        public override bool CheckIfAccessoryConflict(Pawn studier)
        {
            //没有相关hediff就不冲突，可添加
            var hediffs = studier.health.hediffSet.hediffs;
            List<Hediff> hediffs1 = new List<Hediff>();
            foreach (var hediff in hediffs)
            {
                if ((hediff.def.tags != null) && hediff.def.tags.Contains("LC_Accessory_Mouth"))
                    hediffs1.Add(hediff);
            }
            if (hediffs1.NullOrEmpty())
            {
                Log.Message("没有检测到口部hediff");
                return true;
            }

            //如果有相同的hediff则不进行添加操作，否则清理重复部位的hediff
            foreach (var hediff in hediffs1)
            {
                if (hediff.def == Def.HediffDefOf.Accessory_MeatLantern)
                {
                    Log.Message("检测到相同Hediff");
                    return false;
                }
                else
                    studier.health.RemoveHediff(hediff);
            }
            return true;
        }

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

            if (!DebugSettings.ShowDevGizmos)
                yield break;

            yield return new Command_Action
            {
                
                defaultLabel = "v",
                action = delegate
                {
                    //Log.Warning("froce visible");
                    Invisibility.BecomeInvisible();
                }
            };

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
    }
}
