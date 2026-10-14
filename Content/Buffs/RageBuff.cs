using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaModIgrulkiTV.Content.Buffs
{
	/// <summary>
	/// Бафф режима Ярости (Calamity-style).
	/// Дает +35% общего урона и +10% шанса критического удара.
	/// При активности игрок объят яростным багровым пламенем.
	/// </summary>
	public class RageBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = false;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			// Прирост урона и критического шанса
			player.GetDamage(DamageClass.Generic) += 0.35f;
			player.GetCritChance(DamageClass.Generic) += 10f;

			// Визуальные эффекты: багровое пламя и яростные искры
			if (!Main.dedServ && Main.rand.NextBool(2))
			{
				Vector2 dustPos = player.position + new Vector2(Main.rand.Next(player.width), Main.rand.Next(player.height));
				Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, Vector2.Zero, 100, Color.OrangeRed, 1.4f);
				d.noGravity = true;
				d.velocity = player.velocity * 0.2f + new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2.5f, -0.5f));

				Lighting.AddLight(player.Center, 1.0f, 0.3f, 0.1f);
			}
		}
	}
}
