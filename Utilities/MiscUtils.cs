using ReLogic.Graphics;
using Terraria;
using Terraria.ID;

namespace Radiance.Utilities
{
    partial class RadianceUtils
    {
        public static Item GetPlayerHeldItem() => Main.mouseItem.type == ItemID.None ? Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] : Main.mouseItem;
        //public static string WrapString(string input, int length, DynamicSpriteFont font, float scale)
        //{
        //    string output = "";
        //    string[] words = input.Split();

        //    string line = "";
        //    foreach (string str in words)
        //    {
        //        if (str == "NEWBLOCK")
        //        {
        //            output += "\n\n";
        //            line = "";
        //            continue;
        //        }

        //        if (font.MeasureString(line).X * scale < length)
        //        {
        //            output += " " + str;
        //            line += " " + str;
        //        }
        //        else
        //        {
        //            output += "\n" + str;
        //            line = str;
        //        }
        //    }
        //    return output.Substring(1);
        //}
    }

}
