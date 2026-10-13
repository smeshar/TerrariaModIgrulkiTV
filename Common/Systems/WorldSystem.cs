using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.GameContent.Generation;

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
		/// Карта делится на узнаваемые регионы:
		/// - Запад: Озерная Лиурния (вода, синий кирпич, мрамор)
		/// - Центр: Замогилье (зеленые холмы, серый камень, руины)
		/// - Восток: Звездные пустоши / Кэлид (багровые блоки, плоть, пепел, кости)
		/// - Север: Плато Альтус и Вершины Великанов (золотой песчаник, снег и лед)
		/// </summary>
		private void GenerateLandsBetweenBiomes(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Формирование Междуземья (Замогилье, Лиурния, Кэлид, Альтус)...";

			int worldWidth = Main.maxTilesX;
			int worldHeight = Main.maxTilesY;

			// Определение границ биомов по ширине (в процентах от размера мира)
			int liurniaEnd = (int)(worldWidth * 0.30f);
			int limgraveEnd = (int)(worldWidth * 0.70f);
			// Оставшаяся правая часть (70% - 100%) — Кэлид / Звездные пустоши

			for (int x = 0; x < worldWidth; x++)
			{
				progress.Set((float)x / worldWidth);

				// Поверхностная высота
				int surfaceY = (int)Main.worldSurface;

				// 1. Зона Кэлида (Восточная треть карты) — Багровые земли, Гниль
				if (x >= limgraveEnd)
				{
					ApplyCaelidStyle(x, surfaceY);
				}
				// 2. Зона Лиурнии (Западная треть карты) — Затопленные низины
				else if (x < liurniaEnd)
				{
					ApplyLiurniaStyle(x, surfaceY);
				}
				// 3. Зона Замогилья (Центр) — Равнины и руины
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
		/// Генерация тематических мини-данжей Elden Ring (Катакомбы, Руины церквей, Лагеря).
		/// </summary>
		private void GenerateThematicDungeons(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Возведение замков, катакомб и мест благодати...";

			// Размещение стартовой церкви (аналог Церкви Эллех) около стартовой точки игрока
			int churchX = Main.spawnTileX + 30;
			int churchY = (int)Main.worldSurface - 2;

			BuildRuinedChurch(churchX, churchY);
		}

		/// <summary>
		/// Пример генератора структуры: Разрушенная церковь (каменный кирпич, костер, платформы).
		/// </summary>
		public static void BuildRuinedChurch(int startX, int startY)
		{
			int width = 24;
			int height = 12;

			for (int x = startX; x < startX + width; x++)
			{
				for (int y = startY; y < startY + height; y++)
				{
					// Очищаем внутреннее пространство
					WorldGen.KillTile(x, y);

					// Пол церкви из серого кирпича
					if (y == startY + height - 1)
					{
						WorldGen.PlaceTile(x, y, TileID.GrayBrick, mute: true, forced: true);
					}
					// Стены здания (с разрушенными арками)
					else if ((x == startX || x == startX + width - 1) && y > startY + 3)
					{
						WorldGen.PlaceTile(x, y, TileID.GrayBrick, mute: true, forced: true);
					}
					// Фоновые каменные стены
					else
					{
						WorldGen.PlaceWall(x, y, WallID.GrayBrick);
					}
				}
			}

			// Размещаем костер (Место благодати / Campfire) в центре церкви
			int fireX = startX + width / 2;
			int fireY = startY + height - 2;
			WorldGen.PlaceTile(fireX, fireY, TileID.Campfire, mute: true, forced: true);
		}
	}
}
