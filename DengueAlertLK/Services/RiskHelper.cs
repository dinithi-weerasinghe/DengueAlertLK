namespace DengueAlertLK.Services;

public static class RiskHelper
{
    public const int HighFrom = 100;     // >= 100  -> High
    public const int MediumFrom = 50;    // 50-99   -> Medium, below -> Low

    public static string Level(int cases) =>
        cases >= HighFrom ? "High" : cases >= MediumFrom ? "Medium" : "Low";
}