using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace EldenRingDnD.Content.Items.Weapons
{
	/// <summary>
	/// Двуручник Темной Луны (Dark Moon Greatsword / Elden Ring).
	/// Демонстрирует использование существующих ванильных спрайтов и анимаций:
	/// Спрайт заимствован у ванильного ItemID.IceSickle / Frostbrand без создания PNG файла.
	/// </summary>
	public class DarkMoonGreatsword : ModItem
	{
		// Использование ванильного спрайта Terraria напрямую:
		public override string Texture => $"Terraria/Images/Item_{ItemID.IceSickle}";

		public override void SetDefaults()
		{
			Item.width = 46;
			Item.height = 48;

			// Параметры атаки и урона (стиль D&D / Elden Ring: масштабирование от силы и интеллекта)
			Item.damage = 65;
			Item.DamageType = DamageClass.Magic; // Или Hybrid (Magic/Melee)
			Item.mana = 12;                      // Затраты ОК / Маны на взмах
			Item.knockBack = 5.5f;
			Item.crit = 8;                       // 8% шанс крита

			// Ванильная анимация взмаха меча
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.UseSound = SoundID.Item28; // Магический ледяной звук
			Item.autoReuse = true;

			// Ванильный снаряд: выпускает ледяное лунное лезвие
			Item.shoot = ProjectileID.IceSickle;
			Item.shootSpeed = 11f;

			Item.value = Item.buyPrice(gold: 15);
			Item.rare = ItemRarityID.Cyan; // Редкость уровня позднего хардмода
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			// Добавляем частицы лунной пыли при каждом взмахе
			if (Main.rand.NextBool(2))
			{
				Dust.NewDust(
					new Vector2(hitbox.X, hitbox.Y),
					hitbox.Width,
					hitbox.Height,
					DustID.IceTorch,
					0f,
					0f,
					150,
					default,
					1.3f
				);
			}
		}

		public override void AddRecipes()
		{
			// Рецепт создания из ванильных материалов (магические руны и кристаллы)
			CreateRecipe()
				.AddIngredient(ItemID.TitaniumBar, 12)
				.AddIngredient(ItemID.FallenStar, 10)
				.AddIngredient(ItemID.SoulofNight, 8)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
