using LCAnomalyCore.Comp;

namespace MeatLantern.Comp
{
    public class CompProperties_MeatLanternEgg : LC_CompProperties_InteractableEgg
    {
        /// <summary>
        /// Comp
        /// </summary>
        public CompProperties_MeatLanternEgg()
        {
            compClass = typeof(CompMeatLanternEgg);
        }
    }
}