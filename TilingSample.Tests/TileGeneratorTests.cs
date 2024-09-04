using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TilingSample.Tests
{
    public class TileGeneratorTests
    {
        const string SmallFileName = "mountain_4000x1800";
        private const string LargeFileName = "landscape_12000x6000";

        [Fact]
        public async Task L_CanCreateTiles()
        {
            var file = Path.Combine(Environment.CurrentDirectory, $"{SmallFileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{SmallFileName}");
            var td = new DirectoryInfo(tileDir);
            if (td.Exists)
                td.Delete(true);

            var gen = new TileGenerator();

            await gen.GenerateTilesAsync(file, tileDir);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Fact]
        public async Task XL_CanCreateTiles()
        {
            var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
            var td = new DirectoryInfo(tileDir);
            if (td.Exists)
                td.Delete(true);

            var gen = new TileGenerator();

            await gen.GenerateTilesAsync(file, tileDir);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Fact]
        public async Task XL_CanCreateTilesAsync()
        {

            var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
            var td = new DirectoryInfo(tileDir);
            if(td.Exists)
                td.Delete(true);

            var gen = new TileGenerator();

            await gen.GenerateTilesAsync(file, tileDir);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Fact]
        public async Task XL_CanCreateTilesAsyncInParallel()
        {
            var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
            var td = new DirectoryInfo(tileDir);
            if (td.Exists)
                td.Delete(true);

            var gen = new TileGenerator();

            await gen.GenerateTilesAsync(file, tileDir, -1);

            var tiles = Directory.EnumerateFiles(tileDir, "*.*", SearchOption.AllDirectories);
            Assert.True(tiles.Any());
        }

        [Fact]
        public async Task XL_CanCreateTilesAsyncInParallel_LimitingParallelizationToOne()
        {
            var file = Path.Combine(Environment.CurrentDirectory, $"{LargeFileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{LargeFileName}");
            var td = new DirectoryInfo(tileDir);
            if (td.Exists)
                td.Delete(true);

            var gen = new TileGenerator();

            await gen.GenerateTilesAsync(file, tileDir, 1);

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
            var fileName = SmallFileName;
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
            var fileName = SmallFileName;
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
            var fileName = LargeFileName;
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"{fileName}x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            rndr.RenderBitmap(tileDir, fn, 0, 0, zoomFactor);
        }


        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        [InlineData(0.125f)]
        public async Task CanRenderAsyncAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = SmallFileName;
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
        }

        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        public async Task CanRenderAsyncAtImageCenterWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = SmallFileName;
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");

            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x1700_y900z-{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");

            var cx = (4000 / 2) - (rndr.Width / 2);
            var cy = (1800 / 2) - (rndr.Height / 2);

            var x = cx;// * zoomFactor;
            var y = cy;// * zoomFactor;

            await rndr.RenderBitmapAsync(tileDir, fn, x, y, zoomFactor);
        }


        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        [InlineData(0.125f)]
        [InlineData(0.0625f)]
        [InlineData(0.03125f)]
        public async Task CanRenderAsyncHugeImageAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = LargeFileName;
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");

            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"{fileName}x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
        }
    }
}
