using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            List<HoverUIData> dataToRemove = new List<HoverUIData>();
            foreach (var data in activeHoverData)
            {
                List<HoverUIElement> elementsToRemove = new List<HoverUIElement>();
                foreach (var item in data.elements)
                {
                    if (item.updateTimer)
                    {
                        if (item.timer < HoverUIElement.timerMax)
                            item.timer++;

                        item.updateTimer = false;
                        continue;
                    }
                    if (item.timer > 0)
                        item.timer--; 
                    else
                        elementsToRemove.Add(item);
                }
                data.elements.RemoveAll(elementsToRemove.Contains);
                if (data.elements.All(x => x.timer == 0))
                    dataToRemove.Add(data);
            }
            activeHoverData.RemoveAll(dataToRemove.Contains);
            ////horribly inefficient mess of loops (all in the name of visual polish)
            //foreach (HoverUIData data in currentHoveredObjects)
            //{
            //    HoverUIData oldData = activeHoverData.FirstOrDefault(x => x.entity == data.entity);
            //    if (oldData != null) //handles updating values
            //    {
            //        for (int i = 0; i < Math.Min(data.elements.Count, oldData.elements.Count); i++) //todo: make this not suck
            //        {
            //            data.elements[i].timer = oldData.elements[i].timer;
            //        }
            //        activeHoverData.Remove(oldData);
            //    }
            //    activeHoverData.Add(data);
            //}
            //List<HoverUIData> newList = new List<HoverUIData>();
            //for (int i = 0; i < activeHoverData.Count; i++)
            //{
            //    HoverUIData data = activeHoverData[i];
            //    if (currentHoveredObjects.Any(x => x.entity == data.entity))
            //    {
            //        for (int j = 0; j < data.elements.Count; j++)
            //        {
            //            if (data.elements[j].timer < 20)
            //                data.elements[j].timer++;
            //        }
            //        newList.Add(data);
            //        continue;
            //    }

            //    if (data.elements.Any(x => x.timer > 0))
            //    {
            //        newList.Add(data);
            //        data.elements.ForEach(x => x.timer = Math.Max(0, --x.timer));
            //    }
            //}
            //activeHoverData = newList;
            //currentHoveredObjects.Clear();
        }
    }
}