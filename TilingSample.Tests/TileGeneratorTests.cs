using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace TilingSample.Tests
{
    public class TileGeneratorTests
    {
        const string smallFileName = "mountain_4000x1800";
        private const string largeFileName = "landscape_12000x6000";

        [Fact]
        public void CanCreateTiles()
        {
            var file = Path.Combine(Environment.CurrentDirectory, $"{smallFileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{smallFileName}");

            var gen = new TileGenerator();

            gen.GenerateTiles(file, tileDir);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Fact]
        public void CanCreateTiles_Huge()
        {
            var file = Path.Combine(Environment.CurrentDirectory, $"{largeFileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{largeFileName}");

            var gen = new TileGenerator();

            gen.GenerateTiles(file, tileDir);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        [InlineData(0.125f)]
        public void CanRenderAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = smallFileName;
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x0_y0-z{(1f/zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            rndr.RenderBitmap(tileDir, fn, 0, 0, zoomFactor);
        }

        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        public void CanRenderAtImageCenterWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = smallFileName;
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x1700_y900z-{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

            var cx = (4000 / 2) - (rndr.Width/2);
            var cy = (1800 / 2) - (rndr.Height / 2);

            var x = cx;// * zoomFactor;
            var y = cy;// * zoomFactor;

            rndr.RenderBitmap(tileDir, fn, x, y, zoomFactor);
        }


        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        [InlineData(0.125f)]
        [InlineData(0.0625f)]
        [InlineData(0.03125f)]
        public void CanRenderHugeImageAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = largeFileName;
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"{fileName}x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            rndr.RenderBitmap(tileDir, fn, 0, 0, zoomFactor);
        }
    }
}
