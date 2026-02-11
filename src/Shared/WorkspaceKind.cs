namespace Flow.Launcher.Plugin.AllWorkspace.Shared
{
    /// <summary>
    /// 工作区类型（与 VSCode/Cursor URI 解析一致）
    /// </summary>
    public enum WorkspaceKind
    {
        Local = 1,
        Codespaces = 2,
        RemoteWSL = 3,
        RemoteSSH = 4,
        RemoteContainers = 5,
        DevContainer = 6,
    }
}
