using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaModIgrulkiTV.Common.Systems;
using TerrariaModIgrulkiTV.Content.Buffs;

namespace TerrariaModIgrulkiTV.Common.Players
{
	/// <summary>
	/// Класс игрока, управляющий механикой шкал Адреналина и Ярости в стиле Calamity Mod.
	/// </summary>
	public class CalamityGaugePlayer : ModPlayer
	{
		public const float MaxAdrenaline = 10000f;
		public const float MaxRage = 10000f;

		public float Adrenaline = 0f;
		public float Rage = 0f;

		private bool adrenalineWasMax = false;
		private bool rageWasMax = false;

		private int rageCombatTimer = 0;

		public override void Initialize()
		{
			Adrenaline = 0f;
			Rage = 0f;
			adrenalineWasMax = false;
			rageWasMax = false;
			rageCombatTimer = 0;
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			// Активация Адреналина
			if (KeybindSystem.AdrenalineHotKey != null && KeybindSystem.AdrenalineHotKey.JustPressed)
			{
				if (Adrenaline >= MaxAdrenaline && !Player.HasBuff<AdrenalineBuff>() && !Player.dead)
				{
					ActivateAdrenaline();
				}
			}

			// Активация Ярости
			if (KeybindSystem.RageHotKey != null && KeybindSystem.RageHotKey.JustPressed)
			{
				if (Rage >= MaxRage && !Player.HasBuff<RageBuff>() && !Player.dead)
				{
					ActivateRage();
				}
			}
		}

		private void ActivateAdrenaline()
		{
			Adrenaline = 0f;
			adrenalineWasMax = false;

			Player.AddBuff(ModContent.BuffType<AdrenalineBuff>(), 300); // 5 секунд
			SoundEngine.PlaySound(SoundID.Item4, Player.Center);

			// Кольцо электрических вспышек вокруг игрока
			for (int i = 0; i < 28; i++)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(7f, 7f);
				Dust d = Dust.NewDustPerfect(Player.Center, DustID.Electric, vel, 100, Color.Cyan, 1.4f);
				d.noGravity = true;
			}

			CombatText.NewText(Player.Hitbox, Color.Cyan, "ADRENALINE!", true);
		}

		private void ActivateRage()
		{
			Rage = 0f;
			rageWasMax = false;

			Player.AddBuff(ModContent.BuffType<RageBuff>(), 300); // 5 секунд
			SoundEngine.PlaySound(SoundID.Roar, Player.Center);

			// Кольцо багрового огня вокруг игрока
			for (int i = 0; i < 28; i++)
			{
				Vector2 vel = Main.rand.NextVector2CircularEdge(7f, 7f);
				Dust d = Dust.NewDustPerfect(Player.Center, DustID.Torch, vel, 100, Color.OrangeRed, 1.7f);
				d.noGravity = true;
			}

			CombatText.NewText(Player.Hitbox, Color.OrangeRed, "RAGE!", true);
		}

		public override void PostUpdateMiscEffects()
		{
			if (Player.dead)
			{
				Adrenaline = 0f;
				Rage = 0f;
				return;
			}

			// 1. Проверка активного боя с боссом
			bool bossAlive = IsBossAliveNearPlayer();

			// Накопление Адреналина: происходит ТОЛЬКО во время боя с боссом
			if (bossAlive && !Player.HasBuff<AdrenalineBuff>())
			{
				// 30 секунд (1800 кадров) на полное заполнение
				Adrenaline = Math.Min(MaxAdrenaline, Adrenaline + (MaxAdrenaline / 1800f));

				if (Adrenaline >= MaxAdrenaline && !adrenalineWasMax)
				{
					adrenalineWasMax = true;
					SoundEngine.PlaySound(SoundID.MaxMana, Player.Center);
					CombatText.NewText(Player.Hitbox, Color.Cyan, "Adrenaline Ready!", false, true);
				}
			}
			else if (!bossAlive && !Player.HasBuff<AdrenalineBuff>())
			{
				// Вне боя с боссом адреналин постепенно спадает
				Adrenaline = Math.Max(0f, Adrenaline - (MaxAdrenaline / 360f));
				if (Adrenaline < MaxAdrenaline)
				{
					adrenalineWasMax = false;
				}
			}

			// 2. Накопление Ярости: происходит при нахождении рядом с врагами или получении урона
			bool enemyNear = IsHostileEnemyNear(650f);

			if (enemyNear)
			{
				rageCombatTimer = 360; // 6 секунд в бою
				if (!Player.HasBuff<RageBuff>())
				{
					Rage = Math.Min(MaxRage, Rage + (MaxRage / 1200f));
				}
			}
			else if (rageCombatTimer > 0)
			{
				rageCombatTimer--;
			}
			else if (!Player.HasBuff<RageBuff>())
			{
				// Вне боя ярость медленно угасает
				Rage = Math.Max(0f, Rage - (MaxRage / 600f));
			}

			if (Rage >= MaxRage && !rageWasMax)
			{
				rageWasMax = true;
				SoundEngine.PlaySound(SoundID.Item74, Player.Center);
				CombatText.NewText(Player.Hitbox, Color.OrangeRed, "Rage Ready!", false, true);
			}
			else if (Rage < MaxRage)
			{
				rageWasMax = false;
			}
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			// Главное правило Calamity: любое получение урона мгновенно сбрасывает Адреналин в ноль!
			if (Adrenaline > 0f)
			{
				Adrenaline = 0f;
				adrenalineWasMax = false;
				CombatText.NewText(Player.Hitbox, Color.DeepSkyBlue, "Adrenaline Lost!", false, true);
			}

			// Получение урона дает ощутимый прирост Ярости
			if (!Player.HasBuff<RageBuff>())
			{
				float rageGain = Math.Max(600f, info.Damage * 70f);
				Rage = Math.Min(MaxRage, Rage + rageGain);
				rageCombatTimer = 360;
			}
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			Adrenaline = 0f;
			Rage = 0f;
			adrenalineWasMax = false;
			rageWasMax = false;
		}

		private bool IsBossAliveNearPlayer()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && (npc.boss || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail))
				{
					if (Vector2.Distance(Player.Center, npc.Center) < 3600f)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsHostileEnemyNear(float maxDist)
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.lifeMax > 5 && npc.type != NPCID.TargetDummy)
				{
					if (Vector2.Distance(Player.Center, npc.Center) < maxDist)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
