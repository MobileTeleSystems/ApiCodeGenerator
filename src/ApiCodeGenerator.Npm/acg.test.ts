import * as proc from "child_process";
import * as acg from "./acg";
import { resolve } from "path";

jest.mock("child_process");
jest.mock("detect-libc", () => ({
  isNonGlibcLinuxSync: jest.fn(() => false),
}));

describe("getDotnetRuntimes", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("should resolve with a list of runtimes", async () => {
    const mockData =
      "Microsoft.NETCore.App 8.0.8 [/usr/share/dotnet/shared/Microsoft.NETCore.App]";

    (proc.exec as unknown as jest.Mock).mockImplementation((command, callback) => {
      callback(null, mockData, "");
    });

    const runtimes = await acg.getDotnetRuntimes();
    expect(runtimes).toEqual([mockData]);
  });

  it("should resolve with null if the dotnet command is not found", async () => {
    const error = new Error("Command failed: dotnet not found");
    (proc.exec as unknown as jest.Mock).mockImplementation((command, callback) => {
      callback(error, "", "");
    });

    const runtimes = await acg.getDotnetRuntimes();
    expect(runtimes).toBeNull();
  });

  it("should reject with an AcgError if the process fails", async () => {
    const error = new Error("some other error");
    (proc.exec as unknown as jest.Mock).mockImplementation((command, callback) => {
      callback(error, "", "");
    });

    await expect(acg.getDotnetRuntimes()).rejects.toThrow(
      "Get list of .NET runtimes failed."
    );
  });
});

const fs = require("fs").promises;
const path = require("path");

jest.mock("fs", () => ({
  promises: {
    readdir: jest.fn(),
    readFile: jest.fn(),
  },
  existsSync: jest.fn(),
}));

describe("getNswagRuntimes", () => {
  it("should return a list of NetX.X directories", async () => {
    const mockDirs = ["Net50", "Net60", "NotNetDir"];

    const fsMock = fs as any;
    fsMock.readdir.mockResolvedValue(mockDirs);

    const nswagPath = path.resolve(
      __dirname,
      "node_modules",
      "nswag",
      "bin",
      "binaries"
    );
    const runtimes = await acg.getNswagRuntimes(nswagPath);

    expect(runtimes).toEqual([
      { rt: "5.0", dir: path.join(nswagPath, "Net50") },
      { rt: "6.0", dir: path.join(nswagPath, "Net60") },
    ]);
  });

  it("should return an empty array if no suitable directories exist", async () => {
    const fsMock = fs as any;
    fsMock.readdir.mockResolvedValue(["SomeDir", "AnotherDir"]);

    const nswagPath = path.resolve(
      __dirname,
      "node_modules",
      "nswag",
      "bin",
      "binaries"
    );
    const runtimes = await acg.getNswagRuntimes(nswagPath);

    expect(runtimes).toEqual([]);
  });

  it("should throw an error if directory reading fails", async () => {
    const fsMock = fs as any;
    fsMock.readdir.mockRejectedValue(new Error("Failed to read directory"));

    await expect(acg.getNswagRuntimes()).rejects.toThrow(
      "Failed to read directory"
    );
  });
});

