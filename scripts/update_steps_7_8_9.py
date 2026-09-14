import os
import time
from playwright.sync_api import sync_playwright

def run():
    artifact_dir = r"C:\Users\Administrator\.gemini\antigravity-ide\brain\15c16276-b5dd-43c2-90b5-ae994e9413c3"
    docs_images_dir = r"e:\projects\nopCommerce_4.90.3_Source\docs\images"

    chrome_path = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
    base_url = "http://localhost:59580"

    print("Launching browser for updating Steps 7, 8, 9...")
    with sync_playwright() as p:
        browser = p.chromium.launch(
            executable_path=chrome_path,
            headless=True,
            args=["--start-maximized", "--disable-gpu", "--no-sandbox"]
        )
        context = browser.new_context(
            viewport={"width": 1440, "height": 900}
        )
        page = context.new_page()
        page.set_default_timeout(60000)

        def save_step(filename):
            path1 = os.path.join(artifact_dir, filename)
            path2 = os.path.join(docs_images_dir, filename)
            page.screenshot(path=path1, full_page=False)
            page.screenshot(path=path2, full_page=False)
            print(f"Saved: {filename}")

        # Login in Persian
        print("Logging in...")
        page.goto(f"{base_url}/changelanguage/2?returnurl=/login")
        page.wait_for_timeout(1500)
        page.fill("#Email", "admin@yourStore.com")
        page.wait_for_timeout(200)
        page.fill("#Password", "admin")
        page.wait_for_timeout(300)
        page.click("button.login-button")
        page.wait_for_timeout(2500)

        # -------------------------------------------------------------
        # Step 7: Customer Notifications Inbox (/fa/customer/notifications)
        # -------------------------------------------------------------
        print("Step 7: Capturing Customer Notifications Inbox...")
        page.goto(f"{base_url}/fa/customer/notifications")
        page.wait_for_timeout(2500)
        save_step("mod1_07_customer_inbox.png")

        # -------------------------------------------------------------
        # Step 8: Customer Notification Preferences (/fa/customer/notifications/preferences)
        # -------------------------------------------------------------
        print("Step 8: Capturing Customer Notification Preferences...")
        page.goto(f"{base_url}/fa/customer/notifications/preferences")
        page.wait_for_timeout(2500)
        save_step("mod1_08_customer_preferences.png")

        # -------------------------------------------------------------
        # Step 9: Admin Automated Notification Workflows (/Admin/UserNotifications/Workflows)
        # -------------------------------------------------------------
        print("Step 9: Capturing Admin Workflows in Persian...")
        page.goto(f"{base_url}/Admin/UserNotifications/Workflows")
        page.wait_for_timeout(2500)
        save_step("mod1_09_automated_workflows.png")

        browser.close()
        print("Steps 7, 8, 9 refreshed successfully!")

if __name__ == "__main__":
    run()
