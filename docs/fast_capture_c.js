const puppeteer = require('./node_modules/puppeteer-core');
const fs = require('fs');
const path = require('path');

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const outputDir = path.join(__dirname, 'images');

(async () => {
    const browser = await puppeteer.launch({
        executablePath: chromePath,
        headless: true,
        args: ['--no-sandbox', '--disable-setuid-sandbox', '--window-size=1440,900']
    });

    const page = await browser.newPage();
    await page.setViewport({ width: 1440, height: 900 });

    console.log('Logging in...');
    await page.goto('http://localhost:59580/login');
    await page.type('#Email', 'admin@yourStore.com');
    await page.type('#Password', 'admin');
    await page.click('button.login-button');
    await page.waitForNavigation();

    console.log('Opening AI Duplicate List...');
    await page.goto('http://localhost:59580/Admin/AiDuplicateProduct/List');
    
    console.log('Saving screenshot...');
    await page.screenshot({ path: path.join(outputDir, 'module_c_duplicate_detection.png'), fullPage: false });
    console.log('Done module_c!');

    await browser.close();
})();
