
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Dull
{
    public static class Program
    {
        public static void Main()
        {

            /*var bmp = new SKBitmap(1920, 1080, SKColorType.Rgba16161616,  SKAlphaType.Unpremul);


            for (int x = 0; x < 1920; x++) //96
            {
                for (int y = 0; y < 1080; y++) //54
                {
                    //if (random.Next() == 0)
                    bmp.SetPixel(x, y, new SKColor(128, 128, 128, 128));
                }
            }
            var filename = ;
            using (var file = File.Create(filename))
            using (var image = SKImage.FromBitmap(bmp))
            {
                var data = image.Encode(SKEncodedImageFormat.Png, 100);
                data.SaveTo(file);
            }*/

            /*var random = new Random();
            var data = new byte[96 * 54 * 4];
            int i = 0;
            for (int x = 0; x < 96; x++)
                for (int y = 0; y < 54; y++, i++)
                {
                    data[i * 4 + 0] = 128;
                    data[i * 4 + 1] = 128;
                    data[i * 4 + 2] = 128;
                    data[i * 4 + 3] = 128;
                }
            var image = Image.Load<Rgba32>(data);*/

            Console.WriteLine("Starting...");
            int width = 48;
            int height = 26;
            double glitvhiness = 0.1;
            ushort max_value = ushort.MaxValue / 2;
            ushort min_value = ushort.MinValue;
            Random random = new Random();
            bool is_normal = false;
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            int frames = 512;
            var encoder = new PngEncoder();
            encoder.BitDepth = PngBitDepth.Bit16;
            encoder.ChunkFilter = PngChunkFilter.None;
            encoder.CompressionLevel = PngCompressionLevel.BestCompression; //NoCompression;
            encoder.ColorType = PngColorType.RgbWithAlpha;
            encoder.FilterMethod = PngFilterMethod.None;
            encoder.InterlaceMethod = PngInterlaceMode.Adam7;
            encoder.TransparentColorMode = PngTransparentColorMode.Clear;
            var options = new ResizeOptions();
            options.Compand = false;
            options.Mode = ResizeMode.Stretch;
            options.PremultiplyAlpha = true;
            options.Sampler = KnownResamplers.NearestNeighbor;
            options.Size = new Size(1920, 1080);

            System.IO.Directory.CreateDirectory($"try-{timestamp}");
            for (int i = 0; i < frames; i++)
            {
                if (frames < 32 || i % (frames / 32) == 0) Console.WriteLine($"Doing frame #{i}");
                using (Image<Rgba64> image = new Image<Rgba64>(width, height))
                {

                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                        {
                            is_normal = random.NextDouble() >= glitvhiness;
                            ushort gray = Convert.ToUInt16(is_normal ? 0 : random.Next(min_value, max_value / 2));
                            image[x, y] = new Rgba64(gray, gray, gray, Convert.ToUInt16((is_normal ? 0 : random.Next(min_value, max_value + 1))));
                            /*is_normal ?
                               new Rgba64(0, 0, 0, 0) :
                               new Rgba64(
                                   Convert.ToUInt16(random.Next(min_value, max_value / 2)),
                                   Convert.ToUInt16(random.Next(min_value, max_value / 2)),
                                   Convert.ToUInt16(random.Next(min_value, max_value / 2)),
                                   Convert.ToUInt16(random.Next(max_value / 2, max_value + 1)));*/
                        }
                    //image.SaveAsPng($"try-{timestamp.ToString()}/noize-{i.ToString()}.png", encoder);
                    image.Mutate(x => x.Resize(options));
                    image.SaveAsPng($"try-{timestamp.ToString()}/animation-{i:0000}.png", encoder);
                }
            }
            Console.WriteLine("Done...");
        }
    }

}
