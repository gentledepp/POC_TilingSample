﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;

namespace TilingSample
{
    public class TileRenderer : IDisposable
    {
        private readonly ITileCache _cache;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileRenderer(int width, int height)
            : this(width,height, null)
        {
        }

        public TileRenderer(int width, int height, ITileCacheFactory cacheFactory)
        {
            Width = width;
            Height = height;
            _cache = cacheFactory?.Create();
        }


        public void RenderBitmap(string tileFolderPath, string outputPath, float x, float y, float zoomFactor = 1)
        {
            using var tileBitmap = RenderBitmap(tileFolderPath, x, y, zoomFactor);


            // Save the tile as a JPEG file
            using var image = SKImage.FromBitmap(tileBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            using var output = File.OpenWrite(outputPath);
            data.SaveTo(output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileFolderPath"></param>
        /// <param name="offsetX">Horizontal offset in original size pixels</param>
        /// <param name="offsetY">Vertical offset in original size pixels</param>
        /// <param name="zoomFactor"> Example zoom factor: 1.0 means fully zoomed in at highest detail (z0), 0.5 means one level zoomed out (z1), etc.</param>
        /// <returns></returns>
        public SKBitmap RenderBitmap(string tileFolderPath, float offsetX, float offsetY, float zoomFactor = 1)
        {
            var tileSize = TileConstants.TileSize;

            SKBitmap bitmap = new SKBitmap(Width, Height);
            using var canvas = new SKCanvas(bitmap);
            // Calculate the appropriate zoom level from the zoom factor
            // Higher zoomFactor corresponds to lower zoom level folder (z0, z1, etc.)
            int zoomLevel = (int)Math.Round(Math.Log(1f / zoomFactor, 2)); // Correct zoom level calculation
            
            // Calculate the size of each tile at the current zoom level
            float tileSizeAtZoom = tileSize * zoomFactor;

            // Apply translation to the canvas to handle panning (move the viewport)
            canvas.Translate(-offsetX*zoomFactor, -offsetY* zoomFactor);

            // Apply scaling to the canvas to handle zoom
            canvas.Scale(1f / zoomFactor);

            var offsetXAtZoom = offsetX * zoomFactor;
            var offsetYAtZoom = offsetY * zoomFactor;
            var widthAtZoom = canvas.LocalClipBounds.Width / zoomFactor;
            var heightAtZoom = canvas.LocalClipBounds.Height / zoomFactor;

            int startTileX = (int)Math.Floor(offsetXAtZoom / tileSize);
            int endTileX = (int)Math.Ceiling((offsetXAtZoom + widthAtZoom) / tileSize);
            int startTileY = (int)Math.Floor(offsetYAtZoom / tileSize);
            int endTileY = (int)Math.Ceiling((offsetYAtZoom + heightAtZoom) / tileSize);


            // Loop through the visible range of tiles and draw them
            for (int tileX = startTileX; tileX < endTileX; tileX++)
            {
                for (int tileY = startTileY; tileY < endTileY; tileY++)
                {
                    // Local copies of tile indices for use inside the Task
                    int localTileX = tileX;
                    int localTileY = tileY;
                    // Construct the file path to the tile using the new naming format
                    string tilePath = Path.Combine(tileFolderPath, $"z{zoomLevel}", $"y{tileY}_x{tileX}.jpg");

                    // Check if the tile file exists
                    if (File.Exists(tilePath))
                    {
                        // Load the tile bitmap
                        using var tileBitmap = SKBitmap.Decode(tilePath);

                        var t = tileSizeAtZoom;
                        // Calculate the position to draw the tile on the canvas
                        float drawX = tileX * t;
                        float drawY = tileY * t;

                        var area = new SKRect(drawX, drawY, drawX + t, drawY + t);

                        // we can check if the area we are drawing is actually within the local clip bounds
                        var isVisible = canvas.LocalClipBounds.IntersectsWith(area);
                        if (isVisible)
                        {
                            // Draw the tile on the canvas
                            canvas.DrawBitmap(tileBitmap, area);
                        }
                    }
                }
            }

            return bitmap;
        }

        public async Task RenderBitmapAsync(string tileFolderPath, string outputPath, float x, float y, float zoomFactor = 1)
        {

            var tileProvider = new Func<string, string, Task<Stream>>((folder, fileName) =>
                LoadTileStreamAsync(Path.Combine(tileFolderPath, folder), fileName));

            using var tileBitmap = await RenderBitmapAsync(tileProvider, x, y, zoomFactor);


            // Save the tile as a JPEG file
            using var image = SKImage.FromBitmap(tileBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            using var output = File.OpenWrite(outputPath);
            data.SaveTo(output);
        }

        public async Task<SKBitmap> RenderBitmapAsync(Func<string,string, Task<Stream>> tileProvider, float offsetX, float offsetY, float zoomFactor = 1)
        {
            var tileSize = TileConstants.TileSize;

            SKBitmap bitmap = new SKBitmap(Width, Height);
            using var canvas = new SKCanvas(bitmap);
            // Calculate the appropriate zoom level from the zoom factor
            // Higher zoomFactor corresponds to lower zoom level folder (z0, z1, etc.)
            int zoomLevel = (int)Math.Round(Math.Log(1f / zoomFactor, 2)); // Correct zoom level calculation

            // Calculate the size of each tile at the current zoom level
            float tileSizeAtZoom = tileSize * zoomFactor;

            // Apply translation to the canvas to handle panning (move the viewport)
            canvas.Translate(-offsetX * zoomFactor, -offsetY * zoomFactor);

            // Apply scaling to the canvas to handle zoom
            canvas.Scale(1f / zoomFactor);

            var offsetXAtZoom = offsetX * zoomFactor;
            var offsetYAtZoom = offsetY * zoomFactor;
            var widthAtZoom = canvas.LocalClipBounds.Width / zoomFactor;
            var heightAtZoom = canvas.LocalClipBounds.Height / zoomFactor;

            int startTileX = (int)Math.Floor(offsetXAtZoom / tileSize);
            int endTileX = (int)Math.Ceiling((offsetXAtZoom + widthAtZoom) / tileSize);
            int startTileY = (int)Math.Floor(offsetYAtZoom / tileSize);
            int endTileY = (int)Math.Ceiling((offsetYAtZoom + heightAtZoom) / tileSize);

            // List to hold all tasks for rendering tiles
            var tileLoadTasks = new List<Task<(float x,float y, SKBitmap tileBitmap)>>();

            // Loop through the visible range of tiles and render them asynchronously
            for (int tileX = startTileX; tileX < endTileX; tileX++)
            {
                for (int tileY = startTileY; tileY < endTileY; tileY++)
                {
                    // Local copies of tile indices for use inside the Task
                    int localTileX = tileX;
                    int localTileY = tileY;

                    // Start a new task to load and render each tile
                    tileLoadTasks.Add(Task.Run(async () =>
                    {
                        var bmp = await LoadTileAsync($"z{zoomLevel}", $"y{localTileY}_x{localTileX}.jpg", tileProvider);
                        // Calculate the position to draw the tile on the canvas
                        float drawX = localTileX * tileSizeAtZoom;
                        float drawY = localTileY * tileSizeAtZoom;

                        return (x: drawX, y: drawY, tileBitmap: bmp);
                    }));
                }
            }

            while (tileLoadTasks.Any())
            {
                var tileLoadTask = await Task.WhenAny(tileLoadTasks);
                tileLoadTasks.Remove(tileLoadTask);

                var (x,y,tileBitmap) = await tileLoadTask;

                if (tileBitmap != null)
                {
                    try
                    {
                        // Ensure the draw area is within the visible portion of the canvas
                        var area = new SKRect(x, y, x + tileSizeAtZoom, y + tileSizeAtZoom);
                        bool isVisible = canvas.LocalClipBounds.IntersectsWith(area);

                        if (isVisible)
                        {
                            // Draw the tile on the canvas
                            canvas.DrawBitmap(tileBitmap, area);
                     
                        }
                    }
                    finally
                    {
                        if (_cache is null)
                            tileBitmap.Dispose();
                    }
                }
            }

            return bitmap;
        }

        private async Task<Stream> LoadTileStreamAsync(string zoomFolderName, string tileFileName)
        {
            var tilePath = Path.Combine(zoomFolderName, tileFileName);

            if (!File.Exists(tilePath))
            {
                return null;
            }

            using FileStream fs = new FileStream(tilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            var memoryStream = new MemoryStream();

            // Asynchronously copy the file content to the memory stream
            await fs.CopyToAsync(memoryStream);

            // Decode the bitmap from the memory stream
            memoryStream.Position = 0;
            return memoryStream;
        }

        // Asynchronous method to load a tile from disk
        private async Task<SKBitmap> LoadTileAsync(string zoomFolderName, string tileFileName, Func<string, string, Task<Stream>> tileProvider)
        {
            if (_cache is { } cache)
            {
                return await cache.GetOrCreateAsync(zoomFolderName, tileFileName,
                    (zfn, fn) => LoadTile(zfn, fn, tileProvider));
            }

            return await LoadTile(zoomFolderName, tileFileName, tileProvider);
        }

        private static async Task<SKBitmap> LoadTile(string zoomFolderName, string tileFileName, Func<string, string, Task<Stream>> tileProvider)
        {
            using var stream = await tileProvider.Invoke(zoomFolderName, tileFileName);
            if (stream is null)
                return null;
            return SKBitmap.Decode(stream);
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }
    }

    public interface ITileCache : IDisposable
    {
        Task<SKBitmap> GetOrCreateAsync(string zoomFolderName, string tileFileName,
            Func<string, string, Task<SKBitmap>> tileLoader);
    }

    public interface ITileCacheFactory
    {
        ITileCache Create();
    }
}
