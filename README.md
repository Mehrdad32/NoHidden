<div dir="rtl">

<p align="center">
  <img src="NoHidden/logo.png" width="96" alt="NoHidden Logo">
</p>

<h1 align="center">NoHidden 3</h1>

<p align="center">
  ابزار سبک ویندوز برای بازیابی فایل‌های مخفی‌شده روی فلش و بررسی الگوهای رایج بدافزارهای USB
</p>

<p align="center">
  <a href="README.en.md">🇬🇧 English README</a>
</p>

<p align="center">
  <a href="https://github.com/Mehrdad32/NoHidden/actions/workflows/build.yml">
    <img src="https://github.com/Mehrdad32/NoHidden/actions/workflows/build.yml/badge.svg" alt="Build">
  </a>
  <a href="https://github.com/Mehrdad32/NoHidden/releases/latest">
    <img src="https://img.shields.io/github/v/release/Mehrdad32/NoHidden?label=release" alt="Latest release">
  </a>
  <a href="LICENSE.txt">
    <img src="https://img.shields.io/badge/license-Apache%202.0-blue.svg" alt="Apache 2.0">
  </a>
</p>

---

## دانلود

نسخه ۳ به‌صورت **Self-contained** منتشر می‌شود؛ یعنی برای اجرای برنامه لازم نیست .NET را جداگانه نصب کنید.

