# CopyDate 微软商店提交说明

本文记录打包、手动提交，以及本仓库现有的「半自动」交包方式。  
**打包始终使用 `CopyDate.Package`，与是否使用 `msstore` 无关。**

---

## 1. 两种提交方式对照

| 步骤 | 手动（网页） | 半自动（本仓库脚本） |
|------|--------------|----------------------|
| 改版本号 | 自己改 | 自己改 |
| 打 `.msixupload` | Visual Studio / MSBuild + **CopyDate.Package** | 同左 |
| 上传包并提交认证 | Partner Center 网页 | `tools/Submit-StorePackage.ps1` |
| 看认证进度 | Partner Center | 同左 |

效果等价：都是把商店上传包交到 Partner Center 并进入认证流程。

---

## 2. 关于 `msstore`

### 用到了什么

- `msstore reconfigure`：配置 Seller ID、Tenant ID、Client ID、Client Secret（Secret 存在本机凭据管理器）
- `msstore info` / `msstore apps list`：验证凭据、列出应用

### 为什么 `msstore publish` 不行

CLI 报错类似：

```text
We could not find a project publisher for the project at '...\CopyDate'.
```

`msstore publish` / `init` 主要支持 WinUI、MAUI、Flutter、Electron、PWA 等内置类型。  
CopyDate 是 **WinForms + WAP（`CopyDate.Package.wapproj`）**，CLI 找不到对应的 project publisher，因此：

- 不会调用 `CopyDate.Package` 打包  
- 不能一键 `msstore publish`  

凭据可以配通（`apps list` 正常），卡点在**项目类型适配**，不是账号本身。

### 半自动交包实际怎么走

1. 仍用 **MSBuild + CopyDate.Package** 打出 `.msixupload`  
2. 用 **Partner Center 提交 API**（脚本读取 `msstore` 存好的 Secret）上传并 commit  

**没有**使用 `msstore publish` 命令。

---

## 3. 前置条件（半自动，只需配置一次）

1. Partner Center 开发者账号；应用已存在（本应用 Product ID：`9NMF25PCV1PF`）
2. Microsoft Entra 应用已关联到 Partner Center，角色选 **Manager（Windows）**  
   - CopyDate 是 MSIX 上架，一般**不需要** Manager（Desktop）
3. 本机已执行过类似：

```text
msstore reconfigure --tenantId <TenantID> --sellerId <SellerID> --clientId <ClientID> --clientSecret <Secret>
```

成功时会提示 Configuration saved / It seems to be working。

Seller ID 位置：Partner Center → 齿轮 → Account settings → Developer / Identifiers → **Seller ID**。

---

## 4. 发版前改版本

每次提交的包身份版本必须**高于**商店里已有版本（四段，如 `1.2.2.0`）。  
同版本号、不同内容会被商店拒绝。

需要改动的位置：

- [`CopyDate/CopyDate.csproj`](../CopyDate/CopyDate.csproj)：`Version` / `InformationalVersion`
- [`CopyDate.Package/Package.appxmanifest`](../CopyDate.Package/Package.appxmanifest)：`Identity Version`
- [`CHANGELOG.md`](../CHANGELOG.md)
- 商店「此版本的新增功能」文案：[`tools/store-release-notes-1.2.2.json`](../tools/store-release-notes-1.2.2.json)（或按版本新建同结构 JSON），以及可选更新 [`store/listing.md`](listing.md)

打完包后建议抽查 MSIX 内清单，确认 `Identity Version` 与文件名一致（文件名对、包内仍是旧版本会导致提交失败）。

---

## 5. 打包（CopyDate.Package）

### Visual Studio（与以往习惯一致）

1. 打开 `CopyDate.slnx`（或 `.sln`）
2. 将 **CopyDate.Package** 设为启动项目（如需）
3. 平台选 **x64**
4. 右键 CopyDate.Package → **发布 → 创建应用程序包…**（Create App Packages）
5. 选择上传到 Microsoft Store，生成 `.msixupload`

