using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using log4net;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RecentlyPlayedWorlds.Systems;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.IO;
using Terraria.ModLoader;

namespace RecentlyPlayedWorlds {
  [SuppressMessage("ReSharper", "UnusedType.Global")]
  [SuppressMessage("ReSharper", "UnusedMember.Local")]
  [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
  public class RecentlyPlayedWorlds : Mod {
    private static ILog _staticLogger;
    
    public override void Load() {
      _staticLogger = this.Logger;
      
      IL_UIWorldListItem.ctor += IL_UIWorldListItemOnctor;
      IL_UIWorldSelect.UpdateWorldsList += IL_UIWorldSelectOnUpdateWorldsList;
    }
    
    private static void IL_UIWorldListItemOnctor(ILContext il) {
      try {
        ILCursor ilCursor = new(il);

        if (!ilCursor.TryGotoNext(MoveType.Before, i => i.MatchRet())) {
          _staticLogger.Error("Could not locate return in \"UIWorldListItem::ctor\". Unable to perform patch.");
          return;
        }

        ilCursor.Emit(OpCodes.Ldarg_0);
        ilCursor.Emit<RecentlyPlayedWorlds>(OpCodes.Call, nameof(AppendLastPlayedIcon));
      }
      catch (Exception) {
        MonoModHooks.DumpIL(ModContent.GetInstance<RecentlyPlayedWorlds>(), il);
      }
    }
    
    // IL_004A: ldnull
    // IL_004B: ldftn     int32 Terraria.GameContent.UI.States.UIWorldSelect::LastPlayed(class Terraria.IO.WorldFileData)
    // IL_0051: newobj    instance void class [System.Private.CoreLib]System.Func`2<class Terraria.IO.WorldFileData, int32>::.ctor(object, native int)
    // IL_0056: call      class [System.Linq]System.Linq.IOrderedEnumerable`1<!!0> [System.Linq]System.Linq.Enumerable::ThenByDescending<class Terraria.IO.WorldFileData, int32>(class [System.Linq]System.Linq.IOrderedEnumerable`1<!!0>, class [System.Private.CoreLib]System.Func`2<!!0, !!1>)
    private static void IL_UIWorldSelectOnUpdateWorldsList(ILContext il) {
      try {
        ILCursor ilCursor = new(il);

        if (!ilCursor.TryGotoNext(MoveType.After, i => i.MatchCall(typeof(Enumerable), "ThenByDescending"))) {
          _staticLogger.Error("Could not locate \"System.Linq.Enumerable::ThenByDescending\" in \"UIWorldSelect::UpdateWorldsList\". Unable to perform patch.");
          return;
        }
        
        ilCursor.Emit(OpCodes.Ldnull);
        ilCursor.Emit(OpCodes.Ldftn, typeof(RecentlyPlayedWorlds).GetMethod("LastPlayed", BindingFlags.NonPublic | BindingFlags.Static, new[] { typeof(WorldFileData) }));
        ilCursor.Emit(OpCodes.Newobj, typeof(Func<WorldFileData, int>).GetConstructor(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public, new[] { typeof(object), typeof(IntPtr) }));
        ilCursor.Emit(OpCodes.Call, typeof(Enumerable).GetMethods(BindingFlags.Public | BindingFlags.Static).First(methodInfo => methodInfo.Name == "ThenByDescending" && methodInfo.GetParameters().Length == 2).MakeGenericMethod(typeof(WorldFileData), typeof(int)));
      }
      catch (Exception) {
        MonoModHooks.DumpIL(ModContent.GetInstance<RecentlyPlayedWorlds>(), il);
      }
    }

    private static void AppendLastPlayedIcon([SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")] UIWorldListItem worldListItem) {
      WorldsEnteredByPlayer player = Main.ActivePlayerFileData.Player.GetModPlayer<WorldsEnteredByPlayer>();

      if (!player.WorldsEntered.ContainsKey(worldListItem.Data.GetFileName(false))) {
        // _staticLogger.Debug($"Player has not entered world \"{worldListItem.Data.GetFileName()}\".");
        return;
      }
      
      DynamicSpriteFont font = FontAssets.MouseText.Value;
      Vector2 worldNameStringLength = font.MeasureString(worldListItem.Data.GetWorldName());

      UIImage worldEnteredIcon = new(TextureAssets.Cursors[3]) {
        HAlign = 0f,
        VAlign = 0f,
        IgnoresMouseInteraction = true
      };
      
      //   UIImage worldEnteredIcon = new(TextureAssets.Cursors[3]) {
      //     HAlign = 1f,
      //     VAlign = 0f,
      //     IgnoresMouseInteraction = true
      //   };
      
      worldEnteredIcon.Left.Set(64f + 6f + worldNameStringLength.X + 6f, 0f);
      worldListItem.Append(worldEnteredIcon);
    }
    
    private static int LastPlayed([SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")] WorldFileData file) {
      WorldsEnteredByPlayer modPlayer = Main.ActivePlayerFileData.Player.GetModPlayer<WorldsEnteredByPlayer>();
      return modPlayer.WorldsEntered.TryGetValue(file.GetFileName(false), out ulong timestamp) ? (int)timestamp : 0;
    }
  }
}