using Microsoft.Xna.Framework;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria;
using AutoSummonEX.UI.Controls;
using System;
using Terraria.ID;

namespace AutoSummonEX.UI.Panels
{
    public class MinionSlotSubPanel : UIPanel
    {
        public AutoSummonItemSlot ItemSlot { get; private set; }

        private UIText labelCost;
        private UIText labelCount;
        private UIText labelTotal;

        private MiniButton btnCostAdd;
        private MiniButton btnCostSub;

        private MiniButton btnCountAdd;
        private MiniButton btnCountSub;
        private MiniButton btnFill;

        private int slotCost = 1;
        private int usageCount = 1;

        private int delayedFrames = 0;

        public Func<float> GetPlayerMaxMinionSlots;

        public MinionSlotSubPanel()
        {
            this.SetPadding(10);
            this.BackgroundColor = new Color(50, 60, 100) * 0.85f;
            this.BorderColor = new Color(30, 30, 40);
            this.Width.Set(360f, 0f);
            this.Height.Set(95f, 0f);

            ItemSlot = new AutoSummonItemSlot(0.85f);
            ItemSlot.Left.Set(10f, 0f);
            ItemSlot.Top.Set(10f, 0f);
            Append(ItemSlot);

            // ✅ 添加仆从限制逻辑
            ItemSlot.CanAcceptItem = item =>
            {
                if (item == null || item.IsAir || item.shoot <= ProjectileID.None)
                    return false;

                Projectile p = new Projectile();
                p.SetDefaults(item.shoot);

                return p.minion && !p.sentry;
            };

            labelCost = new UIText("", 0.85f);
            labelCost.Top.Set(5f, 0f);
            Append(labelCost);

            btnCostSub = new MiniButton("-");
            btnCostSub.Top.Set(2f, 0f);
            btnCostSub.OnClick += () => { slotCost = Math.Max(0, slotCost - 1); Refresh(); };
            Append(btnCostSub);

            btnCostAdd = new MiniButton("+");
            btnCostAdd.Top.Set(2f, 0f);
            btnCostAdd.OnClick += () => { slotCost++; Refresh(); };
            Append(btnCostAdd);

            labelCount = new UIText("", 0.85f);
            labelCount.Top.Set(30f, 0f);
            Append(labelCount);

            btnCountSub = new MiniButton("-");
            btnCountSub.Top.Set(27f, 0f);
            btnCountSub.OnClick += () => { usageCount = Math.Max(0, usageCount - 1); Refresh(); };
            Append(btnCountSub);

            btnCountAdd = new MiniButton("+");
            btnCountAdd.Top.Set(27f, 0f);
            btnCountAdd.OnClick += () => { usageCount++; Refresh(); };
            Append(btnCountAdd);

            btnFill = new MiniButton(Language.GetTextValue("Mods.AutoSummonEX.UI.Button.Fill"));
            btnFill.Top.Set(27f, 0f);
            btnFill.OnClick += () =>
            {
                float max = GetPlayerMaxMinionSlots?.Invoke() ?? 4f;
                usageCount = (int)Math.Floor(max / Math.Max(1f, slotCost));
                Refresh();
            };
            Append(btnFill);

            labelTotal = new UIText("", 0.85f);
            labelTotal.Top.Set(55f, 0f);
            Append(labelTotal);

            Refresh();
        }

        private void Refresh()
        {
            RefreshTextOnly();
            ScheduleLayout();
        }

        private void RefreshTextOnly()
        {
            labelCost.SetText(GetCostLabelText());
            labelCount.SetText(GetCountLabelText());

            float max = GetPlayerMaxMinionSlots?.Invoke() ?? 4f;
            float used = slotCost * usageCount;
            labelTotal.SetText(Language.GetTextValue("Mods.AutoSummonEX.UI.MinionSlotPanelInfo") + used.ToString("0.0") + " / " + max);
        }

        private void ScheduleLayout()
        {
            delayedFrames = 0;
            OnUpdate -= WaitForTextAndLayout;
            OnUpdate += WaitForTextAndLayout;
        }

        private void WaitForTextAndLayout(UIElement _)
        {
            delayedFrames++;
            if (delayedFrames < 2) return;

            LayoutUI();
            OnUpdate -= WaitForTextAndLayout;
        }

        private void LayoutUI()
        {
            float baseLeft = 80f;
            float costLabelWidth = labelCost.GetDimensions().Width;
            float countLabelWidth = labelCount.GetDimensions().Width;
            float totalLabelWidth = labelTotal.GetDimensions().Width;

            float maxLabelWidth = Math.Max(Math.Max(costLabelWidth, countLabelWidth), totalLabelWidth);
            float labelRightX = baseLeft + maxLabelWidth + 10f;

            labelCost.Left.Set(baseLeft, 0f);
            labelCount.Left.Set(baseLeft, 0f);
            labelTotal.Left.Set(baseLeft, 0f);

            btnCostSub.Left.Set(labelRightX, 0f);
            btnCostAdd.Left.Set(labelRightX + 42f, 0f);

            btnCountSub.Left.Set(labelRightX, 0f);
            btnCountAdd.Left.Set(labelRightX + 42f, 0f);
            btnFill.Left.Set(labelRightX + 84f, 0f);

            btnFill.Recalculate();
            float fillLeft = btnFill.Left.Pixels;
            float fillWidth = btnFill.GetDimensions().Width;

            float panelMinWidth = fillLeft + fillWidth + 20f; // 用 Fill 按钮位置和宽度 + padding 计算
            this.Width.Set(Math.Max(360f, panelMinWidth), 0f);
            Recalculate();
        }

        private string GetCostLabelText() => Language.GetTextValue("Mods.AutoSummonEX.UI.Label.Cost") + slotCost;
        private string GetCountLabelText() => Language.GetTextValue("Mods.AutoSummonEX.UI.Label.Count") + usageCount;

        public bool ShouldBeVisible()
        {
            return !ItemSlot.Item.IsAir;
        }

        public float GetPanelFullWidth()
        {
            float fillLeft = btnFill.Left.Pixels;
            float fillWidth = btnFill.GetDimensions().Width;
            return Math.Max(360f, fillLeft + fillWidth + 20f);
        }
    }
}
