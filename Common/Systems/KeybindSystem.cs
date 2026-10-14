using Terraria.ModLoader;

namespace TerrariaModIgrulkiTV.Common.Systems
{
	/// <summary>
	/// Система регистрации горячих клавиш для механик Calamity (Адреналин и Ярость).
	/// </summary>
	public class KeybindSystem : ModSystem
	{
		public static ModKeybind AdrenalineHotKey { get; private set; } = null!;
		public static ModKeybind RageHotKey { get; private set; } = null!;

		public override void Load()
		{
			// Клавиши по умолчанию: V для Адреналина, B для Ярости (как в Calamity)
			AdrenalineHotKey = KeybindLoader.RegisterKeybind(Mod, "Adrenaline", "V");
			RageHotKey = KeybindLoader.RegisterKeybind(Mod, "Rage", "B");
		}

		public override void Unload()
		{
			AdrenalineHotKey = null!;
			RageHotKey = null!;
		}
	}
}
