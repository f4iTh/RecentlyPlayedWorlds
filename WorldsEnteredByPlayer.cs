using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RecentlyPlayedWorlds {
  [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
  public class WorldsEnteredByPlayer : ModPlayer {
    public Dictionary<string, ulong> WorldsEntered = new();

    public override void SaveData(TagCompound tag) {
      List<TagCompound> list = this.WorldsEntered
        .Select(item => new TagCompound { { "worldUniqueId", item.Key }, { "timestamp", item.Value } }).ToList();

      tag["WorldsEntered"] = list;
    }

    public override void LoadData(TagCompound tag) {
      IList<TagCompound> list = tag.GetList<TagCompound>("WorldsEntered");

      foreach (TagCompound item in list) {
        var worldUniqueId = item.Get<string>("worldUniqueId");
        var timestamp = item.Get<ulong>("timestamp");
        this.WorldsEntered[worldUniqueId] = timestamp;
      }
    }

    public override void OnEnterWorld() {
      if (Main.ActiveWorldFileData == null)
        return;

      var timestamp = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
      var worldUniqueId = Main.ActiveWorldFileData.UniqueId.ToString();


      this.WorldsEntered[worldUniqueId] = timestamp;
    }
  }
}