import * as proc from "child_process";
import * as fs from "fs";
import * as path from "path";
import * as os from "os";
import * as http from "http";
import * as https from "https";

const dotnetCmd = "dotnet";
const defaultBinaries = path.join(__dirname, "binaries");
const netRuntimeRegex = /^Microsoft\.NETCore\.App\s(\S+)\s\[[^\]]+\]$/gm;
const acgToolName = "ApiCodeGenerator.MSBuild";

export interface AcgRuntimeConfig {
  acgBinaryDir?: string;
  noDownloadAcgBinary?: boolean;
  acgBinarySite?: string;
  nswagToolDir?: string;
  apiDocumentDir?: string;
  apiReferences?: ApiReference[];
  extensions?: string[];
  variables?: Record<string, string>;
}

export interface Options {
  acgBinaryDir?: string | undefined;
  noDownloadBinary?: boolean;
  acgBinarySite?: string;
}

export interface ApiReference {
  nswag: string;
  document?: string;
  out?: string;
  variables?: Record<string, string>
}

export interface NswagRuntime {
  rt: string;
  dir: string;
}

export interface ToolsInfo {
  cmd: string;
  cmdArgs: string[];
  nswagTool: string;
  extensions: string[];
}

export interface DownloadBinariesResult {
  acgDir: string;
  executable: string;
  args: string[];
}

class AcgError extends Error {
  cause?: Error;

  constructor(message: string, options?: { cause?: Error }) {
    super(message);
    this.name = "AcgError";
    if (options?.cause) {
      this.cause = options.cause;
    }
  }
}

let verboseLog = false;

/**
 * Outputs verbose log messages to console if verbose mode is enabled.
 * @param arg - Arguments to log to console
 */
function verbose(...arg: any[]) {
  if (verboseLog) {
    console.log(...arg);
  }
}

/**
 * Enables verbose logging mode for debugging purposes.
 */
function enableVerbose() {
  verboseLog = true;
}

/**
 * Gets the ACG binary directory from configuration, environment variables, or npm config.
 * @param rc - Runtime configuration object
 * @returns The ACG binary directory path or undefined if not configured
 */
function getAcgBinaryDir(rc: AcgRuntimeConfig): string | undefined {
  return (
    process.env.ACG_BINARY_DIR ||
    rc.acgBinaryDir ||
    process.env.npm_config_acg_binary_dir
  );
}

/**
 * Determines whether binary download should be skipped based on configuration and environment variables.
 * @param rc - Runtime configuration object
 * @returns True if binary download should be skipped, false otherwise
 */
function getNoDownloadBinary(rc: AcgRuntimeConfig): boolean {
  return !!(
    process.env.NO_DOWNLOAD_ACG_BINARY ||
    rc.noDownloadAcgBinary ||
    process.env.npm_config_no_download_acg_binary
  );
}

/**
 * Gets the URL for downloading ACG binaries from configuration, environment variables, or defaults to GitHub releases.
 * @param rc - Runtime configuration object
 * @returns The URL of the binary site
 */
function getAcgBinarySite(rc: AcgRuntimeConfig): string {
  const site =
    process.env.ACG_BINARY_SITE ||
    rc.acgBinarySite ||
    process.env.npm_config_acg_binary_site ||
    "https://github.com/MobileTeleSystems/ApiCodeGenerator/releases/download";

  const siteNormalized = site.endsWith("/") ? site.slice(0, -1) : site;
  return siteNormalized;
}

/**
 * Determines the Runtime Identifier (RID) for the current operating system and architecture.
 * @returns Runtime identifier string (e.g., 'win-x64', 'linux-x64', 'osx-arm64')
 * @throws {AcgError} If OS or architecture is not supported
 */
function getRuntimeIdentifier(): string {
  const osMap: Record<string, string> = {
    win32: "win",
    linux: "linux",
    darwin: "osx",
  };

  const archMap: Record<string, string> = {
    x64: "x64",
    ia32: "x86",
    arm64: "arm64",
  };

  const osName = osMap[os.platform()];
  const osArch = archMap[os.arch()];

  if (!osName || !osArch) {
    throw new AcgError("Unsupported OS or architecture");
  }

  let detectLibc: any;
  try {
    detectLibc = require("detect-libc");
  } catch {
    detectLibc = null;
  }

  const osSuffix =
    osName === "linux" && detectLibc?.isNonGlibcLinuxSync?.()
      ? "-musl"
      : "";
  return `${osName}${osSuffix}-${osArch}`;
}

