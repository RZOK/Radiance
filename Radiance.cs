global using Terraria.ModLoader;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Radiance.Core;
global using Radiance.Core.Interfaces;
global using Radiance.Utilities;
global using System;
global using System.Linq;
global using Terraria.Audio;
global using Terraria.ID;
global using Terraria.IO;
global using System.IO;
global using Terraria.ModLoader.IO;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Microsoft.Xna.Framework;
global using System.Collections.Generic;
global using static Radiance.Utilities.RadianceUtils;
global using static Microsoft.Xna.Framework.MathHelper;
using ReLogic.Content;
using CsvHelper;
using Radiance.Core.Config;

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
            if (!Main.dedServ)
            {
                LoadAssets();
            }
        }
        private void LoadAssets()
        {
            blankTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Blank").Value;
            notBlankTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/NotBlank").Value;
            debugTexture = ModContent.Request<Texture2D>("Radiance/Content/ExtraTextures/Debug").Value;

            if (ModContent.GetInstance<RadianceConfig>().PreloadAssets)
            {
                Stream file = GetFileStream("LoadableTextures.txt");
                StreamReader reader = new StreamReader(file);
                List<string> textures = reader.ReadToEnd().Split("\n").ToList();
                reader.Close();
                foreach (string texture in textures)
                {
                    string trimmedTexture = texture.TrimEnd('\r', '\n');
                    if (trimmedTexture == string.Empty)
                        continue;

                    ModContent.Request<Texture2D>($"Radiance/{trimmedTexture}", AssetRequestMode.ImmediateLoad);
                }
            }
        }
        private void UnloadAssets()
        {
            blankTexture = null;
            notBlankTexture = null;
            debugTexture = null;
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
                UnloadAssets();
            }
        }
    }
}