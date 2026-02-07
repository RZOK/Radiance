namespace Radiance.Content.Items.Tools.Misc
{
    public class MultifacetedLens : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Multifaceted Lens");
            Tooltip.SetDefault("Right click an Apparatus to always be viewing it\nAllows the visibility of Rays to be toggled while in your inventory");
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Green;
        }
    }

    public class MultifacetedLensBuilderToggle : BuilderToggle
    {
        public override bool Active() => Main.LocalPlayer.HasItem(ModContent.ItemType<MultifacetedLens>());

        public override string Texture => $"{nameof(Radiance)}/Content/Items/Tools/Misc/MultifacetedLens_BuilderToggle";
        public override string HoverTexture => $"{nameof(Radiance)}/Content/Items/Tools/Misc/MultifacetedLens_BuilderToggle_Outline";

        public override string DisplayValue()
        {
            string text = "Ray Visibility: ";
            string[] textMessages = new[] { "Off", "On" };

            return text + textMessages[CurrentState];
        }

        public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams)
        {
            switch (CurrentState)
            {
                case 0:
                    drawParams.Color = Color.Gray;
                    break;

                case 1:
                    drawParams.Color = Color.White;
                    break;
            }
            return true;
        }
    }
    public class MultifacetedLensHoverElement : HoverUIElement
    {
        public MultifacetedLensHoverElement() : base("MultifacetedLens") { }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>($"{nameof(Radiance)}/Content/Items/Tools/Misc/MultifacetedLens_HoverUI").Value;
            float scale = Math.Clamp(timerModifier + 0.5f, 0.5f, 1);

            spriteBatch.Draw(tex, realDrawPosition, null, Color.White * timerModifier * 1f, 0, tex.Size() / 2, scale, SpriteEffects.None, 0);
        }
    }
}