/**
 * Downloads and extracts ACG binaries from the specified site.
 * @param binarySite - URL of the site to download binaries from
 * @param rid - Runtime identifier for the target platform
 * @param pkgVersion - Package version to download
 * @param destination - Optional destination directory for extracted binaries
 * @returns Result object containing the binary directory, executable path, and arguments
 * @throws {AcgError} If download or extraction fails
 */
async function downloadBinaries(
  binarySite: string,
  rid: string,
  pkgVersion: string,
  destination?: string
): Promise<DownloadBinariesResult> {
  const targetDir = destination || path.join(defaultBinaries, rid);

  const dllPath = path.join(targetDir, acgToolName + ".dll");
  if (!fs.existsSync(dllPath)) {
    const url = [binarySite, "v" + pkgVersion, rid + ".tgz"].join("/");
    console.log(
      "Download binaries from '" + url + "' to '" + targetDir + "'"
    );
    if (!fs.existsSync(targetDir))
      fs.mkdirSync(targetDir, { recursive: true });

    const zlib = require("zlib");
    const tar = require("tar");
    const { pipeline } = require("stream/promises");

    const downloadAndExtract = async (location: string): Promise<void> => {
      const httpClient = location.startsWith("https")
        ? https
        : http;

      return new Promise((resolve, reject) => {
        const request = httpClient.get(location, (response) => {
          verbose("STATUS", response.statusCode);
          if (response.statusCode !== 200) {
            reject(
              new Error(
                `Server responded with status code ${response.statusCode}`
              )
            );
            return;
          }

          if (
            response.statusCode >= 300 &&
            response.statusCode < 400 &&
            response.headers.location
          ) {
            return downloadAndExtract(response.headers.location)
              .then(resolve)
              .catch(reject);
          }


          const gunzip = zlib.createGunzip();
          const extract = tar.extract({ cwd: targetDir });

          pipeline(response, gunzip, extract)
            .then(resolve)
            .catch(reject);
        });

        request.on("error", reject);
      });
    };

    await downloadAndExtract(url);
  }

  const osName = os.platform();
  return {
    acgDir: targetDir,
    executable: acgToolName + (osName === "win32" ? ".exe" : ""),
    args: [],
  };
}

/**
 * Retrieves a list of installed .NET runtimes on the system.
 * @returns Promise that resolves to an array of .NET runtime strings, or null if dotnet CLI is not available
 * @throws {AcgError} If an unexpected error occurs during runtime detection
 */
function getDotnetRuntimes(): Promise<string[] | null> {
  return new Promise((resolve, reject) => {
    const command = `${dotnetCmd} --list-runtimes`;

    proc.exec(command, (error, stdout, stderr) => {
      if (error) {
        if (error.message.startsWith("Command failed: ")) {
          resolve(null);
        } else {
          reject(
            new AcgError("Get list of .NET runtimes failed.", { cause: error })
          );
        }
      } else {
        const runtimes = [...stdout.matchAll(netRuntimeRegex)].map(
          (m) => m[0]
        );
        resolve(runtimes);
      }
    });
  });
}

/**
 * Extracts the required .NET runtime version from the ACG runtime configuration file.
 * @param dir - Directory containing the ACG binaries and runtime config
 * @returns The major version of the required .NET runtime
 * @throws {AcgError} If runtime config file is invalid or version format is incorrect
 */
async function getAcgRuntime(dir: string): Promise<string> {
  const fileName = acgToolName + ".runtimeconfig.json";
  const runtimeConfig = await fs.promises.readFile(
    path.join(dir, fileName),
    "utf-8"
  );
  const json = JSON.parse(runtimeConfig);
  const version = json.runtimeOptions.includedFrameworks?.length >= 1
    ? json.runtimeOptions.includedFrameworks[0].version
    : json.runtimeOptions.framework.version;
  const trimedVersion =
    version && version.substring(0, version.lastIndexOf("."));
  if (trimedVersion) {
    return trimedVersion;
  }
  throw new AcgError(`Invalid version format: ${version}`);
}

/**
 * Discovers available Nswag runtime versions in the specified directory.
 * @param nswagPath - Optional path to the Nswag binaries directory. Defaults to node_modules/nswag/bin/binaries
 * @returns Array of available Nswag runtime objects containing runtime version and directory path
 */
