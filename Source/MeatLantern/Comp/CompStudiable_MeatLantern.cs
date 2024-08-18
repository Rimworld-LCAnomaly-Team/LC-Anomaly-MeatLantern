using LCAnomalyLibrary.Comp;

namespace MeatLantern.Comp
{
    public class CompStudiable_MeatLantern : LC_CompStudiable
    {
        private LC_CompStudyUnlocks compStudyUnlocks => parent.GetComp<LC_CompStudyUnlocks>();

        public override float GetWorkSpeedOffset()
        {
            if (compStudyUnlocks.Completed)
            {
                return 0.08f;
            }
            else if (compStudyUnlocks.Progress >= 2)
            {
                return 0.04f;
            }

            return 0;
        }

        public override float GetWorkSuccessRateOffset()
        {
            if (compStudyUnlocks.Completed)
            {
                return 0.12f;
            }
            else if (compStudyUnlocks.Progress >= 1)
            {
                return 0.06f;
            }

            return 0;
        }
    }
}
