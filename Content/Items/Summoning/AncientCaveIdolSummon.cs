using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaModIgrulkiTV.Content.NPCs.Bosses.CaveIdol;

namespace TerrariaModIgrulkiTV.Content.Items.Summoning
{
	/// <summary>
	/// Предмет призыва босса «Каменный Идол Подземелья».
	/// Гарантированно выпадает с босса и может быть использован только в подземелье.
	/// </summary>
	public class AncientCaveIdolSummon : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.maxStack = 20;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
		}

		public override bool CanUseItem(Player player)
		{
			// Разрешено использовать только в подземных слоях (пещеры/каверны) и когда босс еще не призван
			bool isUnderground = player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight || player.ZoneUnderworldHeight;
			bool bossNotAlive = !NPC.AnyNPCs(ModContent.NPCType<CaveIdolBoss>());

			return isUnderground && bossNotAlive;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				SoundEngine.PlaySound(SoundID.Roar, player.Center);

				int bossType = ModContent.NPCType<CaveIdolBoss>();
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					// Спавним босса прямо над игроком в пробужденном состоянии
					int npcIndex = NPC.NewNPC(
						player.GetSource_ItemUse(Item),
						(int)player.Center.X,
						(int)player.Center.Y - 260,
						bossType
					);

					if (npcIndex < Main.maxNPCs)
					{
						Main.npc[npcIndex].ai[0] = 2f; // Фаза активного боя
						Main.npc[npcIndex].netUpdate = true;
					}
				}
				else
				{
					NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: bossType);
				}
			}

			return true;
		}

		public override void AddRecipes()
		{
			// Рецепт для удобства: можно создать из камня и аметистов на верстаке
			CreateRecipe()
				.AddIngredient(ItemID.StoneBlock, 30)
				.AddIngredient(ItemID.Amethyst, 3)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
