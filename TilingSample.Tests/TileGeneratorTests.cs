﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace TilingSample.Tests
{
    public class TileGeneratorTests
    {
        private readonly ITestOutputHelper _output;
        const string SmallFileName = "mountain_4000x1800";
        private const string LargeFileName = "landscape_12000x6000";

        public TileGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
        }

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
        public async Task CanRenderAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = SmallFileName;
            var tileDir = await ArrangeTiles(fileName);

            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x0_y0-z{(1f/zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            rndr.RenderBitmap(tileDir, fn, 0, 0, zoomFactor);
        }

        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        public async Task CanRenderAtImageCenterWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = SmallFileName;
            var tileDir = await ArrangeTiles(fileName);
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
        public async Task CanRenderHugeImageAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = LargeFileName;
            var tileDir = await ArrangeTiles(fileName);
            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"{fileName}x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
        }


        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        [InlineData(0.125f)]
        public async Task CanRenderAsyncAtOriginWithDifferentZoomFactors(float zoomFactor)
        {
            var fileName = SmallFileName;
            var tileDir = await ArrangeTiles(fileName);
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
            var tileDir = await ArrangeTiles(fileName);

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
            var tileDir = await ArrangeTiles(fileName);

            var rndr = new TileRenderer(800, 600);
            var fn = Path.Combine(Environment.CurrentDirectory, $"{fileName}x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);
        }


        [Theory]
        [InlineData(1.0f)]
        [InlineData(0.5f)]
        [InlineData(0.25f)]
        [InlineData(0.125f)]
        public async Task CanRenderAsyncAtOriginWithDifferentZoomFactors_UsingCache(float zoomFactor)
        {
            var fileName = SmallFileName;
            var tileDir = await ArrangeTiles(fileName);

            var factory = new TestCache.Factory();
            using var rndr = new TileRenderer(800, 600, factory);
            var fn = Path.Combine(Environment.CurrentDirectory, $"x0_y0-z{(1f / zoomFactor).ToString(CultureInfo.InvariantCulture)}-rnder{rndr.Width}x{rndr.Height}.jpeg");


            var sw = new Stopwatch();
            sw.Start();

            // first render loads all tiles
            await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

            sw.Stop();
            var nonCached = sw.Elapsed;

            sw.Reset();
            sw.Start();

            // second render already has all tiles => should be magnitudes faster
            await rndr.RenderBitmapAsync(tileDir, fn, 0, 0, zoomFactor);

            sw.Stop();
            var cached = sw.Elapsed;

            _output.WriteLine($"noncached: {nonCached}\ncached:   {cached}");

            cached.ShouldBeLessThan(nonCached);

        }

        private async Task<string> ArrangeTiles(string fileName)
        {
            var file = Path.Combine(Environment.CurrentDirectory, $"{fileName}.jpg");
            var tileDir = Path.Combine(Environment.CurrentDirectory, $"tiles_{fileName}");
            var td = new DirectoryInfo(tileDir);
            if (!td.Exists)
            {
                _output.WriteLine($"tileDir not found - recreating tiles");
                await new TileGenerator().GenerateTilesAsync(file, tileDir, int.MaxValue);
            }
            return tileDir;
        }

        /// <summary>
        /// Note: This is a primitive cache implementation.
        /// Please use IMemoryCache which can also remove entries after some time (e.g 30 seconds) and allows a maximum number of entries (e.g. 12)
        /// Also, in iCL Filler there is a public class `InMemoryCache : IInMemoryCache` which also **Disposes** entries when they are removed! (SKBitmap must be disposed to avoid memory leaks)
        /// </summary>
        private class TestCache : ITileCache
        {
            public class Factory : ITileCacheFactory
            {
                public ITileCache Create()
                {
                    return new TestCache();
                }
            }

            private ConcurrentDictionary<string, SKBitmap> _cache = new();

            public async Task<SKBitmap> GetOrCreateAsync(string zoomFolderName, string tileFileName, Func<string, string, Task<SKBitmap>> tileLoader)
            {
                var key = Path.Combine(zoomFolderName, tileFileName);


                if (_cache.TryGetValue(key, out var bitmap))
                    return bitmap;

                bitmap = await tileLoader(zoomFolderName, tileFileName);
                if (!_cache.TryAdd(key, bitmap))
                {
                    bitmap.Dispose();
                    return _cache[key];
                }

                return bitmap;
            }

            public void Dispose()
            {
                foreach (var x in _cache)
                {
                    x.Value?.Dispose();
                }
            }
        }
    }
}
