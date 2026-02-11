using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Flow.Launcher.Plugin.AllWorkspace.Shared;

namespace Flow.Launcher.Plugin.AllWorkspace
{
    public partial class SettingsView : UserControl
    {
        private readonly PluginInitContext _context;
        private readonly Settings _settings;

        public SettingsView(PluginInitContext context, Settings settings)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            DataContext = _settings;
            InitializeComponent();
        }

        private void Save(object sender = null, RoutedEventArgs e = null)
            => _context.API.SaveSettingJsonStorage<Settings>();

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in listView.SelectedItems.Cast<string>().ToArray())
                _settings.CustomWorkspaces.Remove(item);
            Save();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            var uri = addUri.Text?.Trim();
            if (string.IsNullOrEmpty(uri)) return;
            try
            {
                var (kind, _, _) = ParseVSCodeUri.GetTypeWorkspace(uri);
                if (kind == WorkspaceKind.Local)
                    uri = new Uri(uri).AbsoluteUri;
                addUri.Clear();
                if (_settings.CustomWorkspaces.Contains(uri)) return;
                _settings.CustomWorkspaces.Add(uri);
                Save();
            }
            catch (Exception ex)
            {
                _context.API.ShowMsgError("添加失败", ex.Message);
            }
        }
    }
}
