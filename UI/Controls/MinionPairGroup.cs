using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace AutoSummonEX.UI
{
    public class MinionPairGroup : UIElement
    {
        private readonly List<MinionSlotPair> slotPairs = new();
        private const float PairSpacing = 6f;

        public MinionPairGroup()
        {
            this.SetPadding(0);
            EnsureHasEmptyPair();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            EnsureHasEmptyPair();
            RemoveExtraEmptyPairs();
            UpdateLayout();
        }

        private void EnsureHasEmptyPair()
        {
            if (!slotPairs.Exists(p => p.GetItem().IsAir))
            {
                var newPair = new MinionSlotPair();
                slotPairs.Add(newPair);
                Append(newPair);
            }
        }

        private void RemoveExtraEmptyPairs()
        {
            int emptyCount = 0;
            for (int i = slotPairs.Count - 1; i >= 0; i--)
            {
                var pair = slotPairs[i];
                if (pair.GetItem().IsAir)
                {
                    emptyCount++;
                    if (emptyCount > 1)
                    {
                        slotPairs.RemoveAt(i);
                        RemoveChild(pair);
                    }
                }
            }
        }

        private void UpdateLayout()
        {
            float top = 0f;
            float maxWidth = 0f;
            foreach (var pair in slotPairs)
            {
                pair.Top.Set(top, 0f);
                pair.Left.Set(0f, 0f);
                pair.Recalculate();
                top += pair.Height.Pixels + PairSpacing;

                float w = pair.GetPanelFullWidth();
                if (w > maxWidth)
                    maxWidth = w;
            }

            Width.Set(maxWidth, 0f);
            Height.Set(top - PairSpacing, 0f); // 最后一个不加 spacing
            Recalculate();
        }

        public float GetGroupFullWidth()
        {
            float max = 0f;
            foreach (var pair in slotPairs)
            {
                float w = pair.GetPanelFullWidth();
                if (w > max) max = w;
            }
            return max;
        }

        public IEnumerable<MinionSlotPair> GetAllPairs() => slotPairs;
    }
}
