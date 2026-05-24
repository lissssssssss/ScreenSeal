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
- **应用选择**：启动时显示主界面，勾选需要遮挡隐私信息的 IM 应用（微信、QQ、钉钉等）
- **托盘常驻**：主窗口隐藏于托盘，关闭选择界面后仍在后台运行
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
├── MainWindow.xaml   # 主界面（应用选择）+ 托盘
└── App.xaml
```

## 在 macOS 上获取 Windows 可执行文件（GitHub Actions）

无需本地 Windows 环境，推送代码后由 CI 自动打包：

1. 打开仓库 [Actions](https://github.com/lissssssssss/ScreenSeal/actions) 页
2. 选择 **Build Windows Release** 工作流，进入最近一次成功的运行
3. 在 **Artifacts** 区域下载 `ScreenSeal-win-x64`，解压得到 `ScreenSeal.exe`
4. 将 exe 拷贝到 Windows 10/11 机器上双击运行

也可在 Actions 页点击 **Run workflow** 手动触发构建。

打 tag 发布 Release（例如 `v1.0.0`）时，会自动创建 GitHub Release 并附上 exe：

```bash
git tag v1.0.0
git push origin v1.0.0
```

## 说明

本仓库在 macOS/Linux 上仅可编辑代码；**运行需在 Windows 上执行**。设计文档见 [ScreenSeal.md](./ScreenSeal.md)。
