using Terraria.ModLoader;

namespace TerrariaModIgrulkiTV
{
	/// <summary>
	/// Главный класс мода Elden Ring & D&D.
	/// Имя класса и корневое пространство имен совпадают с именем папки мода TerrariaModIgrulkiTV.
	/// </summary>
	public class TerrariaModIgrulkiTV : Mod
	{
		public static TerrariaModIgrulkiTV Instance { get; private set; } = null!;

		public override void Load()
		{
			Instance = this;
			Logger.Info("TerrariaModIgrulkiTV (Elden Ring & D&D) initialized successfully.");
		}

		public override void Unload()
		{
			Instance = null!;
		}
	}
}
