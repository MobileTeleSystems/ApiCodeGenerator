module.exports = {
  preset: "ts-jest",
  testEnvironment: "node",
  roots: ["<rootDir>"],
  testMatch: ["**/*.test.ts"],
  moduleFileExtensions: ["ts", "js", "json"],
  globals: {
    "ts-jest": {
      tsconfig: {
        target: "ES2020",
        module: "commonjs",
        lib: ["ES2020"],
        strict: true,
        esModuleInterop: true,
        skipLibCheck: true,
        forceConsistentCasingInFileNames: true,
      },
      babelConfig: {
        plugins: ["@babel/plugin-transform-typescript"],
      },
    },
  },
};