async function getNswagRuntimes(
  nswagPath?: string
): Promise<NswagRuntime[]> {
  if (!nswagPath) {
    const paths = [
      ... (require.resolve.paths("nswag")?.slice(1, 1) || []),
      path.join(__dirname, "node_modules"),
    ];
    nswagPath = path.join(
      path.dirname(require.resolve("nswag/package.json", { paths })),
      "bin",
      "binaries"
    );
  }

  const dirs = await fs.promises.readdir(nswagPath);
  return dirs
    .filter((d) => d.startsWith("Net"))
    .map((d) => ({
      rt: d.substring(3, d.length - 1) + ".0",
      dir: path.join(nswagPath!, d),
    }));
}

/**
 * Loads ACG extensions from built-in and npm package sources.
 * @param binariesDir - Directory containing ACG binaries
 * @param rc - Runtime configuration containing extension package names
 * @param requireMock - Optional mock require function for testing purposes
 * @returns Array of paths to extension DLL files
 * @throws {AcgError} If an extension cannot be loaded
 */
function getExtensions(binariesDir: string, rc: AcgRuntimeConfig, requireMock?: NodeJS.Require): string[] {
  const result: string[] = [];
  requireMock ??= require;

  const builtinExtensions = [
    path.join(binariesDir, "ApiCodeGenerator.OpenApi.dll"),
    path.join(binariesDir, "ApiCodeGenerator.AsyncApi.dll"),
  ];
  result.push(...builtinExtensions.filter((p) => fs.existsSync(p)));

  if (rc.extensions) {
    const paths = ["node_modules"];
    for (const pkg of rc.extensions) {
      try {
        const jsPath = requireMock.resolve(pkg + "/acg.js", { paths });
        const dlls = requireMock(jsPath);
        dlls && result.push(...dlls);
      } catch (error) {
        throw new AcgError("Unable load extension " + pkg, {
          cause: error as Error,
        });
      }
    }
  }
  return result;
}

/**
 * Discovers and resolves API references (NSWAG configuration files) in the specified directory.
 * @param dir - Root directory to search for .nswag files
 * @param rc - Runtime configuration containing API reference overrides and settings
 * @returns Array of resolved API references with document paths and output locations
 */
async function getApiReferences(
  dir: string,
  rc: AcgRuntimeConfig
): Promise<ApiReference[]> {

  const excludedPaths = require.resolve.paths("tar")?.filter(i => i.startsWith(dir)) || [];
  const findNswag = async (dir: string): Promise<string[]> => {
    const res: string[] = [];
    const entries = await fs.promises.readdir(dir, { withFileTypes: true });
    for (const entry of entries) {
      if (entry.isDirectory()) {
        const dirPath = path.join(dir, entry.name);
        if (!excludedPaths.includes(dirPath)) {
          res.push(...(await findNswag(dirPath)));
        }
      } else {
        if (entry.name.endsWith(".nswag")) {
          res.push(path.join(dir, entry.name));
        }
      }
    }
    return res;
  };

  const nswagFiles = await findNswag(dir);
  verbose("Found nswag files:", nswagFiles);
  const references = nswagFiles
    .map((nswag) => {
      const relativeNswag = path.relative(dir, nswag);
      const ref =
        rc.apiReferences &&
        Array.isArray(rc.apiReferences) &&
        rc.apiReferences.find((r) => r.nswag === relativeNswag);

      if (ref) {
        validateApiReference(ref);
        return ref;
      }

      const apiDocumentDir = rc.apiDocumentDir || path.dirname(nswag);
      const fileName = path.basename(nswag);
      const extensions = [".json", ".yaml", ".yml"];
      const document = extensions
        .map((e) =>
          path.join(apiDocumentDir, fileName.replace(".nswag", e))
        )
        .find((d) => fs.existsSync(d));

      return {
        nswag: relativeNswag,
        document: document,
        out: relativeNswag.replace(".nswag", ".g.ts"),
      };
    });

  return references;
}

/**
 * Validates that an API reference has all required properties and that referenced files exist.
 * @param ref - API reference to validate
 * @returns The validated API reference with all required properties
 * @throws {AcgError} If required properties are missing or referenced files do not exist
 */
