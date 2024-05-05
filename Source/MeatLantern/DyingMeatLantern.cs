using RimWorld;
using Verse;

namespace MeatLantern
{
    /// <summary>
    /// 肉食提灯死亡特效对象（有传参和生成蛋对象的职能）
    /// </summary>
    public class DyingMeatLantern : Thing
    {
        private int bioSignature;

        private int completeTick;

        public override void Tick()
        {
            //动画播完就执行操作
            if (Find.TickManager.TicksGame >= completeTick)
            {
                Complete();
            }
        }

        public void InitWith(Pawn meatLantern)
        {
            //传递生物特征，播放effecter特效，记录动画播放完的时间
            bioSignature = meatLantern.TryGetComp<CompMeatLantern>().biosignature;
            Log.Warning($"bioget={bioSignature}");
            Effecter effecter = EffecterDefOf.MeatExplosionExtraLarge.SpawnMaintained(base.Position, base.Map);
            completeTick = base.TickSpawned + effecter.ticksLeft;
        }

        public void Complete()
        {
            //if (FilthMaker.TryMakeFilth(base.PositionHeld, base.Map, ThingDefOf.Filth_RevenantBloodPool))
            //{
            //    EffecterDefOf.RevenantKilledCompleteBurst.SpawnMaintained(base.PositionHeld, base.Map);
            //    foreach (IntVec3 item in CellRect.CenteredOn(base.PositionHeld, 2))
            //    {
            //        Plant plant = item.GetPlant(base.Map);
            //        if (plant != null && plant.MaxHitPoints < 100)
            //        {
            //            plant.Destroy();
            //        }
            //    }
            //}

            //生成肉食提灯的蛋，销毁自己
            Thing thing = ThingMaker.MakeThing(ThingDefOf.MeatLanternEgg);
            thing.TryGetComp<CompBiosignatureOwner>().biosignature = bioSignature;
            GenSpawn.Spawn(thing, base.PositionHeld, base.Map);
            Destroy();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref completeTick, "completeTick", 0);
            Scribe_Values.Look(ref bioSignature, "bioSignature", 0);
        }
    }
}
