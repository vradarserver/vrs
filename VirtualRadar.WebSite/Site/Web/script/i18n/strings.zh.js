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

    // Simple strings.
    /*
       Numbers in braces (e.g. the {0} in 'FL{0}') mark a point where a value will be substituted into the text.
       You can move these markers around (e.g. you can make it '{0}FL') but you must not remove them or alter
       the value within the braces. If you want to use an open or closing brace in these strings then you must
       enter two braces - e.g. to display {FL10} you would have to use '{{FL{0}}}'.

       Quotation marks (i.e. "") can be replaced with more appropriate values. If you want to use an apostrophe
       in the text then put a backslash before it (e.g. 'It\'s never too late') or put the entire text in
       double-quotes instead of single-quotes (e.g. "It's never too late").

       If you want to use a backslash in your text then enter two backslashes - e.g. '\\o/' will display as '\o/'.

       The semi-colon (;) at the end of each line is important. Leave those in place. Only translate the text
       within apostrophes.
    */

    // [[ MARKER START SIMPLE STRINGS ]]

    VRS.$$.Add =                                        '添加';
    VRS.$$.AddCondition =                               '添加条件';
    VRS.$$.AddCriteria =                                '添加规则';
    VRS.$$.AddFilter =                                  '添加过滤器';
    VRS.$$.ADSB =                                       'ADS-B';  /** THIS IS NEW! **/
    VRS.$$.ADSB0 =                                      'ADS-B v0';  /** THIS IS NEW! **/
    VRS.$$.ADSB1 =                                      'ADS-B v1';  /** THIS IS NEW! **/
    VRS.$$.ADSB2 =                                      'ADS-B v2';  /** THIS IS NEW! **/
    VRS.$$.AircraftNotTransmittingCallsign =            '航空器没有传输其航班号';
    VRS.$$.AircraftClass =                              '航空器类型';
    VRS.$$.Airport =                                    '机场';
    VRS.$$.AirportDataThumbnails =                      '缩略图 (来自airport-data.com)';
    VRS.$$.AllAltitudes =                               '所有高度';
    VRS.$$.AllRows =                                    '所有行';
    VRS.$$.Altitude =                                   '高度';
    VRS.$$.AltitudeAndSpeedGraph =                      '高度&速度图';
    VRS.$$.AltitudeAndVerticalSpeed =                   '高度&垂直速度';
    VRS.$$.AltitudeGraph =                              '高度图';
    VRS.$$.AltitudeType =                               '高度类型';
    VRS.$$.AllAircraft =                                '所有航空器';
    VRS.$$.Amphibian =                                  '两栖飞机';
    VRS.$$.AnnounceSelected =                           '通知所有已选择航空器';
    VRS.$$.Ascending =                                  '升序';
    VRS.$$.AutoSelectAircraft =                         '自动选择航空器';
    VRS.$$.AverageSignalLevel =                         '平均信号电平';
    VRS.$$.Barometric =                                 '气压';
    VRS.$$.Bearing =                                    '方位';
    VRS.$$.Between =                                    '在范围内';
    VRS.$$.Callsign =                                   '航班号';
    VRS.$$.CallsignAndShortRoute =                      '航班号&短路线';
    VRS.$$.CallsignMayNotBeCorrect =                    '可能不正确的航班号';
    VRS.$$.CentreOnSelectedAircraft =                   '在地图上显示航空器';
    VRS.$$.Civil =                                      '民航';
    VRS.$$.CivilOrMilitary =                            '民航/军航';
    VRS.$$.ClosestToCurrentLocation =                   '距离当前最近';
    VRS.$$.CofACategory =                               'C/A(适航证) 类别';  // certificate of airworthiness category
    VRS.$$.CofAExpiry =                                 'C/A(适航证) 期限';  // certificate of airworthiness expiry
    VRS.$$.Columns =                                    '列';
    VRS.$$.Contains =                                   '包含';
    VRS.$$.CountAdsb =                                  'ADS-B计数';
    VRS.$$.Country =                                    '国家';
    VRS.$$.CountModeS =                                 'Mode-S计数';
    VRS.$$.CountPositions =                             '坐标计数';
    VRS.$$.Criteria =                                   '规则';
    VRS.$$.CurrentLocationInstruction =                 '设置你的当前坐标请单击 "设置当前坐标" 并拖拽标记.';
    VRS.$$.CurrentRegDate =                             '有效注册日期';
    VRS.$$.Date =                                       '日期';
    VRS.$$.DateTimeShort =                              '{0} {1}';  // Where "{0}" is a date, e.g. 10/10/2013; and "{1}" is a time, e.g. 17:41:32.
    VRS.$$.DefaultSetting =                             '< 默认 >';
    VRS.$$.DegreesAbbreviation =                        '{0}°';
    VRS.$$.DeRegDate =                                  '取消注册日期';
    VRS.$$.DesktopPage =                                '桌面版页面';
    VRS.$$.DesktopReportPage =                          '桌面版报告页面';
    VRS.$$.DetailItems =                                '航空器详细项';
    VRS.$$.DetailPanel =                                '详情面板';
    VRS.$$.DisableAutoSelect =                          '禁止自动选择';
    VRS.$$.Distance =                                   '距离';
    VRS.$$.Distances =                                  '距离';
    VRS.$$.DoNotImportAutoSelect =                      'Do not import auto-select settings';  /** THIS IS NEW! **/
    VRS.$$.DoNotImportCurrentLocation =                 'Do not import current location';  /** THIS IS NEW! **/
    VRS.$$.DoNotImportRequestFeedId =                   'Do not import request feed ID';  /** THIS IS NEW! **/
    VRS.$$.DoNotImportLanguageSettings =                'Do not import language settings';  /** THIS IS NEW! **/
    VRS.$$.DoNotImportSplitters =                       'Do not import splitters';  /** THIS IS NEW! **/
    VRS.$$.DoNotShow =                                  '不显示';
    VRS.$$.Duration =                                   '持续时间';
    VRS.$$.Electric =                                   '电力引擎';
    VRS.$$.EnableAutoSelect =                           '开启自动选择';
    VRS.$$.EnableFilters =                              '启用过滤器';
    VRS.$$.EnableInfoWindow =                           '启用信息窗口';
    VRS.$$.End =                                        '结束';
    VRS.$$.EndTime =                                    '结束时间';
    VRS.$$.EndsWith =                                   '结束:';
    VRS.$$.Engines =                                    '引擎';
    VRS.$$.EngineType =                                 '引擎类型';
    VRS.$$.Equals =                                     '匹配';
    VRS.$$.EraseBeforeImport =                          'Erase all settings before import';  /** THIS IS NEW! **/
    VRS.$$.ExportSettings =                             'Export Settings';  /** THIS IS NEW! **/
    VRS.$$.Feet =                                       '英尺';
    VRS.$$.FeetAbbreviation =                           '{0} ft';
    VRS.$$.FeetPerMinuteAbbreviation =                  '{0} ft/m';
    VRS.$$.FeetPerSecondAbbreviation =                  '{0} ft/s';
    VRS.$$.FetchPage =                                  '获取';
    VRS.$$.FillOpacity =                                '填充透明度';
    VRS.$$.Filters =                                    '过滤器';
    VRS.$$.FindAllPermutationsOfCallsign =              '查询航班号的所有排列';
    VRS.$$.FirstAltitude =                              '最初高度';
    VRS.$$.FirstHeading =                               '最初航向';
    VRS.$$.FirstFlightLevel =                           '最初飞行高度层';
    VRS.$$.FirstLatitude =                              '最初纬度';
    VRS.$$.FirstLongitude =                             '最初经度';
    VRS.$$.FirstOnGround =                              '最初地面坐标';
    VRS.$$.FirstRegDate =                               '最初注册日期';
    VRS.$$.FirstSpeed =                                 '最初速度';
    VRS.$$.FirstSquawk =                                '最初Squawk';
    VRS.$$.FirstVerticalSpeed =                         '最初垂直速度';
    VRS.$$.FlightDetailShort =                          '详情';
    VRS.$$.FlightLevel =                                '飞行高度层';
    VRS.$$.FlightLevelAbbreviation =                    'FL{0}';
    VRS.$$.FlightLevelAndVerticalSpeed =                'FL & VSI';
    VRS.$$.FlightLevelHeightUnit =                      '飞行高度层高度单位';
    VRS.$$.FlightLevelTransitionAltitude =              '飞行高度层过渡高度';
    VRS.$$.FlightsCount =                               '观测次数';
    VRS.$$.FlightsListShort =                           '航班';
    VRS.$$.FlightSimPage =                              '飞行模拟版页面';
    VRS.$$.FlightSimTitle =                             'Virtual Radar Server';
    VRS.$$.ForcePhoneOff =                              '非移动设备';  // As in "force the page to ignore the fact that this is a smart phone"
    VRS.$$.ForcePhoneOn =                               '移动设备';  // As in "force the page to pretend that this is a smart phone"
    VRS.$$.ForceTabletOff =                             '非平板设备';  // As in "force the page to ignore the fact that this is a tablet PC"
    VRS.$$.ForceTabletOn =                              '平板设备';  // As in "force the page to use the settings for a tablet PC"
    VRS.$$.FromAltitude =                               '起始 {0}';
    VRS.$$.FromToAltitude =                             '{0} 至 {1}';
    VRS.$$.FromToDate =                                 '{0} 至 {1}';
    VRS.$$.FromToFlightLevel =                          '{0} 至 {1}';
    VRS.$$.FromToSpeed =                                '{0} 至 {1}';
    VRS.$$.FromToSquawk =                               '{0} 至 {1}';
    VRS.$$.FurthestFromCurrentLocation =                '距离当前最远';
    VRS.$$.GenericName =                                '通用名';
    VRS.$$.Geometric =                                  '几何';
    VRS.$$.GeometricAltitudeIndicator =                 'GPS';  /** THIS IS NEW! **/  // A ** SHORT ** indication that the reported altitude is geometric (i.e. usually coming from a GPS unit) as opposed to barometric (i.e. coming off one or more pressure sensors).
    VRS.$$.GoogleMapsCouldNotBeLoaded =                 '地图无法加载';
    VRS.$$.GotoCurrentLocation =                        '转到当前坐标';
    VRS.$$.GotoSelectedAircraft =                       '转到已选择航空器';
    VRS.$$.GroundAbbreviation =                         'GND';
    VRS.$$.Ground =                                     '地面';
    VRS.$$.GroundTrack =                                '地面轨迹';
    VRS.$$.GroundVehicle =                              '地面车辆';
    VRS.$$.Gyrocopter =                                 '螺旋桨飞机';
    VRS.$$.HadAlert =                                   '警报';
    VRS.$$.HadEmergency =                               '遇险';
    VRS.$$.HadSPI =                                     'SPI';  // SPI is the name of a pulse in Mode-S, used when ATC has asked for ident from aircraft.
    VRS.$$.Heading =                                    '航向';
    VRS.$$.HeadingType =                                '航向类型';
    VRS.$$.Heights =                                    '高度';
    VRS.$$.Helicopter =                                 '直升飞机';
    VRS.$$.Help =                                       '帮助';
    VRS.$$.HideAircraftNotOnMap =                       '在地图上隐藏航空器';
    VRS.$$.HideEmptyPinTextLines =                      '隐藏空标签行';
    VRS.$$.HideNoPosition =                             '存在坐标';
    VRS.$$.HighContrastMap =                            '对比';  // <-- please try to keep this one short, it appears as a button on the map and there may not be a lot of room
    VRS.$$.Icao =                                       'ICAO代码';
    VRS.$$.Import =                                     'Import';  /** THIS IS NEW! **/
    VRS.$$.ImportFailedTitle =                          'Import Settings Failed';  /** THIS IS NEW! **/
    VRS.$$.ImportFailedBody =                           'Could not import your settings. The reported error was: {0}';  /** THIS IS NEW! **/
    VRS.$$.ImportSettings =                             'Import Settings';  /** THIS IS NEW! **/
    VRS.$$.Index =                                      '索引';
    VRS.$$.IndicatedAirSpeed =                          '指示';
    VRS.$$.IndicatedAirSpeedShort =                     'IAS';  /** THIS IS NEW! **/  // <-- please try to keep this short, an abbreviation if possible
    VRS.$$.Interesting =                                '关注';
    VRS.$$.IntervalSeconds =                            '更新间隔(秒)';
    VRS.$$.IsMilitary =                                 '军航';
    VRS.$$.Jet =                                        '喷气引擎';
    VRS.$$.JustPositions =                              '坐标';
    VRS.$$.KilometreAbbreviation =                      '{0} km';
    VRS.$$.Kilometres =                                 '公里';
    VRS.$$.KilometresPerHour =                          '公里/小时';
    VRS.$$.KilometresPerHourAbbreviation =              '{0} km/h';
    VRS.$$.Knots =                                      '节';
    VRS.$$.KnotsAbbreviation =                          '{0} kts';
    VRS.$$.LandPlane =                                  '陆上飞机';
    VRS.$$.LastAltitude =                               '最后高度';
    VRS.$$.LastFlightLevel =                            '最后飞行高度层';
    VRS.$$.LastHeading =                                '最后航向';
    VRS.$$.LastOnGround =                               '最后地面坐标';
    VRS.$$.LastLatitude =                               '最后纬度';
    VRS.$$.LastLongitude =                              '最后经度';
    VRS.$$.LastSpeed =                                  '最后速度';
    VRS.$$.LastSquawk =                                 '最后Squawk';
    VRS.$$.LastVerticalSpeed =                          '最后垂直速度';
    VRS.$$.Latitude =                                   '纬度';
    VRS.$$.Layout =                                     '布局';
    VRS.$$.Layout1 =                                    '经典';
    VRS.$$.Layout2 =                                    '高详情, 地图在上';
    VRS.$$.Layout3 =                                    '高详情, 地图在下';
    VRS.$$.Layout4 =                                    '高列表, 地图在上';
    VRS.$$.Layout5 =                                    '高列表, 地图在下';
    VRS.$$.Layout6 =                                    '高详情和列表';
    VRS.$$.ListAircraftClass =                          'A/C(适航证) 类型';
    VRS.$$.ListAirportDataThumbnails =                  '缩略图 (来自airport-data.com)';
    VRS.$$.ListAltitude =                               '高度';
    VRS.$$.ListAltitudeType =                           '高度类型';
    VRS.$$.ListAltitudeAndVerticalSpeed =               '高度&垂直速度';
    VRS.$$.ListAverageSignalLevel =                     '平均信号电平';
    VRS.$$.ListBearing =                                '方位';
    VRS.$$.ListCallsign =                               '航班号';
    VRS.$$.ListCivOrMil =                               '民航/军航';
    VRS.$$.ListCofACategory =                           'C/A(适航证) 类别';  // Certificate of airworthiness category
    VRS.$$.ListCofAExpiry =                             'C/A(适航证) 期限';  // Certificate of airworthiness expiry
    VRS.$$.ListCountAdsb =                              'ADS-B 消息';
    VRS.$$.ListCountMessages =                          '消息';
    VRS.$$.ListCountModeS =                             'Mode-S 消息';
    VRS.$$.ListCountPositions =                         '位置消息';
    VRS.$$.ListCountry =                                '国家';
    VRS.$$.ListCurrentRegDate =                         '当前注册日期';  // Date of current registration
    VRS.$$.ListDeRegDate =                              '取消注册日期';  // as in the date it was taken off the register
    VRS.$$.ListDistance =                               '距离';
    VRS.$$.ListDuration =                               '持续时间';
    VRS.$$.ListEndTime =                                '最后消息';  // As in the date and time of the last message.
    VRS.$$.ListEngines =                                '引擎';
    VRS.$$.ListFirstAltitude =                          '最初高度';
    VRS.$$.ListFirstFlightLevel =                       '最初飞行高度层';
    VRS.$$.ListFirstHeading =                           '最初航向';
    VRS.$$.ListFirstLatitude =                          '最初纬度';
    VRS.$$.ListFirstLongitude =                         '最初经度';
    VRS.$$.ListFirstOnGround =                          '最初地面坐标';
    VRS.$$.ListFirstRegDate =                           '最初注册';  // Date of first registration
    VRS.$$.ListFirstSpeed =                             '最初速度';
    VRS.$$.ListFirstSquawk =                            '最初Squawk';
    VRS.$$.ListFirstVerticalSpeed =                     '最初垂直速度';
    VRS.$$.ListFlightLevel =                            '飞行高度层';
    VRS.$$.ListFlightLevelAndVerticalSpeed =            'FL & VSI';
    VRS.$$.ListFlightsCount =                           '观测次数';
    VRS.$$.ListGenericName =                            '通用名';
    VRS.$$.ListHadAlert =                               '警报';
    VRS.$$.ListHadEmergency =                           '遇险';
    VRS.$$.ListHadSPI =                                 'SPI';  // Name of a pulse in Mode-S, may not need translation. Used when ATC has asked for ident from aircraft.
    VRS.$$.ListHeading =                                '航向';
    VRS.$$.ListHeadingType =                            '高度类型';
    VRS.$$.ListIcao =                                   'ICAO代码';
    VRS.$$.ListInteresting =                            '关注';
    VRS.$$.ListLastAltitude =                           '最后高度';
    VRS.$$.ListLastFlightLevel =                        '最后飞行高度层';
    VRS.$$.ListLastHeading =                            '最后航向';
    VRS.$$.ListLastLatitude =                           '最后纬度';
    VRS.$$.ListLastLongitude =                          '最后经度';
    VRS.$$.ListLastOnGround =                           '最后地面坐标';
    VRS.$$.ListLastSpeed =                              '最后速度';
    VRS.$$.ListLastSquawk =                             '最后Squawk';
    VRS.$$.ListLastVerticalSpeed =                      '最后垂直速度';
    VRS.$$.ListLatitude =                               '纬度';
    VRS.$$.ListLongitude =                              '经度';
    VRS.$$.ListNotes =                                  '注意';
    VRS.$$.ListManufacturer =                           '制造商';
    VRS.$$.ListMaxTakeoffWeight =                       '最大起飞重量';
    VRS.$$.ListModel =                                  '机型';
    VRS.$$.ListModelIcao =                              '机型';
    VRS.$$.ListModeSCountry =                           'Mode-S国家';
    VRS.$$.ListModelSilhouette =                        '轮廓';
    VRS.$$.ListModelSilhouetteAndOpFlag =               '标志';
    VRS.$$.ListOperator =                               '航空公司';
    VRS.$$.ListOperatorFlag =                           '标志';
    VRS.$$.ListOperatorIcao =                           '航空公司ICAO代码';
    VRS.$$.ListOwnershipStatus =                        '所属状态';
    VRS.$$.ListPicture =                                '图片';
    VRS.$$.ListPopularName =                            '昵称';
    VRS.$$.ListPreviousId =                             '前一ID';
    VRS.$$.ListReceiver =                               '接收器';
    VRS.$$.ListRegistration =                           '注册号';
    VRS.$$.ListRowNumber =                              '行号';
    VRS.$$.ListRoute =                                  '路线';
    VRS.$$.ListSerialNumber =                           '序号';
    VRS.$$.ListSignalLevel =                            '信号';
    VRS.$$.ListSpecies =                                '类型';
    VRS.$$.ListSpeed =                                  '速度';
    VRS.$$.ListSpeedType =                              '速度类型';
    VRS.$$.ListSquawk =                                 'Squawk';
    VRS.$$.ListStartTime =                              '开始时间';
    VRS.$$.ListStatus =                                 '状态';
    VRS.$$.ListTargetAltitude =                         'A/P 高度';
    VRS.$$.ListTargetHeading =                          'A/P 航向';
    VRS.$$.ListTotalHours =                             '小时总计';
    VRS.$$.ListTransponderType =                        '应答机';
    VRS.$$.ListTransponderTypeFlag =                    '应答机类型标志';
    VRS.$$.ListUserTag =                                '标签';
    VRS.$$.ListVerticalSpeed =                          '垂直速度';
    VRS.$$.ListVerticalSpeedType =                      '垂直速度类型';
    VRS.$$.ListWtc =                                    'WTC';
    VRS.$$.ListYearBuilt =                              '出厂';
    VRS.$$.Longitude =                                  '经度';
    VRS.$$.Manufacturer =                               '制造商';
    VRS.$$.Map =                                        '地图';
    VRS.$$.MaxTakeoffWeight =                           '最大起飞重量';
    VRS.$$.Menu =                                       '菜单';
    VRS.$$.MenuBack =                                   '返回';
    VRS.$$.MessageCount =                               '消息计数';
    VRS.$$.MetreAbbreviation =                          '{0} m';
    VRS.$$.MetrePerSecondAbbreviation =                 '{0} m/sec';
    VRS.$$.MetrePerMinuteAbbreviation =                 '{0} m/min';
    VRS.$$.Metres =                                     '米';
    VRS.$$.MilesPerHour =                               '英里/小时';
    VRS.$$.MilesPerHourAbbreviation =                   '{0} mph';
    VRS.$$.Military =                                   '军航';
    VRS.$$.MobilePage =                                 '移动版页面';
    VRS.$$.MobileReportPage =                           '移动版报告页面';
    VRS.$$.Model =                                      '机型';
    VRS.$$.ModelIcao =                                  '机型代码';
    VRS.$$.ModeS =                                      'Mode-S';  /** THIS IS NEW! **/
    VRS.$$.ModeSCountry =                               'Mode-S国家';
    VRS.$$.MovingMap =                                  '移动地图';
    VRS.$$.MuteOff =                                    '关闭静音';
    VRS.$$.MuteOn =                                     '开启静音';
    VRS.$$.NauticalMileAbbreviation =                   '{0} nmi';
    VRS.$$.NauticalMiles =                              '海里';
    VRS.$$.No =                                         '否';
    VRS.$$.NoLocalStorage =                             '该浏览器不支持本地存储. 您的配置信息将不能保存.\n\n如果您在"私人模式"访问请尝试转换关闭状态. 私人模式在某些浏览器上不能进行本地存储.';
    VRS.$$.None =                                       '无';
    VRS.$$.Notes =                                      '注意';
    VRS.$$.NoSettingsFound =                            '设置未找到';
    VRS.$$.NotBetween =                                 '范围之外';
    VRS.$$.NotContains =                                '不包含';
    VRS.$$.NotEndsWith =                                '不以结尾';
    VRS.$$.NotEquals =                                  '不匹配';
    VRS.$$.NotStartsWith =                              '不以开始';
    VRS.$$.OffRadarAction =                             '当航空器超出范围:';
    VRS.$$.OffRadarActionWait =                         '取消选择航空器';
    VRS.$$.OffRadarActionEnableAutoSelect =             '启用自动选择';
    VRS.$$.OffRadarActionNothing =                      '无';
    VRS.$$.OfPages =                                    ': {0:N0}';  // As in "1 of 10" pages
    VRS.$$.OnlyAircraftOnMap =                          '仅列出可见';
    VRS.$$.OnlyAutoSelected =                           '仅通知自动选择航班详细';
    VRS.$$.Operator =                                   '航空公司';
    VRS.$$.OperatorCode =                               '航空公司代码';
    VRS.$$.OperatorFlag =                               '航空公司标志';
    VRS.$$.Options =                                    '选项';
    VRS.$$.OverwriteExistingSettings =                  'Overwrite existing settings';  /** THIS IS NEW! **/
    VRS.$$.OwnershipStatus =                            '所属状态';
    VRS.$$.PageAircraft =                               '航空器';
    VRS.$$.AircraftDetailShort =                        '详情';
    VRS.$$.PageFirst =                                  '首页';
    VRS.$$.PageGeneral =                                '通用';
    VRS.$$.PageLast =                                   '末页';
    VRS.$$.PageList =                                   '列表';
    VRS.$$.PageListShort =                              '列表';
    VRS.$$.PageMapShort =                               '地图';
    VRS.$$.PageNext =                                   '后一页';
    VRS.$$.PagePrevious =                               '前一页';
    VRS.$$.PaneAircraftDisplay =                        '航空器显示';
    VRS.$$.PaneAircraftTrails =                         '航空器轨迹';
    VRS.$$.PaneAudio =                                  '声音';
    VRS.$$.PaneAutoSelect =                             '自动选择';
    VRS.$$.PaneCurrentLocation =                        '当前坐标';
    VRS.$$.PaneDataFeed =                               '数据提供';
    VRS.$$.PaneDetailSettings =                         '航空器详情';
    VRS.$$.PaneInfoWindow =                             '航空器信息窗口';
    VRS.$$.PaneListSettings =                           '列表设置';
    VRS.$$.PaneManyAircraft =                           '多航空器报告';
    VRS.$$.PanePermanentLink =                          '永久链接';
    VRS.$$.PaneRangeCircles =                           '范围环';
    VRS.$$.PaneReceiverRange =                          '接收器范围';
    VRS.$$.PaneSingleAircraft =                         '单航空器报告';
    VRS.$$.PaneSortAircraftList =                       '排序航空器列表';
    VRS.$$.PaneSortReport =                             '排序报告';
    VRS.$$.PaneUnits =                                  '单位';
    VRS.$$.Pause =                                      '暂停';
    VRS.$$.PinTextNumber =                              '航空器标签行 {0}';
    VRS.$$.PopularName =                                '昵称';
    VRS.$$.PositionAndAltitude =                        '坐标和高度';
    VRS.$$.PositionAndSpeed =                           '坐标和速度';
    VRS.$$.Picture =                                    '图片';
    VRS.$$.PictureOrThumbnails =                        '图片或缩略图';
    VRS.$$.PinTextLines =                               '标签行数';
    VRS.$$.Piston =                                     '活塞引擎';
    VRS.$$.Pixels =                                     '像素';
    VRS.$$.PoweredByVRS =                               'Virtual Radar Server提供技术支持';
    VRS.$$.PreviousId =                                 '前一ID';
    VRS.$$.Quantity =                                   '数量';
    VRS.$$.RadioMast =                                  '天线';
    VRS.$$.RangeCircleEvenColour =                      '偶数环颜色';
    VRS.$$.RangeCircleOddColour =                       '奇数环颜色';
    VRS.$$.RangeCircles =                               '覆盖范围';
    VRS.$$.Receiver =                                   '接收器';
    VRS.$$.ReceiverRange =                              '接收器范围';
    VRS.$$.Refresh =                                    '刷新';
    VRS.$$.Registration =                               '注册号';
    VRS.$$.RegistrationAndIcao =                        '注册号和ICAO代码';
    VRS.$$.Remove =                                     '删除';
    VRS.$$.RemoveAll =                                  '删除所有';
    VRS.$$.ReportCallsignInvalid =                      '航班号报告';
    VRS.$$.ReportCallsignValid =                        '航班号报告: {0}';
    VRS.$$.ReportEmpty =                                '没有找到符合该规则的记录';
    VRS.$$.ReportFreeForm =                             '自定义报告';
    VRS.$$.ReportIcaoInvalid =                          'ICAO代码报告';
    VRS.$$.ReportIcaoValid =                            'ICAO代码报告: {0}';
    VRS.$$.ReportRegistrationInvalid =                  '注册号报告';
    VRS.$$.ReportRegistrationValid =                    '注册号报告: {0}';
    VRS.$$.ReportTodaysFlights =                        '当日航班';
    VRS.$$.ReportYesterdaysFlights =                    '昨日航班';
    VRS.$$.Reports =                                    '报告';
    VRS.$$.ReportsAreDisabled =                         '服务器权限禁止运行报告';
    VRS.$$.Resume =                                     '恢复';
    VRS.$$.Reversing =                                  '反转';
    VRS.$$.ReversingShort =                             'REV';  /** THIS IS NEW! **/
    VRS.$$.Route =                                      '路线';
    VRS.$$.RouteShort =                                 '路线 (短)';
    VRS.$$.RouteFull =                                  '路线 (全)';
    VRS.$$.RouteNotKnown =                              '路线未知';
    VRS.$$.RowNumber =                                  '行号';
    VRS.$$.Rows =                                       '行数';
    VRS.$$.RunReport =                                  '生成报告';
    VRS.$$.SeaPlane =                                   '水上飞机';
    VRS.$$.Select =                                     '选择';
    VRS.$$.SeparateTwoValues =                          ' 和 ';
    VRS.$$.SerialNumber =                               '序号';
    VRS.$$.ServerFetchFailedTitle =                     '获取失败';
    VRS.$$.ServerFetchFailedBody =                      '无法从服务器获取. 错误 "{0}" 状态 "{1}".';
    VRS.$$.ServerFetchTimedOut =                        '请求超时.';
    VRS.$$.ServerReportExceptionBody =                  '在生成报告时服务器遇到异常. 异常 "{0}"';
    VRS.$$.ServerReportExceptionTitle =                 '服务器异常';
    VRS.$$.SetCurrentLocation =                         '设置当前坐标';
    VRS.$$.Settings =                                   '设置';
    VRS.$$.SettingsPage =                               '设置';
    VRS.$$.Shortcuts =                                  '快捷操作';
    VRS.$$.ShowAltitudeStalk =                          '显示高度线';
    VRS.$$.ShowAltitudeType =                           '显示高度类型';
    VRS.$$.ShowCurrentLocation =                        '显示当前坐标';
    VRS.$$.ShowDetail =                                 '显示详情';
    VRS.$$.ShowForAllAircraft =                         '显示所有航空器';
    VRS.$$.ShowEmptyValues =                            '显示空值';
    VRS.$$.ShowForSelectedOnly =                        '仅显示选择的航空器';
    VRS.$$.ShowRangeCircles =                           '显示范围环';
    VRS.$$.ShowShortTrails =                            '显示短轨迹';
    VRS.$$.ShowSpeedType =                              '显示速度类型';
    VRS.$$.ShowTrackType =                              '显示航向类型';
    VRS.$$.ShowUnits =                                  '显示单位';
    VRS.$$.ShowVerticalSpeedType =                      '显示垂直速度类型';
    VRS.$$.ShowVsiInSeconds =                           '显示每秒垂直速度';
    VRS.$$.SignalLevel =                                '信号电平';
    VRS.$$.Silhouette =                                 '轮廓';
    VRS.$$.SilhouetteAndOpFlag =                        '轮廓和标志';
    VRS.$$.SiteTimedOut =                               '在非交互期间该站点暂停. 关闭消息框恢复更新.';
    VRS.$$.SortBy =                                     '排序';
    VRS.$$.Species =                                    '类型';
    VRS.$$.Speed =                                      '速度';
    VRS.$$.SpeedGraph =                                 '速度图';
    VRS.$$.Speeds =                                     '速度';
    VRS.$$.SpeedType =                                  '速度类型';
    VRS.$$.Squawk =                                     'Squawk';
    VRS.$$.Squawk7000 =                                 'No squawk assigned';  /** THIS IS NEW! **/
    VRS.$$.Squawk7500 =                                 'Aircraft hijacking';  /** THIS IS NEW! **/
    VRS.$$.Squawk7600 =                                 'Radio failure';  /** THIS IS NEW! **/
    VRS.$$.Squawk7700 =                                 'General emergency';  /** THIS IS NEW! **/
    VRS.$$.Start =                                      '开始';
    VRS.$$.StartsWith =                                 '开始:';
    VRS.$$.StartTime =                                  '开始时间';
    VRS.$$.Status =                                     '状态';
    VRS.$$.StatuteMileAbbreviation =                    '{0} mi';
    VRS.$$.StatuteMiles =                               '法定英里';
    VRS.$$.StorageEngine =                              '存储引擎';
    VRS.$$.StorageSize =                                '存储大小';
    VRS.$$.StrokeOpacity =                              '路线透明度';
    VRS.$$.SubmitRoute =                                '提交路线';
    VRS.$$.SubmitRouteCorrection =                      '提交路线修正';
    VRS.$$.SuppressAltitudeStalkWhenZoomedOut =         '放大超出范围时限制高度线';
    VRS.$$.TargetAltitude =                             '目标高度';
    VRS.$$.TargetHeading =                              '目标航向';
    VRS.$$.ThenBy =                                     '依据';
    VRS.$$.Tiltwing =                                   '倾斜翼飞机';
    VRS.$$.TimeTracked =                                '跟踪持续';
    VRS.$$.TitleAircraftDetail =                        '航空器详情';
    VRS.$$.TitleAircraftList =                          '航空器列表';
    VRS.$$.TitleFlightDetail =                          '详情';
    VRS.$$.TitleFlightsList =                           '航班';
    VRS.$$.ToAltitude =                                 '至 {0}';
    VRS.$$.TitleSiteTimedOut =                          '超时';
    VRS.$$.TotalHours =                                 '小时总计';
    VRS.$$.TrackingCountAircraft =                      '跟踪 {0:N0} 航空器';
    VRS.$$.TrackingCountAircraftOutOf =                 '跟踪 {0:N0} 航空器 (越界 {1:N0})';
    VRS.$$.TransponderType =                            '应答机';
    VRS.$$.TransponderTypeFlag =                        '应答机标志';
    VRS.$$.TrueAirSpeed =                               '真实';
    VRS.$$.TrueAirSpeedShort =                          'TAS';  /** THIS IS NEW! **/  // Keep this one short, an abbreviation if possible.
    VRS.$$.TrueHeading =                                '真实航向';
    VRS.$$.TrueHeadingShort =                           '真实';
    VRS.$$.Turbo =                                      '涡轮增压引擎';
    VRS.$$.Unknown =                                    '未知';
    VRS.$$.UseBrowserLocation =                         '使用GPS定位';
    VRS.$$.UseRelativeDates =                           '使用相对日期';
    VRS.$$.UserTag =                                    '使用标签';
    VRS.$$.VerticalSpeed =                              '垂直速度';
    VRS.$$.VerticalSpeedType =                          '垂直速度类型';
    VRS.$$.VirtualRadar =                               'Virtual Radar Server';
    VRS.$$.Volume25 =                                   '音量 25%';
    VRS.$$.Volume50 =                                   '音量 50%';
    VRS.$$.Volume75 =                                   '音量 75%';
    VRS.$$.Volume100 =                                  '音量 100%';
    VRS.$$.VrsVersion =                                 '版本 {0}';
    VRS.$$.WakeTurbulenceCategory =                     '尾流';
    VRS.$$.Warning =                                    '警告';
    VRS.$$.WorkingInOfflineMode =                       '脱机工作';
    VRS.$$.WtcLight =                                   '轻微';
    VRS.$$.WtcMedium =                                  '中等';
    VRS.$$.WtcHeavy =                                   '严重';
    VRS.$$.YearBuilt =                                  '出厂年份';
    VRS.$$.Yes =                                        '是';

    // Date picker text
    VRS.$$.DateClose =                                  '完成';  // Keep this short
    VRS.$$.DateCurrent =                                '今天';  // Keep this short
    VRS.$$.DateNext =                                   '后一天';  // Keep this short
    VRS.$$.DatePrevious =                               '前一天';  // Keep this short
    VRS.$$.DateWeekAbbr =                               '周';  // Keep this very short
    VRS.$$.DateYearSuffix =                             '年';  // This is displayed after the year
    // If your language has a different month format when days preceed months, and the date picker
    // should be using that month format, then set this to true. Otherwise leave at false.
    VRS.$$.DateUseGenetiveMonths =                      false;

    // Text-to-speech formatting
    VRS.$$.SayCallsign =                                '航班号 {0}.';
    VRS.$$.SayHyphen =                                  '杠';
    VRS.$$.SayIcao =                                    'ICOA代码 {0}.';
    VRS.$$.SayModelIcao =                               '机型 {0}.';
    VRS.$$.SayOperator =                                '航空公司 {0}.';
    VRS.$$.SayRegistration =                            '注册号 {0}.';
    VRS.$$.SayRouteNotKnown =                           '路线未知.';
    VRS.$$.SayFromTo =                                  '行程从 {0} 至 {1}.';
    VRS.$$.SayFromToVia =                               '行程从 {0} 经过 {1} 至 {2}.';

    VRS.$$.SayAlpha =                                   'alfuh';
    VRS.$$.SayBravo =                                   'bravo';
    VRS.$$.SayCharlie =                                 'charlie';
    VRS.$$.SayDelta =                                   'delta';
    VRS.$$.SayEcho =                                    'echo';
    VRS.$$.SayFoxtrot =                                 'foxed-rot';
    VRS.$$.SayGolf =                                    'golf';
    VRS.$$.SayHotel =                                   'hotel';
    VRS.$$.SayIndia =                                   'india';
    VRS.$$.SayJuliet =                                  'juliet';
    VRS.$$.SayKilo =                                    'key-low';
    VRS.$$.SayLima =                                    'leamah';
    VRS.$$.SayMike =                                    'mike';
    VRS.$$.SayNovember =                                'november';
    VRS.$$.SayOscar =                                   'oscar';
    VRS.$$.SayPapa =                                    'papa';
    VRS.$$.SayQuebec =                                  'quebec';
    VRS.$$.SayRomeo =                                   'romeo';
    VRS.$$.SaySierra =                                  'sierra';
    VRS.$$.SayTango =                                   'tango';
    VRS.$$.SayUniform =                                 'uniform';
    VRS.$$.SayVictor =                                  'victor';
    VRS.$$.SayWhiskey =                                 'whiskey';
    VRS.$$.SayXRay =                                    'x-ray';
    VRS.$$.SayYankee =                                  'yankee';
    VRS.$$.SayZulu =                                    'zulu';


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
