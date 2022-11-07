using Microsoft.Xna.Framework;
using Radiance.Core.Systems;
using System;
using Terraria;
using Terraria.ModLoader.IO;

namespace Radiance.Core
{
    public class RadianceRay : TagSerializable
    {
        public int id = 0;
        public Vector2 startPos = Vector2.Zero;
        public Vector2 endPos = Vector2.Zero;
        public int transferRate = 20;
        public bool isInterferred = false;
        public bool active = false;

        #region Utility Methods

        public static int NewRadianceRay(Vector2 startPosition, Vector2 endPosition)
        {
            int num = Radiance.maxRays;
            for (int i = 0; i < Radiance.maxRays; i++)
            {
                if (Radiance.radianceRay[i] == null || !Radiance.radianceRay[i].active)
                {
                    num = i;
                    break;
                }
            }
            if (num == 1000)
            {

            }
            Main.NewText(num);
            Radiance.radianceRay[num] = new RadianceRay();
            RadianceRay radianceRay = Radiance.radianceRay[num];
            radianceRay.startPos = startPosition;
            radianceRay.endPos = endPosition;
            radianceRay.active = true;
            RadianceTransferSystem.Instance.rayList.Add(radianceRay);
            return num;
        }
        public static Vector2 SnapToCenterOfTile(Vector2 input)
        {
            return new Vector2((int)(Math.Floor(input.X / 16) * 16), (int)(Math.Floor(input.Y / 16) * 16)) + new Vector2(8, 8);
        }
#nullable enable
        public static RadianceRay? FindRay(Vector2 pos)
#nullable disable
        {
            for (int i = 0; i < Radiance.maxRays; i++)
            {
                RadianceRay ray = Radiance.radianceRay[i];
                if (ray != null && ray.active && (ray.startPos == pos || ray.endPos == pos))
                {
                    return ray;
                }
            }
            return null;
        }

        #endregion

        #region Ray Methods

        public void Update()
        {
            if (Main.GameUpdateCount % 60 == 0)
                DetectIntersection();
            SnapToPosition(startPos, endPos);
        }
        public void SnapToPosition(Vector2 start, Vector2 end)
        {
            Vector2 objectiveStartPosition = SnapToCenterOfTile(start);
            startPos = Vector2.Lerp(startPos, objectiveStartPosition, 0.5f);

            Vector2 objectiveEndPosition = SnapToCenterOfTile(end);
            endPos = Vector2.Lerp(endPos, objectiveEndPosition, 0.5f);
        }
        
        public void DetectIntersection()
        {

        }
        public void Kill()
        {
            if (!active)
            {
                return;
            }
            active = false;
        }

        #endregion

        #region TagCompound Stuff
        public static readonly Func<TagCompound, RadianceRay> DESERIALIZER = DeserializeData;
        public TagCompound SerializeData()
        {
            return new TagCompound()
            {
                ["StartPos"] = startPos,
                ["EndPos"] = endPos,
                ["Active"] = active
            };
        }

        public static RadianceRay DeserializeData(TagCompound tag)
        {
            RadianceRay radianceRay = new()
            {
                startPos = tag.Get<Vector2>("StartPos"),
                endPos = tag.Get<Vector2>("EndPos"),
                active = tag.Get<bool>("Active")
            };
            return radianceRay;
        }

        #endregion

    }
}