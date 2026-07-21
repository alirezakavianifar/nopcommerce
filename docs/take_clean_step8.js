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
    try {
        await page.goto('http://localhost:59580/login', { waitUntil: 'domcontentloaded' });
        await page.type('#Email', 'admin@yourStore.com');
        await page.type('#Password', 'admin');
        await page.click('button.login-button');
        await page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 10000 }).catch(() => {});
    } catch(e) {}

    console.log('Switching to Persian language...');
    try {
        await page.goto('http://localhost:59580/changelanguage/2?returnurl=/customer/leader-groups', { waitUntil: 'domcontentloaded' });
        await page.evaluate(() => new Promise(r => setTimeout(r, 1200)));
        await page.screenshot({ path: path.join(outputDir, 'step8_customer_leader_groups.png'), fullPage: false });
        console.log('Saved clean step8_customer_leader_groups.png!');
    } catch (e) {
        console.error('Error:', e.message);
    }

    await browser.close();
})();
