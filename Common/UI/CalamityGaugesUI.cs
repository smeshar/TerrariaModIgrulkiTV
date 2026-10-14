using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaModIgrulkiTV.Common.Players;
using TerrariaModIgrulkiTV.Common.Systems;
using TerrariaModIgrulkiTV.Content.Buffs;

namespace TerrariaModIgrulkiTV.Common.UI
{
	/// <summary>
	/// Отрисовка интерфейса шкал Адреналина и Ярости в стиле Calamity Mod.
	/// </summary>
	public class CalamityGaugesUI : ModSystem
	{
		private static Asset<Texture2D>? rageBorder;
		private static Asset<Texture2D>? rageFill;
		private static Asset<Texture2D>? adrenalineBorder;
		private static Asset<Texture2D>? adrenalineFill;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				rageBorder = ModContent.Request<Texture2D>("TerrariaModIgrulkiTV/Content/UI/RageBarBorder");
				rageFill = ModContent.Request<Texture2D>("TerrariaModIgrulkiTV/Content/UI/RageBarFill");
				adrenalineBorder = ModContent.Request<Texture2D>("TerrariaModIgrulkiTV/Content/UI/AdrenalineBarBorder");
				adrenalineFill = ModContent.Request<Texture2D>("TerrariaModIgrulkiTV/Content/UI/AdrenalineBarFill");
			}
		}

		public override void Unload()
		{
			rageBorder = null;
			rageFill = null;
			adrenalineBorder = null;
			adrenalineFill = null;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int resourceBarsIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarsIndex == -1)
			{
				resourceBarsIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
			}

			if (resourceBarsIndex != -1)
			{
				layers.Insert(resourceBarsIndex + 1, new LegacyGameInterfaceLayer(
					"TerrariaModIgrulkiTV: Calamity Gauges",
					DrawGaugesLayer,
					InterfaceScaleType.UI
				));
			}
		}

		private static bool DrawGaugesLayer()
		{
			if (Main.gameMenu || Main.hideUI)
			{
				return true;
			}

			Player player = Main.LocalPlayer;
			if (!player.active || player.dead)
			{
				return true;
			}

			CalamityGaugePlayer modPlayer = player.GetModPlayer<CalamityGaugePlayer>();
			SpriteBatch spriteBatch = Main.spriteBatch;

			// Позиционирование шкал по центру в нижней части экрана (над хотбаром или снизу)
			int screenW = Main.screenWidth;
			int screenH = Main.screenHeight;

			// Координаты: размещаем две шкалы бок о бок
			int totalWidth = 320;
			int startX = (screenW - totalWidth) / 2;
			int startY = screenH - 58;

			// 1. Отрисовка шкалы Ярости (Слева)
			DrawGauge(
				spriteBatch,
				startX,
				startY,
				modPlayer.Rage,
				CalamityGaugePlayer.MaxRage,
				player.HasBuff<RageBuff>(),
				rageBorder?.Value,
				rageFill?.Value,
				"ЯРОСТЬ",
				"Rage",
				Color.OrangeRed,
				Color.Crimson,
				KeybindSystem.RageHotKey?.GetAssignedKeys().Count > 0 ? KeybindSystem.RageHotKey.GetAssignedKeys()[0] : "B"
			);

			// 2. Отрисовка шкалы Адреналина (Справа)
			DrawGauge(
				spriteBatch,
				startX + 162,
				startY,
				modPlayer.Adrenaline,
				CalamityGaugePlayer.MaxAdrenaline,
				player.HasBuff<AdrenalineBuff>(),
				adrenalineBorder?.Value,
				adrenalineFill?.Value,
				"АДРЕНАЛИН",
				"Adrenaline",
				Color.Cyan,
				Color.DeepSkyBlue,
				KeybindSystem.AdrenalineHotKey?.GetAssignedKeys().Count > 0 ? KeybindSystem.AdrenalineHotKey.GetAssignedKeys()[0] : "V"
			);

			return true;
		}

		private static void DrawGauge(
			SpriteBatch sb,
			int x,
			int y,
			float value,
			float maxValue,
			bool isBuffActive,
			Texture2D? borderTex,
			Texture2D? fillTex,
			string label,
			string labelEn,
			Color primaryColor,
			Color secondaryColor,
			string keybindName)
		{
			if (borderTex == null || fillTex == null)
			{
				return;
			}

			float percent = MathHelper.Clamp(value / maxValue, 0f, 1f);
			bool isReady = percent >= 0.999f;

			// Пульсация при полной готовности
			float pulse = 0f;
			if (isReady || isBuffActive)
			{
				pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 7f) * 0.4f + 0.6f;
			}

			// 1. Заливка шкалы (прогресс)
			int fillW = (int)(fillTex.Width * (isBuffActive ? 1f : percent));
			if (fillW > 0)
			{
				Rectangle fillSource = new Rectangle(0, 0, fillW, fillTex.Height);
				Vector2 fillPos = new Vector2(x + 5, y + 5);
				Color fillColor = isBuffActive ? Color.Lerp(primaryColor, Color.White, pulse) : primaryColor;
				sb.Draw(fillTex, fillPos, fillSource, fillColor);
			}

			// 2. Рамка шкалы
			Color borderColor = Color.White;
			if (isReady)
			{
				borderColor = Color.Lerp(Color.White, primaryColor, pulse * 0.7f);
			}
			sb.Draw(borderTex, new Vector2(x, y), borderColor);

			// 3. Текстовая метка и проценты
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			string statusText;

			if (isBuffActive)
			{
				statusText = "АКТИВНО!";
			}
			else if (isReady)
			{
				statusText = $"ГОТОВО! [{keybindName}]";
			}
			else
			{
				statusText = $"{label} {(int)(percent * 100)}%";
			}

			Vector2 textSize = font.MeasureString(statusText) * 0.72f;
			Vector2 textPos = new Vector2(x + (borderTex.Width - textSize.X) / 2f, y + (borderTex.Height - textSize.Y) / 2f);

			// Тень текста
			sb.DrawString(font, statusText, textPos + new Vector2(1, 1), Color.Black * 0.85f, 0f, Vector2.Zero, 0.72f, SpriteEffects.None, 0f);

			// Сам текст с легким переливом при готовности
			Color textColor = isBuffActive || isReady ? Color.Lerp(primaryColor, Color.White, pulse) : Color.LightGray;
			sb.DrawString(font, statusText, textPos, textColor, 0f, Vector2.Zero, 0.72f, SpriteEffects.None, 0f);
		}
	}
}
