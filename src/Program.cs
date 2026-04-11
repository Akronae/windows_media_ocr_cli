using System;
using System.CommandLine;
using System.CommandLine.Completions;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;

var fileOption = new Option<string>(
    name: "--file",
    description: "The file to read and display on the console."
);

var stdinOption = new Option<bool>(
    name: "--stdin",
    description: "Read image data from stdin.",
    getDefaultValue: () => false
);

var languageOption = new Option<string>(
    name: "--language",
    description: "The language that should be used during OCR.",
    getDefaultValue: () => "en-US"
);
var modeOption = new Option<OcrOutputMode>(
    name: "--mode",
    description: "The OCR output mode.",
    getDefaultValue: () => OcrOutputMode.json
);

var rootCommand = new RootCommand("Start an OCR analysis using Windows local OcrEngine.")
{
    fileOption,
    stdinOption,
    languageOption,
    modeOption,
};

rootCommand.AddValidator(cmdResult =>
{
    var file = cmdResult.GetValueForOption(fileOption);
    var stdin = cmdResult.GetValueForOption(stdinOption);
    if (string.IsNullOrEmpty(file) && !stdin)
    {
        cmdResult.ErrorMessage = "Either --file or --stdin must be provided.";
    }
});

rootCommand.SetHandler(Handler, fileOption, stdinOption, languageOption, modeOption);

return await rootCommand.InvokeAsync(args);

static async Task Handler(string filepath, bool useStdin, string language, OcrOutputMode mode)
{
    OcrResult result;

    if (useStdin)
    {
        using var memoryStream = new MemoryStream();
        await Console.OpenStandardInput().CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var randomAccessStream = new InMemoryRandomAccessStream();
        await randomAccessStream.WriteAsync(memoryStream.ToArray().AsBuffer());
        randomAccessStream.Seek(0);

        result = await RecognizeAsync(randomAccessStream, language);
    }
    else if (!string.IsNullOrEmpty(filepath))
    {
        var path = Path.GetFullPath(filepath);
        var storageFile = await StorageFile.GetFileFromPathAsync(path);
        using var randomAccessStream = await storageFile.OpenReadAsync();
        result = await RecognizeAsync(randomAccessStream, language);
    }
    else
    {
        // This should be unreachable due to command-line validation
        throw new InvalidOperationException(
            "Unreachable code: either --file or --stdin must be provided."
        );
    }

    var txt = "";

    if (mode == OcrOutputMode.json)
    {
        txt = JsonSerializer.Serialize(result);
    }
    else if (mode == OcrOutputMode.text)
    {
        var sb = new StringBuilder();
        foreach (var l in result.Lines)
        {
            var line = new StringBuilder();

            foreach (var word in l.Words)
            {
                line.Append(word.Text);
                if (!language.Contains("zh"))
                {
                    line.Append(" ");
                }
            }
            sb.Append(line);
            sb.Append(Environment.NewLine);
        }
        txt = sb.ToString();
    }

    Console.WriteLine(txt);
}

static async Task<OcrResult> RecognizeAsync(IRandomAccessStream randomAccessStream, string language)
{
    var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
    using var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
        BitmapPixelFormat.Bgra8,
        BitmapAlphaMode.Premultiplied
    );
    var lang = new Language(language);
    if (OcrEngine.IsLanguageSupported(lang))
    {
        var engine = OcrEngine.TryCreateFromLanguage(lang);
        if (engine != null)
        {
            return await engine.RecognizeAsync(softwareBitmap);
        }
        else
        {
            throw new Exception($"Could not instanciate OcrEngine for language {language}.");
        }
    }
    else
    {
        throw new Exception($"Language {language} is not supported");
    }
}

enum OcrOutputMode
{
    json,
    text,
}
