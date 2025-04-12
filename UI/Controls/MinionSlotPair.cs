using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.ID;
using AutoSummonEX.UI.Controls;
using AutoSummonEX.UI.Panels;

namespace AutoSummonEX.UI
{
    public class MinionSlotPair : UIElement
    {
        public AutoSummonItemSlot ItemSlot { get; private set; }
        public MinionSlotSubPanel SubPanel { get; private set; }

        private const float SlotScale = 0.85f;
        private const float SlotTop = 0f;
        private const float PanelSpacing = 5f;

        public MinionSlotPair()
        {
            Height.Set(105f, 0f);
            this.SetPadding(0);

            ItemSlot = new AutoSummonItemSlot(SlotScale);
            ItemSlot.Top.Set(SlotTop, 0f);
            ItemSlot.Left.Set(0f, 0f);
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
            Append(SubPanel);
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
                    Append(SubPanel);

                SubPanel.Left.Set(ItemSlot.Left.Pixels + ItemSlot.Width.Pixels + PanelSpacing, 0f);
                SubPanel.Recalculate();
            }

            // ✅ 每帧设置自身宽度 = 仆从槽 + 间距 + 子面板宽度
            Width.Set(GetPanelFullWidth(), 0f);
        }

        public float GetPanelFullWidth()
        {
            float slotWidth = ItemSlot.Width.Pixels;
            float subPanelWidth = SubPanel?.GetPanelFullWidth() ?? 0f;
            float spacing = 10f;
            return spacing + slotWidth + spacing + subPanelWidth;
        }

        public Item GetItem() => ItemSlot.Item;

        public float GetUsedSlotCount() => SubPanel.GetUsageSlotCost();
    }
}
