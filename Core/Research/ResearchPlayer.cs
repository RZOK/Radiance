using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Radiance.Content.EncycloradiaEntries;
using Radiance.Core.Research.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            activeBoard = new ResearchBoard() { availableComponents = new List<ResearchComponent>(researchComponents) };
        }
        public override void UpdateDead()
        {
            tablePosition = null;
        }
    }
}