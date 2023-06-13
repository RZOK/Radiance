global using Terraria.ModLoader;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using System.IO;
global using Radiance.Core;
global using Radiance.Core.Interfaces;
global using Radiance.Utilities;
global using System;
global using System.Linq;
global using Terraria.Audio;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.ModLoader.IO;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Microsoft.Xna.Framework;
global using System.Collections.Generic;
using Radiance.Content.Items.BaseItems;

namespace Radiance
{
    public class Radiance : Mod
    {
        public static Radiance Instance { get; set; }

		public const int maxDistanceBetweenPoints = 1000;
        public const float encycolradiaLineScale = 0.9f;

        public static Texture2D blankTexture;
        public static Texture2D notBlankTexture;
        public static Texture2D debugTexture;

        public Radiance()
		{
			Instance = this;
		}
        public override void Load()
        {
            blankTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Blank").Value;
            notBlankTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            debugTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Debug").Value; //unload these
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetEasy.NetEasy.HandleModule(reader, whoAmI);
        }
        public override void Unload()
        {
            if (!Main.dedServ)
            {
                Instance = null;
            }
        }
    }
}