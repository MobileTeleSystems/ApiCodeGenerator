const fs = require('fs');
const path = require('path');

const acgrcPath = path.join(process.env.INIT_CWD, '.acgrc');
const packageJsonPath = path.join(process.cwd(), 'package.json');

try {
  var acgrcJson = {};
  if (fs.existsSync(acgrcPath)) {
    console.log('Reading ' + acgrcPath);
    const acgrcContent = fs.readFileSync(acgrcPath, 'utf8');
    acgrcJson = JSON.parse(acgrcContent);
  }

  if (!Array.isArray(acgrcJson.extensions)) {
    acgrcJson.extensions = [];
  }

  const packageContent = fs.readFileSync(packageJsonPath, 'utf8');
  const packageJson = JSON.parse(packageContent);

  const newExtension = packageJson.name;
  if (!acgrcJson.extensions.includes(newExtension)) {
    acgrcJson.extensions.push(newExtension);
  }

  console.log('Writing ' + acgrcPath);
  fs.writeFileSync(acgrcPath, JSON.stringify(acgrcJson, null, 2), 'utf8');

} catch (error) {
  console.error(`Update .acgrc failed: ${error.message}`);
}
