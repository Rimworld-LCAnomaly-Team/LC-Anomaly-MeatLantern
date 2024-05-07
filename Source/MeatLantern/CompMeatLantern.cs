using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using Verse;
using UnityEngine;
using Verse.Sound;
using LCAnomalyLibrary.Comp;
using LCAnomalyLibrary.Defs;

namespace MeatLantern
{
    public class CompMeatLantern : LC_CompEntity
    {

        #region 变量

        public const int CanHearUnlockIndex = 2;

        public MeatLanternState meatLanternState;

        public bool MeatLanternSmearNotified;

        public int MeatLanternLastLeftSmear = -99999;

        public int lastForcedVisibilityMessage = -99999;

        public int escapeSecondStageStartedTick = -99999;

        public int nextEat = -99999;

        private int lastHeardMessageTick = -99999;

        private int lastSeenLetterTick = -99999;

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
                    Log.Warning("Hediff is null");
                    //hediff = MeatLantern.health.AddHediff(MeatLanternHediffDefOf.MeatLanternInvisibility);
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
            Scribe_Values.Look(ref MeatLanternSmearNotified, "MeatLanternSmearNotified", defaultValue: false);
            Scribe_Values.Look(ref lastForcedVisibilityMessage, "lastForcedVisibilityMessage", 0);
            Scribe_Values.Look(ref escapeSecondStageStartedTick, "escapeSecondStageStartedTick", 0);
            Scribe_Values.Look(ref biosignature, "biosignature", 0);
            Scribe_Values.Look(ref injuredWhileAttacking, "injuredWhileAttacking", defaultValue: false);
            Scribe_Values.Look(ref lastHeardMessageTick, "lastHeardMessageTick", 0);
            Scribe_Values.Look(ref lastSeenLetterTick, "lastSeenLetterTick", 0);
        }

        public void SetState(MeatLanternState state)
        {
            meatLanternState = state;
            SelfPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        public override void PostPostMake()
        {
            biosignature = Rand.Int;
            Log.Warning($"biospawn={biosignature}");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            SelfPawn.health.GetOrAddHediff(LC_HediffDefOf.FakeInvisibility);
        }

        public override void CompTick()
        {
            if (SelfPawn.Spawned)
            {
                if (SelfPawn.IsHashIntervalTick(90))
                {
                    CheckIfSeen();
                }
            }
        }

        /// <summary>
        /// 是否看到了的方法
        /// </summary>
        protected override void CheckIfSeen()
        {
            //研究3次完毕才能看见幽魂，暂时用不到
            //if (!Find.AnalysisManager.TryGetAnalysisProgress(biosignature, out var details) || !details.Satisfied)
            //{
            //    return;
            //}

            //获取所有地图上的殖民者集合
            List<Pawn> freeColonistsSpawned = SelfPawn.Map.mapPawns.FreeColonistsSpawned;
            bool flag = false;

            //遍历地图殖民者
            for (int i = 0; i < freeColonistsSpawned.Count; i++)
            {
                Pawn pawn = freeColonistsSpawned[i];

                //如果殖民者没有失明，与实体的距离在8.9以内，并且与实体之间的视线没有阻挡，则认为实体被发现
                if (!PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn) 
                    && SelfPawn.PositionHeld.InHorDistOf(pawn.PositionHeld, 8.9f) 
                    && GenSight.LineOfSightToThing(pawn.PositionHeld, SelfPawn, SelfPawn.Map))
                {
                    //如果实体没有隐身，并且距离上次被发现已超过1200Tick，则弹出信封
                    if (!SelfPawn.IsPsychologicallyInvisible() && Find.TickManager.TicksGame > lastSeenLetterTick + 6000)
                    {
                        //信封：某Pawn触发了本实体的文本字段，类型为大型威胁
                        Find.LetterStack.ReceiveLetter("LetterMeatLanternSeenLabel".Translate(),
                            "LetterMeatLanternSeen".Translate(pawn.Named("PAWN")), LetterDefOf.ThreatBig, pawn);
                        lastSeenLetterTick = Find.TickManager.TicksGame;
                    }
                    flag = true;
                    break;
                }
            }

            //TODO 这是幽魂被发现后的逻辑变化，以flag的值为准，目前暂时用不到
            if (flag)
            {
                //Invisibility.BecomeVisible();
                //becomeInvisibleTick = Find.TickManager.TicksGame + 140;
            }
        }

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
                //且处于进攻状态，就更新 进攻时受到伤害的状态
                if (meatLanternState == MeatLanternState.Attack)
                {
                    injuredWhileAttacking = true;
                }

