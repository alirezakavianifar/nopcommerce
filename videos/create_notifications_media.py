import os
import time
import subprocess
from playwright.sync_api import sync_playwright

output_dir = r"e:\projects\nopCommerce_4.90.3_Source\videos"
screenshots_dir = os.path.join(output_dir, "screenshots_UserNotifications")
os.makedirs(screenshots_dir, exist_ok=True)

chrome_path = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
ffmpeg_path = r"C:\Python314\Lib\site-packages\imageio_ffmpeg\binaries\ffmpeg-win-x86_64-v7.1.exe"
font_path = os.path.join(output_dir, "Vazirmatn-Bold.ttf")

print("Starting Playwright walkthrough for User Notifications System in Persian...")

with sync_playwright() as p:
    browser = p.chromium.launch(
        executable_path=chrome_path,
        headless=True,
        args=["--start-maximized", "--disable-gpu", "--no-sandbox"]
    )
    
    # 1728x1080 standard video resolution
    context = browser.new_context(
        viewport={"width": 1728, "height": 1080},
        record_video_dir=os.path.join(output_dir, "temp_rec"),
        record_video_size={"width": 1728, "height": 1080}
    )
    
    page = context.new_page()
    page.set_default_timeout(30000)
    
    # --- Step 1: Login as Admin ---
    print("1. Logging into Admin...")
    page.goto("http://localhost:59580/en/login")
    page.fill("#Email", "admin@yourStore.com")
    page.fill("#Password", "admin")
    page.click("button.login-button")
    page.wait_for_load_state("networkidle")
    time.sleep(1)
    
    # Switch to Persian
    print("2. Switching to Persian language...")
    page.goto("http://localhost:59580/fa/")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    
    # --- Step 2: Storefront Bell Icon & Flyout (Persian) ---
    print("3. Capturing Storefront Persian Bell Icon & Flyout...")
    page.screenshot(path=os.path.join(screenshots_dir, "01_storefront_homepage_fa.png"))
    
    # Hover & click bell
    bell = page.locator("#notif-bell-trigger")
    if bell.is_visible():
        bell.click()
        time.sleep(2)
        page.screenshot(path=os.path.join(screenshots_dir, "02_storefront_bell_flyout_fa.png"))
        time.sleep(2)
        # Close flyout
        bell.click()
        time.sleep(1)
    
    # --- Step 3: Customer Notification Preferences (Persian) ---
    print("4. Capturing Customer Notification Preferences...")
    page.goto("http://localhost:59580/fa/customer/notifications/preferences")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "03_customer_preferences_dashboard_fa.png"))
    
    # Test audio chime click
    try:
        page.locator("button[onclick*='playChime']").click()
        time.sleep(1.5)
    except Exception as e:
        print("Chime test click:", e)
        
    # Toggle switch and save
    page.locator("button[type='submit']").click()
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "04_customer_preferences_saved_fa.png"))
    time.sleep(1)
    
    # --- Step 4: Customer Notification Hub / Inbox (Persian) ---
    print("5. Capturing Customer Notification Hub (Inbox)...")
    page.goto("http://localhost:59580/fa/customer/notifications")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "05_customer_notification_hub_inbox_fa.png"))
    time.sleep(1)
    
    # --- Step 5: Admin Workflows Dashboard ---
    print("6. Capturing Admin Workflows Dashboard...")
    page.goto("http://localhost:59580/Admin/UserNotifications/Workflows")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "06_admin_workflows_list.png"))
    time.sleep(1)
    
    # --- Step 6: Admin Create / Configure Workflow ---
    print("7. Capturing Admin Create Workflow Form...")
    page.goto("http://localhost:59580/Admin/UserNotifications/CreateWorkflow")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "07_admin_create_workflow.png"))
    time.sleep(1)
    
    # --- Step 7: Admin Announcements Dashboard ---
    print("8. Capturing Admin Announcements Dashboard...")
    page.goto("http://localhost:59580/Admin/UserNotifications/List")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "08_admin_announcements_list.png"))
    time.sleep(1)
    
    # --- Step 8: Admin FarazSMS Gateway Settings ---
    print("9. Capturing Admin FarazSMS Settings...")
    page.goto("http://localhost:59580/Admin/UserNotifications/FarazSms")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "09_admin_faraz_sms_config.png"))
    time.sleep(1)
    
    # --- Step 9: Admin Delivery Queue Logs ---
    print("10. Capturing Admin Delivery Queue Logs...")
    page.goto("http://localhost:59580/Admin/UserNotifications/Queue")
    page.wait_for_load_state("networkidle")
    time.sleep(2)
    page.screenshot(path=os.path.join(screenshots_dir, "10_admin_delivery_queue_logs.png"))
    time.sleep(2)
    
    # Return to Persian Storefront Hub for final view
    print("11. Returning to Storefront Persian Hub...")
    page.goto("http://localhost:59580/fa/customer/notifications")
    page.wait_for_load_state("networkidle")
    time.sleep(3)
    
    context.close()
    video_path = page.video.path()
    browser.close()

