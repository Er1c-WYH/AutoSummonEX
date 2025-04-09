using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;

namespace AutoSummonEX.UI.Controls
{
    public class UISectionPanel : UIPanel
    {
        private UIText titleText;

        public UISectionPanel(string title, float width = 0f, float height = 200f)
        {
            this.SetPadding(10);
            this.Width.Set(width, width == 0f ? 1f : 0f); // 默认撑满父面板
            this.Height.Set(height, 0f);
            this.BackgroundColor = new Color(45, 50, 70) * 0.85f;
            this.BorderColor = new Color(20, 20, 30);

            titleText = new UIText(title, 0.9f, true); // true = 粗体
            titleText.Top.Set(0f, 0f);
            titleText.HAlign = 0.5f;
            Append(titleText);
        }

        public void SetTitle(string newTitle)
        {
            titleText?.SetText(newTitle);
        }

        public void SetPanelHeight(float newHeight)
        {
            Height.Set(newHeight, 0f);
            Recalculate();
        }

        public float GetContentStartY()
        {
            return titleText.GetOuterDimensions().Height + 10f;
        }
    }
}