function validateApiReference(ref: ApiReference): Required<ApiReference> {
  if (!ref.document) {
    throw new AcgError(
      `Property 'document' is required. Check apiRefereces for '${ref.nswag}' in .acgrc.`
    );
  }
  if (!fs.existsSync(ref.document)) {
    throw new AcgError(`File '${ref.document}' not exists.`);
  }
  if (!ref.out) {
    throw new AcgError(
      `Property 'out' is required. Check apiRefereces for '${ref.nswag}' in .acgrc.`
    );
  }
  return ref as Required<ApiReference>;
}

/**
 * Selects the most appropriate Nswag tool directory based on available .NET runtimes and ACG requirements.
 * @param netRuntimes - Array of installed .NET runtimes, or null if not available
 * @param acgRuntime - Required .NET runtime version for ACG
 * @param nswagRuntimes - Array of available Nswag runtime installations
 * @returns Path to the selected Nswag tool directory, or null if no compatible version is found
 */
function getAcgNswagToolDir(
  netRuntimes: string[] | null,
  acgRuntime: string,
  nswagRuntimes: NswagRuntime[]
): string | null {
  const parseMajorVersion = (versionString: string): number => {
    let innerEx: Error | null = null;
    try {
      if (versionString.charAt(0).match(/\d/)) {
        return parseInt(
          versionString.substring(0, versionString.indexOf(".")),
          10
        );
      }
      if (versionString.startsWith("net")) {
        const pos = versionString.indexOf(".");
        if (pos > 3) {
          return parseInt(versionString.substring(3, pos - 3), 10);
        }
      } else if (versionString.startsWith("Net")) {
        return parseInt(versionString.substring(3), 10) / 10;
      }
    } catch (ex) {
      innerEx = ex as Error;
    }
    throw new AcgError(`Unknown version format: '${versionString}'`, {
      cause: innerEx || undefined,
    });
  };

  const parseNetRuntimeMajorVersion = (versionString: string): number => {
    const match = Array.from(versionString.matchAll(netRuntimeRegex));
    if (match && match[0]) {
      return parseMajorVersion(match[0][1]);
    }
    throw new AcgError(
      `Invalid .NET runtime data format: '${versionString}'. Expected: 'Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]'`
    );
  };

  const runtimes = netRuntimes
    ?.map((r) => parseNetRuntimeMajorVersion(r))
    .sort((a, b) => b - a)
    ?? [parseMajorVersion(acgRuntime)]; // if binary includes framework, then get version from runtimeconfig

  const map = [acgRuntime]
    .map((l) => ({
      version: parseMajorVersion(l),
      acg: l,
      nswag: nswagRuntimes.find(
        (r) => parseMajorVersion(r.rt) === parseMajorVersion(l)
      ),
    }))
    .map((i) => ({
      ...i,
      net: runtimes.includes(i.version)
        ? i.version
        : runtimes.find((r) => r > i.version),
    }))
    .sort((a, b) => b.version - a.version);

  const selected =
    map.find((i) => i.version === i.net) ||
    map.find((i) => i.version < i.net!);

  if (selected && selected.nswag) {
    return selected.nswag.dir;
  }
  return null;
}

/**
 * Gathers and prepares all information needed to execute the code generation tool.
 * @param rc - Runtime configuration object
 * @param opt - Optional override options for binary directory and download behavior
 * @returns Tool information including command, arguments, Nswag tool path, and extensions
 * @throws {AcgError} If required tools or runtimes are not found
 */
