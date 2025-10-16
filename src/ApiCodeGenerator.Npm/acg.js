#!/usr/bin/env node
"use strict";

var proc = require("child_process");
var fs = require("fs");
var path = require("path");

var dotnetCmd = "dotnet";
var acgToolName = "ApiCodeGenerator.MSBuild.dll"
var binariesDir = path.join(__dirname, "binaries");
var rc = {}

if (fs.existsSync(".acgrc")) {
  var json = fs.readFileSync(".acgrc");
  if (json) {
    try {
      rc = JSON.parse(json)
    }
    catch (error) {
      error("Error loading .acgrc file", error);
    }
  }
}

function getDotnetRuntimes() {
  var infoArgs = ["--list-runtimes"];
  var result = proc.spawnSync(dotnetCmd, infoArgs, { encoding: 'utf-8' });
  if (result.error) {
    if (result.error.message === "spawnSync dotnet ENOENT") {
      error(".NET are required. To continue, please install .NET Runtime 8 or hight.");
    }
    else {
      throw result.error;
    }
  }

  var runtimes = [...result.stdout.matchAll(/^Microsoft.NETCore.App (\S+)/gm)].map(m => m[1]);

  return runtimes;
}

function getAcgRuntime(dir) {
  var fileName = acgToolName.replace(".dll", ".runtimeconfig.json");
  var runtimeConfig = fs.readFileSync(dir ? path.join(dir, fileName) : path.join(binariesDir, fileName));
  var json = JSON.parse(runtimeConfig);
  var version = json.runtimeOptions.framework.version;
  return version.substring(0, version.length - 2);
}

function getNswagRuntimes(nswagPath) {
  if (!nswagPath) {
    var paths = [
      path.join(process.cwd(), "node_modules"),
      path.join(__dirname, "node_modules"),
    ]
    nswagPath = path.join(path.dirname(require.resolve("nswag/package.json", { paths })), "bin", "binaries");
  }
  return fs.readdirSync(nswagPath)
    .filter(function (d) { return d.startsWith("Net") })
    .map(function (d) { return { rt: d.substring(3, d.length - 1) + ".0", dir: path.join(nswagPath, d) } });
}

function getExtensions() {
  var result = [];

  var openApi = [path.join(binariesDir, "ApiCodeGenerator.OpenApi.dll"), path.join(binariesDir, "..", "ApiCodeGenerator.OpenApi.dll")];
  result.push(...openApi.filter(p => fs.existsSync(p)));

  var asyncApi = [path.join(binariesDir, "ApiCodeGenerator.AsyncApi.dll"), path.join(binariesDir, "..", "ApiCodeGenerator.AsyncApi.dll")];
  result.push(...asyncApi.filter(p => fs.existsSync(p)));

  if (rc.extensions) {
    var paths = [path.join(process.cwd(), "node_modules")];
    for (var pkg of rc.extensions) {
      try {
        var jsPath = require.resolve(pkg + "/acg.js", { paths });
        var dlls = require(jsPath);
        result.push(...dlls);
      } catch (error) {
        error("Unable load extension " + pkg, error);
      }
    }
  }
  return result;
}

function getOpenApiReferences(dir) {
  var findNswag = function (dir) {
    var res = [];
    for (var entry of fs.readdirSync(dir, { withFileTypes: true })) {
      if (entry.isFile()) {
        if (entry.name.endsWith(".nswag"))
          res.push(path.join(dir, entry.name));
      }
      else {
        res.push(...findNswag(path.join(dir, entry.name)));
      }
    }
    return res;
  }
  var nswagFiles = findNswag(dir);
  var references = nswagFiles
    .map(function (nswag) {
      var relativeNswag = path.relative(process.cwd(), nswag);
      var ref = rc.apiReferences && rc.apiReferences instanceof Array && rc.apiReferences.find(function (r) { return r.nswag === relativeNswag })
      if (ref) {
        if (!ref.document) {
          error("Property 'document' is required. Check apiRefereces for '" + relativeNswag + "' in .acgrc.")
        }
        if (!fs.existsSync(ref.document)) {
          error("File '" + ref.document + "' not exists.");
        }
        if (!ref.out) {
          error("Property 'out' is required. Check apiRefereces for '" + relativeNswag + "' in .acgrc.")
        }
        return ref;
      }

      var dir = rc.apiDocumentDir || path.dirname(nswag);
      var fileName = path.basename(nswag);
      var extensions = [".json", ".yaml", ".yml"];
      var document = extensions.map(e => path.join(dir, fileName.replace(".nswag", e))).find(d => fs.existsSync(d));

      return {
        nswag: nswag,
        document: document,
        out: nswag.replace(".nswag", ".g.ts")
      }
    })

  return references;
}

function isCompatible(appRuntime, dotnetRuntimes) {
  return dotnetRuntimes.find(function (r) { return r.startsWith(appRuntime); });
}

function error(...args) {
  console.error(...args);
  process.exit(1);
}

try {
  var acgRuntime = getAcgRuntime();
  var netRuntimes = getDotnetRuntimes();

  if (!isCompatible(acgRuntime, netRuntimes)) {
    error(".NET Runtime are required. Please install .NET Runtime " + acgRuntime + " or hight. Installed runtimes: " + netRuntimes.toString());
  }

  var nswagRuntimes = getNswagRuntimes(rc.nswag);
  if (!(nswagRuntimes.length > 0)) {
    error("Nswag binaries not found.");
  }

  var nswagTool = nswagRuntimes.find(function (r) { return acgRuntime.startsWith(r.rt) });
  if (!nswagTool) {
    error("Nswag binaries for runtime " + acgRuntime + ". Available runtimes: " + nswagRuntimes.map(function (r) { return r.rt }).toString());
  }

  console.log("Using Nswag from:", nswagTool.dir);

  var extensions = getExtensions();
  var oaReferences = getOpenApiReferences(process.cwd());

  for (var ref of oaReferences) {
    if (!ref.document) {
      console.warn("OpenApi/AsyncApi document for file '" + ref.nswag + "' not found.");
      continue;
    }
    var vars = Object.assign({}, rc.variables || {}, ref.variables || {});
    var varsArg = Object.keys(vars).map(k => k + "=" + vars[k]).join(",");

    var args = [
      path.join(binariesDir, acgToolName),
      ref.document,
      ref.nswag,
      ref.out,
      ...extensions.map(function (e) { return ["-e", e] }).flat(),
      ...varsArg ? ["-v", varsArg] : v,
      "--nswagTool",
      nswagTool.dir
    ]

    console.log("Generate code for " + ref.nswag);
    try {
      var childResult = proc.spawnSync(dotnetCmd, args, { stdio: 'inherit' });
      if (childResult.status != 0) {
        if (typeof childResult.status === 'number') {
          process.exit(childResult.status);
        } else {
          error(childResult.error);
        }
      }
    } catch (error) {
      if (typeof error.status === 'number') {
        process.exit(error.status);
      } else {
        error(error);
      }
    }
  }

} catch (error) {
  console.error("Unexpected error");
  console.debug(error);
  process.exit(1);
}
