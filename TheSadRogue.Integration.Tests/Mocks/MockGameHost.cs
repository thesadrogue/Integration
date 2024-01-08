﻿using System;
using System.IO;
using SadConsole;
using SadConsole.Input;
using SadConsole.Renderers;
using SadRogue.Primitives;

namespace SadRogue.Integration.Tests.Mocks
{
    /// <summary>
    /// A basic SadConsole GameHost based on the one built by Thraka in the SadConsole repository that initializes
    /// SadConsole far enough to unit test functionality involving rendering/fonts.
    /// </summary>
    public class MockGameHost : GameHost
    {
        private class Texture : ITexture
        {
            private readonly SixLabors.ImageSharp.Image _graphic;

            public string ResourcePath { get; private set; }

            public int Height => _graphic.Height;

            public int Width => _graphic.Width;

            public int Size => Width * Height;

            public void Dispose()
            {
                _graphic.Dispose();
            }
            public Color GetPixel(Point position) => throw new NotImplementedException();
            public Color GetPixel(int index) => throw new NotImplementedException();
            public Color[] GetPixels() => throw new NotImplementedException();
            public void SetPixels(Color[] colors) => throw new NotImplementedException();

            public void SetPixels(ReadOnlySpan<Color> colors) => throw new NotImplementedException();

            public void SetPixel(Point position, Color color) => throw new NotImplementedException();
            public void SetPixel(int index, Color color) => throw new NotImplementedException();
            public ICellSurface ToSurface(TextureConvertMode mode, int surfaceWidth, int surfaceHeight, TextureConvertBackgroundStyle backgroundStyle = TextureConvertBackgroundStyle.Pixel, TextureConvertForegroundStyle foregroundStyle = TextureConvertForegroundStyle.Block, Color[] cachedColorArray = null, ICellSurface cachedSurface = null) => throw new NotImplementedException();

            public Texture(string path)
            {
                using (Stream fontStream = new FileStream(path, FileMode.Open))
                    _graphic = SixLabors.ImageSharp.Image.Load(fontStream);

                ResourcePath = path;
            }

            public Texture(Stream textureStream)
            {
                _graphic = SixLabors.ImageSharp.Image.Load(textureStream);
                ResourcePath = null!;
            }
        }


        public MockGameHost()
        {
            Instance = this;
            base.LoadDefaultFonts("");
        }

        public override IKeyboardState GetKeyboardState()
        {
            throw new NotImplementedException();
        }

        public override IRenderStep GetRendererStep(string name) => new MockRenderStep();

        public override IMouseState GetMouseState()
        {
            throw new NotImplementedException();
        }

        public override void ResizeWindow(int width, int height, bool resizeOutputSurface = false)
            => throw new NotImplementedException();

        public override ITexture CreateTexture(int width, int height) => throw new NotImplementedException();

        public override IRenderer GetRenderer(string name)
        {
            return null;
        }

        public override ITexture GetTexture(string resourcePath)
        {
            return new Texture(resourcePath);
        }

        public override ITexture GetTexture(Stream textureStream)
        {
            return new Texture(textureStream);
        }

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }
}
