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

    console.log('2. Navigating to Admin Customer List...');
    await page.goto('http://localhost:59580/Admin/Customer/List', { waitUntil: 'domcontentloaded' });
    await page.evaluate(() => new Promise(r => setTimeout(r, 1000)));

    console.log('3. Navigating to Admin Vendors List...');
    await page.goto('http://localhost:59580/Admin/Vendor/List', { waitUntil: 'domcontentloaded' });
    await page.evaluate(() => new Promise(r => setTimeout(r, 1000)));

    // Set admin VendorId via Customer Edit page
    console.log('4. Navigating to Customer Edit page...');
    await page.goto('http://localhost:59580/Admin/Customer/Edit/1', { waitUntil: 'domcontentloaded' });
    await page.evaluate(() => new Promise(r => setTimeout(r, 1000)));

    const vendorSelect = await page.$('#VendorId');
    if (vendorSelect) {
        await page.select('#VendorId', '1');
        await Promise.all([
            page.waitForNavigation({ waitUntil: 'domcontentloaded' }),
            page.click('button[name="save"]')
        ]);
        console.log('Admin assigned to Vendor 1!');
    }

    console.log('5. Navigating to Seller Dashboard...');
    await page.goto('http://localhost:59580/seller/dashboard', { waitUntil: 'domcontentloaded' });
    await page.evaluate(() => new Promise(r => setTimeout(r, 2000)));

    console.log('6. Saving clean Persian seller dashboard screenshot...');
    await page.screenshot({ path: path.join(outputDir, 'step3_seller_dashboard.png'), fullPage: false });
    console.log('Saved clean step3_seller_dashboard.png!');

    await browser.close();
})();
