using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;

namespace AutoSummonEX.UI.Controls
{
    public class DragableUIPanel : UIPanel
    {
        private bool dragging = false;
        private Vector2 offset;
        private bool allowDragging = true;
        private int visibleDelay = 0;

        public void SetDraggable(bool allow)
        {
            allowDragging = allow;
            visibleDelay = 0; // 防止刚打开时误触拖动
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);

            if (!allowDragging) return;

            // 只在点击到面板本体时触发拖动
            if (evt.Target == this)
            {
                dragging = true;
                offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);
            }
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            dragging = false;

            if (!allowDragging) return;

            if (evt.Target == this)
            {
                SummonUIHelper.SavePanelPosition(Left.Pixels, Top.Pixels);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // 防止面板刚显示出来立即触发拖动
            if (visibleDelay < 2)
            {
                visibleDelay++;
                return;
            }

            if (dragging && allowDragging)
            {
                Left.Set(Main.mouseX - offset.X, 0f);
                Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }
        }
    }
}
