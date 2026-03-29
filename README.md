# windows_media_ocr_cli

> 🔎 Fast OCR CLI for Windows: outputs structured data (bounding rects, text) using the local Windows OCR API

![image](https://github.com/user-attachments/assets/3a832c94-5030-41d8-9454-6869ec7cfcc1)

---

## Features

- OCR image files or image data from stdin
- Outputs results as JSON (with bounding boxes) or plain text
- Supports multiple languages (default: en-US)
- Fast, local processing (no cloud required)
- Simple CLI interface

## Requirements

- Windows 10/11
- .NET Framework 4.8.1 or later
- Windows OCR API support (built-in on modern Windows)

## Installation

Download the latest executable from [Releases](https://github.com/Akronae/windows_media_ocr_cli/releases)

Or build from source:

```sh
git clone https://github.com/Akronae/windows_media_ocr_cli.git
cd windows_media_ocr_cli
dotnet build
```

## Usage

### Basic examples

```sh
# OCR from file
windows_media_ocr_cli.exe --file image.png

# OCR from stdin (pipe image data)
type image.png | windows_media_ocr_cli.exe --stdin
```

### All options

```sh
windows_media_ocr_cli.exe --file <image> [--language <lang>] [--mode <json|text>]
windows_media_ocr_cli.exe --stdin [--language <lang>] [--mode <json|text>]
```

| Option     | Description                                       | Default |
| ---------- | ------------------------------------------------- | ------- |
| --file     | Path to image file                                |         |
| --stdin    | Read image data from stdin                        | false   |
| --language | OCR language (e.g. en-US, fr-FR, zh-CN)           | en-US   |
| --mode     | Output format: json (with bounding boxes) or text | json    |

### Output formats

- **json**: Full OCR result, including bounding rectangles and lines/words.
- **text**: Plain text output (lines joined, no structure).

## Troubleshooting

- Make sure you are running on Windows 10/11 with .NET Framework 4.8.1+ installed.
- If you see errors about missing OCR API, update your Windows system.
- For large images, prefer file input over stdin for performance.

## FAQ

**Q: Can I use this on Linux or macOS?**
A: No, this tool relies on the Windows OCR API.

**Q: How do I specify a different language?**
A: Use `--language <lang>`, e.g. `--language fr-FR`.

**Q: What image formats are supported?**
A: Any format supported by Windows Imaging APIs (PNG, JPEG, BMP, etc).

## License

MIT