日常调试托盘功能用 **CopyDate** 项目 F5 即可，不必每次打包。说明见仓库根目录 `如何调试如何部署.txt`。

### 命令行 MSBuild 示例

```powershell
& "${env:ProgramFiles}\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  "E:\source\CopyDate\CopyDate.Package\CopyDate.Package.wapproj" `
  /restore /t:Rebuild `
  /p:Configuration=Release `
  /p:Platform=x64 `
  /p:AppxBundle=Never `
  /p:UapAppxPackageBuildMode=StoreUpload `
  /p:AppxPackageDir="E:\source\CopyDate\CopyDate.Package\AppPackages\\" `
  /p:AppxAutoIncrementPackageRevision=False
```

产物示例：

```text
CopyDate.Package\AppPackages\CopyDate.Package_<版本>_x64.msixupload
```

不要用 `dotnet build CopyDate.slnx` 打包装工程：`.wapproj` 依赖 Visual Studio 的 DesktopBridge / MSBuild。

---

## 6. 提交到商店

### 方式 A：网页手动提交

1. 打开 [Partner Center](https://partner.microsoft.com/dashboard) → Copy Date  
2. 新建提交 → 上传 `.msixupload`  
3. 填写「此版本的新增功能」等 → 提交认证  

上架文案、隐私、能力说明等见 [`listing.md`](listing.md)。

### 方式 B：脚本半自动提交

需本机已配置 `msstore` 凭据，并用 **PowerShell 7（`pwsh`）** 运行：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass `
  -File "E:\source\CopyDate\tools\Submit-StorePackage.ps1" `
  -MsixUploadPath "E:\source\CopyDate\CopyDate.Package\AppPackages\CopyDate.Package_1.2.2.0_x64.msixupload"
```

脚本会：

1. 读取凭据管理器中的 Client Secret  
2. 创建提交（若有失败草稿会先删）  
3. 替换应用包，写入各语言 release notes（默认读 `tools/store-release-notes-1.2.2.json`）  
4. 将 `.msixupload` 打成 zip 上传  
5. commit，并轮询至 `PreProcessing` 等状态  

相关工具：

| 文件 | 作用 |
|------|------|
| `tools/Submit-StorePackage.ps1` | 上传并提交 |
| `tools/Delete-PendingSubmission.ps1` | 删除进行中的失败/草稿提交 |
| `tools/Commit-StoreSubmission.ps1` | 仅对已上传的 submission 执行 commit（排障用） |
| `tools/store-release-notes-*.json` | 商店更新说明文案 |

成功后到 Partner Center 查看认证与发布进度即可。

---

## 7. 常见问题

**同版本无法提交**  
报错含 `must be uniquely identified by their full names` / 两个包同为 `…_1.2.x.0_X64_`：说明该版本已在商店存在（或包内版本与文件名不一致）。抬高 `Package.appxmanifest` 版本后 **Clean + Rebuild**，并确认包内 `AppxManifest.xml` 的 Version。

**Manager（Windows） vs Manager（Desktop）**  
MSIX / Windows 应用商店提交用 **Manager（Windows）** 即可。Desktop 多对应 MSI/EXE 未打包通道。

**`msstore apps list` 曾显示无应用**  
先确认 Entra 应用已加入 Partner Center 且角色正确；凭据配好后应能列出 Copy Date（`9NMF25PCV1PF`）等。

**脚本要求 pwsh**  
Windows PowerShell 5 会把 JSON 属性名改成 PascalCase，导致 `fileStatus` 等字段提交无效。请用 PowerShell 7。

---

## 8. 简要结论

- 打包：永远用 **CopyDate.Package**。  
- 交包：网页手动，或 `Submit-StorePackage.ps1`（Partner Center API）。  
- `msstore`：只负责凭据配置与连通性检查，**不能**对本仓库执行 `publish`。  
