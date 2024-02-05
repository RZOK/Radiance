using Radiance.Content.UI.NewEntryAlert;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Terraria.UI;

namespace Radiance.Core.Loaders
{
    //robbed from slr
    class UILoader : ModSystem
    {
        public static List<UserInterface> UserInterfaces;
        public static List<SmartUIState> UIStates;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Mod Mod = Radiance.Instance;

            UserInterfaces = new List<UserInterface>();
            UIStates = new List<SmartUIState>();

            foreach (Type t in Mod.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(SmartUIState)) && !t.IsAbstract)
                {
                    var state = (SmartUIState)Activator.CreateInstance(t, null);
                    var userInterface = new UserInterface();
                    userInterface.SetState(state);

                    UIStates?.Add(state);
                    UserInterfaces?.Add(userInterface);
                    
                }
            }
        }

        public override void Unload()
        {
            if (Main.dedServ)
                return;

            UIStates.ForEach(n => n.Unload());
            UserInterfaces = null;
            UIStates = null;
        }

        public static void AddLayer(List<GameInterfaceLayer> layers, UserInterface userInterface, UIState state, int index, bool visible, InterfaceScaleType scale)
        {
            string name = state == null ? "Unknown" : state.ToString();
            layers.Insert(index, new LegacyGameInterfaceLayer("Radiance: " + name,
                delegate
                {
                    if (visible)
                    {
                        userInterface.Update(Main._drawInterfaceGameTime);
                        state.Draw(Main.spriteBatch);
                    }
                    return true;
                }, scale));
        }

        public static T GetUIState<T>() where T : SmartUIState => UIStates.FirstOrDefault(n => n is T) as T;

        public static void ReloadState<T>() where T : SmartUIState
        {
            var index = UIStates.IndexOf(GetUIState<T>());
            UIStates[index] = (T)Activator.CreateInstance(typeof(T), null);
            UserInterfaces[index] = new UserInterface();
            UserInterfaces[index].SetState(UIStates[index]);
        }
    }

    class AutoUISystem : ModSystem
    {
        public static float MapHeight;
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            MapHeight = 0;
            if (Main.mapEnabled)
            {
                if (!Main.mapFullscreen && Main.mapStyle == 1)
                {
                    MapHeight = 256;
                }

                if (MapHeight + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight)
                    MapHeight = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
            }
            for (int k = 0; k < UILoader.UIStates.Count; k++)
            {
                var state = UILoader.UIStates[k];
                UILoader.AddLayer(layers, UILoader.UserInterfaces[k], state, state.InsertionIndex(layers), state.Visible, state.Scale);
            }
        }
    }
}
