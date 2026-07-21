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

    console.log('1. Navigating to login page...');
    await page.goto('http://localhost:59580/login', { waitUntil: 'networkidle2' });

    console.log('2. Entering credentials...');
    await page.type('#Email', 'admin@yourStore.com');
    await page.type('#Password', 'admin');
    
    console.log('3. Clicking login button...');
    await Promise.all([
        page.waitForNavigation({ waitUntil: 'networkidle2' }),
        page.click('button.login-button')
    ]);

    console.log('4. Visiting leader-groups...');
    await page.goto('http://localhost:59580/customer/leader-groups', { waitUntil: 'networkidle2' });

    await page.evaluate(() => new Promise(r => setTimeout(r, 1500)));
    await page.screenshot({ path: path.join(outputDir, 'step8_customer_leader_groups.png'), fullPage: false });
    console.log('Saved authenticated step8_customer_leader_groups.png!');

    await browser.close();
})();
