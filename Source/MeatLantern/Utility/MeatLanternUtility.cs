using MeatLantern.Effect;
using MeatLantern.Setting;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MeatLantern.Utility
{
    public static class MeatLanternUtility
    {
        #region 字段

        public static readonly FloatRange SearchForTargetCooldownRangeDays = new(1f, 2f);

        private readonly static HashSet<Pawn> tmpTargets = [];

        #endregion

        #region 方法

        #region 肉食提灯

        /// <summary>
        /// 在Pawn周围扫描目标
        /// </summary>
        /// <param name="pawn">pawn</param>
        /// <param name="radius">范围</param>
        /// <returns>符合条件的受害者List</returns>
        public static List<Pawn> ScanForTarget_NewTemp(Pawn pawn, float radius)
        {
            tmpTargets.Clear();
            TraverseParms traverseParms = TraverseParms.For(TraverseMode.NoPassClosedDoorsOrWater);

            RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to)
                => to.Allows(traverseParms, isDestination: true), delegate (Region x)
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
                return [.. tmpTargets];
            }

            return null;
        }

        /// <summary>
        /// 验证目标是否合法
        /// </summary>
        /// <param name="pawn">目标</param>
        /// <returns>是否合法</returns>
        public static bool ValidTarget(Pawn pawn)
        {
            //不吃同类和倒地的单位
            return ((pawn.kindDef != Def.PawnKindDefOf.MeatLanternEscaped)
                && (pawn.kindDef != Def.PawnKindDefOf.MeatLanternContained)
                && !pawn.Downed);
        }

        /// <summary>
        /// 验证附近是否有人类单位
        /// </summary>
        /// <param name="pos">位置</param>
        /// <param name="map">地图</param>
        /// <param name="radius">范围</param>
        /// <returns></returns>
        public static int NearbyHumanlikePawnCount(IntVec3 pos, Map map, float radius)
        {
            int num = 0;
            foreach (var item in map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn).OfType<Pawn>())
            {
                if (ValidTarget(item) && !item.Downed && pos.InHorDistOf(item.Position, radius))
                {
                    num++;
                }
            }

            return num;
        }

        /// <summary>
        /// 获取最近的目标
        /// </summary>
        /// <param name="pawn">单位</param>
        /// <param name="radius">范围</param>
        /// <returns>符合条件的目标</returns>
        public static Pawn GetClosestTargetInRadius(Pawn pawn, float radius)
        {
            List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
            float num = float.MaxValue;
            Pawn result = null;
            foreach (Pawn item in list.OfType<Pawn>())
            {
                if (ValidTarget(item) && pawn.Position.InHorDistOf(item.Position, radius)
                    && item.Position.DistanceToSquared(pawn.Position) < num
                    && GenSight.LineOfSightToThing(pawn.Position, item, pawn.Map))
                {
                    num = item.Position.DistanceToSquared(pawn.Position);
                    result = item;
                }
            }

            return result;
        }

        /// <summary>
        /// 肉食提灯死后操作
        /// </summary>
        /// <param name="pawn">单位</param>
        /// <param name="map">地图</param>
        public static void OnMeatLanternDeath(Pawn pawn,Map map)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelMeatLanternKilled".Translate()
                , "LetterMeatLanternKilled".Translate()
                , LetterDefOf.PositiveEvent
                , new LookTargets(pawn.PositionHeld, pawn.MapHeld));

            ((DyingMeatLantern)GenSpawn
                .Spawn(Def.ThingDefOf.DyingMeatLantern, pawn.Position, map))
                .InitWith(pawn);
        }

        /// <summary>
        /// 肉食提灯逃脱操作
        /// </summary>
        /// <param name="pawn">单位</param>
        /// <param name="map">地图</param>
        public static void OnMeatLanternEscape(Pawn pawn, Map map)
        {
            ((EscapingMeatLantern)GenSpawn
                .Spawn(Def.ThingDefOf.EscapingMeatLantern, pawn.Position, map))
                .InitWithEscaping(pawn);
        }

        /// <summary>
        /// 肉食提灯吞噬操作
        /// </summary>
        /// <param name="instigator">加害者</param>
        /// <param name="victims">受害者List</param>
        public static void OnMeatLanternEat(Pawn instigator, List<Pawn> victims)
        {
            ((EatingMeatLantern)GenSpawn
                .Spawn(Def.ThingDefOf.EatingMeatLantern, instigator.Position, instigator.MapHeld))
                .InitWith(instigator, victims);
        }

        /// <summary>
        /// 应用吞噬伤害
        /// </summary>
        /// <param name="victims">受害者List</param>
        /// <param name="selfPawn">加害者</param>
        public static void DoDamageByEat(List<Pawn> victims, Pawn instigator)
        {
            DamageInfo dinfo = new(DamageDefOf.Cut, 100, 25, -1f, instigator);

            foreach (Pawn p in victims)
            {
                if (!p.Dead)
                    p.TakeDamage(dinfo);
            }
        }

        #endregion

        #region EGO

        /// <summary>
        /// 嚼嚼旋风技能
        /// </summary>
        /// <param name="victim">受害者</param>
        /// <param name="instigator">加害者</param>
        /// <param name="damageRange">伤害范围</param>
        /// <param name="armorPenetration">破甲率</param>
        /// <param name="suckPercent">治愈比率</param>
        /// <param name="suckPercent">击晕比率</param>
        public static void DoBiteOnPawn(Pawn victim, Pawn instigator, FloatRange damageRange, float armorPenetration, float suckPercent, float stunPercent)
        {
            //造成伤害
            float damage = damageRange.RandomInRange;
            DamageInfo dInfo = new(DamageDefOf.Bite, damage, armorPenetration, -1f, instigator, null, Def.ThingDefOf.EgoWeapon_MeatLantern);
            victim.TakeDamage(dInfo);

            //机械族不可被击晕和吸血
            if (!victim.def.race.IsMechanoid)
            {
                DoHeal(instigator, damage, suckPercent);

                //击晕
                DamageInfo dInfo_Stun = new(DamageDefOf.Stun, damage * stunPercent);
                victim.TakeDamage(dInfo_Stun);

                EffecterDefOf.MeatExplosionSmall.SpawnMaintained(victim.Position, victim.MapHeld);
            }
            else
            {
                EffecterDefOf.MetalhorrorDeath.SpawnMaintained(victim.Position, victim.MapHeld);
            }

            Def.SoundDefOf.MeatLantern_Eat.PlayOneShot(new TargetInfo(victim.Position, victim.MapHeld));
        }

        /// <summary>
        /// 吸血恢复
        /// </summary>
        /// <param name="selfPawn">单位</param>
        /// <param name="healAmount">恢复量</param>
        /// <param name="percent">恢复比率</param>
        private static void DoHeal(Pawn selfPawn, float healAmount, float percent)
        {
            List<Hediff_Injury> list = [];
            selfPawn.health.hediffSet.GetHediffs(ref list);

            if (list.Count > 0)
            {
                Hediff_Injury injury = list.RandomElement();
                injury.Heal(healAmount * percent);

                //是否允许显示吸血字样
                if (Setting_MeatLantern_Main.Settings.If_ShowVampireText)
                {
                    StringBuilder sb = new();
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

        #endregion

        #endregion
    }
}