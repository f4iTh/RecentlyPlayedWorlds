using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RecentlyPlayedWorlds.Common.Systems {
  /// <summary>The system for handling which worlds a player has entered.</summary>
  [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
  public class WorldsEnteredByPlayer : ModPlayer {
    /// <summary>A dictionary of entered worlds and when the world was last entered.</summary>
    public Dictionary<string, ulong> WorldsEntered = new();

    /// <inheritdoc cref="ModPlayer.SaveData" />
    public override void SaveData(TagCompound tag) {
      List<TagCompound> list = this.WorldsEntered
        .Select(item => new TagCompound { { "worldUniqueId", item.Key }, { "timestamp", item.Value } }).ToList();

      tag["WorldsEntered"] = list;
    }

    /// <inheritdoc cref="ModPlayer.LoadData" />
    public override void LoadData(TagCompound tag) {
      IList<TagCompound> list = tag.GetList<TagCompound>("WorldsEntered");

      foreach (TagCompound item in list) {
        string worldUniqueId = item.Get<string>("worldUniqueId");
        ulong timestamp = item.Get<ulong>("timestamp");
        this.WorldsEntered[worldUniqueId] = timestamp;
      }
    }

    /// <inheritdoc cref="ModPlayer.OnEnterWorld" />
    public override void OnEnterWorld() {
      if (Main.ActiveWorldFileData == null)
        return;

      ulong timestamp = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
      string worldUniqueId = Main.ActiveWorldFileData.UniqueId.ToString();
      this.WorldsEntered[worldUniqueId] = timestamp;
    }
  }
}