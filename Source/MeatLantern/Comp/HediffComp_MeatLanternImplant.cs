using MeatLantern.Setting;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace MeatLantern.Comp
{
    public class HediffComp_MeatLanternImplant : HediffComp
    {
        private StringBuilder sb_MoteVampire;

        public HediffCompProperties_MeatLanternImplant Props => (HediffCompProperties_MeatLanternImplant)props;

        public override void Notify_SurgicallyRemoved(Pawn surgeon)
        {

        }

        public override void Notify_SurgicallyReplaced(Pawn surgeon)
        {

        }

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
            sb_MoteVampire = new StringBuilder();
        }

        /// <summary>
        /// 当pawn攻击敌人的时候应用
        /// </summary>
        public void Notify_OnSelfPawnAttackOther(float damageAmount)
        {
            HealSelfPawn(Pawn, damageAmount, 0.5f);
        }

        /// <summary>
        /// 治愈身上的随机伤口
        /// </summary>
        /// <param name="selfPawn">pawn</param>
        /// <param name="healAmount">治愈值</param>
        /// <param name="healAmount">治愈比率</param>
        private void HealSelfPawn(Pawn selfPawn, float healAmount, float percent)
        {
            List<Hediff_Injury> list = new List<Hediff_Injury>();
            selfPawn.health.hediffSet.GetHediffs(ref list);

            if(list.Count > 0)
            {
                Hediff_Injury injury = list.RandomElement();
                injury.Heal(healAmount * percent);

                //是否允许显示吸血字样
                if (Setting_MeatLantern_Main.Settings.If_ShowVampireText)
                {
                    
                    sb_MoteVampire.Clear();
                    sb_MoteVampire.Append(Translator.Translate("LC_MeatLantern_VampireAmountThrowText"));
                    sb_MoteVampire.Append(healAmount * percent);

                    //是否允许显示恢复部位
                    if (Setting_MeatLantern_Main.Settings.If_ShowVampireHealPartText)
                    {
                        sb_MoteVampire.Append(Translator.Translate("LC_MeatLantern_VampireBodypartThrowText"));
                        sb_MoteVampire.Append(injury.Part.Label.Translate());
                    }

                    MoteMaker.ThrowText(selfPawn.Position.ToVector3(), selfPawn.Map, sb_MoteVampire.ToString(), Color.green);
                }

                //是否允许显示治疗特效
                if (Setting_MeatLantern_Main.Settings.If_ShowVampireHealVFX)
                    FleckMaker.ThrowMetaIcon(selfPawn.Position, selfPawn.Map, FleckDefOf.HealingCross, 0.42f);
            }
                
        }
    }
}
