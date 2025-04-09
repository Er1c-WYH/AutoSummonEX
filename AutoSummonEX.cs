using Terraria.ModLoader;

namespace AutoSummonEX
{
    public class AutoSummonEX : Mod
    {
        public static AutoSummonEX Instance { get; private set; }

        public static ModKeybind ToggleAutoSummonUIKeybind;

        public bool UIVisible => AutoSummonUISystem.Visible;

        public override void Load()
        {
            Instance = this;
            ToggleAutoSummonUIKeybind = KeybindLoader.RegisterKeybind(this, "Toggle Auto Summon UI", "F21");
            // SentryClassifier.BuildCache();
        }

        public override void Unload()
        {
            ToggleAutoSummonUIKeybind = null;
            Instance = null;
        }

        public void ShowUI()
        {
            AutoSummonUISystem.Visible = true;
        }

        public void CloseUI()
        {
            AutoSummonUISystem.Visible = false;
        }
    }
}
