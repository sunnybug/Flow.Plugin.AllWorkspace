using System;
using System.Collections.Generic;
using Flow.Launcher.Plugin;

namespace Flow.Launcher.Plugin.AllWorkspace
{
    /// <summary>Flow Launcher 插件：支持 Cursor/VSCode/Trae 等 IDE，快速打开 SSH/最近打开目录。</summary>
    public class AllWorkspace : IPlugin
    {
        private PluginInitContext _context;

        /// <inheritdoc />
        public void Init(PluginInitContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public List<Result> Query(Query query)
        {
            return new List<Result>();
        }
    }
}
