const puppeteer = require('./node_modules/puppeteer-core');
const fs = require('fs');
const path = require('path');

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const outputDir = path.join(__dirname, 'images');

if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

const steps = [
    { name: 'step1_admin_discounts.png', url: 'http://localhost:59580/Admin/Discount/List' },
    { name: 'step2_amazing_discounts.png', url: 'http://localhost:59580/amazing-discounts' },
    { name: 'step3_seller_dashboard.png', url: 'http://localhost:59580/seller/dashboard' },
    { name: 'step4_seller_product_add.png', url: 'http://localhost:59580/seller/product/add' },
    { name: 'step5_admin_seller_marketing.png', url: 'http://localhost:59580/Admin/SellerMarketing/List' },
    { name: 'step6_admin_group_purchase.png', url: 'http://localhost:59580/Admin/GroupPurchase/List' },
    { name: 'step7_admin_reward_rules.png', url: 'http://localhost:59580/Admin/RewardRule/List' },
    { name: 'step8_customer_leader_groups.png', url: 'http://localhost:59580/customer/leader-groups' },
    { name: 'step9_customer_subgroup_history.png', url: 'http://localhost:59580/customer/subgroup-history' },
    { name: 'step10_customer_wallet.png', url: 'http://localhost:59580/customer/wallet' },
    { name: 'step11_customer_lottery.png', url: 'http://localhost:59580/customer/lottery' }
];

(async () => {
    console.log('Launching Chrome...');
    const browser = await puppeteer.launch({
        executablePath: chromePath,
        headless: true,
        args: ['--no-sandbox', '--disable-setuid-sandbox', '--window-size=1440,900']
    });

    const page = await browser.newPage();
    await page.setViewport({ width: 1440, height: 900 });

    console.log('Logging in as admin@yourStore.com...');
    try {
        await page.goto('http://localhost:59580/login', { waitUntil: 'domcontentloaded' });
        await page.type('#Email', 'admin@yourStore.com');
        await page.type('#Password', 'admin');
        await page.click('button.login-button');
        await page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 10000 }).catch(() => {});
        console.log('Logged in!');
    } catch (e) {
        console.log('Login note:', e.message);
    }

    console.log('Switching to Persian language...');
    try {
        await page.goto('http://localhost:59580/changelanguage/2?returnurl=/', { waitUntil: 'domcontentloaded' });
    } catch (e) {}

    for (const step of steps) {
        console.log(`Navigating to ${step.url}...`);
        try {
            await page.goto(step.url, { waitUntil: 'domcontentloaded', timeout: 15000 });
            await page.evaluate(() => new Promise(r => setTimeout(r, 1200)));
            const savePath = path.join(outputDir, step.name);
            await page.screenshot({ path: savePath, fullPage: false });
            console.log(`Saved screenshot: ${step.name}`);
        } catch (err) {
            console.error(`Error taking screenshot for ${step.name}:`, err.message);
        }
    }

    await browser.close();
    console.log('All screenshots captured successfully!');
})();
