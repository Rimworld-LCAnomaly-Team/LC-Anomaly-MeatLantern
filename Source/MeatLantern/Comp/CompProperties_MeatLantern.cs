using LCAnomalyLibrary.Comp;

namespace MeatLantern.Comp
{
    public class CompProperties_MeatLantern : LC_CompProperties_Entity
    {
        public int eatCooldownTick = 600;

        public CompProperties_MeatLantern()
        {
            compClass = typeof(CompMeatLantern);
        }
    }
}