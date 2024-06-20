using UnityEngine;
using Verse;

namespace MeatLantern.Setting
{
    public class Setting_MeatLantern_Main : Mod
    {
        public Setting_MeatLantern_Main(ModContentPack content) : base(content)
        {
            Setting_MeatLantern_Main.Settings = base.GetSettings<Setting_MeatLantern>();
        }

        public override string SettingsCategory()
        {
            return Translator.Translate("Setting_MeatLantern_Label");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Widgets.Checkbox(0f, 40f, ref Settings.If_ShowVampireText, 24f, false, false, null, null);
            Widgets.Label(new Rect(35f, 41f, inRect.width - 50f, 24f), Translator.Translate("LC_MeatLantern_ShowVampireText"));

            Widgets.Checkbox(0f, 80f, ref Settings.If_ShowVampireHealPartText, 24f, false, false, null, null);
            Widgets.Label(new Rect(35f, 81f, inRect.width - 50f, 24f), Translator.Translate("LC_MeatLantern_ShowVampireHealPartText"));

            Widgets.Checkbox(0f, 120f, ref Settings.If_ShowVampireHealVFX, 24f, false, false, null, null);
            Widgets.Label(new Rect(35f, 121f, inRect.width - 50f, 24f), Translator.Translate("LC_MeatLantern_ShowVampireHealVFX"));

            base.DoSettingsWindowContents(inRect);
        }

        public static Setting_MeatLantern Settings;
    }
}