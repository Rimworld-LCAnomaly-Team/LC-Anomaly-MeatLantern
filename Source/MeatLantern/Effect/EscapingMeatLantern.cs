using LCAnomalyCore.Misc;
using MeatLantern.Comp;
using MeatLantern.Job;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MeatLantern.Effect
{
    /// <summary>
    /// 肉食提灯逃脱收容特效对象
    /// </summary>
    public class EscapingMeatLantern : LC_FX_Standard
    {
        public override void InitWith(Pawn targetPawn)
        {
            DoDefaultFX();
        }

        public void InitWithEscaping(Pawn targetPawn)
        {
            this.isEscaping = true;
            var comp = targetPawn.TryGetComp<CompMeatLantern>();

            //传递生物特征
            bioSignature = comp.biosignature;

            //销毁未出逃提灯
            targetPawn.Destroy();

            DoDefaultFX();
        }

        private void DoDefaultFX()
        {
            //播放血肉特效，记录动画播放完的时间
            Effecter effecter = EffecterDefOf.MeatExplosionExtraLarge.SpawnMaintained(base.Position, base.Map);
            completeTick = base.TickSpawned + effecter.ticksLeft + 120;
            Def.SoundDefOf.MeatLantern_Escape.PlayOneShot(new TargetInfo(base.Position, base.Map));

            hasInited = true;
        }

        public override void Complete()
        {
            if (!hasInited)
            {
                //Log.Warning($"特效：在未初始化时尝试生成特效对象{Def.ThingDefOf.EscapingMeatLantern.label.Translate()}，对象即将被销毁以避免错误。");
                Destroy();
                return;
            }

            //生成逃脱收容的肉食提灯
            Pawn pawn = PawnGenerator.GeneratePawn(Def.PawnKindDefOf.MeatLanternEscaped, Faction.OfEntities);
            GenSpawn.Spawn(pawn, TryFindTargetCell(50), base.MapHeld);
            CompMeatLantern compMeatLantern = pawn.TryGetComp<CompMeatLantern>();
            compMeatLantern.SetState(MeatLanternState.Wait);

            if (isEscaping)
            {
                //传递生物特征
                compMeatLantern.biosignature = bioSignature;
            }

            //销毁自己
            Destroy();
        }

        /// <summary>
        /// 瞬移逃到某个位置
        /// </summary>
        /// <param name="radius">范围</param>
        /// <returns>目标位置</returns>
        private IntVec3 TryFindTargetCell(int radius)
        {
            IntVec3 pos = base.Position;

            CellFinder.TryFindRandomReachableNearbyCell(
                base.Position, base.MapHeld
                , radius, TraverseParms.For(TraverseMode.PassDoors)
                , null, null, out pos);

            return pos;
        }
    }
}