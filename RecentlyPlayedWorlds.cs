using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RecentlyPlayedWorlds.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.ModLoader;

namespace RecentlyPlayedWorlds {
  public class RecentlyPlayedWorlds : Mod {
    public override void Load() {
      On_UIWorldListItem.ctor += On_UIWorldListItemOnctor;
      On_UIWorldSelect.UpdateWorldsList += On_UIWorldSelectOnUpdateWorldsList;
    }

    private static void On_UIWorldListItemOnctor(On_UIWorldListItem.orig_ctor orig, UIWorldListItem self, WorldFileData data, int orderInList, bool canBePlayed) {
      orig(self, data, orderInList, canBePlayed);

      WorldsEnteredByPlayer modPlayer = Main.ActivePlayerFileData.Player.GetModPlayer<WorldsEnteredByPlayer>();
      if (!modPlayer.WorldsEntered.ContainsKey(data.GetFileName(false))) return;

      UIImage element = new(TextureAssets.Cursors[3]) {
        HAlign = 1f,
        VAlign = 0f,
        IgnoresMouseInteraction = true
      };
      self.Append(element);
    }

    private static void On_UIWorldSelectOnUpdateWorldsList(On_UIWorldSelect.orig_UpdateWorldsList orig, UIWorldSelect self) {
      try {
        MethodInfo addIndividualWorldMigrationButtons = typeof(UIWorldSelect).GetMethod("AddIndividualWorldMigrationButtons", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo addAutomaticWorldMigrationButtons = typeof(UIWorldSelect).GetMethod("AddAutomaticWorldMigrationButtons", BindingFlags.Instance | BindingFlags.NonPublic);
        UIList worldList = (UIList)typeof(UIWorldSelect).GetField("_worldList", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(self);

        worldList!.Clear();

        IOrderedEnumerable<WorldFileData> orderedEnumerable = new List<WorldFileData>(Main.WorldList)
          .OrderByDescending(CanWorldBePlayed)
          .ThenByDescending(x => x.IsFavorite)
          .ThenByDescending(LastPlayed)
          .ThenBy(x => x.Name)
          .ThenBy(x => x.GetFileName());

        int num = 0;
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (WorldFileData item in orderedEnumerable!) 
          worldList.Add(new UIWorldListItem(item, num++, CanWorldBePlayed(item)));

        addIndividualWorldMigrationButtons!.Invoke(self, null);

        // ReSharper disable once PossibleMultipleEnumeration
        if (!orderedEnumerable!.Any()) addAutomaticWorldMigrationButtons!.Invoke(self, null);
      }
      catch (Exception) {
        orig(self);
      } 
    }

    private static bool CanWorldBePlayed(WorldFileData file) {
      bool num = Main.ActivePlayerFileData.Player.difficulty == 3;
      bool flag = file.GameMode == 3;

      return num == flag && SystemLoader.CanWorldBePlayed(Main.ActivePlayerFileData, file, out ModSystem _);
    }

    private static int LastPlayed(WorldFileData file) {
      WorldsEnteredByPlayer modPlayer = Main.ActivePlayerFileData.Player.GetModPlayer<WorldsEnteredByPlayer>();
      if (modPlayer.WorldsEntered.TryGetValue(file.GetFileName(false), out ulong timestamp))
        return (int)timestamp;

      return 0;
    }
  }
}