print(f"Recorded raw video to: {video_path}")

# Rename / Move recorded video to clean MP4 name
base_video_mp4 = os.path.join(output_dir, "User Notifications & Workflows System.mp4")
subtitled_video_mp4 = os.path.join(output_dir, "User Notifications & Workflows System_subtitled.mp4")
srt_file = os.path.join(output_dir, "User Notifications & Workflows System_fa.srt")
ass_file = os.path.join(output_dir, "User Notifications & Workflows System_fa.ass")

# Convert webm video to standardized MP4
print(f"Converting raw video to standard MP4: {base_video_mp4}...")
conv_cmd = [
    ffmpeg_path, "-y",
    "-i", video_path,
    "-c:v", "libx264",
    "-pix_fmt", "yuv420p",
    "-r", "30",
    base_video_mp4
]
subprocess.run(conv_cmd, check=True)

# Get video duration
ffprobe_duration_cmd = [
    ffmpeg_path, "-i", base_video_mp4
]
res = subprocess.run(ffprobe_duration_cmd, stderr=subprocess.PIPE, stdout=subprocess.PIPE, text=True)
print("Video metadata parsed.")

# Create SRT Subtitles
srt_content = """1
00:00:00,500 --> 00:00:07,000
ورود به فروشگاه به زبان فارسی و مشاهده زنگوله اعلانات در هدر سایت
(Notification Bell Icon)

2
00:00:07,500 --> 00:00:14,000
باز کردن منوی کشویی اعلانات با تب‌های دسته‌بندی فارسی
(همه، سفارش‌ها، تخفیف‌ها و پیشنهادات، سیستم)

3
00:00:14,500 --> 00:00:22,000
داشبورد تنظیمات دریافت اعلانات کاربری (/customer/notifications/preferences)
سفارشی‌سازی اعلان‌های شناور، صدای زنگ، پیامک و ایمیل

4
00:00:22,500 --> 00:00:29,000
تست صدای زنگ و ذخیره‌سازی موفقیت‌آمیز تنظیمات دریافت اعلانات
با پیام تایید سبز فارسی

5
00:00:29,500 --> 00:00:36,000
صندوق پیام‌ها و هاب اعلان‌های کاربر (/customer/notifications)
فیلتر موضوعات، جستجوی هوشمند کدها و عملیات مدیریت اعلان‌ها

6
00:00:36,500 --> 00:00:43,000
پنل مدیریت پیشرفته nopCommerce - فرآیندهای خودکار اعلانات
(/Admin/UserNotifications/Workflows)

7
00:00:43,500 --> 00:00:49,000
ایجاد فرآیند خودکار با محرک‌های هوشمند
(ثبت‌نام، سبد خرید رها شده، وضعیت سفارش و تخفیف پویا)

8
00:00:49,500 --> 00:00:54,500
مدیریت اطلاعیه‌ها و بنرهای سراسری فروشگاه
(/Admin/UserNotifications/List)

9
00:00:55,000 --> 00:01:00,000
تنظیمات درگاه پیامک FarazSMS / IPPanel و کدهای پترن خدماتی
(/Admin/UserNotifications/FarazSms)

10
00:01:00,500 --> 00:01:08,000
گزارش و صف ارسال اعلانات چندکاناله و بازگشت به فروشگاه
(/Admin/UserNotifications/Queue)
"""

with open(srt_file, "w", encoding="utf-8") as f:
    f.write(srt_content)

print(f"Created SRT subtitle: {srt_file}")

