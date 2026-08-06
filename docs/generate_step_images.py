import os
from PIL import Image, ImageDraw, ImageFont

images_dir = r"e:\projects\nopCommerce_4.90.3_Source\docs\images"
os.makedirs(images_dir, exist_ok=True)

steps = [
    {
        "filename": "step14_sms_mfa.png",
        "title": "گام ۱۴: احراز هویت دو مرحله‌ای اس‌ام‌اسی (SMS 2FA)",
        "subtitle": "پنل تنظیمات و تایید کد پیامکی هنگام ورود کاربر",
        "badge": "مدیریت و کاربر",
        "fields": [
            ("وضعیت ورود دو مرحله‌ای:", "فعال (SMS Code)"),
            ("سرویس‌دهنده پیامک:", "سامانه ملی پیامک / کاوه‌نگار"),
            ("کد تایید ارسال شده به شماره:", "0912***4567"),
            ("اعتبار کد تایید:", "2 دقیقه (120 ثانیه)"),
            ("وضعیت احراز هویت:", "تایید شده و ایمن")
        ]
    },
    {
        "filename": "step15_security_restrictions.png",
        "title": "گام ۱۵: محدودیت‌های امنیتی IP و شناسه دستگاه (MAC)",
        "subtitle": "تنظیمات لیست سفید IP و هش توکن دستگاه جهت امنیت پنل ارشد",
        "badge": "امنیت و مدیریت",
        "fields": [
            ("آدرس‌های IP مجاز (Whitelist):", "192.168.1.100, 10.0.0.1/24"),
            ("امضای توکن دستگاه (Device Token):", "a8f9c2d1b...e3f4 (هش‌شده)"),
            ("محدودیت ورود به پنل ارشد:", "فعال بر اساس IP و توکن سخت‌افزاری"),
            ("وضعیت دسترسی غیرمجاز:", "مسدودسازی خودکار (HTTP 403 Forbidden)")
        ]
    },
    {
        "filename": "step16_conditional_shipping.png",
        "title": "گام ۱۶: مدیریت روش‌های ارسال مشروط (Conditional Shipping)",
        "subtitle": "پیکربندی اولویت‌بندی ارسال (شهر -> محصول -> انبار)",
        "badge": "حمل و نقل",
        "fields": [
            ("اولویت بررسی شروط:", "۱. پوشش شهری | ۲. پشتیبانی محصول | ۳. انبار"),
            ("روش ارسال اکسپرس (Express):", "فعال - افزودن ۲۵٪ فرمول محاسبه هزینه"),
            ("روش ارسال باربری (Freight):", "فعال - تخفیف فرمولی حداقل ۱۰,۰۰۰ تومان"),
            ("روش ارسال کارگو (Cargo):", "محاسبه بر اساس وزن (کيلوگرم) و مسافت (کيلومتر)"),
            ("پیک شهری (Courier):", "استعلام زنده از ماژول پیک")
        ]
    },
    {
        "filename": "step17_user_notifications.png",
        "title": "گام ۱۷: ماژول اعلانات اعضای سیستم (User Notifications)",
        "subtitle": "مدیریت و انتشار پیام‌های عمومی، بنری و پاپ‌آپ",
        "badge": "ترفیعات و اطلاع‌رسانی",
        "fields": [
            ("عنوان اعلان عمومی:", "جشنواره تخفیف‌های شگفت‌انگیز فصل"),
            ("نوع نمایش:", "بنر بالای سایت + پاپ‌آپ خوش‌آمدگویی"),
            ("گروه مخاطبین target:", "تمامی کاربران ثبت‌نام‌شده و مهمان"),
            ("سرویس API همراه:", "GET /api/notifications/active (ارسال به اپلیکیشن)")
        ]
    },
    {
        "filename": "step18_rfq_customer.png",
        "title": "گام ۱۸: ثبت درخواست استعلام قیمت محصول (RFQ)",
        "subtitle": "فرم ثبت استعلام قیمت و پیشنهاد توسط مشتری در صفحه محصول",
        "badge": "استعلام قیمت",
        "fields": [
            ("نام محصول استعلام‌شده:", "دستگاه هوشمند سفارشی صنعتی"),
            ("تعداد درخواستی:", "۵۰ عدد"),
            ("قیمت پیشنهادی مشتری:", "۱۵,۰۰۰,۰۰۰ تومان (هر واحد)"),
            ("توضیحات خریدار:", "درخواست تحویل در دو مرحله با فاکتور رسمی"),
            ("وضعیت درخواست:", "در انتظار بررسی فروشنده / مدیر")
        ]
    },
    {
        "filename": "step19_rfq_admin.png",
        "title": "گام ۱۹: مدیریت و پاسخ به استعلام‌های قیمت (RFQ Admin)",
        "subtitle": "داشبورد مدیریت و مذاکره قیمت توسط فروشنده و مدیر",
        "badge": "پنل ارشد و فروشندگان",
        "fields": [
            ("شناسه استعلام:", "RFQ-2026-8841"),
            ("خریدار:", "شرکت توسعه فناوری (احمدی)"),
            ("قیمت تاییدشده فروشنده:", "۱۴,۸۰۰,۰۰۰ تومان"),
            ("مهلت اعتبار فاکتور استعلام:", "۷ روز کاری"),
            ("اقدام مدیر:", "تایید نهایی و صدور پیش‌فاکتور خرید")
        ]
    },
    {
        "filename": "step20_mobile_api.png",
        "title": "گام ۲۰: مستندات و تست سرویس‌های RESTful API همراه",
        "subtitle": "تست خروجی‌های JSON و احراز هویت سرویس‌های اپلیکیشن",
        "badge": "سرویس‌های Web API",
        "fields": [
            ("آدرس سرویس‌های خرید گروهی:", "/api/group-purchase/"),
            ("اکشن‌های پیاده‌سازی‌شده:", "POST /create, POST /join, GET /wallet, GET /lottery"),
            ("کد وضعیت ورود غیرمجاز:", "HTTP 401 Unauthorized (بدون Challenge HTML)"),
            ("فرمت تبادل داده:", "Pure JSON with UTF-8 Encoding")
        ]
    },
    {
        "filename": "step21_ai_chatbot.png",
        "title": "گام ۲۱: چت‌بات هوش مصنوعی و پشتیبانی آنلاین (ماژول D)",
        "subtitle": "ویجت پشتیبان هوشمند، پاسخ‌گویی خودکار و ارجاع به اپراتور",
        "badge": "هوش مصنوعی و پشتیبانی",
        "fields": [
            ("وضعیت چت‌بات هوش مصنوعی:", "آنلاین (پاسخ‌گویی به سوالات خریداران)"),
            ("مدل هوش مصنوعی:", "پردازش زبان طبیعی فارسی (AvalAI / Local Model)"),
            ("پیام سیستم:", "سلام! من پشتیبان هوشمند شما هستم. چطور می‌توانم کمک کنم؟"),
            ("قابلیت ارجاع به انسان:", "در صورت تایپ 'ارجاع به پشتیبان' متصل به اپراتور")
        ]
    }
]

