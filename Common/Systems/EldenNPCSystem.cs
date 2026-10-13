using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaModIgrulkiTV.Common.Systems
{
	/// <summary>
	/// Система расстановки ванильных NPC по фиксированным точкам карты (как в Elden Ring / D&D).
	/// NPC создаются в конкретных тематических локациях и привязываются к своим точкам.
	/// </summary>
	public class EldenNPCSystem : ModSystem
	{
		// Флаг, были ли уже расставлены ключевые сюжетные NPC
		public static bool InitialNPCsPlaced = false;

		/// <summary>
		/// Определение фиксированных точек спавна NPC на карте.
		/// Координаты указываются в тайлах (Tile X, Tile Y).
		/// </summary>
		public struct NPCPositionConfig
		{
			public int NpcType;
			public int TileX;
			public int TileY;
			public string Description;

			public NPCPositionConfig(int type, int x, int y, string desc)
			{
				NpcType = type;
				TileX = x;
				TileY = y;
				Description = desc;
			}
		}

		public static List<NPCPositionConfig> FixedSpawnPoints = new()
		{
			// Примеры фиксированных локаций (координаты легко настраиваются под вашу карту):
			// 1. Гид (Аналог Белоликого Варрэ у стартового шатра)
			new NPCPositionConfig(NPCID.Guide, 200, 300, "First Step / Шатер первого шага"),

			// 2. Торговец (Аналог торговца Кале в Церкви Эллех)
			new NPCPositionConfig(NPCID.Merchant, 250, 305, "Church of Elleh / Церковь Эллех"),

			// 3. Медсестра (Аналог Фии / Служанки в Круглом Столе)
			new NPCPositionConfig(NPCID.Nurse, 350, 280, "Roundtable Hold / Крепость Круглого Стола"),

			// 4. Оружейник (Аналог кузнеца Хьюга в руинах кузницы)
			new NPCPositionConfig(NPCID.ArmsDealer, 400, 290, "Blacksmith Ruins / Кузница"),

			// 5. Деметр / Трактирщик (Таверна D&D / Башня магов)
			new NPCPositionConfig(NPCID.DD2Bartender, 500, 310, "Wayward Inn / Постоялый двор")
		};

		/// <summary>
		/// Предотвращает естественный спавн городских жителей в построенные дома.
		/// Только зафиксированные на карте NPC будут присутствовать в мире.
		/// </summary>
		public override void PostUpdateTime()
		{
			WorldGen.prioritizedTownNPCType = 0;
			if (Main.townNPCCanSpawn != null)
			{
				for (int i = 0; i < Main.townNPCCanSpawn.Length; i++)
				{
					Main.townNPCCanSpawn[i] = false;
				}
			}
		}

		public override void OnWorldLoad()
		{
			// Проверяем при загрузке мира и гарантируем присутствие жителей на местах
			EnsureFixedNPCsExist();
		}

		public override void PostWorldGen()
		{
			// Вызывается после первичной генерации/загрузки карты
			EnsureFixedNPCsExist();
		}

		/// <summary>
		/// Спавнит отсутствующих NPC в их фиксированных координатах и назначает дом.
		/// </summary>
		public static void EnsureFixedNPCsExist()
		{
			foreach (var config in FixedSpawnPoints)
			{
				// Проверяем, существует ли уже этот NPC на карте
				int existingNpcIndex = NPC.FindFirstNPC(config.NpcType);

				if (existingNpcIndex == -1)
				{
					// Спавним NPC в заданной точке тайла
					int worldX = config.TileX * 16;
					int worldY = config.TileY * 16;

					int npcIndex = NPC.NewNPC(
						Terraria.Entity.GetSource_NaturalSpawn(),
						worldX,
						worldY,
						config.NpcType
					);

					if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
					{
						NPC npc = Main.npc[npcIndex];
						npc.homeTileX = config.TileX;
						npc.homeTileY = config.TileY;
						npc.homeless = false;
						npc.netUpdate = true;
					}
				}
				else
				{
					// Если NPC уже есть, убеждаемся, что его дом зафиксирован
					NPC npc = Main.npc[existingNpcIndex];
					npc.homeTileX = config.TileX;
					npc.homeTileY = config.TileY;
					npc.homeless = false;
				}
			}

			InitialNPCsPlaced = true;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["InitialNPCsPlaced"] = InitialNPCsPlaced;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			InitialNPCsPlaced = tag.GetBool("InitialNPCsPlaced");
		}
	}
}
