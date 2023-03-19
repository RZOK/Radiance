using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Radiance.Core
{
    public class RadianceInterfacePlayer : ModPlayer
    {
        public List<HoverUIData> currentHoveredObjects = new List<HoverUIData>();
        public List<HoverUIData> activeHoverData = new List<HoverUIData>();

        public float newEntryUnlockedTimer = 0;
        public string incompleteEntryText = string.Empty;

        public override void ResetEffects()
        {
            incompleteEntryText = string.Empty;
        }

        public override void PostUpdate()
        {
            //horribly inefficient mess of loops (all in the name of visual polish)
            foreach (HoverUIData data in currentHoveredObjects)
            {
                HoverUIData oldData = activeHoverData.FirstOrDefault(x => x.entity == data.entity);
                if (oldData != null) //handles updating values
                {
                    for (int i = 0; i < Math.Min(data.elements.Count, oldData.elements.Count); i++) //todo: make this not suck
                    {
                        data.elements[i].timer = oldData.elements[i].timer;
                    }
                    activeHoverData.Remove(oldData);
                }
                activeHoverData.Add(data);
            }
            List<HoverUIData> newList = new List<HoverUIData>();
            for (int i = 0; i < activeHoverData.Count; i++)
            {
                HoverUIData data = activeHoverData[i];
                if (currentHoveredObjects.Any(x => x.entity == data.entity))
                {
                    for (int j = 0; j < data.elements.Count; j++)
                    {
                        if (data.elements[j].timer < 20)
                            data.elements[j].timer++;
                    }
                    newList.Add(data);
                    continue;
                }

                if (data.elements.Any(x => x.timer > 0))
                {
                    newList.Add(data);
                    data.elements.ForEach(x => x.timer = Math.Max(0, --x.timer));
                }
            }
            activeHoverData = newList;
            currentHoveredObjects.Clear();
        }
    }
}