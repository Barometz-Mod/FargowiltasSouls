using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Buffs.Masomode;

namespace FargowiltasSouls.Projectiles.Champions
{
    public class NatureCloudRaining : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_238";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Nature Cloud");
            Main.projFrames[projectile.type] = 6;
        }

        public override void SetDefaults()
        {
            projectile.width = 54;
            projectile.height = 28;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.timeLeft = 1200;
            projectile.tileCollide = false;
            
            cooldownSlot = 1;
        }

        public override void AI()
        {
            if (++projectile.ai[0] > 8)
            {
                projectile.ai[0] = 0;

                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(projectile.position.X + 14 + Main.rand.Next(projectile.width - 28),
                        projectile.position.Y + projectile.height + 4, 0f, 5f,
                        ModContent.ProjectileType<NatureRain>(), projectile.damage, 0f, Main.myPlayer);
                }
            }

            if (++projectile.ai[1] > 600)
            {
                projectile.alpha += 5;
                if (projectile.alpha > 255)
                {
                    projectile.alpha = 255;
                    projectile.Kill();
                }
            }

            if (++projectile.frameCounter > 8)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame > 5)
                    projectile.frame = 0;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(BuffID.Wet, 300);
            if (FargoSoulsWorld.MasochistMode)
                target.AddBuff(BuffID.Frostburn, 300);
        }
    }
}