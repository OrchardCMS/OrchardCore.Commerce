namespace System.Globalization;

public static class CultureInfoExtensions
{
    public static RegionInfo TryGetRegionInfo(this CultureInfo cultureInfo)
    {
        try
        {
            return new RegionInfo(cultureInfo.Name);
        }
        catch
        {
            return null;
        }
    }
}
