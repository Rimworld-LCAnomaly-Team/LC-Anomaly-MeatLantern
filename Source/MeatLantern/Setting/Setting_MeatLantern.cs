using Verse;

namespace MeatLantern.Setting
{
    public class Setting_MeatLantern : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.If_ShowVampireText, "If_ShowVampireText", true, false);
            Scribe_Values.Look<bool>(ref this.If_ShowVampireHealPartText, "If_ShowVampireHealPartText", false, false);
            Scribe_Values.Look<bool>(ref this.If_ShowVampireHealVFX, "If_ShowVampireHealVFX", true, false);
        }

        public bool If_ShowVampireText = true;
        public bool If_ShowVampireHealPartText = false;
        public bool If_ShowVampireHealVFX = true;
    }
}
