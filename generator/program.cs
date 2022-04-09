
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

Console.WriteLine("Starting...");
int width = 48;
int height = 26;
double glitchiness = 0.1;
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

System.IO.Directory.CreateDirectory($"frames-{timestamp}");
for (int i = 0; i < frames; i++)
{
    if (frames < 32 || i % (frames / 32) == 0) Console.WriteLine($"Doing frame #{i}");
    using (Image<Rgba64> image = new Image<Rgba64>(width, height))
    {

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                is_normal = random.NextDouble() >= glitchiness;
                ushort gray = Convert.ToUInt16(is_normal ? 0 : random.Next(min_value, max_value / 2));
                image[x, y] = new Rgba64(gray, gray, gray, Convert.ToUInt16((is_normal ? 0 : random.Next(min_value, max_value + 1))));

                //? for color use :
                /*
                image[x, y] = is_normal ?
                   new Rgba64(0, 0, 0, 0) :
                   new Rgba64(
                       Convert.ToUInt16(random.Next(min_value, max_value + 1)),
                       Convert.ToUInt16(random.Next(min_value, max_value + 1)),
                       Convert.ToUInt16(random.Next(min_value, max_value + 1)),
                       Convert.ToUInt16(random.Next(min_value, max_value + 1)));
                */
            }
        image.Mutate(x => x.Resize(options));
        image.SaveAsPng($"frames-{timestamp.ToString()}/animation-{i:0000}.png", encoder);
    }
}
Console.WriteLine("Done...");
