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

interface RuntimeConfig {
  acg_binary_dir?: string;
  no_download_acg_binary?: boolean;
  acg_binary_site?: string;
  nswag?: string;
  apiDocumentDir?: string;
  apiReferences?: ApiReference[];
  extensions?: string[];
  variables?: Record<string, string>;
  [key: string]: any;
}

interface ApiReference {
  nswag: string;
  document?: string;
  out?: string;
}

interface NswagRuntime {
  rt: string;
  dir: string;
}

interface ToolsInfo {
  cmd: string;
  cmdArgs: string[];
  nswagTool: string;
  extensions: string[];
}

interface GenerateFileRef {
  document: string;
  nswag: string;
  out: string;
}

interface DownloadBinariesResult {
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
function verbose(...arg: any[]) {
  if (verboseLog) {
    console.log(...arg);
  }
}

function enableVerbose() {
  verboseLog = true;
}

function getAcgBinaryDir(rc: RuntimeConfig): string | undefined {
  return (
    process.env.ACG_BINARY_DIR ||
    rc.acg_binary_dir ||
    process.env.npm_config_acg_binary_dir
  );
}

function getNoDownloadBinary(rc: RuntimeConfig): boolean {
  return !!(
    process.env.NO_DOWNLOAD_ACG_BINARY ||
    rc.no_download_acg_binary ||
    process.env.npm_config_no_download_acg_binary
  );
}

function getAcgBinarySite(rc: RuntimeConfig): string {
  const site =
    process.env.ACG_BINARY_SITE ||
    rc.acg_binary_site ||
    process.env.npm_config_acg_binary_site ||
    "https://github.com/MobileTeleSystems/ApiCodeGenerator/releases/download";

  const siteNormalized = site.endsWith("/") ? site.slice(0, -1) : site;
  return siteNormalized;
}

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

function getExtensions(binariesDir: string, rc: RuntimeConfig, requireMock?: NodeJS.Require): string[] {
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

async function getApiReferences(
  dir: string,
  rc: RuntimeConfig
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
        if (!ref.document) {
          throw new AcgError(
            `Property 'document' is required. Check apiRefereces for '${relativeNswag}' in .acgrc.`
          );
        }
        if (!fs.existsSync(ref.document)) {
          throw new AcgError(`File '${ref.document}' not exists.`);
        }
        if (!ref.out) {
          throw new AcgError(
            `Property 'out' is required. Check apiRefereces for '${relativeNswag}' in .acgrc.`
          );
        }
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

async function getToolsInfo(
  rc: RuntimeConfig,
  opt?: Record<string, any>
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
    if (!allowDownload || rid.startsWith("win")) {
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

  const nswagRuntimes = await getNswagRuntimes(rc.nswag);
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

function generateFile(
  acgRc: RuntimeConfig,
  ref: GenerateFileRef,
  toolsInfo: ToolsInfo,
  variables?: Record<string, string>
): Promise<void> {
  return new Promise((resolve, reject) => {
    const vars = Object.assign({}, acgRc && variables || {});
    const varsArg = Object.keys(vars)
      .map((k) => k + "=" + vars[k])
      .join(",");

    const args = [
      ...toolsInfo.cmdArgs,
      ref.document,
      ref.nswag,
      ref.out,
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
}

async function run(
  pkgDir: string,
  rc: RuntimeConfig,
  opt?: Record<string, any>
): Promise<void> {
  verbose("pkgDir =", pkgDir);

  const toolsInfo = await getToolsInfo(rc, opt);
  verbose("Tools info:", toolsInfo);

  const apiReferences = await getApiReferences(pkgDir, rc);
  verbose("apiReferences =", apiReferences);

  for (const ref of apiReferences) {
    if (!ref.document) {
      console.warn(
        "OpenApi/AsyncApi document for file '" + ref.nswag + "' not found."
      );
      continue;
    } else {
      console.log("Generate code for " + ref.nswag);
      await generateFile(rc, ref as GenerateFileRef, toolsInfo, rc?.variables);
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
