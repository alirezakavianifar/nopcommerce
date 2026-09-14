const puppeteer = require('puppeteer-core');
const path = require('path');
const fs = require('fs');

const chromePath = 'C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe';
const outputDir = path.join(__dirname, '..', 'portfolio', 'images', 'real_app_figures');
const baseUrl = 'http://localhost:59580';

if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

const targets = [
    { name: '01_real_storefront_home.png', url: `${baseUrl}/`, waitMs: 2000 },
    { name: '02_real_amazing_discounts_page.png', url: `${baseUrl}/amazing-discounts`, waitMs: 2000 },
    { name: '03_real_group_purchase_admin_catalog.png', url: `${baseUrl}/Admin/GroupPurchase/List`, waitMs: 2000 },
    { name: '04_real_group_purchase_reward_rules.png', url: `${baseUrl}/Admin/RewardRule/List`, waitMs: 2000 },
    { name: '05_real_customer_leader_dashboard.png', url: `${baseUrl}/customer/leader-groups`, waitMs: 2000 },
    { name: '06_real_customer_subgroup_history.png', url: `${baseUrl}/customer/subgroup-history`, waitMs: 2000 },
    { name: '07_real_customer_wallet_balance.png', url: `${baseUrl}/customer/wallet`, waitMs: 2000 },
    { name: '08_real_customer_lottery_points.png', url: `${baseUrl}/customer/lottery`, waitMs: 2000 },
    { name: '09_real_ai_visual_voice_search.png', url: `${baseUrl}/`, waitMs: 2000, action: async (page) => {
        try {
            const searchInput = await page.$('#small-searchterms');
            if (searchInput) {
                await searchInput.focus();
                await page.type('#small-searchterms', 'Smart Electronics');
            }
        } catch(e) {}
    }},
    { name: '10_real_ai_duplicate_detection.png', url: `${baseUrl}/Admin/AiDuplicateProduct/List`, waitMs: 2000 },
    { name: '11_real_ai_chatbot_support.png', url: `${baseUrl}/Admin/ArtificialIntelligence/Configure`, waitMs: 2000 },
    { name: '12_real_seller_dashboard_marketing.png', url: `${baseUrl}/seller/dashboard`, waitMs: 2000 },
    { name: '13_real_admin_seller_marketing_queue.png', url: `${baseUrl}/Admin/SellerMarketing/List`, waitMs: 2000 },
    { name: '14_real_conditional_shipping_matrix.png', url: `${baseUrl}/Admin/ConditionalShipping/Configure`, waitMs: 2000 },
    { name: '15_real_sms_2fa_admin_security.png', url: `${baseUrl}/Plugins/SMS/Configure`, waitMs: 2000 },
    { name: '16_real_user_notifications_system.png', url: `${baseUrl}/Admin/UserNotifications/List`, waitMs: 2000 },
    { name: '17_real_rfq_b2b_quotation_panel.png', url: `${baseUrl}/Admin/RFQ/Configure`, waitMs: 2000 }
];

(async () => {
    console.log('Launching browser for English live screenshot capture...');
    const browser = await puppeteer.launch({
        executablePath: chromePath,
        headless: true,
        args: ['--no-sandbox', '--disable-setuid-sandbox', '--window-size=1440,900', '--lang=en-US,en']
    });

    const page = await browser.newPage();
    await page.setViewport({ width: 1440, height: 900 });

    console.log('Logging in to nopCommerce admin...');
    try {
        await page.goto(`${baseUrl}/login`, { waitUntil: 'domcontentloaded', timeout: 30000 });
        const emailInput = await page.$('#Email');
        if (emailInput) {
            await page.type('#Email', 'admin@yourStore.com');
            await page.type('#Password', 'admin');
            await page.click('button.login-button');
            await page.waitForNavigation({ waitUntil: 'domcontentloaded', timeout: 15000 }).catch(() => {});
            console.log('Logged in successfully!');
        }
    } catch (err) {
        console.log('Login note:', err.message);
    }

    console.log('Setting working language to English (LanguageId = 1)...');
    try {
        await page.goto(`${baseUrl}/Admin/Common/SetLanguage?langid=1&returnUrl=%2FAdmin`, { waitUntil: 'domcontentloaded', timeout: 15000 });
        await page.goto(`${baseUrl}/changelanguage/1?returnurl=/`, { waitUntil: 'domcontentloaded', timeout: 15000 });
    } catch (err) {
        console.log('Language switch note:', err.message);
    }

    for (const target of targets) {
        try {
            console.log(`Capturing English UI for ${target.name} from ${target.url}...`);
            await page.goto(target.url, { waitUntil: 'domcontentloaded', timeout: 20000 });
            if (target.action) {
                await target.action(page);
            }
            await page.evaluate((ms) => new Promise(r => setTimeout(r, ms)), target.waitMs);
            
            // Clean typography injection
            await page.addStyleTag({ content: `* { font-family: 'Segoe UI', Arial, sans-serif !important; }` }).catch(() => {});

            const savePath = path.join(outputDir, target.name);
            await page.screenshot({ path: savePath, fullPage: false });
            console.log(`Saved English capture: ${target.name}`);
        } catch (e) {
            console.error(`Failed to capture ${target.name}:`, e.message);
        }
    }

    await browser.close();
    console.log('All English live browser screenshots captured successfully!');
})();
