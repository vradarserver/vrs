// Copyright © 2013 onwards, Andrew Whewell
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

    VRS.$$.Add =                                        'Adicionar';
    VRS.$$.AddCondition =                               'Adicionar condição';
    VRS.$$.AddCriteria =                                'Adicionar critérios';
    VRS.$$.AddFilter =                                  'Adicionar filtro';
    VRS.$$.ADSB =                                       'ADS-B';
    VRS.$$.ADSB0 =                                      'ADS-B v0';
    VRS.$$.ADSB1 =                                      'ADS-B v1';
    VRS.$$.ADSB2 =                                      'ADS-B v2';
    VRS.$$.AircraftNotTransmittingCallsign =            'A aeronave não está transmitindo seu indicativo';
    VRS.$$.AircraftClass =                              'Classe da Aeronave';
    VRS.$$.Airport =                                    'Aeroporto';
    VRS.$$.AirportDataThumbnails =                      'Miniaturas (airport-data.com)';
    VRS.$$.AllAltitudes =                               'Todas as altitudes';
    VRS.$$.AllRows =                                    'Todas as linhas';
    VRS.$$.Altitude =                                   'Altitude';
    VRS.$$.AltitudeAndSpeedGraph =                      'Altitude & Velocidade';
    VRS.$$.AltitudeAndVerticalSpeed =                   'Altitude & VSI';
    VRS.$$.AltitudeGraph =                              'Altitude Graph';
    VRS.$$.AltitudeType =                               'Altimetro';
    VRS.$$.AllAircraft =                                'Mostrar todos';
    VRS.$$.Amphibian =                                  'Anfíbio';
    VRS.$$.AnnounceSelected =                           'Anunciar detalhes de aeronaves selecionadas';
    VRS.$$.Ascending =                                  'Crescente';
    VRS.$$.AutoSelectAircraft =                         'Auto selecionar as aeronaves';
    VRS.$$.AverageSignalLevel =                         'Avg. Signal Level';
    VRS.$$.Barometric =                                 'barométrico';
    VRS.$$.Bearing =                                    'Rumo';
    VRS.$$.Between =                                    'Entre';
    VRS.$$.Callsign =                                   'Indicativo';
    VRS.$$.CallsignAndShortRoute =                      'Indicativo & Rota';
    VRS.$$.CallsignMayNotBeCorrect =                    'Indicativo podem não estar corretos';
    VRS.$$.CentreOnSelectedAircraft =                   'Mostrar no mapa';
    VRS.$$.Civil =                                      'Civil';
    VRS.$$.CivilOrMilitary =                            'Civil / MIlitar';
    VRS.$$.ClosestToCurrentLocation =                   'Mais próxima';
    VRS.$$.CofACategory =                               'C/A Category';  // certificate of airworthiness category
    VRS.$$.CofAExpiry =                                 'C/A Expiry';  // certificate of airworthiness expiry
    VRS.$$.Columns =                                    'Columns';
    VRS.$$.Contains =                                   'Contenha';
    VRS.$$.CountAdsb =                                  'Mensagens ADS-B';
    VRS.$$.Country =                                    'País';
    VRS.$$.CountModeS =                                 'Mode-S Count';  /** THIS IS NEW! **/
    VRS.$$.CountPositions =                             'Position Count';
    VRS.$$.Criteria =                                   'Filtro';
    VRS.$$.CurrentLocationInstruction =                 'Para definir a sua localização atual clique "Definir a localização atual" e arraste o marcador.';
    VRS.$$.CurrentRegDate =                             'Current Reg. Date';
    VRS.$$.Date =                                       'Data';
    VRS.$$.DateTimeShort =                              '{0} {1}';  // Where "{0}" is a date, e.g. 10/10/2013; and "{1}" is a time, e.g. 17:41:32.
    VRS.$$.DefaultSetting =                             '< Padrão >';
    VRS.$$.DegreesAbbreviation =                        '{0}°';
    VRS.$$.DeRegDate =                                  'De-reg. Date';
    VRS.$$.DesktopPage =                                'Desktop Page';
    VRS.$$.DesktopReportPage =                          'Desktop Report Page';
    VRS.$$.DetailItems =                                'Aircraft Detail Items';
    VRS.$$.DetailPanel =                                'Painel de detalhes';
    VRS.$$.DisableAutoSelect =                          'Desativar auto-seleção';
    VRS.$$.Distance =                                   'Distancia';
    VRS.$$.Distances =                                  'Distancias';
    VRS.$$.DoNotImportAutoSelect =                      'Do not import auto-select settings';
    VRS.$$.DoNotImportCurrentLocation =                 'Não importar a localização';
    VRS.$$.DoNotImportRequestFeedId =                   'Do not import request feed ID';
    VRS.$$.DoNotImportLanguageSettings =                'Não importar a configuração de idioma';
    VRS.$$.DoNotImportSplitters =                       'Do not import splitters';
    VRS.$$.DoNotShow =                                  'Não mostrar';
    VRS.$$.Duration =                                   'Duração';
    VRS.$$.Electric =                                   'Electric';
    VRS.$$.EnableAutoSelect =                           'Ativar auto-seleção';
    VRS.$$.EnableFilters =                              'Ativar filtros';
    VRS.$$.EnableInfoWindow =                           'Enable info window';
    VRS.$$.End =                                        'End';
    VRS.$$.EndTime =                                    'End Time';
    VRS.$$.EndsWith =                                   'Termina com';
    VRS.$$.Engines =                                    'Motores';
    VRS.$$.EngineType =                                 'Tipo de Motor';
    VRS.$$.Equals =                                     'Extamente';
    VRS.$$.EraseBeforeImport =                          'Apague todas as configurações antes da importação';
    VRS.$$.ExportSettings =                             'Exportar configurações';
    VRS.$$.Feet =                                       'Pés';
    VRS.$$.FeetAbbreviation =                           '{0} ft';
    VRS.$$.FeetPerMinuteAbbreviation =                  '{0} ft/m';
    VRS.$$.FeetPerSecondAbbreviation =                  '{0} ft/s';
    VRS.$$.FetchPage =                                  'Buscar';
    VRS.$$.FillOpacity =                                'Transparencia do gráfico de alcance';
    VRS.$$.Filters =                                    'Filtros';
    VRS.$$.FindAllPermutationsOfCallsign =              'Find all permutations of callsign';
    VRS.$$.First =                                      'First';  /** THIS IS NEW! **/
    VRS.$$.FirstAltitude =                              'Primeira altitude';
    VRS.$$.FirstHeading =                               'Primeira proa';
    VRS.$$.FirstFlightLevel =                           'Primeiro FL';
    VRS.$$.FirstLatitude =                              'Primeira Latitude';
    VRS.$$.FirstLongitude =                             'Primeira Longitude';
    VRS.$$.FirstOnGround =                              'Primeira vez no solo';
    VRS.$$.FirstRegDate =                               'First Reg.Date';
    VRS.$$.FirstSpeed =                                 'Primeira Velocidade';
    VRS.$$.FirstSquawk =                                'Primeiro Squawk';
    VRS.$$.FirstVerticalSpeed =                         'Primeira Velocidade Vertical';
    VRS.$$.FlightDetailShort =                          'Detalhe';
    VRS.$$.FlightLevel =                                'Nível de voo';
    VRS.$$.FlightLevelAbbreviation =                    'FL{0}';
    VRS.$$.FlightLevelAndVerticalSpeed =                'FL & VS';
    VRS.$$.FlightLevelHeightUnit =                      'Nivel de voo';
    VRS.$$.FlightLevelTransitionAltitude =              'Altitude de transição';
    VRS.$$.FlightsCount =                               'Contador de voos';
    VRS.$$.FlightsListShort =                           'Voos';
    VRS.$$.FlightSimPage =                              'Pagina Flight Sim';
    VRS.$$.FlightSimTitle =                             'Virtual Radar - FSX';
    VRS.$$.ForcePhoneOff =                              'Nao é um smartphone';  // As in "force the page to ignore the fact that this is a smart phone"
    VRS.$$.ForcePhoneOn =                               'É um smartphone';  // As in "force the page to pretend that this is a smart phone"
    VRS.$$.ForceTabletOff =                             'Não é um tablet';  // As in "force the page to ignore the fact that this is a tablet PC"
    VRS.$$.ForceTabletOn =                              'É um tablet';  // As in "force the page to use the settings for a tablet PC"
    VRS.$$.FromAltitude =                               'Acima de {0}';
    VRS.$$.FromToAltitude =                             '{0} a {1}';
    VRS.$$.FromToDate =                                 '{0} a {1}';
    VRS.$$.FromToFlightLevel =                          '{0} a {1}';
    VRS.$$.FromToSpeed =                                '{0} a {1}';
    VRS.$$.FromToSquawk =                               '{0} a {1}';
    VRS.$$.FurthestFromCurrentLocation =                'Mais distante';
    VRS.$$.GenericName =                                'Generic Name';
    VRS.$$.Geometric =                                  'Geometric';
    VRS.$$.GeometricAltitudeIndicator =                 'GPS';  // A ** SHORT ** indication that the reported altitude is geometric (i.e. usually coming from a GPS unit) as opposed to barometric (i.e. coming off one or more pressure sensors).
    VRS.$$.GoogleMapsCouldNotBeLoaded =                 'Google Maps não pôde ser carregado';
    VRS.$$.GotoCurrentLocation =                        'Ir para a localização atual';
    VRS.$$.GotoSelectedAircraft =                       'Ir para a aeronave selecionada';
    VRS.$$.GroundAbbreviation =                         'GND';
    VRS.$$.Ground =                                     'Solo';
    VRS.$$.GroundTrack =                                'Ground track';
    VRS.$$.GroundVehicle =                              'Ground Vehicle';
    VRS.$$.Gyrocopter =                                 'Gyrocopter';
    VRS.$$.HadAlert =                                   'Had Alert';
    VRS.$$.HadEmergency =                               'Had Emergency';
    VRS.$$.HadSPI =                                     'Had Ident';  // SPI is the name of a pulse in Mode-S, used when ATC has asked for ident from aircraft.
    VRS.$$.Heading =                                    'Proa';
    VRS.$$.HeadingType =                                'Tipo de proa';
    VRS.$$.Heights =                                    'Altitudes';
    VRS.$$.Helicopter =                                 'Helicóptero';
    VRS.$$.Help =                                       'Ajuda';
    VRS.$$.HideAircraftNotOnMap =                       'Ocultar aeronaves que não estejam no mapa';
    VRS.$$.HideEmptyPinTextLines =                      'Ocultar linhas vazias';
    VRS.$$.HideNoPosition =                             'Has position';
    VRS.$$.HighContrastMap =                            'Contraste';  // <-- please try to keep this one short, it appears as a button on the map and there may not be a lot of room
    VRS.$$.Icao =                                       'ICAO';
    VRS.$$.Import =                                     'Importar';
    VRS.$$.ImportFailedTitle =                          'A importação falhoud';
    VRS.$$.ImportFailedBody =                           'Não foi possível importar as suas definições. O erro relatado foi: {0}';
    VRS.$$.ImportSettings =                             'Importar configurações';
    VRS.$$.Index =                                      'Index';
    VRS.$$.IndicatedAirSpeed =                          'Indicated';
    VRS.$$.IndicatedAirSpeedShort =                     'IAS';  // <-- please try to keep this short, an abbreviation if possible
    VRS.$$.Interesting =                                'Interesting';
    VRS.$$.IntervalSeconds =                            'Atualizar a cada (seg)';
    VRS.$$.IsMilitary =                                 'Militar';
    VRS.$$.Jet =                                        'Jet';
    VRS.$$.JustPositions =                              'Posição';
    VRS.$$.KilometreAbbreviation =                      '{0} km';
    VRS.$$.Kilometres =                                 'Quilômetros';
    VRS.$$.KilometresPerHour =                          'Km/h';
    VRS.$$.KilometresPerHourAbbreviation =              '{0} km/h';
    VRS.$$.Knots =                                      'Knots';
    VRS.$$.KnotsAbbreviation =                          '{0} kts';
    VRS.$$.LandPlane =                                  'Landplane';
    VRS.$$.Last =                                       'Last';  /** THIS IS NEW! **/
    VRS.$$.LastAltitude =                               'Última Altitude';
    VRS.$$.LastFlightLevel =                            'Último FL';
    VRS.$$.LastHeading =                                'Última proa';
    VRS.$$.LastOnGround =                               'Última vez em solo';
    VRS.$$.LastLatitude =                               'Última Latitude';
    VRS.$$.LastLongitude =                              'Última Longitude';
    VRS.$$.LastSpeed =                                  'Última velocidade';
    VRS.$$.LastSquawk =                                 'Último Squawk';
    VRS.$$.LastVerticalSpeed =                          'Última velocidade vertical';
    VRS.$$.Latitude =                                   'Latitude';
    VRS.$$.Layout =                                     'Tema';
    VRS.$$.Layout1 =                                    'Classico';
    VRS.$$.Layout2 =                                    'Tall Detail, Map Top';
    VRS.$$.Layout3 =                                    'Tall Detail, Map Bottom';
    VRS.$$.Layout4 =                                    'Tall List, Map Top';
    VRS.$$.Layout5 =                                    'Tall List, Map Bottom';
    VRS.$$.Layout6 =                                    'Tall Detail and List';
    VRS.$$.ListAircraftClass =                          'A/C Class';
    VRS.$$.ListAirportDataThumbnails =                  'Miniaturas (airport-data.com)';
    VRS.$$.ListAltitude =                               'Altitude';
    VRS.$$.ListAltitudeType =                           'Alt. Type';
    VRS.$$.ListAltitudeAndVerticalSpeed =               'Alt & VSI';
    VRS.$$.ListAverageSignalLevel =                     'Avg. Sig';
    VRS.$$.ListBearing =                                'Rumo';
    VRS.$$.ListCallsign =                               'Indicativo';
    VRS.$$.ListCivOrMil =                               'Civ/Mil';
    VRS.$$.ListCofACategory =                           'C/A Cat.';  // Certificate of airworthiness category
    VRS.$$.ListCofAExpiry =                             'C/A Expiry';  // Certificate of airworthiness expiry
    VRS.$$.ListCountAdsb =                              'ADS-B Msgs.';
    VRS.$$.ListCountMessages =                          'Msgs.';
    VRS.$$.ListCountModeS =                             'Mode-S Msgs.';
    VRS.$$.ListCountPositions =                         'Posn. Msgs.';
    VRS.$$.ListCountry =                                'País';
    VRS.$$.ListCurrentRegDate =                         'Current Reg.';  // Date of current registration
    VRS.$$.ListDeRegDate =                              'De-reg Date';  // as in the date it was taken off the register
    VRS.$$.ListDistance =                               'Distancia';
    VRS.$$.ListDuration =                               'Duração';
    VRS.$$.ListEndTime =                                'Última Message';  // As in the date and time of the last message.
    VRS.$$.ListEngines =                                'Motores';
    VRS.$$.ListFirstAltitude =                          'From Alt.';
    VRS.$$.ListFirstFlightLevel =                       'From FL';
    VRS.$$.ListFirstHeading =                           'Do Hdg.';
    VRS.$$.ListFirstLatitude =                          'From Lat.';
    VRS.$$.ListFirstLongitude =                         'From Lng.';
    VRS.$$.ListFirstOnGround =                          'From On Gnd.';
    VRS.$$.ListFirstRegDate =                           'Primeiro Registro';  // Date of first registration
    VRS.$$.ListFirstSpeed =                             'From Speed';
    VRS.$$.ListFirstSquawk =                            'From Squawk';
    VRS.$$.ListFirstVerticalSpeed =                     'From VSI';
    VRS.$$.ListFlightLevel =                            'FL';
    VRS.$$.ListFlightLevelAndVerticalSpeed =            'FL & VSI';
    VRS.$$.ListFlightsCount =                           'Visto';
    VRS.$$.ListGenericName =                            'Nome generico';
    VRS.$$.ListHadAlert =                               'Alert';
    VRS.$$.ListHadEmergency =                           'Emergency';
    VRS.$$.ListHadSPI =                                 'SPI';  // Name of a pulse in Mode-S, may not need translation. Used when ATC has asked for ident from aircraft.
    VRS.$$.ListHeading =                                'Hdg.';
    VRS.$$.ListHeadingType =                            'Hdg. Type';
    VRS.$$.ListIcao =                                   'ICAO';
    VRS.$$.ListInteresting =                            'Interesting';
    VRS.$$.ListLastAltitude =                           'To Alt.';
    VRS.$$.ListLastFlightLevel =                        'To FL';
    VRS.$$.ListLastHeading =                            'To Hdg.';
    VRS.$$.ListLastLatitude =                           'To Lat.';
    VRS.$$.ListLastLongitude =                          'To Lng.';
    VRS.$$.ListLastOnGround =                           'To On Gnd.';
    VRS.$$.ListLastSpeed =                              'To Speed';
    VRS.$$.ListLastSquawk =                             'To Squawk';
    VRS.$$.ListLastVerticalSpeed =                      'To VSI';
    VRS.$$.ListLatitude =                               'Lat.';
    VRS.$$.ListLongitude =                              'Lng.';
    VRS.$$.ListNotes =                                  'Notes';
    VRS.$$.ListManufacturer =                           'Manufacturer';
    VRS.$$.ListMaxTakeoffWeight =                       'Max T/O Weight';
    VRS.$$.ListMlat =                                   'MLAT';  /** THIS IS NEW! **/  // Abbreviation of Multilateration
    VRS.$$.ListModel =                                  'Modelo';
    VRS.$$.ListModelIcao =                              'Aeronave';
    VRS.$$.ListModeSCountry =                           'Mode-S Country';
    VRS.$$.ListModelSilhouette =                        'Silhueta';
    VRS.$$.ListModelSilhouetteAndOpFlag =               'Bandeiras';
    VRS.$$.ListOperator =                               'Operador';
    VRS.$$.ListOperatorFlag =                           'Bandeira';
    VRS.$$.ListOperatorIcao =                           'Op. Code';
    VRS.$$.ListOwnershipStatus =                        'Ownership Status';
    VRS.$$.ListPicture =                                'Picture';
    VRS.$$.ListPopularName =                            'Nome Popular';
    VRS.$$.ListPreviousId =                             'Previous ID';
    VRS.$$.ListReceiver =                               'Receiver';
    VRS.$$.ListRegistration =                           'Reg.';
    VRS.$$.ListRowNumber =                              'Linha';
    VRS.$$.ListRoute =                                  'Rota';
    VRS.$$.ListSerialNumber =                           'Serial';
    VRS.$$.ListSignalLevel =                            'Sig';
    VRS.$$.ListSpecies =                                'Species';
    VRS.$$.ListSpeed =                                  'Velocidade';
    VRS.$$.ListSpeedType =                              'Velocimetro';
    VRS.$$.ListSquawk =                                 'Squawk';
    VRS.$$.ListStartTime =                              'Hora';
    VRS.$$.ListStatus =                                 'Status';
    VRS.$$.ListTargetAltitude =                         'A/P Alt.';
    VRS.$$.ListTargetHeading =                          'A/P Hdg.';
    VRS.$$.ListTisb =                                   'TIS-B';  /** THIS IS NEW! **/
    VRS.$$.ListTotalHours =                             'Total Hours';
    VRS.$$.ListTransponderType =                        'Transponder';
    VRS.$$.ListTransponderTypeFlag =                    '';
    VRS.$$.ListUserTag =                                'Tag';
    VRS.$$.ListVerticalSpeed =                          'V.Speed';
    VRS.$$.ListVerticalSpeedType =                      'V.Speed Type';
    VRS.$$.ListWtc =                                    'WTC';
    VRS.$$.ListYearBuilt =                              'Built';
    VRS.$$.Longitude =                                  'Longitude';
    VRS.$$.Manufacturer =                               'Manufacturer';
    VRS.$$.Map =                                        'Map';
    VRS.$$.MaxTakeoffWeight =                           'Peso max. de decolagem';
    VRS.$$.Menu =                                       'Menu';
    VRS.$$.MenuBack =                                   'voltar';
    VRS.$$.MessageCount =                               'Mensagens recebidas';
    VRS.$$.MetreAbbreviation =                          '{0} m';
    VRS.$$.MetrePerSecondAbbreviation =                 '{0} m/seg';
    VRS.$$.MetrePerMinuteAbbreviation =                 '{0} m/min';
    VRS.$$.Metres =                                     'Metros';
    VRS.$$.MilesPerHour =                               'Milhas por hora';
    VRS.$$.MilesPerHourAbbreviation =                   '{0} mph';
    VRS.$$.Military =                                   'Militar';
    VRS.$$.Mlat =                                       'MLAT';  /** THIS IS NEW! **/  // An abbreviation of Multilateration
    VRS.$$.MobilePage =                                 'Mobile Page';
    VRS.$$.MobileReportPage =                           'Mobile Report Page';
    VRS.$$.Model =                                      'Modelo';
    VRS.$$.ModelIcao =                                  'Modelo da aeronave';
    VRS.$$.ModeS =                                      'Mode-S';
    VRS.$$.ModeSCountry =                               'Mode-S Country';
    VRS.$$.MovingMap =                                  'Seguir aeronave';
    VRS.$$.MuteOff =                                    'Desligar mudo';
    VRS.$$.MuteOn =                                     'Mudo';
    VRS.$$.NauticalMileAbbreviation =                   '{0} mn';
    VRS.$$.NauticalMiles =                              'Milhas Náuticas';
    VRS.$$.Neither =                                    'Neither';  /** THIS IS NEW! **/
    VRS.$$.No =                                         'No';
    VRS.$$.NoLocalStorage =                             'Este navegador não suporta o armazenamento local. Suas configurações não serão salvas\n\nSe você está navegando no "modo anonimo", em seguida, va para a versão normal. O modo anomimo não salva as alterações';
    VRS.$$.None =                                       'Nenhum';
    VRS.$$.Notes =                                      'Notas';
    VRS.$$.NoSettingsFound =                            'No settings found';
    VRS.$$.NotBetween =                                 'Não está entre';
    VRS.$$.NotContains =                                'Que não contenha';
    VRS.$$.NotEndsWith =                                'Não termina com';
    VRS.$$.NotEquals =                                  'Exceto';
    VRS.$$.NotStartsWith =                              'Não começa com';
    VRS.$$.OffRadarAction =                             'Quando a aeronave sair do alcance::';
    VRS.$$.OffRadarActionWait =                         'Desmarque a aeronave';
    VRS.$$.OffRadarActionEnableAutoSelect =             'Ativar auto-selecionar';
    VRS.$$.OffRadarActionNothing =                      'Não fazer nada';
    VRS.$$.OfPages =                                    'de {0:N0}';  // As in "1 of 10" pages
    VRS.$$.OnlyAircraftOnMap =                          'Listar apenas os visiveis';
    VRS.$$.OnlyAutoSelected =                           'Apenas anunciar detalhes de aeronaves auto-selecionadas';
    VRS.$$.OnlyUsePre22Icons =                          'Only show old style aircraft markers';  /** THIS IS NEW! **/
    VRS.$$.Operator =                                   'Operador';
    VRS.$$.OperatorCode =                               'Código do operador';
    VRS.$$.OperatorFlag =                               'Bandeira do operador';
    VRS.$$.Options =                                    'Opções';
    VRS.$$.OverwriteExistingSettings =                  'Overwrite existing settings';
    VRS.$$.OwnershipStatus =                            'Status do Proprietário';
    VRS.$$.PageAircraft =                               'Aeronave';
    VRS.$$.AircraftDetailShort =                        'Detail';
    VRS.$$.PageFirst =                                  'Primeira';
    VRS.$$.PageGeneral =                                'Geral';
    VRS.$$.PageLast =                                   'Última';
    VRS.$$.PageList =                                   'Lista';
    VRS.$$.PageListShort =                              'Lista';
    VRS.$$.PageMapShort =                               'Mapa';
    VRS.$$.PageNext =                                   'Próxima';
    VRS.$$.PagePrevious =                               'Anterior';
    VRS.$$.PaneAircraftDisplay =                        'Exibição da aeronave';
    VRS.$$.PaneAircraftTrails =                         'Trilhas das aeronaves';
    VRS.$$.PaneAudio =                                  'Áudio';
    VRS.$$.PaneAutoSelect =                             'auto-selecionar';
    VRS.$$.PaneCurrentLocation =                        'Localização Atual';
    VRS.$$.PaneDataFeed =                               'Data Feed';
    VRS.$$.PaneDetailSettings =                         'Detalhes da aeronave';
    VRS.$$.PaneInfoWindow =                             'Aircraft Info Window';
    VRS.$$.PaneListSettings =                           'Configurações da Lista';
    VRS.$$.PaneManyAircraft =                           'Relatórios de múltiplas Aeronaves';
    VRS.$$.PanePermanentLink =                          'Link permanente';
    VRS.$$.PaneRangeCircles =                           'Círculos de alcance';
    VRS.$$.PaneReceiverRange =                          'Alcance do receptor';
    VRS.$$.PaneSingleAircraft =                         'Relatórios de uma única aeronave';
    VRS.$$.PaneSortAircraftList =                       'Sort Aircraft List';
    VRS.$$.PaneSortReport =                             'Ordenar Relatório';
    VRS.$$.PaneUnits =                                  'Unidades';
    VRS.$$.Pause =                                      'Pausar';
    VRS.$$.PinTextNumber =                              'Legenda da Aeronave - Linha {0}';
    VRS.$$.PopularName =                                'Popular Name';
    VRS.$$.PositionAndAltitude =                        'Posição e altitude';
    VRS.$$.PositionAndSpeed =                           'Posição e velocidade';
    VRS.$$.Picture =                                    'Imagem';
    VRS.$$.PictureOrThumbnails =                        'Picture or Thumbnails';
    VRS.$$.PinTextLines =                               'Legendas a exibir';
    VRS.$$.Piston =                                     'Piston';
    VRS.$$.Pixels =                                     'pixels';
    VRS.$$.PoweredByVRS =                               'Desenvolvido por Virtual Radar Server';
    VRS.$$.PreviousId =                                 'Previous ID';
    VRS.$$.Quantity =                                   'Quantidade';
    VRS.$$.RadioMast =                                  'Radio Mast';
    VRS.$$.RangeCircleEvenColour =                      'Cor dos círculos pares';
    VRS.$$.RangeCircleOddColour =                       'Cor dos círculos impares';
    VRS.$$.RangeCircles =                               'Círculos de alcance';
    VRS.$$.Receiver =                                   'Receptor';
    VRS.$$.ReceiverRange =                              'Alcance do receptor';
    VRS.$$.Refresh =                                    'Atualizar';
    VRS.$$.Registration =                               'Registro';
    VRS.$$.RegistrationAndIcao =                        'Reg. & Icao';
    VRS.$$.Remove =                                     'Remover';
    VRS.$$.RemoveAll =                                  'Remover tudo';
    VRS.$$.ReportCallsignInvalid =                      'Relatório deste indicativo';
    VRS.$$.ReportCallsignValid =                        'Relatório do ondicativo: {0}';
    VRS.$$.ReportEmpty =                                'Nenhum dado encontrato com o filtro aplicado';
    VRS.$$.ReportFreeForm =                             'Relatório customizado';
    VRS.$$.ReportIcaoInvalid =                          'Relatório deste ICAO';
    VRS.$$.ReportIcaoValid =                            'Relatórios do ICAO: {0}';
    VRS.$$.ReportRegistrationInvalid =                  'Relatório desta matrícula';
    VRS.$$.ReportRegistrationValid =                    'Registration Report for {0}';
    VRS.$$.ReportTodaysFlights =                        'Voos de hoje';
    VRS.$$.ReportYesterdaysFlights =                    'Voos de ontem';
    VRS.$$.Reports =                                    'Reportes';
    VRS.$$.ReportsAreDisabled =                         'Server permissions prohibit the running of reports';
    VRS.$$.Resume =                                     'Resumir';
    VRS.$$.Reversing =                                  'Reversing';
    VRS.$$.ReversingShort =                             'REV';
    VRS.$$.Route =                                      'Rota';
    VRS.$$.RouteShort =                                 'Rota (curta)';
    VRS.$$.RouteFull =                                  'Rota (completa)';
    VRS.$$.RouteNotKnown =                              'Rota não conhecida';
    VRS.$$.RowNumber =                                  'Row Number';
    VRS.$$.Rows =                                       'Linha';
    VRS.$$.RunReport =                                  'Gerar relatório';
    VRS.$$.SeaPlane =                                   'Seaplane';
    VRS.$$.Select =                                     'Selecionar a:';
    VRS.$$.SeparateTwoValues =                          ' e ';
    VRS.$$.SerialNumber =                               'Serial';
    VRS.$$.ServerFetchFailedTitle =                     'Fetch Failed';
    VRS.$$.ServerFetchFailedBody =                      'Could not fetch from the server. The error is "{0}" and the status is "{1}".';
    VRS.$$.ServerFetchTimedOut =                        'The request has timed out.';
    VRS.$$.ServerReportExceptionBody =                  'The server encountered an exception while generating the report. The exception was "{0}"';
    VRS.$$.ServerReportExceptionTitle =                 'Server Exception';
    VRS.$$.SetCurrentLocation =                         'Definir a localização atual';
    VRS.$$.Settings =                                   'Configurações';
    VRS.$$.SettingsPage =                               'Pagina de configuração';
    VRS.$$.Shortcuts =                                  'Atalhos';
    VRS.$$.ShowAltitudeStalk =                          'Mostrar traço da altitude';
    VRS.$$.ShowAltitudeType =                           'Mostrar unidade da altitude';
    VRS.$$.ShowCurrentLocation =                        'Mostrar a localização atual';
    VRS.$$.ShowDetail =                                 'MOstrar detalhes';
    VRS.$$.ShowForAllAircraft =                         'Mostrar para todas as aeronaves';
    VRS.$$.ShowEmergencySquawks =                       'Show emergency squawks';  /** THIS IS NEW! **/  // Followed by "first / last / neither"
    VRS.$$.ShowEmptyValues =                            'Mostrar valores em branco';
    VRS.$$.ShowForSelectedOnly =                        'Mostrar apenas para a aeronave selecionada';
    VRS.$$.ShowInterestingAircraft =                    'Show interesting aircraft';  /** THIS IS NEW! **/  // Followed by "first / last / neither"
    VRS.$$.ShowRangeCircles =                           'Mostrar círculos de alcance';
    VRS.$$.ShowShortTrails =                            'Mostrar trilhas curtas';
    VRS.$$.ShowSpeedType =                              'Mostrar unidade da velocidade';
    VRS.$$.ShowTrackType =                              'Mostrar unidade do rumo';
    VRS.$$.ShowUnits =                                  'Mostrar unidades';
    VRS.$$.ShowVerticalSpeedType =                      'Mostrar unidade da velocidade vertical';
    VRS.$$.ShowVsiInSeconds =                           'Mostrar velocidade vertical por segundo';
    VRS.$$.SignalLevel =                                'Nivel do sinal';
    VRS.$$.Silhouette =                                 'Silhueta';
    VRS.$$.SilhouetteAndOpFlag =                        'Sil. & Op. Flag';
    VRS.$$.SiteTimedOut =                               'O site está em pausa devido à inatividade. Feche a caixa de mensagem para retomar as atualizações.';
    VRS.$$.SortBy =                                     'Ordenar por';
    VRS.$$.Species =                                    'Species';
    VRS.$$.Speed =                                      'Velocidade';
    VRS.$$.SpeedGraph =                                 'Speed Graph';
    VRS.$$.Speeds =                                     'Velocidades';
    VRS.$$.SpeedType =                                  'Velocimetro';
    VRS.$$.Squawk =                                     'Squawk';
    VRS.$$.Squawk7000 =                                 'Nenhum squawk atribuído';
    VRS.$$.Squawk7500 =                                 'Aircraft hijacking';
    VRS.$$.Squawk7600 =                                 'Falha no Radio';
    VRS.$$.Squawk7700 =                                 'Emergência Geral';
    VRS.$$.Start =                                      'Iniciar';
    VRS.$$.StartsWith =                                 'Começa com';
    VRS.$$.StartTime =                                  'Hora de inicio';
    VRS.$$.Status =                                     'Status';
    VRS.$$.StatuteMileAbbreviation =                    '{0} mi';
    VRS.$$.StatuteMiles =                               'Milhas Terrestres';
    VRS.$$.StorageEngine =                              'Storage engine';
    VRS.$$.StorageSize =                                'Storage size';
    VRS.$$.StrokeOpacity =                              'Transparencia da borda do gráfico';
    VRS.$$.SubmitRoute =                                'Enviar rota';
    VRS.$$.SubmitRouteCorrection =                      'Enviar correção de rota';
    VRS.$$.SuppressAltitudeStalkWhenZoomedOut =         'Não mostrar traço da altitude em zoom alto';
    VRS.$$.TargetAltitude =                             'Altitude planejada';
    VRS.$$.TargetHeading =                              'Proa planejada';
    VRS.$$.ThenBy =                                     'em seguida por';
    VRS.$$.Tiltwing =                                   'Tiltwing';
    VRS.$$.TimeTracked =                                'Duração do rasterio';
    VRS.$$.Tisb =                                       'TIS-B';  /** THIS IS NEW! **/
    VRS.$$.TitleAircraftDetail =                        'Detalhes da aeronave ';
    VRS.$$.TitleAircraftList =                          'Lista de aeronaves';
    VRS.$$.TitleFlightDetail =                          'Detalhes';
    VRS.$$.TitleFlightsList =                           'Voos';
    VRS.$$.ToAltitude =                                 'Até {0}';
    VRS.$$.TitleSiteTimedOut =                          'Tempo Esgotado';
    VRS.$$.TotalHours =                                 'Total de horas';
    VRS.$$.TrackingCountAircraft =                      'Rastreando {0:N0} aeronaves';
    VRS.$$.TrackingCountAircraftOutOf =                 'Rastreando {0:N0} aeronaves (de {1:N0})';
    VRS.$$.TrackingOneAircraft =                        'Rastreando 1 aeronave';
    VRS.$$.TrackingOneAircraftOutOf =                   'Rastreando 1 aeronave (de {0:N0})';
    VRS.$$.TransponderType =                            'Transponder';
    VRS.$$.TransponderTypeFlag =                        'Band. do Transponder';
    VRS.$$.TrueAirSpeed =                               'True';
    VRS.$$.TrueAirSpeedShort =                          'TAS';  // Keep this one short, an abbreviation if possible.
    VRS.$$.TrueHeading =                                'True heading';
    VRS.$$.TrueHeadingShort =                           'True';
    VRS.$$.Turbo =                                      'Turbo';
    VRS.$$.Unknown =                                    'Desconhecido';
    VRS.$$.UseBrowserLocation =                         'Use a localização GPS';
    VRS.$$.UseRelativeDates =                           'Manter as datas';
    VRS.$$.UserTag =                                    'User Tag';
    VRS.$$.VerticalSpeed =                              'Velocidade vertical';
    VRS.$$.VerticalSpeedType =                          'Vertical Speed Type';
    VRS.$$.VirtualRadar =                               'Virtual Radar';
    VRS.$$.Volume25 =                                   'Volume 25%';
    VRS.$$.Volume50 =                                   'Volume 50%';
    VRS.$$.Volume75 =                                   'Volume 75%';
    VRS.$$.Volume100 =                                  'Volume 100%';
    VRS.$$.VrsVersion =                                 'Versão {0}';
    VRS.$$.WakeTurbulenceCategory =                     'Esteira de turbulência';
    VRS.$$.Warning =                                    'Aviso';
    VRS.$$.WorkingInOfflineMode =                       'Trabalhando em modo offline';
    VRS.$$.WtcLight =                                   'Leve';
    VRS.$$.WtcMedium =                                  'Médio';
    VRS.$$.WtcHeavy =                                   'Pesado';
    VRS.$$.YearBuilt =                                  'Ano de fabricação';
    VRS.$$.Yes =                                        'Sim';

    // Date picker text
    VRS.$$.DateClose =                                  'Feito';  // Keep this short
    VRS.$$.DateCurrent =                                'Hoje';  // Keep this short
    VRS.$$.DateNext =                                   'Próx.';  // Keep this short
    VRS.$$.DatePrevious =                               'Ante.';  // Keep this short
    VRS.$$.DateWeekAbbr =                               'Wk';  // Keep this very short
    VRS.$$.DateYearSuffix =                             'Ano';  // This is displayed after the year
    // If your language has a different month format when days preceed months, and the date picker
    // should be using that month format, then set this to true. Otherwise leave at false.
    VRS.$$.DateUseGenetiveMonths =                      false;

    // Text-to-speech formatting
    VRS.$$.SayCallsign =                                'Voo {0}.';
    VRS.$$.SayHyphen =                                  'hyphen';
    VRS.$$.SayIcao =                                    'ICAO {0}.';
    VRS.$$.SayModelIcao =                               'Tipo {0}.';
    VRS.$$.SayOperator =                                'Operado pela {0}.';
    VRS.$$.SayRegistration =                            'Registro {0}.';
    VRS.$$.SayRouteNotKnown =                           'Rota desconhecida.';
    VRS.$$.SayFromTo =                                  'Saindo de {0} para {1}.';
    VRS.$$.SayFromToVia =                               'Saindo de {0} via {1} para {2}.';

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
            case 'C':       result = 'Coupled'; break;
            case '1':       result = 'Single'; break;
            case '2':       result = 'Twin'; break;
            case '3':       result = 'Three'; break;
            case '4':       result = 'Four'; break;
            case '5':       result = 'Five'; break;
            case '6':       result = 'Six'; break;
            case '7':       result = 'Seven'; break;
            case '8':       result = 'Eight'; break;
            default:        result = countEngines; break;
        }

        switch(engineType) {
            case VRS.EngineType.Electric:   result += ' electrico'; break;
            case VRS.EngineType.Jet:        result += ' jato'; break;
            case VRS.EngineType.Piston:     result += ' pistão'; break;
            case VRS.EngineType.Turbo:      result += ' turbo'; break;
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
                case VRS.WakeTurbulenceCategory.None:   if(!ignoreNone) result = 'None'; break;
                case VRS.WakeTurbulenceCategory.Light:  result = 'Leve'; break;
                case VRS.WakeTurbulenceCategory.Medium: result = 'Médio'; break;
                case VRS.WakeTurbulenceCategory.Heavy:  result = 'Pesado'; break;
                default: throw 'Unknown wake turbulence category' + category;  // Do not translate this line
            }

            if(expandedDescription && result) {
                switch(category) {
                    case VRS.WakeTurbulenceCategory.Light:  result += ' (up to 7 tons)'; break;
                    case VRS.WakeTurbulenceCategory.Medium: result += ' (up to 135 tons)'; break;
                    case VRS.WakeTurbulenceCategory.Heavy:  result += ' (over 135 tons)'; break;
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
        if(from) result = 'De ' + from;
        if(to) {
            if(result.length) result += ' Para ';
            else              result = 'Para ';
            result += to;
        }
        var stopovers = via ? via.length : 0;
        if(stopovers > 0) {
            result += ' via';
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
