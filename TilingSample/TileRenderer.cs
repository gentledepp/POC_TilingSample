﻿using System;
using System.IO;
using SkiaSharp;

namespace TilingSample
{
    public class TileRenderer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public TileRenderer(int width, int height)
        {
            Width = width;
            Height = height;
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

        public void RenderBitmap(string tileFolderPath, string outputPath, float x, float y, float zoomFactor = 1)
        {
            using var tileBitmap = RenderBitmap(tileFolderPath, x, y, zoomFactor);
           
           
            // Save the tile as a JPEG file
            using var image = SKImage.FromBitmap(tileBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            using var output = File.OpenWrite(outputPath);
            data.SaveTo(output);
        }
    }
}
