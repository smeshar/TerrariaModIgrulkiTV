using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;
using TerrariaModIgrulkiTV.Content.NPCs.Bosses.CaveIdol;

namespace TerrariaModIgrulkiTV.Common.Systems
{
	/// <summary>
	/// Система генерации и настройки мира "Междуземье" (The Lands Between).
	/// Перехватывает ванильные этапы генерации мира:
	/// - Отключает несовместимые ванильные структуры (случайные парящие острова, стандартный данж и пирамиды).
	/// - Добавляет кастомный этап генерации тематических регионов Elden Ring (Замогилье, Лиурния, Кэлид, Альтус, Вершины Великанов).
	/// </summary>
	public class WorldSystem : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			// Список ванильных этапов генерации, которые мы отключаем для создания чистого мира в стиле Elden Ring:
			string[] passesToRemove = new string[]
			{
				"Floating Islands",       // Парящие острова (нарушают Souls-атмосферу)
				"Pyramids",               // Египетские пирамиды
				"Living Trees",           // Случайные гигантские деревья (можно расставить вручную)
				"Spider Caves",           // Случайные паучьи гнезда
				"Jungle Temple",          // Стандартный храм джунглей (заменяется на кастомный данж)
				"Dungeon"                 // Ванильный данж Скелетрона (заменяется замком Грозовой Завесы / Штормвейл)
			};

			foreach (string passName in passesToRemove)
			{
				int index = tasks.FindIndex(genPass => genPass.Name.Equals(passName));
				if (index != -1)
				{
					tasks.RemoveAt(index);
				}
			}

			// Вставляем наш собственный этап генерации рельефа Междуземья
			int terrainIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Terrain"));
			if (terrainIndex != -1)
			{
				tasks.Insert(terrainIndex + 1, new PassLegacy("Elden Ring: Lands Between Biomes", GenerateLandsBetweenBiomes));
			}

