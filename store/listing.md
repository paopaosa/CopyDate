# CopyDate 微软商店上架材料

提交前把 Partner Center 中的包标识（Identity Name / Publisher）同步到 `CopyDate.Package/Package.appxmanifest`。商店会在认证通过后重新签名，本地侧载才需要自己的测试证书。

## 基本信息

- 名称：CopyDate
- 中文简称：日期复制（托盘）
- 定价：免费，无内购
- 类别：效率（Productivity） / 工具
- 最低系统：Windows 10 版本 1809（10.0.17763.0）或更高，含 Windows 11
- 架构：x64、ARM64（self-contained，用户无需预装 .NET）
- 隐私政策：将 `store/privacy.html` 发布到可公网访问的 URL（例如 GitHub Pages），填入商店列表

## 商店描述（简体中文）

短描述（建议 100 字以内，可作副标题）：

从系统托盘一键复制今天的日期。左键复制默认格式，右键选择常见或自定义格式。免费、不联网、不采集数据。

完整描述（粘贴到 Partner Center「说明」）：

CopyDate 是一款常驻 Windows 系统托盘的小工具，用来快速把「今天的日期」复制到剪贴板。写文档、填表、命名文件时，不必再手动输入或切换窗口去看日历。

使用方式很简单：
• 左键单击托盘图标，立即复制你设定的默认日期格式（出厂为 yyyy-MM-dd）。
• 右键打开菜单，每一项都会显示此刻将要复制的文字（含时分秒时会按当前时间刷新），点一下即可复制。
• 复制成功后可选择是否弹出简短提示。

内置今天常用格式，例如：
• 2026-09-17
• 2026/09/17
• 2026年9月17日
• 20260917
• 2026-09-17 15:30 与带秒的日期时间
• 2026年9月17日 15:30:00

你还可以：
• 添加自己的 .NET 日期时间格式（保存前可预览）
• 选择界面语言：简体中文、繁体中文、English，或跟随系统
• 更换托盘日历图标样式：红字白底、浅黑字浅灰底、深蓝底白字
• 可选开机启动（默认关闭，由你在设置中打开）

隐私与收费：
CopyDate 完全免费，没有广告和内购。应用不访问网络、不采集个人信息。偏好设置仅保存在本机。

适用于 Windows 10（1809 及更高版本）和 Windows 11。启动后不会弹出主窗口，请到任务栏右下角的通知区域（托盘）查找当天日期图标。

## Store description (English)

Short description:

Copy today’s date from the system tray. Left-click for your default format; right-click to pick common or custom formats. Free, offline, and private.

Full description (paste into Partner Center Description):

CopyDate is a small Windows system-tray utility that copies today’s date to the clipboard. Use it when writing documents, filling forms, or naming files—no need to type the date by hand or switch away to a calendar.

How it works:
• Left-click the tray icon to copy your default date format (yyyy-MM-dd out of the box).
• Right-click to open a menu. Each item shows the exact text that will be copied (date-time items refresh to the current time when the menu opens). Click to copy.
• Optionally show a brief notification after a successful copy.

Built-in formats for today include:
• 2026-09-17
• 2026/09/17
• 2026年9月17日
• 20260917
• 2026-09-17 15:30 and variants with seconds
• 2026年9月17日 15:30:00

You can also:
• Add your own .NET date/time format strings, with a live preview
• Choose Simplified Chinese, Traditional Chinese, English, or follow Windows
• Switch tray calendar styles: red on white, soft black on light gray, or white on dark blue
• Optionally start CopyDate with Windows (off by default; you turn it on in Settings)

Privacy and price:
CopyDate is free, with no ads and no in-app purchases. It does not access the network or collect personal data. Preferences are stored only on this PC.

Requires Windows 10 (version 1809 or later) or Windows 11. The app does not show a main window at startup—look for today’s date in the notification area (system tray) at the lower-right of the taskbar.

## 搜索关键字

中文：日期, 复制日期, 托盘, 剪贴板, 时间戳, 今日日期, yyyy-MM-dd
English: date, clipboard, tray, timestamp, copy date, today, yyyy-MM-dd

## 截图清单（至少 1 张，建议 1920×1080 或 1366×768）

1. 桌面右下角托盘菜单展开，能看清各日期格式
2. 设置窗口：默认格式、自定义格式、开机启动
3. 记事本或 Word 中粘贴成功的日期（可选）

不要在启动时弹出主窗口。截图前用浅色桌面、放大托盘溢出区，保证图标可见。

## 年龄分级

问卷按“工具 / 效率、无用户生成内容、无位置、无购物、无暴力”填写，预期为全年龄适宜（Everyone）。

## 功能声明 / 受限能力

- `runFullTrust`：经典 Win32 托盘应用所需，在 Partner Center 申报为受限能力并说明“系统托盘 NotifyIcon 与剪贴板”
- `windows.startupTask`：默认关闭，仅当用户在应用设置中打开

## 认证检查

提交前安装并运行 Windows App Certification Kit（WACK）。常见注意：
- 不要默认管理员权限
- 不要启动即显示空白主窗体
- 退出后托盘图标消失
- 包内含 x64 与 ARM64

## 本地发布命令

日常调试（未打包）：

```
dotnet run --project CopyDate/CopyDate.csproj
```

不要用 `dotnet build CopyDate.slnx` 打包装工程：`.wapproj` 依赖 Visual Studio 的 DesktopBridge 目标，请用 Visual Studio 或 Developer Command Prompt 里的 `MSBuild.exe`。

self-contained 发布（供打包工程引用）：

```
dotnet publish CopyDate/CopyDate.csproj -c Release -p:PublishProfile=Properties/PublishProfiles/win-x64.pubxml
dotnet publish CopyDate/CopyDate.csproj -c Release -p:PublishProfile=Properties/PublishProfiles/win-arm64.pubxml
```

MSIX：用 Visual Studio 打开 `CopyDate.slnx`，将 `CopyDate.Package` 设为启动项目。命令行示例（x64 侧载包）：

```
msbuild CopyDate.Package/CopyDate.Package.wapproj /restore /p:Configuration=Release /p:Platform=x64 /p:AppxBundle=Never
```

.NET SDK 会把 WinForms 输出放到包内 `CopyDate\` 子目录，因此清单里启动任务的可执行文件是 `CopyDate\CopyDate.exe`。首次提交前在 Partner Center 预留名称，并把清单中的 Publisher 换成账户对应的 CN=…。商店提交由商店重新签名；本地侧载需用测试证书签名。
