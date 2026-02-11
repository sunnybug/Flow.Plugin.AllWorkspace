using System.Collections.ObjectModel;

namespace Flow.Launcher.Plugin.AllWorkspace
{
    /// <summary>
    /// 插件设置
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// 是否区分 IDE：为 true 时仅加载当前触发关键字对应 IDE 的最近打开目录；为 false 时从所有 IDE 聚合。
        /// </summary>
        public bool DistinguishByIde { get; set; }

        /// <summary>
        /// 是否发现最近打开的工作区
        /// </summary>
        public bool DiscoverWorkspaces { get; set; } = true;

        /// <summary>
        /// 是否发现 SSH 远程机器
        /// </summary>
        public bool DiscoverMachines { get; set; } = true;

        /// <summary>
        /// 用户自定义工作区 URI 列表
        /// </summary>
        public ObservableCollection<string> CustomWorkspaces { get; set; } = new();
    }
}