			// Добавляем этап размещения тематических данжей и замков
			int microBiomesIndex = tasks.FindIndex(genPass => genPass.Name.Equals("Micro Biomes"));
			if (microBiomesIndex != -1)
			{
				tasks.Insert(microBiomesIndex + 1, new PassLegacy("Elden Ring: Dungeons & Castles", GenerateThematicDungeons));
			}
		}

		/// <summary>
		/// Генерация базовой структуры биомов Elden Ring (по горизонтальным зонам карты).
		/// </summary>
		private void GenerateLandsBetweenBiomes(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Формирование Междуземья (Замогилье, Лиурния, Кэлид, Альтус)...";

			int worldWidth = Main.maxTilesX;
			int worldHeight = Main.maxTilesY;

			int liurniaEnd = (int)(worldWidth * 0.30f);
			int limgraveEnd = (int)(worldWidth * 0.70f);

			for (int x = 0; x < worldWidth; x++)
			{
				progress.Set((float)x / worldWidth);

				int surfaceY = (int)Main.worldSurface;

				if (x >= limgraveEnd)
				{
					ApplyCaelidStyle(x, surfaceY);
				}
				else if (x < liurniaEnd)
				{
					ApplyLiurniaStyle(x, surfaceY);
				}
				else
				{
					ApplyLimgraveStyle(x, surfaceY);
				}
			}
		}

		private static void ApplyCaelidStyle(int x, int surfaceY)
		{
			for (int y = surfaceY - 10; y < surfaceY + 40 && y < Main.maxTilesY; y++)
			{
				Tile tile = Main.tile[x, y];
				if (tile.HasTile)
				{
					// Заменяем обычную траву на багрянец / плоть
					if (tile.TileType == TileID.Grass)
					{
						tile.TileType = TileID.CrimsonGrass;
					}
					else if (tile.TileType == TileID.Stone)
					{
						// Местами заменяем камень на плоть или кости
						if (WorldGen.genRand.NextBool(5))
						{
							tile.TileType = TileID.FleshBlock;
						}
					}
				}
			}
		}

		private static void ApplyLiurniaStyle(int x, int surfaceY)
		{
			for (int y = surfaceY - 5; y < surfaceY + 30 && y < Main.maxTilesY; y++)
			{
				Tile tile = Main.tile[x, y];
				if (tile.HasTile && tile.TileType == TileID.Stone)
				{
					// Заменяем камень на мрамор / кристаллы академии Райи Лукарии
					if (WorldGen.genRand.NextBool(4))
					{
						tile.TileType = TileID.Marble;
					}
				}
			}
		}

		private static void ApplyLimgraveStyle(int x, int surfaceY)
		{
			// Традиционный зеленый рельеф с серым замковым кирпичом
		}

		/// <summary>
		/// Генерация тематических мини-данжей Elden Ring (Катакомбы, Руины церквей, Лагеря, Пещерный данж).
		/// </summary>
		private void GenerateThematicDungeons(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Возведение замков, катакомб и мест благодати...";

			// Размещение стартовой церкви (аналог Церкви Эллех) около стартовой точки игрока
			int churchX = Main.spawnTileX + 30;
			int churchY = (int)Main.worldSurface - 2;
			BuildRuinedChurch(churchX, churchY);

			// Размещение пещерного данжа первого босса прямо под точкой спавна игроков
			int caveDungeonX = Main.spawnTileX;
			int caveDungeonY = (int)Main.worldSurface + 65;
			BuildCaveIdolDungeon(caveDungeonX, caveDungeonY);
		}

		/// <summary>
		/// Генератор структуры: Разрушенная церковь (каменный кирпич, костер, платформы).
		/// </summary>
		public static void BuildRuinedChurch(int startX, int startY)
		{
			int width = 24;
			int height = 12;

			for (int x = startX; x < startX + width; x++)
			{
				for (int y = startY; y < startY + height; y++)
				{
					WorldGen.KillTile(x, y);

					if (y == startY + height - 1)
					{
						WorldGen.PlaceTile(x, y, TileID.GrayBrick, mute: true, forced: true);
					}
					else if ((x == startX || x == startX + width - 1) && y > startY + 3)
					{
						WorldGen.PlaceTile(x, y, TileID.GrayBrick, mute: true, forced: true);
					}
					else
					{
						WorldGen.PlaceWall(x, y, WallID.GrayBrick);
					}
				}
			}

			int fireX = startX + width / 2;
			int fireY = startY + height - 2;
			WorldGen.PlaceTile(fireX, fireY, TileID.Campfire, mute: true, forced: true);
		}

		/// <summary>
		/// Генерация пещерного данжа для первого босса («Каменный Идол Подземелья»).
		/// Располагается прямо под стартовой точкой спавна игроков в подземном слое.
		/// Содержит следы древней человеческой стоянки: обветшалые деревянные стены, платформы, потухшие костры, паутину, сундук.
		/// В центре на каменном пьедестале установлена спящая статуя босса.
		/// </summary>
		public static void BuildCaveIdolDungeon(int centerX, int centerY)
		{
			int radiusX = 26;
			int radiusY = 13;

			// 1. Формирование органической пещерной каверны
			for (int x = centerX - radiusX - 3; x <= centerX + radiusX + 3; x++)
			{
				for (int y = centerY - radiusY - 3; y <= centerY + radiusY + 3; y++)
				{
					if (x <= 5 || x >= Main.maxTilesX - 5 || y <= 5 || y >= Main.maxTilesY - 5)
						continue;

					float dx = (x - centerX) / (float)radiusX;
					float dy = (y - centerY) / (float)radiusY;
					float distSq = dx * dx + dy * dy;

					// Внутренняя область пещеры (очистка)
					if (distSq <= 0.88f)
					{
						WorldGen.KillTile(x, y, noItem: true);

						// Задние стены: следы деревянного жилища людей в центре и каменная кладка/пещера
						if (Math.Abs(x - centerX) < 14 && Math.Abs(y - centerY) < 9)
						{
							WorldGen.PlaceWall(x, y, WallID.Wood);
						}
						else
						{
							WorldGen.PlaceWall(x, y, WallID.Cave6Unsafe);
						}
					}
					// Каменная окантовка свода и пола пещеры
					else if (distSq <= 1.08f)
					{
						Tile tile = Main.tile[x, y];
						if (!tile.HasTile)
						{
							WorldGen.PlaceTile(x, y, TileID.Stone, mute: true, forced: true);
						}
					}
				}
			}

			// 2. Декоративные элементы стоянки людей
			int floorY = centerY + radiusY - 2;

			// Разрушенные деревянные балки (опоры)
			for (int bx = centerX - 12; bx <= centerX + 12; bx += 8)
			{
				for (int by = centerY - 5; by <= floorY; by++)
				{
					if (!Main.tile[bx, by].HasTile && WorldGen.genRand.NextBool(3, 4))
					{
						WorldGen.PlaceTile(bx, by, TileID.WoodenBeam, mute: true, forced: true);
					}
				}
			}

			// Разрушенные деревянные платформы
			for (int px = centerX - 18; px <= centerX - 6; px++)
			{
				if (WorldGen.genRand.NextBool(4, 5))
				{
					WorldGen.PlaceTile(px, centerY + 2, TileID.Platforms, mute: true, forced: true, style: 0);
				}
			}
			for (int px = centerX + 6; px <= centerX + 18; px++)
			{
				if (WorldGen.genRand.NextBool(4, 5))
				{
					WorldGen.PlaceTile(px, centerY + 2, TileID.Platforms, mute: true, forced: true, style: 0);
				}
			}

			// Потухшие костры (Campfire, потушенные переключением кадра тайла)
			int fire1X = centerX - 9;
			int fire1Y = floorY - 1;
			WorldGen.PlaceTile(fire1X, fire1Y, TileID.Campfire, mute: true, forced: true);
			if (Main.tile[fire1X, fire1Y].HasTile && Main.tile[fire1X, fire1Y].TileType == TileID.Campfire)
			{
				Main.tile[fire1X, fire1Y].TileFrameX += 18;
				Main.tile[fire1X + 1, fire1Y].TileFrameX += 18;
				Main.tile[fire1X, fire1Y + 1].TileFrameX += 18;
				Main.tile[fire1X + 1, fire1Y + 1].TileFrameX += 18;
			}

			int fire2X = centerX + 10;
			int fire2Y = floorY - 1;
			WorldGen.PlaceTile(fire2X, fire2Y, TileID.Campfire, mute: true, forced: true);
			if (Main.tile[fire2X, fire2Y].HasTile && Main.tile[fire2X, fire2Y].TileType == TileID.Campfire)
			{
				Main.tile[fire2X, fire2Y].TileFrameX += 18;
				Main.tile[fire2X + 1, fire2Y].TileFrameX += 18;
				Main.tile[fire2X, fire2Y + 1].TileFrameX += 18;
				Main.tile[fire2X + 1, fire2Y + 1].TileFrameX += 18;
			}

			// Паутина на сводах пещеры
			for (int wx = centerX - radiusX + 3; wx <= centerX + radiusX - 3; wx++)
			{
				int ceilingY = centerY - radiusY + 3;
				if (!Main.tile[wx, ceilingY].HasTile && WorldGen.genRand.NextBool(3))
				{
					WorldGen.PlaceTile(wx, ceilingY, TileID.Cobweb, mute: true, forced: true);
				}
			}

			// Старый заброшенный верстак
			WorldGen.PlaceTile(centerX - 14, floorY - 1, TileID.WorkBenches, mute: true, forced: true);

			// Заброшенный сундук с базовым подземным лутом
			int chestX = centerX + 14;
			int chestY = floorY - 1;
			int chestIndex = WorldGen.PlaceChest(chestX, chestY, TileID.Containers, style: 1);
			if (chestIndex != -1)
			{
				Chest chest = Main.chest[chestIndex];
				int itemSlot = 0;
				chest.item[itemSlot++].SetDefaults(ItemID.Rope, false);
				chest.item[itemSlot - 1].stack = 40;
				chest.item[itemSlot++].SetDefaults(ItemID.Torch, false);
				chest.item[itemSlot - 1].stack = 25;
				chest.item[itemSlot++].SetDefaults(ItemID.LesserHealingPotion, false);
				chest.item[itemSlot - 1].stack = 5;
				chest.item[itemSlot++].SetDefaults(ItemID.Bomb, false);
				chest.item[itemSlot - 1].stack = 6;
			}

			// 3. Центральный каменный постамент для босса-статуи
			for (int px = centerX - 3; px <= centerX + 3; px++)
			{
				WorldGen.PlaceTile(px, floorY, TileID.GrayBrick, mute: true, forced: true);
				WorldGen.PlaceTile(px, floorY - 1, TileID.GrayBrick, mute: true, forced: true);
			}

			// 4. Размещение спящего босса-статуи в центре пьедестала
			int bossType = ModContent.NPCType<CaveIdolBoss>();
			int bossIndex = NPC.NewNPC(
				new EntitySource_WorldGen(),
				centerX * 16 + 8,
				(floorY - 4) * 16,
				bossType
			);

			if (bossIndex < Main.maxNPCs)
			{
				Main.npc[bossIndex].ai[0] = 0f; // Состояние неподвижной статуи
				Main.npc[bossIndex].netUpdate = true;
			}
		}
	}
}
