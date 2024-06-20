using RimWorld;
using Verse;
using LCAnomalyLibrary.Misc;
using MeatLantern.Comp;

namespace MeatLantern.Effect
{
    /// <summary>
    /// 肉食提灯死亡特效对象（有传参和生成蛋对象的职能）
    /// </summary>
    public class DyingMeatLantern : LC_FX_Dying
    {
        public override void InitWith(Pawn meatLantern)
        {
            //传递生物特征，播放effecter特效，记录动画播放完的时间
            bioSignature = meatLantern.TryGetComp<CompMeatLantern>().biosignature;
            Effecter effecter = EffecterDefOf.MeatExplosionExtraLarge.SpawnMaintained(base.Position, base.Map);
            completeTick = base.TickSpawned + effecter.ticksLeft + 60;
        }

        public override void Complete()
        {
            //生成扭曲血肉脏污
            if (FilthMaker.TryMakeFilth(base.PositionHeld, base.Map, RimWorld.ThingDefOf.Filth_TwistedFlesh))
            {
                //清除1格以内的植物
                foreach (IntVec3 item in CellRect.CenteredOn(base.PositionHeld, 1))
                {
                    Plant plant = item.GetPlant(base.Map);
                    if (plant != null && plant.MaxHitPoints < 100)
                    {
                        plant.Destroy();
                    }
                }
            }

            //生成肉食提灯的蛋，销毁自己
            Thing thing = ThingMaker.MakeThing(Def.ThingDefOf.MeatLanternEgg);
            thing.TryGetComp<CompBiosignatureOwner>().biosignature = bioSignature;
            GenSpawn.Spawn(thing, base.PositionHeld, base.Map);
            Destroy();
        }
    }
}
