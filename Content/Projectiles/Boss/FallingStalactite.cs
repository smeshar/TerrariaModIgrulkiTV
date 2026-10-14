using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaModIgrulkiTV.Content.Projectiles.Boss
{
	/// <summary>
	/// Падающий сталактит, срываемый с потолка пещеры атаками босса.
	/// </summary>
	public class FallingStalactite : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 28;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 360;
			Projectile.ignoreWater = false;
			Projectile.tileCollide = true;
		}

		public override void AI()
		{
			// Гравитационное ускорение вниз
			Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.35f, -10f, 15f);

			// Легкое покачивание в полете
			Projectile.rotation = Projectile.velocity.X * 0.08f;

			// Частицы пыли и камня при падении
			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Stone, Vector2.Zero, 100, default, 0.9f);
				d.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);

			// Раскалывание на каменные обломки
			for (int i = 0; i < 14; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, vel.X, vel.Y, 100, default, 1.2f);
			}
		}
	}
}
