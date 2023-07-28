using ImageMagick;
using SkiaSharp;
using System;
using System.IO;

namespace Project_Gif_Animation_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            // Fetching Image Folder
            string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Image");

            // Customize these values as needed
            int totalFrames = 30;
            int yMovementPerFrame = 10; // Change this value to adjust the vertical movement of the football
            int delayBetweenFramesMilliseconds = 100; // Adjust this value to change the delay between frames
            bool repeat = true; // Set to true if you want the animation to repeat, false otherwise

            // Load the images
            string playgroundImagePath = Path.Combine(imagesFolder, "Playground.png");// Replace with the actual path
            string footballImagePath = Path.Combine(imagesFolder, "Football.png"); // Replace with the actual path

            if (!File.Exists(playgroundImagePath) || !File.Exists(footballImagePath))
            {
                Console.WriteLine("Error: Image file not found.");
                return;
            }

            using (SKBitmap playgroundBitmap = SKBitmap.Decode(playgroundImagePath))
            {
                if (playgroundBitmap == null)
                {
                    Console.WriteLine("Error: Failed to decode playground image.");
                    return;
                }

                using (SKBitmap footballBitmap = SKBitmap.Decode(footballImagePath))
                {
                    if (footballBitmap == null)
                    {
                        Console.WriteLine("Error: Failed to decode football image.");
                        return;
                    }

                    // Customize the initial position of the football
                    int initialX = (playgroundBitmap.Width - footballBitmap.Width) / 2; // Center the ball horizontally
                    int initialY = playgroundBitmap.Height - footballBitmap.Height; // Bottom of the ground

                    // Create a directory to save animation frames (Optional: Customize the directory path)
                    string framesDirectory = "frames";
                    Directory.CreateDirectory(framesDirectory);

                    // Loop through the frames and create animation
                    for (int frameNumber = 0; frameNumber < totalFrames; frameNumber++)
                    {
                        using (SKBitmap frameBitmap = new SKBitmap(playgroundBitmap.Width, playgroundBitmap.Height))
                        {
                            using (SKCanvas canvas = new SKCanvas(frameBitmap))
                            {
                                canvas.DrawBitmap(playgroundBitmap, 0, 0);

                                // Calculate the Y position of the football for this frame
                                int currentY = initialY - (yMovementPerFrame * frameNumber);

                                // Ensure the football stays within the playground bounds
                                int maxY = playgroundBitmap.Height - footballBitmap.Height;
                                currentY = Math.Max(currentY, 0);
                                currentY = Math.Min(currentY, maxY);

                                canvas.DrawBitmap(footballBitmap, initialX, currentY);
                            }

                            // Save the frame to the frames directory
                            string framePath = Path.Combine(framesDirectory, $"frame_{frameNumber}.png");
                            using (FileStream stream = File.Create(framePath))
                            {
                                frameBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
                            }
                        }

                        // Pause to create the delay between frames
                        System.Threading.Thread.Sleep(delayBetweenFramesMilliseconds);
                    }

                    // Optionally repeat the animation
                    if (repeat)
                    {
                        // Loop back to create the frames in reverse order for the bounce effect
                        for (int frameNumber = totalFrames - 1; frameNumber >= 0; frameNumber--)
                        {
                            using (SKBitmap frameBitmap = new SKBitmap(playgroundBitmap.Width, playgroundBitmap.Height))
                            {
                                using (SKCanvas canvas = new SKCanvas(frameBitmap))
                                {
                                    canvas.DrawBitmap(playgroundBitmap, 0, 0);

                                    // Calculate the Y position of the football for this frame (in reverse)
                                    int currentY = initialY - (yMovementPerFrame * frameNumber);

                                    // Ensure the football stays within the playground bounds
                                    int maxY = playgroundBitmap.Height - footballBitmap.Height;
                                    currentY = Math.Max(currentY, 0);
                                    currentY = Math.Min(currentY, maxY);

                                    canvas.DrawBitmap(footballBitmap, initialX, currentY);
                                }

                                // Save the frame to the frames directory
                                string framePath = Path.Combine(framesDirectory, $"frame_{frameNumber + totalFrames}.png");
                                using (FileStream stream = File.Create(framePath))
                                {
                                    frameBitmap.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
                                }
                            }

                            // Pause to create the delay between frames
                            System.Threading.Thread.Sleep(delayBetweenFramesMilliseconds);
                        }
                    }

                    // Step 3: Combine frames to create the GIF image
                    // Customize the output GIF path
                    string gifOutputPath = "output.gif";

                    // Get the list of all frame paths
                    string[] framePaths = Directory.GetFiles(framesDirectory);

                    // Create a collection to store all the animation frames
                    using (MagickImageCollection magickAnimation = new MagickImageCollection())
                    {
                        foreach (var framePath in framePaths)
                        {
                            using (var frameStream = File.OpenRead(framePath))
                            {
                                var magickImage = new MagickImage(frameStream);
                                magickAnimation.Add(magickImage);
                            }
                        }

                        // Set the looping for the animation
                        magickAnimation[0].AnimationIterations = repeat ? 0 : 1;

                        // Save the GIF animation to the output file
                        magickAnimation.Write(gifOutputPath, MagickFormat.Gif);
                    }

                    // Clean up: delete the temporary frames directory
                    Directory.Delete(framesDirectory, true);

                    Console.WriteLine("Animation completed and saved to GIF file.");
                }
            }
        }
    }
}
