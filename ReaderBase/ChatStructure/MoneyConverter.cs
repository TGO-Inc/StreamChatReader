using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace StreamingServices.Chat.Money
{
    internal enum CountryCode
    {
        AFN,
        ALL,
        ANG,
        ARS,
        AUD,
        AWG,
        AZN,
        BAM,
        BBD,
        BGN,
        BMD,
        BND,
        BOB,
        BRL,
        BSD,
        BWP,
        BYN,
        BZD,
        CAD,
        CHF,
        CLP,
        CNY,
        COP,
        CRC,
        CUP,
        CZK,
        DKK,
        DOP,
        EGP,
        EUR,
        FJD,
        FKP,
        GBP,
        GGP,
        GHS,
        GIP,
        GTQ,
        GYD,
        HKD,
        HNL,
        HRK,
        HUF,
        IDR,
        ILS,
        IMP,
        INR,
        IRR,
        ISK,
        JEP,
        JMD,
        JPY,
        KGS,
        KHR,
        KPW,
        KRW,
        KYD,
        KZT,
        LAK,
        LBP,
        LKR,
        LRD,
        MNT,
        MUR,
        MXN,
        MYR,
        NAD,
        NGN,
        NIO,
        NOK,
        NPR,
        NZD,
        OMR,
        PAB,
        PEN,
        PHP,
        PKR,
        PLN,
        PYG,
        QAR,
        RON,
        RSD,
        RUB,
        SAR,
        SBD,
        SCR,
        SEK,
        SGD,
        SHP,
        SOS,
        SRD,
        SVC,
        SYP,
        THB,
        TRY,
        TTD,
        TVD,
        TWD,
        UAH,
        USD,
        UYU,
        UZS,
        VEF,
        VND,
        XCD,
        YER,
        ZAR,
        ZWD,
        ден
    }
    internal class MoneyConverter
    {
        private static readonly Dictionary<char, CountryCode> CurrencyMapChar = new()
        {
            { '؋', CountryCode.AFN },
            { 'ƒ', CountryCode.ANG },
            { '₼', CountryCode.AZN },
            { '₡', CountryCode.CRC },
            { '€', CountryCode.EUR },
            { '£', CountryCode.GBP },
            { '¢', CountryCode.GHS },
            { '₪', CountryCode.ILS },
            { '₹', CountryCode.INR },
            { '₨', CountryCode.NPR },
            { '¥', CountryCode.JPY },
            { '៛', CountryCode.KHR },
            { '₩', CountryCode.KRW },
            { '₭', CountryCode.LAK },
            { '₮', CountryCode.MNT },
            { '₦', CountryCode.NGN },
            { '₱', CountryCode.PHP },
            { '₽', CountryCode.RUB },
            { '﷼', CountryCode.SAR },
            { '฿', CountryCode.THB },
            { '₴', CountryCode.UAH },
            { '$', CountryCode.USD },
            { '₫', CountryCode.VND }
        };
        private static readonly Dictionary<CountryCode, string> CurrencyMapString = new()
        {
            { CountryCode.AFN, "AF" },
            { CountryCode.ALL, "LEK" },
            { CountryCode.ANG, "AN" },
            { CountryCode.ARS, "AR" },
            { CountryCode.AUD, "A"},
            { CountryCode.AWG, "AW" },
            { CountryCode.AZN, "AZ" },
            { CountryCode.BAM, "KM" },
            { CountryCode.BBD, "BB" },
            { CountryCode.BGN, "лв" },
            { CountryCode.BMD, "BM" },
            { CountryCode.BND, "BN" },
            { CountryCode.BOB, "B" },
            { CountryCode.BRL, "R" },
            { CountryCode.BSD, "" },
            { CountryCode.BWP, "BW" },
            { CountryCode.BYN, "BR" },
            { CountryCode.BZD, "BZ" },
            { CountryCode.CAD, "CA" },
            { CountryCode.CHF, "CH" },
            { CountryCode.CLP, "CL" },
            { CountryCode.CNY, "CN" },
            { CountryCode.COP, "CO" },
            { CountryCode.CRC, "CR" },
            { CountryCode.CUP, "CU" },
            { CountryCode.CZK, "Kč" },
            { CountryCode.DKK, "DK" },
            { CountryCode.DOP, "RD" },
            { CountryCode.EGP, "EG" },
            { CountryCode.EUR, "EU" },
            { CountryCode.FJD, "FJ" },
            { CountryCode.FKP, "FK" },
            { CountryCode.GBP, "GB" },
            { CountryCode.GGP, "GG" },
            { CountryCode.GHS, "GH" },
            { CountryCode.GIP, "GI" },
            { CountryCode.GTQ, "GT" },
            { CountryCode.GYD, "GY" },
            { CountryCode.HKD, "HK" },
            { CountryCode.HNL, "HN" },
            { CountryCode.HRK, "KN" },
            { CountryCode.HUF, "FT" },
            { CountryCode.IDR, "RP" },
            { CountryCode.ILS, "IL" },
            { CountryCode.IMP, "IM" },
            { CountryCode.INR, "IN" },
            { CountryCode.IRR, "IR" },
            { CountryCode.ISK, "IS" },
            { CountryCode.JEP, "JE" },
            { CountryCode.JMD, "J" },
            { CountryCode.JPY, "JP" },
            { CountryCode.KGS, "KG" },
            { CountryCode.KHR, "KH" },
            { CountryCode.KPW, "KP" },
            { CountryCode.KRW, "K" },
            { CountryCode.KYD, "KY" },
            { CountryCode.KZT, "KZ" },
            { CountryCode.LAK, "LA" },
            { CountryCode.LBP, "LB" },
            { CountryCode.LKR, "LK" },
            { CountryCode.LRD, "LR" },
            { CountryCode.MNT, "MN" },
            { CountryCode.MUR, "MU" },
            { CountryCode.MXN, "MX" },
            { CountryCode.MYR, "RM" },
            { CountryCode.NAD, "NA" },
            { CountryCode.NGN, "NG" },
            { CountryCode.NIO, "C" },
            { CountryCode.NOK, "NO" },
            { CountryCode.NPR, "NP" },
            { CountryCode.NZD, "NZ" },
            { CountryCode.OMR, "OM" },
            { CountryCode.PAB, "B/." },
            { CountryCode.PEN, "S/." },
            { CountryCode.PHP, "PH" },
            { CountryCode.PKR, "PK" },
            { CountryCode.PLN, "zł" },
            { CountryCode.PYG, "Gs" },
            { CountryCode.QAR, "QA" },
            { CountryCode.RON, "lei" },
            { CountryCode.RSD, "Дин." },
            { CountryCode.RUB, "RU" },
            { CountryCode.SAR, "SA" },
            { CountryCode.SBD, "SB" },
            { CountryCode.SCR, "SC" },
            { CountryCode.SEK, "KR" },
            { CountryCode.SGD, "SG" },
            { CountryCode.SHP, "SH" },
            { CountryCode.SOS, "SO" },
            { CountryCode.SRD, "SR" },
            { CountryCode.SVC, "SV" },
            { CountryCode.SYP, "SY" },
            { CountryCode.THB, "TH" },
            { CountryCode.TRY, "TR" },
            { CountryCode.TTD, "TT" },
            { CountryCode.TVD, "TV" },
            { CountryCode.TWD, "NT" },
            { CountryCode.UAH, "UA" },
            { CountryCode.USD, "US" },
            { CountryCode.UYU, "U" },
            { CountryCode.UZS, "UZ" },
            { CountryCode.VEF, "BS" },
            { CountryCode.VND, "VND" },
            { CountryCode.XCD, "XCD" },
            { CountryCode.YER, "YE" },
            { CountryCode.ZAR, "ZA" },
            { CountryCode.ZWD, "Z" },
            { CountryCode.ден, "MKD" }
        };
        private static readonly HttpClient http = new();
        public static void Convert(ref double amt, string from)
        {
            JObject List = JObject.Parse(
                    http.GetStringAsync($"https://api.exchangerate.host/latest?base=USD")
                .GetAwaiter()
                .GetResult())["rates"] as JObject;

            foreach (var item in List)
            {
                if (item.Key.ToString().Contains(from))
                {
                    amt /= double.Parse(item.Value.ToString());
                    return;
                }
            }
        }
        public static double Convert(string from)
        {
            JObject List = JObject.Parse(
                    http.GetStringAsync($"https://api.exchangerate.host/latest?base=USD")
                .GetAwaiter()
                .GetResult())["rates"] as JObject;

            string currency = string.Empty;
            string value = Regex.Replace(from, @"\s", "").ToUpper();

            foreach(char c in CurrencyMapChar.Keys)
            {
                if (value.Contains(c))
                {
                    int ind = value.IndexOf(c);
                    string splt = value[..ind];
                    value = value[(ind + 1)..];
                    if (splt.Length > 0) 
                        foreach(var item in CurrencyMapString)
                            if (splt.Equals(item.Value))
                            {
                                currency = item.Key.ToString();
                                goto pass;
                            }
                    currency = CurrencyMapChar[c].ToString();
                    goto pass;
                }
            }

            foreach(var c in CurrencyMapString)
            {
                Match m = Regex.Match(value, $"(({c.Key})|({c.Value}))+(?<=.)");
                if (m.Success && m.ValueSpan.Length > 0)
                {
                    Match v = Regex.Match(value, "[\\d.,+-]+");
                    value = v.Value;
                    if (CurrencyMapChar.ContainsKey(value[0])) value = value[1..];
                    currency = c.Key.ToString();
                }
            }
        pass:
            Debug.WriteLine($"{DateTime.UtcNow.ToLocalTime()}: {value} Dontated In {currency}");
            foreach (var item in List)
                if (item.Key.ToString().Contains(currency))
                    return double.Parse(value) / double.Parse(item.Value.ToString());
            return 0;
        }
    }
}




