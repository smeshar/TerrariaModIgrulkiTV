# Elden Ring & D&D: Lands Between — Terraria Mod

Мод для Terraria (tModLoader 1.4.4+), переносящий атмосферу и концепции **Elden Ring** и **Dungeons & Dragons**:
- **Фиксированные NPC**: Жители не приходят сами в построенные коробки, а стоят на заранее определенных позициях карты (лагеря, руины, церкви).
- **Неуязвимость NPC**: Жители абсолютно неуязвимы к мобам, боссам, лаве, ловушкам и игроку (Souls-like стиль).
- **Карта Междуземья**: Замена стандартной генерации на структуру биомов Elden Ring (Замогилье, Лиурния, Кэлид, Альтус, Вершины Великанов) с руинами церквей, кострами и катакомбами.
- **Использование ванильных ресурсов**: Оружие, магия и структуры используют готовые спрайты, снаряды и анимации Terraria.

---

## 1. Что необходимо для разработки

1. **Terraria + tModLoader**:
   - Установите Terraria в Steam.
   - Установите бесплатный **tModLoader** (официально доступен в Steam).
2. **.NET 8.0 SDK**:
   - Скачайте и установите [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
3. **Редактор кода (IDE)**:
   - **Visual Studio Code** с расширениями *C# Dev Kit* / *OmniSharp*.
   - Или **Visual Studio Community 2022** (с компонентом «Разработка классических приложений .NET»).
4. **TEdit (Редактор карт Terraria)**:
   - Скачайте с официального GitHub [TEdit Releases](https://github.com/TEdit/Terraria-Map-Editor/releases).
   - Позволяет вручную рисовать замки, горы, данжи и реки Междуземья с точностью до каждого блока.

---

## 2. Подключение проекта к tModLoader

tModLoader компилирует моды из папки `ModSources`:
`C:\Users\<ВашеИмя>\Documents\My Games\Terraria\tModLoader\ModSources`

Чтобы проект лежал в текущей папке `d:\VSCodeProjects\TerrariaMod` и одновременно был виден в tModLoader, создайте символическую ссылку (запустите PowerShell от имени администратора):

```powershell
New-Item -ItemType SymbolicLink -Path "$HOME\Documents\My Games\Terraria\tModLoader\ModSources\EldenRingDnD" -Target "d:\VSCodeProjects\TerrariaMod"
```

После этого:
1. Запустите **tModLoader**.
2. Перейдите в меню **Разработка модов** (Workshop / Mod Sources).
3. Нажмите **Собрать и перезагрузить** (Build + Reload) напротив `EldenRingDnD`.

---

## 3. Архитектура проекта

- `build.txt` / `description.txt` — метаданные мода для загрузчика.
- `EldenRingDnD.csproj` — файл проекта C# (.NET 8.0).
- `EldenRingDnD.cs` — главный класс мода.
- `Common/Globals/EldenTownNPC.cs`:
  - `CanTownNPCSpawn(...) => false` — запрет естественного заселения в дома.
  - `SetDefaults` / `CanBeHitByNPC` / `ModifyIncomingHit` / `CheckDead` — 100% неуязвимость и иммунитет ко всем видам урона.
  - `PreAI` — удержание NPC в зоне их поста (не падают в пропасти).
- `Common/Systems/EldenNPCSystem.cs`:
  - Список `FixedSpawnPoints` с точными координатами (X, Y в тайлах) для каждого сюжетного NPC.
- `Common/Systems/WorldSystem.cs`:
  - `ModifyWorldGenTasks` — отключение неподходящих ванильных этапов (парящие острова, пирамиды) и внедрение биомов Междуземья.
  - `BuildRuinedChurch` — процедурный строитель руин церквей с местом благодати (костром).
- `Content/Items/Weapons/DarkMoonGreatsword.cs` — пример оружия, использующего ванильный спрайт `ItemID.IceSickle` и анимацию взмаха со снарядом.
- `Localization/` — файлы локализации (`en-US` и `ru-RU`).
- `Maps/` — папка для готовых карт `.wld`.
