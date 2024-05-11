using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using MeatLantern.Effect;

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
            ((EscapingMeatLantern)GenSpawn.Spawn(Def.ThingDefOf.EscapingMeatLantern, pawn.Position, map)).InitWith(pawn);
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
    }
}