describe("getAcgRuntime", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("should return the correct runtime version from framework", async () => {
    const mockRuntimeConfig = JSON.stringify({
      runtimeOptions: {
        framework: {
          version: "5.0.10",
        },
      },
    });

    const fsMock = fs as any;
    fsMock.readFile.mockResolvedValue(mockRuntimeConfig);

    const testPath = path.join(__dirname, "binaries");
    const runtime = await acg.getAcgRuntime(testPath);

    expect(runtime).toBe("5.0");
    expect(fsMock.readFile).toHaveBeenCalledWith(
      path.join(testPath, "ApiCodeGenerator.MSBuild.runtimeconfig.json"),
      "utf-8"
    );
  });

  it("should return the correct runtime version from includedFrameworks", async () => {
    const mockRuntimeConfig = JSON.stringify({
      runtimeOptions: {
        includedFrameworks: [
          {
            name: "Microsoft.NETCore.App",
            version: "8.0.5",
          },
        ],
      },
    });

    const fsMock = fs as any;
    fsMock.readFile.mockResolvedValue(mockRuntimeConfig);

    const testPath = path.join(__dirname, "binaries");
    const runtime = await acg.getAcgRuntime(testPath);

    expect(runtime).toBe("8.0");
  });

  it("should use framework when includedFrameworks is empty", async () => {
    const mockRuntimeConfig = JSON.stringify({
      runtimeOptions: {
        framework: {
          version: "6.0.15",
        },
        includedFrameworks: [],
      },
    });

    const fsMock = fs as any;
    fsMock.readFile.mockResolvedValue(mockRuntimeConfig);

    const testPath = path.join(__dirname, "binaries");
    const runtime = await acg.getAcgRuntime(testPath);

    expect(runtime).toBe("6.0");
  });

  it("should handle version with multiple dots correctly", async () => {
    const mockRuntimeConfig = JSON.stringify({
      runtimeOptions: {
        framework: {
          version: "8.0.0.1",
        },
      },
    });

    const fsMock = fs as any;
    fsMock.readFile.mockResolvedValue(mockRuntimeConfig);

    const testPath = path.join(__dirname, "binaries");
    const runtime = await acg.getAcgRuntime(testPath);

    expect(runtime).toBe("8.0.0");
  });

  it("should handle version with two parts correctly", async () => {
    const mockRuntimeConfig = JSON.stringify({
      runtimeOptions: {
        framework: {
          version: "7.0",
        },
      },
    });

    const fsMock = fs as any;
    fsMock.readFile.mockResolvedValue(mockRuntimeConfig);

    const testPath = path.join(__dirname, "binaries");
    const runtime = await acg.getAcgRuntime(testPath);

    expect(runtime).toBe("7");
  });

  it("should throw an error if reading the config file fails", async () => {
    const fsMock = fs as any;
    fsMock.readFile.mockRejectedValue(new Error("Failed to read file"));

    const testPath = path.join(__dirname, "binaries");

    await expect(acg.getAcgRuntime(testPath)).rejects.toThrow(
      "Failed to read file"
    );
  });

  it("should throw an error if runtime version is not in the expected format", async () => {
    const mockInvalidConfig = JSON.stringify({
      runtimeOptions: {
        framework: {
          version: "invalid-version",
        },
      },
    });

    const fsMock = fs as any;
    fsMock.readFile.mockResolvedValue(mockInvalidConfig);

    const testPath = path.join(__dirname, "binaries");

    await expect(acg.getAcgRuntime(testPath)).rejects.toThrow(
      "Invalid version format: invalid-version"
    );
  });
});

describe("getApiReferences", () => {
  it("should return a list of OpenAPI references", async () => {
    const mockFiles = [
      { name: "example1.nswag", isDirectory: () => false },
      { name: "example2.nswag", isDirectory: () => false },
      { name: "notAnswag.txt", isDirectory: () => false },
    ];

    const fsMock = fs as any;
    fsMock.readdir.mockReturnValue(mockFiles);

    const dir = path.join(__dirname, "apis");
    const rcApiRef = {
      nswag: "example1.nswag",
      document: "/path/to/doc1.yaml",
      out: "/path/to/output1.ts",
    };
    const rc = {
      apiReferences: [rcApiRef],
    };

    const fsSync = require("fs");
    fsSync.existsSync = jest.fn((filePath) => {
      return filePath.endsWith("doc1.yaml");
    });

    const references = await acg.getApiReferences(dir, rc);

    expect(references).toEqual([
      rcApiRef,
      {
        nswag: "example2.nswag",
        document: undefined,
        out: "example2.g.ts",
      },
    ]);
  });

  it("should throw an error if document file not exists", async () => {
    const mockFiles = [{ name: "example.nswag", isDirectory: () => false }];
    const fsMock = fs as any;
    fsMock.readdir.mockReturnValue(mockFiles);

    const dir = path.join(__dirname, "apis");
    const rc = {
      apiReferences: [
        {
          nswag: "example.nswag",
          document: "/path/to/missing.yaml",
          out: "/path/to/output.ts",
        },
      ],
    };

    const fsSync = require("fs");
    fsSync.existsSync = jest.fn(() => false);
    expect(acg.getApiReferences(dir, rc)).rejects.toThrow(
      "File '/path/to/missing.yaml' not exists."
    );
  });

  it("should throw an error if required property 'document' are missing", async () => {
    const mockFiles = [{ name: "example.nswag", isDirectory: () => false }];
    const fsMock = fs as any;
    fsMock.readdir.mockReturnValue(mockFiles);

    const dir = path.join(__dirname, "apis");
    const rc = {
      apiReferences: [
        { nswag: "example.nswag", out: "/path/to/output.ts", document: "" },
      ],
    };

    const fsSync = require("fs");
    fsSync.existsSync = jest.fn(() => true);
    expect(acg.getApiReferences(dir, rc)).rejects.toThrow(
      "Property 'document' is required. Check apiRefereces for 'example.nswag' in .acgrc."
    );
  });

  it("should throw an error if required property 'out' are missing", async () => {
    const mockFiles = [{ name: "example.nswag", isDirectory: () => false }];
    const fsMock = fs as any;
    fsMock.readdir.mockReturnValue(mockFiles);

    const dir = path.join(__dirname, "apis");
    const rc = {
      apiReferences: [
        {
          nswag: "example.nswag",
          document: "/path/to/document.yaml",
          out: "",
        },
      ],
    };

    const fsSync = require("fs");
    fsSync.existsSync = jest.fn(() => true);
    expect(acg.getApiReferences(dir, rc)).rejects.toThrow(
      "Property 'out' is required. Check apiRefereces for 'example.nswag' in .acgrc."
    );
  });
});

