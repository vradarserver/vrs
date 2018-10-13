// Copyright © 2013 onwards, Andrew Whewell and Vincent Deng
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

(function(VRS, /** jQuery= */ $, /** object= */ undefined)
{
    // Singleton declaration
    VRS.$$ = VRS.$$ || {};

    // If your language has a different month format when days preceed months, and the date picker
    // should be using that month format, then set this to true. Otherwise leave at false.
    VRS.$$.DateUseGenetiveMonths = false;

    // [[ MARKER START SIMPLE STRINGS ]]
    // [[ MARKER END SIMPLE STRINGS ]]

    /*
        See the notes below against 'Complicated strings'. This function takes an array of stopovers in a route and
        joins them together into a single sentence for the text-to-speech conversion. So if it is passed an arary
        of "First stopover", "Second stopover" and "Third stopover" then it will return the string
        "First stopover, Second stopover and Third stopover".
     */
    VRS.$$.sayStopovers = function(stopovers)
    {
        var result = '';
        var length = stopovers.length;
        for(var i = 0;i < length;++i) {
            var stopover = stopovers[i];
            var isFirstStopover = i === 0;
            var isLastStopover = i + 1 === length;
            var isMiddleStopover = !isFirstStopover && !isLastStopover;

            if(isLastStopover)        result += ' 和 ';
            else if(isMiddleStopover) result += ', ';

            result += stopover;
        }

        return result;
    };

    // Complicated strings
    /*
       These are javascript functions that take a number of parameters and format some text from them. A function is
       always of this form:
         VRS.$$.<name of function> = function(parameter 1, parameter 2, ..., parameter n)
         {
            ... body of function ...
         };
       Only translate text within single apostrophes in functions. If the English version of a function will suffice
       for your language then delete the function entirely so that the site falls back onto the English version.

       If you are not comfortable with translating text within functions then let me know how the text should be
       displayed in your language and I'll do the function for you.
    */

    /**
     * Returns an elapsed time as a string.
     * @param {number} hours
     * @param {number} minutes
     * @param {number} seconds
     * @param {bool=} showZeroHours
     * @returns {string}
     */
    VRS.$$.formatHoursMinutesSeconds = function(hours, minutes, seconds, showZeroHours)
    {
        /*
            jQuery Globalize only allows formatting of full date-times, which is no good when we want to display spans
            of time larger than 24 hours. The English version of this returns either H:MM:SS or MM:SS depending on
            whether hours is zero and whether showZeroHours is true or false.
        */
        var result = '';
        if(hours || showZeroHours) result = hours.toString() + ':';
        result += VRS.stringUtility.formatNumber(minutes, '00') + ':';
        result += VRS.stringUtility.formatNumber(seconds, '00');

        return result;
    };

    /**
     * Returns the count of engines and the engine type as a translated string.
     * @param {string} countEngines
     * @param {string} engineType
     * @returns {string}
     */
    VRS.$$.formatEngines = function(countEngines, engineType)
    {
        /*
           Returns a string showing the count of engines and the engine type. Examples in English are:
             countEngines = '1' and engine type = VRS.EngineType.Jet:     'Single jet'
             countEngines = '10' and engine type = VRS.EngineType.Piston: '10 piston'
        */
        var result = '';

        switch(countEngines) {
            case 'C':       result = '复合'; break;
            case '1':       result = '单'; break;
            case '2':       result = '双'; break;
            case '3':       result = '三'; break;
            case '4':       result = '四'; break;
            case '5':       result = '五'; break;
            case '6':       result = '六'; break;
            case '7':       result = '七'; break;
            case '8':       result = '八'; break;
            default:        result = countEngines; break;
        }

        switch(engineType) {
            case VRS.EngineType.Electric:   result += ' 电力引擎'; break;
            case VRS.EngineType.Jet:        result += ' 喷气引擎'; break;
            case VRS.EngineType.Piston:     result += ' 活塞引擎'; break;
            case VRS.EngineType.Rocket:     result += ' rocket'; break;
            case VRS.EngineType.Turbo:      result += ' 涡轮增压引擎'; break;
        }

        return result;
    };

    /**
     * Translates the wake turbulence category description.
     * @param {string} category
     * @param {bool} ignoreNone
     * @param {bool} expandedDescription
     * @returns {string}
     */
    VRS.$$.formatWakeTurbulenceCategory = function(category, ignoreNone, expandedDescription)
    {
        /*
           Returns a string showing the wake turbulence category. What makes this different from a simple
           substitution is that in some places I want to show the weight limits for each category. In
           English these follow the category - e.g. Light (up to 7 tons) - but this may not be appropriate
           in other locales.
        */

        var result = '';
        if(category) {
            switch(category) {
                case VRS.WakeTurbulenceCategory.None:   if(!ignoreNone) result = '未知'; break;
                case VRS.WakeTurbulenceCategory.Light:  result = '轻微'; break;
                case VRS.WakeTurbulenceCategory.Medium: result = '中等'; break;
                case VRS.WakeTurbulenceCategory.Heavy:  result = '严重'; break;
                default: throw '未知尾流类型 ' + category;  // Do not translate this line
            }

            if(expandedDescription && result) {
                switch(category) {
                    case VRS.WakeTurbulenceCategory.Light:  result += ' (达到七吨)'; break;
                    case VRS.WakeTurbulenceCategory.Medium: result += ' (达到一百三十五吨)'; break;
                    case VRS.WakeTurbulenceCategory.Heavy:  result += ' (超过一百三十五吨)'; break;
                }
            }
        }

        return result;
    };

    /**
     * Returns the full route details.
     * @param {string} from
     * @param {string} to
     * @param {string[]} via
     * @returns {string}
     */
    VRS.$$.formatRoute = function(from, to, via)
    {
        /*
            Returns a string showing the full route. From and to are strings describing the airport (in English - these
            come out of a database of thousands of English airport descriptions, it would be a nightmare to translate them)
            and via is an array of strings describing airports. In English the end result would be one of:
                From AIRPORT to AIRPORT
                To AIRPORT
                From AIRPORT to AIRPORT via AIRPORT
                From AIRPORT to AIRPORT via AIRPORT, AIRPORT (..., AIRPORT)
                To AIRPORT via AIRPORT
                To AIRPORT via AIRPORT, AIRPORT (..., AIRPORT)
         */
        var result = '';
        if(from) result = '从 ' + from;
        if(to) {
            if(result.length) result += ' 到 ';
            else              result = '到 ';
            result += to;
        }
        var stopovers = via ? via.length : 0;
        if(stopovers > 0) {
            result += ' 途径';
            for(var i = 0;i < stopovers;++i) {
                var stopover = via[i].val;
                if(i > 0) result += ',';
                result += ' ' + stopover;
            }
        }

        return result;
    };

    /**
     * Translates the country name.
     * @param {string} englishCountry
     * @returns {string}
     */
    VRS.$$.translateCountry = function(englishCountry)
    {
        /*
            Returns a translation of the country. If you are happy with English country names then just delete this function
            and the English version will be used. Otherwise you can delete the following line:
        */

        switch (englishCountry) {
             //case 'United States': return '美国';
            case 'Afghanistan':             return '阿富汗';
            case 'Albania':                 return '阿尔巴尼亚';
            case 'Algeria':                 return '阿尔及利亚';
            case 'Angola':                  return '安哥拉';
            case 'Antigua and Barbuda':     return '安提瓜和巴布达';
            case 'Argentina':               return '阿根廷';
            case 'Armenia':                 return '亚美尼亚';
            case 'Aruba':                   return '阿鲁巴岛';
            case 'Australia':               return '奥大利亚';
            case 'Austria':                 return '奥地利';
            case 'Azerbaijan':              return '阿塞拜疆';
            case 'Bahamas':                 return '巴哈马群岛';
            case 'Bahrain':                 return '巴林岛';
            case 'Bangladesh':              return '孟加拉国';
            case 'Barbados':                return '巴巴多斯';
            case 'Belarus':                 return '白俄罗斯';
            case 'Belgium':                 return '比利时';
            case 'Belize':                  return '伯利兹城';
            case 'Benin':                   return '贝宁';
            case 'Bermuda':                 return '百慕大群岛';
            case 'Bhutan':                  return '不丹';
            case 'Bolivia':                 return '玻利维亚';
            case 'Bosnia and Herzegovina':  return '波斯尼亚和黑塞哥维那';
            case 'Botswana':                return '博茨瓦纳';
            case 'Brazil':                  return '巴西';
            case 'Brunei':                  return '文莱';
            case 'Bulgaria':                return '保加利亚';
            case 'Burkina Faso':            return '布基纳法索';
            case 'Burundi':                 return '布隆迪';
            case 'Cambodia':                return '柬埔寨';
            case 'Cameroon':                return '喀麦隆';
            case 'Canada':                  return '加拿大';
            case 'Cape Verde':              return '佛得角';
            case 'Cayman Islands':          return '开曼群岛';
            case 'Central African Republic':
                                            return '中非共和国';
            case 'Chad':                    return '乍得';
            case 'Chile':                   return '智利';
            case 'China':                   return '中国';
            case 'Hong Kong':               return '香港';
            case 'Macau':                   return '澳门';
            case 'Colombia':                return '哥伦比亚';
            case 'Comoros':                 return '科摩罗';
            case 'Congo (Kinshasa)':        return '刚果（金沙萨）';
            case 'Congo (Brazzaville)':     return '刚果（布拉柴维尔）';
            case 'Cook Islands':            return '科克群岛';
            case 'Costa Rica':              return '哥斯达黎加';
            case 'Côte d\'Ivoire':          return '科特迪瓦';
            case 'Croatia':                 return '克罗地亚';
            case 'Cuba':                    return '古巴';
            case 'Cyprus':                  return '塞浦路斯';
            case 'Czech Republic':          return '捷克共和国';
            case 'Denmark':                 return '丹麦';
            case 'Djibouti':                return '吉布提';
            case 'Dominican Republic':      return '多米尼加共和国';
            case 'Ecuador':                 return '厄瓜多尔共和国';
            case 'Egypt':                   return '埃及';
            case 'El Salvador':             return '萨尔瓦多';
            case 'Equatorial Guinea':       return '赤道几内亚';
            case 'Eritrea':                 return '厄立特里亚国';
            case 'Estonia':                 return '爱沙尼亚';
            case 'Ethiopia':                return '埃塞俄比亚';
            case 'Falkland Islands':        return '福克兰群岛';
            case 'Fiji':                    return '斐济';
            case 'Finland':                 return '芬兰';
            case 'France':                  return '法国';
            case 'Gabon':                   return '加蓬';
            case 'Gambia':                  return '冈比亚';
            case 'Georgia':                 return '格鲁吉亚';
            case 'Germany':                 return '德国';
            case 'Ghana':                   return '加纳';
            case 'Greece':                  return '希腊';
            case 'Grenada':                 return '格林纳达';
            case 'Guatemala':               return '危地马拉';
            case 'Guernsey':                return '格恩西岛';
            case 'Guinea':                  return '几内亚';
            case 'Guinea-Bissau':           return '几内亚比绍';
            case 'Guyana':                  return '圭亚那';
            case 'Haiti':                   return '海地';
            case 'Honduras':                return '洪都拉斯';
            case 'Hungary':                 return '匈牙利';
            case 'NATO':                    return '北约组织';
            case 'ICAO':                    return '国际民航组织';
            case 'Iceland':                 return '冰岛';
            case 'India':                   return '印度';
            case 'Indonesia':               return '印度尼西亚';
            case 'Iran':                    return '伊朗';
            case 'Iraq':                    return '伊拉克';
            case 'Ireland':                 return '爱尔兰';
            case 'Isle of Man':             return '马恩岛(英国)';
            case 'Israel':                  return '以色列';
            case 'Italy':                   return '意大利';
            case 'Jamaica':                 return '牙买加';
            case 'Japan':                   return '日本';
            case 'Jordan':                  return '约旦';
            case 'Kazakhstan':              return '哈萨克斯坦';
            case 'Kenya':                   return '肯尼亚';
            case 'Kiribati':                return '基里巴斯';
            case 'North Korea':             return '朝鲜';
            case 'South Korea':             return '韩国';
            case 'Kuwait':                  return '科威特';
            case 'Kyrgyzstan':              return '吉尔吉斯斯坦';
            case 'Laos':                    return '老挝';
            case 'Latvia':                  return '拉脱维亚';
            case 'Lebanon':                 return '黎巴嫩';
            case 'Lesotho':                 return '莱索托';
            case 'Liberia':                 return '利比里亚';
            case 'Libya':                   return '利比亚';
            case 'Lithuania':               return '立陶宛';
            case 'Luxembourg':              return '卢森堡';
            case 'Macedonia':               return '马其顿';
            case 'Madagascar':              return '马达加斯加';
            case 'Malawi':                  return '马拉维';
            case 'Malaysia':                return '马来西亚';
            case 'Maldives':                return '马尔代夫';
            case 'Mali':                    return '马里';
            case 'Malta':                   return '马耳他';
            case 'Marshall Islands':        return '马绍尔群岛';
            case 'Mauritania':              return '毛里塔尼亚';
            case 'Mauritius':               return '毛里求斯';
            case 'Mexico':                  return '墨西哥';
            case 'Micronesia':              return '密克罗尼西亚';
            case 'Moldova':                 return '摩尔多瓦';
            case 'Monaco':                  return '摩纳哥';
            case 'Mongolia':                return '蒙古';
            case 'Montserrat':              return '蒙特色拉特岛';
            case 'Montenegro':              return '黑山共和国';
            case 'Morocco':                 return '摩洛哥';
            case 'Mozambique':              return '莫桑比克';
            case 'Myanmar':                 return '缅甸';
            case 'Namibia':                 return '纳米比亚';
            case 'Nauru':                   return '瑙鲁';
            case 'Nepal':                   return '尼泊尔';
            case 'Netherlands':             return '荷兰';
            case 'Netherlands Antilles':    return '荷属安的列斯群岛';
            case 'New Zealand':             return '新西兰';
            case 'Nicaragua':               return '尼加拉瓜';
            case 'Niger':                   return '尼日尔';
            case 'Nigeria':                 return '尼日利亚';
            case 'Norway':                  return '挪威';
            case 'Oman':                    return '阿曼';
            case 'Pakistan':                return '巴基斯坦';
            case 'Palau':                   return '帕劳';
            case 'Panama':                  return '巴拿马';
            case 'Papua New Guinea':        return '巴布亚新几内亚';
            case 'Paraguay':                return '巴拉圭';
            case 'Perú':                    return '秘鲁';
            case 'Philippines':             return '菲律宾';
            case 'Poland':                  return '波兰';
            case 'Portugal':                return '葡萄牙';
            case 'Qatar':                   return '卡塔尔';
            case 'Romania':                 return '罗马尼亚';
            case 'Russia':                  return '俄罗斯';
            case 'Rwanda':                  return '卢旺达';
            case 'Samoa':                   return '萨摩亚';
            case 'San Marino':              return '圣马力诺';
            case 'São Tomé and Principe':   return '圣多美和普林西比';
            case 'Saudi Arabia':            return '沙特阿拉伯';
            case 'Senegal':                 return '塞内加尔';
            case 'Serbia':                  return '塞尔维亚';
            case 'Seychelles':              return '塞舌尔';
            case 'Sierra Leone':            return '塞拉利昂';
            case 'Singapore':               return '新加坡';
            case 'Slovakia':                return '斯洛伐克';
            case 'Slovenia':                return '斯洛文尼亚';
            case 'Solomon Islands':         return '所罗门群岛';
            case 'Somalia':                 return '索马里';
            case 'South Africa':            return '南非';
            case 'Spain':                   return '西班牙';
            case 'Sri Lanka':               return '斯里兰卡';
            case 'Saint Lucia':             return '圣卢西亚岛';
            case 'Saint Vincent and the Grenadines':
                                            return '圣文森特和格林纳丁斯';
            case 'Sudan':                   return '苏丹';
            case 'Suriname':                return '苏里南';
            case 'Swaziland':               return '斯威士兰';
            case 'Sweden':                  return '瑞典';
            case 'Switzerland':             return '瑞士';
            case 'Syria':                   return '叙利亚共和国';
            case 'Taiwan':                  return '台湾';
            case 'Tajikistan':              return '塔吉克斯坦';
            case 'Tanzania':                return '坦桑尼亚';
            case 'Thailand':                return '泰国';
            case 'Togo':                    return '多哥';
            case 'Tonga':                   return '汤加';
            case 'Trinidad and Tobago':     return '特立尼达和多巴哥';
            case 'Tunisia':                 return '突尼斯';
            case 'Turkey':                  return '土耳其';
            case 'Turkmenistan':            return '土库曼斯坦';
            case 'Uganda':                  return '乌干达';
            case 'Ukraine':                 return '乌克兰';
            case 'United Arab Emirates':    return '阿拉伯联合酋长国';
            case 'United Kingdom':          return '英国';
            case 'United States':           return '美国';
            case 'Uruguay':                 return '乌拉圭';
            case 'Uzbekistan':              return '乌兹别克斯坦';
            case 'Vanuatu':                 return '瓦努阿图';
            case 'Venezuela':               return '委内瑞拉';
            case 'Viet Nam':                return '越南';
            case 'Yemen':                   return '也门';
            case 'Zambia':                  return '赞比亚';
            case 'Zimbabwe':                return '津巴布韦';
            //case 'Unknown or unassigned country':          return '';
            case 'Kosovo':                  return '科索沃';
            case 'Martinique':              return '马提尼克岛(法国)';
            case 'Guadeloupe':              return '瓜德罗普岛';
            case 'Saint Helena':            return '圣赫勒拿(英国)';
            case 'Jersey':                  return '泽西岛(英国)';
            case 'Puerto Rico':             return '波多黎各';
            case 'British Indian Ocean Territory':
                                            return '英属印度洋领地';
            case 'Curaçao':                 return '库拉索';
            case 'New Caledonia':           return '新喀里多尼亚';
            case 'Saint Kitts and Nevis':   return '圣基茨和尼维斯';
            case 'U.S. Virgin Islands':     return '美属维尔京群岛';
            case 'Turks and Caicos Islands':
                                            return '英属特克斯和凯科斯群岛';
            case 'Sint Maarten':            return '荷属圣马丁';
            case 'French Guiana':           return '法属圭亚那';
            case 'Réunion':                 return '留尼旺(法国)';
            case 'French Polynesia':        return '法属玻里尼西亚';
            case 'Western Sahara':          return '西撒哈拉';
            case 'Gibraltar':               return '直布罗陀';
            case 'Guam':                    return '关岛(美国)';
            case 'Caribbean Netherlands':   return '荷兰加勒比区';
            case 'Northern Mariana Islands':
                                            return '北马里亚纳群岛';
            case 'Mayotte':                 return '马约特岛';
            case 'Christmas Island':        return '圣延岛(澳大利亚)';
            case 'Cocos (Keeling) Islands': return '科科斯（基林）群岛(澳大利亚)';
            case 'Greenland':               return '格陵兰(丹麦)';
            case 'South Sudan':             return '南苏丹';
            case 'Saint Martin':            return '法属圣马丁';
            case 'Anguilla':                return '安圭拉岛';
            case 'British Virgin Islands':  return '英属维尔京群岛';
            case 'American Samoa':          return '美属萨摩亚';
            case 'Dominica':                return '多米尼加岛';
            case 'Norfolk Island':          return '诺福克岛(澳大利亚)';
            case 'Niue':                    return '纽埃岛(新西兰)';
            case 'Wallis and Futuna':       return '法属瓦利斯群岛和富图纳群岛';
            case 'Timor-Leste':             return '东帝汶';
            case 'Faroe Islands':           return '法罗群岛';
            case 'Saint Pierre and Miquelon':
                                            return '圣皮埃尔和密克隆群岛(法国)';
            case 'Unknown or unassigned country':
                                            return '未知或未赋值的国家';
                
            default: return englishCountry;
         }

        //return englishCountry;

        /*
            and then remove the "//" from the start of every line after these comments and fill in the translations for the
            lines that start with 'case'. The format for every translated country should be:
                case 'English country name':  return 'ENTER YOUR TRANSLATION HERE';
            Unfortunately the English country names are coming out of the StandingData.sqb database file on the server. You
            can either extract them from there (if you can use sqlite3) or email me and I'll send you a version of this
            function with all of the English country names filled in for the current set of countries. You'll need to update
            this as-and-when the countries change. There are currently about 250 countries in the Standing Data database but
            any that you do not provide a translation for will just be shown in English. Delete the case lines for countries
            where your language's name is the same as the English name.

            If you need to use an apostrophe in your translation then change the single-quotes around the name to double-
            quotes, e.g.
                case 'Ivory Coast':     return "Republique de Cote d'Ivoire";
        */

        // switch(englishCountry) {
        //     case 'Germany':          return 'Allemagne';
        //     case 'United Kingdom':   return 'Grande-Bretagne';
        //     default:                 return englishCountry;
        // }
    };
}(window.VRS = window.VRS || {}, jQuery));
