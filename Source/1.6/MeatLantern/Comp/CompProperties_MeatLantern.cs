using LCAnomalyCore.Comp;

namespace MeatLantern.Comp
{
    public class CompProperties_MeatLantern : CompProperties_Abnormality
    {
        public int eatCooldownTick = 600;

        public CompProperties_MeatLantern()
        {
            compClass = typeof(CompMeatLantern);
        }
    }
}
