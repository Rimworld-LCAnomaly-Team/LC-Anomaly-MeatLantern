using LCAnomalyLibrary.Misc;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MeatLantern
{
    /// <summary>
    /// 肉食提灯逃脱收容特效对象（有传参和生成逃脱对象的职能）
    /// </summary>
    public class EscapingMeatLantern : LC_FX_Dying
    {
        public override void InitWith(Pawn meatLantern)
        {
            //传递生物特征，销毁未出逃提灯，播放血肉特效，记录动画播放完的时间
            bioSignature = meatLantern.TryGetComp<CompMeatLantern>().biosignature;
            meatLantern.Destroy();

            Effecter effecter = EffecterDefOf.MeatExplosionExtraLarge.SpawnMaintained(base.Position, base.Map);
            completeTick = base.TickSpawned + effecter.ticksLeft + 120;
            SoundDefOf.MeatLantern_Escape.PlayOneShot(new TargetInfo(base.Position, base.Map));
        }

        public override void Complete()
        {
            //生成逃脱收容的肉食提灯，传递生物特征，销毁自己
            Pawn pawn = PawnGenerator.GeneratePawn(KindDefOf.MeatLanternEscaped, Faction.OfEntities);
            GenSpawn.Spawn(pawn, EscapeToCell(), base.MapHeld);
            CompMeatLantern compMeatLantern = pawn.TryGetComp<CompMeatLantern>();

            compMeatLantern.biosignature = bioSignature;
            compMeatLantern.SetState(MeatLanternState.Wait);

            Destroy();
        }

        /// <summary>
        /// 瞬移逃到某个位置
        /// </summary>
        /// <returns>目标位置</returns>
        private IntVec3 EscapeToCell()
        {
            IntVec3 invalid = IntVec3.Invalid;
            CellFinder.TryFindRandomCell(base.Map, null, out invalid);
            return invalid;
        }
    }
}
