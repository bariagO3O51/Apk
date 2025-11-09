namespace DevBoxAI.Core.Models;

public class PreviewState
{
    public DeviceFrame SelectedDevice { get; set; } = DeviceFrame.Pixel7;
    public bool IsDarkMode { get; set; } = false;
    public string CurrentScreen { get; set; } = string.Empty;
    public double ZoomLevel { get; set; } = 1.0;
    public bool ShowLayoutBounds { get; set; } = false;
    public bool ShowPerformanceOverlay { get; set; } = false;
}

public enum DeviceFrame
{
    Pixel7,
    Pixel7Pro,
    SamsungS23,
    Tablet10Inch,
    Custom
}

public class DeviceSpec
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public double DensityDpi { get; set; }
}
