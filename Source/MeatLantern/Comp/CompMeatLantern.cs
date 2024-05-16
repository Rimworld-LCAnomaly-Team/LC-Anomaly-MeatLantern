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
            Scribe_Values.Look(ref everRevealed, "everRevealed", defaultValue: false);
            Scribe_Values.Look(ref biosignature, "biosignature", 0);
            Scribe_Values.Look(ref injuredWhileAttacking, "injuredWhileAttacking", defaultValue: false);
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
        public override void AfterStudy()
        {
            Log.Message($"我是：{SelfPawn.Name}，我被研究完了。");
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
        }
    }
}
