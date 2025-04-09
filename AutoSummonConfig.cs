using System.ComponentModel;
using Terraria.ModLoader.Config;
using Microsoft.Xna.Framework;

namespace AutoSummonEX.Config
{
    public class AutoSummonConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [LabelKey("$Mods.AutoSummonEX.Config.PanelPosition")]
        [TooltipKey("$Mods.AutoSummonEX.Config.PanelPosition.Tooltip")]
        [DefaultValue(typeof(Vector2), "600, 300")]
        [Range(0f, 3000f)]
        public Vector2 PanelPosition { get; set; }

        [LabelKey("$Mods.AutoSummonEX.Config.AllowDrag")]
        [TooltipKey("$Mods.AutoSummonEX.Config.AllowDrag.Tooltip")]
        [DefaultValue(false)]
        public bool AllowDrag { get; set; }
    }
}
