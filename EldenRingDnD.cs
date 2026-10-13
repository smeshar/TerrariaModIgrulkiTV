using Terraria.ModLoader;

namespace EldenRingDnD
{
	/// <summary>
	/// Главный класс мода Elden Ring & D&D.
	/// </summary>
	public class EldenRingDnD : Mod
	{
		public static EldenRingDnD Instance { get; private set; } = null!;

		public override void Load()
		{
			Instance = this;
			Logger.Info("Elden Ring & D&D mod initialized.");
		}

		public override void Unload()
		{
			Instance = null!;
		}
	}
}
