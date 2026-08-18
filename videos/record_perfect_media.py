import os
import time
import subprocess
from datetime import timedelta
from playwright.sync_api import sync_playwright

output_dir = r"e:\projects\nopCommerce_4.90.3_Source\videos"
screenshots_dir = os.path.join(output_dir, "screenshots_UserNotifications")
os.makedirs(screenshots_dir, exist_ok=True)

chrome_path = r"C:\Program Files\Google\Chrome\Application\chrome.exe"
ffmpeg_path = r"C:\Python314\Lib\site-packages\imageio_ffmpeg\binaries\ffmpeg-win-x86_64-v7.1.exe"
font_path = os.path.join(output_dir, "Vazirmatn-Bold.ttf")

print("Starting High-Fidelity Persian Video Recording & Screenshot Capture for User Notifications System...")

events = []
def record_event(name, start_t, end_t, text_srt, text_ass):
    events.append({
        "name": name,
        "start": start_t,
        "end": end_t,
        "text_srt": text_srt,
        "text_ass": text_ass
    })
    print(f"[{start_t:.1f}s -> {end_t:.1f}s] {name}", flush=True)

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
    
    # Start timer
    t0 = time.time()
    def curr_t():
        return time.time() - t0

    # --- Scene 1: Login to Admin ---
    s1_start = curr_t()
    page.goto("http://localhost:59580/en/login", wait_until="domcontentloaded")
    page.wait_for_timeout(1000)
    page.fill("#Email", "admin@yourStore.com")
    page.wait_for_timeout(400)
    page.fill("#Password", "admin")
    page.wait_for_timeout(400)
    page.click("button.login-button")
    page.wait_for_timeout(2000)
    s1_end = curr_t()
    record_event(
        "Admin Login & Dashboard Access",
        s1_start, s1_end,
        "ورود به پنل مدیریت nopCommerce و دسترسی به سامانه اعلانات کاربران\n(User Notifications & Workflows)",
        r"‫ورود به پنل مدیریت nopCommerce و دسترسی به سامانه اعلانات کاربران‬\N‫(User Notifications & Workflows)‬"
    )

    # --- Scene 2: Admin Automated Workflows List ---
    s2_start = curr_t()
    page.goto("http://localhost:59580/Admin/UserNotifications/Workflows", wait_until="domcontentloaded")
    page.wait_for_timeout(2000)
    page.screenshot(path=os.path.join(screenshots_dir, "01_admin_workflows_list.png"))
    page.wait_for_timeout(3000)
    s2_end = curr_t()
    record_event(
        "Admin Automated Workflows",
        s2_start, s2_end,
        "مشاهده لیست فرآیندهای خودکار اعلانات بر اساس محرک‌های هوشمند\n(ثبت‌نام، سبد رها شده، وضعیت سفارش و تخفیف پویا)",
        r"‫مشاهده لیست فرآیندهای خودکار اعلانات بر اساس محرک‌های هوشمند‬\N‫(ثبت‌نام، سبد رها شده، وضعیت سفارش و تخفیف پویا)‬"
    )

    # --- Scene 3: Admin Create / Configure Workflow ---
    s3_start = curr_t()
    page.goto("http://localhost:59580/Admin/UserNotifications/CreateWorkflow", wait_until="domcontentloaded")
    page.wait_for_timeout(2000)
    page.screenshot(path=os.path.join(screenshots_dir, "02_admin_create_workflow.png"))
    page.wait_for_timeout(3000)
    s3_end = curr_t()
    record_event(
        "Admin Workflow Configuration",
        s3_start, s3_end,
        "تعریف فرآیند جدید، تنظیم گام‌های زمانی و کانال‌های ارسال چندگانه\n(پیامک، ایمیل، پاپ‌آپ و صندوق ورودی)",
        r"‫تعریف فرآیند جدید، تنظیم گام‌های زمانی و کانال‌های ارسال چندگانه‬\N‫(پیامک، ایمیل، پاپ‌آپ و صندوق ورودی)‬"
    )

    # --- Scene 4: Admin Storefront Announcements ---
    s4_start = curr_t()
    page.goto("http://localhost:59580/Admin/UserNotifications/List", wait_until="domcontentloaded")
    page.wait_for_timeout(2000)
    page.screenshot(path=os.path.join(screenshots_dir, "03_admin_announcements_list.png"))
    page.wait_for_timeout(3000)
    s4_end = curr_t()
    record_event(
        "Admin Announcements Manager",
        s4_start, s4_end,
        "مدیریت اطلاعیه‌ها و بنرهای سراسری فروشگاه با زمان‌بندی دقیق\n(/Admin/UserNotifications/List)",
        r"‫مدیریت اطلاعیه‌ها و بنرهای سراسری فروشگاه با زمان‌بندی دقیق‬\N‫(/Admin/UserNotifications/List)‬"
    )

    # --- Scene 5: Admin FarazSMS & SMS Gateway Settings ---
    s5_start = curr_t()
    page.goto("http://localhost:59580/Admin/UserNotifications/FarazSms", wait_until="domcontentloaded")
    page.wait_for_timeout(2000)
    page.screenshot(path=os.path.join(screenshots_dir, "04_admin_faraz_sms_config.png"))
    page.wait_for_timeout(3000)
    s5_end = curr_t()
    record_event(
        "Admin FarazSMS Configuration",
        s5_start, s5_end,
        "پیکربندی درگاه پیامک خدماتی FarazSMS / IPPanel و کدهای پترن سریع\n(/Admin/UserNotifications/FarazSms)",
        r"‫پیکربندی درگاه پیامک خدماتی FarazSMS / IPPanel و کدهای پترن سریع‬\N‫(/Admin/UserNotifications/FarazSms)‬"
    )

    # --- Scene 6: Admin Delivery Queue & Logs ---
    s6_start = curr_t()
    page.goto("http://localhost:59580/Admin/UserNotifications/Queue", wait_until="domcontentloaded")
    page.wait_for_timeout(2000)
    page.screenshot(path=os.path.join(screenshots_dir, "05_admin_delivery_queue_logs.png"))
    page.wait_for_timeout(3000)
    s6_end = curr_t()
    record_event(
        "Admin Delivery Queue & Logs",
        s6_start, s6_end,
        "صف ارسال و گزارش لاگ وضعیت پیام‌ها در کلیه کانال‌های ارتباطی\n(/Admin/UserNotifications/Queue)",
        r"‫صف ارسال و گزارش لاگ وضعیت پیام‌ها در کلیه کانال‌های ارتباطی‬\N‫(/Admin/UserNotifications/Queue)‬"
    )

    # --- Scene 7: Storefront Persian Homepage & Top Announcement ---
    s7_start = curr_t()
    page.goto("http://localhost:59580/fa/", wait_until="domcontentloaded")
    page.wait_for_timeout(2500)
    page.screenshot(path=os.path.join(screenshots_dir, "06_storefront_homepage_fa.png"))
    page.wait_for_timeout(2500)
    s7_end = curr_t()
    record_event(
        "Storefront Homepage & Top Banner",
        s7_start, s7_end,
        "ورود به فروشگاه به زبان فارسی و مشاهده بنر اطلاعیه و زنگوله اعلانات در هدر\n(Notification Bell & Storefront Announcement Bar)",
        r"‫ورود به فروشگاه به زبان فارسی و مشاهده بنر اطلاعیه و زنگوله اعلانات در هدر‬\N‫(Notification Bell & Storefront Announcement Bar)‬"
    )

    # --- Scene 8: Persian Notification Bell Flyout ---
    s8_start = curr_t()
    bell = page.locator("#notif-bell-toggle")
    if bell.is_visible():
        bell.click()
        page.wait_for_timeout(1500)
        page.screenshot(path=os.path.join(screenshots_dir, "07_storefront_bell_flyout_fa.png"))
        page.wait_for_timeout(2000)
        # Click through tabs
        try:
            tab_promo = page.locator(".notif-flyout-tab[data-category='Promotion']")
            if tab_promo.is_visible():
                tab_promo.click()
                page.wait_for_timeout(1500)
            tab_order = page.locator(".notif-flyout-tab[data-category='Order']")
            if tab_order.is_visible():
                tab_order.click()
                page.wait_for_timeout(1500)
            tab_all = page.locator(".notif-flyout-tab[data-category='All']")
            if tab_all.is_visible():
                tab_all.click()
                page.wait_for_timeout(1500)
        except Exception as ex:
            print("Tab click exception:", ex)
    s8_end = curr_t()
    record_event(
        "Storefront Persian Bell Flyout",
        s8_start, s8_end,
        "باز کردن منوی کشویی اعلانات با تب‌های دسته‌بندی، کارت‌های پیام و کپی آسان کوپن\n(همه، سفارش‌ها، تخفیف‌ها و پیشنهادات، سیستم)",
        r"‫باز کردن منوی کشویی اعلانات با تب‌های دسته‌بندی، کارت‌های پیام و کپی آسان کوپن‬\N‫(همه، سفارش‌ها، تخفیف‌ها و پیشنهادات، سیستم)‬"
    )

    # --- Scene 9: Customer Notification Hub (Inbox) ---
    s9_start = curr_t()
    page.goto("http://localhost:59580/fa/customer/notifications", wait_until="domcontentloaded")
    page.wait_for_timeout(2000)
    page.screenshot(path=os.path.join(screenshots_dir, "08_customer_notification_hub_inbox_fa.png"))
    page.wait_for_timeout(2000)
    # Search filter demo
    try:
        search_box = page.locator("#notif-search-input, input[placeholder*='جستجو']")
        if search_box.is_visible():
            search_box.fill("تخفیف")
            page.wait_for_timeout(1500)
            search_box.fill("")
            page.wait_for_timeout(1000)
    except Exception as ex:
        print("Search demo exception:", ex)
    page.wait_for_timeout(2000)
    s9_end = curr_t()
    record_event(
        "Customer Notification Hub (Inbox)",
        s9_start, s9_end,
        "هاب اعلانات و صندوق ورودی پیام‌های حساب کاربری (/customer/notifications)\nفیلتر موضوعات، جستجوی هوشمند کدها و مدیریت اعلان‌ها",
        r"‫هاب اعلانات و صندوق ورودی پیام‌های حساب کاربری (/customer/notifications)‬\N‫فیلتر موضوعات، جستجوی هوشمند کدها و مدیریت اعلان‌ها‬"
    )

    # --- Scene 10: Customer Notification Preferences & Audio Chime ---
    s10_start = curr_t()
    page.goto("http://localhost:59580/fa/customer/notifications/preferences", wait_until="domcontentloaded")
    page.wait_for_timeout(2000)
    page.screenshot(path=os.path.join(screenshots_dir, "09_customer_preferences_dashboard_fa.png"))
    page.wait_for_timeout(2000)
    
    # Save preferences to show green confirmation banner
    try:
        save_btn = page.locator("button[type='submit']")
        if save_btn.is_visible():
            save_btn.click()
            page.wait_for_timeout(2000)
            page.screenshot(path=os.path.join(screenshots_dir, "10_customer_preferences_saved_fa.png"))
    except Exception as ex:
        print("Save button exception:", ex)
    page.wait_for_timeout(2500)
    s10_end = curr_t()
    record_event(
        "Customer Notification Preferences",
        s10_start, s10_end,
        "داشبورد شخصی‌سازی تنظیمات اعلانات، تست صدای زنگ و تایید سبز ذخیره‌سازی\n(/customer/notifications/preferences)",
        r"‫داشبورد شخصی‌سازی تنظیمات اعلانات، تست صدای زنگ و تایید سبز ذخیره‌سازی‬\N‫(/customer/notifications/preferences)‬"
    )

    # --- Scene 11: Final Closing View ---
    s11_start = curr_t()
    page.goto("http://localhost:59580/fa/", wait_until="domcontentloaded")
    page.wait_for_timeout(3000)
    s11_end = curr_t()
    record_event(
        "System Ready",
        s11_start, s11_end,
        "آماده‌سازی و استقرار کامل سامانه اعلانات و فرآیندهای خودکار nopCommerce\n(User Notifications & Workflows System)",
        r"‫آماده‌سازی و استقرار کامل سامانه اعلانات و فرآیندهای خودکار nopCommerce‬\N‫(User Notifications & Workflows System)‬"
    )

    context.close()
    video_raw_path = page.video.path()
    browser.close()

