// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
#if _NOT_USED_YET_
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.R.Host.Client.Encodings {
    public static class REncodingsMap {
        // https://msdn.microsoft.com/en-us/library/system.text.encoding.getencodings(v=vs.110).aspx
        // https://www.ibm.com/support/knowledgecenter/SSMKHH_9.0.0/com.ibm.etools.mft.doc/ac00408_.htm
        public static Encoding GetEncoding(string rEncodingName) {
            var encSet = _encodingsMap.Keys.FirstOrDefault(x => x.Contains(rEncodingName));
            if(encSet != null) {
                try {
                    return _encodingsMap[encSet].Invoke();
                } catch(ArgumentException) { }
            }
            return Encoding.Default;
        }

        private static Dictionary<HashSet<string>, Func<Encoding>> _encodingsMap = new Dictionary<HashSet<string>, Func<Encoding>>() {
        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-8", "ibm-1208", "ibm-1209", "ibm-5304", "ibm-5305", "ibm-13496", "ibm-13497", "ibm-17592", "ibm-17593",
            "windows-65001", "cp1208", "x-UTF_8J", "unicode-1-1-utf-8", "unicode-2-0-utf-8"},
            () => { return Encoding.UTF8; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-16", "ISO-10646-UCS-2", "ibm-1204", "ibm-1205", "unicode", "csUnicode", "ucs-2"},
            () => { return Encoding.Unicode; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-16BE", "x-utf-16be", "UnicodeBigUnmarked", "ibm-1200", "ibm-1201", "ibm-13488", "ibm-13489", "ibm-17584", "ibm-17585",
             "ibm-21680", "ibm-21681", "ibm-25776", "ibm-25777", "ibm-29872", "ibm-29873", "ibm-61955", "ibm-61956",
             "windows-1201", "cp1200", "cp1201", "UTF16_BigEndian"},
            () => { return Encoding.BigEndianUnicode; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-16LE", "x-utf-16le", "UnicodeLittleUnmarked", "ibm-1202", "ibm-1203", "ibm-13490", "ibm-13491", "ibm-17586", "ibm-17587",
             "ibm-21682", "ibm-21683", "ibm-25778", "ibm-25779", "ibm-29874", "ibm-29875", "UTF16_LittleEndian", "windows-1200"},
            () => { return Encoding.Unicode; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-32", "ISO-10646-UCS-4", "ibm-1236", "ibm-1237", "csUCS4", "ucs-4"},
            () => { return Encoding.UTF32; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-32BE", "UTF32_BigEndian", "ibm-1232", "ibm-1233", "ibm-9424"},
            () => { return Encoding.GetEncoding(12001); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-32LE", "UTF32_LittleEndian", "ibm-1234", "ibm-1235"},
            () => { return Encoding.UTF32; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF16_PlatformEndian"},
            () => { return Encoding.Unicode; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF16_OppositeEndian"},
            () => { return Encoding.BigEndianUnicode; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF32_PlatformEndian"},
            () => { return Encoding.UTF32; }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"UTF32_OppositeEndian"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-16BE,version=1", "UnicodeBig"},
            () => { return Encoding.BigEndianUnicode; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-16LE,version=1", "UnicodeLittle", "x-UTF-16LE-BOM",
             "UTF-16,version=1", "UTF-16,version=2"},
            () => { return Encoding.Unicode; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"UTF-7", "windows-65000", "unicode-1-1-utf-7", "unicode-2-0-utf-7"},
            () => { return Encoding.UTF7; }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"IMAP-mailbox-name"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"SCSU", "ibm-1212", "ibm-1213"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"BOCU-1", "csBOCU-1", "ibm-1214", "ibm-1215"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"CESU-8", "ibm-9400"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISO-8859-1", "ibm-819", "IBM819", "cp819", "latin1", "8859_1", "csISOLatin1", "iso-ir-100", "ISO_8859-1:1987",
            "l1", "819", "windows-28591"},
            () => { return Encoding.GetEncoding(28591); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"US-ASCII", "ASCII", "ANSI_X3.4-1968", "ANSI_X3.4-1986", "ISO_646.irv:1991", "iso_646.irv:1983", "ISO646-US", "us",
             "csASCII", "iso-ir-6", "cp367", "ascii7", "646", "windows-20127", "ibm-367", "IBM367"},
            () => { return Encoding.ASCII; }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"gb18030", "ibm-1392", "windows-54936", "GB18030"},
            () => { return Encoding.GetEncoding(54936); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-912_P100-1995", "ibm-912", "ISO-8859-2", "ISO_8859-2:1987", "latin2", "csISOLatin2",
             "iso-ir-101", "l2", "8859_2", "cp912", "912", "windows-28592"},
            () => { return Encoding.GetEncoding(28592); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-913_P100-2000", "ibm-913", "ISO-8859-3", "ISO_8859-3:1988", "latin3", "csISOLatin3",
             "iso-ir-109", "l3", "8859_3", "cp913", "913", "windows-28593"},
            () => { return Encoding.GetEncoding(28593); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-914_P100-1995", "ibm-914", "ISO-8859-4", "latin4", "csISOLatin4", "iso-ir-110",
             "ISO_8859-4:1988", "l4", "8859_4", "cp914", "914", "windows-28594"},
            () => { return Encoding.GetEncoding(28594); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-915_P100-1995", "ibm-915", "ISO-8859-5", "cyrillic", "csISOLatinCyrillic", "iso-ir-144",
            "ISO_8859-5:1988", "8859_5", "cp915", "915", "windows-28595"},
            () => { return Encoding.GetEncoding(28595); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1089_P100-1995", "ibm-1089", "ISO-8859-6", "arabic", "csISOLatinArabic", "iso-ir-127", "ISO_8859-6:1987", "ECMA-114", "ASMO-708",
             "8859_6", "cp1089", "1089", "windows-28596", "ISO-8859-6-I", "ISO-8859-6-E", "x-ISO-8859-6S"},
            () => { return Encoding.GetEncoding(28596); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-9005_X110-2007", "ibm-9005", "ISO-8859-7", "8859_7", "greek", "greek8", "ELOT_928", "ECMA-118", "csISOLatinGreek",
             "iso-ir-126", "ISO_8859-7:1987", "windows-28597", "sun_eu_greek"},
            () => { return Encoding.GetEncoding(28597); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-813_P100-1995", "ibm-813", "cp813", "813"},
        //    () => { return Encoding.GetEncoding(813); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-5012_P100-1999", "ibm-5012", "ISO-8859-8", "hebrew", "csISOLatinHebrew", "iso-ir-138", "ISO_8859-8:1988",
             "ISO-8859-8-I", "ISO-8859-8-E", "8859_8", "windows-28598", "hebrew8"},
            () => { return Encoding.GetEncoding(28598); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-916_P100-1995", "ibm-916", "cp916", "916"},
        //    () => { return Encoding.GetEncoding(916); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-920_P100-1995", "ibm-920", "ISO-8859-9", "latin5", "csISOLatin5", "iso-ir-148", "ISO_8859-9:1989", "l5",
             "8859_9", "cp920", "920", "windows-28599", "ECMA-128", "turkish8", "turkish"},
            () => { return Encoding.GetEncoding(28599); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"iso-8859_10-1998", "ISO-8859-10", "iso-ir-157", "l6", "ISO_8859-10:1992", "csISOLatin6", "latin6"},
        //    () => { return Encoding.GetEncoding("ISO-8859-10"); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"iso-8859_11-2001", "ISO-8859-11", "thai8", "x-iso-8859-11"},
        //    () => { return Encoding.GetEncoding("ISO-8859-11"); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-921_P100-1995", "ibm-921", "ISO-8859-13", "8859_13", "windows-28603", "cp921", "921", "x-IBM921"},
            () => { return Encoding.GetEncoding(28603); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"iso-8859_14-1998", "ISO-8859-14", "iso-ir-199", "ISO_8859-14:1998", "latin8", "iso-celtic", "l8"},
        //    () => { return Encoding.GetEncoding("ISO-8859-14"); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-923_P100-1998", "ibm-923", "ISO-8859-15", "Latin-9", "l9", "8859_15", "latin0",
            "csisolatin0", "csisolatin9", "iso8859_15_fdis", "cp923", "923", "windows-28605"},
            () => { return Encoding.GetEncoding(28605); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-942_P12A-1999", "ibm-942", "ibm-932", "cp932", "shift_jis78", "sjis78", "ibm-942_VSUB_VPUA", "ibm-932_VSUB_VPUA", "x-IBM942", "x-IBM942C",
             "ibm-943_P15A-2003", "ibm-943", "Shift_JIS", "MS_Kanji", "csShiftJIS", "windows-31j", "csWindows31J", "x-sjis", "x-ms-cp932",
             "cp932", "windows-932", "cp943c", "IBM-943C", "ms932", "pck", "sjis", "ibm-943_VSUB_VPUA", "x-MS932_0213", "x-JISAutoDetect"},
            () => { return Encoding.GetEncoding(932); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase) // Shift_JIS here too in R? "Shift_JIS", 
        //    {"ibm-943_P130-1999", "ibm-943", "cp943", "943", "ibm-943_VASCII_VSUB_VPUA", "x-IBM943"},
        //    () => { return Encoding.GetEncoding(943); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-33722_P12A_P12A-2009_U2", "ibm-33722", "ibm-5050", "ibm-33722_VPUA", "IBM-eucJP",
        //     "ibm-33722_P120-1999", "ibm-33722", "ibm-5050", "cp33722", "33722", "ibm-33722_VASCII_VPUA", "x-IBM33722", "x-IBM33722A", "x-IBM33722C"},
        //    () => { return Encoding.GetEncoding(33722); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-954_P101-2007", "ibm-954", "x-IBM954", "x-IBM954C"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"euc-jp-2007", "EUC-JP", "Extended_UNIX_Code_Packed_Format_for_Japanese", "csEUCPkdFmtJapanese", "X-EUC-JP", "eucjis", "ujis"},
            () => { return Encoding.GetEncoding(51932); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"windows-950-2000", "Big5", "csBig5", "windows-950", "x-windows-950", "x-big5", "ms950",
             "ibm-950_P110-1999", "ibm-950", "cp950", "950", "x-IBM950",
             "ibm-1375_P100-2008", "ibm-1375", "Big5-HKSCS", "big5hk", "HKSCS-BIG5",
             "ibm-1373_P100-2002", "ibm-1373", "windows-950",
             "ibm-5471_P100-2006", "ibm-5471", "Big5-HKSCS", "MS950_HKSCS", "hkbig5", "big5-hkscs:unicode3.0", "x-MS950-HKSCS"},
            () => { return Encoding.GetEncoding(950); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1386_P100-2001", "ibm-1386", "cp1386", "windows-936", "ibm-1386_VSUB_VPUA", "windows-936-2000", "GBK", "CP936", "MS936",
             "ibm-5478_P100-1995", "ibm-5478", "GB_2312-80", "chinese", "iso-ir-58", "csISO58GB231280", "gb2312-1980", "GB2312.1980-0",
             "ibm-1383_P110-1999", "ibm-1383", "GB2312", "csGB2312", "cp1383", "1383", "EUC-CN", "ibm-eucCN", "hp15CN", "ibm-1383_VPUA"},
            () => { return Encoding.GetEncoding(936); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"euc-tw-2014", "EUC-TW"},
        //    () => { return Encoding.GetEncoding("EUC-TW"); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-964_P110-1999", "ibm-964", "ibm-eucTW", "cns11643", "cp964", "964", "ibm-964_VPUA", "x-IBM964"},
        //    () => { return Encoding.GetEncoding(964); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-949_P110-1999", "ibm-949", "cp949", "949", "ibm-949_VASCII_VSUB_VPUA", "x-IBM949",
        //     "ibm-949_P11A-1999", "ibm-949", "cp949c", "ibm-949_VSUB_VPUA", "x-IBM949C", "IBM-949C"},
        //    () => { return Encoding.GetEncoding(949); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-970_P110_P110-2006_U2", "ibm-970", "EUC-KR", "KS_C_5601-1987", "windows-51949", "csEUCKR", "ibm-eucKR", "KSC_5601", "5601", "cp970", "970", "ibm-970_VPUA", "x-IBM970"},
            () => { return Encoding.GetEncoding(51949); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-971_P100-1995", "ibm-971", "ibm-971_VPUA", "x-IBM971"},
        //    () => { return Encoding.GetEncoding(971); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1363_P11B-1998", "ibm-1363", "KS_C_5601-1987", "KS_C_5601-1989", "KSC_5601", "csKSC56011987",
             "korean", "iso-ir-149", "cp1363", "5601", "ksc", "windows-949", "ibm-1363_VSUB_VPUA", "x-IBM1363C", "ms949"},
            () => { return Encoding.GetEncoding(949); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1363_P110-1997", "ibm-1363", "ibm-1363_VASCII_VSUB_VPUA", "x-IBM1363"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-874_P100-1995", "ibm-874", "ibm-9066", "cp874", "TIS-620", "tis620.2533", "eucTH", "x-IBM874",
             "windows-874-2000", "TIS-620", "windows-874", "MS874", "x-windows-874"},
            () => { return Encoding.GetEncoding(874); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1162_P100-1999", "ibm-1162"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-437_P100-1995", "ibm-437", "IBM437", "cp437", "437", "csPC8CodePage437", "windows-437"},
            () => { return Encoding.GetEncoding(437); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-720_P100-1997", "ibm-720", "windows-720", "DOS-720", "x-IBM720"},
            () => { return Encoding.GetEncoding(720); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-737_P100-1997", "ibm-737", "IBM737", "cp737", "windows-737", "737", "x-IBM737"},
            () => { return Encoding.GetEncoding(737); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-775_P100-1996", "ibm-775", "IBM775", "cp775", "csPC775Baltic", "windows-775", "775"},
            () => { return Encoding.GetEncoding(775); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-850_P100-1995", "ibm-850", "IBM850", "cp850", "850", "csPC850Multilingual", "windows-850"},
            () => { return Encoding.GetEncoding(850); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-851_P100-1995", "ibm-851", "IBM851", "cp851", "851", "csPC851"},
            () => { return Encoding.GetEncoding(851); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-852_P100-1995", "ibm-852", "IBM852", "cp852", "852", "csPCp852", "windows-852"},
            () => { return Encoding.GetEncoding(852); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-855_P100-1995", "ibm-855", "IBM855", "cp855", "855", "csIBM855", "csPCp855", "windows-855"},
            () => { return Encoding.GetEncoding(855); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-856_P100-1995", "ibm-856", "IBM856", "cp856", "856", "x-IBM856"},
            () => { return Encoding.GetEncoding(856); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-857_P100-1995", "ibm-857", "IBM857", "cp857", "857", "csIBM857", "windows-857"},
            () => { return Encoding.GetEncoding(857); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-858_P100-1997", "ibm-858", "IBM00858", "CCSID00858", "CP00858", "PC-Multilingual-850+euro", "cp858", "windows-858"},
            () => { return Encoding.GetEncoding(858); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-860_P100-1995", "ibm-860", "IBM860", "cp860", "860", "csIBM860"},
            () => { return Encoding.GetEncoding(860); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-861_P100-1995", "ibm-861", "IBM861", "cp861", "861", "cp-is", "csIBM861", "windows-861"},
            () => { return Encoding.GetEncoding(861); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-862_P100-1995", "ibm-862", "IBM862", "cp862", "862", "csPC862LatinHebrew", "DOS-862", "windows-862"},
            () => { return Encoding.GetEncoding(862); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-863_P100-1995", "ibm-863", "IBM863", "cp863", "863", "csIBM863"},
            () => { return Encoding.GetEncoding(863); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-864_X110-1999", "ibm-864", "IBM864", "cp864", "csIBM864"},
            () => { return Encoding.GetEncoding(864); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-865_P100-1995", "ibm-865", "IBM865", "cp865", "865", "csIBM865"},
            () => { return Encoding.GetEncoding(865); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-866_P100-1995", "ibm-866", "IBM866", "cp866", "866", "csIBM866", "windows-866"},
            () => { return Encoding.GetEncoding(866); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-867_P100-1998", "ibm-867", "x-IBM867"},
            () => { return Encoding.GetEncoding(867); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-868_P100-1995", "ibm-868", "IBM868", "CP868", "868", "csIBM868", "cp-ar"},
            () => { return Encoding.GetEncoding(868); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-869_P100-1995", "ibm-869", "IBM869", "cp869", "869", "cp-gr", "csIBM869", "windows-869"},
            () => { return Encoding.GetEncoding(869); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-878_P100-1996", "ibm-878", "KOI8-R", "koi8", "csKOI8R", "windows-20866", "cp878"},
            () => { return Encoding.GetEncoding(20866); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-901_P100-1999", "ibm-901"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-902_P100-1999", "ibm-902"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-922_P100-1999", "ibm-922", "IBM922", "cp922", "922", "x-IBM922"},
        //    () => { return Encoding.GetEncoding(922); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1168_P100-2002", "ibm-1168", "KOI8-U", "windows-21866"},
            () => { return Encoding.GetEncoding(21866); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-4909_P100-1999", "ibm-4909"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-5346_P100-1998", "ibm-5346", "windows-1250", "cp1250",
             "ibm-1250_P100-1995", "ibm-1250"},
            () => { return Encoding.GetEncoding(1250); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-5347_P100-1998", "ibm-5347", "windows-1251", "cp1251", "ANSI1251",
             "ibm-1251_P100-1995", "ibm-1251"},
            () => { return Encoding.GetEncoding(1251); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-5348_P100-1997", "ibm-5348", "windows-1252", "cp1252",
             "ibm-1252_P100-2000", "ibm-1252"},
            () => { return Encoding.GetEncoding(1252); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-5349_P100-1998", "ibm-5349", "windows-1253", "cp1253",
            "ibm-1253_P100-1995", "ibm-1253"},
            () => { return Encoding.GetEncoding(1253); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-5350_P100-1998", "ibm-5350", "windows-1254", "cp1254",
            "ibm-1254_P100-1995", "ibm-1254"},
            () => { return Encoding.GetEncoding(1254); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-9447_P100-2002", "ibm-9447", "windows-1255", "cp1255",
            "ibm-1255_P100-1995", "ibm-1255", "ibm-5351_P100-1998", "ibm-5351"},
            () => { return Encoding.GetEncoding(1255); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-9448_X100-2005", "ibm-9448", "windows-1256", "cp1256", "x-windows-1256S",
            "ibm-1256_P110-1997", "ibm-1256", "ibm-5352_P100-1998", "ibm-5352"},
            () => { return Encoding.GetEncoding(1256); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-9449_P100-2002", "ibm-9449", "windows-1257", "cp1257",
             "ibm-1257_P100-1995", "ibm-1257", "ibm-5353_P100-1998", "ibm-5353"},
            () => { return Encoding.GetEncoding(1257); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-5354_P100-1998", "ibm-5354", "windows-1258", "cp1258",
             "ibm-1258_P100-1997", "ibm-1258"},
            () => { return Encoding.GetEncoding(1258); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"macos-0_2-10.2", "macintosh", "mac", "csMacintosh", "windows-10000", "macroman", "x-macroman"},
            () => { return Encoding.GetEncoding(10000); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"macos-6_2-10.4", "x-mac-greek", "windows-10006", "macgr", "x-MacGreek"},
            () => { return Encoding.GetEncoding(10006); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"macos-7_3-10.2", "x-mac-cyrillic", "windows-10007", "mac-cyrillic", "maccy", "x-MacCyrillic", "x-MacUkraine"},
            () => { return Encoding.GetEncoding(10007); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"macos-29-10.2", "x-mac-centraleurroman", "windows-10029", "x-mac-ce", "macce", "maccentraleurope", "x-MacCentralEurope"},
            () => { return Encoding.GetEncoding(10029); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"macos-35-10.2", "x-mac-turkish", "windows-10081", "mactr", "x-MacTurkish"},
            () => { return Encoding.GetEncoding(10081); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1051_P100-1995", "ibm-1051", "hp-roman8", "roman8", "r8", "csHPRoman8", "x-roman8"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1276_P100-1995", "ibm-1276", "Adobe-Standard-Encoding", "csAdobeStandardEncoding"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1006_P100-1995", "ibm-1006", "IBM1006", "cp1006", "1006", "x-IBM1006"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1098_P100-1995", "ibm-1098", "IBM1098", "cp1098", "1098", "x-IBM1098"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1124_P100-1996", "ibm-1124", "cp1124", "1124", "x-IBM1124"},
            () => { return Encoding.GetEncoding(1124); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1125_P100-1997", "ibm-1125", "cp1125"},
            () => { return Encoding.GetEncoding(1125); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1129_P100-1997", "ibm-1129"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1131_P100-1997", "ibm-1131", "cp1131"},
            () => { return Encoding.GetEncoding(1131); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1133_P100-1997", "ibm-1133"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISO_2022,locale=ja,version=0", "ISO-2022-JP", "csISO2022JP", "x-windows-iso2022jp", "x-windows-50220"},
            () => { return Encoding.GetEncoding(50222); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISO_2022,locale=ja,version=1", "ISO-2022-JP-1", "JIS_Encoding", "csJISEncoding", "ibm-5054", "JIS", "x-windows-50221"},
            () => { return Encoding.GetEncoding(50221); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ISO_2022,locale=ja,version=2", "ISO-2022-JP-2", "csISO2022JP2"},
        //    () => { return Encoding.GetEncoding("ISO-2022-JP-2"); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ISO_2022,locale=ja,version=3", "JIS7"},
        //    () => { return Encoding.GetEncoding("JIS7"); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ISO_2022,locale=ja,version=4", "JIS8"},
        //    () => { return Encoding.GetEncoding("JIS8"); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISO_2022,locale=ko,version=0", "ISO-2022-KR", "csISO2022KR"},
            () => { return Encoding.GetEncoding(50225); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ISO_2022,locale=ko,version=1", "ibm-25546"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISO_2022,locale=zh,version=0", "ISO-2022-CN", "csISO2022CN", "x-ISO-2022-CN-GB"},
            () => { return Encoding.GetEncoding("ISO-2022-CN"); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ISO_2022,locale=zh,version=1", "ISO-2022-CN-EXT"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ISO_2022,locale=zh,version=2", "ISO-2022-CN-CNS", "x-ISO-2022-CN-CNS"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"HZ", "HZ-GB-2312"},
            () => { return Encoding.GetEncoding("GB2312"); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"x11-compound-text", "COMPOUND_TEXT", "x-compound-text"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=0", "x-ISCII91", "x-iscii-de", "windows-57002", "iscii-dev", "ibm-4902"},
            () => { return Encoding.GetEncoding(57002); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=1", "x-iscii-be", "windows-57003", "iscii-bng", "windows-57006", "x-iscii-as"},
            () => { return Encoding.GetEncoding(57003); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=2", "x-iscii-pa", "windows-57011", "iscii-gur"},
            () => { return Encoding.GetEncoding(57011); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=3", "x-iscii-gu", "windows-57010", "iscii-guj"},
            () => { return Encoding.GetEncoding(57010); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=4", "x-iscii-or", "windows-57007", "iscii-ori"},
            () => { return Encoding.GetEncoding(57007); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=5", "x-iscii-ta", "windows-57004", "iscii-tml"},
            () => { return Encoding.GetEncoding(57004); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=6", "x-iscii-te", "windows-57005", "iscii-tlg"},
            () => { return Encoding.GetEncoding(57005); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=7", "x-iscii-ka", "windows-57008", "iscii-knd"},
            () => { return Encoding.GetEncoding(57008); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ISCII,version=8", "x-iscii-ma", "windows-57009", "iscii-mlm"},
            () => { return Encoding.GetEncoding(57009); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"LMBCS-1", "lmbcs", "ibm-65025"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-37_P100-1995", "ibm-37", "IBM037", "ibm-037", "ebcdic-cp-us", "ebcdic-cp-ca", "ebcdic-cp-wt", "ebcdic-cp-nl", "csIBM037", "cp037", "037", "cpibm37", "cp37"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-273_P100-1995", "ibm-273", "IBM273", "CP273", "csIBM273", "ebcdic-de", "273"},
            () => { return Encoding.GetEncoding(20273); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-277_P100-1995", "ibm-277", "IBM277", "cp277", "EBCDIC-CP-DK", "EBCDIC-CP-NO", "csIBM277", "ebcdic-dk", "277"},
            () => { return Encoding.GetEncoding(20277); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-278_P100-1995", "ibm-278", "IBM278", "cp278", "ebcdic-cp-fi", "ebcdic-cp-se", "csIBM278", "ebcdic-sv", "278"},
            () => { return Encoding.GetEncoding(20278); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-280_P100-1995", "ibm-280", "IBM280", "CP280", "ebcdic-cp-it", "csIBM280", "280"},
            () => { return Encoding.GetEncoding(20280); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-284_P100-1995", "ibm-284", "IBM284", "CP284", "ebcdic-cp-es", "csIBM284", "cpibm284", "284"},
            () => { return Encoding.GetEncoding(20284); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-285_P100-1995", "ibm-285", "IBM285", "CP285", "ebcdic-cp-gb", "csIBM285", "cpibm285", "ebcdic-gb", "285"},
            () => { return Encoding.GetEncoding(20285); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-290_P100-1995", "ibm-290", "IBM290", "cp290", "EBCDIC-JP-kana", "csIBM290"},
            () => { return Encoding.GetEncoding(20290); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-297_P100-1995", "ibm-297", "IBM297", "cp297", "ebcdic-cp-fr", "csIBM297", "cpibm297", "297"},
            () => { return Encoding.GetEncoding(20297); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-420_X120-1999", "ibm-420", "IBM420", "cp420", "ebcdic-cp-ar1", "csIBM420", "420"},
            () => { return Encoding.GetEncoding(20420); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-424_P100-1995", "ibm-424", "IBM424", "cp424", "ebcdic-cp-he", "csIBM424", "424"},
            () => { return Encoding.GetEncoding(20424); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-500_P100-1995", "ibm-500", "IBM500", "CP500", "ebcdic-cp-be", "csIBM500", "ebcdic-cp-ch", "500"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-803_P100-1999", "ibm-803", "cp803"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-838_P100-1995", "ibm-838", "IBM838", "IBM-Thai", "csIBMThai", "cp838", "838", "ibm-9030"},
            () => { return Encoding.GetEncoding(20838); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-870_P100-1995", "ibm-870", "IBM870", "CP870", "ebcdic-cp-roece", "ebcdic-cp-yu", "csIBM870"},
            () => { return Encoding.GetEncoding(870); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-871_P100-1995", "ibm-871", "IBM871", "ebcdic-cp-is", "csIBM871", "CP871", "ebcdic-is", "871"},
            () => { return Encoding.GetEncoding(20871); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-875_P100-1995", "ibm-875", "IBM875", "cp875", "875", "x-IBM875"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-918_P100-1995", "ibm-918", "IBM918", "CP918", "ebcdic-cp-ar2", "csIBM918"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-930_P120-1999", "ibm-930", "ibm-5026", "IBM930", "cp930", "930", "x-IBM930", "x-IBM930A"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-933_P110-1995", "ibm-933", "cp933", "933", "x-IBM933"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-935_P110-1999", "ibm-935", "cp935", "935", "x-IBM935"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-937_P110-1999", "ibm-937", "cp937", "937", "x-IBM937"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-939_P120-1999", "ibm-939", "ibm-931", "ibm-5035", "IBM939", "cp939", "939", "x-IBM939", "x-IBM939A"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1025_P100-1995", "ibm-1025", "cp1025", "1025", "x-IBM1025"},
            () => { return Encoding.GetEncoding(21025); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1026_P100-1995", "ibm-1026", "IBM1026", "CP1026", "csIBM1026", "1026"},
            () => { return Encoding.GetEncoding(1026); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1047_P100-1995", "ibm-1047", "IBM1047", "cp1047", "1047"},
            () => { return Encoding.GetEncoding(1047); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1097_P100-1995", "ibm-1097", "cp1097", "1097", "x-IBM1097"},
        //    () => { return Encoding.GetEncoding(1097); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1112_P100-1995", "ibm-1112", "cp1112", "1112", "x-IBM1112"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1122_P100-1999", "ibm-1122", "cp1122", "1122", "x-IBM1122"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1123_P100-1995", "ibm-1123", "cp1123", "1123", "x-IBM1123"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1130_P100-1997", "ibm-1130"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1132_P100-1998", "ibm-1132"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1137_P100-1999", "ibm-1137"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-4517_P100-2005", "ibm-4517"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1140_P100-1997", "ibm-1140", "IBM01140", "CCSID01140", "CP01140", "cp1140", "ebcdic-us-37+euro"},
            () => { return Encoding.GetEncoding(1140); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1141_P100-1997", "ibm-1141", "IBM01141", "CCSID01141", "CP01141", "cp1141", "ebcdic-de-273+euro"},
            () => { return Encoding.GetEncoding(1141); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1142_P100-1997", "ibm-1142", "IBM01142", "CCSID01142", "CP01142", "cp1142", "ebcdic-dk-277+euro", "ebcdic-no-277+euro"},
            () => { return Encoding.GetEncoding(1142); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1143_P100-1997", "ibm-1143", "IBM01143", "CCSID01143", "CP01143", "cp1143", "ebcdic-fi-278+euro", "ebcdic-se-278+euro"},
            () => { return Encoding.GetEncoding(1143); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1144_P100-1997", "ibm-1144", "IBM01144", "CCSID01144", "CP01144", "cp1144", "ebcdic-it-280+euro"},
            () => { return Encoding.GetEncoding(1144); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1145_P100-1997", "ibm-1145", "IBM01145", "CCSID01145", "CP01145", "cp1145", "ebcdic-es-284+euro"},
            () => { return Encoding.GetEncoding(1145); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1146_P100-1997", "ibm-1146", "IBM01146", "CCSID01146", "CP01146", "cp1146", "ebcdic-gb-285+euro"},
            () => { return Encoding.GetEncoding(1146); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1147_P100-1997", "ibm-1147", "IBM01147", "CCSID01147", "CP01147", "cp1147", "ebcdic-fr-297+euro"},
            () => { return Encoding.GetEncoding(1147); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1148_P100-1997", "ibm-1148", "IBM01148", "CCSID01148", "CP01148", "cp1148", "ebcdic-international-500+euro"},
            () => { return Encoding.GetEncoding(1148); }
        },

        { new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"ibm-1149_P100-1997", "ibm-1149", "IBM01149", "CCSID01149", "CP01149", "cp1149", "ebcdic-is-871+euro"},
            () => { return Encoding.GetEncoding(1149); }
        },

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1153_P100-1999", "ibm-1153", "IBM1153", "x-IBM1153"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1154_P100-1999", "ibm-1154"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1155_P100-1999", "ibm-1155"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1156_P100-1999", "ibm-1156"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1157_P100-1999", "ibm-1157"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1158_P100-1999", "ibm-1158"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1160_P100-1999", "ibm-1160"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1164_P100-1999", "ibm-1164"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1364_P110-2007", "ibm-1364", "x-IBM1364"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1371_P100-1999", "ibm-1371", "x-IBM1371"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1388_P103-2001", "ibm-1388", "ibm-9580", "x-IBM1388"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1390_P110-2003", "ibm-1390", "x-IBM1390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1399_P110-2003", "ibm-1399", "x-IBM1399"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-5123_P100-1999", "ibm-5123"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-8482_P100-1999", "ibm-8482"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-16684_P110-2003", "ibm-16684", "ibm-20780"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-4899_P100-1998", "ibm-4899"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-4971_P100-1999", "ibm-4971"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-9067_X100-2005", "ibm-9067"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-12712_P100-1998", "ibm-12712", "ebcdic-he"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-16804_X110-1999", "ibm-16804", "ebcdic-ar"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-37_P100-1995,swaplfnl", "ibm-37-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1047_P100-1995,swaplfnl", "ibm-1047-s390", "IBM1047_LF"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1140_P100-1997,swaplfnl", "ibm-1140-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1141_P100-1997,swaplfnl", "ibm-1141-s390", "IBM1141_LF"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1142_P100-1997,swaplfnl", "ibm-1142-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1143_P100-1997,swaplfnl", "ibm-1143-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1144_P100-1997,swaplfnl", "ibm-1144-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1145_P100-1997,swaplfnl", "ibm-1145-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1146_P100-1997,swaplfnl", "ibm-1146-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1147_P100-1997,swaplfnl", "ibm-1147-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1148_P100-1997,swaplfnl", "ibm-1148-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1149_P100-1997,swaplfnl", "ibm-1149-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-1153_P100-1999,swaplfnl", "ibm-1153-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-12712_P100-1998,swaplfnl", "ibm-12712-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},

        //{ new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        //    {"ibm-16804_X110-1999,swaplfnl", "ibm-16804-s390"},
        //    () => { return Encoding.GetEncoding(); }
        //},
    };
    }
}
#endif
