#!/usr/bin/env node
"use strict"

const acg = require("./acg");
const fs = require("fs");

const args = process.argv.slice(2);

function showHelp() {
  console.log(`API Code Generator CLI

Usage: acg [options]

Options:
 -h, --help                Show this help message
 -v, --verbose             Enable verbose logging
 --acg-binary-dir <path>   Path to the directory containing ApiCodeGenerator.MSBuild.dll
 --acg-binary-site <url>   URL to download binaries
 --no-download-acg-binary  Disable automatic binary download

Examples:
 acg --help

Description:
 Generates API client code based on API specifications configured in .nswag files.

For more information, visit: https://github.com/MobileTeleSystems/ApiCodeGenerator`);
}

// Check for help flag
if (args.includes('-h') || args.includes('--help')) {
  showHelp();
  process.exit(0);
}

const enableVerbose = args.indexOf("-v") > -1 || args.indexOf("--verbose");

/**
 * Get command line argument value
 * @param {string} name - Argument name
 * @returns {string | undefined } Argument value
 */
function getArgument(name) {
  var flags = args.slice(2),
    index = flags.lastIndexOf(name);

  if (index === -1 || index + 1 >= flags.length) {
    return undefined;
  }

  return flags[index + 1];
}

/** @param {(Error & {status?: number}) | number} error  */
function handleError(error) {
  if (error) {
    if (typeof error === "number") {
      process.exit(error);
    } else {
      if (!(error instanceof acg.AcgError)) {
        console.error("Unexpected error");
      }
      console.error(error.message);
      enableVerbose && console.trace(error);
    }
    process.exit(error.status && typeof error.status === "number" ? error.status : 1);
  }
}

let rc = {};
if (fs.existsSync(".acgrc")) {
  var json = fs.readFileSync(".acgrc");
  if (json) {
    try {
      //@ts-ignore
      rc = JSON.parse(json)
    }
    catch (err) {
      console.error("Error loading .acgrc file");
      //@ts-ignore
      handleError(err);
    }
  }
}

/** @type {import("./acg").Options} */
const opt = {
  acgBinaryDir: getArgument("--acg-binary-dir")
    || acg.getAcgBinaryDir(rc),
  acgBinarySite: getArgument("--acg-binary-site")
    || acg.getAcgBinarySite(rc),
  noDownloadBinary: args.indexOf("--no-download-acg-binary") > -1
    || acg.getNoDownloadBinary(rc)
};

enableVerbose && acg.enableVerbose();

acg
  .run(process.cwd(), rc, opt)
  .catch(handleError);
