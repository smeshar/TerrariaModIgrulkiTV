using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EldenRingDnD.Common.Globals
{
	/// <summary>
	/// Глобальный модификатор для всех NPC игры:
	/// 1. Отключает ванильное естественное заселение мирных NPC в дома.
	/// 2. Делает мирных NPC (Town NPC) абсолютно неуязвимыми (Souls-like механика).
	/// 3. Удерживает NPC около назначенной позиции (чтобы не уходили в пропасти/леса).
	/// </summary>
	public class EldenTownNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		// Запоминаем исходные координаты, если нужно зафиксировать NPC на посту
		public Vector2? AnchorPosition { get; set; } = null;

		public override void SetDefaults(NPC npc)
		{
			if (npc.townNPC)
			{
				// Полный флаг неуязвимости от Terraria
				npc.dontTakeDamage = true;
				npc.lavaImmune = true;
				npc.trapImmune = true;
				npc.buffImmune[BuffID.OnFire] = true;
				npc.buffImmune[BuffID.Poisoned] = true;
				npc.buffImmune[BuffID.Venom] = true;
				npc.knockBackResist = 0f; // Нельзя оттолкнуть ударом
			}
		}

		/// <summary>
		/// Дополнительная страховка: запрещаем мобам наносить удары по Town NPC.
		/// Возвращаем bool?: false = запрет удара, null = стандартное поведение.
		/// </summary>
		public override bool CanBeHitByNPC(NPC npc, NPC attacker)
		{
			if (npc.townNPC)
			{
				return false;
			}
			return null;
		}

		/// <summary>
		/// Запрещаем снарядам наносить удары по Town NPC.
		/// Возвращаем bool?: false = запрет попадания, null = стандартное поведение.
		/// </summary>
		public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
		{
			if (npc.townNPC)
			{
				return false;
			}
			return null;
		}

		/// <summary>
		/// Обнуление любого входящего урона (если урон каким-то образом просочился).
		/// </summary>
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (npc.townNPC)
			{
				modifiers.SourceDamage *= 0f;
				modifiers.FinalDamage *= 0f;
				modifiers.Knockback *= 0f;
				modifiers.DisableCrit();
			}
		}

		/// <summary>
		/// Защита от смерти: даже при прямом списании HP (скрипты, падения и т.д.) NPC восстанавливает жизнь и не умирает.
		/// </summary>
		public override bool CheckDead(NPC npc)
		{
			if (npc.townNPC)
			{
				npc.life = npc.lifeMax;
				return false; // Отменяем смерть
			}
			return true;
		}

		/// <summary>
		/// Ограничение блуждания: как в Elden Ring, NPC не бродят хаотично по всей карте,
		/// а остаются в радиусе своего лагеря / поста.
		/// </summary>
		public override bool PreAI(NPC npc)
		{
			if (npc.townNPC && npc.active)
			{
				// Если NPC приписан к фиксированному тайлу (homeTileX, homeTileY), не даем ему уходить слишком далеко
				if (npc.homeTileX > 0 && npc.homeTileY > 0)
				{
					Vector2 homeWorldPos = new Vector2(npc.homeTileX * 16, npc.homeTileY * 16);
					float dist = Vector2.Distance(npc.Center, homeWorldPos);

					// Если отошел дальше 250 пикселей (около 15 блоков), разворачиваем и направляем обратно
					if (dist > 250f)
					{
						int directionToHome = npc.Center.X < homeWorldPos.X ? 1 : -1;
						npc.direction = directionToHome;
						npc.velocity.X = directionToHome * 1.0f;
					}
				}
			}

			return base.PreAI(npc);
		}
	}
}
