﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using MeatLantern.Effect;
using MeatLantern.Setting;
using UnityEngine;
using System.Text;

namespace MeatLantern.Utility
{
    public static class MeatLanternUtility
    {
        public static readonly FloatRange SearchForTargetCooldownRangeDays = new FloatRange(1f, 2f);

        private static HashSet<Pawn> tmpTargets = new HashSet<Pawn>();

        public static List<Pawn> ScanForTargets(Pawn pawn, float radius)
        {
            return ScanForTarget_NewTemp(pawn, radius);
        }

        public static List<Pawn> ScanForTarget_NewTemp(Pawn pawn, float radius, bool forced = false)
        {
            tmpTargets.Clear();
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater);

            RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(traverseParms, isDestination: true), delegate (Region x)
            {
                List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                float num = float.MaxValue;

                for (int i = 0; i < list.Count; i++)
                {
                    Pawn pawn2 = (Pawn)list[i];
                    if (ValidTarget(pawn2))
                    {
                        if (pawn.Position.InHorDistOf(pawn2.Position, radius) &&
                        (float)pawn2.Position.DistanceToSquared(pawn.Position) <
                        num && GenSight.LineOfSightToThing(pawn.Position, pawn2, pawn.Map))
                        {
                            tmpTargets.Add(pawn2);
                        }
                    }
                }

                return false;
            });

            if (NearbyHumanlikePawnCount(pawn.Position, pawn.Map, 1f) != 0)
            {
                return tmpTargets.ToList();
            }

            return null;
        }

        /// <summary>
        /// 目标是否合法
        /// </summary>
        /// <param name="pawn">目标</param>
        /// <returns>是否合法</returns>
        public static bool ValidTarget(Pawn pawn)
        {
            ////不能吃异常实体阵营单位
            //if (pawn.Faction != Faction.OfEntities)
            //{
            //    return !pawn.health.hediffSet.HasHediff(RimWorld.HediffDefOf.RevenantHypnosis);
            //}

            return ((pawn.kindDef != Def.PawnKindDefOf.MeatLanternEscaped) && (pawn.kindDef != Def.PawnKindDefOf.MeatLanternContained) && !pawn.Downed);

            //return false;
        }

        /// <summary>
        /// 附近人形单位计数
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="map"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static int NearbyHumanlikePawnCount(IntVec3 pos, Map map, float radius)
        {
            int num = 0;
            foreach (Pawn item in map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
            {
                if (ValidTarget(item) && !item.Downed && pos.InHorDistOf(item.Position, radius))
                {
                    num++;
                }
            }

            return num;
        }

        public static Pawn GetClosestTargetInRadius(Pawn pawn, float radius)
        {
            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
            float num = float.MaxValue;
            Pawn result = null;
            foreach (Pawn item in list)
            {
                if (ValidTarget(item) && pawn.Position.InHorDistOf(item.Position, radius) && (float)item.Position.DistanceToSquared(pawn.Position) < num && GenSight.LineOfSightToThing(pawn.Position, item, pawn.Map))
                {
                    num = item.Position.DistanceToSquared(pawn.Position);
                    result = item;
                }
            }
            //if(result != null)
            //    Log.Warning($"目标：{result.Name} 是最近的目标");
            //else
            //    Log.Warning($"目标：null");

            return result;
        }

        /// <summary>
        /// 死后操作
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="map"></param>
        public static void OnMeatLanternDeath(Pawn pawn, Map map)
        {
            //TODO 这边暂时没用起来，也没调用，参考幽魂
            Find.LetterStack.ReceiveLetter("LetterLabelMeatLanternKilled".Translate(), "LetterMeatLanternKilled".Translate(), LetterDefOf.PositiveEvent, new LookTargets(pawn.PositionHeld, map));
            ((DyingMeatLantern)GenSpawn.Spawn(Def.ThingDefOf.DyingMeatLantern, pawn.Position, map)).InitWith(pawn);
        }

        /// <summary>
        /// 逃脱操作
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="map"></param>
        public static void OnMeatLanternEscape(Pawn pawn, Map map)
        {
            ((EscapingMeatLantern)GenSpawn.Spawn(Def.ThingDefOf.EscapingMeatLantern, pawn.Position, map)).InitWith(pawn, true);
        }

        /// <summary>
        /// 逃脱操作
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="map"></param>
        public static void OnMeatLanternEat(Pawn selfPawn, List<Pawn> victims, Map map)
        {
            ((EatingMeatLantern)GenSpawn.Spawn(Def.ThingDefOf.EatingMeatLantern, selfPawn.Position, map)).InitWith(selfPawn, victims);
        }

        public static void DoDamageByEat(List<Pawn> victims, Pawn selfPawn)
        {
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Cut, 100, 25, -1f, selfPawn);

            foreach (Pawn p in victims)
            {
                if (!p.Dead)
                    p.TakeDamage(dinfo);

            }
        }

        /// <summary>
        /// 嚼嚼旋风技能
        /// </summary>
        /// <param name="victim">受害者</param>
        /// <param name="instigator">加害者</param>
        /// <param name="damageRange">伤害范围</param>
        /// <param name="armorPenetration">破甲率</param>
        /// <param name="percent">治愈比率</param>
        public static void DoBiteOnPawn(Pawn victim, Pawn instigator, FloatRange damageRange, float armorPenetration, float percent)
        {
            float damage = damageRange.RandomInRange;
            DamageInfo dInfo = new DamageInfo(DamageDefOf.Bite, damage, armorPenetration, -1f, instigator, null, Def.ThingDefOf.EgoWeapon_MeatLantern);
            victim.TakeDamage(dInfo);

            //机械族不吸血，但可以造成伤害
            if(!victim.def.race.IsMechanoid)
                DoHeal(instigator, damage, percent);
        }

        private static void DoHeal(Pawn selfPawn, float healAmount, float percent)
        {
            List<Hediff_Injury> list = new List<Hediff_Injury>();
            selfPawn.health.hediffSet.GetHediffs(ref list);

            if (list.Count > 0)
            {
                Hediff_Injury injury = list.RandomElement();
                injury.Heal(healAmount * percent);

                //是否允许显示吸血字样
                if (Setting_MeatLantern_Main.Settings.If_ShowVampireText)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(Translator.Translate("LC_MeatLantern_VampireAmountThrowText"));
                    sb.Append(healAmount * percent);

                    //是否允许显示恢复部位
                    if (Setting_MeatLantern_Main.Settings.If_ShowVampireHealPartText)
                    {
                        sb.Append(Translator.Translate("LC_MeatLantern_VampireBodypartThrowText"));
                        sb.Append(injury.Part.Label.Translate());
                    }

                    MoteMaker.ThrowText(selfPawn.Position.ToVector3(), selfPawn.Map, sb.ToString(), Color.green);
                }

                //是否允许显示治疗特效
                if (Setting_MeatLantern_Main.Settings.If_ShowVampireHealVFX)
                    FleckMaker.ThrowMetaIcon(selfPawn.Position, selfPawn.Map, FleckDefOf.HealingCross, 0.42f);
            }
        }
    }
}
