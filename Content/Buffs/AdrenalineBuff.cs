using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaModIgrulkiTV.Content.Buffs
{
	/// <summary>
	/// Бафф режима Адреналина (Calamity-style).
	/// Дает колоссальное увеличение всего наносимого урона на 150%.
	/// При активности игрок излучает электрические разряды и частицы циана.
	/// </summary>
	public class AdrenalineBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = false;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			// Колоссальный прирост урона (+150% общего урона)
			player.GetDamage(DamageClass.Generic) += 1.50f;

			// Визуальные эффекты: электрические разряды и светящиеся искры
			if (!Main.dedServ && Main.rand.NextBool(2))
			{
				Vector2 dustPos = player.position + new Vector2(Main.rand.Next(player.width), Main.rand.Next(player.height));
				Dust d = Dust.NewDustPerfect(dustPos, DustID.Electric, Vector2.Zero, 100, Color.Cyan, 1.1f);
				d.noGravity = true;
				d.velocity = player.velocity * 0.3f + Main.rand.NextVector2Circular(2f, 2f);

				Lighting.AddLight(player.Center, 0.1f, 0.8f, 1.0f);
			}
		}
	}
}
