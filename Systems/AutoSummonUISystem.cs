using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using AutoSummonEX.Config;
using AutoSummonEX.UI;

namespace AutoSummonEX
{
    public class AutoSummonUISystem : ModSystem
    {
        private static UserInterface uiInterface;
        internal static AutoSummonUI summonUI;

        public static bool Visible { get; set; } = false;

        private static Vector2? lastPanelPosition = null;

        private static bool suppressCloseDueToDrag = false; // ✅ 新增：标记是否是拖动引起的位置变动

        public override void Load()
        {
            if (!Main.dedServ)
            {
                uiInterface = new UserInterface();
            }
        }

        public override void Unload()
        {
            summonUI = null;
            uiInterface = null;
            lastPanelPosition = null;
            Visible = false;
            suppressCloseDueToDrag = false;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (!Main.dedServ && Visible)
            {
                uiInterface?.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (!Main.dedServ && Visible && uiInterface?.CurrentState != null)
            {
                int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
                if (mouseTextIndex != -1)
                {
                    layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                        "AutoSummonEX: Summon UI",
                        () =>
                        {
                            uiInterface.Draw(Main.spriteBatch, new GameTime());
                            return true;
                        },
                        InterfaceScaleType.UI)
                    );
                }
            }
        }

        public override void PostUpdateInput()
        {
            if (Main.dedServ) return;

            var config = ModContent.GetInstance<AutoSummonConfig>();

            if (lastPanelPosition.HasValue && config.PanelPosition != lastPanelPosition.Value)
            {
                if (suppressCloseDueToDrag)
                {
                    suppressCloseDueToDrag = false; // ✅ 不关闭，只清除标记
                }
                else if (Visible)
                {
                    HideUI(); // ✅ 只有非拖动修改时才关闭
                }

                lastPanelPosition = config.PanelPosition;
            }

            if (!lastPanelPosition.HasValue)
            {
                lastPanelPosition = config.PanelPosition;
            }
        }

        public static void ShowUI()
        {
            if (!Main.dedServ)
            {
                summonUI = new AutoSummonUI();
                summonUI.Activate();
                uiInterface?.SetState(summonUI);
                Visible = true;
            }
        }

        public static void HideUI()
        {
            if (!Main.dedServ)
            {
                uiInterface?.SetState(null);
                Visible = false;
            }
        }

        // ✅ 新增：供拖动时标记“不要触发关闭逻辑”
        public static void SuppressNextClose()
        {
            suppressCloseDueToDrag = true;
        }
    }
}
