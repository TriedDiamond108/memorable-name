using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;

namespace NPCs.Bosses
{

    [AutoloadBossHead]

	public class RogueCrystal : ModNPC
	{
		private int ai;
		private int attackTimer = 0;
		private bool fastSpeed = false;

		private bool stunned;
		private int stunnedTimer;


		private int frame = 0;
		private double counting;


		public override void SetStaticDefaults() 
		{
			DisplayName.SetDefault("Rogue Crystal");
			Main.npcFrameCount[npc.type] = 1;
		}

		public override void SetDefaults() 
		{
			npc.width = 64;
			npc.height = 128;
			
			npc.boss = true;
			npc.aiStyle = -1;
			npc.npcSlots = 5f;
			
			npc.lifeMax = 6450;
			npc.damage = 40;
			npc.defense = 10;
			npc.knockBackResist = 0f;

			npc.value = Item.buyPrice(gold: 50);

			npc.lavaImmune = true;
			npc.noTileCollide = true;
			npc.noGravity = true;

			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			music = MusicID.Boss3;

			bossBag = mod.ItemType("RogueCrystalTreasureBag");
		}


		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax * bossLifeScale);
			npc.damage = (int)(npc.damage * 1.3f);
		}


		public override void AI()
		{
			npc.TargetClosest(true);
			Player player = Main.player[npc.target];
			Vector2 target = npc.HasPlayerTarget ? player.Center : Main.npc[npc.target].Center;
			npc.rotation = 0.0f;
			npc.netAlways = true;
			npc.TargetClosest(true);
			if (npc.life >= npc.lifeMax)
			    npc.life = npc.lifeMax;
			if (npc.target < 0 || npc.target == 255 || player.dead || !player.active)
			{
				 npc.TargetClosest(false);
				 npc.direction = 1;
				 npc.velocity.Y = npc.velocity.Y - 0.1f;
				 if(npc.timeLeft > 20)
				 {
				 	 npc.timeLeft = 20;
					 return;
				 }
			}
			if(stunned)
			{
				npc.velocity.X=0.0f;
				npc.velocity.Y=0.0f;
				stunnedTimer++;
				if(stunnedTimer >= 100)
				{
					stunned = false;
					stunnedTimer = 0;
				}
			}
			ai++;
			npc.ai[0] = (float)ai * 1f;
			int distance = (int)Vector2.Distance(target, npc.Center);
			if((double)npc.ai[0] < 300)
			{
				frame = 0;
				MoveTowards(npc, target, (float)(distance > 300 ? 20f : 13f), 30f);
				npc.netUpdate = true;
			} else if((double)npc.ai[0] >= 300 && (double)npc.ai[0] < 450.0)
			{
				stunned = true;
				frame = 0;
				npc.defense = 60;
				npc.damage = 5;
				MoveTowards(npc, target, (float)(distance > 300 ? 13f :7f), 30f);
				npc.netUpdate = true;
			} else if((double)npc.ai[0] >= 450.0)
			{
			    frame = 0;
				stunned = false;
				npc.damage = (int)(40 * 1.3f);
				npc.defense = 15;
				if (!fastSpeed)
				{
					fastSpeed = true;
				} else
				{
					if((double)npc.ai[0] % 50 == 0)
					{
						float speed = 15f;
						Vector2 vector = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
						float x = player.position.X + (float)(player.width / 2) - vector.X;
						float y = player.position.Y + (float)(player.height / 2) - vector.Y;
						float distance2 = (float)Math.Sqrt(x * x + y *y);
						float factor = speed / distance2;
						npc.velocity.X = x * factor;
						npc.velocity.Y = y * factor;
					}
				}
				npc.netUpdate = true;
			}
			if((double)npc.ai[0] % (Main.expertMode ? 100 : 150) == 0 && !stunned && !fastSpeed)
			{
				attackTimer++;
				if (attackTimer <= 2)
				{
					frame = 0;
					npc.velocity.X = 0f;
					npc.velocity.Y = 0f;
					Vector2 shootPos = npc.Center;
					float accuracy = 5f * (npc.life / npc.lifeMax);
					Vector2 shootVel = target - shootPos + new Vector2(Main.rand.NextFloat(-accuracy, accuracy), Main.rand.NextFloat(-accuracy / accuracy));
					shootVel.Normalize();
					shootVel *= 5f;
					for(int i = 0; i < (Main.expertMode ? 3 : 2); i++)
					{
						Projectile.NewProjectile(shootPos.X + (float)(-100 * npc.direction) + (float)Main.rand.Next(40, -41), shootPos.Y - (float)Main.rand.Next(-50, 40), shootVel.X, shootVel.Y, mod.ProjectileType("LethiferousOrb"), npc.damage / 3, 5f);
					}
				} else
				{
					attackTimer = 0;
				}
			}



			if ((double)npc.ai[0] >= 650.0)
			{
				ai = 0;
				npc.alpha = 0;
				fastSpeed = false;
			}
		}


		public override void FindFrame(int frameHeight)
		{
			if(frame == 0)
			{
				counting += 1.0;
				if(counting < 8.0)
				{
					npc.frame.Y = 0;
				} else if(counting < 16.0)
				{
					npc.frame.Y = frameHeight;
				} else if(counting < 24.0)
				{
					npc.frame.Y = frameHeight * 2;
				} else if(counting < 32.0)
				{
				    npc.frame.Y = frameHeight * 3;
				} else
				{
					counting = 0.0;
				}
			}
			else if (frame == 1)
			{
				npc.frame.Y = frameHeight * 4;
			}
		{
		}
	}


	private void MoveTowards(NPC npc, Vector2 playerTarget, float speed, float turnResistance)
	{
		var move = playerTarget - npc.Center;
		float length = move.Length();
		if(length > speed)
		{
			move *= speed / length;
		}
		move = (npc.velocity * turnResistance + move) / (turnResistance + 1f);
		length = move.Length();
		if(length > speed)
		{
			move *= speed / length;
		}
		npc.velocity=move;
	}


	public override void NPCLoot()
	{
		LethiferousWorld.DownedRogueCrystal = true;
		if(Main.expertMode)
		{
			npc.DropBossBags();
		} else
		{
			Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, ItemID.LifeCrystal, Main.rand.Next(1, 3));
		}
	}


	public override void BossLoot(ref string name, ref int potionType)
	{
	    potionType = ItemID.HealingPotion;
	}	
}
}