using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

//! --- data setting ---

const string arg_name_help = "--help";
const string arg_name_save_folder = "--folder";
const string arg_name_color = "--with-color";
const string arg_name_save_initial = "--with-initial-frame";
const string arg_name_save_final_not = "--no-final-frame";
const string arg_name_frames = "--frames";
const string arg_name_initial_width = "--width-initial";
const string arg_name_initial_height = "--height-initial";
const string arg_name_final_width = "--width-final";
const string arg_name_final_height = "--height-final";
const string arg_name_glitchiness = "--glitch";
const string arg_name_start_index = " --index";
const string arg_name_color_value_min = "--color-min-value";
const string arg_name_color_value_max = "--color-max-value";
const string arg_name_prefix_final = "--final-prefix-name";
const string arg_name_prefix_initial = "--initial-prefix-name";
const string arg_name_filename_number_pattern = "--filename-digits-pattern";
const int default_frames = 32;
const int default_initial_width = 96;
const int default_initial_height = 54;
const int default_final_width = 1920;
const int default_final_height = 1080;
const double default_glitchiness = 0.1;
const int default_start_index = 0;
const int default_color_value_min = ushort.MinValue;
const int default_color_value_max = ushort.MaxValue / 4;
const string default_prefix_final = "animation";
const string default_prefix_initial = "throbber";
const string default_filename_number_pattern = "D4";


Dictionary<string, string> args_help = new Dictionary<string, string> {
    { arg_name_help, "this ⇓" },
    { arg_name_color, "make glitches colorfull" },
    { arg_name_save_folder, $"specifiy folder name --default \"frames-{{\\unix-timestamp-{DateTime.Now.Second}\\}}" },
    { arg_name_save_initial, "make saves of original not scaled generated images "},
    { arg_name_save_final_not, $"do not resize and saves final image --warrning \"{arg_name_save_initial}\" must be used "},
    { arg_name_frames, $"specify amount of generated frames --default {default_frames}" },
    { arg_name_initial_width, $"specify width of originaly generated image --default {default_initial_width}"},
    { arg_name_initial_height, $"specify height of originaly generated image --default {default_initial_height}"},
    { arg_name_final_width, $"specify width of final image --default {default_final_width}"},
    { arg_name_final_height, $"specify height of final image --default {default_final_height}"},
    { arg_name_glitchiness, $"specify how often glitchy occurs from 0 to 1 --default {default_glitchiness}"},
    { arg_name_start_index, $"specify from which index frame naming starts --default {default_start_index}"},
    { arg_name_color_value_min, $"minimal color brightness from {ushort.MinValue} to {ushort.MaxValue} --default {default_color_value_min}" },
    { arg_name_color_value_max, $"maximum color brightness from {ushort.MinValue} to {ushort.MaxValue} --default {default_color_value_max}"},
    { arg_name_prefix_final, $"filename prefix for final images --default {default_prefix_final}"},
    { arg_name_prefix_initial, $"filename prefix for initialy generated images --default {default_prefix_initial}"},
    { arg_name_filename_number_pattern, $"number pattern in filename --default {default_filename_number_pattern}"}
};

//! --- arguments parsing ---

var arguments = args.ToList();

if (arguments.Contains(arg_name_help))
{
    Console.WriteLine(
        args_help.Aggregate<KeyValuePair<string, string>, StringBuilder, string>(
            new StringBuilder($"process://output {arg_name_help} --  \"\\ \n\n\n"),
            (b, x) => b.Append($"{x.Key} << \"{x.Value} || \\ \n\n"),
            (b) => b.ToString() + "\n\n\n \" --output end"));
    return;
}

Console.WriteLine(arguments.Aggregate<string, string>("process://parse --args <<", (r, x) => r + $" {x}"));
string? GetArgValue(string key) => arguments?.ElementAtOrDefault(arguments?.IndexOf(key) + 1 ?? -1);
int GetArgValueInt(string key, int value) { int r; int.TryParse(GetArgValue(key) ?? value.ToString(), out r); return r; }
UInt16 GetArgValueUInt16(string key, UInt16 value) { UInt16 r; UInt16.TryParse(GetArgValue(key) ?? value.ToString(), out r); return r; }
double GetArgValueDouble(string key, double value) { double r; double.TryParse(GetArgValue(key) ?? value.ToString(), out r); return r; }

