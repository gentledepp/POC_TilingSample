using SkiaSharp;
using System;
using System.IO;
using TilingSample;

public class TileGenerator
{
    /// <summary>
    /// Correct Understanding of Zoom Levels
    /// Here's a corrected understanding of how the zoom levels work:
    /// 
    /// z0: Highest zoom level(maximum detail), with the largest number of tiles.
    ///     z1: One level zoomed out, with fewer tiles (each tile represents a larger portion of the image).
    /// z2: Further zoomed out, with even fewer tiles, and so on.
    ///     Adjusted Tile Rendering Logic
    ///     We should adjust the code so that it correctly uses the zoom level for determining which tiles to load from which folder.The zoom level should match the zoomFactor, where:
    /// 
    /// Zoom Level 0 (z0): corresponds to the highest resolution of the image (zoomFactor = 1).
    /// Zoom Level 1 (z1): corresponds to zoomFactor = 0.5 (image is scaled down by half).
    /// Zoom Level 2 (z2): corresponds to zoomFactor = 0.25, and so on.
    /// </summary>
    /// <param name="sourceImagePath"></param>
    /// <param name="outputDirectory"></param>
    public void GenerateTiles(string sourceImagePath, string outputDirectory)
    {
        // Load the source image
        using var input = File.OpenRead(sourceImagePath);
        using var original = SKBitmap.Decode(input);

        const int tileSize = TileConstants.TileSize;
        // Calculate maximum zoom level based on the largest dimension
        int maxZoomLevel = (int)Math.Ceiling(Math.Log(Math.Max(original.Width, original.Height) / (double)TileConstants.TileSize) / Math.Log(2));

        for (int zoom = 0; zoom <= maxZoomLevel; zoom++)
        {
            int zoomFactor = (int)Math.Pow(2, zoom);
            int width = original.Width / zoomFactor;
            int height = original.Height / zoomFactor;

            // Resize the image to the current zoom level size
            using var scaledBitmap = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);

            // Create the output directory for this zoom level
            string zoomDirectory = Path.Combine(outputDirectory, $"z{zoom}");
            Directory.CreateDirectory(zoomDirectory);

            // Loop through the image to create tiles
            for (int x = 0; x < width; x += tileSize)
            {
                for (int y = 0; y < height; y += tileSize)
                {
                    // Determine the tile dimensions
                    var tileRect = new SKRectI(x, y, Math.Min(x + tileSize, width), Math.Min(y + tileSize, height));
                    using var tileBitmap = new SKBitmap(tileRect.Width, tileRect.Height);
                    using var canvas = new SKCanvas(tileBitmap);

                    // Draw the tile from the scaled image
                    canvas.DrawBitmap(scaledBitmap, tileRect, new SKRect(0, 0, tileRect.Width, tileRect.Height));

                    // Construct the tile's file path
                    var tilePath = Path.Combine(zoomDirectory, $"y{y / tileSize}_x{x / tileSize}.jpg");

                    // Save the tile as a JPEG file
                    using var image = SKImage.FromBitmap(tileBitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
                    using var output = File.OpenWrite(tilePath);
                    data.SaveTo(output);
                }
            }
        }

    }
}