| معماری | مناسب برای | دانلود |
|---|---|---|
| Windows 64-bit (x64) | تقریباً همه رایانه‌های امروزی | **[دانلود NoHidden 3 برای x64](https://github.com/Mehrdad32/NoHidden/releases/latest/download/NoHidden-v3.0.0-win-x64.exe)** |
| Windows 32-bit (x86) | ویندوزهای ۳۲ بیتی قدیمی‌تر | **[دانلود NoHidden 3 برای x86](https://github.com/Mehrdad32/NoHidden/releases/latest/download/NoHidden-v3.0.0-win-x86.exe)** |

صفحه همه نسخه‌ها و SHA-256 فایل‌های انتشار:

**[GitHub Releases](https://github.com/Mehrdad32/NoHidden/releases/latest)**

> اگر نمی‌دانید کدام نسخه را بگیرید، در بیشتر سیستم‌های امروزی نسخه **x64** انتخاب درست است.

---

## NoHidden چه مشکلی را حل می‌کند؟

بعضی بدافزارهای USB فایل‌ها و پوشه‌های واقعی کاربر را با Attributeهای **Hidden** و **System** مخفی می‌کنند و به‌جای آن‌ها میانبر، اسکریپت یا فایل اجرایی فریبنده قرار می‌دهند. در نتیجه ممکن است فلش پر باشد اما کاربر فایل‌های خودش را در Explorer نبیند.

NoHidden برای همین سناریو ساخته شده است:

- فایل‌ها و پوشه‌های کاربری مخفی‌شده را پیدا می‌کند.
- امکان بازگرداندن Visibility آن‌ها را بدون تغییر محتوا می‌دهد.
- الگوهای مشکوک رایج بدافزارهای USB را بررسی می‌کند.
- فایل مشکوک را بدون حذف، قابل‌اجرا نبودن می‌کند.
- امکان بررسی فایل با Microsoft Defender را می‌دهد.
- می‌تواند SHA-256 فایل را محلی محاسبه و گزارش همان Hash را در VirusTotal باز کند.
- از حذف کورکورانه فایل‌های EXE/BAT/VBS و مشابه آن‌ها خودداری می‌کند.

---

## قابلیت‌های نسخه 3.0.0

### بازیابی فایل‌های مخفی

NoHidden فایل‌ها و پوشه‌های کاربری دارای Attributeهای زیر را پیدا می‌کند:

- `Hidden`
- `System`
- یا ترکیب هر دو

برای این موارد دکمه **نمایش مجدد / Restore visibility** نمایش داده می‌شود. این عملیات فقط Attributeهای Hidden/System را برمی‌دارد و محتوای فایل را تغییر نمی‌دهد.

### تشخیص الگوهای مشکوک USB

Scanner موارد زیر را بررسی می‌کند:

- `autorun.inf`
- فایل‌های `.bat`، `.cmd`، `.vbs`، `.vbe`، `.js`، `.jse`، `.wsf`، `.wsh`، `.ps1` و `.hta`
- فایل‌های `.exe`، `.com`، `.scr` و `.pif`
- پسوند دوگانه مانند `photo.jpg.exe`
- فایل اجرایی همنام پوشه، مانند:
  - `Documents\`
  - `Documents.exe`
- Shortcutهای `.lnk`
- Shortcutهایی که ابزارهایی مانند CMD، PowerShell، WScript، CScript، MSHTA یا Rundll32 را اجرا می‌کنند
- فایل‌های اجرایی یا اسکریپت‌های Hidden/System

> تشخیص Heuristic به معنی «اثبات قطعی بدافزار بودن» نیست. NoHidden به‌جای حذف خودکار، دلیل تشخیص و اکشن‌های امن‌تر را نمایش می‌دهد.

---

## روند پیشنهادی استفاده

1. NoHidden را به‌صورت عادی اجرا کنید.
2. فلش USB را متصل کنید.
3. از بخش **USB scanner** درایو موردنظر را انتخاب کنید.
4. روی **بررسی حافظه USB / Scan USB** کلیک کنید.
5. نتیجه‌ها را مرور کنید.
6. برای فایل‌ها و پوشه‌های مخفی کاربری، **Restore visibility** را بزنید.
7. برای فایل‌های مشکوک ابتدا **Defender scan** یا **VirusTotal** را بررسی کنید.
8. اگر فایل همچنان مشکوک است و نمی‌خواهید آن را حذف کنید، **Neutralize** را انتخاب کنید.
9. پس از انجام عملیات، دوباره Scan بزنید.

اسکن اولیه **غیرمخرب** است و هیچ فایلی را تغییر نمی‌دهد.

---

## معنی وضعیت‌ها

| وضعیت | معنی |
|---|---|
| Recovery / بازیابی | فایل یا پوشه مخفی‌شده‌ای که NoHidden می‌تواند Visibility آن را برگرداند |
| Low / کم | الگوی ضعیف و نیازمند بررسی |
| Suspicious / مشکوک | رفتار یا محل فایل نیازمند توجه است |
| High risk / پرخطر | چند نشانه مهم از الگوهای رایج بدافزار USB وجود دارد |
| Critical / بحرانی | برای یافته‌های بسیار پرخطر رزرو شده است |

---

## دکمه‌ها و اکشن‌ها

### Scan USB / بررسی حافظه USB

فلش انتخاب‌شده را به‌صورت Recursive بررسی می‌کند. هنگام Scan می‌توانید عملیات را Cancel کنید. Cancel کردن هیچ فایلی را تغییر نمی‌دهد.

### Refresh

فهرست حافظه‌های USB متصل را دوباره از ویندوز می‌خواند.

### Restore visibility / نمایش مجدد

فقط برای فایل‌ها و پوشه‌های کاربری مخفی‌شده نمایش داده می‌شود و Attributeهای Hidden/System را حذف می‌کند.

NoHidden عمداً این دکمه را برای فایل‌های اجرایی، Script، Shortcut و `autorun.inf` نشان نمی‌دهد.

### Restore all hidden items / نمایش همه موارد مخفی

همه موارد قابل‌بازیابی تشخیص‌داده‌شده را Visible می‌کند. فایل‌های مشکوک در این عملیات وارد نمی‌شوند.

### Defender scan / بررسی با Defender

فایل انتخاب‌شده را برای Custom Scan به Microsoft Defender می‌دهد.

این اکشن با حالت **بدون Remediation خودکار** اجرا می‌شود؛ یعنی هدف NoHidden در این مرحله گرفتن نظر Defender است، نه حذف خودکار فایل.

بسته به تنظیمات ویندوز ممکن است UAC نمایش داده شود.

### VirusTotal

NoHidden فایل را Upload نمی‌کند.

ابتدا **SHA-256** فایل روی کامپیوتر شما محاسبه می‌شود و سپس صفحه گزارش همان Hash در VirusTotal باز می‌شود.

اگر VirusTotal قبلاً این Hash را ندیده باشد، ممکن است گزارشی برای آن وجود نداشته باشد.

### Neutralize / غیرفعال‌سازی

فایل مشکوک را حذف نمی‌کند؛ فقط نام آن را به شکل زیر تغییر می‌دهد:

```text
suspicious.exe
→
suspicious.exe.nohidden-disabled
```

به این ترتیب فایل دیگر با دوبار کلیک به‌صورت معمول اجرا نمی‌شود و اصل داده همچنان باقی می‌ماند.

برای برگرداندن دستی فایل، اگر مطمئن شدید سالم است، می‌توانید پسوند `.nohidden-disabled` را حذف کنید.

### Protect Windows / ایمن‌سازی ویندوز

AutoRun و AutoPlay را در سطح سیستم غیرفعال می‌کند تا باز کردن فلش ناشناس ریسک کمتری داشته باشد.

این تنظیم نیاز به Administrator دارد. NoHidden ابتدا دلیل نیاز به دسترسی را توضیح می‌دهد و فقط پس از تأیید شما، با UAC دوباره اجرا می‌شود.

### آیکون اطلاعات آنتی‌ویروس

NoHidden اطلاعات محصول امنیتی ثبت‌شده در Windows Security Center را نمایش می‌دهد.

کد فنی وضعیت آنتی‌ویروس **امتیاز امنیتی یا درصد سلامت نیست** و صرفاً برای عیب‌یابی نمایش داده می‌شود.

---

## حریم خصوصی

NoHidden با رویکرد **Offline-first** طراحی شده است.

- اسکن USB به‌صورت محلی انجام می‌شود.
- NoHidden فایل‌های شما را خودکار Upload نمی‌کند.
- Defender Scan روی موتور امنیتی خود ویندوز انجام می‌شود.
- اکشن VirusTotal فقط SHA-256 را محلی محاسبه و صفحه گزارش Hash را باز می‌کند.
- هیچ Analytics یا Telemetry اختصاصی در NoHidden وجود ندارد.
- اکشن آنلاین بدون اقدام صریح کاربر اجرا نمی‌شود.

---

## محدودیت‌ها و مشکلات شناخته‌شده نسخه 3.0.0

- فایل‌های Release فعلاً **Code-signed نیستند**؛ ممکن است Windows SmartScreen برای فایل تازه‌منتشرشده هشدار نشان دهد. فایل را فقط از صفحه رسمی GitHub Release دریافت کنید و در صورت حساسیت SHA-256 را با فایل `SHA256SUMS.txt` مقایسه کنید.
- تشخیص‌های Heuristic حکم قطعی Malware نیستند؛ فایل‌های مشکوک را قبل از هر اقدام بررسی کنید.
- NoHidden عمداً فایل مشکوک را خودکار Delete نمی‌کند.
- Neutralize در نسخه 3.0.0 هنوز دکمه Undo داخلی ندارد؛ بازگرداندن آن با حذف دستی پسوند `.nohidden-disabled` انجام می‌شود.
- VirusTotal در این نسخه نتیجه را داخل خود برنامه نمایش نمی‌دهد؛ گزارش Hash در مرورگر باز می‌شود و فایل Upload نمی‌شود.
- Defender Scan فقط زمانی قابل استفاده است که Microsoft Defender CLI روی ویندوز در دسترس باشد.
- NoHidden فعلاً Integration مستقیم با موتور CLI آنتی‌ویروس‌های Third-party ندارد.
- بعضی USB SSDها یا Deviceهایی که ویندوز آن‌ها را `Fixed drive` گزارش می‌کند ممکن است در فهرست Removable Drive نمایش داده نشوند.
- نسخه ARM64 جداگانه فعلاً منتشر نمی‌شود.
- بعضی فایل‌های سالم نیز می‌توانند BAT/EXE/Script باشند؛ وجود پسوند به‌تنهایی دلیل بدافزار بودن نیست.
- اگر فلش در حین Scan جدا شود، Scan ناقص تلقی می‌شود و NoHidden آن را به‌اشتباه Clean اعلام نمی‌کند.

---

## زبان‌ها

رابط برنامه:

- فارسی 🇮🇷
- English 🇬🇧

زبان انتخاب‌شده در پروفایل کاربر ذخیره می‌شود و در اجرای بعدی حفظ خواهد شد.

---

## برای توسعه‌دهندگان

NoHidden 3 با این Stack ساخته شده است:

- .NET 10
- WPF
- C#
- CommunityToolkit.Mvvm
- `NoHidden.Core` برای Scanner و منطق مستقل از UI
- GitHub Actions برای Build/Test/Release

Build:

```powershell
dotnet restore
dotnet build -c Debug
```

Run:

```powershell
dotnet run --project .\NoHidden\NoHidden.csproj
```

تست Scanner با Fixture بی‌خطر روی فلش:

**[docs/TESTING.md](docs/TESTING.md)**

---

## نسخه قدیمی و تاریخچه پروژه

NoHidden سال‌ها قبل برای رفع مشکل مخفی‌شدن فایل‌ها در اثر بدافزارهای USB ساخته شد. نسخه 3 یک بازنویسی مدرن با .NET 10 و WPF است.

مطلب قدیمی پروژه در سایت Mehrdad32:

**[نرم افزار No Hidden برای رفع مخفی شدن بر اثر ویروس](https://mehrdad32.ir/669/no_hidden_software/)**

---

## لایسنس

این پروژه تحت **Apache License 2.0** منتشر شده است.

[مشاهده LICENSE](LICENSE.txt)

---

<p align="center">
  ساخته شده توسط <a href="https://github.com/Mehrdad32">Mehrdad32</a>
</p>

</div>
