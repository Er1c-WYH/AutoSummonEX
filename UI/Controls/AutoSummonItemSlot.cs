using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace AutoSummonEX.UI.Controls
{
    public class AutoSummonItemSlot : UIElement
    {
        private readonly Texture2D _backgroundTexture;
        private readonly float _scale;
        private Item _item;

        // ✅ 可配置：限制可以放入的物品类型
        public Func<Item, bool> CanAcceptItem = _ => true;

        public AutoSummonItemSlot(float scale = 1f)
        {
            _scale = scale;
            _backgroundTexture = TextureAssets.InventoryBack4.Value;
            _item = new Item();
            _item.TurnToAir();

            Width.Set(_backgroundTexture.Width * _scale, 0f);
            Height.Set(_backgroundTexture.Height * _scale, 0f);
        }

        public Item Item => _item;

        public void SetItem(Item newItem)
        {
            _item = newItem?.Clone() ?? new Item();
            if (_item.IsAir)
                _item.TurnToAir();
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);

            Player player = Main.LocalPlayer;

            // ✅ 如果玩家正在攻击/挥舞武器 → 禁止交互
            if (player.itemAnimation > 0 || player.itemTime > 0)
                return;

            // ✅ 告知游戏：我们在处理 UI，阻止攻击逻辑干扰
            Main.LocalPlayer.mouseInterface = true;
            Main.blockMouse = true;

            Item mouse = Main.mouseItem;

            if (_item.IsAir && !mouse.IsAir)
            {
                if (CanAcceptItem != null && !CanAcceptItem(mouse))
                {
                    Main.NewText("该物品不能放入这个栏位", Color.Red);
                    return;
                }

                _item = mouse.Clone();
                Main.mouseItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (!_item.IsAir)
            {
                if (mouse.IsAir)
                {
                    Main.mouseItem = _item.Clone();
                    _item.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else if (mouse.type == _item.type && _item.stack < _item.maxStack)
                {
                    int transfer = Math.Min(mouse.stack, _item.maxStack - _item.stack);
                    _item.stack += transfer;
                    mouse.stack -= transfer;

                    if (mouse.stack <= 0)
                        Main.mouseItem.TurnToAir();

                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else
                {
                    if (CanAcceptItem != null && !CanAcceptItem(mouse))
                    {
                        Main.NewText("该物品不能放入这个栏位", Color.Red);
                        return;
                    }

                    Item temp = _item.Clone();
                    _item = mouse.Clone();
                    Main.mouseItem = temp;
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }

            CleanItem(ref _item);
            CleanItem(ref Main.mouseItem);

            Recipe.FindRecipes();
        }

        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);
            if (!_item.IsAir)
            {
                Main.LocalPlayer.QuickSpawnItem(_item.GetSource_DropAsItem(), _item, _item.stack);
                _item.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }

        private void CleanItem(ref Item item)
        {
            if (item == null || item.stack <= 0 || item.type <= ItemID.None)
                item.TurnToAir();
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            Vector2 pos = GetInnerDimensions().Position();
            spriteBatch.Draw(_backgroundTexture, pos, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);

            if (!_item.IsAir)
            {
                Texture2D tex = TextureAssets.Item[_item.type].Value;
                Rectangle frame = Main.itemAnimations[_item.type]?.GetFrame(tex) ?? tex.Bounds;

                float drawScale = 1f;
                float maxSize = 32f;

                if (frame.Width > maxSize || frame.Height > maxSize)
                    drawScale = maxSize / Math.Max(frame.Width, frame.Height);

                drawScale *= _scale;
                Vector2 drawPos = pos + _backgroundTexture.Size() * _scale / 2f - frame.Size() * drawScale / 2f;

                spriteBatch.Draw(tex, drawPos, frame, Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);

                if (_item.stack > 1)
                {
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch,
                        FontAssets.ItemStack.Value, _item.stack.ToString(),
                        pos + new Vector2(10f, 26f) * _scale,
                        Color.White, 0f, Vector2.Zero, new Vector2(_scale));
                }
            }

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.blockMouse = true;

                Main.HoverItem = _item.Clone();
                Main.hoverItemName = _item.Name;
            }
        }
    }
}
