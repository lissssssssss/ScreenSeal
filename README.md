# 屏谧 (ScreenSeal)

Windows 全局隐私遮挡锁定辅助工具。一键遮挡 IM 聊天窗口、自定义区域或全屏模糊，支持窗口锁定与全局热键。

## 功能

- **精准遮挡**：自动识别微信、QQ、TIM、钉钉、企业微信、飞书等 IM 窗口并覆盖遮罩
- **自定义区域**：鼠标拖拽选取屏幕矩形区域遮挡
- **全屏模糊**：虚拟多屏范围内的半透明模糊遮罩
- **窗口锁定**：遮挡期间禁用目标窗口交互（`EnableWindow`）
- **全局热键**（可在设置中修改）：
  - `Ctrl+Shift+Q`：开启/关闭 IM 精准遮挡
  - `Ctrl+Shift+W`：自定义区域遮挡
  - `Ctrl+Shift+E`：解锁全部
- **托盘常驻**：无任务栏主窗口，右键菜单操作
- **配置持久化**：`%AppData%\ScreenSeal\config.json`

## 环境要求

- Windows 10 1903+ / Windows 11
- 构建需 [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## 构建与运行

```bash
cd ScreenSeal
dotnet restore
dotnet build
dotnet run
```

## 发布单文件 exe

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

输出：`ScreenSeal/bin/Release/net8.0-windows/win-x64/publish/ScreenSeal.exe`

## 项目结构

```
ScreenSeal/
├── Models/           # 配置与窗口信息模型
├── Services/         # 热键、枚举、遮挡、锁定、配置
│   └── Native/       # User32 P/Invoke
├── Views/            # 遮罩层、区域选择、设置窗口
├── MainWindow.xaml   # 托盘入口（隐藏主窗口）
└── App.xaml
```

## 说明

本仓库在 macOS/Linux 上仅可编辑代码；**编译与运行需在 Windows 上执行**。设计文档见 [ScreenSeal.md](./ScreenSeal.md)。
