using Terraria;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.ID;

namespace AutoSummonEX
{
    public class AutoSummonPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (AutoSummonEX.ToggleAutoSummonUIKeybind?.JustPressed ?? false)
            {
                bool willOpen = !AutoSummonUISystem.Visible;

                // 如果是要打开 UI 且当前背包是关的 → 强制打开背包
                if (willOpen && !Main.playerInventory)
                {
                    Main.playerInventory = true;
                }

                // 切换 UI 状态（必须调用 ShowUI/HideUI）
                if (willOpen)
                {
                    AutoSummonUISystem.ShowUI();
                }
                else
                {
                    AutoSummonUISystem.HideUI();
                }

                Main.NewText($"UI {(willOpen ? "打开" : "关闭")}");
            }
        }

        public override void OnEnterWorld()
        {
            AutoSummonUISystem.HideUI();
        }

        public override void PostUpdate()
        {
            if (Main.dedServ) return;

            // 玩家关闭背包 → 强制关闭 UI
            if (!Main.playerInventory && AutoSummonUISystem.Visible)
            {
                AutoSummonUISystem.HideUI();
                return;
            }

            // 自动打开：在背包中手持召唤武器时
            if (Main.playerInventory &&
                Main.mouseItem != null &&
                Main.mouseItem.type != ItemID.None &&
                SummonUIHelper.IsValidSummonWeapon(Main.mouseItem) &&
                !AutoSummonUISystem.Visible)
            {
                AutoSummonUISystem.ShowUI();
            }
        }
    }
}
