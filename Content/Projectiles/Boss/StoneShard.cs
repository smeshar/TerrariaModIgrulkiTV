using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaModIgrulkiTV.Content.Projectiles.Boss
{
	/// <summary>
	/// Светящийся аметистовый каменный осколок, выпускаемый боссом во второй фазе.
	/// </summary>
	public class StoneShard : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			// Быстрое вращение кристаллического осколка
			Projectile.rotation += 0.25f;

			// Аметистовое свечение
			Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.7f);

			// След из магических искр
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemAmethyst, -Projectile.velocity * 0.2f, 100, default, 1.1f);
				d.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

			// Вспышка аметистовых искр при попадании
			for (int i = 0; i < 16; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst, vel.X, vel.Y, 100, default, 1.3f);
			}
		}
	}
}
