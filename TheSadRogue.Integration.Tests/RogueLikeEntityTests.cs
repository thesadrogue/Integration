using SadRogue.Integration.Keybindings;
using SadRogue.Primitives;
using Xunit;

namespace SadRogue.Integration.Tests
{
    public class RogueLikeEntityTests
    {
        [Fact]
        public void NewFromColorsAndGlyphTest()
        {
            var entity = new RogueLikeEntity((0,0), Color.Chartreuse, Color.Salmon, '1');

            Assert.Equal(Color.Chartreuse, entity.AppearanceSingle!.Appearance.Foreground);
            Assert.Equal(Color.Salmon, entity.AppearanceSingle!.Appearance.Background);
            Assert.Equal('1', entity.AppearanceSingle!.Appearance.Glyph);
            Assert.Equal(new Point(0,0), entity.Position);
            Assert.True(entity.IsWalkable); //the default
            Assert.True(entity.IsTransparent); //the default
        }

        [Fact]
        public void NewFromPositionAndGlyphTest()
        {
            var entity = new RogueLikeEntity((1,1), 2);
            Assert.Equal(Color.White, entity.AppearanceSingle!.Appearance.Foreground);
            Assert.Equal(Color.Transparent, entity.AppearanceSingle!.Appearance.Background);
            Assert.Equal(2, entity.AppearanceSingle!.Appearance.Glyph);
            Assert.Equal(new Point(1,1), entity.Position);
        }

        [Fact]
        public void NewFromPositionColorAndGlyphTest()
        {
            var entity = new RogueLikeEntity((1,3), Color.Cyan, 2);
            Assert.Equal(Color.Cyan, entity.AppearanceSingle!.Appearance.Foreground);
            Assert.Equal(Color.Transparent, entity.AppearanceSingle!.Appearance.Background);
            Assert.Equal(2, entity.AppearanceSingle!.Appearance.Glyph);
            Assert.Equal(new Point(1,3), entity.Position);
        }
        [Fact]
        public void AddComponentTest()
        {
            var entity = new RogueLikeEntity((1,3), Color.Cyan, 2);
            var component = new KeybindingsComponent();

            Assert.Empty(component.Motions);
            Assert.Empty(component.Actions);

            entity.AllComponents.Add(component);

            Assert.Single(entity.SadComponents);
            Assert.Single(entity.GoRogueComponents);
        }
    }
}
