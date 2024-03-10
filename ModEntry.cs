using System.Diagnostics.CodeAnalysis;
using log4net;
using RecentlyPlayedWorlds.Common.IL;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ModLoader;

namespace RecentlyPlayedWorlds {
  /// <summary>
  /// The mod entry point.
  /// </summary>
  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
  public class ModEntry : Mod {
    /// <summary>
    /// Enables easy access to logging across the entire project.
    /// </summary>
    internal static ILog StaticLogger;

    /// <inheritdoc cref="Mod.Load"/>
    public override void Load() {
      StaticLogger = this.Logger;

      IL_UIWorldListItem.ctor += UIWorldListItemPatches.IL_UIWorldListItemOnctor;
      IL_UIWorldSelect.UpdateWorldsList += UIWorldSelectPatches.IL_UIWorldSelectOnUpdateWorldsList;
    }
  }
}