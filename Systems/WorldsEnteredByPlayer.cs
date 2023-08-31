using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RecentlyPlayedWorlds.Systems {
  public class WorldsEnteredByPlayer : ModPlayer {
    public Dictionary<string, ulong> WorldsEntered = new();

    public override void SaveData(TagCompound tag) {
      List<TagCompound> list = this.WorldsEntered
        .Select(item => new TagCompound { { "worldName", item.Key }, { "timestamp", item.Value } })
        .ToList();
      tag["WorldsEntered"] = list;
    }

    public override void LoadData(TagCompound tag) {
      IList<TagCompound> list = tag.GetList<TagCompound>("WorldsEntered");
      foreach (TagCompound item in list) {
        string worldName = item.Get<string>("worldName");
        ulong timestamp = item.Get<ulong>("timestamp");
        this.WorldsEntered[worldName] = timestamp;
      }
    }

    public override void OnEnterWorld() {
      if (Main.ActiveWorldFileData == null)
        return;

      ulong unixTimestamp = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
      string fileName = Main.ActiveWorldFileData.GetFileName(false);

      if (this.WorldsEntered.ContainsKey(fileName))
        this.WorldsEntered[fileName] = unixTimestamp;
      else
        this.WorldsEntered.Add(fileName, unixTimestamp);
    }
  }
}