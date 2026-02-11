using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.AllWorkspace.Shared
{
    public class VSCodeStorageFile
    {
        [JsonPropertyName("openedPathsList")]
        public OpenedPathsList OpenedPathsList { get; set; }
    }

    public class OpenedPathsList
    {
        [JsonPropertyName("workspaces3")]
        public List<dynamic> Workspaces3 { get; set; }

        [JsonPropertyName("entries")]
        public List<FolderUriEntry> Entries { get; set; }
    }

    public class FolderUriEntry
    {
        [JsonPropertyName("folderUri")]
        public string FolderUri { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }
    }
}
