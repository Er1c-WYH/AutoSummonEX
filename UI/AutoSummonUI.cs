using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;
using AutoSummonEX.UI.Controls;
using Terraria.ModLoader;
using AutoSummonEX.Config;
using AutoSummonEX.UI.Panels;
using System;
using Terraria.ID;

// ⚠️ 前略：using 语句不变

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

        private UISectionPanel minionPanel;
        private UISectionPanel sentryPanel;

        private MinionSlotSubPanel minionSubPanel;

        public override void OnInitialize()
        {
            panel = new DragableUIPanel();
            var config = ModContent.GetInstance<AutoSummonConfig>();
            panel.Left.Set(config.PanelPosition.X, 0f);
            panel.Top.Set(config.PanelPosition.Y, 0f);
            panel.Width.Set(400f, 0f);
            panel.SetPadding(10);
            panel.SetDraggable(config.AllowDrag);
            Append(panel);

            float nextTop = 0f;

            titleText = new UIText(Language.GetTextValue("Mods.AutoSummonEX.UI.PanelTitle"));
            titleText.Top.Set(nextTop, 0f);
            titleText.HAlign = 0.5f;
            panel.Append(titleText);
            nextTop += 30f;

            // ── 仆从区域 ──
            minionPanel = new UISectionPanel(Language.GetTextValue("Mods.AutoSummonEX.UI.MinionSectionTitle"), 0f, 260f);
            minionPanel.Top.Set(nextTop, 0f);
            minionPanel.HAlign = 0.5f;
            panel.Append(minionPanel);
            nextTop += minionPanel.Height.Pixels + 28f;

            // ✅ 仆从槽（移出子面板，始终存在）
            minionItemSlot = new AutoSummonItemSlot(0.85f);
            minionItemSlot.Left.Set(10f, 0f);
            minionItemSlot.Top.Set(minionPanel.GetContentStartY(), 0f);
            minionItemSlot.CanAcceptItem = item =>
            {
                if (item == null || item.IsAir || item.shoot <= ProjectileID.None)
                    return false;
                Projectile p = new Projectile();
                p.SetDefaults(item.shoot);
                return p.minion && !p.sentry;
            };
            minionPanel.Append(minionItemSlot);

            // ✅ 子面板（不嵌入槽中，动态显示）
            minionSubPanel = new MinionSlotSubPanel();
            minionSubPanel.Top.Set(minionItemSlot.Top.Pixels, 0f);
            minionSubPanel.GetPlayerMaxMinionSlots = () => Main.LocalPlayer.maxMinions;

            // 仆从栏位信息
            minionSlotText = new UIText("");
            minionSlotText.Top.Set(minionPanel.Height.Pixels - 40f, 0f);
            minionSlotText.HAlign = 0.5f;
            minionPanel.Append(minionSlotText);

            // ── 哨兵区域 ──
            sentryPanel = new UISectionPanel(Language.GetTextValue("Mods.AutoSummonEX.UI.SentrySectionTitle"), 0f, 220f);
            sentryPanel.Top.Set(nextTop, 0f);
            sentryPanel.HAlign = 0.5f;
            panel.Append(sentryPanel);
            nextTop += sentryPanel.Height.Pixels + 28f;

            sentryItemSlot = new AutoSummonItemSlot(0.85f);
            sentryItemSlot.HAlign = 0.5f;
            sentryItemSlot.Top.Set(35f, 0f);
            sentryItemSlot.CanAcceptItem = item =>
            {
                if (item == null || item.IsAir)
                    return false;
                return item.CountsAsClass(DamageClass.Summon) && item.sentry;
            };
            sentryPanel.Append(sentryItemSlot);

            sentrySlotText = new UIText("");
            sentrySlotText.Top.Set(sentryPanel.Height.Pixels - 40f, 0f);
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

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.blockMouse = true;
            }

            // ✅ 控制子面板出现
            if (!minionItemSlot.Item.IsAir)
            {
                if (!minionPanel.HasChild(minionSubPanel))
                    minionPanel.Append(minionSubPanel);

                float left = minionItemSlot.Left.Pixels + minionItemSlot.Width.Pixels + 10f;
                minionSubPanel.Left.Set(left, 0f);
                minionSubPanel.Recalculate();
            }
            else
            {
                if (minionPanel.HasChild(minionSubPanel))
                    minionPanel.RemoveChild(minionSubPanel);
            }

            // ✅ 动态宽度
            float subRight = minionSubPanel?.Parent != null
                ? minionSubPanel.Left.Pixels + minionSubPanel.Width.Pixels
                : minionItemSlot.Left.Pixels + minionItemSlot.Width.Pixels;

            float panelWidth = Math.Max(400f, subRight + 40f);
            panel.Width.Set(panelWidth, 0f);

            // ✅ 同步高度
            float bottom = panelBottomSpacer.Top.Pixels + panelBottomSpacer.GetOuterDimensions().Height;
            panel.Height.Set(bottom + 10f, 0f);
            panel.Recalculate();

            // ✅ 仆从栏位信息
            Player player = Main.LocalPlayer;
            float usedMinions = player.slotsMinions;
            int maxMinions = player.maxMinions;
            minionSlotText.SetText(Language.GetTextValue("Mods.AutoSummonEX.UI.MinionSlotInfo") + usedMinions.ToString("0.0") + " / " + maxMinions);

            // ✅ 哨兵栏位信息
            int maxSentries = player.maxTurrets;
            int activeSentries = 0;
            foreach (var proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.sentry)
                    activeSentries++;
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