bool is_color_version = arguments.Contains(arg_name_color);
bool do_save_initial = arguments.Contains(arg_name_save_initial);
bool do_save_final = !arguments.Contains(arg_name_save_final_not);
string directory = GetArgValue(arg_name_save_folder) ?? $"frames-{DateTimeOffset.Now.ToUnixTimeSeconds()}";
int frames = GetArgValueInt(arg_name_frames, default_frames);
int initial_width = GetArgValueInt(arg_name_initial_width, default_initial_width);
int initial_height = GetArgValueInt(arg_name_initial_height, default_initial_height);
int final_width = GetArgValueInt(arg_name_final_width, default_final_width);
int final_height = GetArgValueInt(arg_name_final_height, default_final_height);
double glitchiness = GetArgValueDouble(arg_name_glitchiness, default_glitchiness);
int index = GetArgValueInt(arg_name_start_index, default_start_index);
int color_value_min = GetArgValueUInt16(arg_name_color_value_min, default_color_value_min);
int color_value_max = GetArgValueUInt16(arg_name_color_value_max, default_color_value_max) + 1;
string final_prefix = GetArgValue(arg_name_prefix_final) ?? default_prefix_final;
string initial_prefix = GetArgValue(arg_name_prefix_initial) ?? default_prefix_initial;
string filename_digits = GetArgValue(arg_name_filename_number_pattern) ?? default_filename_number_pattern;

Console.WriteLine("process://start");

var encoder = new PngEncoder();
encoder.BitDepth = PngBitDepth.Bit16;
encoder.ChunkFilter = PngChunkFilter.None;
encoder.CompressionLevel = PngCompressionLevel.BestCompression;
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

if (!do_save_final && !do_save_initial || frames <= 0) throw new ApplicationException(
    "All frames opted out. Nothing to do",
    (frames <= 0) ? new Exception($"Argument \"{arg_name_frames}\" must be positive")
        : new Exception($"Argument \"{arg_name_save_final_not}\" cannot be used without \"{arg_name_save_initial}\""));

Console.WriteLine("process://start");
System.IO.Directory.CreateDirectory(directory);
bool is_normal = false;
ushort gray = 0;
Random random = new Random();

for (int i = 0; i < frames; i++, index++)
{
    if (frames < 32 || i % (frames / 32) == 0) Console.WriteLine($"process://frame --number #{i}");
    using (Image<Rgba64> image = new Image<Rgba64>(initial_width, initial_height))
    {
        for (int x = 0; x < initial_width; x++)
            for (int y = 0; y < initial_height; y++)
            {
                is_normal = random.NextDouble() >= glitchiness;
                if (is_color_version && !is_normal) gray = Convert.ToUInt16(is_normal ? 0 : random.Next(color_value_min, color_value_max));
                image[x, y] = is_color_version ?
                        new Rgba64(gray, gray, gray, Convert.ToUInt16((is_normal ? 0 : random.Next(color_value_min, color_value_max + 1)))) :
                        is_normal ?
                            new Rgba64(0, 0, 0, 0) :
                            new Rgba64(
                                Convert.ToUInt16(random.Next(color_value_min, color_value_max)),
                                Convert.ToUInt16(random.Next(color_value_min, color_value_max)),
                                Convert.ToUInt16(random.Next(color_value_min, color_value_max)),
                                Convert.ToUInt16(random.Next(color_value_min, color_value_max)));
            }
        string a = Path.Combine(directory, $"{initial_prefix}-{index.ToString(filename_digits)}.png");
        string b = Path.Combine(directory, $"{final_prefix}-{index.ToString(filename_digits)}.png");
        if (do_save_initial) image.SaveAsPng(a, encoder);
        if (do_save_final)
        {
            image.Mutate(x => x.Resize(options));
            image.SaveAsPng(b, encoder);
        }
    }
}
Console.WriteLine("process://shutdown");
