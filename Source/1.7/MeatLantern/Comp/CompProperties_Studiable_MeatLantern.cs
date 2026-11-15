using LCAnomalyCore.Comp;

namespace MeatLantern.Comp
{
    public class CompProperties_Studiable_MeatLantern : CompProperties_AbnormalityStudiable
    {
        public CompProperties_Studiable_MeatLantern()
        {
            compClass = typeof(CompStudiable_MeatLantern);
        }
    }
}
