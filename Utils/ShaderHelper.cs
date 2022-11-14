﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Radiance.Utils
{
    public static class ShaderHelper
    {
        public static bool HasParameter(this Effect effect, string parameterName)
        {
            foreach (EffectParameter parameter in effect.Parameters)
            {
                if (parameter.Name == parameterName)
                    return true;
            }

            return false;
        }

        public static void ActivateScreenShader(string ShaderName, Vector2 vec = default)
        {
            if (Main.netMode != NetmodeID.Server && !Filters.Scene[ShaderName].IsActive())
            {
                Filters.Scene.Activate(ShaderName, vec);
            }
        }

        public static ScreenShaderData GetScreenShader(string ShaderName) => Filters.Scene[ShaderName].GetShader();
    }
}
