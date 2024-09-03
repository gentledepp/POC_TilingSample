using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace TilingSample.Tests
{
    public class TileGeneratorTests
    {
        [Fact]
        public void CanCarateTiles()
        {
            var file = Path.Combine(Environment.CurrentDirectory, "high-resolution-mountains-and-lakes-vast-xl.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, "tiles");

            var gen = new TileGenerator();

            gen.GenerateTiles(file, tileDir);

            Assert.True(Directory.EnumerateFiles(tileDir).Any());
        }


        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        public void CanRenderAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var tileDir = Path.Combine(Environment.CurrentDirectory, "tiles");
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x0_y0-{zoomFactor.ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            rndr.RenderBitmap(tileDir, fn, 0, 0, zoomFactor);
        }

        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        public void CanRenderAtImageCenterWithDifferentZoomFactors(float zoomFactor)
        {
            var tileDir = Path.Combine(Environment.CurrentDirectory, "tiles");
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x1700_y900-{zoomFactor.ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            rndr.RenderBitmap(tileDir, fn, 1700, 900, zoomFactor);
        }
    }
}
