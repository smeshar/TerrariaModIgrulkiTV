using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaModIgrulkiTV.Content.Items.Summoning;
using TerrariaModIgrulkiTV.Content.Projectiles.Boss;

namespace TerrariaModIgrulkiTV.Content.NPCs.Bosses.CaveIdol
{
	/// <summary>
	/// Босс «Каменный Идол Подземелья» (Ancient Cave Idol).
	/// Первый босс игры, слабее Короля Слизней.
	/// Изначально стоит в виде неподвижной статуи, пока игрок не нанесет удар.
	/// </summary>
	[AutoloadBossHead]
	public class CaveIdolBoss : ModNPC
	{
		// Состояния AI
		private const int STATE_DORMANT = 0;    // Статуя (неподвижен, урон от касания = 0)
		private const int STATE_AWAKENING = 1;  // Пробуждение (тряска, вспышки, рев)
		private const int STATE_PHASE1 = 2;     // Фаза 1: парение, удары о землю, падающие сталактиты
		private const int STATE_PHASE2 = 3;     // Фаза 2 (<50% HP): ярость аметистового ядра, осколки камня

		private ref float State => ref NPC.ai[0];
		private ref float Timer => ref NPC.ai[1];
		private ref float AttackCounter => ref NPC.ai[2];

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 4;

			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.MPAllowedEnemies[Type] = true;

			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Bleeding] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
		}

		public override void SetDefaults()
		{
			NPC.width = 64;
			NPC.height = 76;

			// Параметры босса: слабее Короля Слизней (у КС 2000 HP и 40 урона)
			NPC.lifeMax = 1350;
			NPC.damage = 0; // В спящем виде урон = 0
			NPC.defense = 6;
			NPC.knockBackResist = 0f;

			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;

			NPC.HitSound = SoundID.NPCHit41;
			NPC.DeathSound = SoundID.NPCDeath43;

			NPC.value = Item.buyPrice(gold: 15);
			Music = MusicID.Boss1;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
				new FlavorTextBestiaryInfoElement("Древний идол, высеченный первыми обитателями подземных пещер. Потревоженный от векового сна, он обрушивает на чужаков каменные своды и мощь подземных глубин.")
			});
		}

		public override void AI()
		{
			// Поиск ближайшей цели
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
			{
				NPC.TargetClosest(true);
			}

			Player target = Main.player[NPC.target];

			switch ((int)State)
			{
				case STATE_DORMANT:
					AI_Dormant();
					break;

				case STATE_AWAKENING:
					AI_Awakening();
					break;

				case STATE_PHASE1:
					AI_Combat(target, isPhase2: false);
					break;

				case STATE_PHASE2:
					AI_Combat(target, isPhase2: true);
					break;
			}
		}

		private void AI_Dormant()
		{
			// Неподвижная статуя
			NPC.velocity = Vector2.Zero;
			NPC.damage = 0; // Не наносит урон при касании

			// Если игрок ударил статую
			if (NPC.justHit || NPC.life < NPC.lifeMax)
			{
				State = STATE_AWAKENING;
				Timer = 0f;
				AttackCounter = 0f;
				NPC.netUpdate = true;

				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
			}

			// Редкие частицы каменной пыли
			if (Main.rand.NextBool(20))
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, 0, 0, 100, default, 0.9f);
			}
		}

		private void AI_Awakening()
		{
			NPC.velocity = Vector2.Zero;
			NPC.damage = 0;
			Timer++;

			// Вибрация и треск пробуждающейся статуи
			NPC.position.X += (float)Math.Sin(Timer * 0.8f) * 2f;

			// Вспышки аметистовых кристаллов из трещин
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.GemAmethyst, Vector2.Zero, 100, default, 1.2f);
				d.noGravity = true;
			}

			Lighting.AddLight(NPC.Center, 0.5f * (Timer / 90f), 0.1f, 0.7f * (Timer / 90f));

			if (Timer >= 90f) // 1.5 секунды
			{
				State = STATE_PHASE1;
				Timer = 0f;
				NPC.damage = 15; // Активация контактного урона
				NPC.netUpdate = true;

				SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);

				// Взрыв каменных обломков
				for (int i = 0; i < 30; i++)
				{
					Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, vel.X, vel.Y, 100, default, 1.5f);
				}
			}
		}

		private void AI_Combat(Player target, bool isPhase2)
		{
			// Если игрок умер или ушел слишком далеко
			if (target.dead || Vector2.Distance(NPC.Center, target.Center) > 4000f)
			{
				NPC.velocity.Y -= 0.3f;
				if (NPC.timeLeft > 60)
				{
					NPC.timeLeft = 60;
				}
				return;
			}

			NPC.damage = isPhase2 ? 20 : 15;
			Lighting.AddLight(NPC.Center, isPhase2 ? 0.7f : 0.4f, 0.1f, isPhase2 ? 0.9f : 0.5f);

			// Переход во 2 фазу при HP < 50%
			if (!isPhase2 && NPC.life < NPC.lifeMax / 2)
			{
				State = STATE_PHASE2;
				Timer = 0f;
				NPC.netUpdate = true;

				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				for (int i = 0; i < 25; i++)
				{
					Dust.NewDustPerfect(NPC.Center, DustID.GemAmethyst, Main.rand.NextVector2Circular(5f, 5f), 100, default, 1.4f).noGravity = true;
				}
			}

			Timer++;
			float slamCooldown = isPhase2 ? 160f : 210f;

			// 1. Поведение парения над игроком
			if (Timer < slamCooldown - 40f)
			{
				Vector2 desiredPos = target.Center + new Vector2(0f, -190f);
				Vector2 move = desiredPos - NPC.Center;
				float speed = isPhase2 ? 5.5f : 4f;

				if (move.Length() > 20f)
				{
					move.Normalize();
					NPC.velocity = Vector2.Lerp(NPC.velocity, move * speed, 0.05f);
				}

				// Во второй фазе выпускает кристаллы в игрока между ударами
				if (isPhase2 && Timer % 55f == 0f && Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 shootDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 6.5f;
					Projectile.NewProjectile(
						NPC.GetSource_FromAI(),
						NPC.Center,
						shootDir,
						ModContent.ProjectileType<StoneShard>(),
						12,
						1f,
						Main.myPlayer
					);
				}
			}
			// 2. Подготовка к сокрушительному удару о землю (зависание в воздухе)
			else if (Timer < slamCooldown)
			{
				NPC.velocity *= 0.85f;

				// Предупреждающие частицы
				if (Main.rand.NextBool(2))
				{
					Dust d = Dust.NewDustPerfect(NPC.Center + new Vector2(0f, 35f), DustID.Stone, new Vector2(0, 3f), 100, default, 1.2f);
					d.noGravity = true;
				}
			}
			// 3. Совершение удара вниз (Slam Down)
			else if (Timer == slamCooldown)
			{
				NPC.velocity = new Vector2(0f, 15f);
				SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
			}
			// 4. Полет вниз до уровня игрока
			else
			{
				// Достигли уровня игрока или пола
				if (NPC.Center.Y >= target.Center.Y + 20f || Timer >= slamCooldown + 35f)
				{
					// Удар о землю!
					SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
					NPC.velocity = Vector2.Zero;
					Timer = 0f;
					NPC.netUpdate = true;

					// Создаем каменную волну пыли по бокам
					for (int i = -1; i <= 1; i += 2)
					{
						for (int j = 0; j < 12; j++)
						{
							Vector2 dustVel = new Vector2(i * Main.rand.NextFloat(3f, 8f), -Main.rand.NextFloat(1f, 4f));
							Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, dustVel.X, dustVel.Y, 100, default, 1.3f);
						}
					}

					// Срываем 2-3 сталактита с потолка пещеры над игроком
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						int stalactiteCount = isPhase2 ? 3 : 2;
						for (int k = 0; k < stalactiteCount; k++)
						{
							float spawnOffsetX = Main.rand.NextFloat(-140f, 140f);
							Vector2 stalactitePos = new Vector2(target.Center.X + spawnOffsetX, target.Center.Y - 280f);
							Vector2 stalactiteVel = new Vector2(0f, 1.5f);

							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								stalactitePos,
								stalactiteVel,
								ModContent.ProjectileType<FallingStalactite>(),
								12,
								1f,
								Main.myPlayer
							);
						}
					}
				}
			}
		}

		public override void FindFrame(int frameHeight)
		{
			// Управление анимацией
			if (State == STATE_DORMANT)
			{
				NPC.frame.Y = 0 * frameHeight; // Кадр 0: Спящая статуя
			}
			else if (State == STATE_AWAKENING)
			{
				NPC.frame.Y = 1 * frameHeight; // Кадр 1: Треснувшая статуя с горящими глазами
			}
			else
			{
				// Кадры 2 и 3: Парящий босс с ожившими каменными кулаками
				NPC.frameCounter++;
				if (NPC.frameCounter > 12)
				{
					NPC.frameCounter = 0;
					NPC.frame.Y = (NPC.frame.Y == 2 * frameHeight) ? 3 * frameHeight : 2 * frameHeight;
				}
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			// 1. Гарантированный дроп золота: от 12 до 20 золотых монет
			npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 12, 20));

			// 2. Гарантированный дроп призывалки самого себя
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AncientCaveIdolSummon>(), 1));

			// Дополнительно: целебные зелья для новичков
			npcLoot.Add(ItemDropRule.Common(ItemID.LesserHealingPotion, 1, 3, 6));
		}

		public override void OnKill()
		{
			// Дублирующая гарантия выпадения золота и призывалки при смерти босса
			Item.NewItem(NPC.GetSource_Loot(), NPC.Hitbox, ItemID.GoldCoin, Main.rand.Next(12, 21));
			Item.NewItem(NPC.GetSource_Loot(), NPC.Hitbox, ModContent.ItemType<AncientCaveIdolSummon>(), 1);
		}
	}
}
