namespace Bucket.Updater.Models
{
    public class UpdateInfo
    {
        public string Version { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public bool IsPrerelease { get; set; }
        public List<UpdateAsset> Assets { get; set; } = new();
        public string DownloadUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public UpdateChannel Channel { get; set; }
        public SystemArchitecture Architecture { get; set; }
    }
}