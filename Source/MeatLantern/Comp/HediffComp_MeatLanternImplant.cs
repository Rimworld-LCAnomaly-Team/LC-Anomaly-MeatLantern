using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MeatLantern.Comp
{
    public class HediffComp_MeatLanternImplant : HediffComp
    {

        public HediffCompProperties_MeatLanternImplant Props => (HediffCompProperties_MeatLanternImplant)props;

        public override void Notify_SurgicallyRemoved(Pawn surgeon)
        {

        }

        public override void Notify_SurgicallyReplaced(Pawn surgeon)
        {

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
            Log.Message($"吸血咯，healAmount = {healAmount}，percent = {percent}");

            List<Hediff_Injury> list = new List<Hediff_Injury>();
            selfPawn.health.hediffSet.GetHediffs(ref list);

            if(list.Count > 0 )
                list.RandomElement().Heal(healAmount * percent);
        }
    }
}
