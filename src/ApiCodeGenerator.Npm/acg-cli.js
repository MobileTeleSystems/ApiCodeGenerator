#!/usr/bin/env node
"use strict"

const acg = require("./acg");
const fs = require("fs");

const enableVerbose = process.argv.indexOf("-v") > -1;

/**
 * Get command line argument value
 * @param {string} name - Argument name
 * @returns {string | undefined } Argument value
 */
function getArgument(name) {
  var flags = process.argv.slice(2),
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

const opt = {
  acgBinaryDir: getArgument("--acg-binary-dir")
    || acg.getAcgBinaryDir(rc),
  acgBinarySite: getArgument("--acg-binary-site")
    || acg.getAcgBinarySite(rc),
  noDownloadBinary: process.argv.indexOf("--no-download-acg-binary") > -1
    || acg.getNoDownloadBinary(rc)
};

enableVerbose && acg.enableVerbose();

acg
  .run(process.cwd(), rc, opt)
  .catch(handleError);
