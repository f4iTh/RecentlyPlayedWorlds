using System.Diagnostics.CodeAnalysis;
using log4net;
using RecentlyPlayedWorlds.Common.IL;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader;

namespace RecentlyPlayedWorlds {
  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
  public class ModEntry : Mod {
    internal static ILog StaticLogger;

    public override void Load() {
      StaticLogger = this.Logger;

      IL_UIWorldListItem.ctor += UIWorldListItemPatches.IL_UIWorldListItemOnctor;
      IL_UIWorldSelect.UpdateWorldsList += UIWorldSelectPatches.IL_UIWorldSelectOnUpdateWorldsList;
    }
  }
}