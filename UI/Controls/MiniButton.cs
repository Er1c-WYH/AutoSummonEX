using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.Audio;
using Terraria.ID;
using System;

namespace AutoSummonEX.UI.Controls
{
    public class MiniButton : UIPanel
    {
        public delegate void ClickAction();
        public event ClickAction OnClick;

        private readonly UIText label;

        private Color baseColor = new Color(63, 82, 151) * 0.7f;
        private Color hoverColor = new Color(73, 94, 171) * 0.85f;

        public MiniButton(string text, float height = 22f)
        {
            Height.Set(height, 0f);
            BackgroundColor = baseColor;
            BorderColor = Color.Black;

            label = new UIText(text, 0.8f);
            label.HAlign = 0.5f;
            label.VAlign = 0.5f;
            Append(label);

            SetText(text); // 初始化时自适应
        }

        public void SetText(string newText)
        {
            label.SetText(newText);
            label.Recalculate();
            Recalculate();

            Width.Pixels = 0f;
            RecalculateLater();
        }

        private void RecalculateLater()
        {
            OnUpdate -= DelayedRecalculate;
            OnUpdate += DelayedRecalculate;
        }

        private void DelayedRecalculate(UIElement element)
        {
            float padding = 12f;
            float textWidth = label.GetDimensions().Width;
            float minWidth = 30f; // ✅ 更低的最小宽度

            Width.Set(Math.Max(minWidth, textWidth + padding), 0f);
            Recalculate();

            OnUpdate -= DelayedRecalculate;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            BackgroundColor = IsMouseHovering ? hoverColor : baseColor;
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
            OnClick?.Invoke();
        }
    }
}
