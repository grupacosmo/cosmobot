using UnityEngine;

namespace Cosmobot.Utils
{
    public static class GameInfo
    {
        public static string Name => Application.productName;
        public static string CompanyName => Application.companyName;
        public static string Version => Application.version;
        public const string BugReportUrl = "https://github.com/grupacosmo/cosmobot/issues";
        public const string WebsiteUrl = "https://https://cosmopk.pl/";
    }
}