describe("generateFile", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("should resolve when the process exits successfully", async () => {
    (proc.spawn as unknown as jest.Mock).mockImplementation((command, args, options) => ({
      on: (ev: string, callback: (...arg: any[]) => void) =>
        ev === "close" && setTimeout(() => callback(), 0)
    }));

    const acgRc = {};
    const ref = {
      document: "doc.yaml",
      nswag: "config.nswag",
      out: "output.ts",
    };
    const toolsInfo = {
      cmd: "node",
      cmdArgs: [],
      nswagTool: "",
      extensions: [],
    };

    await expect(
      acg.generateFile(acgRc, ref, toolsInfo)
    ).resolves.toBeUndefined();
  });

  it("should reject if the process exits with a non-zero code", async () => {
    (proc.spawn as unknown as jest.Mock).mockImplementation((command, args, options) => ({
      on: (ev: string, callback: (...arg: any[]) => void) => {
        const code = 1;
        ev === "close" && setTimeout(() => callback(code), 0);
      }
    }));

    const acgRc = {};
    const ref = {
      document: "doc.yaml",
      nswag: "config.nswag",
      out: "output.ts",
    };
    const toolsInfo = {
      cmd: "node",
      cmdArgs: [],
      nswagTool: "",
      extensions: [],
    };

    await expect(acg.generateFile(acgRc, ref, toolsInfo)).rejects.toThrow(
      "Code generation failed. Exit code: 1"
    );
  });

  it("should reject if the process encounters an error", async () => {
    (proc.spawn as unknown as jest.Mock).mockImplementation((command, args, options) => ({
      on: (ev: string, callback: (...arg: any[]) => void) => {
        const error = new Error("Command not found");
        ev === "error" && setTimeout(() => callback(error), 0);
      }
    }));

    const acgRc = {};
    const ref = {
      document: "doc.yaml",
      nswag: "config.nswag",
      out: "output.ts",
    };
    const toolsInfo = {
      cmd: "node",
      cmdArgs: [],
      nswagTool: "",
      extensions: [],
    };

    await expect(acg.generateFile(acgRc, ref, toolsInfo)).rejects.toThrow(
      "Code generation failed."
    );
  });
});

describe("getAcgNswagToolDir", () => {
  it("should return the correct nswag tool directory when compatible runtime is found", () => {
    const netRuntimes = [
      "Microsoft.NETCore.App 7.0.0 [/usr/share/dotnet/shared/Microsoft.NETCore.App]",
      "Microsoft.NETCore.App 8.0.0 [/usr/share/dotnet/shared/Microsoft.NETCore.App]",
    ];

    const acgRuntime = "8.0";

    const nswagRuntimes = [
      { rt: "7.0.0", dir: "/path/to/nswag/7.0.0" },
      { rt: "8.0.0", dir: "/path/to/nswag/8.0.0" },
    ];

    const result = acg.getAcgNswagToolDir(netRuntimes, acgRuntime, nswagRuntimes);

    expect(result).toEqual("/path/to/nswag/8.0.0");
  });

  it("should return null if no compatible runtime version is found", () => {
    const netRuntimes = [
      "Microsoft.NETCore.App 2.0.0 [/usr/share/dotnet/shared/Microsoft.NETCore.App]",
    ];

    const acgRuntime = "8.0";

    const nswagRuntimes = [
      { rt: "2.0.0", dir: "/path/to/nswag/2.0.0" },
      { rt: "7.0.0", dir: "/path/to/nswag/7.0.0" },
    ];

    const result = acg.getAcgNswagToolDir(netRuntimes, acgRuntime, nswagRuntimes);

    expect(result).toBeNull();
  });

  it("should handle missing nswag runtimes gracefully", () => {
    const netRuntimes = [
      "Microsoft.NETCore.App 8.0.0 [/usr/share/dotnet/shared/Microsoft.NETCore.App]",
    ];

    const acgRuntime = "8.0";

    const nswagRuntimes: any[] = [];

    const result = acg.getAcgNswagToolDir(netRuntimes, acgRuntime, nswagRuntimes);

    expect(result).toBeNull();
  });

  it("should select only by acgRuntime when netRuntimes is not provided", () => {
    const acgRuntime = "8.0";

    const nswagRuntimes = [
      { rt: "7.0.0", dir: "/path/to/nswag/7.0.0" },
      { rt: "8.0.0", dir: "/path/to/nswag/8.0.0" },
      { rt: "9.0.0", dir: "/path/to/nswag/9.0.0" },
    ];

    const result = acg.getAcgNswagToolDir(null, acgRuntime, nswagRuntimes);

    expect(result).toEqual("/path/to/nswag/8.0.0");
  });
});

