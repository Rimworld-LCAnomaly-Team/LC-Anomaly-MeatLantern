﻿using LCAnomalyCore.Misc;
using MeatLantern.Utility;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MeatLantern.Effect
{
    /// <summary>
    /// 肉食提灯吞噬特效对象
    /// </summary>
    public class EatingMeatLantern : LC_FX_Standard
    {
        private List<Pawn> victims;
        private Pawn selfPawn;

        public void InitWith(Pawn selfPawn, List<Pawn> victims)
        {
            this.selfPawn = selfPawn;
            this.victims = victims;

            //播放特效，记录动画播放完的时间
            DoEatEffect(selfPawn);
            completeTick = TickSpawned + 15;

            hasInited = true;
        }

        public override void Complete()
        {
            if (!hasInited)
            {
                //Log.Warning($"特效：在未初始化时尝试生成特效对象{Def.ThingDefOf.EatingMeatLantern.label.Translate()}，对象即将被销毁以避免错误。");
                Destroy();
                return;
            }

            //执行伤害方法
            MeatLanternUtility.DoDamageByEat(this.victims, this.selfPawn);

            //销毁自己
            Destroy();
        }

        /// <summary>
        /// 生成吞噬特效（fleck + sound）
        /// </summary>
        private void DoEatEffect(Pawn selfPawn)
        {
            Vector3 temp = selfPawn.Position.ToVector3();

            Vector3 loc = new()
            {
                x = temp.x + 0.5f,
                y = temp.y,
                z = temp.z + 1.5f
            };

            Def.SoundDefOf.MeatLantern_Eat.PlayOneShot(new TargetInfo(selfPawn.Position, selfPawn.Map));
            FleckMaker.ThrowFireGlow(loc, selfPawn.Map, 2);
            FleckMaker.Static(loc, selfPawn.Map, Def.FleckDefOf.MeatLantern_Escaped_Mouth_Eat);
        }
    }
}