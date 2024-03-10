using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RecentlyPlayedWorlds.Common.Systems;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace RecentlyPlayedWorlds.Common.IL {
  /// <summary>A class containing patches for <see cref="Terraria.GameContent.UI.States.UIWorldSelect" />.</summary>
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class UIWorldSelectPatches {
    /// <summary>A patch to inject sorting by last accessed after sorting by <c>IsFavorite</c> and before sorting by <c>Name</c>.</summary>
    /// <param name="il">The IL context.</param>
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public static void IL_UIWorldSelectOnUpdateWorldsList(ILContext il) {
      try {
        ILCursor ilCursor = new(il);

        if (!ilCursor.TryGotoNext(MoveType.After, i => i.MatchCall(typeof(Enumerable), "ThenByDescending"))) {
          ModEntry.StaticLogger.Error("Could not locate \"System.Linq.Enumerable::ThenByDescending\" in \"UIWorldSelect::UpdateWorldsList\". Unable to perform patch.");
          return;
        }

        ilCursor.Emit(OpCodes.Ldnull);
        ilCursor.Emit(OpCodes.Ldftn, typeof(UIWorldSelectPatches).GetMethod("LastPlayed", BindingFlags.NonPublic | BindingFlags.Static, new[] { typeof(WorldFileData) }));
        ilCursor.Emit(OpCodes.Newobj, typeof(Func<WorldFileData, ulong>).GetConstructor(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public, new[] { typeof(object), typeof(IntPtr) }));
        ilCursor.Emit(OpCodes.Call, typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).First(methodInfo => methodInfo.Name == "ThenByDescending" && methodInfo.GetParameters().Length == 2).MakeGenericMethod(typeof(WorldFileData), typeof(int)));
      }
      catch (Exception) {
        MonoModHooks.DumpIL(ModContent.GetInstance<ModEntry>(), il);
      }
    }

    /// <summary>Gets when a world was accessed by the current player. Returns zero if it has not been accessed before.</summary>
    /// <param name="file">The world file data.</param>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private static ulong LastPlayed([SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")] WorldFileData file) {
      WorldsEnteredByPlayer modPlayer = Main.ActivePlayerFileData.Player.GetModPlayer<WorldsEnteredByPlayer>();
      return modPlayer.WorldsEntered.GetValueOrDefault(file.UniqueId.ToString(), 0UL);
    }
  }
}