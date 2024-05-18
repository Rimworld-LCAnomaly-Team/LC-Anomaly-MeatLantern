﻿using LCAnomalyLibrary.Misc;
using RimWorld;
using Verse.Sound;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using MeatLantern.Utility;

namespace MeatLantern.Effect
{
    public class EatingMeatLantern : LC_FX_Dying
    {
        private List<Pawn> victims;
        private Pawn selfPawn;

        public override void InitWith(Pawn targetPawn)
        {
            throw new System.NotImplementedException();
        }

        public void InitWith(Pawn selfPawn, List<Pawn> victims)
        {
            this.selfPawn = selfPawn;
            this.victims = victims;

            //播放特效，记录动画播放完的时间
            DoEatEffect(selfPawn);
            completeTick = TickSpawned + 15;
        }

        public override void Complete()
        {
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

            Vector3 loc = new Vector3()
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