print(f"Recorded raw video to: {video_raw_path}", flush=True)

# File paths
base_video_mp4 = os.path.join(output_dir, "User Notifications & Workflows System.mp4")
subtitled_video_mp4 = os.path.join(output_dir, "User Notifications & Workflows System_subtitled.mp4")
srt_file = os.path.join(output_dir, "User Notifications & Workflows System_fa.srt")
ass_file = os.path.join(output_dir, "User Notifications & Workflows System_fa.ass")

# Convert raw video to standard MP4
print(f"Converting raw video to standard 30fps MP4: {base_video_mp4}...", flush=True)
conv_cmd = [
    ffmpeg_path, "-y",
    "-i", video_raw_path,
    "-c:v", "libx264",
    "-pix_fmt", "yuv420p",
    "-r", "30",
    base_video_mp4
]
subprocess.run(conv_cmd, check=True)

# Generate synchronized SRT
def format_srt_time(seconds):
    td = timedelta(seconds=seconds)
    total_sec = int(td.total_seconds())
    millis = int((td.total_seconds() - total_sec) * 1000)
    hours = total_sec // 3600
    minutes = (total_sec % 3600) // 60
    sec = total_sec % 60
    return f"{hours:02d}:{minutes:02d}:{sec:02d},{millis:03d}"

