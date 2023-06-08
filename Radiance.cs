using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using System.IO;

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