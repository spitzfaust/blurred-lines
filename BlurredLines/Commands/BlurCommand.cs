using System;
using System.IO;
using System.Linq;
using BlurredLines.Calculation;
using BlurredLines.Processing;
using Serilog.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace BlurredLines.Commands
{
    public class BlurCommand : BaseCommand
    {
        private readonly BlurOptions options;

        public BlurCommand(BlurOptions options) : base(options)
        {
            this.options = options;
        }

        public int Run()
        {
            logger.Information("Running blur command with in file path: '{InFilePath}', " +
                               "out file path '{OutFilePath}' " +
                               "and kernel size {KernelSize}.",
                options.InFilePath,
                options.OutFilePath,
                options.KernelSize);
            if (!File.Exists(options.InFilePath))
            {
                logger.Error("There exists no file at the given path '{InFilePath}'.", options.InFilePath);
                return 1;
            }

            if (options.KernelSize < 1)
            {
                logger.Error("Kernel size has to be between 1 and 9 and not {KernelSize}.", options.KernelSize);
                return 1;
            }

            if (options.Sigma < 0)
            {
                logger.Error("Sigma has to be a number > 0.");
                return 1;
            }

            logger.Information("Loading image {InFilePath}.", options.InFilePath);
            using (var image = Image.Load<Rgb24>(options.InFilePath))
            {
                logger.Information("Loaded image {InFilePath} with width {Width} and height {Height}.",
                    options.InFilePath,
                    image.Width,
                    image.Height);

                var gaussianBlur = new GaussianBlur(logger);
                logger.Information("Starting image blur processing.");
                var blurredImage = gaussianBlur.Apply(image, options.KernelSize, options.Sigma);
                logger.Information("Finished image blur processing.");
                
                logger.Debug("Storing image at '{OutputImageLocation}'.", Path.GetFullPath(options.OutFilePath));
                blurredImage.Save(options.OutFilePath);
                logger.Debug("Successfully stored image at '{OutputImageLocation}'.", Path.GetFullPath(options.OutFilePath));
            }

            return 0;
        }
    }
}
