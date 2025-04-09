using AutoSummonEX.Config;
using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace AutoSummonEX
{
    public static class SummonUIHelper
    {
        public static void SaveConfig(ModConfig config)
        {
            MethodInfo saveMethod = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
            saveMethod?.Invoke(null, new object[] { config });
        }

        public static void SavePanelPosition(float x, float y)
        {
            // ✅ 新增：告诉系统“本次修改是拖动引起的，不要触发自动关闭”
            AutoSummonUISystem.SuppressNextClose();

            var config = ModContent.GetInstance<AutoSummonConfig>();
            config.PanelPosition = new Vector2(x, y);
            SaveConfig(config);
        }

        public static bool IsValidSummonWeapon(Item item)
        {
            if (item == null || item.IsAir || item.type <= ItemID.None)
                return false;

            if (item.shoot > ProjectileID.None)
            {
                Projectile p = new Projectile();
                p.SetDefaults(item.shoot);
                if (p.minion && !p.sentry)
                    return true;
            }

            if (item.CountsAsClass(DamageClass.Summon) && item.sentry)
                return true;

            return false;
        }
    }
}
