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

    console.log('2. Explicitly setting working language to Persian (LanguageId = 2)...');
    await page.goto('http://localhost:59580/changelanguage/2?returnurl=/', { waitUntil: 'domcontentloaded' });

    console.log('3. Navigating to seller product add page (/seller/product/add)...');
    const response = await page.goto('http://localhost:59580/seller/product/add', { waitUntil: 'domcontentloaded' });
    console.log('Response status:', response.status());

    await page.evaluate(() => new Promise(r => setTimeout(r, 2000)));

    console.log('4. Saving clean Persian seller product add screenshot...');
    await page.screenshot({ path: path.join(outputDir, 'step4_seller_product_add.png'), fullPage: false });
    console.log('Saved 100% Persian step4_seller_product_add.png!');

    await browser.close();
})();
