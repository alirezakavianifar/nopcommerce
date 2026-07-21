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

    console.log('1. Logging in as admin...');
    await page.goto('http://localhost:59580/login', { waitUntil: 'domcontentloaded' });
    await page.type('#Email', 'admin@yourStore.com');
    await page.type('#Password', 'admin');
    await Promise.all([
        page.waitForNavigation({ waitUntil: 'domcontentloaded' }),
        page.click('button.login-button')
    ]);

    console.log('2. Setting working language to Persian (LanguageId = 2)...');
    await page.goto('http://localhost:59580/changelanguage/2?returnurl=/', { waitUntil: 'domcontentloaded' });

    console.log('3. Capturing Module B (Smart Voice & Visual Search) on Homepage...');
    await page.goto('http://localhost:59580/', { waitUntil: 'domcontentloaded' });
    await page.evaluate(() => new Promise(r => setTimeout(r, 1500)));
    await page.screenshot({ path: path.join(outputDir, 'module_b_smart_search.png'), fullPage: false });
    console.log('Saved module_b_smart_search.png!');

    console.log('4. Capturing Module C (AI Duplicate Product Detection Queue) in Admin...');
    await page.goto('http://localhost:59580/Admin/AiDuplicateProduct/List', { waitUntil: 'domcontentloaded' });
    await page.evaluate(() => new Promise(r => setTimeout(r, 2000)));
    await page.screenshot({ path: path.join(outputDir, 'module_c_duplicate_detection.png'), fullPage: false });
    console.log('Saved module_c_duplicate_detection.png!');

    await browser.close();
})();
