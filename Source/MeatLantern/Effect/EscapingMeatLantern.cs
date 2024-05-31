﻿using LCAnomalyLibrary.Misc;
using MeatLantern.Comp;
using MeatLantern.Job;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MeatLantern.Effect
{
    /// <summary>
    /// 肉食提灯逃脱收容特效对象（有传参和生成逃脱对象的职能）
    /// </summary>
    public class EscapingMeatLantern : LC_FX_Escaping
    {
        public override void InitWith(Pawn meatLantern, bool isEscaping)
        {
            this.isEscaping = isEscaping;

            if(isEscaping)
            {
                //传递生物特征
                bioSignature = meatLantern.TryGetComp<CompMeatLantern>().biosignature;
                //销毁未出逃提灯
                meatLantern.Destroy();
            }

            //放血肉特效，记录动画播放完的时间
            Effecter effecter = EffecterDefOf.MeatExplosionExtraLarge.SpawnMaintained(base.Position, base.Map);
            completeTick = base.TickSpawned + effecter.ticksLeft + 120;
            Def.SoundDefOf.MeatLantern_Escape.PlayOneShot(new TargetInfo(base.Position, base.Map));
        }

        public override void Complete()
        {
            //生成逃脱收容的肉食提灯
            Pawn pawn = PawnGenerator.GeneratePawn(Def.PawnKindDefOf.MeatLanternEscaped, Faction.OfEntities);
            GenSpawn.Spawn(pawn, EscapeToCell(), base.MapHeld);
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
        /// <returns>目标位置</returns>
        private IntVec3 EscapeToCell()
        {
            IntVec3 invalid = IntVec3.Invalid;
            CellFinder.TryFindRandomCell(base.Map, null, out invalid);
            return invalid;
        }
    }
}
