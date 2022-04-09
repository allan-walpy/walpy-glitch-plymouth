using System.Collections.Generic;
using System.Linq;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

//! --- data setting ---

const string arg_help_name = "--help";
const string arg_save_folder_name = "--folder";
const string arg_color_name = "--with-color";
const string arg_save_initial_image_name = "--with-initial-image";



const int initial_width = 48;
const int initial_height = 26;
const int final_width = 1920;
const int final_height = 1080;
const double glitchiness = 0.1;
const int color_min_value = ushort.MinValue;
const int color_max_value = ushort.MaxValue / 4;




const int frames = 2;

Dictionary<string, string> args_help = new Dictionary<string, string> {
    { arg_help_name, "this ⇓" },
    { arg_color_name, "make glitches colorfull" },
    { arg_save_folder_name, $"specifiy folder name --default \"frames-{{unix-timestamp}}" },
    { arg_save_initial_image_name, "make saves of original not scaled generated images "}
};

//! --- arguments parsing ---

var arguments = args.ToList(); arguments.RemoveAt(-1);
Console.WriteLine(arguments.Aggregate<string, string>("process://parse --args --", (r, x) => $" {x}"));

if (arguments.Contains(arg_help_name))
{
    Console.WriteLine(
        args_help.Aggregate<KeyValuePair<string, string>, StringBuilder, string>(
            new StringBuilder($"process://output {arg_help_name} --  \"\\ \n"),
            (b, x) => b.Append($"{x.Key} && \"{x.Value} || \\ \n"),
            (b) => b.ToString() + "\" --output end"));
    return;
}


bool is_color_version = arguments.Contains(arg_color_name);
bool do_save_initial_frame = arguments.Contains(arg_save_initial_image_name);
string directory = arguments.ElementAtOrDefault(arguments.IndexOf( arg_save_folder_name))
    ?? $"frames-{DateTimeOffset.Now.ToUnixTimeSeconds()}";


Console.WriteLine("process://start");

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
options.Size = new Size(final_width, final_height);

System.IO.Directory.CreateDirectory(directory);

bool is_normal = false;
Random random = new Random();

for (int i = 0; i < frames; i++)
{
    if (frames < 32 || i % (frames / 32) == 0) Console.WriteLine($"process://frame --number #{i}");
    using (Image<Rgba64> image = new Image<Rgba64>(initial_width, initial_height))
    {

        for (int x = 0; x < initial_width; x++)
            for (int y = 0; y < initial_height; y++)
            {
                is_normal = random.NextDouble() >= glitchiness;


                ushort gray = Convert.ToUInt16(is_normal ? 0 : random.Next(color_min_value, color_max_value));
                image[x, y] =
                    is_color_version ?
                        new Rgba64(gray, gray, gray, Convert.ToUInt16((is_normal ? 0 : random.Next(color_min_value, color_max_value + 1)))) :
                        is_normal ?
                            new Rgba64(0, 0, 0, 0) :
                            new Rgba64(
                                Convert.ToUInt16(random.Next(color_min_value, color_max_value)),
                                Convert.ToUInt16(random.Next(color_min_value, color_max_value)),
                                Convert.ToUInt16(random.Next(color_min_value, color_max_value)),
                                Convert.ToUInt16(random.Next(color_min_value, color_max_value)));
            }
        if (do_save_initial_frame) image.SaveAsPng($"{directory}{Path.PathSeparator}animation-initial-{i:D4}.png", encoder);
        image.Mutate(x => x.Resize(options));
        image.SaveAsPng($"{directory}{Path.PathSeparator}animation-{i:D4}.png", encoder);
    }
}
Console.WriteLine("Done...");
