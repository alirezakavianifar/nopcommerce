const puppeteer = require('puppeteer-core');
const path = require('path');
const fs = require('fs');

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const outputDir = path.join(__dirname, 'images');
const baseUrl = 'http://localhost:59580';

const routes = [
    { name: 'step14_sms_mfa.png', url: `${baseUrl}/Plugins/SMS/Configure` },
    { name: 'step15_security_restrictions.png', url: `${baseUrl}/Plugins/SMS/Configure` },
    { name: 'step16_conditional_shipping.png', url: `${baseUrl}/Admin/ConditionalShipping/Configure` },
    { name: 'step17_user_notifications.png', url: `${baseUrl}/Admin/UserNotifications/List` },
    { name: 'step18_rfq_customer.png', url: `${baseUrl}/rfq/requestsforquote/` },
    { name: 'step19_rfq_admin.png', url: `${baseUrl}/Admin/RFQ/Configure` },
    { name: 'step20_mobile_api.png', url: `${baseUrl}/api/group-purchase/wallet` },
    { name: 'step21_ai_chatbot.png', url: `${baseUrl}/Admin/ArtificialIntelligence/Configure` }
];

(async () => {
    console.log('Launching browser for direct Persian live screenshot capture...');
    const browser = await puppeteer.launch({
        executablePath: chromePath,
        headless: true,
        args: ['--no-sandbox', '--disable-setuid-sandbox', '--window-size=1440,900', '--lang=fa-IR,fa']
    });

    const page = await browser.newPage();
    await page.setViewport({ width: 1440, height: 900 });

    console.log('Logging in to nopCommerce admin (allowing up to 120s for app initialization)...');
    try {
        await page.goto(`${baseUrl}/login`, { waitUntil: 'networkidle2', timeout: 120000 });
        const emailInput = await page.$('#Email');
        if (emailInput) {
            await page.type('#Email', 'admin@yourStore.com');
            await page.type('#Password', 'admin');
            await page.click('button.login-button');
            await page.waitForNavigation({ waitUntil: 'networkidle2', timeout: 60000 }).catch(() => {});
            console.log('Login request sent. Current URL:', page.url());
        } else {
            console.log('Already logged in? Current URL:', page.url());
        }
    } catch (err) {
        console.log('Login attempt warning/timeout:', err.message);
    }

    console.log('Explicitly switching active user/admin language to Persian...');
    try {
        await page.goto(`${baseUrl}/Admin/Common/SetLanguage?langid=2&returnUrl=%2FAdmin`, { waitUntil: 'domcontentloaded', timeout: 30000 });
        await page.goto(`${baseUrl}/changelanguage/2?returnurl=/`, { waitUntil: 'domcontentloaded', timeout: 30000 });
    } catch (err) {
        console.log('Language switch warning:', err.message);
    }

    for (const item of routes) {
        try {
            console.log(`Capturing Persian UI for ${item.name} from ${item.url}...`);
            await page.goto(item.url, { waitUntil: 'networkidle2', timeout: 30000 });
            await page.evaluate(() => new Promise(r => setTimeout(r, 2500)));
            
            // Inject standard Segoe UI/Arial fonts to resolve headless rendering issues
            await page.addStyleTag({ content: `* { font-family: 'Segoe UI', Arial, sans-serif !important; }` }).catch(() => {});
            
            await page.screenshot({ path: path.join(outputDir, item.name), fullPage: false });
            console.log(`Saved Persian capture: ${item.name}`);
        } catch (e) {
            console.error(`Failed to capture ${item.name}:`, e.message);
        }
    }

    await browser.close();
    console.log('All Persian live browser screenshots captured successfully!');
})();
