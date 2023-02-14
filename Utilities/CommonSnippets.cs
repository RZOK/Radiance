using Microsoft.Xna.Framework;
using Radiance.Core;
using static Radiance.Utilities.CommonColors;

namespace Radiance.Utilities
{
    static class CommonSnippets
    {
        public static CustomTextSnippet BWSnippet(this string text) => new(text, Color.White, Color.Black);
        public static CustomTextSnippet DarkColorSnippet(this string text, Color color) => new(text, color, color.GetDarkColor());
    }
}
