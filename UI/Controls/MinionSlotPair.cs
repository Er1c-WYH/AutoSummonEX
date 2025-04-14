using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using AutoSummonEX.UI.Controls;
using AutoSummonEX.UI.Panels;
using Terraria.GameContent.UI.Elements;
using System;

namespace AutoSummonEX.UI
{
    public class MinionSlotPair : UIPanel
    {
        public AutoSummonItemSlot ItemSlot { get; private set; }
        public MinionSlotSubPanel SubPanel { get; private set; }

        private const float SlotScale = 0.85f;
        private const float SlotTop = 0f;
        private const float PanelSpacing = 5f;
        private const float SubPanelEstimatedWidth = 300f; // ⛑ 缓冲用：SubPanel 未生成时的预估宽度

        public MinionSlotPair()
        {
            BackgroundColor = new Color(255, 0, 0, 60); // 可去除
            BorderColor = new Color(255, 255, 255, 180);
            Height.Set(105f, 0f);
            this.SetPadding(0);

            ItemSlot = new AutoSummonItemSlot(SlotScale);
            ItemSlot.Top.Set(SlotTop, 0f);
            ItemSlot.Left.Set(2f, 0f);
            ItemSlot.CanAcceptItem = item =>
            {
                if (item == null || item.IsAir || item.shoot <= ProjectileID.None)
                    return false;
                Projectile p = new Projectile();
                p.SetDefaults(item.shoot);
                return p.minion && !p.sentry;
            };
            Append(ItemSlot);

            SubPanel = new MinionSlotSubPanel();
            SubPanel.Top.Set(SlotTop, 0f);
            SubPanel.Left.Set(ItemSlot.Left.Pixels + ItemSlot.Width.Pixels + PanelSpacing, 0f);
            SubPanel.GetPlayerMaxMinionSlots = () => Main.LocalPlayer.maxMinions;
            SubPanel.SetItemSlot(ItemSlot);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ItemSlot.Item.IsAir)
            {
                if (HasChild(SubPanel))
                    RemoveChild(SubPanel);
            }
            else
            {
                if (!HasChild(SubPanel))
                {
                    Append(SubPanel);
                }

                SubPanel.Left.Set(ItemSlot.Left.Pixels + ItemSlot.Width.Pixels + PanelSpacing, 0f);
                SubPanel.Recalculate();
            }

            float width = GetPanelFullWidth();
            Width.Set(Math.Max(width, 240f), 0f); // ✅ 再兜底一次宽度
        }

        public float GetPanelFullWidth()
        {
            if (!HasChild(SubPanel))
            {
                float estimate = ItemSlot.Width.Pixels + PanelSpacing + SubPanelEstimatedWidth;
                return estimate;
            }

            SubPanel.Recalculate();
            float actual = SubPanel.Left.Pixels + SubPanel.GetPanelFullWidth() + 20f;
            Main.NewText($"pair宽度{SubPanel.Left.Pixels} + {SubPanel.GetPanelFullWidth()} = {actual}");
            Main.NewText($"subpanel实际左侧距离{ItemSlot.Left.Pixels} + {ItemSlot.Width.Pixels} + {PanelSpacing} = {ItemSlot.Left.Pixels + ItemSlot.Width.Pixels + PanelSpacing}");
            return actual;
        }

        public Item GetItem() => ItemSlot.Item;

        public float GetUsedSlotCount() => SubPanel.GetUsageSlotCost();
    }
}
