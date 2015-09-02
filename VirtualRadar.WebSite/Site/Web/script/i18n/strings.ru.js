// Copyright © 2013 onwards, Andrew Whewell and Sergey Serov.
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

(function(VRS, $, undefined)
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

    VRS.$$.Add =                                        'Добавить';
    VRS.$$.AddCondition =                               'Добавить условие';
    VRS.$$.AddCriteria =                                'Добавить критерий';
    VRS.$$.AddFilter =                                  'Добавить фильтр';
    VRS.$$.ADSB =                                       'ADS-B';  /** THIS IS NEW! **/
    VRS.$$.ADSB0 =                                      'ADS-B v0';  /** THIS IS NEW! **/
    VRS.$$.ADSB1 =                                      'ADS-B v1';  /** THIS IS NEW! **/
    VRS.$$.ADSB2 =                                      'ADS-B v2';  /** THIS IS NEW! **/
    VRS.$$.AircraftNotTransmittingCallsign =            'Самолет не передает свой позывной';
    VRS.$$.AircraftClass =                              'Класс самолета';
    VRS.$$.Airport =                                    'Аэропорт';
    VRS.$$.AirportDataThumbnails =                      'Миниатюры (airport-data.com)';
    VRS.$$.AllAltitudes =                               'Для всех высот';
    VRS.$$.AllRows =                                    'Все строки';
    VRS.$$.Altitude =                                   'Высота';
    VRS.$$.AltitudeAndSpeedGraph =                      'График Высота/Скорость';
    VRS.$$.AltitudeAndVerticalSpeed =                   'Высота и верт.скорость';
    VRS.$$.AltitudeGraph =                              'График высоты';
    VRS.$$.AltitudeType =                               'Вид высоты';
    VRS.$$.AllAircraft =                                'Все';
    VRS.$$.Amphibian =                                  'Амфибия';
    VRS.$$.AnnounceSelected =                           'Сообщить детали для выбранного самолета (англ.)';
    VRS.$$.Ascending =                                  'по возрастанию';
    VRS.$$.AutoSelectAircraft =                         'Включить автоматический выбор';
    VRS.$$.AverageSignalLevel =                         'Средний уровень сигнала';
    VRS.$$.Barometric =                                 'Барометрическая';
    VRS.$$.Bearing =                                    'Азимут';
    VRS.$$.Between =                                    'в диапазоне от';
    VRS.$$.Callsign =                                   'Позывной';
    VRS.$$.CallsignAndShortRoute =                      'Позывной/Маршрут';
    VRS.$$.CallsignMayNotBeCorrect =                    'Позывной может быть ошибочным';
    VRS.$$.CentreOnSelectedAircraft =                   'Показать на карте';
    VRS.$$.Civil =                                      'Гражданский';
    VRS.$$.CivilOrMilitary =                            'Гражданский/Военный';
    VRS.$$.ClosestToCurrentLocation =                   'Ближайший';
    VRS.$$.CofACategory =                               'Категория сертификата летной годности';  // certificate of airworthiness category
    VRS.$$.CofAExpiry =                                 'Окончание срока действия сертификата летной годности';  // certificate of airworthiness expiry
    VRS.$$.Columns =                                    'Столбцы';
    VRS.$$.Contains =                                   'содержит';
    VRS.$$.CountAdsb =                                  'Счетчик сообщений ADS-B';
    VRS.$$.Country =                                    'Страна регистрации';
    VRS.$$.CountModeS =                                 'Счетчик сообщений Mode-S';
    VRS.$$.CountPositions =                             'Количество сообщений о местоположении';
    VRS.$$.Criteria =                                   'Критерии';
    VRS.$$.CurrentLocationInstruction =                 'Отметьте "Установить текущую позицию на карте" и перетащите маркер';
    VRS.$$.CurrentRegDate =                             'Дата действующей регистрации';
    VRS.$$.Date =                                       'Дата';
    VRS.$$.DateTimeShort =                              '{0} {1}';  // Where "{0}" is a date, e.g. 10/10/2013; and "{1}" is a time, e.g. 17:41:32.
    VRS.$$.DefaultSetting =                             '< По умолчанию >';
    VRS.$$.DegreesAbbreviation =                        '{0}°';
    VRS.$$.DeRegDate =                                  'Дата исключения из реестра';
    VRS.$$.DesktopPage =                                'Для настольного компьютера';
    VRS.$$.DesktopReportPage =                          'Отчет для настольного компьютера';
    VRS.$$.DetailItems =                                'Подробные данные о самолете';
    VRS.$$.DetailPanel =                                'Формуляр';
    VRS.$$.DisableAutoSelect =                          'Отключить автоматический выбор';
    VRS.$$.Distance =                                   'Расстояние';
    VRS.$$.Distances =                                  'Расстояние';
    VRS.$$.DoNotImportAutoSelect =                      'Не импортировать настройки автоматического выбора';
    VRS.$$.DoNotImportCurrentLocation =                 'Не импортировать текущее местоположение';
    VRS.$$.DoNotImportRequestFeedId =                   'Не импортировать запрос ID канала';
    VRS.$$.DoNotImportLanguageSettings =                'Не импортировать настройки языка';
    VRS.$$.DoNotImportSplitters =                       'Не импортировать сплиттеры';
    VRS.$$.DoNotShow =                                  'Не показывать';
    VRS.$$.Duration =                                   'Длительность';
    VRS.$$.Electric =                                   'Электрический';
    VRS.$$.EnableAutoSelect =                           'Включить автоматический выбор';
    VRS.$$.EnableFilters =                              'Включить фильтры';
    VRS.$$.EnableInfoWindow =                           'Включить информационное окно';
    VRS.$$.End =                                        'Стоп';
    VRS.$$.EndTime =                                    'Время окончания';
    VRS.$$.EndsWith =                                   'заканчивается на';
    VRS.$$.Engines =                                    'Количество и тип двигателей';
    VRS.$$.EngineType =                                 'Тип двигателя';
    VRS.$$.Equals =                                     'совпадает с';
    VRS.$$.EraseBeforeImport =                          'Удалить все настройки перед импортом';
    VRS.$$.ExportSettings =                             'Экспортировать настройки';
    VRS.$$.Feet =                                       'Футы';
    VRS.$$.FeetAbbreviation =                           '{0} фт';
    VRS.$$.FeetPerMinuteAbbreviation =                  '{0} фт/мин';
    VRS.$$.FeetPerSecondAbbreviation =                  '{0} фт/с';
    VRS.$$.FetchPage =                                  'Обновить';
    VRS.$$.FillOpacity =                                'Непрозрачность заливки';
    VRS.$$.Filters =                                    'Фильтры';
    VRS.$$.FindAllPermutationsOfCallsign =              'Найти все изменения позывного';
    VRS.$$.First =                                      'Начальный';
    VRS.$$.FirstAltitude =                              'Начальная высота';
    VRS.$$.FirstHeading =                               'Начальный курс';
    VRS.$$.FirstFlightLevel =                           'Начальный FL';
    VRS.$$.FirstLatitude =                              'Начальная широта';
    VRS.$$.FirstLongitude =                             'Начальная долгота';
    VRS.$$.FirstOnGround =                              'Начальный On Ground';
    VRS.$$.FirstRegDate =                               'Дата первой регистрации';
    VRS.$$.FirstSpeed =                                 'Начальная скорость';
    VRS.$$.FirstSquawk =                                'Начальный сквок';
    VRS.$$.FirstVerticalSpeed =                         'Начальная верт.скорость';
    VRS.$$.FlightDetailShort =                          'Формуляр';
    VRS.$$.FlightLevel =                                'Эшелон';
    VRS.$$.FlightLevelAbbreviation =                    'FL{0}';
    VRS.$$.FlightLevelAndVerticalSpeed =                'FL & Верт.скор.';
    VRS.$$.FlightLevelHeightUnit =                      'Единица измерения номера эшелона, в сотнях';
    VRS.$$.FlightLevelTransitionAltitude =              'Высота эшелона перехода';
    VRS.$$.FlightsCount =                               'Количество наблюдений';
    VRS.$$.FlightsListShort =                           'Полеты';
    VRS.$$.FlightSimPage =                              'Flight Sim Страница';
    VRS.$$.FlightSimTitle =                             'Virtual Radar - FSX';
    VRS.$$.ForcePhoneOff =                              'Не учитывать смартфон';  // As in "force the page to ignore the fact that this is a smart phone"
    VRS.$$.ForcePhoneOn =                               'Учитывать смартфон';  // As in "force the page to pretend that this is a smart phone"
    VRS.$$.ForceTabletOff =                             'Не учитывать планшет';  // As in "force the page to ignore the fact that this is a tablet PC"
    VRS.$$.ForceTabletOn =                              'Учитывать планшет';  // As in "force the page to use the settings for a tablet PC"
    VRS.$$.FromAltitude =                               'От {0}';
    VRS.$$.FromToAltitude =                             '{0} до {1}';
    VRS.$$.FromToDate =                                 '{0} до {1}';
    VRS.$$.FromToFlightLevel =                          '{0} до {1}';
    VRS.$$.FromToSpeed =                                '{0} до {1}';
    VRS.$$.FromToSquawk =                               '{0} до {1}';
    VRS.$$.FurthestFromCurrentLocation =                'Наиболее удаленный';
    VRS.$$.GenericName =                                'Общее название';
    VRS.$$.Geometric =                                  'Истинная';
    VRS.$$.GeometricAltitudeIndicator =                 'GPS';  /** THIS IS NEW! **/  // A ** SHORT ** indication that the reported altitude is geometric (i.e. usually coming from a GPS unit) as opposed to barometric (i.e. coming off one or more pressure sensors).
    VRS.$$.GoogleMapsCouldNotBeLoaded =                 'Карта Google не может быть загружена';
    VRS.$$.GotoCurrentLocation =                        'Приемник в центре';
    VRS.$$.GotoSelectedAircraft =                       'Самолет в центре';
    VRS.$$.GroundAbbreviation =                         'GND';
    VRS.$$.Ground =                                     'Путевая';
    VRS.$$.GroundTrack =                                'Истинный';
    VRS.$$.GroundVehicle =                              'Наземный транспорт';
    VRS.$$.Gyrocopter =                                 'Автожир';
    VRS.$$.HadAlert =                                   'Изменение сквока';
    VRS.$$.HadEmergency =                               'Сигнал бедствия';
    VRS.$$.HadSPI =                                     'Запрос опознавания SPI';  // SPI is the name of a pulse in Mode-S, used when ATC has asked for ident from aircraft.
    VRS.$$.Heading =                                    'Курс';
    VRS.$$.HeadingType =                                'Тип курса';
    VRS.$$.Heights =                                    'Высота';
    VRS.$$.Helicopter =                                 'Вертолет';
    VRS.$$.Help =                                       'Помощь';
    VRS.$$.HideAircraftNotOnMap =                       'Скрыть самолеты вне окна карты';
    VRS.$$.HideEmptyPinTextLines =                      'Скрыть пустые строки у меток';
    VRS.$$.HideNoPosition =                             'Скрыть самолеты, не передающие позицию';
    VRS.$$.HighContrastMap =                            'Контраст';  // <-- please try to keep this one short, it appears as a button on the map and there may not be a lot of room
    VRS.$$.Icao =                                       'ICAO адрес';
    VRS.$$.Import =                                     'Импорт';
    VRS.$$.ImportFailedTitle =                          'Ошибка импортирования настроек';
    VRS.$$.ImportFailedBody =                           'Не удалось импортировать настройки. Ошибка: {0}';
    VRS.$$.ImportSettings =                             'Импорт настроек';
    VRS.$$.Index =                                      'Индекс';
    VRS.$$.IndicatedAirSpeed =                          'Приборная';
    VRS.$$.IndicatedAirSpeedShort =                     'IAS';  /** THIS IS NEW! **/  // <-- please try to keep this short, an abbreviation if possible
    VRS.$$.Interesting =                                'Интересующий';
    VRS.$$.IntervalSeconds =                            'Период обновления (с)';
    VRS.$$.IsMilitary =                                 'Военный';
    VRS.$$.Jet =                                        'Газотурбинный';
    VRS.$$.JustPositions =                              'Монохромная';
    VRS.$$.KilometreAbbreviation =                      '{0} км';
    VRS.$$.Kilometres =                                 'Километры';
    VRS.$$.KilometresPerHour =                          'Километры/Час';
    VRS.$$.KilometresPerHourAbbreviation =              '{0} км/ч';
    VRS.$$.Knots =                                      'Узлы';
    VRS.$$.KnotsAbbreviation =                          '{0} уз.';
    VRS.$$.LandPlane =                                  'Сухопутный';
    VRS.$$.Last =                                       'Конечный';
    VRS.$$.LastAltitude =                               'Конечная высота';
    VRS.$$.LastFlightLevel =                            'Конечный FL';
    VRS.$$.LastHeading =                                'Конечный курс';
    VRS.$$.LastOnGround =                               'Конечный On Ground';
    VRS.$$.LastLatitude =                               'Конечная широта';
    VRS.$$.LastLongitude =                              'Конечная долгота';
    VRS.$$.LastSpeed =                                  'Конечная скорость';
    VRS.$$.LastSquawk =                                 'Конечный сквок';
    VRS.$$.LastVerticalSpeed =                          'Конечная верт.скорость';
    VRS.$$.Latitude =                                   'Широта';
    VRS.$$.Layout =                                     'Компоновка экрана';
    VRS.$$.Layout1 =                                    'Классическая';
    VRS.$$.Layout2 =                                    'Формуляр вертикально, карта вверху';
    VRS.$$.Layout3 =                                    'Формуляр вертикально, карта внизу';
    VRS.$$.Layout4 =                                    'Список вертикально, карта вверху';
    VRS.$$.Layout5 =                                    'Список вертикально, карта внизу';
    VRS.$$.Layout6 =                                    'Формуляр и список вертикально';
    VRS.$$.ListAircraftClass =                          'Класс';
    VRS.$$.ListAirportDataThumbnails =                  'Миниатюры (airport-data.com)';
    VRS.$$.ListAltitude =                               'Высота';
    VRS.$$.ListAltitudeType =                           'Вид высоты';
    VRS.$$.ListAltitudeAndVerticalSpeed =               'Выс./в.скор.';
    VRS.$$.ListAverageSignalLevel =                     'Ср.сигн.';
    VRS.$$.ListBearing =                                'Азимут';
    VRS.$$.ListCallsign =                               'Позывн.';
    VRS.$$.ListCivOrMil =                               'Назначение';
    VRS.$$.ListCofACategory =                           'Кат. СЛГ';  // Certificate of airworthiness category
    VRS.$$.ListCofAExpiry =                             'Срок СЛГ';  // Certificate of airworthiness expiry
    VRS.$$.ListCountAdsb =                              'ADS-B';
    VRS.$$.ListCountMessages =                          'Сообщ.';
    VRS.$$.ListCountModeS =                             'Mode-S';
    VRS.$$.ListCountPositions =                         'С коорд.';
    VRS.$$.ListCountry =                                'Страна рег.';
    VRS.$$.ListCurrentRegDate =                         'Регистр.';  // Date of current registration
    VRS.$$.ListDeRegDate =                              'Исключ.';  // as in the date it was taken off the register
    VRS.$$.ListDistance =                               'Расстояние';
    VRS.$$.ListDuration =                               'Длит.набл.';
    VRS.$$.ListEndTime =                                'Крайнее';  // As in the date and time of the last message.
    VRS.$$.ListEngines =                                'Колич./Тип дв.';
    VRS.$$.ListFirstAltitude =                          'От выс.';
    VRS.$$.ListFirstFlightLevel =                       'С FL';
    VRS.$$.ListFirstHeading =                           'С курсом';
    VRS.$$.ListFirstLatitude =                          'От шир.';
    VRS.$$.ListFirstLongitude =                         'От долг.';
    VRS.$$.ListFirstOnGround =                          'На земле';
    VRS.$$.ListFirstRegDate =                           'Перв.рег.';  // Date of first registration
    VRS.$$.ListFirstSpeed =                             'С скор.';
    VRS.$$.ListFirstSquawk =                            'Со cквоком';
    VRS.$$.ListFirstVerticalSpeed =                     'С верт.скор.';
    VRS.$$.ListFlightLevel =                            'Эшелон';
    VRS.$$.ListFlightLevelAndVerticalSpeed =            'FL & Верт.скор.';
    VRS.$$.ListFlightsCount =                           'Набл.';
    VRS.$$.ListGenericName =                            'Общ.назв.';
    VRS.$$.ListHadAlert =                               'Тревожная сигнализация';
    VRS.$$.ListHadEmergency =                           'Аварийная обстановка';
    VRS.$$.ListHadSPI =                                 'Условие SPI';  // Name of a pulse in Mode-S, may not need translation. Used when ATC has asked for ident from aircraft.
    VRS.$$.ListHeading =                                'Курс';
    VRS.$$.ListHeadingType =                            'Тип курса';
    VRS.$$.ListIcao =                                   'ICAO';
    VRS.$$.ListInteresting =                            'Интерес';
    VRS.$$.ListLastAltitude =                           'До выс.';
    VRS.$$.ListLastFlightLevel =                        'До FL';
    VRS.$$.ListLastHeading =                            'На курс';
    VRS.$$.ListLastLatitude =                           'До шир.';
    VRS.$$.ListLastLongitude =                          'До длг.';
    VRS.$$.ListLastOnGround =                           'До GND';
    VRS.$$.ListLastSpeed =                              'До скор.';
    VRS.$$.ListLastSquawk =                             'До cквока';
    VRS.$$.ListLastVerticalSpeed =                      'До верт.выс.';
    VRS.$$.ListLatitude =                               'Широта';
    VRS.$$.ListLongitude =                              'Долгота';
    VRS.$$.ListNotes =                                  'Прим.';
    VRS.$$.ListManufacturer =                           'Изготов.';
    VRS.$$.ListMaxTakeoffWeight =                       'Макс.взлт.масса';
    VRS.$$.ListMlat =                                   'MLAT';  /** THIS IS NEW! **/  // Abbreviation of Multilateration
    VRS.$$.ListModel =                                  'Модель самолета';
    VRS.$$.ListModelIcao =                              'Тип';
    VRS.$$.ListModeSCountry =                           'Страна';
    VRS.$$.ListModelSilhouette =                        'Силуэт';
    VRS.$$.ListModelSilhouetteAndOpFlag =               'Силуэт/Лого';
    VRS.$$.ListOperator =                               'Оператор';
    VRS.$$.ListOperatorFlag =                           'Логотип';
    VRS.$$.ListOperatorIcao =                           'Код а/к';
    VRS.$$.ListOwnershipStatus =                        'Стат.влад.';
    VRS.$$.ListPicture =                                'Изображ.';
    VRS.$$.ListPopularName =                            'Прозвище';
    VRS.$$.ListPreviousId =                             'Пред.№';
    VRS.$$.ListReceiver =                               'Приемник';
    VRS.$$.ListRegistration =                           'Регистр.';
    VRS.$$.ListRowNumber =                              '';
    VRS.$$.ListRoute =                                  'Маршрут';
    VRS.$$.ListSerialNumber =                           'Зав.№';
    VRS.$$.ListSignalLevel =                            'Мгн.сигн.';
    VRS.$$.ListSpecies =                                'Тип';
    VRS.$$.ListSpeed =                                  'Скор.';
    VRS.$$.ListSpeedType =                              'Тип скор.';
    VRS.$$.ListSquawk =                                 'Сквок';
    VRS.$$.ListStartTime =                              'Перв.сообщ.';
    VRS.$$.ListStatus =                                 'Статус';
    VRS.$$.ListTargetAltitude =                         'Выс. AП';
    VRS.$$.ListTargetHeading =                          'Курс АП';
    VRS.$$.ListTisb =                                   'TIS-B';  /** THIS IS NEW! **/
    VRS.$$.ListTotalHours =                             'Общ.налет';
    VRS.$$.ListTransponderType =                        'Трансп.';
    VRS.$$.ListTransponderTypeFlag =                    '';  /** THIS IS NEW! **/
    VRS.$$.ListUserTag =                                'Тэг';
    VRS.$$.ListVerticalSpeed =                          'В/скор.';
    VRS.$$.ListVerticalSpeedType =                      'Тип в/скор.';
    VRS.$$.ListWtc =                                    'МВМ';
    VRS.$$.ListYearBuilt =                              'Дата произв.';
    VRS.$$.Longitude =                                  'Долгота';
    VRS.$$.Manufacturer =                               'Изготовитель';
    VRS.$$.Map =                                        'Карта';
    VRS.$$.MaxTakeoffWeight =                           'Макс.взлт.масса';
    VRS.$$.Menu =                                       'Меню';
    VRS.$$.MenuBack =                                   'назад';
    VRS.$$.MessageCount =                               'Количество сообщений';
    VRS.$$.MetreAbbreviation =                          '{0} м';
    VRS.$$.MetrePerSecondAbbreviation =                 '{0} м/с';
    VRS.$$.MetrePerMinuteAbbreviation =                 '{0} м/мин';
    VRS.$$.Metres =                                     'Метры';
    VRS.$$.MilesPerHour =                               'Мили/час';
    VRS.$$.MilesPerHourAbbreviation =                   '{0} ми/ч';
    VRS.$$.Military =                                   'Военный';
    VRS.$$.Mlat =                                       'MLAT';  /** THIS IS NEW! **/  // An abbreviation of Multilateration
    VRS.$$.MobilePage =                                 'Для мобильного компьютера';
    VRS.$$.MobileReportPage =                           'Страница мобильного отчета';
    VRS.$$.Model =                                      'Модель';
    VRS.$$.ModelIcao =                                  'Код типа';
    VRS.$$.ModeS =                                      'Mode S';
    VRS.$$.ModeSCountry =                               'Страна';
    VRS.$$.MovingMap =                                  'Перемещение карты';
    VRS.$$.MuteOff =                                    'Выкл.звук';
    VRS.$$.MuteOn =                                     'Вкл.звук';
    VRS.$$.NauticalMileAbbreviation =                   '{0} м.миль';
    VRS.$$.NauticalMiles =                              'Морские мили';
    VRS.$$.Neither =                                    'Никакой';
    VRS.$$.No =                                         'Нет';
    VRS.$$.NoLocalStorage =                             'Ваш браузер не поддерживает локальное сохранение. Параметры конфигурации не будут сохранены.\n\nЕсли вы просматриваете в "Private Mode", то попробуйте переключиться. "Private Mode" отключает локальное сохранение в некоторых браузерах';
    VRS.$$.None =                                       'Нет';
    VRS.$$.Notes =                                      'Прим.';
    VRS.$$.NoSettingsFound =                            'Параметр не найден';
    VRS.$$.NotBetween =                                 'вне диапазона от';
    VRS.$$.NotContains =                                'не содержит';
    VRS.$$.NotEndsWith =                                'не заканчивается на';
    VRS.$$.NotEquals =                                  'не совпадает с';
    VRS.$$.NotStartsWith =                              'не начинается с';
    VRS.$$.OffRadarAction =                             'Когда самолет выходит из зоны:';
    VRS.$$.OffRadarActionWait =                         'Снять выделение';
    VRS.$$.OffRadarActionEnableAutoSelect =             'Включить автоматический выбор';
    VRS.$$.OffRadarActionNothing =                      'Ничего не делать';
    VRS.$$.OfPages =                                    'по {0:N0}';  // As in "1 of 10" pages
    VRS.$$.OnlyAircraftOnMap =                          'Только в окне карты';
    VRS.$$.OnlyAutoSelected =                           'Только для авто выбранных самолетов';
    VRS.$$.OnlyUsePre22Icons =                          'Использовать старый стиль метки';
    VRS.$$.Operator =                                   'Оператор';
    VRS.$$.OperatorCode =                               'Код авиакомпании';
    VRS.$$.OperatorFlag =                               'Логотип авиакомпании';
    VRS.$$.Options =                                    'Настройки';
    VRS.$$.OverwriteExistingSettings =                  'Заменить существующие настройки';
    VRS.$$.OwnershipStatus =                            'Статус владельца';
    VRS.$$.PageAircraft =                               'Самолет';
    VRS.$$.AircraftDetailShort =                        'Формуляр';
    VRS.$$.PageFirst =                                  'Первая';
    VRS.$$.PageGeneral =                                'Общие';
    VRS.$$.PageLast =                                   'Последняя';
    VRS.$$.PageList =                                   'Список';
    VRS.$$.PageListShort =                              'Список';
    VRS.$$.PageMapShort =                               'Карта';
    VRS.$$.PageNext =                                   'Следующая';
    VRS.$$.PagePrevious =                               'Предыдущая';
    VRS.$$.PaneAircraftDisplay =                        'Отображение метки';
    VRS.$$.PaneAircraftTrails =                         'Траектория полета на карте';
    VRS.$$.PaneAudio =                                  'Звук';
    VRS.$$.PaneAutoSelect =                             'Автоматический выбор';
    VRS.$$.PaneCurrentLocation =                        'Текущее местоположение';
    VRS.$$.PaneDataFeed =                               'Источник данных';
    VRS.$$.PaneDetailSettings =                         'Сведения в формуляре';
    VRS.$$.PaneInfoWindow =                             'Информационное окно самолета';
    VRS.$$.PaneListSettings =                           'Отображаемые сведения';
    VRS.$$.PaneManyAircraft =                           'Общий отчет по самолетам';
    VRS.$$.PanePermanentLink =                          'Постоянная ссылка';
    VRS.$$.PaneRangeCircles =                           'Круговая шкала дальности';
    VRS.$$.PaneReceiverRange =                          'Диаграмма зоны приема';
    VRS.$$.PaneSingleAircraft =                         'Отчет по конкретному самолету';
    VRS.$$.PaneSortAircraftList =                       'Сортировка самолетов';
    VRS.$$.PaneSortReport =                             'Сортировка отчетов';
    VRS.$$.PaneUnits =                                  'Единицы измерения';
    VRS.$$.Pause =                                      'Пауза';
    VRS.$$.PinTextNumber =                              '{0}-я строка метки';
    VRS.$$.PopularName =                                'Популярное название';
    VRS.$$.PositionAndAltitude =                        'Цветовая градация высоты';
    VRS.$$.PositionAndSpeed =                           'Цветовая градация скорости';
    VRS.$$.Picture =                                    'Изображение';
    VRS.$$.PictureOrThumbnails =                        'Изображения или миниатюры';
    VRS.$$.PinTextLines =                               'Количество строк у меток';
    VRS.$$.Piston =                                     'Поршневой';
    VRS.$$.Pixels =                                     'пикс.';
    VRS.$$.PoweredByVRS =                               'Создано в Virtual Radar Server';
    VRS.$$.PreviousId =                                 'Предыдущий №';
    VRS.$$.Quantity =                                   'Количество';
    VRS.$$.RadioMast =                                  'Радиомачта';
    VRS.$$.RangeCircleEvenColour =                      'Цвет и толщина четных';
    VRS.$$.RangeCircleOddColour =                       'Цвет и толщина нечетных';
    VRS.$$.RangeCircles =                               'Круговая шкала дальности';
    VRS.$$.Receiver =                                   'Приемник';
    VRS.$$.ReceiverRange =                              'Диаграмма зоны приема';
    VRS.$$.Refresh =                                    'Обновить';
    VRS.$$.Registration =                               'Регистр. номер';
    VRS.$$.RegistrationAndIcao =                        'Рег./ICAO';
    VRS.$$.Remove =                                     'Удалить';
    VRS.$$.RemoveAll =                                  'Удалить все';
    VRS.$$.ReportCallsignInvalid =                      'Отчет по позывным';
    VRS.$$.ReportCallsignValid =                        'Отчет по позывному {0}';
    VRS.$$.ReportEmpty =                                'Не было найдено записей для заданного критерия';
    VRS.$$.ReportFreeForm =                             'Произвольный отчет';
    VRS.$$.ReportIcaoInvalid =                          'Отчет по ICAO адресам';
    VRS.$$.ReportIcaoValid =                            'Отчет по ICAO адресу {0}';
    VRS.$$.ReportRegistrationInvalid =                  'Отчет по рег. номеру';
    VRS.$$.ReportRegistrationValid =                    'Отчет по рег. номеру {0}';
    VRS.$$.ReportTodaysFlights =                        'За текущие сутки';
    VRS.$$.ReportYesterdaysFlights =                    'За предыдущие сутки';
    VRS.$$.Reports =                                    'Отчеты';
    VRS.$$.ReportsAreDisabled =                         'Настройки сервера запрещают создание отчетов';
    VRS.$$.Resume =                                     'Возобновить';
    VRS.$$.Reversing =                                  'Реверсивный';
    VRS.$$.ReversingShort =                             'РЕВ';
    VRS.$$.Route =                                      'Маршрут';
    VRS.$$.RouteShort =                                 'Маршрут (кратко)';
    VRS.$$.RouteFull =                                  'Маршрут (подробно)';
    VRS.$$.RouteNotKnown =                              'Маршрут не известен';
    VRS.$$.RowNumber =                                  'Номер строки';
    VRS.$$.Rows =                                       'Количество строк';
    VRS.$$.RunReport =                                  'Сформировать отчет';
    VRS.$$.SeaPlane =                                   'Гидросамолет';
    VRS.$$.Select =                                     'Выбор';
    VRS.$$.SeparateTwoValues =                          ' до: ';
    VRS.$$.SerialNumber =                               'Заводской номер';
    VRS.$$.ServerFetchFailedTitle =                     'Ошибка запроса';
    VRS.$$.ServerFetchFailedBody =                      'Не удалось получить с сервера. Ошибка "{0}" со статусом "{1}".';
    VRS.$$.ServerFetchTimedOut =                        'Истекло время запроса.';
    VRS.$$.ServerReportExceptionBody =                  'На сервере получено исключение при формировании отчета. Исключение "{0}"';
    VRS.$$.ServerReportExceptionTitle =                 'Получено исключение сервера';
    VRS.$$.SetCurrentLocation =                         'Установить текущую позицию на карте';
    VRS.$$.Settings =                                   'Настройки';
    VRS.$$.SettingsPage =                               'Настройки';
    VRS.$$.Shortcuts =                                  'Ярлыки выбора';
    VRS.$$.ShowAltitudeStalk =                          'Показывать линию высоты';
    VRS.$$.ShowAltitudeType =                           'Показывать вид высоты';
    VRS.$$.ShowCurrentLocation =                        'Показывать текущее местоположение';
    VRS.$$.ShowDetail =                                 'Показать формуляр';
    VRS.$$.ShowForAllAircraft =                         'Для всех';
    VRS.$$.ShowEmergencySquawks =                       'Показывать аварийные сквоки';  // Followed by "first / last / neither"
    VRS.$$.ShowEmptyValues =                            'Показывать пустые значения';
    VRS.$$.ShowForSelectedOnly =                        'Для выбранного самолета';
    VRS.$$.ShowInterestingAircraft =                    'Показывать интересующие самолеты';     // Followed by "first / last / neither"
    VRS.$$.ShowRangeCircles =                           'Показывать шкалу';
    VRS.$$.ShowShortTrails =                            'Показывать короткий след';
    VRS.$$.ShowSpeedType =                              'Показывать тип скорости';
    VRS.$$.ShowTrackType =                              'Показывать тип курса';
    VRS.$$.ShowUnits =                                  'Показывать единицы измерения';
    VRS.$$.ShowVerticalSpeedType =                      'Показывать тип вертикальной скорости';
    VRS.$$.ShowVsiInSeconds =                           'Показывать вертикальную скорость в м/с';
    VRS.$$.SignalLevel =                                'Мгновенный уровень сигнала';
    VRS.$$.Silhouette =                                 'Силуэт';
    VRS.$$.SilhouetteAndOpFlag =                        'Силуэт/Лого';
    VRS.$$.SiteTimedOut =                               'Вывод на экран был приостановлен в виду не активности. Закрыть окно для возобновления';
    VRS.$$.SortBy =                                     'сначала по';
    VRS.$$.Species =                                    'Тип';
    VRS.$$.Speed =                                      'Скорость';
    VRS.$$.SpeedGraph =                                 'График скорости';
    VRS.$$.Speeds =                                     'Скорость';
    VRS.$$.SpeedType =                                  'Тип скорости';
    VRS.$$.Squawk =                                     'Сквок';
    VRS.$$.Squawk7000 =                                 'Полет по ПВП в неконтролируемом ВП';
    VRS.$$.Squawk7500 =                                 'Захват самолета';
    VRS.$$.Squawk7600 =                                 'Потеря связи';
    VRS.$$.Squawk7700 =                                 'Авария или нештатная ситуация';
    VRS.$$.Start =                                      'Старт';
    VRS.$$.StartsWith =                                 'начинается с';
    VRS.$$.StartTime =                                  'Время старта';
    VRS.$$.Status =                                     'Статус';
    VRS.$$.StatuteMileAbbreviation =                    '{0} ми';
    VRS.$$.StatuteMiles =                               'Сухопутные мили';
    VRS.$$.StorageEngine =                              'Подсистема хранилища';
    VRS.$$.StorageSize =                                'Размер хранилища';
    VRS.$$.StrokeOpacity =                              'Непрозрачность контура';
    VRS.$$.SubmitRoute =                                'Добавить маршрут данного рейса в базу';
    VRS.$$.SubmitRouteCorrection =                      'Внести изменения в маршрут данного рейса';
    VRS.$$.SuppressAltitudeStalkWhenZoomedOut =         'Убирать линию высоты при мелком масштабе';
    VRS.$$.TargetAltitude =                             'Заданная высота';
    VRS.$$.TargetHeading =                              'Заданный курс';
    VRS.$$.ThenBy =                                     'затем по';
    VRS.$$.Tiltwing =                                   'C поворотным крылом';
    VRS.$$.TimeTracked =                                'Длительность отслеживания';
    VRS.$$.Tisb =                                       'TIS-B';  /** THIS IS NEW! **/
    VRS.$$.TitleAircraftDetail =                        'Формуляр';
    VRS.$$.TitleAircraftList =                          'Список';
    VRS.$$.TitleFlightDetail =                          'Формуляр';
    VRS.$$.TitleFlightsList =                           'Полеты';
    VRS.$$.ToAltitude =                                 'До {0}';
    VRS.$$.TitleSiteTimedOut =                          'Тайм-аут';
    VRS.$$.TotalHours =                                 'Общее количество часов';
    VRS.$$.TrackingCountAircraft =                      'Отслеживается: {0:N0}';
    VRS.$$.TrackingCountAircraftOutOf =                 'Отслеживается: {0:N0} (из {1:N0})';
    VRS.$$.TrackingOneAircraft =                        'Отслеживается: 1';
    VRS.$$.TrackingOneAircraftOutOf =                   'Отслеживается: 1 (из {0:N0})';
    VRS.$$.TransponderType =                            'Транспондер';
    VRS.$$.TransponderTypeFlag =                        'Тип транспондера';
    VRS.$$.TrueAirSpeed =                               'Истинная';
    VRS.$$.TrueAirSpeedShort =                          'TAS';  /** THIS IS NEW! **/  // Keep this one short, an abbreviation if possible.
    VRS.$$.TrueHeading =                                'Истинный курс';
    VRS.$$.TrueHeadingShort =                           'Истин.';
    VRS.$$.Turbo =                                      'Турбовинтовой';
    VRS.$$.Unknown =                                    'Неизвестный';
    VRS.$$.UseBrowserLocation =                         'Использовать GPS местоположение';
    VRS.$$.UseRelativeDates =                           'Использовать относительные данные';
    VRS.$$.UserTag =                                    'Пользовательский тэг';
    VRS.$$.VerticalSpeed =                              'Верт.скорость';
    VRS.$$.VerticalSpeedType =                          'Тип верт.скорости';
    VRS.$$.VirtualRadar =                               'Виртуальный Радар';
    VRS.$$.Volume25 =                                   'Громкость 25%';
    VRS.$$.Volume50 =                                   'Громкость 50%';
    VRS.$$.Volume75 =                                   'Громкость 75%';
    VRS.$$.Volume100 =                                  'Громкость 100%';
    VRS.$$.VrsVersion =                                 'Версия {0}';
    VRS.$$.WakeTurbulenceCategory =                     'Категория турбулентности в следе';
    VRS.$$.Warning =                                    'Предупреждающий сигнал';
    VRS.$$.WorkingInOfflineMode =                       'работа в автономном режиме';
    VRS.$$.WtcLight =                                   'Легкий';
    VRS.$$.WtcMedium =                                  'Средний';
    VRS.$$.WtcHeavy =                                   'Тяжелый';
    VRS.$$.YearBuilt =                                  'Год выпуска';
    VRS.$$.Yes =                                        'Да';

    // Date picker text
    VRS.$$.DateClose =                                  'Выполнено';  // Keep this short
    VRS.$$.DateCurrent =                                'Сегодня';  // Keep this short
    VRS.$$.DateNext =                                   'Завтра';  // Keep this short
    VRS.$$.DatePrevious =                               'Вчера';  // Keep this short
    VRS.$$.DateWeekAbbr =                               'Нед';  // Keep this very short
    VRS.$$.DateYearSuffix =                             'г.';  // This is displayed after the year
    // If your language has a different month format when days preceed months, and the date picker
    // should be using that month format, then set this to true. Otherwise leave at false.
    VRS.$$.DateUseGenetiveMonths =                      false;

    // Text-to-speech formatting
    VRS.$$.SayCallsign =                                'Call sign {0}.';
    VRS.$$.SayHyphen =                                  'hyphen';
    VRS.$$.SayIcao =                                    'eye co {0}.';
    VRS.$$.SayModelIcao =                               'Type {0}.';
    VRS.$$.SayOperator =                                'Operated by {0}.';
    VRS.$$.SayRegistration =                            'Registration {0}.';
    VRS.$$.SayRouteNotKnown =                           'Route not known.';
    VRS.$$.SayFromTo =                                  'Travelling from {0} to {1}.';
    VRS.$$.SayFromToVia =                               'Travelling from {0} via {1} to {2}.';

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

            if(isLastStopover)        result += ' and ';
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
            case 'C':       result = 'Спаренный,'; break;
            case '1':       result = 'Один,'; break;
            case '2':       result = 'Два,'; break;
            case '3':       result = 'Три,'; break;
            case '4':       result = 'Четыре,'; break;
            case '5':       result = 'Пять,'; break;
            case '6':       result = 'Шесть,'; break;
            case '7':       result = 'Семь,'; break;
            case '8':       result = 'Восемь,'; break;
            default:        result = countEngines; break;
        }

        switch(engineType) {
            case VRS.EngineType.Electric:   result += ' электрический'; break;
            case VRS.EngineType.Jet:        result += ' реактивный'; break;
            case VRS.EngineType.Piston:     result += ' поршневой'; break;
            case VRS.EngineType.Turbo:      result += ' газотурбинный'; break;
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
                case VRS.WakeTurbulenceCategory.None:   if(!ignoreNone) result = 'Нет данных'; break;
                case VRS.WakeTurbulenceCategory.Light:  result = 'Легкий'; break;
                case VRS.WakeTurbulenceCategory.Medium: result = 'Средний'; break;
                case VRS.WakeTurbulenceCategory.Heavy:  result = 'Тяжелый'; break;
                default: throw 'Unknown wake turbulence category ' + category;  // Do not translate this line
            }

            if(expandedDescription && result) {
                switch(category) {
                    case VRS.WakeTurbulenceCategory.Light:  result += ' (до 7 т.)'; break;
                    case VRS.WakeTurbulenceCategory.Medium: result += ' (от 7 до 136 т.)'; break;
                    case VRS.WakeTurbulenceCategory.Heavy:  result += ' (более 136 т.)'; break;
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
        if(from) result = 'Из ' + from;
        if(to) {
            if(result.length) result += ' в ';
            else              result = 'В ';
            result += to;
        }
        var stopovers = via ? via.length : 0;
        if(stopovers > 0) {
            result += ' через';
            for(var i = 0;i < stopovers;++i) {
                var stopover = via[i];
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

        return englishCountry;

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
