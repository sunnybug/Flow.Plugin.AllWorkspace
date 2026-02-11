# AllWorkspace

Flow Launcher 插件：整合 **Cursor**、**VSCode**、**Trae** 的最近打开目录与 SSH 连接，一个入口快速打开。

## 功能

- 从各 IDE 的 `storage.json` / `state.vscdb` 读取**最近打开的工作区**
- 从 SSH config 读取**远程主机**，一键用对应 IDE 打开 SSH 连接
- 支持**自定义工作区 URI**（在设置中添加）

## 触发关键字

- **cu**：默认关键字，可只显示 Cursor 或全部（见下方「区分 IDE」）
- **tr**：需在 Flow Launcher 中为该插件再添加一条，关键字设为 `tr`
- **vsc**：同上，再添加一条，关键字设为 `vsc`

同一插件可添加多次，分别设置不同 Action Keyword（cu / tr / vsc），即可按关键字触发。

## 配置（设置面板）

- **发现最近打开的工作区**：是否从各 IDE 读取最近打开列表（默认开）
- **发现 SSH 远程机器**：是否从 SSH config 读取主机（默认开）
- **区分 IDE**：
  - **不勾选（默认）**：任意关键字都会显示**所有** IDE 的最近打开 + SSH
  - **勾选**：仅显示当前关键字对应的 IDE（cu → 仅 Cursor，tr → 仅 Trae，vsc → 仅 VS Code）
- **自定义工作区 URI**：可添加额外 folder/file URI，与上述列表一起显示并打开

## 使用

- 输入关键字（如 `cu`）后输入文字可模糊搜索标题
- 选中一项回车：用对应 IDE 打开该工作区或 SSH
- Ctrl + 回车：若为本地目录，在文件资源管理器中打开
- 右键本地工作区：可选「在文件资源管理器中打开」

## 版本

1.0.0