# Create ASS Subtitles for high-quality font rendering
ass_content = """[Script Info]
Title: Persian Subtitles for User Notifications & Workflows System
ScriptType: v4.00+
WrapStyle: 0
ScaledBorderAndShadow: yes
YCbCr Matrix: None
PlayResX: 1728
PlayResY: 1080

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Vazirmatn,58,&H00FFFFFF,&H00000000,&H00000000,&H90000000,-1,0,0,0,100,100,0,0,1,4,2,2,40,40,65,1

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:00.50,0:00:07.00,Default,,0,0,0,,‫ورود به فروشگاه به زبان فارسی و مشاهده زنگوله اعلانات در هدر سایت‬\\N‫(Notification Bell Icon)‬
Dialogue: 0,0:00:07.50,0:00:14.00,Default,,0,0,0,,‫باز کردن منوی کشویی اعلانات با تب‌های دسته‌بندی فارسی‬\\N‫(همه، سفارش‌ها، تخفیف‌ها و پیشنهادات، سیستم)‬
Dialogue: 0,0:00:14.50,0:00:22.00,Default,,0,0,0,,‫داشبورد تنظیمات دریافت اعلانات کاربری (/customer/notifications/preferences)‬\\N‫سفارشی‌سازی اعلان‌های شناور، صدای زنگ، پیامک و ایمیل‬
Dialogue: 0,0:00:22.50,0:00:29.00,Default,,0,0,0,,‫تست صدای زنگ و ذخیره‌سازی موفقیت‌آمیز تنظیمات دریافت اعلانات‬\\N‫با پیام تایید سبز فارسی‬
Dialogue: 0,0:00:29.50,0:00:36.00,Default,,0,0,0,,‫صندوق پیام‌ها و هاب اعلان‌های کاربر (/customer/notifications)‬\\N‫فیلتر موضوعات، جستجوی هوشمند کدها و عملیات مدیریت اعلان‌ها‬
Dialogue: 0,0:00:36.50,0:00:43.00,Default,,0,0,0,,‫پنل مدیریت پیشرفته nopCommerce - فرآیندهای خودکار اعلانات‬\\N‫(/Admin/UserNotifications/Workflows)‬
Dialogue: 0,0:00:43.50,0:00:49.00,Default,,0,0,0,,‫ایجاد فرآیند خودکار با محرک‌های هوشمند‬\\N‫(ثبت‌نام، سبد خرید رها شده، وضعیت سفارش و تخفیف پویا)‬
Dialogue: 0,0:00:49.50,0:00:54.50,Default,,0,0,0,,‫مدیریت اطلاعیه‌ها و بنرهای سراسری فروشگاه‬\\N‫(/Admin/UserNotifications/List)‬
Dialogue: 0,0:00:55.00,0:01:00.00,Default,,0,0,0,,‫تنظیمات درگاه پیامک FarazSMS / IPPanel و کدهای پترن خدماتی‬\\N‫(/Admin/UserNotifications/FarazSms)‬
Dialogue: 0,0:01:00.50,0:01:08.00,Default,,0,0,0,,‫گزارش و صف ارسال اعلانات چندکاناله و بازگشت به فروشگاه‬\\N‫(/Admin/UserNotifications/Queue)‬
"""

with open(ass_file, "w", encoding="utf-8") as f:
    f.write(ass_content)

print(f"Created ASS subtitle: {ass_file}")

# Burn in subtitles using FFmpeg with ASS file
# Note: Escape path for ffmpeg filter
ass_escaped = ass_file.replace("\\", "/").replace(":", "\\:")
fonts_dir_escaped = output_dir.replace("\\", "/").replace(":", "\\:")

sub_filter = f"ass='{ass_escaped}':fontsdir='{fonts_dir_escaped}'"

print(f"Burning subtitles into {subtitled_video_mp4}...")
sub_cmd = [
    ffmpeg_path, "-y",
    "-i", base_video_mp4,
    "-vf", sub_filter,
    "-c:v", "libx264",
    "-pix_fmt", "yuv420p",
    "-c:a", "copy",
    subtitled_video_mp4
]

burn_res = subprocess.run(sub_cmd, stderr=subprocess.PIPE, stdout=subprocess.PIPE, text=True)
if burn_res.returncode != 0:
    print("ASS burn error, falling back to srt subtitles filter:", burn_res.stderr)
    srt_escaped = srt_file.replace("\\", "/").replace(":", "\\:")
    sub_cmd_srt = [
        ffmpeg_path, "-y",
        "-i", base_video_mp4,
        "-vf", f"subtitles='{srt_escaped}':fontsdir='{fonts_dir_escaped}'",
        "-c:v", "libx264",
        "-pix_fmt", "yuv420p",
        "-c:a", "copy",
        subtitled_video_mp4
    ]
    subprocess.run(sub_cmd_srt, check=True)

print(f"Successfully generated subtitled MP4: {subtitled_video_mp4}")
