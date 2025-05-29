namespace Vintellitour_Framework.ViewModels
{
    public class MapViewModel
    {
        public string SearchQuery { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
        public string? ClickedLayer { get; set; }
        public int? ClickedLayerGid { get; set; }
        public bool IsMounted { get; set; }
    }
}
