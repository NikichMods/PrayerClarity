using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx.Logging;

namespace PrayerClarity
{
    internal static class Localization
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Warned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static Assembly _assembly;
        private static ManualLogSource _log;
        private static string _activeCode;
        private static Dictionary<string, string> _active;
        private static Dictionary<string, string> _english;
        private static CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");

        internal static void Initialize(Assembly assembly, ManualLogSource log)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _log = log;
            _english = Load("en");
            UseCurrentGameLanguage();
        }

        internal static void UseCurrentGameLanguage()
        {
            string code = Normalize(R.CurrentLanguage());
            if (string.Equals(code, _activeCode, StringComparison.OrdinalIgnoreCase) && _active != null) return;
            _activeCode = code;
            _active = Load(code);
            _culture = CultureFor(code);
        }

        internal static string F(string key, params object[] args)
        {
            // Two older presentation seams still ask for these legacy keys with a
            // duration parameter. Render them through the same accepted day-based
            // wording as the shared semantic model instead of exposing real-time
            // minutes again.
            if (string.Equals(key, "buff.skull", StringComparison.Ordinal) && args != null && args.Length >= 2)
            {
                float duration = Convert.ToSingle(args[1], CultureInfo.InvariantCulture);
                return F("active.skull", args[0]) + " · " +
                       F("active.timer_days", PrayerForecast.DurationParameterToGameDays(duration));
            }

            if (string.Equals(key, "buff.sin_shard", StringComparison.Ordinal) && args != null && args.Length >= 1)
            {
                float duration = Convert.ToSingle(args[0], CultureInfo.InvariantCulture);
                return F("active.sin_shard") + " · " +
                       F("active.timer_days", PrayerForecast.DurationParameterToGameDays(duration));
            }

            string template = T(key);
            try { return string.Format(_culture, template, args); }
            catch (FormatException ex)
            {
                WarnOnce("format:" + key, "Invalid localization format for '" + key + "': " + ex.Message);
                string fallback = Lookup(_english, key) ?? key;
                return string.Format(CultureInfo.GetCultureInfo("en-US"), fallback, args);
            }
        }

        private static string T(string key)
        {
            // Probe 0.1.9 verified that the game's native small_font already owns
            // universal inline quality symbols. Keep these as presentation glyphs,
            // not translated words, so Technology rows reuse the exact vanilla art
            // in every supported language without extra UI objects or custom assets.
            switch (key)
            {
                case "quality.bronze": return "(s1)";
                case "quality.silver": return "(s2)";
                case "quality.gold": return "(s3)";
            }

            string value = Lookup(_active, key) ?? Lookup(_english, key);
            if (value != null) return value;
            WarnOnce("missing:" + key, "Missing localization key '" + key + "'.");
            return key;
        }

        private static string Lookup(Dictionary<string, string> dictionary, string key)
        {
            if (dictionary == null || key == null) return null;
            string value;
            return dictionary.TryGetValue(key, out value) ? value : null;
        }

        private static Dictionary<string, string> Load(string code)
        {
            Dictionary<string, string> cached;
            if (Cache.TryGetValue(code, out cached)) return cached;

            string suffix = ".lang." + code + ".json";
            string resource = _assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
            if (resource == null)
            {
                if (!string.Equals(code, "en", StringComparison.OrdinalIgnoreCase))
                {
                    WarnOnce("locale:" + code, "No embedded locale '" + code + "'; using English.");
                    return _english ?? Load("en");
                }
                throw new InvalidOperationException("Embedded English localization resource is missing.");
            }

            Dictionary<string, string> parsed = ReadResource(resource);

            // Sibling editions share the base Clarity dictionary. Rebalanced may embed
            // an optional locale overlay with mechanics-specific strings; Vanilla does
            // not include these resources and therefore keeps the accepted 1.0.20 text
            // byte-for-byte at the source level.
            string overlaySuffix = ".lang_rebalanced." + code + ".json";
            string overlayResource = _assembly.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith(overlaySuffix, StringComparison.OrdinalIgnoreCase));
            if (overlayResource != null)
            {
                Dictionary<string, string> overlay = ReadResource(overlayResource);
                foreach (KeyValuePair<string, string> pair in overlay)
                    parsed[pair.Key] = pair.Value;
            }

            Cache[code] = parsed;
            return parsed;
        }

        private static Dictionary<string, string> ReadResource(string resource)
        {
            using (Stream stream = _assembly.GetManifestResourceStream(resource))
            using (StreamReader reader = new StreamReader(stream ?? throw new InvalidOperationException("Cannot open localization resource " + resource), Encoding.UTF8, true))
                return FlatJson.Parse(reader.ReadToEnd());
        }

        private static string Normalize(string raw)
        {
            string code = string.IsNullOrWhiteSpace(raw) ? "en" : raw.Trim().ToLowerInvariant().Replace('-', '_');
            if (code == "zh" || code.StartsWith("zh_", StringComparison.Ordinal)) return "zh_cn";
            if (code == "pt" || code.StartsWith("pt_", StringComparison.Ordinal)) return "pt_br";
            switch (code)
            {
                case "en": case "fr": case "de": case "es": case "ko": case "ja": case "ru": case "it": case "pl":
                    return code;
                default:
                    return "en";
            }
        }

        private static CultureInfo CultureFor(string code)
        {
            try
            {
                switch (code)
                {
                    case "fr": return CultureInfo.GetCultureInfo("fr-FR");
                    case "de": return CultureInfo.GetCultureInfo("de-DE");
                    case "zh_cn": return CultureInfo.GetCultureInfo("zh-CN");
                    case "es": return CultureInfo.GetCultureInfo("es-ES");
                    case "pt_br": return CultureInfo.GetCultureInfo("pt-BR");
                    case "ko": return CultureInfo.GetCultureInfo("ko-KR");
                    case "ja": return CultureInfo.GetCultureInfo("ja-JP");
                    case "ru": return CultureInfo.GetCultureInfo("ru-RU");
                    case "it": return CultureInfo.GetCultureInfo("it-IT");
                    case "pl": return CultureInfo.GetCultureInfo("pl-PL");
                    default: return CultureInfo.GetCultureInfo("en-US");
                }
            }
            catch { return CultureInfo.InvariantCulture; }
        }

        private static void WarnOnce(string id, string message)
        {
            if (!Warned.Add(id)) return;
            _log?.LogWarning("PC_LOCALIZATION_WARNING " + message);
        }

        private static class FlatJson
        {
            internal static Dictionary<string, string> Parse(string json)
            {
                Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
                int index = 0;
                SkipWhite(json, ref index);
                Expect(json, ref index, '{');
                SkipWhite(json, ref index);

                while (index < json.Length && json[index] != '}')
                {
                    string key = ReadString(json, ref index);
                    SkipWhite(json, ref index);
                    Expect(json, ref index, ':');
                    SkipWhite(json, ref index);
                    string value = ReadString(json, ref index);
                    result[key] = value;
                    SkipWhite(json, ref index);
                    if (index < json.Length && json[index] == ',')
                    {
                        index++;
                        SkipWhite(json, ref index);
                    }
                    else break;
                }

                Expect(json, ref index, '}');
                SkipWhite(json, ref index);
                if (index != json.Length) throw new FormatException("Unexpected data after JSON object.");
                return result;
            }

            private static string ReadString(string text, ref int index)
            {
                Expect(text, ref index, '"');
                StringBuilder builder = new StringBuilder();
                while (index < text.Length)
                {
                    char c = text[index++];
                    if (c == '"') return builder.ToString();
                    if (c != '\\') { builder.Append(c); continue; }
                    if (index >= text.Length) throw new FormatException("Invalid JSON escape.");
                    char escaped = text[index++];
                    switch (escaped)
                    {
                        case '"': builder.Append('"'); break;
                        case '\\': builder.Append('\\'); break;
                        case '/': builder.Append('/'); break;
                        case 'b': builder.Append('\b'); break;
                        case 'f': builder.Append('\f'); break;
                        case 'n': builder.Append('\n'); break;
                        case 'r': builder.Append('\r'); break;
                        case 't': builder.Append('\t'); break;
                        case 'u':
                            if (index + 4 > text.Length) throw new FormatException("Invalid JSON unicode escape.");
                            builder.Append((char)int.Parse(text.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            index += 4;
                            break;
                        default: throw new FormatException("Unsupported JSON escape: \\" + escaped);
                    }
                }
                throw new FormatException("Unterminated JSON string.");
            }

            private static void SkipWhite(string text, ref int index)
            {
                while (index < text.Length && char.IsWhiteSpace(text[index])) index++;
            }

            private static void Expect(string text, ref int index, char expected)
            {
                if (index >= text.Length || text[index] != expected) throw new FormatException("Expected '" + expected + "' at offset " + index + ".");
                index++;
            }
        }
    }
}
