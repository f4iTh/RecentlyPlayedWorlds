using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RecentlyPlayedWorlds.Common.Systems;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace RecentlyPlayedWorlds.Common.IL {
  /// <summary>A class containing patches for <see cref="Terraria.GameContent.UI.Elements.UIWorldListItem" />.</summary>
  [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  public class UIWorldListItemPatches {
    /// <summary>A patch to add an icon to indicate that the player has previously entered the world.</summary>
    /// <param name="il">The IL context.</param>
    public static void IL_UIWorldListItemOnctor(ILContext il) {
      try {
        ILCursor ilCursor = new(il);

        if (!ilCursor.TryGotoNext(MoveType.Before, i => i.MatchRet())) {
          ModEntry.StaticLogger.Error("Could not locate return in \"UIWorldListItem::ctor\". Unable to perform patch.");
          return;
        }

        ilCursor.Emit(OpCodes.Ldarg_0);
        ilCursor.Emit<UIWorldListItemPatches>(OpCodes.Call, nameof(UIWorldListItemPatches.AppendLastPlayedIcon));
      }
      catch (Exception) {
        MonoModHooks.DumpIL(ModContent.GetInstance<ModEntry>(), il);
      }
    }

    /// <summary>Adds an icon after the world name to indicate that the player has previously entered the world.</summary>
    /// <param name="worldListItem">The world list item.</param>
    private static void AppendLastPlayedIcon([SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")] UIWorldListItem worldListItem) {
      WorldsEnteredByPlayer player = Main.ActivePlayerFileData.Player.GetModPlayer<WorldsEnteredByPlayer>();

      if (!player.WorldsEntered.ContainsKey(worldListItem.Data.UniqueId.ToString()))
        return;

      DynamicSpriteFont font = FontAssets.MouseText.Value;
      Vector2 worldNameStringLength = font.MeasureString(worldListItem.Data.GetWorldName());

      UIImage worldEnteredIcon = new(TextureAssets.Cursors[3]) {
        HAlign = 0f,
        VAlign = 0f,
        IgnoresMouseInteraction = true
      };

      // Top-right; has overlap with some other mod(s)
      //   UIImage worldEnteredIcon = new(TextureAssets.Cursors[3]) {
      //     HAlign = 1f,
      //     VAlign = 0f,
      //     IgnoresMouseInteraction = true
      //   };

      worldEnteredIcon.Left.Set(64f + 6f + worldNameStringLength.X + 6f, 0f);
      worldListItem.Append(worldEnteredIcon);
    }
  }
}