async function getToolsInfo(
  rc: AcgRuntimeConfig,
  opt?: Options
): Promise<ToolsInfo> {
  if (!opt) opt = {};
  let acgBinariesDir = opt.acgBinaryDir || getAcgBinaryDir(rc) || defaultBinaries;
  verbose("acgBinariesDir = ", acgBinariesDir);
  const allowDownload =
    !(opt.noDownloadBinary || getNoDownloadBinary(rc)) && acgBinariesDir === defaultBinaries;
  verbose("allowDownload = ", allowDownload);
  const netRuntimes = await getDotnetRuntimes();
  let cmd = dotnetCmd;
  let cmdArgs: string[] = [
    "--roll-forward",
    "Major",
    path.join(acgBinariesDir, acgToolName + ".dll"),
  ];

  const rid = getRuntimeIdentifier();
  verbose("RID =", rid);

  if (!netRuntimes) {
    if (!allowDownload) {
      throw new AcgError(
        ".NET are required. To continue, please install .NET Runtime 8 or higher."
      );
    } else {
      // get path to downloaded tool with included framework
      const pkg = JSON.parse(
        fs.readFileSync(path.join(__dirname, "package.json"), "utf-8")
      );
      const binarySite = opt.acgBinarySite || getAcgBinarySite(rc);
      const result = await downloadBinaries(binarySite, rid, pkg.version);
      acgBinariesDir = result.acgDir;
      cmd = path.join(acgBinariesDir, result.executable);
      cmdArgs = result.args;
    }
  }

  const nswagRuntimes = await getNswagRuntimes(rc.nswagToolDir);
  verbose("Nswag runtimes:", nswagRuntimes);

  let acgRuntime = await getAcgRuntime(acgBinariesDir);
  verbose("acgRuntime = ", acgRuntime);

  let nswagTool = getAcgNswagToolDir(netRuntimes, acgRuntime, nswagRuntimes);
  verbose("nswagTool =", nswagTool);

  if (!nswagTool) {
    throw new AcgError(
      "Nswag for runtime '" +
      acgRuntime +
      "' not found. Available runtimes: " +
      nswagRuntimes
    );
  }

  console.log("Using Nswag from:", nswagTool);

  const extensions = getExtensions(acgBinariesDir, rc);
  return { cmd, cmdArgs, nswagTool, extensions };
}

/**
 * Generates code for a single API reference by executing the ACG tool.
 * @param acgRc - Runtime configuration object
 * @param ref - API reference containing the NSWAG config and output file
 * @param toolsInfo - Tool information including command and arguments
 * @returns Promise that resolves when code generation completes successfully
 * @throws {AcgError} If code generation fails or validation fails
 */
function generateFile(
  acgRc: AcgRuntimeConfig,
  ref: ApiReference,
  toolsInfo: ToolsInfo
): Promise<void> {

  try {
    const validRef = validateApiReference(ref);
    return new Promise((resolve, reject) => {
      const vars = Object.assign({}, acgRc?.variables ?? {}, ref.variables ?? {});
      const varsArg = Object.keys(vars)
        .map((k) => k + "=" + vars[k])
        .join(",");

      const args = [
        ...toolsInfo.cmdArgs,
        validRef.document,
        validRef.nswag,
        validRef.out,
        ...toolsInfo.extensions.map((e) => ["-e", e]).flat(),
        ...(varsArg ? ["-v", varsArg] : []),
        "--nswagTool",
        toolsInfo.nswagTool,
      ];

      verbose("Arguments", args);

      const child = proc.spawn(toolsInfo.cmd, args, { stdio: 'inherit' });
      child.on("error", (error) =>
        reject(new AcgError("Code generation failed.", { cause: error }))
      );
      child.on("close", (code) => {
        if (code === null || code > 0) {
          reject(new AcgError(`Code generation failed. Exit code: ${code}`));
        } else {
          resolve();
        }
      });
    });
  } catch (err) {
    return Promise.reject(err);
  }
}

/**
 * Main entry point that orchestrates the code generation process.
 * Discovers all API references in the specified directory and generates code for each one.
 * @param pkgDir - Root package directory to search for NSWAG configuration files
 * @param rc - Runtime configuration object
 * @param opt - Optional override options for binary directory and download behavior
 * @throws {AcgError} If initialization or code generation fails
 */
async function run(
  pkgDir: string,
  rc: AcgRuntimeConfig,
  opt?: Options
): Promise<void> {
  verbose("pkgDir =", pkgDir);

  const toolsInfo = await getToolsInfo(rc, opt);
  verbose("Tools info:", toolsInfo);

  const apiReferences = await getApiReferences(pkgDir, rc);
  verbose("apiReferences =", apiReferences);

  for (const ref of apiReferences) {
    if (!ref.document) {
      console.warn(
        `OpenApi/AsyncApi document for file '${ref.nswag}' not found.`
      );
      continue;
    } else {
      console.log("Generate code for " + ref.nswag);
      await generateFile(rc, ref, toolsInfo);
    }
  }
}


export {
  AcgError,
  run,
  enableVerbose,
  generateFile,
  getAcgBinaryDir,
  getAcgBinarySite,
  getAcgNswagToolDir,
  getAcgRuntime,
  getApiReferences,
  getDotnetRuntimes,
  getExtensions,
  getNoDownloadBinary,
  getNswagRuntimes,
  getRuntimeIdentifier,
};
