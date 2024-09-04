using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TilingSample;

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
public class TileGenerator
{
    
    /// <summary>
    /// This async variant is perfectly usable for Asp.NET since it is single threaded in nature, but uses async calls to not block threads!
    /// This merely ensures that file-writes are not blocking the threads. Nothing else.
    /// </summary>
    /// <param name="sourceImagePath"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="tileSize"></param>
    /// <returns></returns>
    public Task GenerateTilesAsync(string sourceImagePath, string outputDirectory)
    {
        return GenerateTilesAsync(sourceImagePath, outputDirectory, maxParallelTasks: 1);
    }

    /// <summary>
    /// Golden goose: By default runs in parallel and asynchronous and if needed the degree of parallelism can be reduced
    /// </summary>
    /// <param name="sourceImagePath"></param>
    /// <param name="outputDirectory"></param>
    /// <param name="maxParallelTasks"></param>
    /// <returns></returns>
    public Task GenerateTilesAsync(string sourceImagePath, string outputDirectory, int maxParallelTasks = -1)
    {
        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        using var sourceImageStream = File.OpenRead(sourceImagePath);
        var tileOutputStreamProvider = new Func<string, string, Task<Stream>>((folderName, fileName) =>
            DefaultStreamProvider(Path.Combine(outputDirectory, folderName), fileName));
        return GenerateTilesAsync(sourceImageStream, tileOutputStreamProvider, maxParallelTasks: 1);
    }

    /// <summary>
    /// Golden goose: By default runs in parallel and asynchronous and if needed the degree of parallelism can be reduced
    /// </summary>
    /// <param name="sourceImageStream"></param>
    /// <param name="tileOutputStreamProvider"></param>
    /// <param name="maxParallelTasks"></param>
    /// <returns></returns>
    public async Task GenerateTilesAsync(Stream sourceImageStream, Func<string, string, Task<Stream>> tileOutputStreamProvider, int maxParallelTasks = -1)
    {
        const int tileSize = TileConstants.TileSize;
        using SKBitmap originalBitmap = SKBitmap.Decode(sourceImageStream);
        int originalWidth = originalBitmap.Width;
        int originalHeight = originalBitmap.Height;

        // Determine the maximum zoom level needed
        int maxZoomLevel = (int)Math.Ceiling(Math.Log(Math.Max(originalWidth, originalHeight) / (double)tileSize, 2));

        // List to keep track of all tasks for generating tiles
        var tileCreationTasks = new List<Task>();

        var @lock = new SemaphoreSlim(maxParallelTasks <= 0 ? int.MaxValue : maxParallelTasks);

        // Loop over each zoom level
        for (int zoom = 0; zoom <= maxZoomLevel; zoom++)
        {
            int zoomLevelFactor = (int)Math.Pow(2, zoom);
            int zoomWidth = (int)Math.Ceiling(originalWidth / (double)zoomLevelFactor);
            int zoomHeight = (int)Math.Ceiling(originalHeight / (double)zoomLevelFactor);

            // Ensure the output directory for the current zoom level
            var zoomFolderName = $"z{zoom}";
         

            // Create all tiles in parallel
            for (int x = 0; x < (int)Math.Ceiling(zoomWidth / (double)tileSize); x++)
            {
                for (int y = 0; y < (int)Math.Ceiling(zoomHeight / (double)tileSize); y++)
                {
                    // Create a local copy of the variables to avoid closure issues
                    int localX = x;
                    int localY = y;

                    if (maxParallelTasks == 1)
                    {
                        await CreateTileAsync(originalBitmap, zoomLevelFactor, tileSize, localX, localY,
                            zoomFolderName, @lock, tileOutputStreamProvider);
                    }
                    else
                    {
                        // Add a task to create each tile
                        tileCreationTasks.Add(Task.Run(async () =>
                        {
                            await CreateTileAsync(originalBitmap, zoomLevelFactor, tileSize, localX, localY,
                                zoomFolderName, @lock, tileOutputStreamProvider);
                        }));
                    }
                }
            }
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tileCreationTasks);
    }

    // Asynchronous method to create a single tile
    private async Task CreateTileAsync(SKBitmap originalBitmap, int zoomLevelFactor, int tileSize, int x, int y, string zoomDir, SemaphoreSlim @lock, Func<string, string, Task<Stream>> streamProvider = null)
    {
        await @lock.WaitAsync();

        var p = streamProvider ?? DefaultStreamProvider;

        try
        {
            int originalWidth = originalBitmap.Width;
            int originalHeight = originalBitmap.Height;

            // Calculate the source rectangle in the original image at this zoom level
            int srcX = x * tileSize * zoomLevelFactor;
            int srcY = y * tileSize * zoomLevelFactor;
            int srcWidth = Math.Min(tileSize * zoomLevelFactor, originalWidth - srcX);
            int srcHeight = Math.Min(tileSize * zoomLevelFactor, originalHeight - srcY);

            // Create the tile
            using (SKBitmap tileBitmap = new SKBitmap(tileSize, tileSize))
            using (SKCanvas tileCanvas = new SKCanvas(tileBitmap))
            {
                // Draw the scaled portion of the original image onto the tile
                SKRect destRect = new SKRect(0, 0, tileSize, tileSize);
                SKRect srcRect = new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight);
                tileCanvas.DrawBitmap(originalBitmap, srcRect, destRect);


                using (var memoryStream = new MemoryStream())
                {
                    // Encode the bitmap to the memory stream
                    tileBitmap.Encode(memoryStream, SKEncodedImageFormat.Jpeg, 90); // Save with 90% quality

                    // Reset the memory stream position to the beginning
                    memoryStream.Position = 0;
                    
                    // Write the memory stream to the file asynchronously
                    using(var fs = await p(zoomDir, $"y{y}_x{x}.jpg"))
                    {
                        await memoryStream.CopyToAsync(fs);
                    }
                }
            }
        }
        finally
        {
            @lock.Release(1);
        }
    }


    private static Task<Stream> DefaultStreamProvider(string zoomDir, string tileFileName)
    {
        // Save the tile to disk
        var di = new DirectoryInfo(zoomDir);
        if (!di.Exists) di.Create();

        string tilePath = Path.Combine(zoomDir, tileFileName);
        var fs = new FileStream(tilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        return Task.FromResult<Stream>(fs);
    }
}