describe("getRuntimeIdentifier", () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  it("should return correct runtime identifier for win32 platform", () => {
    const os = require("os");
    jest.spyOn(os, "platform").mockReturnValue("win32");
    jest.spyOn(os, "arch").mockReturnValue("x64");

    const result = acg.getRuntimeIdentifier();
    expect(result).toBe("win-x64");
  });

  it("should return correct runtime identifier for linux platform with glibc", () => {
    const os = require("os");
    const detectLibc = require("detect-libc");
    jest.spyOn(os, "platform").mockReturnValue("linux");
    jest.spyOn(os, "arch").mockReturnValue("arm64");
    (detectLibc.isNonGlibcLinuxSync as jest.Mock).mockReturnValue(false);

    const result = acg.getRuntimeIdentifier();
    expect(result).toBe("linux-arm64");
  });

  it("should return correct runtime identifier for linux platform with musl", () => {
    const os = require("os");
    const detectLibc = require("detect-libc");
    jest.spyOn(os, "platform").mockReturnValue("linux");
    jest.spyOn(os, "arch").mockReturnValue("x64");
    (detectLibc.isNonGlibcLinuxSync as jest.Mock).mockReturnValue(true);

    const result = acg.getRuntimeIdentifier();
    expect(result).toBe("linux-musl-x64");
  });

  it("should return correct runtime identifier for darwin platform", () => {
    const os = require("os");
    jest.spyOn(os, "platform").mockReturnValue("darwin");
    jest.spyOn(os, "arch").mockReturnValue("x64");

    const result = acg.getRuntimeIdentifier();
    expect(result).toBe("osx-x64");
  });

  it("should throw an error for unsupported OS or architecture", () => {
    const os = require("os");
    jest.spyOn(os, "platform").mockReturnValue("unsupported");
    jest.spyOn(os, "arch").mockReturnValue("unknown");

    expect(() => acg.getRuntimeIdentifier()).toThrow("Unsupported OS or architecture");
  });
});

describe("getExtensions", () => {
  const fs = require("fs");
  it("should return builtin extensions", () => {
    const binariesDir = path.join(__dirname, "binaries");
    const rc = {};

    fs.existsSync.mockReturnValue(true);  // Assume all standard files exist

    const result = acg.getExtensions(binariesDir, rc);

    expect(result).toEqual([
      path.join(binariesDir, "ApiCodeGenerator.OpenApi.dll"),
      path.join(binariesDir, "ApiCodeGenerator.AsyncApi.dll"),
    ]);

    // Ensure existsSync called for each standard file
    expect(fs.existsSync).toHaveBeenCalledWith(path.join(binariesDir, "ApiCodeGenerator.OpenApi.dll"));
    expect(fs.existsSync).toHaveBeenCalledWith(path.join(binariesDir, "ApiCodeGenerator.AsyncApi.dll"));
  });

  const mockResolvedPath = 'mock-pkg';
  it("should return extensions defined in rc", () => {
    const mockPackage = 'mock-package';
    const binariesDir = path.join(__dirname, "binaries");
    const mockExtensions = ['/mock/path1', '/mock/path2'];
    const rc = { extensions: [mockPackage] };

    fs.existsSync.mockReturnValue(true);  // Assume standard files exist

    const requireMock: NodeJS.Require = Object.assign(
      (id: string) => id === mockResolvedPath ? mockExtensions : require(id),
      {
        resolve: Object.assign(
          (r: string, o?: NodeJS.RequireResolveOptions) => r.startsWith(mockPackage) ? mockResolvedPath : require.resolve(r, o),) as NodeJS.RequireResolve,
        cache: require.cache,
        extensions: require.extensions,
        main: require.main
      }
    )

    const result = acg.getExtensions(binariesDir, rc, requireMock);

    expect(result).toEqual(
      [
        path.join(binariesDir, "ApiCodeGenerator.OpenApi.dll"),
        path.join(binariesDir, "ApiCodeGenerator.AsyncApi.dll"),
        ...mockExtensions
      ]
    );
  });


  it("should handle errors in module loading gracefully", () => {
    const binariesDir = path.join(__dirname, "binaries");
    const rc = {
      extensions: ['nonexistent-package']
    };

    fs.existsSync.mockReturnValue(true);  // Assume standard files exist

    const result = () => acg.getExtensions(binariesDir, rc);

    expect(result).toThrow(acg.AcgError);
  });
});
