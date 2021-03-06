﻿using FargowiltasSouls.Items.Summons;
using FargowiltasSouls.Projectiles;
using FargowiltasSouls.Projectiles.Deathrays;
using FargowiltasSouls.Projectiles.Masomode;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using FargowiltasSouls.Buffs.Masomode;
using FargowiltasSouls.Projectiles.MutantBoss;

namespace FargowiltasSouls.NPCs
{
    public partial class EModeGlobalNPC
    {
        public void KingSlimeAI(NPC npc)
        {
            slimeBoss = npc.whoAmI;
            npc.color = Main.DiscoColor * 0.2f;
            if (masoBool[1])
            {
                if (npc.velocity.Y == 0f) //start attack
                {
                    masoBool[1] = false;
                    if (Main.netMode != 1)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            Projectile.NewProjectile(new Vector2(npc.Center.X + Main.rand.Next(-5, 5), npc.Center.Y - 15),
                                new Vector2(Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-8, -5)),
                                ProjectileID.SpikedSlimeSpike, npc.damage / 5, 0f, Main.myPlayer);
                        }
                    }
                }
            }
            else if (npc.velocity.Y > 0)
            {
                masoBool[1] = true;
            }

            if ((masoBool[0] || npc.life < npc.lifeMax * .5f) && npc.HasPlayerTarget)
            {
                Player p = Main.player[npc.target];

                Counter++;
                if (Counter >= 90) //slime rain
                {
                    Counter = 0;
                    Main.PlaySound(SoundID.Item21, p.Center);
                    if (Main.netMode != 1)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 spawn = p.Center;
                            spawn.X += Main.rand.Next(-200, 201);
                            spawn.Y -= Main.rand.Next(600, 901);
                            Vector2 speed = p.Center - spawn;
                            speed.Normalize();
                            speed *= masoBool[0] ? 10f : 5f;
                            speed = speed.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-5, 5)));
                            Projectile.NewProjectile(spawn, speed, ModContent.ProjectileType<SlimeBallHostile>(), npc.damage / 5, 0f, Main.myPlayer);
                        }
                    }
                }

                if (++Timer > 300)
                {
                    Timer = 0;
                    const float gravity = 0.15f;
                    float time = masoBool[0] ? 60f : 120f;
                    Vector2 distance = Main.player[npc.target].Center - npc.Center + Main.player[npc.target].velocity * 30f;
                    distance.X = distance.X / time;
                    distance.Y = distance.Y / time - 0.5f * gravity * time;
                    for (int i = 0; i < 10; i++)
                    {
                        Projectile.NewProjectile(npc.Center, distance + Main.rand.NextVector2Square(-0.5f, 0.5f) * (masoBool[0] ? 3 : 1),
                            ModContent.ProjectileType<SlimeSpike>(), npc.damage / 5, 0f, Main.myPlayer);
                    }
                }
            }

            if (!masoBool[0]) //is not berserk
            {
                SharkCount = 0;

                if (npc.HasPlayerTarget)
                {
                    Player player = Main.player[npc.target];
                    if (player.active && !player.dead && player.Center.Y < npc.position.Y && npc.Distance(player.Center) < 1000f)
                    {
                        Counter2++; //timer runs if player is above me and nearby
                        if (Counter2 >= 600 && Main.netMode != 1) //go berserk
                        {
                            masoBool[0] = true;
                            npc.netUpdate = true;
                            NetUpdateMaso(npc.whoAmI);
                            if (Main.netMode == 2)
                                NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("King Slime has enraged!"), new Color(175, 75, 255));
                            else
                                Main.NewText("King Slime has enraged!", 175, 75, 255);
                        }
                    }
                    else
                    {
                        Counter2 = 0;
                    }
                }
            }
            else //is berserk
            {
                SharkCount = 1;

                if (!masoBool[2])
                {
                    masoBool[2] = true;
                    Main.PlaySound(15, npc.Center, 0);
                }

                if (Counter < 60) //slime rain much faster
                    Counter = 60;

                if (Timer < 270) //aimed spikes much faster
                    Timer = 270;

                if (npc.HasValidTarget && Main.player[npc.target].position.Y > npc.position.Y) //player went back down
                {
                    masoBool[0] = false;
                    masoBool[2] = false;
                    NetUpdateMaso(npc.whoAmI);
                }

                /*if (npc.HasPlayerTarget)
                {
                    Player p = Main.player[npc.target];

                    Counter2++;
                    if (Counter2 >= 4) //spray random slime spikes
                    {
                        Counter2 = 0;
                        if (Main.netMode != 1)
                        {
                            Vector2 speed = p.Center - npc.Center;
                            speed.Normalize();
                            speed *= 16f + Main.rand.Next(-50, 51) * 0.04f;
                            if (speed.X < 0)
                                speed = speed.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-15, 31)));
                            else
                                speed = speed.RotatedBy(MathHelper.ToRadians(Main.rand.Next(-30, 16)));
                            Projectile.NewProjectile(npc.position.X + Main.rand.Next(npc.width), npc.position.Y + Main.rand.Next(npc.height), speed.X, speed.Y, ProjectileID.SpikedSlimeSpike, npc.damage / 4, 0f, Main.myPlayer);
                        }
                    }

                    if (p.active && !p.dead)
                    {
                        npc.noTileCollide = false;
                        p.pulley = false;
                        p.controlHook = false;
                        if (p.mount.Active)
                            p.mount.Dismount(p);

                        p.AddBuff(BuffID.Slimed, 2);
                        p.AddBuff(ModContent.BuffType<Crippled>(), 2);
                        p.AddBuff(ModContent.BuffType<ClippedWings>(), 2);
                    }
                    else
                    {
                        npc.noTileCollide = true;
                    }
                }*/
            }
        }

        public void EyeOfCthulhuAI(NPC npc)
        {
            eyeBoss = npc.whoAmI;

            Counter++;
            if (Counter >= 600)
            {
                Counter = 0;
                if (npc.life <= npc.lifeMax * 0.65 && NPC.CountNPCS(NPCID.ServantofCthulhu) < 12 && Main.netMode != 1)
                {
                    Vector2 vel = new Vector2(2, 2);
                    for (int i = 0; i < 4; i++)
                    {
                        int n = NPC.NewNPC((int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height), NPCID.ServantofCthulhu);
                        if (n != 200)
                        {
                            Main.npc[n].velocity = vel.RotatedBy(Math.PI / 2 * i);
                            if (Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                    }
                }
            }

            if (npc.life < npc.lifeMax / 2)
            {
                if (npc.ai[0] == 3 && (npc.ai[1] == 0 || npc.ai[1] == 5))
                {
                    if (npc.ai[2] < 2)
                    {
                        npc.ai[2]--;
                        npc.alpha += 4;
                        for (int i = 0; i < 3; i++)
                        {
                            int d = Dust.NewDust(npc.position, npc.width, npc.height, 229, 0f, 0f, 0, default(Color), 1.5f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].noLight = true;
                            Main.dust[d].velocity *= 4f;
                        }
                        if (npc.alpha > 255)
                        {
                            npc.alpha = 255;
                            if (Main.netMode != 1 && npc.HasPlayerTarget)
                            {
                                Vector2 distance = npc.Center - Main.player[npc.target].Center;
                                npc.Center = Main.player[npc.target].Center;
                                distance.X *= 1.5f;
                                if (distance.X > 1200)
                                    distance.X = 1200;
                                else if (distance.X < -1200)
                                    distance.X = -1200;
                                if (distance.Y > 0)
                                    distance.Y *= -1;
                                npc.position.X -= distance.X;
                                npc.position.Y += distance.Y;
                                npc.netUpdate = true;
                                npc.ai[2] = 60;
                                npc.ai[1] = 5f;//
                            }
                        }
                    }
                    else
                    {
                        npc.alpha -= 4;
                        if (npc.alpha < 0)
                        {
                            npc.alpha = 0;
                        }
                        else
                        {
                            npc.ai[2]--;
                            npc.position -= npc.velocity / 2;
                            for (int i = 0; i < 3; i++)
                            {
                                int d = Dust.NewDust(npc.position, npc.width, npc.height, 229, 0f, 0f, 0, default(Color), 1.5f);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].noLight = true;
                                Main.dust[d].velocity *= 4f;
                            }
                        }
                    }
                }

                npc.dontTakeDamage = npc.alpha > 50;

                if (Counter2 > 0)
                {
                    if (Counter2 % 6 == 0 && Main.netMode != 1)
                        Projectile.NewProjectile(new Vector2(npc.Center.X + Main.rand.Next(-15, 15), npc.Center.Y), npc.velocity / 10, ModContent.ProjectileType<BloodScythe>(), npc.damage / 4, 1f, Main.myPlayer);
                    Counter2--;

                }
                if (npc.ai[1] == 3f) //during dashes in phase 2
                {
                    Counter2 = 30;
                    masoBool[0] = false;
                    if (Main.netMode != 1)
                        FargoGlobalProjectile.XWay(8, npc.Center, ModContent.ProjectileType<BloodScythe>(), 1.5f, npc.damage / 4, 0);
                }
                /*if (++Timer > 600)
                {
                    Timer = 0;
                    if (npc.HasValidTarget)
                    {
                        Player player = Main.player[npc.target];
                        Main.PlaySound(29, (int)player.position.X, (int)player.position.Y, 104, 1f, 0f);
                        if (Main.netMode != 1)
                        {
                            Vector2 spawnPos = player.Center;
                            int direction;
                            if (player.velocity.X == 0f)
                                direction = player.direction;
                            else
                                direction = Math.Sign(player.velocity.X);
                            spawnPos.X += 600 * direction;
                            spawnPos.Y -= 600;
                            Vector2 speed = Vector2.UnitY;
                            for (int i = 0; i < 30; i++)
                            {
                                Projectile.NewProjectile(spawnPos, speed, ModContent.ProjectileType<BloodScythe>(), npc.damage / 4, 1f, Main.myPlayer);
                                spawnPos.X += 72 * direction;
                                speed.Y += 0.15f;
                            }
                        }
                    }
                }*/
            }
            else
            {
                npc.alpha = 0;
                npc.dontTakeDamage = false;
            }
        }

        public void EaterOfWorldsAI(NPC npc)
        {
            eaterBoss = npc.whoAmI;
            boss = npc.whoAmI;
            Counter++;
            if (Counter >= 6) //cursed flamethrower, roughly same direction as head
            {
                Counter = 0;
                if (Main.netMode != 1)
                {
                    Vector2 velocity = new Vector2(5f, 0f).RotatedBy(npc.rotation - Math.PI / 2.0 + MathHelper.ToRadians(Main.rand.Next(-15, 16)));
                    Projectile.NewProjectile(npc.Center, velocity, ProjectileID.EyeFire, npc.damage / 6, 0f, Main.myPlayer);
                }
            }
        }

        public void BrainOfCthulhuAI(NPC npc)
        {
            brainBoss = npc.whoAmI;
            if (npc.alpha == 0)
            {
                npc.damage = npc.defDamage;
            }
            else
            {
                npc.damage = 0;
                if (npc.ai[0] != -2 && npc.HasPlayerTarget && npc.Distance(Main.player[npc.target].Center) < 300) //stay at a minimum distance
                {
                    npc.Center = Main.player[npc.target].Center + Main.player[npc.target].DirectionTo(npc.Center) * 300;
                }
            }

            if (!npc.dontTakeDamage) //vulnerable
            {
                if (npc.buffType[0] != 0) //constant debuff cleanse
                {
                    npc.buffImmune[npc.buffType[0]] = true;
                    npc.DelBuff(0);
                }
                if (!masoBool[0]) //spawn illusions
                {
                    masoBool[0] = true;
                    if (Main.netMode != 1)
                    {
                        bool recolor = SoulConfig.Instance.BossRecolors && FargoSoulsWorld.MasochistMode;
                        int type = recolor ? ModContent.NPCType<BrainIllusion2>() : ModContent.NPCType<BrainIllusion>();
                        int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, type, npc.whoAmI, npc.whoAmI, -1, 1);
                        if (n != 200 && Main.netMode == 2)
                            NetMessage.SendData(23, -1, -1, null, n);
                        n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, type, npc.whoAmI, npc.whoAmI, 1, -1);
                        if (n != 200 && Main.netMode == 2)
                            NetMessage.SendData(23, -1, -1, null, n);
                        n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, type, npc.whoAmI, npc.whoAmI, 1, 1);
                        if (n != 200 && Main.netMode == 2)
                            NetMessage.SendData(23, -1, -1, null, n);
                        n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<BrainClone>(), npc.whoAmI);
                        if (n != 200 && Main.netMode == 2)
                            NetMessage.SendData(23, -1, -1, null, n);
                    }
                }

                void MakeDust(Vector2 spawn)
                {
                    for (int i = 0; i < 24; i++) //dust ring
                    {
                        Vector2 vector6 = Vector2.UnitY * 12f;
                        vector6 = vector6.RotatedBy((i - (24 / 2 - 1)) * 6.28318548f / 24) + spawn;
                        Vector2 vector7 = vector6 - spawn;
                        int d = Dust.NewDust(vector6 + vector7, 0, 0, 90, 0f, 0f, 0, default(Color), 3f);
                        Main.dust[d].scale = 3f;
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity = vector7;
                    }
                };

                if (--Counter < 0) //confusion timer
                {
                    Counter = 600;
                    Main.PlaySound(15, (int)npc.Center.X, (int)npc.Center.Y, 0);

                    Vector2 offset = npc.Center - Main.player[npc.target].Center;

                    Vector2 spawnPos = Main.player[npc.target].Center;
                    spawnPos.X += offset.X;
                    spawnPos.Y += offset.Y;
                    MakeDust(spawnPos);

                    spawnPos = Main.player[npc.target].Center;
                    spawnPos.X += offset.X;
                    spawnPos.Y -= offset.Y;
                    MakeDust(spawnPos);

                    spawnPos = Main.player[npc.target].Center;
                    spawnPos.X -= offset.X;
                    spawnPos.Y += offset.Y;
                    MakeDust(spawnPos);

                    spawnPos = Main.player[npc.target].Center;
                    spawnPos.X -= offset.X;
                    spawnPos.Y -= offset.Y;
                    MakeDust(spawnPos);
                }
                else if (Counter == 540) //inflict confusion after telegraph
                {
                    if (npc.Distance(Main.player[Main.myPlayer].Center) < 3000)
                        Main.player[Main.myPlayer].AddBuff(BuffID.Confused, Main.expertMode && Main.expertDebuffTime > 1 ? 150 : 300);

                    if (npc.HasValidTarget && Main.netMode != 1) //laser spreads from each illusion
                    {
                        Vector2 offset = npc.Center - Main.player[npc.target].Center;

                        const int degree = 8;

                        Vector2 spawnPos = Main.player[npc.target].Center;
                        spawnPos.X += offset.X;
                        spawnPos.Y += offset.Y;
                        Projectile.NewProjectile(spawnPos, new Vector2(0, -4), ModContent.ProjectileType<BrainofConfusion>(), 0, 0, Main.myPlayer);
                        for (int i = -1; i <= 1; i++)
                            Projectile.NewProjectile(spawnPos, Main.player[npc.target].DirectionFrom(spawnPos).RotatedBy(MathHelper.ToRadians(degree) * i), ModContent.ProjectileType<DestroyerLaser>(), npc.damage / 4, 0f, Main.myPlayer);

                        spawnPos = Main.player[npc.target].Center;
                        spawnPos.X += offset.X;
                        spawnPos.Y -= offset.Y;
                        Projectile.NewProjectile(spawnPos, new Vector2(0, -4), ModContent.ProjectileType<BrainofConfusion>(), 0, 0, Main.myPlayer);
                        for (int i = -1; i <= 1; i++)
                            Projectile.NewProjectile(spawnPos, Main.player[npc.target].DirectionFrom(spawnPos).RotatedBy(MathHelper.ToRadians(degree) * i), ModContent.ProjectileType<DestroyerLaser>(), npc.damage / 4, 0f, Main.myPlayer);

                        spawnPos = Main.player[npc.target].Center;
                        spawnPos.X -= offset.X;
                        spawnPos.Y += offset.Y;
                        Projectile.NewProjectile(spawnPos, new Vector2(0, -4), ModContent.ProjectileType<BrainofConfusion>(), 0, 0, Main.myPlayer);
                        for (int i = -1; i <= 1; i++)
                            Projectile.NewProjectile(spawnPos, Main.player[npc.target].DirectionFrom(spawnPos).RotatedBy(MathHelper.ToRadians(degree) * i), ModContent.ProjectileType<DestroyerLaser>(), npc.damage / 4, 0f, Main.myPlayer);

                        spawnPos = Main.player[npc.target].Center;
                        spawnPos.X -= offset.X;
                        spawnPos.Y -= offset.Y;
                        Projectile.NewProjectile(spawnPos, new Vector2(0, -4), ModContent.ProjectileType<BrainofConfusion>(), 0, 0, Main.myPlayer);
                        for (int i = -1; i <= 1; i++)
                            Projectile.NewProjectile(spawnPos, Main.player[npc.target].DirectionFrom(spawnPos).RotatedBy(MathHelper.ToRadians(degree) * i), ModContent.ProjectileType<DestroyerLaser>(), npc.damage / 4, 0f, Main.myPlayer);
                    }
                }

                int b = Main.LocalPlayer.FindBuffIndex(BuffID.Confused);
                if (b != -1 && Main.LocalPlayer.buffTime[b] == 60)
                {
                    Main.PlaySound(36, (int)npc.Center.X, (int)npc.Center.Y, -1, 1f, 0f);
                    MakeDust(Main.LocalPlayer.Center);
                }
            }
        }

        public void CreeperAI(NPC npc)
        {
            if (++Timer >= 600)
            {
                int count = NPC.CountNPCS(NPCID.Creeper) - 1;
                Timer = (20 - count) * 29;
                if (Timer < 0)
                    Timer = 0;

                if (npc.HasPlayerTarget && Main.netMode != 1)
                {
                    Vector2 speed = Main.player[npc.target].Center - npc.Center;
                    speed.Y -= Math.Abs(speed.X) * 0.1f; //account for gravity
                    speed.X += Main.rand.Next(-10, 11);
                    speed.Y += Main.rand.Next(-30, 21);
                    speed.Normalize();
                    speed *= 10f;
                    Projectile.NewProjectile(npc.Center, speed, ProjectileID.GoldenShowerHostile, npc.damage / 4, 0f, Main.myPlayer);
                }

                npc.netUpdate = true;
            }
        }

        public bool QueenBeeAI(NPC npc)
        {
            beeBoss = npc.whoAmI;

            if (!masoBool[0] && npc.life < npc.lifeMax / 3 * 2 && npc.HasPlayerTarget)
            {
                masoBool[0] = true;

                Vector2 vector72 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(Main.rand.Next(20) * npc.direction), npc.position.Y + (float)npc.height * 0.8f);

                int num594 = NPC.NewNPC((int)vector72.X, (int)vector72.Y, ModContent.NPCType<RoyalSubject>(), 0, 0f, 0f, 0f, 0f, 255);
                Main.npc[num594].velocity.X = (float)Main.rand.Next(-200, 201) * 0.002f;
                Main.npc[num594].velocity.Y = (float)Main.rand.Next(-200, 201) * 0.002f;
                Main.npc[num594].localAI[0] = 60f;
                Main.npc[num594].netUpdate = true;

                if (Main.netMode == 0)
                    Main.NewText("Royal Subject has awoken!", 175, 75, 255);
                else if (Main.netMode == 2)
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Royal Subject has awoken!"), new Color(175, 75, 255));
            }

            if (!masoBool[1] && npc.life < npc.lifeMax / 3 && npc.HasPlayerTarget)
            {
                masoBool[1] = true;

                Vector2 vector72 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(Main.rand.Next(20) * npc.direction), npc.position.Y + (float)npc.height * 0.8f);

                int num594 = NPC.NewNPC((int)vector72.X, (int)vector72.Y, ModContent.NPCType<RoyalSubject>(), 0, 0f, 0f, 0f, 0f, 255);
                Main.npc[num594].velocity.X = (float)Main.rand.Next(-200, 201) * 0.1f;
                Main.npc[num594].velocity.Y = (float)Main.rand.Next(-200, 201) * 0.1f;
                Main.npc[num594].localAI[0] = 60f;
                Main.npc[num594].netUpdate = true;

                if (Main.netMode == 0)
                    Main.NewText("Royal Subject has awoken!", 175, 75, 255);
                else if (Main.netMode == 2)
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Royal Subject has awoken!"), new Color(175, 75, 255));

                NPC.SpawnOnPlayer(npc.target, ModContent.NPCType<RoyalSubject>()); //so that both dont stack for being spawned from qb
            }

            if (!masoBool[2] && npc.life < npc.lifeMax / 2) //enable new attack and roar below 50%
            {
                masoBool[2] = true;
                Main.PlaySound(15, npc.Center, 0);
            }

            if (NPC.AnyNPCs(ModContent.NPCType<RoyalSubject>()))
            {
                npc.ai[0] = 3; //always shoot stingers mode
                RegenTimer = 480;
            }

            //only while stationary mode
            if (npc.ai[0] == 3f || npc.ai[0] == 1f)
            {
                if (masoBool[2] && ++Timer > 600)
                {
                    if (Timer < 690) //slow down
                    {
                        if (!masoBool[3])
                        {
                            masoBool[3] = true;
                            npc.netUpdate = true;
                            for (int i = 0; i < 36; i++)
                            {
                                Vector2 vector6 = Vector2.UnitY * 9f;
                                vector6 = vector6.RotatedBy((i - (36 / 2 - 1)) * 6.28318548f / 36) + npc.Center;
                                Vector2 vector7 = vector6 - npc.Center;
                                int d = Dust.NewDust(vector6 + vector7, 0, 0, 87, 0f, 0f, 0, default(Color), 4f);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].velocity = vector7;
                            }
                            Main.PlaySound(15, npc.Center, 0);
                        }

                        if (Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                        {
                            npc.velocity *= 0.975f;
                        }
                        else
                        {
                            Timer--; //stall this section until has line of sight
                            return true;
                        }
                    }
                    else if (Timer < 840) //spray bees
                    {
                        if (masoBool[3])
                        {
                            masoBool[3] = false;
                            npc.netUpdate = true;
                        }
                        npc.velocity = Vector2.Zero;
                        if (++Counter > 2)
                        {
                            Counter = 0;
                            if (Main.netMode != 1)
                            {
                                Projectile.NewProjectile(npc.Center + Vector2.UnitY * 15, 12f * Vector2.UnitX.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-45, 45))), ModContent.ProjectileType<Bee>(), npc.damage / 5, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center + Vector2.UnitY * 15, -12f * Vector2.UnitX.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-45, 45))), ModContent.ProjectileType<Bee>(), npc.damage / 5, 0f, Main.myPlayer);
                            }
                        }
                    }
                    else if (Timer > 900) //wait for 1 second then return to normal AI
                    {
                        Timer = 0;
                        npc.netUpdate = true;
                    }

                    if (npc.netUpdate)
                    {
                        if (Main.netMode == 2)
                        {
                            NetMessage.SendData(27, -1, -1, null, npc.whoAmI);
                            NetUpdateMaso(npc.whoAmI);
                        }
                        npc.netUpdate = false;
                    }
                    return false;
                }

                Counter++;
                if (Counter >= 90)
                {
                    Counter = 0;
                    Counter2++;
                    if (Counter2 > 3)
                    {
                        if (Main.netMode != 1)
                            Projectiles.FargoGlobalProjectile.XWay(16, npc.Center, ProjectileID.Stinger, 6, 11, 1);
                        Counter2 = 0;
                    }
                    else
                    {
                        if (Main.netMode != 1)
                            Projectiles.FargoGlobalProjectile.XWay(8, npc.Center, ProjectileID.Stinger, 6, 11, 1);
                    }
                }
            }

            return true;
        }

        public void SkeletronAI(NPC npc)
        {
            skeleBoss = npc.whoAmI;
            if (!masoBool[0])
            {
                masoBool[0] = true;
                if (Main.netMode != 1 && !NPC.downedBoss3)
                    Item.NewItem(npc.Hitbox, ModContent.ItemType<BloodiedSkull>());
            }
            if (Counter != 0)
            {
                Timer++;

                if (Timer >= 3600)
                {
                    Timer = 0;

                    bool otherHandStillAlive = false;
                    for (int i = 0; i < 200; i++) //look for hand that belongs to me
                    {
                        if (Main.npc[i].active && Main.npc[i].type == NPCID.SkeletronHand && Main.npc[i].ai[1] == npc.whoAmI)
                        {
                            otherHandStillAlive = true;
                            break;
                        }
                    }

                    if (Main.netMode != 1)
                    {
                        int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.SkeletronHand, npc.whoAmI, 0f, 0f, 0f, 0f, npc.target);
                        if (n != 200)
                        {
                            Main.npc[n].ai[0] = (Counter == 1) ? 1f : -1f;
                            Main.npc[n].ai[1] = npc.whoAmI;
                            Main.npc[n].life = Main.npc[n].lifeMax / 4;
                            Main.npc[n].netUpdate = true;
                            if (Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                    }

                    if (!otherHandStillAlive)
                    {
                        if (Counter == 1)
                            Counter = 2;
                        else
                            Counter = 1;
                    }
                    else
                    {
                        Counter = 0;
                    }
                }
            }

            if (npc.ai[1] == 1f || npc.ai[1] == 2f) //spinning or DG mode
            {
                npc.localAI[2]++;
                float ratio = (float)npc.life / npc.lifeMax;
                float threshold = 20f + 100f * ratio;
                if (npc.localAI[2] >= threshold) //spray bones
                {
                    npc.localAI[2] = 0f;
                    if (threshold > 0 && npc.HasPlayerTarget && Main.netMode != 1)
                    {
                        Vector2 speed = Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * 6f;
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 vel = speed.RotatedBy(Math.PI * 2 / 8 * i);
                            vel += npc.velocity * (1f - ratio);
                            vel.Y -= Math.Abs(vel.X) * 0.2f;
                            Projectile.NewProjectile(npc.Center, vel, ModContent.ProjectileType<SkeletronBone>(), npc.defDamage / 9 * 2, 0f, Main.myPlayer);
                        }
                    }
                }
            }

            if (npc.ai[1] == 2f)
            {
                npc.defense = 9999;
                npc.damage = npc.defDamage * 15;
            }
        }

        public void SkeletronHandAI(NPC npc)
        {
            if (npc.life < npc.lifeMax / 2)
            {
                if (--Counter < 0)
                {
                    Counter = (int)(60f + 120f * npc.life / npc.lifeMax);
                    if (npc.HasPlayerTarget && Main.netMode != 1)
                    {
                        Vector2 speed = new Vector2(0f, -3f);
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 vel = speed.RotatedBy(Math.PI * 2 / 8 * i);
                            vel.Y -= Math.Abs(vel.X) * 0.2f;
                            Projectile.NewProjectile(npc.Center, vel, ModContent.ProjectileType<SkeletronBone>(), npc.damage / 4, 0f, Main.myPlayer);
                        }
                    }
                }
                if (--Counter2 < 0)
                {
                    Counter2 = 300;
                    if (npc.HasPlayerTarget && Main.netMode != 1)
                    {
                        Vector2 speed = Main.player[npc.target].Center - npc.Center;
                        speed.X += Main.rand.Next(-20, 21);
                        speed.Y += Main.rand.Next(-20, 21);
                        speed.Normalize();
                        speed *= 3f;
                        Projectile.NewProjectile(npc.Center, speed, ProjectileID.Skull, npc.damage / 4, 0, Main.myPlayer, -1f, 0f);
                    }
                }
            }

            if (Main.npc[(int)npc.ai[1]].ai[1] == 1f || Main.npc[(int)npc.ai[1]].ai[1] == 2f) //spinning or DG mode
            {
                if (!masoBool[0])
                {
                    masoBool[0] = true;
                    if (Main.netMode != 1 && npc.HasPlayerTarget) //throw undead miner
                    {
                        float gravity = 0.4f; //shoot down
                        const float time = 60f;
                        Vector2 distance = Main.player[npc.target].Center - npc.Center;
                        distance.X = distance.X / time;
                        distance.Y = distance.Y / time - 0.5f * gravity * time;
                        int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.BoneThrowingSkeleton);
                        if (n != 200)
                        {
                            Main.npc[n].velocity = distance;
                            if (Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                    }
                }
            }
            else
            {
                masoBool[0] = false;
            }
        }

        public void WallOfFleshAI(NPC npc)
        {
            wallBoss = npc.whoAmI;

            if (npc.ai[3] == 0f) //when spawned in, make one eye invul
            {
                for (int i = 0; i < 200; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].type == NPCID.WallofFleshEye && Main.npc[i].realLife == npc.whoAmI)
                    {
                        Main.npc[i].ai[2] = -1f;
                        Main.npc[i].netUpdate = true;
                        npc.ai[3] = 1f;
                        npc.netUpdate = true;
                        break;
                    }
                }
            }

            if (masoBool[0]) //phase 2
            {
                if (++Counter > 600)
                {
                    Counter = 0;
                    Counter2 = 0;
                    masoBool[1] = !masoBool[1];
                    masoBool[2] = false;
                    npc.netUpdate = true;
                }
                else if (Counter < 240) //special attacks
                {
                    if (masoBool[1]) //cursed inferno attack
                    {
                        if (++Counter2 > 5)
                        {
                            Counter2 = 0;
                            if (!masoBool[2])
                            {
                                masoBool[2] = true;
                                Timer = (int)(npc.Center.X + Math.Sign(npc.velocity.X) * 2500);
                            }
                            if (Math.Abs(npc.Center.X - Timer) > 800)
                            {
                                Vector2 spawnPos = new Vector2(Timer, npc.Center.Y);
                                Main.PlaySound(SoundID.Item34, spawnPos);
                                Timer += Math.Sign(npc.velocity.X) * -24; //wall of flame advances closer
                                const int offsetY = 800;
                                const int speed = 14;
                                if (Main.netMode != 1)
                                {
                                    Projectile.NewProjectile(spawnPos + Vector2.UnitY * offsetY, Vector2.UnitY * -speed, ModContent.ProjectileType<CursedFlamethrower>(), npc.damage / 4, 0f, Main.myPlayer);
                                    Projectile.NewProjectile(spawnPos + Vector2.UnitY * offsetY / 2, Vector2.UnitY * speed, ModContent.ProjectileType<CursedFlamethrower>(), npc.damage / 4, 0f, Main.myPlayer);
                                    Projectile.NewProjectile(spawnPos + Vector2.UnitY * -offsetY / 2, Vector2.UnitY * -speed, ModContent.ProjectileType<CursedFlamethrower>(), npc.damage / 4, 0f, Main.myPlayer);
                                    Projectile.NewProjectile(spawnPos + Vector2.UnitY * -offsetY, Vector2.UnitY * speed, ModContent.ProjectileType<CursedFlamethrower>(), npc.damage / 4, 0f, Main.myPlayer);

                                    //Projectile.NewProjectile(spawnPos + Vector2.UnitY * offsetY, Vector2.UnitY * -speed, ProjectileID.CursedFlameHostile, npc.damage / 4, 0f, Main.myPlayer);
                                    //Projectile.NewProjectile(spawnPos + Vector2.UnitY * -offsetY, Vector2.UnitY * speed, ProjectileID.CursedFlameHostile, npc.damage / 4, 0f, Main.myPlayer);
                                }
                            }
                            else
                            {
                                Counter = 240; //immediately end
                            }
                        }
                    }
                    else //ichor attack
                    {
                        if (++Counter2 > 10)
                        {
                            Counter2 = 0;
                            if (Main.netMode != 1)
                            {
                                Vector2 target = npc.Center;
                                target.X += Math.Sign(npc.velocity.X) * 1800f * Counter / 240f; //gradually targets further and further
                                for (int i = 0; i < 4; i++)
                                {
                                    Vector2 speed = target - npc.Center;
                                    speed.Y -= Math.Abs(speed.X) * 0.2f; //account for gravity
                                                                         //speed.Normalize(); speed *= 8f;
                                    speed /= 45f * 3f; //ichor has 3 updates per tick
                                    speed += npc.velocity / 3f;
                                    speed.X += Main.rand.Next(-20, 21) * 0.08f;
                                    speed.Y += Main.rand.Next(-20, 21) * 0.08f;
                                    Projectile.NewProjectile(npc.Center, speed, ProjectileID.GoldenShowerHostile, npc.damage / 5, 0f, Main.myPlayer);
                                }
                            }
                        }
                    }
                }
            }
            else if (npc.life < npc.lifeMax / 2) //enter phase 2
            {
                masoBool[0] = true;
                npc.netUpdate = true;
                Main.PlaySound(15, (int)npc.position.X, (int)npc.position.Y, 0);
            }

            /*if (--Counter < 0)
            {
                Counter = 60 + (int)(120f * npc.life / npc.lifeMax);
                if (Main.netMode != 1 && npc.HasPlayerTarget && Main.player[npc.target].active) //vanilla spaz p1 shoot fireball code
                {
                    Vector2 Speed = Main.player[npc.target].Center - npc.Center;
                    if (Speed.X * npc.velocity.X > 0) //don't shoot fireballs behind myself
                    {
                        Speed.Normalize();
                        int Damage;
                        Speed *= 10f;
                        Damage = npc.damage / 12;
                        Speed.X += Main.rand.Next(-40, 41) * 0.02f;
                        Speed.Y += Main.rand.Next(-40, 41) * 0.02f;
                        Speed += Main.player[npc.target].velocity / 5;
                        Projectile.NewProjectile(npc.Center + Speed * 4f, Speed, ProjectileID.CursedFlameHostile, Damage, 0f, Main.myPlayer);
                    }
                }
            }

            if (--Timer < 0) //ichor vomit
            {
                Timer = 300 + 300 * (int)((float)npc.life / npc.lifeMax);
                if (npc.HasPlayerTarget && Main.netMode != 1 && Main.player[npc.target].active)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 speed = Main.player[npc.target].Center - npc.Center;
                        speed.Y -= Math.Abs(speed.X) * 0.2f; //account for gravity
                        speed.Normalize();
                        speed *= 8f;
                        speed += npc.velocity / 3f;
                        speed.X += Main.rand.Next(-20, 21) * 0.08f;
                        speed.Y += Main.rand.Next(-20, 21) * 0.08f;
                        Projectile.NewProjectile(npc.Center, speed, ProjectileID.GoldenShowerHostile, npc.damage / 25, 0f, Main.myPlayer);
                    }
                }
            }*/

            if (npc.HasPlayerTarget && (Main.player[npc.target].dead || Vector2.Distance(npc.Center, Main.player[npc.target].Center) > 3000))
            {
                npc.TargetClosest(true);
                if (Main.player[npc.target].dead || Vector2.Distance(npc.Center, Main.player[npc.target].Center) > 3000)
                {
                    npc.position.X += 60 * Math.Sign(npc.velocity.X); //move faster to despawn
                    if (!masoBool[3]) //drop a resummon
                    {
                        masoBool[3] = true;
                        if (Main.netMode != 1 && !Main.hardMode)
                            Item.NewItem(npc.Hitbox, ModContent.ItemType<FleshierDoll>());
                    }
                }
                else if (Math.Abs(npc.velocity.X) > 6f)
                {
                    npc.position.X -= (Math.Abs(npc.velocity.X) - 6f) * Math.Sign(npc.velocity.X);
                }
            }
            else if (Math.Abs(npc.velocity.X) > 6f)
                npc.position.X -= (Math.Abs(npc.velocity.X) - 6f) * Math.Sign(npc.velocity.X);

            //dont do aura with swarm active
            if (Main.player[Main.myPlayer].active & !Main.player[Main.myPlayer].dead && Main.player[Main.myPlayer].ZoneUnderworldHeight && !(bool)ModLoader.GetMod("Fargowiltas").Call("SwarmActive"))
            {
                float velX = npc.velocity.X;
                if (velX > 5f)
                    velX = 5f;
                else if (velX < -5f)
                    velX = -5f;

                for (int i = 0; i < 10; i++) //dust
                {
                    Vector2 dustPos = new Vector2(2000 * npc.direction, 0f).RotatedBy(Math.PI / 3 * (-0.5 + Main.rand.NextDouble()));
                    int d = Dust.NewDust(npc.Center + dustPos, 0, 0, DustID.Fire);
                    Main.dust[d].scale += 1f;
                    Main.dust[d].velocity.X = velX;
                    Main.dust[d].velocity.Y = npc.velocity.Y;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                }

                if (++npc.localAI[1] > 15f)
                {
                    npc.localAI[1] = 0f; //tongue the player if they're 2000-2800 units away
                    if (Math.Abs(2400 - npc.Distance(Main.player[Main.myPlayer].Center)) < 400)
                    {
                        if (!Main.player[Main.myPlayer].tongued)
                            Main.PlaySound(15, Main.player[Main.myPlayer].Center, 0);
                        Main.player[Main.myPlayer].AddBuff(BuffID.TheTongue, 10);
                    }
                }
            }

            if (npc.life < npc.lifeMax / 10)
            {
                Counter++;
                if (!masoBool[3])
                {
                    masoBool[3] = true;
                    Main.PlaySound(15, (int)npc.position.X, (int)npc.position.Y, 0);
                }
            }
        }

        public bool WallOfFleshEyeAI(NPC npc)
        {
            if (masoBool[3])
                return true;

            if (npc.realLife != -1 && Main.npc[npc.realLife].GetGlobalNPC<EModeGlobalNPC>().masoBool[0]
                && Main.npc[npc.realLife].GetGlobalNPC<EModeGlobalNPC>().Counter < 240)
                npc.localAI[1] = 0; //dont fire during mouth's special attacks

            float maxTime = 540f;

            if (npc.realLife != -1 && Main.npc[npc.realLife].GetGlobalNPC<EModeGlobalNPC>().masoBool[3])
            {
                if (npc.ai[1] < maxTime - 180) //dont lower this if it's already telegraphing laser
                    maxTime = 240f;

                npc.localAI[1] = 0f; //no more regular lasers
            }

            if (++npc.ai[1] >= maxTime)
            {
                npc.ai[1] = 0f;
                if (npc.ai[2] == 0f)
                    npc.ai[2] = 1f;
                else
                    npc.ai[2] *= -1f;

                if (npc.ai[2] > 0 && Main.netMode != 1) //FIRE LASER
                {
                    Vector2 speed = Vector2.UnitX.RotatedBy(npc.ai[3]);
                    float ai0 = (npc.realLife != -1 && Main.npc[npc.realLife].velocity.X > 0) ? 1f : 0f;
                    if (Main.netMode != 1)
                        Projectile.NewProjectile(npc.Center, speed, mod.ProjectileType("PhantasmalDeathrayWOF"), npc.damage / 4, 0f, Main.myPlayer, ai0, npc.whoAmI);

                }
                npc.netUpdate = true;
            }

            if (npc.ai[2] >= 0f)
            {
                npc.alpha = 175;
                npc.dontTakeDamage = true;
                if (npc.ai[1] <= 90)
                {
                    masoBool[3] = true;
                    npc.AI();
                    masoBool[3] = false;
                    npc.localAI[1] = 0f;
                    npc.rotation = npc.ai[3];
                    return false;
                }
                else
                {
                    npc.ai[2] = 1;
                }
            }
            else
            {
                npc.alpha = 0;
                npc.dontTakeDamage = false;
                if (npc.ai[1] > maxTime - 180f)
                {
                    if (Main.rand.Next(4) < 3) //dust telegraphs switch
                    {
                        int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 90, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 114, default(Color), 3.5f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 1.8f;
                        Main.dust[dust].velocity.Y -= 0.5f;
                        if (Main.rand.Next(4) == 0)
                        {
                            Main.dust[dust].noGravity = false;
                            Main.dust[dust].scale *= 0.5f;
                        }
                    }

                    float stopTime = maxTime - 90f;
                    if (npc.ai[1] == stopTime) //shoot warning dust in phase 2
                    {
                        int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                        if (t != -1)
                        {
                            if (npc.Distance(Main.player[t].Center) < 3000)
                                Main.PlaySound(15, (int)Main.player[t].position.X, (int)Main.player[t].position.Y, 0);
                            npc.ai[2] = -2f;
                            npc.ai[3] = (npc.Center - Main.player[t].Center).ToRotation();
                            if (npc.realLife != -1 && Main.npc[npc.realLife].velocity.X > 0)
                                npc.ai[3] += (float)Math.PI;

                            float ai0 = (npc.realLife != -1 && Main.npc[npc.realLife].velocity.X > 0) ? 1f : 0f;
                            Vector2 speed = Vector2.UnitX.RotatedBy(npc.ai[3]);
                            if (Main.netMode != 1)
                                Projectile.NewProjectile(npc.Center, speed, mod.ProjectileType("PhantasmalDeathrayWOFS"), 0, 0f, Main.myPlayer, ai0, npc.whoAmI);
                        }
                        npc.netUpdate = true;
                    }
                    else if (npc.ai[1] > stopTime)
                    {
                        masoBool[3] = true;
                        npc.AI();
                        masoBool[3] = false;
                        npc.localAI[1] = 0f;
                        npc.rotation = npc.ai[3];
                        return false;
                    }
                }
            }

            return true;
        }

        public bool RetinazerAI(NPC npc)
        {
            if (npc.life == 1)
                RegenTimer = 2;

            retiBoss = npc.whoAmI;
            bool spazAlive = BossIsAlive(ref spazBoss, NPCID.Spazmatism);

            if (!masoBool[0]) //start phase 2
            {
                masoBool[0] = true;
                npc.ai[0] = 1f;
                npc.ai[1] = 0.0f;
                npc.ai[2] = 0.0f;
                npc.ai[3] = 0.0f;
                npc.netUpdate = true;
            }

            npc.dontTakeDamage = npc.life == 1;
            //become vulnerable again when both twins at 1hp
            if (npc.dontTakeDamage && npc.HasPlayerTarget && (!BossIsAlive(ref spazBoss, NPCID.Spazmatism) || Main.npc[spazBoss].life == 1))
                npc.dontTakeDamage = false;

            if (npc.ai[0] < 4f) //going to phase 3
            {
                if (npc.life <= npc.lifeMax / 2)
                {
                    //npc.ai[0] = 4f;
                    npc.ai[0] = 604f; //initiate spin immediately
                    npc.netUpdate = true;
                    Main.PlaySound(15, (int)npc.Center.X, (int)npc.Center.Y, 0);
                }
            }
            else //in phase 3
            {
                Player p = Main.player[Main.myPlayer];
                const float auraDistance = 2000;
                float range = npc.Distance(p.Center);
                if (range > auraDistance && range < 10000)
                    p.AddBuff(BuffID.Burning, 2);

                Vector2 dustPos = Vector2.Normalize(p.Center - npc.Center) * auraDistance;
                for (int i = 0; i < 20; i++) //dust
                {
                    int d = Dust.NewDust(npc.Center + dustPos.RotatedBy(Math.PI / 3 * (-0.5 + Main.rand.NextDouble())), 0, 0, DustID.Fire);
                    Main.dust[d].velocity = npc.velocity;
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].scale++;
                }

                if (npc.life == 1 && --Counter2 < 0) //when brought to 1hp, begin shooting dark stars
                {
                    Counter2 = 240;
                    if (Main.netMode != 1 && npc.HasPlayerTarget)
                    {
                        Vector2 distance = Main.player[npc.target].Center - npc.Center;
                        distance.Normalize();
                        distance *= 10f;
                        for (int i = 0; i < 12; i++)
                            Projectile.NewProjectile(npc.Center, distance.RotatedBy(2 * Math.PI / 12 * i),
                                ModContent.ProjectileType<DarkStar>(), npc.damage / 5, 0f, Main.myPlayer);
                    }
                }

                //dust code
                if (Main.rand.Next(4) < 3)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 90, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default(Color), 3.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    if (Main.rand.Next(4) == 0)
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                SharkCount = 253;

                //become vulnerable again when both twins at 1hp
                if (npc.dontTakeDamage && (!BossIsAlive(ref spazBoss, NPCID.Spazmatism) || Main.npc[spazBoss].life == 1))
                    npc.dontTakeDamage = false;

                //2*pi * (# of full circles) / (seconds to finish rotation) / (ticks per sec)
                const float rotationInterval = 2f * (float)Math.PI * 1f / 4f / 60f;

                npc.ai[0]++; //base value is 4
                switch (Counter) //laser code idfk
                {
                    case 0:
                        if (!npc.HasValidTarget)
                        {
                            npc.ai[0]--; //stop counting up if player is dead
                            if (!spazAlive) //despawn REALLY fast
                                npc.velocity.Y -= 0.5f;
                        }
                        if (npc.ai[0] > 604f)
                        {
                            npc.ai[0] = 4f;
                            if (npc.HasPlayerTarget)
                            {
                                Counter++;
                                npc.ai[3] = -npc.rotation;
                                if (--npc.ai[2] > 295f)
                                    npc.ai[2] = 295f;
                                masoBool[2] = (Main.player[npc.target].Center.X - npc.Center.X < 0);

                                for (int i = 0; i < 72; i++) //warning dust ring
                                {
                                    Vector2 vector6 = Vector2.UnitY * 60f;
                                    vector6 = vector6.RotatedBy((i - (72 / 2 - 1)) * 6.28318548f / 72) + npc.Center;
                                    Vector2 vector7 = vector6 - npc.Center;
                                    int d = Dust.NewDust(vector6 + vector7, 0, 0, 90, 0f, 0f, 0, default(Color), 3f);
                                    Main.dust[d].noGravity = true;
                                    Main.dust[d].velocity = vector7;
                                }

                                Main.PlaySound(36, (int)npc.Center.X, (int)npc.Center.Y, -1, 1f, 0f); //eoc roar
                            }
                            npc.netUpdate = true;
                            if (Main.netMode == 2) //synchronize counter with clients
                            {
                                var netMessage = mod.GetPacket();
                                netMessage.Write((byte)5);
                                netMessage.Write((byte)npc.whoAmI);
                                netMessage.Write(masoBool[2]);
                                netMessage.Write(Counter);
                                netMessage.Send();
                            }
                        }
                        break;

                    case 1: //slowing down, beginning rotation
                        npc.velocity *= 1f - (npc.ai[0] - 4f) / 120f;
                        npc.localAI[1] = 0f;
                        //if (--npc.ai[2] > 295f) npc.ai[2] = 295f;
                        npc.ai[3] -= (npc.ai[0] - 4f) / 120f * rotationInterval * (masoBool[2] ? 1f : -1f);
                        npc.rotation = -npc.ai[3];

                        if (npc.ai[0] >= 124f) //FIRE LASER
                        {
                            if (Main.netMode != 1)
                            {
                                Vector2 speed = Vector2.UnitX.RotatedBy(npc.rotation);
                                Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<PhantasmalDeathray>(), npc.damage / 2, 0f, Main.myPlayer, 0f, npc.whoAmI);
                            }
                            Counter++;
                            npc.ai[0] = 4f;
                            npc.netUpdate = true;
                            if (Main.netMode == 2) //synchronize counter with clients
                            {
                                var netMessage = mod.GetPacket();
                                netMessage.Write((byte)5);
                                netMessage.Write((byte)npc.whoAmI);
                                netMessage.Write(masoBool[2]);
                                netMessage.Write(Counter);
                                netMessage.Send();
                            }
                        }
                        return false;

                    case 2: //spinning full speed
                        npc.velocity = Vector2.Zero;
                        npc.localAI[1] = 0f;
                        //if (--npc.ai[2] > 295f) npc.ai[2] = 295f;
                        npc.ai[3] -= rotationInterval * (masoBool[2] ? 1f : -1f);
                        npc.rotation = -npc.ai[3];

                        if (npc.ai[0] >= 244f)
                        {
                            Counter++;
                            npc.ai[0] = 4f;
                            npc.netUpdate = true;
                            if (Main.netMode == 2) //synchronize counter with clients
                            {
                                var netMessage = mod.GetPacket();
                                netMessage.Write((byte)5);
                                netMessage.Write((byte)npc.whoAmI);
                                netMessage.Write(masoBool[2]);
                                netMessage.Write(Counter);
                                netMessage.Send();
                            }
                        }
                        else if (!npc.HasValidTarget) //end spin immediately if player dead
                        {
                            npc.TargetClosest(false);
                            if (!npc.HasValidTarget)
                                npc.ai[0] = 244f;
                        }
                        return false;

                    case 3: //laser done, slowing down spin, moving again
                        npc.velocity *= (npc.ai[0] - 4f) / 60f;
                        npc.localAI[1] = 0f;
                        //if (--npc.ai[2] > 295f) npc.ai[2] = 295f;
                        npc.ai[3] -= (1f - (npc.ai[0] - 4f) / 60f) * rotationInterval * (masoBool[2] ? 1f : -1f);
                        npc.rotation = -npc.ai[3];

                        if (npc.ai[0] >= 64f)
                        {
                            Counter = 0;
                            npc.ai[0] = 4f;
                            npc.netUpdate = true;
                            if (Main.netMode == 2) //synchronize counter with clients
                            {
                                var netMessage = mod.GetPacket();
                                netMessage.Write((byte)5);
                                netMessage.Write((byte)npc.whoAmI);
                                netMessage.Write(masoBool[2]);
                                netMessage.Write(Counter);
                                netMessage.Send();
                            }
                        }
                        return false;

                    default:
                        Counter = 0;
                        npc.ai[0] = 4f;
                        npc.netUpdate = true;
                        if (Main.netMode == 2) //synchronize counter with clients
                        {
                            var netMessage = mod.GetPacket();
                            netMessage.Write((byte)5);
                            netMessage.Write((byte)npc.whoAmI);
                            netMessage.Write(masoBool[2]);
                            netMessage.Write(Counter);
                            netMessage.Send();
                        }
                        break;
                }

                //npc.position += npc.velocity / 4f;

                //if (Counter == 600 && Main.netMode != 1 && npc.HasPlayerTarget)
                //{
                //    Vector2 vector200 = Main.player[npc.target].Center - npc.Center;
                //    vector200.Normalize();
                //    float num1225 = -1f;
                //    if (vector200.X < 0f)
                //    {
                //        num1225 = 1f;
                //    }
                //    vector200 = vector200.RotatedBy(-num1225 * 1.04719755f, default(Vector2));
                //    Projectile.NewProjectile(npc.Center, vector200, ModContent.ProjectileType<PhantasmalDeathray>(), npc.damage / 2, 0f, Main.myPlayer, num1225 * 0.0104719755f, npc.whoAmI);
                //    npc.netUpdate = true;
                //}
            }

            /*if (!BossIsAlive(ref spazBoss, NPCID.Spazmatism) && targetAlive)
            {
                Timer--;

                if (Timer <= 0)
                {
                    Timer = 600;
                    if (Main.netMode != 1)
                    {
                        int spawn = NPC.NewNPC((int)npc.position.X + Main.rand.Next(-1000, 1000), (int)npc.position.Y + Main.rand.Next(-400, -100), NPCID.Spazmatism);
                        if (spawn != 200)
                        {
                            Main.npc[spawn].life = Main.npc[spawn].lifeMax / 4;
                            if (Main.netMode == 2)
                            {
                                NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Spazmatism has been revived!"), new Color(175, 75, 255));
                                NetMessage.SendData(23, -1, -1, null, spawn);
                            }
                            else
                            {
                                Main.NewText("Spazmatism has been revived!", 175, 75, 255);
                            }
                        }
                    }
                }
            }*/

            return true;
        }

        public void SpazmatismAI(NPC npc)
        {
            if (npc.life == 1)
                RegenTimer = 2;

            spazBoss = npc.whoAmI;
            bool retiAlive = BossIsAlive(ref retiBoss, NPCID.Retinazer);

            if (!masoBool[0]) //spawn in phase 2
            {
                masoBool[0] = true;
                npc.ai[0] = 1f;
                npc.ai[1] = 0.0f;
                npc.ai[2] = 0.0f;
                npc.ai[3] = 0.0f;
                npc.netUpdate = true;
            }

            npc.dontTakeDamage = npc.life == 1;
            //become vulnerable again when both twins at 1hp
            if (npc.dontTakeDamage && npc.HasPlayerTarget && (!BossIsAlive(ref retiBoss, NPCID.Retinazer) || Main.npc[retiBoss].life == 1))
                npc.dontTakeDamage = false;

            if (npc.ai[0] < 4f)
            {
                if (npc.life <= npc.lifeMax / 2) //going to phase 3
                {
                    npc.ai[0] = 4f;
                    npc.netUpdate = true;
                    Main.PlaySound(15, (int)npc.Center.X, (int)npc.Center.Y, 0);

                    int index = npc.FindBuffIndex(BuffID.CursedInferno);
                    if (index != -1)
                        npc.DelBuff(index); //remove cursed inferno debuff if i have it

                    npc.buffImmune[BuffID.CursedInferno] = true;
                    npc.buffImmune[BuffID.OnFire] = true;
                    npc.buffImmune[BuffID.ShadowFlame] = true;
                    npc.buffImmune[BuffID.Frostburn] = true;
                }
            }
            else //in phase 3
            {
                npc.position += npc.velocity / 10f;

                if (npc.ai[1] == 0f) //not dashing
                {
                    if (retiAlive && (Main.npc[retiBoss].ai[0] < 4f || Main.npc[retiBoss].GetGlobalNPC<EModeGlobalNPC>().Counter == 0)) //reti is in normal AI
                    {
                        npc.ai[1] = 1; //switch to dashing
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                    }

                    if (++Counter > 40)
                    {
                        Counter = 0;
                        if (Main.netMode != 1 && npc.HasPlayerTarget) //vanilla spaz p1 shoot fireball code
                        {
                            Vector2 Speed = Main.player[npc.target].Center - npc.Center;
                            Speed.Normalize();
                            int Damage;
                            if (Main.expertMode)
                            {
                                Speed *= 14f;
                                Damage = 22;
                            }
                            else
                            {
                                Speed *= 12f;
                                Damage = 25;
                            }
                            Damage = (int)(Damage * (1 + FargoSoulsWorld.TwinsCount * .0125));
                            Projectile.NewProjectile(npc.Center + Speed * 4f, Speed, ProjectileID.CursedFlameHostile, Damage, 0f, Main.myPlayer);
                        }
                    }
                }
                else //dashing
                {
                    if (retiAlive && Main.npc[retiBoss].ai[0] >= 4f && Main.npc[retiBoss].GetGlobalNPC<EModeGlobalNPC>().Counter != 0) //reti is doing the spin
                    {
                        npc.ai[1] = 0; //switch to not dashing
                        npc.netUpdate = true;
                    }
                    if (npc.HasValidTarget && ++Counter > 3) //cursed flamethrower when dashing
                    {
                        Counter = 0;
                        Projectile.NewProjectile(npc.Center, npc.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-6f, 6f))) * 0.5f, ProjectileID.EyeFire, npc.damage / 4, 0f, Main.myPlayer);
                    }
                }

                if (npc.life == 1 && --Counter2 < 0) //when brought to 1hp, begin shooting dark stars
                {
                    Counter2 = 120;
                    if (Main.netMode != 1 && npc.HasPlayerTarget)
                    {
                        Vector2 distance = Main.player[npc.target].Center - npc.Center;
                        distance.Normalize();
                        distance *= 14f;
                        for (int i = 0; i < 8; i++)
                            Projectile.NewProjectile(npc.Center, distance.RotatedBy(2 * Math.PI / 8 * i),
                                ModContent.ProjectileType<DarkStar>(), npc.damage / 5, 0f, Main.myPlayer);
                    }
                }

                //dust code
                if (Main.rand.Next(4) < 3)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 89, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default(Color), 3.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    if (Main.rand.Next(4) == 0)
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                SharkCount = 254;
            }

            /*if (!retiAlive && npc.HasPlayerTarget && Main.player[npc.target].active)
            {
                Timer--;

                if (Timer <= 0)
                {
                    Timer = 600;
                    if (Main.netMode != 1)
                    {
                        int spawn = NPC.NewNPC((int)npc.position.X + Main.rand.Next(-1000, 1000), (int)npc.position.Y + Main.rand.Next(-400, -100), NPCID.Retinazer);
                        if (spawn != 200)
                        {
                            Main.npc[spawn].life = Main.npc[spawn].lifeMax / 4;
                            if (Main.netMode == 2)
                            {
                                NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Retinazer has been revived!"), new Color(175, 75, 255));
                                NetMessage.SendData(23, -1, -1, null, spawn);
                            }
                            else
                            {
                                Main.NewText("Retinazer has been revived!", 175, 75, 255);
                            }
                        }
                    }
                }
            }*/
        }

        public bool DestroyerAI(NPC npc)
        {
            destroyBoss = npc.whoAmI;
            
            if (!masoBool[0])
            {
                if (npc.life < (int)(npc.lifeMax * .75))
                {
                    masoBool[0] = true;
                    Counter = 900;
                    npc.netUpdate = true;
                    if (npc.HasPlayerTarget)
                        Main.PlaySound(15, (int)Main.player[npc.target].position.X, (int)Main.player[npc.target].position.Y, 0);
                }
                RegenTimer = 2;
            }
            else
            {
                if (npc.HasValidTarget && !Main.dayTime)
                {
                    if (masoBool[1]) //spinning
                    {
                        npc.netUpdate = true;
                        npc.velocity += npc.velocity.RotatedBy(Math.PI / 2) * npc.velocity.Length() / Counter2;
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + 1.57f;

                        if (++npc.localAI[2] > 45) //shoot star spreads into the circle
                        {
                            npc.localAI[2] = 0;
                            if (Main.netMode != 1 && !Main.player[npc.target].HasBuff(ModContent.BuffType<LightningRod>()))
                            {
                                Vector2 distance = Main.player[npc.target].Center - npc.Center;
                                double angleModifier = MathHelper.ToRadians(5) * distance.Length() / 1800.0;
                                distance.Normalize();
                                distance *= 7f;
                                int type = ModContent.ProjectileType<DarkStar>();
                                Projectile.NewProjectile(npc.Center, distance.RotatedBy(-angleModifier), type, npc.damage / 12, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center, distance, type, npc.damage / 12, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center, distance.RotatedBy(angleModifier), type, npc.damage / 12, 0f, Main.myPlayer);
                            }
                        }

                        Vector2 pivot = npc.Center;
                        pivot += Vector2.Normalize(npc.velocity.RotatedBy(Math.PI / 2)) * 600;

                        if (++Timer > 100)
                        {
                            Timer = 0;
                            if (Main.netMode != 1)
                            {
                                const int max = 6;
                                for (int i = 0; i < max; i++)
                                {
                                    Vector2 speed = npc.DirectionTo(pivot).RotatedBy(2 * Math.PI / max * i);
                                    Vector2 spawnPos = pivot - speed * 600;
                                    Projectile.NewProjectile(spawnPos, speed, ModContent.ProjectileType<DestroyerLaser>(), npc.damage / 12, 0f, Main.myPlayer);
                                }
                            }
                        }

                        for (int i = 0; i < 20; i++) //arena dust
                        {
                            Vector2 offset = new Vector2();
                            double angle = Main.rand.NextDouble() * 2d * Math.PI;
                            offset.X += (float)(Math.Sin(angle) * 600);
                            offset.Y += (float)(Math.Cos(angle) * 600);
                            Dust dust = Main.dust[Dust.NewDust(pivot + offset - new Vector2(4, 4), 0, 0, 112, 0, 0, 100, Color.White, 1f)];
                            dust.velocity = Vector2.Zero;
                            if (Main.rand.Next(3) == 0)
                                dust.velocity += Vector2.Normalize(offset) * 5f;
                            dust.noGravity = true;
                        }

                        Player target = Main.player[npc.target];
                        if (target.active && !target.dead) //arena effect
                        {
                            float distance = target.Distance(pivot);
                            if (distance > 600 && distance < 3000)
                            {
                                Vector2 movement = pivot - target.Center;
                                float difference = movement.Length() - 600;
                                movement.Normalize();
                                movement *= difference < 17f ? difference : 17f;
                                target.position += movement;

                                for (int i = 0; i < 20; i++)
                                {
                                    int d = Dust.NewDust(target.position, target.width, target.height, 112, 0f, 0f, 0, default(Color), 2f);
                                    Main.dust[d].noGravity = true;
                                    Main.dust[d].velocity *= 5f;
                                }
                            }
                        }

                        if (++Counter > 300) //go back to normal AI
                        {
                            Counter = 0;
                            Counter2 = 0;
                            masoBool[1] = false;
                            masoBool[2] = false;
                            NetUpdateMaso(npc.whoAmI);
                        }
                    }
                    else
                    {
                        float num14 = 16f;    //max speed?
                        float num15 = 0.1f;   //turn speed?
                        float num16 = 0.15f;   //acceleration?

                        Vector2 target = Main.player[npc.target].Center;
                        if (masoBool[2]) //move MUCH faster, approach a position nearby
                        {
                            num15 = 0.4f;
                            num16 = 0.5f;

                            target += Main.player[npc.target].DirectionTo(npc.Center) * 600;

                            if (++Counter > 300) //move way faster if still not in range
                                num14 *= 2f;

                            if (npc.Distance(target) < 50)
                            {
                                Counter = 0;
                                Counter2 = (int)npc.Distance(Main.player[npc.target].Center);
                                masoBool[1] = true;
                                npc.velocity = 20 * npc.DirectionTo(Main.player[npc.target].Center).RotatedBy(-Math.PI / 2);
                                NetUpdateMaso(npc.whoAmI);
                                Main.PlaySound(15, (int)Main.player[npc.target].position.X, (int)Main.player[npc.target].position.Y, 0);
                                if (Main.netMode != 1)
                                {
                                    for (int i = 0; i < Main.maxProjectiles; i++)
                                    {
                                        if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<DarkStar>())
                                            Main.projectile[i].Kill();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (++Counter > 900) //change state
                            {
                                Counter = 0;
                                masoBool[2] = true;
                                NetUpdateMaso(npc.whoAmI);
                            }
                            else if (Counter == 900 - 120) //telegraph with roar
                            {
                                Main.PlaySound(15, (int)Main.player[npc.target].position.X, (int)Main.player[npc.target].position.Y, 0);
                            }
                        }
                        float num17 = target.X;
                        float num18 = target.Y;

                        float num21 = num17 - npc.Center.X;
                        float num22 = num18 - npc.Center.Y;
                        float num23 = (float)Math.Sqrt((double)num21 * (double)num21 + (double)num22 * (double)num22);

                        //ground movement code but it always runs
                        float num2 = (float)Math.Sqrt(num21 * num21 + num22 * num22);
                        float num3 = Math.Abs(num21);
                        float num4 = Math.Abs(num22);
                        float num5 = num14 / num2;
                        float num6 = num21 * num5;
                        float num7 = num22 * num5;
                        if ((npc.velocity.X > 0f && num6 > 0f || npc.velocity.X < 0f && num6 < 0f) && (npc.velocity.Y > 0f && num7 > 0f || npc.velocity.Y < 0f && num7 < 0f))
                        {
                            if (npc.velocity.X < num6)
                                npc.velocity.X += num16;
                            else if (npc.velocity.X > num6)
                                npc.velocity.X -= num16;
                            if (npc.velocity.Y < num7)
                                npc.velocity.Y += num16;
                            else if (npc.velocity.Y > num7)
                                npc.velocity.Y -= num16;
                        }
                        if (npc.velocity.X > 0f && num6 > 0f || npc.velocity.X < 0f && num6 < 0f || npc.velocity.Y > 0f && num7 > 0f || npc.velocity.Y < 0f && num7 < 0f)
                        {
                            if (npc.velocity.X < num6)
                                npc.velocity.X += num15;
                            else if (npc.velocity.X > num6)
                                npc.velocity.X -= num15;
                            if (npc.velocity.Y < num7)
                                npc.velocity.Y += num15;
                            else if (npc.velocity.Y > num7)
                                npc.velocity.Y -= num15;

                            if (Math.Abs(num7) < num14 * 0.2f && (npc.velocity.X > 0f && num6 < 0f || npc.velocity.X < 0f && num6 > 0f))
                            {
                                if (npc.velocity.Y > 0f)
                                    npc.velocity.Y += num15 * 2f;
                                else
                                    npc.velocity.Y -= num15 * 2f;
                            }
                            if (Math.Abs(num6) < num14 * 0.2f && (npc.velocity.Y > 0f && num7 < 0f || npc.velocity.Y < 0f && num7 > 0f))
                            {
                                if (npc.velocity.X > 0f)
                                    npc.velocity.X += num15 * 2f;
                                else
                                    npc.velocity.X -= num15 * 2f;
                            }
                        }
                        else if (num3 > num4)
                        {
                            if (npc.velocity.X < num6)
                                npc.velocity.X += num15 * 1.1f;
                            else if (npc.velocity.X > num6)
                                npc.velocity.X -= num15 * 1.1f;

                            if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num14 * 0.5f)
                            {
                                if (npc.velocity.Y > 0f)
                                    npc.velocity.Y += num15;
                                else
                                    npc.velocity.Y -= num15;
                            }
                        }
                        else
                        {
                            if (npc.velocity.Y < num7)
                                npc.velocity.Y += num15 * 1.1f;
                            else if (npc.velocity.Y > num7)
                                npc.velocity.Y -= num15 * 1.1f;

                            if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num14 * 0.5f)
                            {
                                if (npc.velocity.X > 0f)
                                    npc.velocity.X += num15;
                                else
                                    npc.velocity.X -= num15;
                            }
                        }
                        npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + 1.57f;
                        npc.netUpdate = true;
                        npc.localAI[0] = 1f;

                        float ratio = (float)npc.life / npc.lifeMax;
                        if (ratio > 0.5f)
                            ratio = 0.5f;
                        npc.position += npc.velocity * (.5f - ratio);
                    }

                    if (Main.netMode == 2 && npc.netUpdate && --npc.netSpam < 0) //manual mp sync control
                    {
                        npc.netUpdate = false;
                        npc.netSpam = 5;
                        NetMessage.SendData(23, -1, -1, null, npc.whoAmI);
                    }
                    return false;
                }
            }

            return true;
        }

        public void DestroyerSegmentAI(NPC npc)
        {
            if (npc.realLife >= 0 && npc.realLife < 200 && Main.npc[npc.realLife].life > 0 && npc.life > 0)
            {
                npc.defense = npc.defDefense;

                if (Main.npc[npc.realLife].GetGlobalNPC<EModeGlobalNPC>().masoBool[1]) //spinning
                {
                    if (!masoBool[0])
                        masoBool[0] = true;

                    Counter2 = 180;
                    Vector2 pivot = Main.npc[npc.realLife].Center;
                    pivot += Vector2.Normalize(Main.npc[npc.realLife].velocity.RotatedBy(Math.PI / 2)) * 600;
                    if (npc.Distance(pivot) < 600) //make sure body doesnt coil into the circling zone
                        npc.Center = pivot + npc.DirectionFrom(pivot) * 600;

                    //enrage if player is outside the ring
                    /*if (Main.npc[npc.realLife].HasValidTarget && Main.npc[npc.realLife].GetGlobalNPC<EModeGlobalNPC>().Counter > 30 && Main.player[Main.npc[npc.realLife].target].Distance(pivot) > 600 && Main.netMode != 1 && Main.rand.Next(120) == 0)
                    {
                        Vector2 distance = Main.player[npc.target].Center - npc.Center;
                        distance.X += Main.rand.Next(-200, 201);
                        distance.Y += Main.rand.Next(-200, 201);

                        double angleModifier = MathHelper.ToRadians(10) * distance.Length() / 1200.0;
                        distance.Normalize();
                        distance *= Main.rand.NextFloat(20f, 30f);

                        int type = ModContent.ProjectileType<DarkStar>();
                        int p = Projectile.NewProjectile(npc.Center, distance.RotatedBy(-angleModifier), type, npc.damage / 3, 0f, Main.myPlayer);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].timeLeft = 150;
                        p = Projectile.NewProjectile(npc.Center, distance, type, npc.damage / 3, 0f, Main.myPlayer);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].timeLeft = 150;
                        p = Projectile.NewProjectile(npc.Center, distance.RotatedBy(angleModifier), type, npc.damage / 3, 0f, Main.myPlayer);
                        if (p != Main.maxProjectiles)
                            Main.projectile[p].timeLeft = 150;
                    }*/
                }

                if (Counter2 > 0) //no lasers or stars while or shortly after spinning
                {
                    Counter2--;
                    npc.defense = 9999; //boosted defense during this same duration
                    if (npc.localAI[0] > 1350)
                        npc.localAI[0] = 1350;
                }
                else if (npc.ai[2] != 0)
                {
                    npc.localAI[0] = 0f;
                    int cap = Main.npc[npc.realLife].lifeMax / Main.npc[npc.realLife].life;
                    if (cap > 70) //prevent meme scaling at super low life
                        cap = 70;
                    Counter += Main.rand.Next(2 + cap) + 1;
                    if (Counter >= Main.rand.Next(1400, 26000))
                    {
                        Counter = 0;
                        if (Main.netMode != 1 && npc.HasPlayerTarget)
                        {
                            Vector2 distance = Main.player[npc.target].Center - npc.Center;
                            double angleModifier = MathHelper.ToRadians(5) * distance.Length() / 1800.0;
                            distance.Normalize();
                            distance *= 8f;
                            int type = ModContent.ProjectileType<DarkStar>();
                            Projectile.NewProjectile(npc.Center, distance.RotatedBy(-angleModifier), type, npc.damage / 5, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, distance.RotatedBy(angleModifier), type, npc.damage / 5, 0f, Main.myPlayer);
                        }
                    }
                }
            }
            else if (Main.netMode != 1)
            {
                npc.life = 0;
                if (Main.netMode == 2)
                    NetMessage.SendData(23, -1, -1, null, npc.whoAmI);
                npc.active = false;
                //npc.checkDead();
                return;
            }

            if (npc.buffType[0] != 0)
                npc.DelBuff(0);
        }

        public void SkeletronPrimeAI(NPC npc)
        {
            primeBoss = npc.whoAmI;
            npc.dontTakeDamage = !masoBool[0];

            if (npc.ai[0] != 2f) //in phase 1
            {
                if (npc.life < npc.lifeMax * .75) //enter phase 2
                {
                    npc.ai[0] = 2f;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;

                    if (Main.netMode != 1)
                    {
                        if (!NPC.AnyNPCs(NPCID.PrimeLaser)) //revive all dead limbs
                        {
                            int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeLaser, npc.whoAmI, 1f, npc.whoAmI, 0f, 150f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                        if (!NPC.AnyNPCs(NPCID.PrimeSaw))
                        {
                            int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeSaw, npc.whoAmI, 1f, npc.whoAmI, 0f, 0f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                        if (!NPC.AnyNPCs(NPCID.PrimeCannon))
                        {
                            int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeCannon, npc.whoAmI, -1f, npc.whoAmI, 0f, 150f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                        if (!NPC.AnyNPCs(NPCID.PrimeVice))
                        {
                            int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeVice, npc.whoAmI, -1f, npc.whoAmI, 0f, 0f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                    }

                    Main.PlaySound(15, (int)npc.position.X, (int)npc.position.Y, 0);
                    return;
                }

                if (npc.ai[0] != 1f) //limb is dead and needs reviving
                {
                    npc.ai[3]++;
                    if (npc.ai[3] > 1800f) //revive a dead limb
                    {
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                        if (Main.netMode != 1)
                        {
                            int n = 200;
                            switch ((int)npc.ai[0])
                            {
                                case 3: //laser
                                    n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeLaser, npc.whoAmI, 1f, npc.whoAmI, 0f, 150f, npc.target);
                                    break;
                                case 4: //cannon
                                    n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeCannon, npc.whoAmI, -1f, npc.whoAmI, 0f, 0f, npc.target);
                                    break;
                                case 5: //saw
                                    n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeSaw, npc.whoAmI, 1f, npc.whoAmI, 0f, 0f, npc.target);
                                    break;
                                case 6: //vice
                                    n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeVice, npc.whoAmI, -1f, npc.whoAmI, 0f, 150f, npc.target);
                                    break;
                                default:
                                    break;
                            }
                            if (n < 200)
                            {
                                Main.npc[n].life = Main.npc[n].lifeMax / 4;
                                Main.npc[n].netUpdate = true;
                            }
                        }

                        if (!NPC.AnyNPCs(NPCID.PrimeLaser)) //look for any other dead limbs
                            npc.ai[0] = 3f;
                        else if (!NPC.AnyNPCs(NPCID.PrimeCannon))
                            npc.ai[0] = 4f;
                        else if (!NPC.AnyNPCs(NPCID.PrimeSaw))
                            npc.ai[0] = 5f;
                        else if (!NPC.AnyNPCs(NPCID.PrimeVice))
                            npc.ai[0] = 6f;
                        else
                            npc.ai[0] = 1f;
                    }
                }

                if (++Timer >= 360)
                {
                    Timer = 0;

                    if (npc.HasPlayerTarget) //skeleton commando rockets LUL
                    {
                        Vector2 speed = Main.player[npc.target].Center - npc.Center;
                        speed.X += Main.rand.Next(-20, 21);
                        speed.Y += Main.rand.Next(-20, 21);
                        speed.Normalize();

                        int damage = npc.damage / 4;

                        if (Main.netMode != 1)
                        {
                            Projectile.NewProjectile(npc.Center, 3f * speed, ProjectileID.RocketSkeleton, damage, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, 3f * speed.RotatedBy(MathHelper.ToRadians(5f)), ProjectileID.RocketSkeleton, damage, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, 3f * speed.RotatedBy(MathHelper.ToRadians(-5f)), ProjectileID.RocketSkeleton, damage, 0f, Main.myPlayer);
                        }

                        Main.PlaySound(SoundID.Item11, npc.Center);
                    }
                }
            }
            else //in phase 2
            {
                npc.dontTakeDamage = false;

                int timeToShoot = 300;

                if (npc.ai[1] == 1f && npc.ai[2] > 2f) //spinning
                {
                    timeToShoot = 120;
                    if (npc.HasValidTarget)
                        npc.position += npc.DirectionTo(Main.player[npc.target].Center) * 5;
                }
                else if (npc.ai[1] == 2f) //dg phase
                {
                    if (!Main.dayTime)
                        npc.position -= npc.velocity / 10;
                }
                else //not spinning
                {
                    npc.position += npc.velocity / 3f;
                }

                if (++Timer >= timeToShoot) //skeleton commando rockets LUL
                {
                    Timer = 0;
                    if (npc.ai[1] != 2 && npc.HasPlayerTarget) //dont do during DG
                    {
                        Vector2 speed = Main.player[npc.target].Center - npc.Center;
                        speed.Normalize();

                        int damage = npc.defDamage * 2 / 7;

                        if (Main.netMode != 1)
                        {
                            Projectile.NewProjectile(npc.Center, 4f * speed, ProjectileID.RocketSkeleton, damage, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, 4f * speed.RotatedBy(MathHelper.ToRadians(3f)), ProjectileID.RocketSkeleton, damage, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, 4f * speed.RotatedBy(MathHelper.ToRadians(-3f)), ProjectileID.RocketSkeleton, damage, 0f, Main.myPlayer);
                        }

                        Main.PlaySound(SoundID.Item11, npc.Center);
                    }
                }

                if (!masoBool[1] && npc.ai[3] >= 0f) //spawn 4 more limbs
                {
                    npc.ai[3]++;
                    if (npc.ai[3] >= 180f)
                    {
                        masoBool[1] = true;
                        npc.ai[3] = -1f;
                        npc.netUpdate = true;
                        if (Main.netMode != 1)
                        {
                            foreach (NPC l in Main.npc.Where(l => l.active && l.ai[1] == npc.whoAmI))
                            {
                                switch (l.type) //my first four limbs become super mode
                                {
                                    case NPCID.PrimeCannon:
                                    case NPCID.PrimeLaser:
                                    case NPCID.PrimeSaw:
                                    case NPCID.PrimeVice:
                                        l.GetGlobalNPC<EModeGlobalNPC>().masoBool[1] = true;
                                        l.GetGlobalNPC<EModeGlobalNPC>().NetUpdateMaso(l.whoAmI);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            //now spawn the other four
                            int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeLaser, npc.whoAmI, -1f, npc.whoAmI, 0f, 150f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                            n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeSaw, npc.whoAmI, -1f, npc.whoAmI, 0f, 0f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                            n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeCannon, npc.whoAmI, 1f, npc.whoAmI, 0f, 150f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                            n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PrimeVice, npc.whoAmI, 1f, npc.whoAmI, 0f, 0f, npc.target);
                            if (n != 200 && Main.netMode == 2)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                        Main.PlaySound(15, (int)npc.position.X, (int)npc.position.Y, 0);
                    }
                }
            }
        }

        public bool PrimeLimbAI(NPC npc)
        {
            if (npc.type == NPCID.PrimeSaw)
            {
                if (!masoBool[1] && ++Counter2 >= 2) //flamethrower in same direction that saw is pointing
                {
                    Counter2 = 0;
                    Vector2 velocity = new Vector2(7f, 0f).RotatedBy(npc.rotation + Math.PI / 2.0);
                    if (Main.netMode != 1)
                        Projectile.NewProjectile(npc.Center, velocity, ProjectileID.FlamesTrap, npc.damage / 4, 0f, Main.myPlayer);
                    Main.PlaySound(SoundID.Item34, npc.Center);
                }
            }
            else if (npc.type == NPCID.PrimeCannon)
            {
                if (npc.ai[2] != 0f)
                {
                    if (npc.localAI[0] > 30f)
                    {
                        npc.localAI[0] = 0f;
                        if (Main.netMode != 1)
                        {
                            Vector2 speed = new Vector2(16f, 0f).RotatedBy(npc.rotation + Math.PI / 2);
                            Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<DarkStar>(), npc.damage / 5, 0f, Main.myPlayer);
                        }
                    }
                }
                else
                {
                    npc.localAI[0]++;
                    npc.ai[3]++;
                }
            }

            //all limbs
            if (!masoBool[0])
            {
                RegenTimer = 2;
                if (Main.npc[(int)npc.ai[1]].type == NPCID.SkeletronPrime && Main.npc[(int)npc.ai[1]].ai[0] == 2f)
                {
                    masoBool[0] = true;
                    npc.defDamage = npc.defDamage * 4 / 3;
                    npc.damage = npc.defDamage;
                    int heal = npc.lifeMax - npc.life;
                    npc.life = npc.lifeMax;
                    if (heal > 0)
                        CombatText.NewText(npc.Hitbox, CombatText.HealLife, heal);
                    npc.netUpdate = true;
                }
            }
            else
            {
                npc.dontTakeDamage = !(Fargowiltas.Instance.CalamityLoaded && Revengeance);
                int ai1 = (int)npc.ai[1];
                if (!(ai1 > -1 && ai1 < 200 && Main.npc[ai1].active && Main.npc[ai1].type == NPCID.SkeletronPrime))
                {
                    npc.StrikeNPCNoInteraction(9999, 0f, 0); //die if prime gone
                    return false;
                }
                npc.target = Main.npc[ai1].target;
                if (Main.npc[ai1].ai[1] == 3 || !npc.HasValidTarget) //return to normal AI
                    return true;
                if (masoBool[1]) //swipe AI
                {
                    if (!masoBool[3])
                    {
                        masoBool[3] = true;
                        switch (npc.type)
                        {
                            case NPCID.PrimeCannon: Counter = -1; Counter2 = -1; break;
                            case NPCID.PrimeLaser: Counter = 1; Counter2 = -1; break;
                            case NPCID.PrimeSaw: Counter = -1; Counter2 = 1; break;
                            case NPCID.PrimeVice: Counter = 1; Counter2 = 1; break;
                            default: break;
                        }
                    }
                    if (++npc.ai[2] < 180)
                    {
                        Vector2 target = Main.player[npc.target].Center;
                        target.X += 400 * Counter;
                        target.Y += 400 * Counter2;
                        npc.velocity = (target - npc.Center) / 30;
                        if (npc.ai[2] == 140)
                            for (int i = 0; i < 20; i++)
                            {
                                int d = Dust.NewDust(npc.position, npc.width, npc.height, 112, npc.velocity.X * .4f, npc.velocity.Y * .4f, 0, Color.White, 2);
                                Main.dust[d].scale += 1f;
                                Main.dust[d].velocity *= 3f;
                                Main.dust[d].noGravity = true;
                            }
                    }
                    else if (npc.ai[2] == 180)
                    {
                        npc.velocity = npc.DirectionTo(Main.player[npc.target].Center) * 20;
                        npc.netUpdate = true;
                        Counter *= -1;
                        Counter2 *= -1;
                    }
                    else if (npc.ai[2] < 240)
                    {
                        npc.velocity *= 0.996f;
                    }
                    else
                    {
                        npc.ai[2] = 0;
                        npc.netUpdate = true;
                    }
                    npc.rotation = Main.npc[ai1].DirectionTo(npc.Center).ToRotation() - (float)Math.PI / 2;
                    if (npc.netUpdate)
                    {
                        if (Main.netMode == 2)
                        {
                            NetMessage.SendData(23, -1, -1, null, npc.whoAmI);
                            var netMessage = mod.GetPacket();
                            netMessage.Write((byte)13);
                            netMessage.Write((byte)npc.whoAmI);
                            netMessage.Write(Counter);
                            netMessage.Write(Counter2);
                            netMessage.Send();
                        }
                        npc.netUpdate = false;
                    }
                    return false;
                }
                else if (Main.npc[ai1].ai[1] == 1 || Main.npc[ai1].ai[1] == 2) //other limbs while prime spinning
                {
                    int d = Dust.NewDust(npc.position, npc.width, npc.height, 112, npc.velocity.X * .4f, npc.velocity.Y * .4f, 0, Color.White, 2);
                    Main.dust[d].noGravity = true;
                    if (!masoBool[2]) //AND STRETCH HIS ARMS OUT JUST FOR YOU
                    {
                        int rotation = 0;
                        switch (npc.type)
                        {
                            case NPCID.PrimeCannon: rotation = 0; break;
                            case NPCID.PrimeLaser: rotation = 1; break;
                            case NPCID.PrimeSaw: rotation = 2; break;
                            case NPCID.PrimeVice: rotation = 3; break;
                            default: break;
                        }
                        Vector2 offset = Main.player[Main.npc[ai1].target].Center - Main.npc[ai1].Center;
                        offset = offset.RotatedBy(Math.PI / 2 * rotation + Math.PI / 4);
                        offset = Vector2.Normalize(offset) * (offset.Length() + 200);
                        if (offset.Length() < 600)
                            offset = Vector2.Normalize(offset) * 600;
                        Vector2 target = Main.npc[ai1].Center + offset;

                        npc.velocity = (target - npc.Center) / 20;

                        if (++Counter > 60)
                        {
                            masoBool[2] = true;
                            Counter = (int)Main.npc[ai1].Distance(npc.Center);
                            if (Counter < 300)
                                Counter = 300;
                            npc.localAI[3] = Main.npc[ai1].DirectionTo(npc.Center).ToRotation();

                            if (Main.netMode == 2) //MP sync
                            {
                                var netMessage = mod.GetPacket();
                                netMessage.Write((byte)12);
                                netMessage.Write((byte)npc.whoAmI);
                                netMessage.Write(masoBool[2]);
                                netMessage.Write(Counter);
                                netMessage.Write(npc.localAI[3]);
                                netMessage.Send();
                            }
                        }
                    }
                    else //spinning
                    {
                        float range = Counter; //extend further to hit player if beyond current range
                        if (Main.npc[ai1].HasValidTarget && Main.npc[ai1].Distance(Main.player[Main.npc[ai1].target].Center) > range)
                            range = Main.npc[ai1].Distance(Main.player[Main.npc[ai1].target].Center);

                        npc.Center = Main.npc[ai1].Center + new Vector2(range, 0f).RotatedBy(npc.localAI[3]);
                        const float rotation = 0.1f;
                        npc.localAI[3] += rotation;
                        if (npc.localAI[3] > (float)Math.PI)
                        {
                            npc.localAI[3] -= 2f * (float)Math.PI;
                            npc.netUpdate = true;
                        }
                    }
                    npc.rotation = Main.npc[ai1].DirectionTo(npc.Center).ToRotation() - (float)Math.PI / 2;
                    return false;
                }

                if (masoBool[2])
                {
                    masoBool[2] = false;
                    Counter = 0;
                    if (Main.netMode == 2) //MP sync
                    {
                        var netMessage = mod.GetPacket();
                        netMessage.Write((byte)12);
                        netMessage.Write((byte)npc.whoAmI);
                        netMessage.Write(masoBool[2]);
                        netMessage.Write(Counter);
                        netMessage.Write(npc.localAI[3]);
                        netMessage.Send();
                    }
                }
            }

            return true;
        }

        public void PlanteraAI(NPC npc)
        {
            if (!masoBool[0]) //spawn protective crystal ring once
            {
                masoBool[0] = true;
                if (Main.netMode != 1)
                {
                    const int max = 5;
                    const float distance = 125f;
                    float rotation = 2f * (float)Math.PI / max;
                    for (int i = 0; i < max; i++)
                    {
                        Vector2 spawnPos = npc.Center + new Vector2(distance, 0f).RotatedBy(rotation * i);
                        int n = NPC.NewNPC((int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<CrystalLeaf>(), 0, npc.whoAmI, distance, 0, rotation * i);
                        if (Main.netMode == 2 && n < 200)
                            NetMessage.SendData(23, -1, -1, null, n);
                    }
                }
            }
            if (!NPC.downedPlantBoss && Main.netMode != 1 && npc.HasPlayerTarget && !masoBool[1]
                && Collision.CanHit(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
            {
                masoBool[1] = true;
                Item.NewItem(npc.Hitbox, ModContent.ItemType<PlanterasFruit>());
            }

            if (npc.life <= npc.lifeMax / 2) //phase 2
            {
                //Aura(npc, 700, ModContent.BuffType<IvyVenom>(), true, 188);
                masoBool[1] = true;
                npc.defense += 21;

                if (!masoBool[2])
                {
                    masoBool[2] = true;
                    if (Main.netMode != 1)
                    {
                        const int max = 10;
                        const float distance = 250;
                        float rotation = 2f * (float)Math.PI / max;
                        for (int i = 0; i < max; i++)
                        {
                            Vector2 spawnPos = npc.Center + new Vector2(distance, 0f).RotatedBy(rotation * i);
                            int n = NPC.NewNPC((int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<CrystalLeaf>(), 0, npc.whoAmI, distance, 0, rotation * i);
                            if (Main.netMode == 2 && n < 200)
                                NetMessage.SendData(23, -1, -1, null, n);
                        }
                    }
                }

                if (Counter >= 30)
                {
                    Counter = 0;

                    int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                    if (t != -1 && Main.netMode != 1)
                    {
                        int damage = 22;
                        int type = ProjectileID.SeedPlantera;

                        if (Main.rand.Next(2) == 0)
                        {
                            damage = 27;
                            type = ProjectileID.PoisonSeedPlantera;
                        }
                        else if (Main.rand.Next(6) == 0)
                        {
                            damage = 31;
                            type = ProjectileID.ThornBall;
                        }

                        if (!Main.player[t].ZoneJungle)
                            damage = damage * 2;
                        else if (Main.expertMode)
                            damage = damage * 9 / 10;

                        Vector2 velocity = Main.player[t].Center - npc.Center;
                        velocity.Normalize();
                        velocity *= Main.expertMode ? 17f : 15f;

                        int p = Projectile.NewProjectile(npc.Center + velocity * 3f, velocity, type, damage, 0f, Main.myPlayer);
                        if (type != ProjectileID.ThornBall)
                            Main.projectile[p].timeLeft = 300;
                    }
                }

                if (++Timer > 90)
                {
                    Timer = 0;
                    if (Main.netMode != 1)
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<DicerPlantera>(), npc.damage / 5, 0f, Main.myPlayer, 120, 300);
                }

                /*Timer++;
                if (Timer >= 300)
                {
                    Timer = 0;

                    int tentaclesToSpawn = 12;
                    for (int i = 0; i < 200; i++)
                    {
                        if (Main.npc[i].active && Main.npc[i].type == NPCID.PlanterasTentacle && Main.npc[i].ai[3] == 0f)
                        {
                            tentaclesToSpawn--;
                        }
                    }

                    for (int i = 0; i < tentaclesToSpawn; i++)
                    {
                        NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.PlanterasTentacle, npc.whoAmI);
                    }
                }*/

                SharkCount = 0;

                if (npc.HasPlayerTarget)
                {
                    if (Main.player[npc.target].venom)
                    {
                        npc.defense *= 2;
                        Counter++;
                        SharkCount = 1;

                        if (RegenTimer > 120)
                            RegenTimer = 120;
                    }
                }
                else
                {
                    npc.position -= npc.velocity * 0.1f;
                }

                //dont regen above half, this avoids exiting phase 2 by healing
                if (RegenTimer <= 2 && npc.life + 1 + npc.lifeMax / 25 >= npc.lifeMax / 2)
                {
                    npc.life = npc.lifeMax / 2;
                    npc.lifeRegen = 0;
                    RegenTimer = 2;
                }
            }
        }

        public void PlanterasHookAI(NPC npc)
        {
            npc.damage = 0;
            npc.defDamage = 0;

            /*if (NPC.FindFirstNPC(NPCID.PlanterasHook) == npc.whoAmI)
            {
                npc.color = Color.LightGreen;
                PrintAI(npc);
            }*/

            if (BossIsAlive(ref NPC.plantBoss, NPCID.Plantera) && Main.npc[NPC.plantBoss].life < Main.npc[NPC.plantBoss].lifeMax / 2 && Main.npc[NPC.plantBoss].HasValidTarget)
            {
                if (npc.Distance(Main.player[Main.npc[NPC.plantBoss].target].Center) > 600)
                {
                    Vector2 targetPos = Main.player[Main.npc[NPC.plantBoss].target].Center / 16; //pick a new target pos near player
                    targetPos.X += Main.rand.Next(-25, 26);
                    targetPos.Y += Main.rand.Next(-25, 26);

                    if (WorldGen.SolidTile(Framing.GetTileSafely((int)targetPos.X, (int)targetPos.Y)) //check the tile can be grappled
                        || Framing.GetTileSafely((int)targetPos.X, (int)targetPos.Y).wall > 0)
                    {
                        npc.localAI[0] = 600; //reset vanilla timer for picking new block
                        if (Main.netMode != 1)
                            npc.netUpdate = true;

                        npc.ai[0] = targetPos.X;
                        npc.ai[1] = targetPos.Y;
                    }
                }

                npc.position += npc.velocity;
            }
        }

        public void GolemAI(NPC npc)
        {
            /*if (npc.ai[0] == 0f && npc.velocity.Y == 0f) //manipulating golem jump ai
                        {
                            if (npc.ai[1] > 0f)
                            {
                                npc.ai[1] += 5f; //count up to initiate jump faster
                            }
                            else
                            {
                                float threshold = -2f - (float)Math.Round(18f * npc.life / npc.lifeMax);

                                if (npc.ai[1] < threshold) //jump activates at npc.ai[1] == -1
                                    npc.ai[1] = threshold;
                            }
                        }*/

            if (Main.player[Main.myPlayer].active && Main.player[Main.myPlayer].Distance(npc.Center) < 2000)
                Main.player[Main.myPlayer].AddBuff(ModContent.BuffType<LowGround>(), 2);

            if (!masoBool[3])
            {
                npc.position.X += npc.velocity.X / 2f;
                if (npc.velocity.Y < 0)
                {
                    npc.position.Y += npc.velocity.Y * 0.5f;
                    if (npc.velocity.Y > -2)
                        npc.velocity.Y = 20;
                }
            }

            if (masoBool[0])
            {
                if (npc.velocity.Y == 0f)
                {
                    masoBool[0] = false;
                    masoBool[3] = Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16] != null &&
                        Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16].wall == WallID.LihzahrdBrickUnsafe;

                    if (Main.netMode != 1) //landing attacks
                    {
                        Vector2 spawnPos = new Vector2(npc.position.X, npc.Center.Y);
                        if (masoBool[3]) //in temple
                        {
                            Counter++;
                            if (Counter == 1) //plant geysers
                            {
                                spawnPos.X -= npc.width * 7;
                                for (int i = 0; i < 6; i++)
                                {
                                    int tilePosX = (int)spawnPos.X / 16 + npc.width * i * 3 / 16;
                                    int tilePosY = (int)spawnPos.Y / 16;// + 1;

                                    /*if (Main.tile[tilePosX, tilePosY] == null)
                                        Main.tile[tilePosX, tilePosY] = new Tile();

                                    while (!(Main.tile[tilePosX, tilePosY].nactive() && Main.tileSolid[(int)Main.tile[tilePosX, tilePosY].type]))
                                    {
                                        tilePosY++;
                                        if (Main.tile[tilePosX, tilePosY] == null)
                                            Main.tile[tilePosX, tilePosY] = new Tile();
                                    }*/

                                    Projectile.NewProjectile(tilePosX * 16 + 8, tilePosY * 16 + 8, 0f, 0f, ModContent.ProjectileType<GolemGeyser>(), npc.damage / 5, 0f, Main.myPlayer, npc.whoAmI);
                                }
                            }
                            else if (Counter == 2) //rocks fall
                            {
                                Counter = 0;
                                if (npc.HasPlayerTarget)
                                {
                                    for (int i = -2; i <= 2; i++)
                                    {
                                        int tilePosX = (int)Main.player[npc.target].Center.X / 16;
                                        int tilePosY = (int)Main.player[npc.target].Center.Y / 16;// + 1;
                                        tilePosX += 4 * i;

                                        if (Main.tile[tilePosX, tilePosY] == null)
                                            Main.tile[tilePosX, tilePosY] = new Tile();

                                        while (!(Main.tile[tilePosX, tilePosY].nactive() && Main.tileSolid[Main.tile[tilePosX, tilePosY].type]))
                                        {
                                            tilePosY--;
                                            if (Main.tile[tilePosX, tilePosY] == null)
                                                Main.tile[tilePosX, tilePosY] = new Tile();
                                        }

                                        Vector2 spawn = new Vector2(tilePosX * 16 + 8, tilePosY * 16 + 8);
                                        Projectile.NewProjectile(spawn, Vector2.Zero, ModContent.ProjectileType<GolemBoulder>(), npc.damage / 5, 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }
                        else //outside temple
                        {
                            spawnPos.X -= npc.width * 7;
                            for (int i = 0; i < 6; i++)
                            {
                                int tilePosX = (int)spawnPos.X / 16 + npc.width * i * 3 / 16;
                                int tilePosY = (int)spawnPos.Y / 16;// + 1;

                                if (Main.tile[tilePosX, tilePosY] == null)
                                    Main.tile[tilePosX, tilePosY] = new Tile();

                                while (!(Main.tile[tilePosX, tilePosY].nactive() && Main.tileSolid[(int)Main.tile[tilePosX, tilePosY].type]))
                                {
                                    tilePosY++;
                                    if (Main.tile[tilePosX, tilePosY] == null)
                                        Main.tile[tilePosX, tilePosY] = new Tile();
                                }

                                if (npc.HasPlayerTarget && Main.player[npc.target].position.Y > tilePosY * 16)
                                {
                                    Projectile.NewProjectile(tilePosX * 16 + 8, tilePosY * 16 + 8, 6.3f, 6.3f,
                                        ProjectileID.FlamesTrap, npc.damage / 5, 0f, Main.myPlayer);
                                    Projectile.NewProjectile(tilePosX * 16 + 8, tilePosY * 16 + 8, -6.3f, 6.3f,
                                        ProjectileID.FlamesTrap, npc.damage / 5, 0f, Main.myPlayer);
                                }

                                Projectile.NewProjectile(tilePosX * 16 + 8, tilePosY * 16 + 8, 0f, -8f, ProjectileID.GeyserTrap, npc.damage / 5, 0f, Main.myPlayer);

                                Projectile.NewProjectile(tilePosX * 16 + 8, tilePosY * 16 + 8 - 640, 0f, -8f, ProjectileID.GeyserTrap, npc.damage / 5, 0f, Main.myPlayer);
                                Projectile.NewProjectile(tilePosX * 16 + 8, tilePosY * 16 + 8 - 640, 0f, 8f, ProjectileID.GeyserTrap, npc.damage / 5, 0f, Main.myPlayer);
                            }
                            if (npc.HasPlayerTarget)
                            {
                                for (int i = -2; i <= 2; i++)
                                {
                                    int tilePosX = (int)Main.player[npc.target].Center.X / 16;
                                    int tilePosY = (int)Main.player[npc.target].Center.Y / 16;// + 1;
                                    tilePosX += 4 * i;

                                    if (Main.tile[tilePosX, tilePosY] == null)
                                        Main.tile[tilePosX, tilePosY] = new Tile();

                                    for (int j = 0; j < 30; j++)
                                    {
                                        if (Main.tile[tilePosX, tilePosY].nactive() && Main.tileSolid[Main.tile[tilePosX, tilePosY].type])
                                            break;
                                        tilePosY--;
                                        if (Main.tile[tilePosX, tilePosY] == null)
                                            Main.tile[tilePosX, tilePosY] = new Tile();
                                    }

                                    Vector2 spawn = new Vector2(tilePosX * 16 + 8, tilePosY * 16 + 8);
                                    Projectile.NewProjectile(spawn, Vector2.Zero, ModContent.ProjectileType<GolemBoulder>(), npc.damage / 5, 0f, Main.myPlayer);
                                }
                            }
                        }

                        //golem's anti-air fireball spray (whenever he lands while player is below)
                        /*if (npc.HasPlayerTarget && Main.player[npc.target].position.Y > npc.position.Y + npc.height)
                        {
                            float gravity = 0.2f; //shoot down
                            const float time = 60f;
                            Vector2 distance = Main.player[npc.target].Center - npc.Center;
                            distance += Main.player[npc.target].velocity * 45f;
                            distance.X = distance.X / time;
                            distance.Y = distance.Y / time - 0.5f * gravity * time;
                            if (Math.Sign(distance.Y) != Math.Sign(gravity))
                                distance.Y = 0f; //cannot arc shots to hit someone on the same elevation
                            int max = masoBool[3] ? 1 : 3;
                            for (int i = -max; i <= max; i++)
                            {
                                Projectile.NewProjectile(npc.Center.X, npc.Center.Y, distance.X + i * 1.5f, distance.Y,
                                    ModContent.ProjectileType<GolemFireball>(), npc.damage / 5, 0f, Main.myPlayer, gravity, 0f);
                            }
                        }*/
                    }
                }
            }
            else if (npc.velocity.Y > 0)
            {
                masoBool[0] = true;
            }

            if (++Counter2 >= 540 && npc.velocity.Y == 0) //spray spiky balls, only when on ground
            {
                Counter2 = 0;
                if (Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16] != null && //in temple
                    Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16].wall == WallID.LihzahrdBrickUnsafe)
                {
                    for (int i = 0; i < 8; i++)
                        Projectile.NewProjectile(npc.position.X + Main.rand.Next(npc.width), npc.position.Y + Main.rand.Next(npc.height),
                            Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-10, -6), ModContent.ProjectileType<GolemSpikyBall>(), npc.damage / 5, 0f, Main.myPlayer);
                }
                else //outside temple
                {
                    for (int i = 0; i < 16; i++)
                        Projectile.NewProjectile(npc.position.X + Main.rand.Next(npc.width), npc.position.Y + Main.rand.Next(npc.height),
                            Main.rand.NextFloat(-1f, 1f), Main.rand.Next(-20, -9), ModContent.ProjectileType<GolemSpikyBall>(), npc.damage / 4, 0f, Main.myPlayer);
                }
            }

            /*Counter2++;
            if (Counter2 > 240) //golem's anti-air fireball spray (when player is above)
            {
                Counter2 = 0;
                if (npc.HasPlayerTarget && Main.player[npc.target].position.Y < npc.position.Y
                    && Main.netMode != 1) //shoutouts to arterius
                {
                    bool inTemple = Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16] != null && //in temple
                        Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16].wall == WallID.LihzahrdBrickUnsafe;

                    float gravity = -0.2f; //normally floats up
                    //if (Main.player[npc.target].position.Y > npc.position.Y + npc.height) gravity *= -1f; //aim down if player below golem
                    const float time = 60f;
                    Vector2 distance = Main.player[npc.target].Center - npc.Center;
                    distance += Main.player[npc.target].velocity * 45f;
                    distance.X = distance.X / time;
                    distance.Y = distance.Y / time - 0.5f * gravity * time;
                    if (Math.Sign(distance.Y) != Math.Sign(gravity))
                        distance.Y = 0f; //cannot arc shots to hit someone on the same elevation
                    int max = inTemple ? 1 : 3;
                    for (int i = -max; i <= max; i++)
                    {
                        Projectile.NewProjectile(npc.Center.X, npc.Center.Y, distance.X + i, distance.Y,
                            ModContent.ProjectileType<GolemFireball>(), npc.damage / 5, 0f, Main.myPlayer, gravity, 0f);
                    }
                }
            }*/

            if (Fargowiltas.Instance.CalamityLoaded && Revengeance)
            {
                if (masoBool[1])
                    npc.dontTakeDamage = false;
                else
                    masoBool[1] = !NPC.AnyNPCs(NPCID.GolemHead);
            }

            if (!npc.dontTakeDamage)
            {
                npc.life += 4; //healing stuff
                if (npc.life > npc.lifeMax)
                    npc.life = npc.lifeMax;
                Timer++;
                if (Timer >= 75)
                {
                    Timer = Main.rand.Next(30);
                    CombatText.NewText(npc.Hitbox, CombatText.HealLife, 240);
                }
            }
        }

        public void GolemFistAI(NPC npc)
        {
            if (npc.buffType[0] != 0)
            {
                npc.buffImmune[npc.buffType[0]] = true;
                npc.DelBuff(0);
            }

            if (npc.ai[0] == 0f && masoBool[0] && Framing.GetTileSafely(npc.Center).wall != WallID.LihzahrdBrickUnsafe)
            {
                masoBool[0] = false;
                if (Main.netMode != 1)
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<FuseBomb>(), npc.damage / 4, 0f, Main.myPlayer);
            }
            masoBool[0] = npc.ai[0] != 0f;

            if (npc.velocity.Length() > 14)
                npc.position -= Vector2.Normalize(npc.velocity) * (npc.velocity.Length() - 14);

            if (npc.life < npc.lifeMax / 2 && NPC.golemBoss > -1 && NPC.golemBoss < 200 && Main.npc[NPC.golemBoss].active && Main.npc[NPC.golemBoss].type == NPCID.Golem)
            {
                npc.life = npc.lifeMax; //fully heal when below half health and golem still alive
                Timer = 75; //immediately display heal
            }
            npc.life += 167;
            if (npc.life > npc.lifeMax)
                npc.life = npc.lifeMax;
            Timer++;
            if (Timer >= 75)
            {
                Timer = Main.rand.Next(30);
                CombatText.NewText(npc.Hitbox, CombatText.HealLife, 9999);
            }
        }

        public bool GolemHeadAI(NPC npc)
        {
            if (npc.type == NPCID.GolemHead)
            {
                npc.dontTakeDamage = false;
                npc.life += 4;
                if (npc.life > npc.lifeMax)
                    npc.life = npc.lifeMax;

                Timer++;
                if (Timer >= 75)
                {
                    Timer = Main.rand.Next(30);
                    CombatText.NewText(npc.Hitbox, CombatText.HealLife, 240);
                }
            }
            //detatched head
            else
            {
                if (!masoBool[0]) //default mode
                {
                    npc.position += npc.velocity * 0.25f;
                    npc.position.Y += npc.velocity.Y * 0.25f;

                    if (!npc.noTileCollide && npc.HasPlayerTarget && Collision.SolidCollision(npc.position, npc.width, npc.height)) //unstick from walls
                        npc.position += npc.DirectionTo(Main.player[npc.target].Center) * 4;

                    if (++Counter > 540)
                    {
                        Counter = 0;
                        Counter2 = 0;
                        masoBool[0] = true;
                        masoBool[2] = Framing.GetTileSafely(npc.Center).wall == WallID.LihzahrdBrickUnsafe;
                        npc.netUpdate = true;
                        if (Main.netMode == 2)
                            NetUpdateMaso(npc.whoAmI);
                    }
                }
                else //deathray time
                {
                    if (!(NPC.golemBoss > -1 && NPC.golemBoss < 200 && Main.npc[NPC.golemBoss].active && Main.npc[NPC.golemBoss].type == NPCID.Golem))
                    {
                        npc.StrikeNPCNoInteraction(9999, 0f, 0); //die if golem is dead
                        return false;
                    }

                    npc.noTileCollide = true;

                    const int fireTime = 120;
                    if (++Counter < fireTime) //move to above golem
                    {
                        Vector2 target = Main.npc[NPC.golemBoss].Center;
                        target.Y -= 250;
                        if (target.Y > Counter2) //counter2 stores lowest remembered golem position
                            Counter2 = (int)target.Y;
                        target.Y = Counter2;
                        if (npc.HasPlayerTarget && Main.player[npc.target].position.Y < target.Y)
                            target.Y = Main.player[npc.target].position.Y;
                        /*if (masoBool[2]) //in temple
                        {
                            target.Y -= 250;
                            if (target.Y > Counter2) //counter2 stores lowest remembered golem position
                                Counter2 = (int)target.Y;
                            target.Y = Counter2;
                        }
                        else if (npc.HasPlayerTarget)
                        {
                            target.Y = Main.player[npc.target].Center.Y - 250;
                        }*/
                        npc.velocity = (target - npc.Center) / 30;
                    }
                    else if (Counter == fireTime) //fire deathray
                    {
                        npc.velocity = Vector2.Zero;
                        if (npc.HasPlayerTarget) //stores if player is on head's left at this moment
                            masoBool[1] = Main.player[npc.target].Center.X < npc.Center.X;
                        npc.netUpdate = true;
                        if (Main.netMode != 1)
                            Projectile.NewProjectile(npc.Center, Vector2.UnitY, ModContent.ProjectileType<PhantasmalDeathrayGolem>(), npc.damage / 4, 0f, Main.myPlayer, 0f, npc.whoAmI);
                    }
                    else if (Counter < fireTime + 150)
                    {
                        npc.velocity.X += masoBool[1] ? -.18f : .18f;

                        Tile tile = Framing.GetTileSafely(npc.Center); //stop if reached a wall, but only 1sec after started firing
                        if (Counter > fireTime + 60 & tile.nactive() && tile.type == TileID.LihzahrdBrick && tile.wall == WallID.LihzahrdBrickUnsafe)
                        {
                            npc.velocity = Vector2.Zero;
                            npc.netUpdate = true;
                            Counter = 0;
                            Counter2 = 0;
                            masoBool[0] = false;
                        }
                    }
                    else
                    {
                        npc.velocity = Vector2.Zero;
                        npc.netUpdate = true;
                        Counter = 0;
                        Counter2 = 0;
                        masoBool[0] = false;
                    }

                    if (!masoBool[0] && Main.netMode != 1) //spray overhead lasers after dash
                    {
                        bool inTemple = Framing.GetTileSafely(npc.Center).wall == WallID.LihzahrdBrickUnsafe;
                        int max = inTemple ? 6 : 10;
                        int speed = inTemple ? -6 : -11;
                        for (int i = -max; i <= max; i++)
                        {
                            int p = Projectile.NewProjectile(npc.Center, speed * Vector2.UnitY.RotatedBy(Math.PI / 2 / max * i),
                                ModContent.ProjectileType<EyeBeam2>(), npc.damage / 5, 0f, Main.myPlayer);
                            if (p != Main.maxProjectiles)
                                Main.projectile[p].timeLeft = 1200;
                        }
                    }

                    if (npc.netUpdate)
                    {
                        npc.netUpdate = false;
                        if (Main.netMode == 2)
                            NetUpdateMaso(npc.whoAmI);
                    }
                    return false;
                }
            }

            return true;
        }

        public void DukeFishronAI(NPC npc)
        {
            fishBoss = npc.whoAmI;
            if (masoBool[3]) //fishron EX
            {
                if (npc.Distance(Main.player[Main.myPlayer].Center) < 1800f)
                    Main.player[Main.myPlayer].AddBuff(ModContent.BuffType<OceanicSeal>(), 2);
                fishBossEX = npc.whoAmI;
                npc.position += npc.velocity * 0.5f;
                switch ((int)npc.ai[0])
                {
                    case -1: //just spawned
                        if (npc.ai[2] == 2 && Main.netMode != 1) //create spell circle
                        {
                            int ritual1 = Projectile.NewProjectile(npc.Center, Vector2.Zero,
                                ModContent.ProjectileType<FishronRitual>(), 0, 0f, Main.myPlayer, npc.lifeMax, npc.whoAmI);
                            if (ritual1 == 1000) //failed to spawn projectile, abort spawn
                                npc.active = false;
                            Main.PlaySound(SoundID.Item84, npc.Center);
                        }
                        masoBool[2] = true;
                        break;

                    case 0: //phase 1
                        if (!masoBool[1])
                            npc.dontTakeDamage = false;
                        masoBool[2] = false;
                        npc.ai[2]++;
                        break;

                    case 1: //p1 dash
                        Counter++;
                        if (Counter > 5)
                        {
                            Counter = 0;
                            if (Main.netMode != 1)
                            {
                                int n = NPC.NewNPC((int)npc.position.X + Main.rand.Next(npc.width), (int)npc.position.Y + Main.rand.Next(npc.height), NPCID.DetonatingBubble);
                                if (n != 200 && Main.netMode == 2)
                                    NetMessage.SendData(23, -1, -1, null, n);
                                n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubbleEX>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.DirectionTo(Main.player[npc.target].Center);
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                            }
                        }
                        break;

                    case 2: //p1 bubbles
                        if (npc.ai[2] == 0f && Main.netMode != 1)
                            Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer, 1f, npc.target + 1);
                        break;

                    case 3: //p1 drop nados
                        if (npc.ai[2] == 60f && Main.netMode != 1)
                        {
                            const int max = 32;
                            float rotation = 2f * (float)Math.PI / max;
                            for (int i = 0; i < max; i++)
                            {
                                int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubbleEX>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = Vector2.UnitY.RotatedBy(rotation * i);
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                            }

                            SpawnRazorbladeRing(npc, 18, 10f, npc.damage / 6, 1f);
                        }
                        break;

                    case 4: //phase 2 transition
                        masoBool[1] = false;
                        masoBool[2] = true;
                        if (npc.ai[2] == 1 && Main.netMode != 1)
                            Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<FishronRitual>(), 0, 0f, Main.myPlayer, npc.lifeMax / 4, npc.whoAmI);
                        if (npc.ai[2] >= 114)
                        {
                            Counter++;
                            if (Counter > 6) //display healing effect
                            {
                                Counter = 0;
                                int heal = (int)(npc.lifeMax * Main.rand.NextFloat(0.1f, 0.12f));
                                npc.life += heal;
                                int max = npc.ai[0] == 9 && !Fargowiltas.Instance.MasomodeEXLoaded ? npc.lifeMax / 2 : npc.lifeMax;
                                if (npc.life > max)
                                    npc.life = max;
                                CombatText.NewText(npc.Hitbox, CombatText.HealLife, heal);
                            }
                        }
                        break;

                    case 5: //phase 2
                        if (!masoBool[1])
                            npc.dontTakeDamage = false;
                        masoBool[2] = false;
                        npc.ai[2]++;
                        break;

                    case 6: //p2 dash
                        goto case 1;

                    case 7: //p2 spin & bubbles
                        npc.position -= npc.velocity * 0.5f;
                        Counter++;
                        if (Counter > 1)
                        {
                            Counter = 0;
                            if (Main.netMode != 1)
                            {
                                int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubbleEX>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.velocity.RotatedBy(Math.PI / 2);
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                                n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubbleEX>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.velocity.RotatedBy(-Math.PI / 2);
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                            }
                        }
                        break;

                    case 8: //p2 cthulhunado
                        if (Main.netMode != 1 && npc.ai[2] == 60)
                        {
                            Vector2 spawnPos = Vector2.UnitX * npc.direction;
                            spawnPos = spawnPos.RotatedBy(npc.rotation);
                            spawnPos *= npc.width + 20f;
                            spawnPos /= 2f;
                            spawnPos += npc.Center;
                            Projectile.NewProjectile(spawnPos.X, spawnPos.Y, npc.direction * 2f, 8f, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer);
                            Projectile.NewProjectile(spawnPos.X, spawnPos.Y, npc.direction * -2f, 8f, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer);
                            Projectile.NewProjectile(spawnPos.X, spawnPos.Y, 0f, 2f, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer);

                            SpawnRazorbladeRing(npc, 12, 12.5f, npc.damage / 6, 0.75f);
                            SpawnRazorbladeRing(npc, 12, 10f, npc.damage / 6, -2f);
                        }
                        break;

                    case 9: //phase 3 transition
                        if (npc.ai[2] == 1f)
                        {
                            for (int i = 0; i < npc.buffImmune.Length; i++)
                                npc.buffImmune[i] = true;
                            while (npc.buffTime[0] != 0)
                                npc.DelBuff(0);
                            npc.defDamage = (int)(npc.defDamage * 1.2f);
                        }
                        goto case 4;

                    case 10: //phase 3
                             //vanilla fishron has x1.1 damage in p3. p2 has x1.2 damage...
                             //npc.damage = (int)(npc.defDamage * 1.2f * (Main.expertMode ? 0.6f * Main.damageMultiplier : 1f));
                        masoBool[2] = false;
                        Timer++;
                        //if (Timer >= 60 + (int)(540.0 * npc.life / npc.lifeMax)) //yes that needs to be a double
                        if (Timer >= 900)
                        {
                            Timer = 0;
                            if (Main.netMode != 1) //spawn cthulhunado
                                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer, 1f, npc.target + 1);
                        }
                        break;

                    case 11: //p3 dash
                        Counter++;
                        if (Counter > 1)
                        {
                            Counter = 0;
                            if (Main.netMode != 1)
                            {
                                int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubbleEX>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.velocity.RotatedBy(Math.PI / 2);
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                                n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubbleEX>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.velocity.RotatedBy(-Math.PI / 2);
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                            }
                        }
                        goto case 10;

                    case 12: //p3 *teleports behind you*
                        if (npc.ai[2] == 15f)
                        {
                            if (Main.netMode != 1)
                            {
                                SpawnRazorbladeRing(npc, 5, 9f, npc.damage / 6, 1f, true);
                                SpawnRazorbladeRing(npc, 5, 9f, npc.damage / 6, -0.5f, true);
                            }
                        }
                        else if (npc.ai[2] == 16f)
                        {
                            if (Main.netMode != 1)
                            {
                                Vector2 spawnPos = Vector2.UnitX * npc.direction; //GODLUL
                                spawnPos = spawnPos.RotatedBy(npc.rotation);
                                spawnPos *= npc.width + 20f;
                                spawnPos /= 2f;
                                spawnPos += npc.Center;
                                Projectile.NewProjectile(spawnPos.X, spawnPos.Y, npc.direction * 2f, 8f, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer);
                                Projectile.NewProjectile(spawnPos.X, spawnPos.Y, npc.direction * -2f, 8f, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer);

                                const int max = 24;
                                float rotation = 2f * (float)Math.PI / max;
                                for (int i = 0; i < max; i++)
                                {
                                    int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubbleEX>());
                                    if (n < 200)
                                    {
                                        Main.npc[n].velocity = npc.velocity.RotatedBy(rotation * i);
                                        Main.npc[n].velocity.Normalize();
                                        Main.npc[n].netUpdate = true;
                                        if (Main.netMode == 2)
                                            NetMessage.SendData(23, -1, -1, null, n);
                                    }
                                }
                            }
                        }
                        goto case 10;

                    default:
                        break;
                }
            }
            else //fishron regular
            {
                npc.position += npc.velocity * 0.25f;
                switch ((int)npc.ai[0])
                {
                    case -1: //just spawned
                             /*if (npc.ai[2] == 1 && Main.netMode != 1) //create spell circle
                             {
                                 int p2 = Projectile.NewProjectile(npc.Center, Vector2.Zero,
                                     ModContent.ProjectileType<FishronRitual2>(), 0, 0f, Main.myPlayer, 0f, npc.whoAmI);
                                 if (p2 == 1000) //failed to spawn projectile, abort spawn
                                     npc.active = false;
                             }*/
                        npc.dontTakeDamage = true;
                        break;

                    case 0: //phase 1
                        if (!masoBool[1])
                            npc.dontTakeDamage = false;
                        if (!Main.player[npc.target].ZoneBeach)
                            npc.ai[2]++;
                        break;

                    case 1: //p1 dash
                        Counter++;
                        if (Counter > 5)
                        {
                            Counter = 0;

                            if (Main.netMode != 1)
                            {
                                int n = NPC.NewNPC((int)npc.position.X + Main.rand.Next(npc.width), (int)npc.position.Y + Main.rand.Next(npc.height), NPCID.DetonatingBubble);
                                if (n != 200 && Main.netMode == 2)
                                    NetMessage.SendData(23, -1, -1, null, n);
                            }
                        }
                        break;

                    case 2: //p1 bubbles
                        break;

                    case 3: //p1 drop nados
                        if (npc.ai[2] == 60f && Main.netMode != 1)
                        {
                            SpawnRazorbladeRing(npc, 12, 10f, npc.damage / 4, 1f);
                        }
                        break;

                    case 4: //phase 2 transition
                        npc.dontTakeDamage = true;
                        masoBool[1] = false;
                        if (npc.ai[2] == 120)
                        {
                            int heal = npc.lifeMax - npc.life;
                            npc.life = npc.lifeMax;
                            CombatText.NewText(npc.Hitbox, CombatText.HealLife, heal);
                        }
                        break;

                    case 5: //phase 2
                        if (!masoBool[1])
                            npc.dontTakeDamage = false;
                        if (!Main.player[npc.target].ZoneBeach)
                            npc.ai[2]++;
                        break;

                    case 6: //p2 dash
                        goto case 1;

                    case 7: //p2 spin & bubbles
                        npc.position -= npc.velocity * 0.25f;
                        Counter++;
                        if (Counter > 1)
                        {
                            Counter = 0;
                            if (Main.netMode != 1)
                            {
                                int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubble>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.velocity.RotatedBy(Math.PI / 2);
                                    Main.npc[n].velocity *= -npc.spriteDirection;
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                            }
                        }
                        break;

                    case 8: //p2 cthulhunado
                        if (Main.netMode != 1 && npc.ai[2] == 60)
                        {
                            Vector2 spawnPos = Vector2.UnitX * npc.direction;
                            spawnPos = spawnPos.RotatedBy(npc.rotation);
                            spawnPos *= npc.width + 20f;
                            spawnPos /= 2f;
                            spawnPos += npc.Center;
                            Projectile.NewProjectile(spawnPos.X, spawnPos.Y, 0f, 8f, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer);

                            SpawnRazorbladeRing(npc, 12, 10f, npc.damage / 4, 2f);
                        }
                        break;

                    case 9: //phase 3 transition
                        npc.dontTakeDamage = true;
                        npc.defDefense = 0;
                        npc.defense = 0;
                        masoBool[1] = false;
                        if (npc.ai[2] == 120)
                        {
                            int max = Fargowiltas.Instance.MasomodeEXLoaded ? npc.lifeMax : npc.lifeMax / 2;
                            int heal = max - npc.life;
                            npc.life = max;
                            CombatText.NewText(npc.Hitbox, CombatText.HealLife, heal);
                        }
                        break;

                    case 10: //phase 3
                        Timer++;
                        /*if (Timer >= 600) //spawn cthulhunado
                        {
                            Timer = 0;
                            if (Main.netMode != 1)
                                Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer, 1f, npc.target + 1);
                        }*/
                        if (!Main.player[npc.target].ZoneBeach)
                            npc.ai[2]++;
                        break;

                    case 11: //p3 dash
                        Counter++;
                        if (Counter > 2)
                        {
                            Counter = 0;
                            if (Main.netMode != 1)
                            {
                                int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubble>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.velocity.RotatedBy(Math.PI / 2);
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                                n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<DetonatingBubble>());
                                if (n < 200)
                                {
                                    Main.npc[n].velocity = npc.velocity.RotatedBy(-Math.PI / 2);
                                    Main.npc[n].velocity.Normalize();
                                    Main.npc[n].netUpdate = true;
                                    if (Main.netMode == 2)
                                        NetMessage.SendData(23, -1, -1, null, n);
                                }
                            }
                        }
                        goto case 10;

                    case 12: //p3 *teleports behind you*
                        if (npc.ai[2] == 15f)
                        {
                            SpawnRazorbladeRing(npc, 6, 8f, npc.damage / 4, -0.75f);
                        }
                        /*else if (npc.ai[2] == 16f)
                        {
                            if (Main.netMode != 1)
                            {
                                Vector2 spawnPos = Vector2.UnitX * npc.direction;
                                spawnPos = spawnPos.RotatedBy(npc.rotation);
                                spawnPos *= npc.width + 20f;
                                spawnPos /= 2f;
                                spawnPos += npc.Center;
                                Projectile.NewProjectile(spawnPos.X, spawnPos.Y, 0f, 8f, ProjectileID.SharknadoBolt, 0, 0f, Main.myPlayer);
                            }
                        }*/
                        goto case 10;

                    default:
                        break;
                }
            }
        }

        public void CultistAI(NPC npc)
        {
            cultBoss = npc.whoAmI;

            if (!masoBool[0])
            {
                masoBool[0] = true;
                if (Main.netMode != 1 && !NPC.downedAncientCultist)
                    Item.NewItem(npc.Hitbox, ModContent.ItemType<LunaticSigil>());
            }

            Timer++;
            if (Timer >= 1200)
            {
                Timer = 0;

                if (NPC.CountNPCS(NPCID.AncientCultistSquidhead) < 4 && Main.netMode != 1)
                {
                    int n = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCID.AncientCultistSquidhead, 0, 0f, 0f, 0f, 0f, npc.target);
                    if (n != 200 && Main.netMode == 2)
                        NetMessage.SendData(23, -1, -1, null, n);
                }
            }

            if (npc.ai[3] == -1f)
            {
                if (npc.ai[1] >= 120f && npc.ai[1] < 419f) //skip summoning ritual LMAO
                {
                    npc.ai[1] = 419f;
                    npc.netUpdate = true;
                }
            }
            else
            {
                int damage = (int)(75 * (1 + FargoSoulsWorld.CultistCount * .0125)); //necessary because calameme
                switch ((int)npc.ai[0])
                {
                    case -1:
                        if (npc.ai[1] == 419f)
                        {
                            npc.ai[0] = 0f;
                            npc.ai[1] = 0f;
                            npc.ai[3] = 11f;
                            npc.netUpdate = true;
                        }
                        break;

                    case 2:
                        if (npc.ai[1] == 3f && Main.netMode != 1) //ice mist, frost wave support
                        {
                            int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                            if (t != -1 && Main.player[t].active)
                            {
                                for (int i = 0; i < 200; i++)
                                {
                                    if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBossClone)
                                    {
                                        Vector2 distance = Main.player[t].Center - Main.npc[i].Center;
                                        distance.Normalize();
                                        distance = distance.RotatedByRandom(Math.PI / 12);
                                        distance *= Main.rand.NextFloat(6f, 9f);
                                        Projectile.NewProjectile(Main.npc[i].Center, distance,
                                            ProjectileID.FrostWave, damage / 3, 0f, Main.myPlayer);
                                    }
                                }
                            }
                        }
                        break;

                    case 3:
                        if (npc.ai[1] == 3f && Main.netMode != 1) //fireballs, solar goop support
                        {
                            for (int i = 0; i < 200; i++)
                            {
                                if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBossClone)
                                {
                                    int n = NPC.NewNPC((int)Main.npc[i].Center.X, (int)Main.npc[i].Center.Y, NPCID.SolarGoop);
                                    if (n < 200)
                                    {
                                        Main.npc[n].velocity.X = Main.rand.Next(-10, 11);
                                        Main.npc[n].velocity.Y = Main.rand.Next(-15, -4);
                                        if (Main.netMode == 2)
                                            NetMessage.SendData(23, -1, -1, null, n);
                                    }
                                }
                            }
                        }
                        break;

                    case 4:
                        if (npc.ai[1] == 19f && npc.HasPlayerTarget && Main.netMode != 1) //lightning orb, lightning support
                        {
                            for (int i = 0; i < 200; i++)
                            {
                                if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBossClone)
                                {
                                    Vector2 dir = Main.player[npc.target].Center - Main.npc[i].Center;
                                    float ai1New = Main.rand.Next(100);
                                    Vector2 vel = Vector2.Normalize(dir.RotatedByRandom(Math.PI / 4)) * 7f;
                                    Projectile.NewProjectile(Main.npc[i].Center, vel, ProjectileID.CultistBossLightningOrbArc,
                                        damage / 15 * 6, 0, Main.myPlayer, dir.ToRotation(), ai1New);
                                }
                            }
                        }
                        break;

                    case 7:
                        if (npc.ai[1] == 3f && Main.netMode != 1) //ancient light, phantasmal eye support
                        {
                            for (int i = 0; i < 200; i++)
                            {
                                if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBossClone)
                                {
                                    Projectile.NewProjectile(Main.npc[i].Center, Main.npc[i].DirectionTo(Main.player[npc.target].Center) * 10f, ProjectileID.StardustJellyfishSmall, damage / 3, 0f, Main.myPlayer);
                                    /*Vector2 speed = Vector2.UnitX.RotatedByRandom(Math.PI);
                                    speed *= 6f;
                                    Projectile.NewProjectile(Main.npc[i].Center, speed,
                                        ProjectileID.PhantasmalEye, damage / 3, 0f, Main.myPlayer);
                                    Projectile.NewProjectile(Main.npc[i].Center, speed.RotatedBy(Math.PI * 2 / 3),
                                        ProjectileID.PhantasmalEye, damage / 3, 0f, Main.myPlayer);
                                    Projectile.NewProjectile(Main.npc[i].Center, speed.RotatedBy(-Math.PI * 2 / 3),
                                        ProjectileID.PhantasmalEye, damage / 3, 0f, Main.myPlayer);*/
                                }
                            }
                        }
                        break;

                    case 8:
                        if (npc.ai[1] == 3f) //ancient doom, nebula sphere support
                        {
                            int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                            if (t != -1 && Main.player[t].active)
                            {
                                for (int i = 0; i < 200; i++)
                                {
                                    if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBossClone)
                                        Projectile.NewProjectile(Main.npc[i].Center, Vector2.Zero,
                                            ProjectileID.NebulaSphere, damage / 15 * 6, 0f, Main.myPlayer);
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public void AncientLightAI(NPC npc)
        {
            npc.dontTakeDamage = true;
            npc.immortal = true;
            npc.chaseable = false;
            if (npc.buffType[0] != 0)
                npc.DelBuff(0);
            if (masoBool[0])
            {
                if (npc.HasPlayerTarget)
                {
                    Vector2 speed = Main.player[npc.target].Center - npc.Center;
                    speed.Normalize();
                    speed *= 9f;

                    npc.ai[2] += speed.X / 100f;
                    if (npc.ai[2] > 9f)
                        npc.ai[2] = 9f;
                    if (npc.ai[2] < -9f)
                        npc.ai[2] = -9f;
                    npc.ai[3] += speed.Y / 100f;
                    if (npc.ai[3] > 9f)
                        npc.ai[3] = 9f;
                    if (npc.ai[3] < -9f)
                        npc.ai[3] = -9f;
                }
                else
                {
                    npc.TargetClosest(false);
                }

                Counter++;
                if (Counter > 240)
                {
                    npc.HitEffect(0, 9999);
                    npc.active = false;
                }

                npc.velocity.X = npc.ai[2];
                npc.velocity.Y = npc.ai[3];
            }
        }

        public void MoonLordCoreAI(NPC npc)
        {
            moonBoss = npc.whoAmI;
            RegenTimer = 2;
            //npc.defense = masoStateML >= 0 && masoStateML <= 3 ? 0 : npc.defDefense;

            if (!masoBool[3])
            {
                masoBool[3] = true;
                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<LunarRitual>(),
                          (int)(100.0 * (1.0 + FargoSoulsWorld.MoonlordCount * .0125)), 0f, Main.myPlayer, 0f, npc.whoAmI);
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ModContent.ProjectileType<FragmentRitual>(), 0, 0f, Main.myPlayer, 0f, npc.whoAmI);
                }
            }

            if (!npc.dontTakeDamage && Main.netMode != 1)
                Counter++; //phases transition twice as fast when core is exposed

            if (Main.player[Main.myPlayer].active && !Main.player[Main.myPlayer].dead && masoStateML >= 0 && masoStateML <= 3)
                Main.player[Main.myPlayer].AddBuff(ModContent.BuffType<NullificationCurse>(), 2);

            npc.position -= npc.velocity / 3; //SLOW DOWN

            /*if (!masoBool[0])
            {
                masoBool[0] = npc.life < npc.lifeMax / 2; //remembers even if core goes above 50% hp
                if (masoBool[0]) //roar
                {
                    Main.PlaySound(15, Main.player[Main.myPlayer].Center, 0);
                    npc.netUpdate = true;
                }
            }*/

            if (!npc.dontTakeDamage) //only when vulnerable
            {
                if (!masoBool[0])
                {
                    masoBool[0] = true;
                    Main.PlaySound(15, Main.player[Main.myPlayer].Center, 0);
                    npc.netUpdate = true;
                }

                Timer++;
                if (Timer >= 240)
                {
                    Timer = 0;
                    npc.netUpdate = true;

                    if (Main.netMode != 1)
                    {
                        switch (masoStateML)
                        {
                            case 0: //melee
                                for (int i = 0; i < 3; i++)
                                {
                                    NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                    if (bodyPart.active)
                                    {
                                        int damage = (int)(25 * (1 + FargoSoulsWorld.MoonlordCount * .0125));
                                        for (int j = -3; j <= 3; j++)
                                        {
                                            Projectile.NewProjectile(bodyPart.Center,
                                                6f * bodyPart.DirectionFrom(Main.player[npc.target].Center).RotatedBy(Math.PI / 2 / 3 * j),
                                                ProjectileID.CultistBossFireBall, damage, 0f, Main.myPlayer);
                                        }
                                    }
                                }
                                break;
                            case 1: //ranged
                                    /*for (int i = 0; i < 12; i++) //spawn lightning
                                    {
                                        Point tileCoordinates = Main.player[npc.target].Top.ToTileCoordinates();

                                        if (Main.rand.Next(2) == 0)
                                        {
                                            tileCoordinates.X += Main.rand.Next(-40, 41);
                                            tileCoordinates.Y += Main.rand.Next(30, 41) * (Main.rand.Next(2) == 0 ? 1 : -1);
                                        }
                                        else
                                        {
                                            tileCoordinates.X += Main.rand.Next(30, 41) * (Main.rand.Next(2) == 0 ? 1 : -1);
                                            tileCoordinates.Y += Main.rand.Next(-40, 41);
                                        }

                                        for (int index = 0; index < 10 && !WorldGen.SolidTile(tileCoordinates.X, tileCoordinates.Y) && tileCoordinates.Y > 10; ++index)
                                            tileCoordinates.Y -= 1;

                                        Projectile.NewProjectile(tileCoordinates.X * 16 + 8, tileCoordinates.Y * 16 + 17, 0f, 0f, 578, 0, 1f, Main.myPlayer);
                                    }*/
                                for (int i = 0; i < 3; i++) //shoot lightning bolt
                                {
                                    NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                    if (bodyPart.active &&
                                        ((i == 2 && bodyPart.type == NPCID.MoonLordHead) ||
                                        bodyPart.type == NPCID.MoonLordHand))
                                    {
                                        Vector2 spawnOffset = bodyPart.Center - Main.player[npc.target].Center;
                                        for (int j = -1; j <= 1; j++)
                                        {
                                            Projectile.NewProjectile(Main.player[npc.target].Center + spawnOffset.RotatedBy(MathHelper.ToRadians(10) * j),
                                                Vector2.Zero, 578, 0, 1f, Main.myPlayer);
                                        }

                                        /*Vector2 dir = Main.player[npc.target].Center - bodyPart.Center;
                                        float ai1New = Main.rand.Next(100);
                                        Vector2 vel = Vector2.Normalize(dir.RotatedByRandom(Math.PI / 4)) * 6f;
                                        int damage = (int)(30 * (1 + FargoSoulsWorld.MoonlordCount * .0125));
                                        Projectile.NewProjectile(bodyPart.Center, vel, ProjectileID.CultistBossLightningOrbArc,
                                            damage, 0, Main.myPlayer, dir.ToRotation(), ai1New);*/
                                    }
                                }
                                //Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileID.CultistBossLightningOrb, (int)(30 * (1 + FargoSoulsWorld.MoonlordCount * .0125)), 0f, Main.myPlayer);
                                break;
                            case 2: //magic
                                for (int i = 0; i < 3; i++)
                                {
                                    NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                    if (bodyPart.active &&
                                        ((i == 2 && bodyPart.type == NPCID.MoonLordHead) ||
                                        bodyPart.type == NPCID.MoonLordHand))
                                    {
                                        int damage = (int)(35 * (1 + FargoSoulsWorld.MoonlordCount * .0125));
                                        const int max = 8;
                                        for (int j = 0; j < max; j++)
                                        {
                                            Projectile.NewProjectile(bodyPart.Center,
                                                2.5f * bodyPart.DirectionFrom(Main.player[npc.target].Center).RotatedBy(Math.PI * 2 / max * j),
                                                ProjectileID.NebulaLaser, damage, 0f, Main.myPlayer);
                                        }

                                        /*Vector2 distance = Main.player[npc.target].Center - bodyPart.Center;
                                        distance.Normalize();
                                        distance *= 7f;
                                        int damage = (int)(35 * (1 + FargoSoulsWorld.MoonlordCount * .0125));
                                        for (int j = -1; j <= 1; j += 2) //aim above and below player
                                        {
                                            Vector2 speed = distance.RotatedBy(Math.PI / 5 * j);
                                            for (int k = -1; k <= 1; k++) //fire a 5-spread each
                                            {
                                                Projectile.NewProjectile(bodyPart.Center, speed.RotatedBy(Math.PI / 32 * k),
                                                    ProjectileID.NebulaLaser, damage, 0f, Main.myPlayer);
                                            }
                                        }*/
                                    }
                                }
                                break;
                            case 3: //summoner
                                for (int i = 0; i < 3; i++)
                                {
                                    NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                    if (bodyPart.active &&
                                        ((i == 2 && bodyPart.type == NPCID.MoonLordHead) ||
                                        bodyPart.type == NPCID.MoonLordHand))
                                    {
                                        Vector2 speed = Main.player[npc.target].Center - bodyPart.Center;
                                        speed.Normalize();
                                        speed *= 6f;
                                        for (int j = -1; j <= 1; j++)
                                        {
                                            Vector2 vel = speed.RotatedBy(MathHelper.ToRadians(10) * j);
                                            int n = NPC.NewNPC((int)bodyPart.Center.X, (int)bodyPart.Center.Y, NPCID.AncientLight, 0, 0f, (Main.rand.NextFloat() - 0.5f) * 0.3f * 6.28318548202515f / 60f, vel.X, vel.Y);
                                            if (n < 200)
                                            {
                                                Main.npc[n].velocity = vel;
                                                Main.npc[n].netUpdate = true;
                                                if (Main.netMode == 2)
                                                    NetMessage.SendData(23, -1, -1, null, n);
                                            }
                                        }
                                    }
                                }
                                break;
                            default: //phantasmal eye rings
                                if (Main.netMode != 1)
                                {
                                    const int max = 3;
                                    const int speed = 9;
                                    const float rotationModifier = 0.5f;
                                    int damage = (int)(40 * (1 + FargoSoulsWorld.MoonlordCount * .0125));
                                    float rotation = 2f * (float)Math.PI / max;
                                    Vector2 vel = Vector2.UnitY * speed;
                                    int type = ModContent.ProjectileType<MutantSphereRing>();
                                    for (int i = 0; i < max; i++)
                                    {
                                        vel = vel.RotatedBy(rotation);
                                        Projectile.NewProjectile(npc.Center, vel, type, damage, 0f, Main.myPlayer, rotationModifier, speed);
                                        Projectile.NewProjectile(npc.Center, vel, type, damage, 0f, Main.myPlayer, -rotationModifier, speed);
                                    }
                                    Main.PlaySound(SoundID.Item84, npc.Center);
                                }
                                break;
                        }
                    }

                    /*if (--Counter2 < 0)
                    {
                        Counter2 = 600;
                        Main.PlaySound(15, (int)npc.Center.X, (int)npc.Center.Y, 0);
                        if (Main.netMode != 1 && npc.HasPlayerTarget)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                NPC bodyPart = Main.npc[(int)npc.localAI[i]];
                                if (bodyPart.active)
                                {
                                    bodyPart.localAI[0] = (Main.player[npc.target].Center - bodyPart.Center).ToRotation();
                                    Vector2 speed = Vector2.UnitX.RotatedBy(bodyPart.localAI[0]);
                                    Projectile.NewProjectile(bodyPart.Center, speed, ModContent.ProjectileType<PhantasmalDeathrayMLSmall>(), 0, 0f, Main.myPlayer, 0f, bodyPart.whoAmI);
                                }
                            }
                        }
                    }
                    else if (Counter2 == 540)
                    {
                        if (Main.netMode != 1)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                NPC bodyPart = Main.npc[(int)npc.localAI[i]];
                                if (bodyPart.active)
                                {
                                    Vector2 speed = Vector2.UnitX.RotatedBy(bodyPart.localAI[0]);
                                    int damage = (int)(75 * (1 + FargoSoulsWorld.MoonlordCount * .0125));
                                    Projectile.NewProjectile(bodyPart.Center, speed, ModContent.ProjectileType<PhantasmalDeathrayML>(), damage, 0f, Main.myPlayer, 0f, bodyPart.whoAmI);
                                }
                            }
                        }
                    }*/
                }
            }

            if (npc.ai[0] == 2f) //moon lord is dead
            {
                if (!masoBool[1]) //check once when dead
                {
                    masoBool[1] = true;
                    //stop all attacks (and become intangible lol) after i die
                    if (Main.netMode != 1 && NPC.CountNPCS(NPCID.MoonLordCore) == 1)
                    {
                        masoStateML = 4;
                        if (Main.netMode == 2) //sync damage phase with clients
                        {
                            var netMessage = mod.GetPacket();
                            netMessage.Write((byte)4);
                            netMessage.Write((byte)npc.whoAmI);
                            netMessage.Write(Counter);
                            netMessage.Write(masoStateML);
                            netMessage.Send();
                        }
                    }
                }
                Counter = 0;
                Counter2 = 600;
                Timer = 0;
            }
            else //moon lord isn't dead
            {
                if (++Counter > 1800)
                {
                    Counter = 0;
                    if (Main.netMode != 1)
                    {
                        if (++masoStateML > 4)
                            masoStateML = 0;
                        if (Main.netMode == 2) //sync damage phase with clients
                        {
                            var netMessage = mod.GetPacket();
                            netMessage.Write((byte)4);
                            netMessage.Write((byte)npc.whoAmI);
                            netMessage.Write(Counter);
                            netMessage.Write(masoStateML);
                            netMessage.Send();
                        }
                    }
                }
            }

            switch (masoStateML)
            {
                case 0: Main.monolithType = 3; break;
                case 1: Main.monolithType = 0; break;
                case 2: Main.monolithType = 1; break;
                case 3: Main.monolithType = 2; break;
                default: break;
            }
        }

        public void MoonLordSocketAI(NPC npc)
        {
            RegenTimer = 2;
            //npc.defense = masoStateML >= 0 && masoStateML <= 3 ? 0 : npc.defDefense;

            if (npc.ai[0] == -2f) //eye socket is empty
            {
                if (npc.ai[1] == 0f //happens every 32 ticks
                    && Main.npc[(int)npc.ai[3]].ai[0] != 2f //will stop when ML dies
                    && Main.npc[(int)npc.ai[3]].GetGlobalNPC<EModeGlobalNPC>().masoBool[0]) //only during p3
                {
                    Timer++;
                    if (Timer >= 29) //warning dust, reset timer
                    {
                        bool fireLaser = true;
                        /*for (int i = 0; i < Main.maxNPCs; i++) //find this ML's true eye (they're synced, so any is fine)
                            if (Main.npc[i].active && Main.npc[i].type == NPCID.MoonLordFreeEye && Main.npc[i].ai[3] == npc.ai[3])
                            {
                                if (Main.npc[i].ai[0] == 4 && Main.npc[i].ai[1] > 800) //if free eyes are firing deathray, delay own ray
                                {
                                    fireLaser = false;
                                    Timer = 27;
                                }
                                break;
                            }*/
                        if (fireLaser)
                        {
                            Timer = 0;
                            int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                            if (t != -1)
                            {
                                npc.localAI[0] = (Main.player[t].Center - npc.Center).ToRotation();
                                /*Vector2 offset = Vector2.UnitX.RotatedBy(npc.localAI[0]) * 10f;
                                for (int i = 0; i < 300; i++) //dust warning line for laser
                                {
                                    int d = Dust.NewDust(npc.Center + offset * i, 1, 1, 111, 0f, 0f, 0, default(Color), 1.5f);
                                    Main.dust[d].noGravity = true;
                                    Main.dust[d].velocity *= 0.5f;
                                }*/
                                if (Main.netMode != 1)
                                    Projectile.NewProjectile(npc.Center, Vector2.UnitX.RotatedBy(npc.localAI[0]), ModContent.ProjectileType<PhantasmalDeathrayMLSmall>(), 0, 0f, Main.myPlayer, 0, npc.whoAmI);
                            }
                        }
                    }
                    if (Timer == 3) //FIRE LASER
                    {
                        int t = npc.HasPlayerTarget ? npc.target : npc.FindClosestPlayer();
                        if (t != -1)
                        {
                            if (Main.netMode != 1)
                            {
                                float newRotation = (Main.player[t].Center - npc.Center).ToRotation();
                                float difference = newRotation - npc.localAI[0];
                                const float PI = (float)Math.PI;
                                float rotationDirection = PI / 4f / 120f; //positive is CW, negative is CCW
                                if (difference < -PI)
                                    difference += 2f * PI;
                                if (difference > PI)
                                    difference -= 2f * PI;
                                if (difference < 0f)
                                    rotationDirection *= -1f;
                                Vector2 speed = Vector2.UnitX.RotatedBy(npc.localAI[0]);
                                int damage = (int)(60 * (1 + FargoSoulsWorld.MoonlordCount * .0125));
                                Projectile.NewProjectile(npc.Center, speed, ModContent.ProjectileType<PhantasmalDeathrayML>(), damage, 0f, Main.myPlayer, rotationDirection, npc.whoAmI);
                            }
                        }
                    }
                    npc.netUpdate = true;
                }
            }
        }
    }
}
