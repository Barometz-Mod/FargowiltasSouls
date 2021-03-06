using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using FargowiltasSouls.Projectiles.Minions;
using FargowiltasSouls.Buffs.Minions;
using Microsoft.Xna.Framework;

namespace FargowiltasSouls.Items.Weapons.BossDrops
{
    public class BrainStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mind Break");
            Tooltip.SetDefault("'An old foe beaten into submission..'");
            DisplayName.AddTranslation(GameCulture.Chinese, "精神崩坏");
            Tooltip.AddTranslation(GameCulture.Chinese, "'一个被迫屈服的老对手..'");
        }

        public override void SetDefaults()
        {
            item.damage = 14;
            item.summon = true;
            item.mana = 10;
            item.width = 26;
            item.height = 28;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = 1;
            item.noMelee = true;
            item.knockBack = 3;
            item.rare = 2;
            item.UseSound = SoundID.Item44;
            item.shoot = ModContent.ProjectileType<BrainProj>();
            item.shootSpeed = 10f;
            item.buffType = ModContent.BuffType<BrainMinion>();
            item.autoReuse = true;
            item.value = Item.sellPrice(0, 2);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            player.AddBuff(item.buffType, 2);
            Vector2 spawnPos = Main.MouseWorld;
            Projectile.NewProjectile(spawnPos, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, 1f);
            return false;
        }
    }
}
