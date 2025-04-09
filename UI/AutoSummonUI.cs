using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using AutoSummonEX.UI.Controls;
using Terraria.ModLoader;
using AutoSummonEX.Config;
using Terraria.ID;

namespace AutoSummonEX.UI
{
    public class AutoSummonUI : UIState
    {
        private DragableUIPanel panel;
        private UIText titleText;
        private UIText minionSlotText;
        private UIText sentrySlotText;
        private AutoSummonItemSlot minionItemSlot;
        private AutoSummonItemSlot sentryItemSlot;
        private UIFancyButton toggleButton;
        private UIFancyButton summonButton;
        private UIFancyButton desummonButton;

        private UIElement panelBottomSpacer;

        private bool autoSummonOn = false;

        public override void OnInitialize()
        {
            panel = new DragableUIPanel();
            var config = ModContent.GetInstance<AutoSummonConfig>();
            panel.Left.Set(config.PanelPosition.X, 0f);
            panel.Top.Set(config.PanelPosition.Y, 0f);
            panel.Width.Set(300f, 0f);
            panel.SetPadding(10);
            panel.SetDraggable(config.AllowDrag);
            Append(panel);

            float nextTop = 0f;

            titleText = new UIText(Language.GetTextValue("Mods.AutoSummonEX.UI.PanelTitle"));
            titleText.Top.Set(nextTop, 0f);
            titleText.HAlign = 0.5f;
            panel.Append(titleText);
            nextTop += 30f;

            var minionPanel = new UISectionPanel(Language.GetTextValue("Mods.AutoSummonEX.UI.MinionSectionTitle"), 0f, 220f);// 子面板标题
            minionPanel.Top.Set(nextTop, 0f);
            panel.Append(minionPanel);
            nextTop += minionPanel.Height.Pixels + 28f;

            minionItemSlot = new AutoSummonItemSlot(0.85f);// 仆从槽位
            minionItemSlot.HAlign = 0.5f;
            minionItemSlot.Top.Set(35f, 0f); // 距离子面板顶部一些距离
            minionPanel.Append(minionItemSlot);

            minionItemSlot.CanAcceptItem = item =>
            {
                if (item == null || item.IsAir || item.shoot <= ProjectileID.None)
                    return false;

                Projectile p = new Projectile();
                p.SetDefaults(item.shoot);

                bool result = p.minion && !p.sentry;
                return result;
            };

            // 仆从栏数值文本
            minionSlotText = new UIText("");
            minionSlotText.Top.Set(minionPanel.Height.Pixels - 40f, 0f); // 调整为你希望的位置
            minionSlotText.HAlign = 0.5f;
            minionPanel.Append(minionSlotText);

            var sentryPanel = new UISectionPanel(Language.GetTextValue("Mods.AutoSummonEX.UI.SentrySectionTitle"), 0f, 220f);// 子面板标题
            sentryPanel.Top.Set(nextTop, 0f);
            panel.Append(sentryPanel);
            nextTop += sentryPanel.Height.Pixels + 28f;

            sentryItemSlot = new AutoSummonItemSlot(0.85f);// 哨兵槽位
            sentryItemSlot.HAlign = 0.5f;
            sentryItemSlot.Top.Set(35f, 0f);
            sentryPanel.Append(sentryItemSlot);

            // ✅ 限制为哨兵武器
            sentryItemSlot.CanAcceptItem = item =>
            {
                if (item == null || item.IsAir)
                    return false;
                bool result = item.CountsAsClass(DamageClass.Summon) && item.sentry;
                return result;
            };

            // 哨兵栏数值文本
            sentrySlotText = new UIText("");
            sentrySlotText.Top.Set(minionPanel.Height.Pixels - 40f, 0f);
            sentrySlotText.HAlign = 0.5f;
            sentryPanel.Append(sentrySlotText);

            float buttonSpacing = 44f;

            toggleButton = new UIFancyButton(GetToggleText());
            toggleButton.Top.Set(nextTop, 0f);
            toggleButton.HAlign = 0.5f;
            toggleButton.OnClick += ToggleAuto;
            panel.Append(toggleButton);

            summonButton = new UIFancyButton(Language.GetTextValue("Mods.AutoSummonEX.UI.Button.Summon"));
            summonButton.Top.Set(nextTop + buttonSpacing, 0f);
            summonButton.HAlign = 0.5f;
            panel.Append(summonButton);

            desummonButton = new UIFancyButton(Language.GetTextValue("Mods.AutoSummonEX.UI.Button.DeSummon"));
            desummonButton.Top.Set(nextTop + buttonSpacing * 2, 0f);
            desummonButton.HAlign = 0.5f;
            panel.Append(desummonButton);

            panelBottomSpacer = new UIElement();
            panelBottomSpacer.Top.Set(nextTop + buttonSpacing * 3 + 10f, 0f);
            panelBottomSpacer.Height.Set(1f, 0f);
            panel.Append(panelBottomSpacer);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float bottom = panelBottomSpacer.Top.Pixels + panelBottomSpacer.GetOuterDimensions().Height;
            panel.Height.Set(bottom + 10f, 0f);
            panel.Recalculate();

            Player player = Main.LocalPlayer;

            float usedMinions = player.slotsMinions;
            int maxMinions = player.maxMinions;
            minionSlotText.SetText(Language.GetTextValue("Mods.AutoSummonEX.UI.MinionSlotInfo") + usedMinions.ToString("0.0") + " / " + maxMinions);

            int maxSentries = player.maxTurrets;
            int activeSentries = 0;
            foreach (var proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.sentry)
                {
                    activeSentries++;
                }
            }
            sentrySlotText.SetText(Language.GetTextValue("Mods.AutoSummonEX.UI.SentrySlotInfo") + activeSentries.ToString() + " / " + maxSentries);
        }

        private void ToggleAuto()
        {
            autoSummonOn = !autoSummonOn;
            toggleButton.SetText(GetToggleText());

            string msg = Language.GetTextValue("Mods.AutoSummonEX.UI.Button." + (autoSummonOn ? "Enable" : "Disable"));
            Main.NewText($"自动召唤已切换：{msg}");
        }

        private string GetToggleText()
        {
            return Language.GetTextValue("Mods.AutoSummonEX.UI.Button." + (autoSummonOn ? "Disable" : "Enable"));
        }

    }
}
