using LCAnomalyLibrary.Misc;
using RimWorld;
using Verse;

namespace MeatLantern
{
    /// <summary>
    /// 肉食提灯逃脱收容特效对象（有传参和生成逃脱对象的职能）
    /// </summary>
    public class EscapingMeatLantern : LC_FX_Dying
    {
        public override void InitWith(Pawn meatLantern)
        {
            //传递生物特征，播放effecter特效，记录动画播放完的时间
            bioSignature = meatLantern.TryGetComp<CompMeatLantern>().biosignature;
            Effecter effecter = EffecterDefOf.MeatExplosionExtraLarge.SpawnMaintained(base.Position, base.Map);
            completeTick = base.TickSpawned + effecter.ticksLeft;
        }

        public override void Complete()
        {
            //生成逃脱收容的肉食提灯，传递生物特征，销毁未出逃的提灯和自己
            Pawn pawn = PawnGenerator.GeneratePawn(KindDefOf.MeatLanternEscaped, Faction.OfEntities);
            GenSpawn.Spawn(pawn, base.PositionHeld, base.MapHeld);
            CompMeatLantern compMeatLantern = pawn.TryGetComp<CompMeatLantern>();

            compMeatLantern.biosignature = bioSignature;
            compMeatLantern.Invisibility.BecomeInvisible(true);
            compMeatLantern.SetState(MeatLanternState.Wait);

            compMeatLantern.parent.Destroy();
            Destroy();
        }
    }
}
