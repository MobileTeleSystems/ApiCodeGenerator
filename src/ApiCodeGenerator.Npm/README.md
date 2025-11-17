# API Code Generator CLI

## Overview

The API Code Generator (ACG) CLI is a command-line tool that generates API client code based on API specifications configured in `.nswag` files. This package provides an easy-to-use interface for automating code generation from OpenAPI/Swagger and AsyncAPI specifications.

## Installation

```bash
npm install -g @mobiletelsystems/api-code-generator
```

Or use locally in your project:

```bash
npm install @mobiletelsystems/api-code-generator
```

## Usage

### Basic Command

```bash
acg [options]
```

The CLI will automatically discover and process `.nswag` configuration files in the current working directory and its subdirectories.

## CLI Options

### `-h, --help`
Display the help message showing all available options and examples.

```bash
acg --help
```

### `-v, --verbose`
Enable verbose logging output. Useful for troubleshooting and understanding the code generation process in detail.

```bash
acg --verbose
```

### `--acg-binary-dir <path>`
Specify the path to the directory containing `ApiCodeGenerator.MSBuild.dll`. Use this to point to a custom location where the generator binaries are installed.

```bash
acg --acg-binary-dir /path/to/binaries
```

### `--acg-binary-site <url>`
Specify a custom URL to download the binaries from. By default, binaries are downloaded from the official repository.

```bash
acg --acg-binary-site https://example.com/binaries
```

### `--no-download-acg-binary`
Disable automatic binary download. Use this flag if you want to use only locally available binaries without attempting to download updates.

```bash
acg --no-download-acg-binary
```

## Configuration and Environment Variables

The CLI supports multiple ways to configure its behavior, in order of precedence (highest to lowest):

1. **Command-line arguments** (highest priority)
2. **Environment variables**
3. **`.acgrc` configuration file**
4. **Default values** (lowest priority)

### Environment Variables

- `ACG_BINARY_DIR` - Path to the directory containing ApiCodeGenerator.MSBuild.dll
- `NO_DOWNLOAD_ACG_BINARY` - Disable automatic binary download (set to any value to enable)
- `ACG_BINARY_SITE` - URL to download binaries from
- `npm_config_acg_binary_dir` - NPM-style configuration for binary directory
- `npm_config_no_download_acg_binary` - NPM-style configuration to disable downloads
- `npm_config_acg_binary_site` - NPM-style configuration for binary site

### Configuration File

The CLI supports a `.acgrc` configuration file in the current working directory. This JSON file allows you to set default options without needing to specify them on the command line every time.

### Configuration Properties

**CLI Options (Binary Management):**
- `acgBinaryDir` - Path to the directory containing ApiCodeGenerator.MSBuild.dll
- `acgBinarySite` - URL to download binaries from
- `noDownloadAcgBinary` - Disable automatic binary download (boolean)

**Code Generation Options:**
- `nswagToolDir` - Path to the folder containing Nswag tools for different .NET versions
- `apiDocumentDir` - Base directory for API documents (if not specified, uses the directory of the .nswag file)
- `extensions` - Array of extension package names to load (e.g., `["acg-preprocessor-jsonpatch"]`)
- `variables` - Global variables object (Record<string, string>) that are used in `.nswag` configuration files to customize code generation
- `apiReferences` - Array of API reference configurations. Each reference must include:
  - `nswag` - Path to the `.nswag` file (relative path from project root)
  - `document` - Path to the API specification file (OpenAPI/AsyncAPI YAML or JSON) - **required if not auto-discovered**
  - `out` - Output file path for generated code - **required if not auto-discovered**
  - `variables` - Variables specific to this API reference (used in `.nswag` configuration, overrides global variables)

### Example `.acgrc`

```json
{
  "acgBinaryDir": "/opt/acg/binaries",
  "acgBinarySite": "https://releases.example.com/acg",
  "noDownloadAcgBinary": false,
  "nswagToolDir": "/opt/nswag/binaries",
  "apiDocumentDir": "openapi",
  "extensions": [
    "acg-preprocessor-jsonpatch"
  ],
  "variables": {
    "clName": "Salsa"
  },
  "apiReferences": [
    {
      "nswag": "migration_api.nswag",
      "document": "openapi/migration_api.yaml",
      "out": "migration_api.g.ts",
      "variables": {
        "clName": "MigrationClient"
      }
    },
    {
      "nswag": "user_api.nswag",
      "document": "openapi/user_api.yaml",
      "out": "user_api.g.ts"
    }
  ]
}
```

### Configuration Notes

- **Property Naming**: All configuration properties use `camelCase` (e.g., `acgBinaryDir`, `noDownloadAcgBinary`, `apiDocumentDir`)
- **Configuration Precedence**: Command-line arguments → Environment variables → `.acgrc` file → Default values
- **Variables in .nswag Files**: Variables defined in `.acgrc` are passed to `.nswag` configuration files and can be referenced within them to customize code generation behavior
- **Global vs. Local Variables**: Global variables are defined at the root level. Per-reference variables in `apiReferences` override global variables for that specific reference
- **Auto-Discovery**: If `document` and `out` paths are not found in `apiReferences`, the CLI will attempt to auto-discover them:
  - **Document**: Searches in `apiDocumentDir` (or `.nswag` file directory if not specified) for matching `.json`, `.yaml`, or `.yml` files
  - **Output**: Defaults to replacing `.nswag` extension with `.g.ts` if not explicitly configured
- **Extension Loading**: Extensions are NPM packages that export a list of DLL paths. Each extension in the `extensions` array should be an installed package name
- **Binary Download**: The CLI attempts to use an installed .NET runtime (8.0 or higher) to execute the binaries. If no .NET runtime is detected, pre-built binaries (which include the .NET framework) are automatically downloaded from the `acgBinarySite`. Set `noDownloadAcgBinary` to `true` to disable automatic downloads (requires .NET runtime to be installed)

## Examples

### Generate code with verbose output
```bash
acg --verbose
```

### Use custom binary directory
```bash
acg --acg-binary-dir ./vendor/acg-binaries
```

### Disable automatic binary downloads
```bash
acg --no-download-acg-binary --acg-binary-dir /usr/local/acg
```

### Combine multiple options
```bash
acg --verbose --acg-binary-dir ./binaries --no-download-acg-binary
```

### Use environment variables
```bash
ACG_BINARY_DIR=/opt/acg/binaries acg --verbose
```

### Configure via `.acgrc` and use environment overrides
```bash
# With .acgrc present:
NO_DOWNLOAD_ACG_BINARY=1 acg  # Overrides acgrc setting
```

## How It Works

1. The CLI resolves configuration from multiple sources (CLI args, environment variables, `.acgrc` file)
2. It detects the runtime identifier (OS and architecture)
3. It checks for available .NET runtimes or downloads pre-built binaries if needed
4. It discovers `.nswag` files in your project and merges them with `apiReferences` configuration
5. For each API reference, it validates required properties (`document`, `out`, `nswag`)
6. It auto-discovers missing `document` paths based on `apiDocumentDir` or the `.nswag` file location
7. For each API reference with a valid document, it generates code using the appropriate Nswag tool
8. It selects the best matching .NET runtime for the code generator and Nswag tool
9. Generated code is output according to your configuration

## For More Information

Visit the project repository: https://github.com/MobileTeleSystems/ApiCodeGenerator
