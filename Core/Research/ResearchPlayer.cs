using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.EncycloradiaEntries;
using Radiance.Core.Research.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ModLoader;
using static Radiance.Core.Encycloradia.EncycloradiaSystem;
using static Radiance.Core.Research.ResearchHandler;
namespace Radiance.Core.Research
{
    public class ResearchPlayer : ModPlayer
    {
        public Vector2? tablePosition;
        public ResearchBoard activeBoard;
        public override void Load()
        {
            RenderTargetsManager.ResizeRenderTargetDelegateEvent += ResizeRenderTarget;
            RenderTargetsManager.DrawToRenderTargetsDelegateEvent += ResearchTable.DrawComponentsToTarget;

            foreach (Type t in Mod.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(ResearchComponent)) && !t.IsAbstract)
                {
                    var component = (ResearchComponent)RuntimeHelpers.GetUninitializedObject(t);
                    Language.GetOrRegister(component.GetNameLocalizedText().Key);
                    Language.GetOrRegister(component.GetTooltipLocalizedText().Key);
                }
            }

            ResizeRenderTarget();
        }
        private void ResizeRenderTarget()
        {
            Main.QueueMainThreadAction(() =>
            {
                if (ResearchTable.componentTarget != null && !ResearchTable.componentTarget.IsDisposed)
                    ResearchTable.componentTarget.Dispose();

                ResearchTable.componentTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            });
        }
        public override void ResetEffects()
        {
            List<ResearchComponent> researchComponents = new List<ResearchComponent>() { 
                new MirrorComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0),
            new MirrorComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0),
            new MirrorComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0),
            new MirrorComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0), new BeamSpawnerComponent(Vector2.Zero, 0) };

            //activeBoard = new ResearchBoard() { availableComponents = new List<ResearchComponent>(researchComponents), components = new List<ResearchComponent>() };
            foreach (ResearchComponent component in activeBoard.availableComponents)
            {
                component.playerPlaced = true;
                component.inDrawer = true;
                if (ResearchUI.Instance.researchTable.heldComponent != component)
                    component.rotation = 0;
            }
        }
        public override void UpdateDead()
        {
            tablePosition = null;
        }
    }
}