def format_ass_time(seconds):
    td = timedelta(seconds=seconds)
    total_sec = int(td.total_seconds())
    centis = int((td.total_seconds() - total_sec) * 100)
    hours = total_sec // 3600
    minutes = (total_sec % 3600) // 60
    sec = total_sec % 60
    return f"{hours:d}:{minutes:02d}:{sec:02d}.{centis:02d}"

srt_lines = []
for idx, ev in enumerate(events, start=1):
    t1 = format_srt_time(ev["start"])
    t2 = format_srt_time(ev["end"])
    srt_lines.append(f"{idx}\n{t1} --> {t2}\n{ev['text_srt']}\n")

with open(srt_file, "w", encoding="utf-8") as f:
    f.write("\n".join(srt_lines))
print(f"Synchronized SRT created: {srt_file}", flush=True)

# Generate synchronized ASS
ass_header = """[Script Info]
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
"""

ass_events = []
for ev in events:
    t1 = format_ass_time(ev["start"])
    t2 = format_ass_time(ev["end"])
    ass_events.append(f"Dialogue: 0,{t1},{t2},Default,,0,0,0,,{ev['text_ass']}")

with open(ass_file, "w", encoding="utf-8") as f:
    f.write(ass_header + "\n".join(ass_events) + "\n")
print(f"Synchronized ASS created: {ass_file}", flush=True)

# Burn subtitles with FFmpeg
ass_escaped = ass_file.replace("\\", "/").replace(":", "\\:")
fonts_dir_escaped = output_dir.replace("\\", "/").replace(":", "\\:")

sub_filter = f"ass='{ass_escaped}':fontsdir='{fonts_dir_escaped}'"
print(f"Burning synchronized subtitles into {subtitled_video_mp4}...", flush=True)
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
    print("ASS filter failed, trying subtitles filter:", burn_res.stderr, flush=True)
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

print(f"SUCCESS: Subtitled video created at: {subtitled_video_mp4}", flush=True)
