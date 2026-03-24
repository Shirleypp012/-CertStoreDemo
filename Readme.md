这是一个可以直接运行的数字证书程序exe
克隆该项目后打开文件夹可以先把pfx文件删除
（因为这是我运行exe之后的产物懒得删了）
<img width="455" height="267" alt="图片" src="https://github.com/user-attachments/assets/e2ff6b1e-f936-4ccf-84b5-c4a146a32dd0" />


CertStoreDemo (C# 数字证书与 PFX 操作演示)

本项目是一个基于 C# (.NET) 的演示程序，旨在展示如何在 .NET 环境中加载、读取和操作 X.509 数字证书。

核心功能包括从 .pfx (Personal Information Exchange) 文件中加载证书，并演示如何访问证书存储区 (Certificate Store)。

🚀 功能特点

PFX 加载：演示如何读取 .pfx / .p12 格式的证书文件。
密码保护：展示如何处理受密码保护的私钥证书。
证书信息提取：读取并显示证书的详细信息（如主题名称、颁发者、有效期、指纹等）。
存储区交互：(可选) 演示如何将证书安装到 Windows 本地证书存储区或从中读取。
原生实现：使用 System.Security.Cryptography.X509Certificates 命名空间，无需第三方库。

️ 环境要求

.NET SDK (6.0, 7.0, 8.0 或更高)
Windows 操作系统 (因为涉及 Windows 证书存储区操作，虽然读取 PFX 文件在跨平台也可行，但存储区操作主要面向 Windows)

📦 快速开始

克隆项目

bash
git clone 
cd CertStoreDemo

运行项目

方式 A：使用 .NET CLI (推荐)
bash
dotnet run

方式 B：直接运行 EXE
双击 CertStoreDemo.exe。注意：如果电脑未安装 .NET Runtime，可能会报错。

关于证书文件 (CursorCertDemo.pfx)

项目中包含一个示例证书文件 CursorCertDemo.pfx。
用途：用于演示程序加载本地证书文件。
安全性：⚠️ 请勿在生产环境中使用此测试证书。它仅用于开发演示，可能没有设置强密码或已过期。
密码：如果程序运行时提示输入密码，请检查 Program.cs 中的硬编码密码，或在控制台输入测试密码（通常是 123456 或空，具体视代码而定）。

## 📂 项目结构
```text
CertStoreDemo/
├── Program.cs              # 主程序
├── CertStoreDemo.exe       # 可执行文件
└── README.md               # 说明文档



🔍 核心代码逻辑简述

程序主要使用了 X509Certificate2 类：

csharp
// 加载 PFX 文件
string pfxPath = "CursorCertDemo.pfx";
string password = "your_password"; // 替换为实际密码

using var cert = new X509Certificate2(pfxPath, password);

// 输出证书信息
Console.WriteLine("主题: {cert.Subject}");
Console.WriteLine("颁发者: {cert.Issuer}");
Console.WriteLine("有效期: {cert.NotBefore} - {cert.NotAfter}");
Console.WriteLine("指纹: {cert.Thumbprint}");

⚠️ 安全警告

私钥保护：.pfx 文件包含私钥，等同于你的数字身份身份证。永远不要将包含真实私钥的 .pfx 文件上传到公共 GitHub 仓库。
当前文件：本项目中的 CursorCertDemo.pfx 是仅供测试生成的虚拟证书，不包含任何真实敏感信息，可以安全提交。
生产环境：在实际项目中，请使用环境变量、Azure Key Vault 或 Windows 证书存储区来管理私钥，而不是直接放在代码目录中。

📤 发布独立版本

生成无需安装 .NET 环境的独立 exe：

bash
dotnet publish -c Release -r win-x64 --self-contained true

📝 许可证

MIT License
Created with ❤️ using C# & .NET
