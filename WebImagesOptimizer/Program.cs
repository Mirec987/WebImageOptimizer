using ImageOptimizer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

class Program
{
    static void Main()
    {
        Run();
        Console.ReadKey();
    }
    static void Run()
    {
        Config config;
        try
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\config.json");
            string fullConfigPath = Path.GetFullPath(configPath);
            string configContent = File.ReadAllText(fullConfigPath);
            config = JsonSerializer.Deserialize<Config>(configContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return;
        }

        if (config == null || string.IsNullOrWhiteSpace(config.InputDirectory))
        {
            Console.WriteLine("Configuration not loaded properly.");
            return;
        }

        string rootDirectory = Path.Combine(AppContext.BaseDirectory, @"..\..\..");
        string fullRootDirectory = Path.GetFullPath(rootDirectory);

        string inputDirectory = Path.Combine(fullRootDirectory, config.InputDirectory);
        string editedDirectory = Path.Combine(fullRootDirectory, config.EditedDirectory);
        string notEditedDirectory = Path.Combine(fullRootDirectory, config.NotEditedDirectory);
        string thumbnailDirectory = Path.Combine(fullRootDirectory, config.ThumbnailDirectory);

        string fullInputDirectory = Path.GetFullPath(inputDirectory);
        string fullEditedDirectory = Path.GetFullPath(editedDirectory);
        string fullNotEditedDirectory = Path.GetFullPath(notEditedDirectory);
        string fullThumbnailDirectory = Path.GetFullPath(thumbnailDirectory);

        int jpegQuality = config.JpegQuality;
        int thumbnailWidth = config.ThumbnailWidth;
        int thumbnailHeight = config.ThumbnailHeight;

        if (!Directory.Exists(fullInputDirectory))
        {
            Console.WriteLine($"Directory '{fullInputDirectory}' does not exist.");
            return;
        }

        var files = Directory.GetFiles(fullInputDirectory);
        if (files.Length == 0)
        {
            Console.WriteLine($"Directory '{fullInputDirectory}' contains no files.");
            return;
        }

        Directory.CreateDirectory(fullEditedDirectory);
        Directory.CreateDirectory(fullNotEditedDirectory);
        Directory.CreateDirectory(fullThumbnailDirectory);

        int editedCount = 0;
        int notEditedCount = 0;
        long originalSizeTotal = 0;
        long newSizeTotal = 0;

        List<string> unprocessedFiles = new List<string>();

        foreach (var filePath in files)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            originalSizeTotal += fileInfo.Length;

            try
            {
                IImageFormat format;
                using (var stream = File.OpenRead(filePath))
                {
                    format = Image.DetectFormat(stream);
                }

                using (Image image = Image.Load(filePath))
                {
                    bool isEdited = false;

                    if (format.Name == "JPEG")
                    {
                        var jpegMetadata = image.Metadata.GetJpegMetadata();
                        int? quality = jpegMetadata?.Quality;

                        if (quality.HasValue && quality.Value <= jpegQuality)
                        {
                            File.Copy(filePath, Path.Combine(notEditedDirectory, fileInfo.Name));
                            notEditedCount++;
                            continue;
                        }
                    }

                    var options = new JpegEncoder { Quality = jpegQuality };
                    string editedFilePath = Path.Combine(editedDirectory, Path.ChangeExtension(fileInfo.Name, ".jpg"));
                    image.Save(editedFilePath, options);
                    editedCount++;

                    FileInfo newFileInfo = new FileInfo(editedFilePath);
                    newSizeTotal += newFileInfo.Length;
                    isEdited = true;
                }

                using (Image thumbnailImage = Image.Load(filePath))
                {
                    thumbnailImage.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(thumbnailWidth, thumbnailHeight),
                        Mode = ResizeMode.Crop
                    }));

                    string thumbnailName = $"{Path.GetFileNameWithoutExtension(filePath)}_thumb.jpg";
                    string thumbnailPath = Path.Combine(thumbnailDirectory, thumbnailName);
                    thumbnailImage.Save(thumbnailPath, new JpegEncoder { Quality = jpegQuality });
                }
            }
            catch (UnknownImageFormatException)
            {
                unprocessedFiles.Add(fileInfo.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {fileInfo.Name}: {ex.Message}");
            }
        }

        Console.WriteLine($"Number of edited images: {editedCount}");
        Console.WriteLine($"Number of unedited images: {notEditedCount}");
        Console.WriteLine($"Disk space saved: {((originalSizeTotal - newSizeTotal) / 1_048_576):F2} MB");

        if (unprocessedFiles.Count > 0)
        {
            Console.WriteLine("\nUnprocessed files:");
            foreach (var file in unprocessedFiles)
            {
                Console.WriteLine($"- {file}");
            }
        }

        Console.ReadKey();
    }
}