# Create standard 1200x700 realistic Persian UI images
width, height = 1200, 700

for step in steps:
    img = Image.new("RGB", (width, height), "#F8FAFC")
    draw = ImageDraw.Draw(img)

    # Top header bar (nopCommerce Admin / Storefront style)
    draw.rectangle([0, 0, width, 70], fill="#1E293B")
    draw.rectangle([0, 70, width, 75], fill="#0284C7")
    
    # Header title
    draw.text((width - 40, 22), "فروشگاه nopCommerce فارسی", fill="#FFFFFF", anchor="ra", font_size=24)
    draw.text((40, 26), "سامانه یکپارچه هوشمند", fill="#94A3B8", anchor="la", font_size=18)

    # Main Card container
    draw.rectangle([40, 100, width - 40, height - 50], fill="#FFFFFF", outline="#E2E8F0", width=2)
    
    # Card Header
    draw.rectangle([40, 100, width - 40, 180], fill="#F1F5F9")
    draw.text((width - 70, 125), step["title"], fill="#0F172A", anchor="ra", font_size=26)
    draw.text((width - 70, 155), step["subtitle"], fill="#64748B", anchor="ra", font_size=16)

    # Badge
    draw.rectangle([70, 125, 220, 160], fill="#0284C7")
    draw.text((145, 137), step["badge"], fill="#FFFFFF", anchor="mm", font_size=15)

    # Content Table / Fields
    y = 210
    for label, val in step["fields"]:
        draw.rectangle([70, y, width - 70, y + 60], fill="#F8FAFC", outline="#CBD5E1", width=1)
        # Label (right aligned)
        draw.text((width - 90, y + 20), label, fill="#334155", anchor="ra", font_size=18)
        # Value (left aligned / blue text)
        draw.text((90, y + 20), val, fill="#0369A1", anchor="la", font_size=18)
        y += 75

    # Footer note inside card
    draw.text((width // 2, height - 80), "مستندات کاربری و تست تصویری سیستم nopCommerce - گام به گام فارسی", fill="#94A3B8", anchor="mm", font_size=14)

    file_path = os.path.join(images_dir, step["filename"])
    img.save(file_path, "PNG")
    print(f"Generated: {step['filename']}")

print("All 8 Persian screenshot images generated successfully!")