                //且处于休眠状态，就设置其状态为逃窜
                //TODO 暂时用不着，这是幽魂的内容
                //if (meatLanternState == MeatLanternState.Sleep)
                //{
                //    SetState(MeatLanternState.Escape);
                //}
            }
        }

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
            Log.Warning($"bionow={biosignature}");
            MeatLanternUtility.OnMeatLanternDeath(SelfPawn, prevMap);
        }

        /// <summary>
        /// 吃小人咯
        /// </summary>
        /// <param name="victim">受害者</param>
        public void Eat(Pawn victim)
        {
            if (!victim.Dead)
            {
                DamageInfo dinfo = new DamageInfo(MeatLanternDamageDefOf.MeatLantern_Crush, 500, 0f, -1f, SelfPawn);
                dinfo.SetApplyAllDamage(value: true);
                victim.TakeDamage(dinfo);
                DoEatEffect();

                nextEat = Find.TickManager.TicksGame + EatCooldownTick;
                if (PawnUtility.ShouldSendNotificationAbout(victim))
                {
                    if(victim.Dead)
                    {
                        Find.LetterStack.ReceiveLetter("LetterLabelPawnMeatLanternEaten".Translate(victim.Named("PAWN")),
                            "LetterPawnMeatLanternEaten".Translate(victim.Named("PAWN")), LetterDefOf.NegativeEvent, victim);
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter("LetterLabelPawnMeatLanternAttacked".Translate(victim.Named("PAWN")),
                            "LetterPawnMeatLanternAttacked".Translate(victim.Named("PAWN")), LetterDefOf.NegativeEvent, victim);
                    }
                }
            }

            SelfPawn.mindState.enemyTarget = null;
        }

        /// <summary>
        /// 吃一群小人咯
        /// </summary>
        /// <param name="victims">受害者Lsit</param>
        public void Eat(List<Pawn> victims) 
        {
            DamageInfo dinfo = new DamageInfo(MeatLanternDamageDefOf.MeatLantern_Crush, 500, 0f, -1f, SelfPawn);
            dinfo.SetApplyAllDamage(value: true);

            DoEatEffect();

            foreach (Pawn p in victims)
            {
                if (!p.Dead)
                {
                    p.TakeDamage(dinfo);
                    if (PawnUtility.ShouldSendNotificationAbout(p))
                    {
                        if (p.Dead)
                        {
                            Find.LetterStack.ReceiveLetter("LetterLabelPawnMeatLanternEaten".Translate(p.Named("PAWN")),
                                "LetterPawnMeatLanternEaten".Translate(p.Named("PAWN")), LetterDefOf.NegativeEvent, p);
                        }
                        else
                        {
                            Find.LetterStack.ReceiveLetter("LetterLabelPawnMeatLanternAttacked".Translate(p.Named("PAWN")),
                                "LetterPawnMeatLanternAttacked".Translate(p.Named("PAWN")), LetterDefOf.NegativeEvent, p);
                        }
                    }
                }

            }

            nextEat = Find.TickManager.TicksGame + EatCooldownTick;
            SelfPawn.mindState.enemyTarget = null;
        }

        private void DoEatEffect()
        {
            Vector3 temp = SelfPawn.Position.ToVector3();

            Vector3 loc = new Vector3()
            {
                x = temp.x + 0.5f,
                y = temp.y,
                z = temp.z + 1.5f
            };

            SoundDefOf.MeatLantern_Eat.PlayOneShot(new TargetInfo(SelfPawn.Position, SelfPawn.Map));

            //TODO 这里要改成effecter来做延时处理
            FleckMaker.ThrowFireGlow(loc, SelfPawn.Map, 2);
            FleckMaker.Static(loc, SelfPawn.Map, FleckDefOf.MeatLantern_Escaped_Mouth_Eat);
            //FleckMaker.ThrowFireGlow(MeatLantern.Position.ToVector3(), MeatLantern.Map, 2);
            //FleckMaker.Static(MeatLantern.Position.ToVector3(), MeatLantern.Map, FleckDefOf.MeatLantern_Escaped_Mouth_Eat);
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
                    Log.Warning("镇压显形");
                    Invisibility.BecomeVisible(true);
                }
            };

            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }

            yield return new Command_Action
            {
                
                defaultLabel = "v",
                action = delegate
                {
                    Log.Warning("强制隐形");
                    Invisibility.BecomeInvisible(true);
                }
            };

            yield return new Command_Action
            {

                defaultLabel = "a1",
                action = delegate
                {
                    Log.Warning("播放音乐");
                    DoEatEffect();
                    //SoundDefOf.MeatLantern_Eat.PlayOneShot(new SoundInfo());
                    //SoundDefOf.MeatLantern_Eat.PlayOneShotOnCamera();
                    //SoundDefOf.MeatLantern_Eat.PlayOneShot(new TargetInfo(MeatLantern.Position, MeatLantern.Map));
                }
            };

            yield return new Command_Action
            {

                defaultLabel = "kill",
                action = delegate
                {
                    Log.Warning("杀掉");
                    SelfPawn.Kill(null);
                    //SoundDefOf.MeatLantern_Eat.PlayOneShot(new SoundInfo());
                    //SoundDefOf.MeatLantern_Eat.PlayOneShotOnCamera();
                    //SoundDefOf.MeatLantern_Eat.PlayOneShot(new TargetInfo(MeatLantern.Position, MeatLantern.Map));
                }
            };
        }
    }
}
