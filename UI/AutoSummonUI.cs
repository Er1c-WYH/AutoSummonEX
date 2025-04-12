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

namespace AutoSummonEX.UI
{
    public class AutoSummonUI : UIState
    {
        private DragableUIPanel panel;
        private UIText titleText;
        private UIText minionSlotText;
        private UIText sentrySlotText;
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

            // --- 仆从标题面板 ---
            minionPanel = new UISectionPanel(Language.GetTextValue("Mods.AutoSummonEX.UI.MinionSectionTitle"), 0f, 260f); // 高度稍微加一点以容纳子面板
            minionPanel.Top.Set(nextTop, 0f);
            panel.Append(minionPanel);
            minionPanel.HAlign = 0.5f; // ✅ 水平居中
            nextTop += minionPanel.Height.Pixels + 28f;

            // ✅ 创建子面板并添加到 UISectionPanel 内部
            minionSubPanel = new MinionSlotSubPanel();
            minionSubPanel.HAlign = 0.5f; // 居中
            minionSubPanel.Top.Set(minionPanel.GetContentStartY(), 0f);
            minionSubPanel.GetPlayerMaxMinionSlots = () => Main.LocalPlayer.maxMinions;
            minionPanel.Append(minionSubPanel);

            float subPanelWidth = minionSubPanel.GetPanelFullWidth();
            minionPanel.Width.Set(subPanelWidth + 20f, 0f); // 额外 padding

            // ✅ 仆从栏数值文本（仍添加在 UISectionPanel 内部）
            minionSlotText = new UIText("");
            minionSlotText.Top.Set(minionPanel.Height.Pixels - 40f, 0f);
            minionSlotText.HAlign = 0.5f;
            minionPanel.Append(minionSlotText);

            sentryPanel = new UISectionPanel(Language.GetTextValue("Mods.AutoSummonEX.UI.SentrySectionTitle"), 0f, 220f);// 子面板标题
            sentryPanel.Top.Set(nextTop, 0f);
            panel.Append(sentryPanel);
            sentryPanel.HAlign = 0.5f; // 水平居中
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

            float minPanelWidth = 400f;
            float dynamicPanelWidth = minionPanel.GetDimensions().Width + 40f;

            panel.Width.Set(Math.Max(minPanelWidth, dynamicPanelWidth), 0f);
        }

        private float initialPanelWidth = -1f;
        private const float panelWidthBuffer = 30f;

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // ✅ 鼠标悬停整个 UI，禁止攻击
            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.blockMouse = true;
            }

            // ✅ 计算当前子面板推荐宽度
            float baseWidth = minionSubPanel.GetPanelFullWidth() + 20f;

            // ✅ 第一次进入：记录初始宽度 + 缓冲
            if (initialPanelWidth < 0f)
            {
                initialPanelWidth = baseWidth + panelWidthBuffer;
                minionPanel.Width.Set(initialPanelWidth, 0f);
            }
            else
            {
                // ✅ 后续：仅当宽度超出原始宽度才更新（防止轻微抖动）
                if (baseWidth > initialPanelWidth)
                {
                    minionPanel.Width.Set(baseWidth, 0f);
                }
            }

            // ✅ 哨兵面板始终与仆从面板等宽
            sentryPanel.Width.Set(minionPanel.Width.Pixels, 0f);

            // ✅ 同步外层 panel 宽度（自动适配所有内容）
            float minPanelWidth = 400f;
            float dynamicPanelWidth = minionPanel.GetDimensions().Width + 40f;
            panel.Width.Set(Math.Max(minPanelWidth, dynamicPanelWidth), 0f);

            // ✅ 同步外层 panel 高度
            float bottom = panelBottomSpacer.Top.Pixels + panelBottomSpacer.GetOuterDimensions().Height;
            panel.Height.Set(bottom + 10f, 0f);
            panel.Recalculate();

            // ✅ 仆从栏位信息更新
            Player player = Main.LocalPlayer;

            float usedMinions = player.slotsMinions;
            int maxMinions = player.maxMinions;
            minionSlotText.SetText(Language.GetTextValue("Mods.AutoSummonEX.UI.MinionSlotInfo") + usedMinions.ToString("0.0") + " / " + maxMinions);

            // ✅ 哨兵栏位信息更新
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
