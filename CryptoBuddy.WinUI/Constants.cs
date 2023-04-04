using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBuddy
{
    public class Constants
    {
        public const string AppTitle = "CryptoBuddy";
        public const int TimerPeriodMillis = 5000;
        public static readonly Dictionary<string, string> CurrencyDictionary = new Dictionary<string, string>()
            {
                {"BTC-USD", "Bitcoin" },
                {"ETH-USD", "Etherium" },
                {"ADA-USD", "Cardano" },
                {"XRP-USD", "XRP" },
                {"DOGE-USD", "Dogecoin" },
                {"LTC-USD", "Litecoin" }
            };
    }
}
