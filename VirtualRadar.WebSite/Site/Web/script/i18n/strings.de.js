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

    VRS.$$.Add =                                        'Hinzufügen';
    VRS.$$.AddCondition =                               'Bedingung hinzufügen';
    VRS.$$.AddCriteria =                                'Kriterien hinzufügen';
    VRS.$$.AddFilter =                                  'Filter hinzufügen';
    VRS.$$.ADSB =                                       'ADS-B';
    VRS.$$.ADSB0 =                                      'ADS-B v0';
    VRS.$$.ADSB1 =                                      'ADS-B v1';
    VRS.$$.ADSB2 =                                      'ADS-B v2';
    VRS.$$.AircraftNotTransmittingCallsign =            'Flugzeug sendet kein Callsign';
    VRS.$$.AircraftClass =                              'Flugzeugklasse';
    VRS.$$.Airport =                                    'Flughafen';
    VRS.$$.AirportDataThumbnails =                      'Skizze (airport-data.com)';
    VRS.$$.AllAltitudes =                               'Alle Höhen';
    VRS.$$.AllRows =                                    'Alle Zeilen';
    VRS.$$.Altitude =                                   'Höhe';
    VRS.$$.AltitudeAndSpeedGraph =                      'Höhen & Geschwindigkeitsgraph';
    VRS.$$.AltitudeAndVerticalSpeed =                   'Höhe & VSI';
    VRS.$$.AltitudeGraph =                              'Höhenschaubild';
    VRS.$$.AltitudeType =                               'Höhentyp';
    VRS.$$.AllAircraft =                                'Alle auflisten';
    VRS.$$.Amphibian =                                  'Amphibie';
    VRS.$$.AnnounceSelected =                           'Details des ausgewählten Flugzeugs anzeigen';
    VRS.$$.Ascending =                                  'Aufsteigend';
    VRS.$$.AutoSelectAircraft =                         'Auto-selektiere Flugzeug';
    VRS.$$.AverageSignalLevel =                         'Durchschn. Signalpegel';
    VRS.$$.Barometric =                                 'Barometrisch';
    VRS.$$.Bearing =                                    'Peilung/Lage';
    VRS.$$.Between =                                    'Zwischen';
    VRS.$$.Callsign =                                   'Callsign';
    VRS.$$.CallsignAndShortRoute =                      'Callsign & Route';
    VRS.$$.CallsignMayNotBeCorrect =                    'Callsign evtl. nicht korrekt';
    VRS.$$.CentreOnSelectedAircraft =                   'Auf Karte anzeigen';
    VRS.$$.Civil =                                      'Zivil';
    VRS.$$.CivilOrMilitary =                            'Zivil / Militär';
    VRS.$$.ClosestToCurrentLocation =                   'Am nächsten zu';
    VRS.$$.CofACategory =                               'C/A  Kategorie';                     // Bescheinigung der Lufttüchtigkeitskategorie (autom transl)
    VRS.$$.CofAExpiry =                                 'C/A Ablauf';                       // Bescheinigung des Lufttüchtigkeitsablaufs (autom. transl).
    VRS.$$.Columns =                                    'Spalten';
    VRS.$$.Contains =                                   'Enthält';
    VRS.$$.CountAdsb =                                  'ADS-B Zähler';
    VRS.$$.Country =                                    'Land';
    VRS.$$.CountModeS =                                 'Mode-S Zähler';
    VRS.$$.CountPositions =                             'Positionszähler';
    VRS.$$.Criteria =                                   'Kriterien';
    VRS.$$.CurrentLocationInstruction =                 'Um Ihren derzeitigen Standort zu setzen, wählen Sie "Setze aktuellen Standort" aus und ziehen Sie die Markierung.';
    VRS.$$.CurrentRegDate =                             'Derzeitiges  Reg. Datum';
    VRS.$$.Date =                                       'Datum';
    VRS.$$.DateTimeShort =                              '{0} {1}';                          // Where "{0}" is a date, e.g. 10/10/2013; and "{1}" is a time, e.g. 17:41:32.
    VRS.$$.DefaultSetting =                             '< Default >';
    VRS.$$.DegreesAbbreviation =                        '{0}°';
    VRS.$$.DeRegDate =                                  'De-reg. Datum';
    VRS.$$.DesktopPage =                                'Desktop Seite';
    VRS.$$.DesktopReportPage =                          'Desktop Berichtsseite';
    VRS.$$.DetailItems =                                'Flugzeug Detail Dinge';
    VRS.$$.DetailPanel =                                'Detailanzeige';
    VRS.$$.DisableAutoSelect =                          'Deaktiviere Auto-Selektieren';
    VRS.$$.Distance =                                   'Entfernung';
    VRS.$$.Distances =                                  'Entfernungen';
    VRS.$$.DoNotImportAutoSelect =                      'Einstellungen für Auto-Selektieren nicht importieren';
    VRS.$$.DoNotImportCurrentLocation =                 'Aktuellen Standort nicht importieren';
    VRS.$$.DoNotImportRequestFeedId =                   '"Request Feed ID" nicht importieren';
    VRS.$$.DoNotImportLanguageSettings =                'Spracheinstellungen nicht importieren';
    VRS.$$.DoNotImportSplitters =                       '"Splitter" nicht importieren';
    VRS.$$.DoNotShow =                                  'Nicht anzeigen	';
    VRS.$$.Duration =                                   'Dauer';
    VRS.$$.Electric =                                   'Elektrik';
    VRS.$$.EnableAutoSelect =                           'Aktiviere Auto-Selektieren';
    VRS.$$.EnableFilters =                              'Aktiviere Filter';
    VRS.$$.EnableInfoWindow =                           'Aktiviere Infofenster';
    VRS.$$.End =                                        'Ende';
    VRS.$$.EndTime =                                    'Endzeit';
    VRS.$$.EndsWith =                                   'Endet mit';
    VRS.$$.Engines =                                    'Triebwerke';
    VRS.$$.EngineType =                                 'Triebwerkstyp';
    VRS.$$.Equals =                                     'Ist';
    VRS.$$.EraseBeforeImport =                          'Löschen aller  Einstellungen vor dem Import';
    VRS.$$.ExportSettings =                             'Einstellungen exportieren';
    VRS.$$.Feet =                                       'Fuß';
    VRS.$$.FeetAbbreviation =                           '{0} Ft';
    VRS.$$.FeetPerMinuteAbbreviation =                  '{0} Ft/m';
    VRS.$$.FeetPerSecondAbbreviation =                  '{0} Ft/s';
    VRS.$$.FetchPage =                                  'Abrufen';
    VRS.$$.FillOpacity =                                'Fülldichte';
    VRS.$$.Filters =                                    'Filter';
    VRS.$$.FindAllPermutationsOfCallsign =              'Finde alle Permutationen des Callsign';
    VRS.$$.FirstAltitude =                              'Erste Höhe';
    VRS.$$.FirstHeading =                               'Erster Kurs';
    VRS.$$.FirstFlightLevel =                           'Erste FH';
    VRS.$$.FirstLatitude =                              'Erste Breitenangabe';
    VRS.$$.FirstLongitude =                             'Erste Längenangabe';
    VRS.$$.FirstOnGround =                              'Erste am Boden';
    VRS.$$.FirstRegDate =                               'Erstes Reg. Datum';
    VRS.$$.FirstSpeed =                                 'Erste Geschwindigkeit';
    VRS.$$.FirstSquawk =                                'Erster "Squawk"';
    VRS.$$.FirstVerticalSpeed =                         'Erste Steig-/Sinkrate';
    VRS.$$.FlightDetailShort =                          'Detail';
    VRS.$$.FlightLevel =                                'Flugebene';
    VRS.$$.FlightLevelAbbreviation =                    'FL{0}';
    VRS.$$.FlightLevelAndVerticalSpeed =                'FL & VSI';
    VRS.$$.FlightLevelHeightUnit =                      'Flugebene Höheneinheit';
    VRS.$$.FlightLevelTransitionAltitude =              'Flugebenen-Übergangshöhe';
    VRS.$$.FlightsCount =                               'Flüge Zähler';
    VRS.$$.FlightsListShort =                           'Flüge';
    VRS.$$.FlightSimPage =                              'Flug Simulator Seite';
    VRS.$$.FlightSimTitle =                             'Virtual Radar - FSX';
    VRS.$$.ForcePhoneOff =                              'Ist kein Telefon';                      // As in "force the page to ignore the fact that this is a smart phone"
    VRS.$$.ForcePhoneOn =                               'Ist Telefon';                          // As in "force the page to pretend that this is a smart phone"
    VRS.$$.ForceTabletOff =                             'Ist kein "Tablet"';                     // As in "force the page to ignore the fact that this is a tablet PC"
    VRS.$$.ForceTabletOn =                              'Ist "Tablet"';                         // As in "force the page to use the settings for a tablet PC"
    VRS.$$.FromAltitude =                               'ab {0}';
    VRS.$$.FromToAltitude =                             '{0} bis {1}';
    VRS.$$.FromToDate =                                 '{0} bis {1}';
    VRS.$$.FromToFlightLevel =                          '{0} bis {1}';
    VRS.$$.FromToSpeed =                                '{0} bis {1}';
    VRS.$$.FromToSquawk =                               '{0} bis {1}';
    VRS.$$.FurthestFromCurrentLocation =                'Am weitesten vom aktuellen Standort entfernt';
    VRS.$$.GenericName =                                'Generischer Name';
    VRS.$$.Geometric =                                  'Geometrisch';
    VRS.$$.GeometricAltitudeIndicator =                 'GPS';                              // A ** SHORT ** indication that the reported altitude is geometric (i.e. usually coming from a GPS unit) as opposed to barometric (i.e. coming off one or more pressure sensors).
    VRS.$$.GoogleMapsCouldNotBeLoaded =                 'Google Maps konnte nicht geladen werden';
    VRS.$$.GotoCurrentLocation =                        'Gehe zu aktuellem Standort';
    VRS.$$.GotoSelectedAircraft =                       'Gehe zu ausgewähltem Flugzeug';
    VRS.$$.GroundAbbreviation =                         'GND';
    VRS.$$.Ground =                                     'Relativ zum Boden (FüG)';
    VRS.$$.GroundTrack =                                'Bodenspur';
    VRS.$$.GroundVehicle =                              'Bodenfahrzeug';
    VRS.$$.Gyrocopter =                                 '"Gyrocopter"';
    VRS.$$.HadAlert =                                   'Hatte Alarm';
    VRS.$$.HadEmergency =                               'Hatte Notfall';
    VRS.$$.HadSPI =                                     'Hatte Identifikatiion';                        // SPI ist der Name eines Pulses in Mode-S, benutzt, wenn ATC um Identifizierung von Flugzeug gebeten hat.
    VRS.$$.Heading =                                    'Kurs';
    VRS.$$.HeadingType =                                'Kurstyp';
    VRS.$$.Heights =                                    'Höhen';
    VRS.$$.Helicopter =                                 'Hubschrauber';
    VRS.$$.Help =                                       'Hilfe';
    VRS.$$.HideAircraftNotOnMap =                       'Flugzeuge ausblenden die nicht auf der Karte angezeigt werden ';
    VRS.$$.HideEmptyPinTextLines =                      'Leere Beschriftungszeilen ausblenden';
    VRS.$$.HideNoPosition =                             'Hat Position';
    VRS.$$.HighContrastMap =                            'Kontrast';                         // <-- please try to keep this one short, it appears as a button on the map and there may not be a lot of room
    VRS.$$.Icao =                                       'ICAO';
    VRS.$$.Import =                                     'Import';
    VRS.$$.ImportFailedTitle =                          'Import der Einstellungen ist fehlgeschlagen';
    VRS.$$.ImportFailedBody =                           'Konnte Einstellungen nicht importieren. Der berichtete Fehler war: {0}';
    VRS.$$.ImportSettings =                             'Importiere Einstellungen';
    VRS.$$.Index =                                      'Index';
    VRS.$$.IndicatedAirSpeed =                          'Angezeigt';
    VRS.$$.IndicatedAirSpeedShort =                     'IAS';                              // <-- please try to keep this short, an abbreviation if possible
    VRS.$$.Interesting =                                'Von Interesse';
    VRS.$$.IntervalSeconds =                            'Intervall aktualisieren (Sek.)';
    VRS.$$.IsMilitary =                                 'Militär';
    VRS.$$.Jet =                                        'Jet';
    VRS.$$.JustPositions =                              'Positionen';
    VRS.$$.KilometreAbbreviation =                      '{0} km';
    VRS.$$.Kilometres =                                 'Kilometer';
    VRS.$$.KilometresPerHour =                          'Kilometer/Stunde';
    VRS.$$.KilometresPerHourAbbreviation =              '{0} km/h';
    VRS.$$.Knots =                                      'Knoten';
    VRS.$$.KnotsAbbreviation =                          '{0} kts';
    VRS.$$.LandPlane =                                  'Landplane';
    VRS.$$.LastAltitude =                               'Letzte Höhe';
    VRS.$$.LastFlightLevel =                            'Letzte FH';
    VRS.$$.LastHeading =                                'Letzter Kurs';
    VRS.$$.LastOnGround =                               'Zuletzt am Boden';
    VRS.$$.LastLatitude =                               'Letzte Breitenangabe';
    VRS.$$.LastLongitude =                              'Letzte Längenangabe';
    VRS.$$.LastSpeed =                                  'Letzte Geschwindigkeit';
    VRS.$$.LastSquawk =                                 'Letztes "Squawk"';
    VRS.$$.LastVerticalSpeed =                          'Letzte Steig-/Sinkrate';
    VRS.$$.Latitude =                                   'Breitenangabe';
    VRS.$$.Layout =                                     'Anordnung';
    VRS.$$.Layout1 =                                    'Klassisch';
    VRS.$$.Layout2 =                                    'Großes Detail, Karte Oben';
    VRS.$$.Layout3 =                                    'Großes Detail, Karte Unten';
    VRS.$$.Layout4 =                                    'Große Liste, Karte Oben';
    VRS.$$.Layout5 =                                    'Große Liste, Karte Unten';
    VRS.$$.Layout6 =                                    'Liste links, Details rechts, keine Karte';
    VRS.$$.ListAircraftClass =                          'A/C Klasse';
    VRS.$$.ListAirportDataThumbnails =                  'Skizze (airport-data.com)';
    VRS.$$.ListAltitude =                               'Höhe';
    VRS.$$.ListAltitudeType =                           'Alt. Typ';
    VRS.$$.ListAltitudeAndVerticalSpeed =               'Alt & VSI';
    VRS.$$.ListAverageSignalLevel =                     'Durchschn. Sig';
    VRS.$$.ListBearing =                                'Plg.';
    VRS.$$.ListCallsign =                               'Callsign';
    VRS.$$.ListCivOrMil =                               'Ziv/Mil';
    VRS.$$.ListCofACategory =                           'C/A Kat.';                 // Bescheinigung der Lufttüchtigkeitskategorie
    VRS.$$.ListCofAExpiry =                             'C/A Ablauf';               // Bescheinigung des Lufttüchtigkeitsablaufs
    VRS.$$.ListCountAdsb =                              'ADS-B Nachr.';
    VRS.$$.ListCountMessages =                          'Nachr.';
    VRS.$$.ListCountModeS =                             'Mode-S Nachr.';
    VRS.$$.ListCountPositions =                         'Pos. Nachr.';
    VRS.$$.ListCountry =                                'Land';
    VRS.$$.ListCurrentRegDate =                         'Aktuelle Reg.';             // Datum der jetzigen Registrierung
    VRS.$$.ListDeRegDate =                              'De-reg Datum';              //, Datum als aus dem Register genommen
    VRS.$$.ListDistance =                               'Entfernung';
    VRS.$$.ListDuration =                               'Dauer';
    VRS.$$.ListEndTime =                                'Letzte Nachricht';             // Datum und Zeit der letzten Nachricht.
    VRS.$$.ListEngines =                                'Triebwerke';
    VRS.$$.ListFirstAltitude =                          'Von Alt.';
    VRS.$$.ListFirstFlightLevel =                       'Von FH';
    VRS.$$.ListFirstHeading =                           'Von Kurs';
    VRS.$$.ListFirstLatitude =                          'Von Lat.';
    VRS.$$.ListFirstLongitude =                         'Von Lng.';
    VRS.$$.ListFirstOnGround =                          'Von "On Gnd."';
    VRS.$$.ListFirstRegDate =                           'Erste Reg.';               // Datum der ersten Registrierung
    VRS.$$.ListFirstSpeed =                             'Von Geschwindigkeit';
    VRS.$$.ListFirstSquawk =                            'Von "Squawk"';
    VRS.$$.ListFirstVerticalSpeed =                     'Von VSI';
    VRS.$$.ListFlightLevel =                            'FH';
    VRS.$$.ListFlightLevelAndVerticalSpeed =            'FH & VSI';
    VRS.$$.ListFlightsCount =                           'Gesehen';
    VRS.$$.ListGenericName =                            'Generischer Name';
    VRS.$$.ListHadAlert =                               'Alarm';
    VRS.$$.ListHadEmergency =                           'Notfall';
    VRS.$$.ListHadSPI =                                 'SPI';                      // Name eines Pulses in Mode-S, nicht notwendig Übersetzung. notwendig, wenn ATC um Ident von Flugzeug gebeten hat.
    VRS.$$.ListHeading =                                'Kurs';
    VRS.$$.ListHeadingType =                            'Kurs Typ';
    VRS.$$.ListIcao =                                   'ICAO';
    VRS.$$.ListInteresting =                            'Von Interesse';
    VRS.$$.ListLastAltitude =                           'Bis Alt.';
    VRS.$$.ListLastFlightLevel =                        'Bis FH';
    VRS.$$.ListLastHeading =                            'Bis Hdg.';
    VRS.$$.ListLastLatitude =                           'Bis Lat.';
    VRS.$$.ListLastLongitude =                          'Bis Lng.';
    VRS.$$.ListLastOnGround =                           'Bis on Gnd.';
    VRS.$$.ListLastSpeed =                              'Bis Geschwindigkeit';
    VRS.$$.ListLastSquawk =                             'Bis "Squawk"';
    VRS.$$.ListLastVerticalSpeed =                      'Bis VSI';
    VRS.$$.ListLatitude =                               'Lat.';
    VRS.$$.ListLongitude =                              'Lng.';
    VRS.$$.ListNotes =                                  'Notizen';
    VRS.$$.ListManufacturer =                           'Hersteller';
    VRS.$$.ListMaxTakeoffWeight =                       'Max. T/O Gewicht';
    VRS.$$.ListMlat =                                   'MLAT';                 //Abkürzung für Multilateration
    VRS.$$.ListModel =                                  'Modell';
    VRS.$$.ListModelIcao =                              'Typ';
    VRS.$$.ListModeSCountry =                           'Mode-S Land';
    VRS.$$.ListModelSilhouette =                        'Silhouette';
    VRS.$$.ListModelSilhouetteAndOpFlag =               'Flaggen';
    VRS.$$.ListOperator =                               'Betreiber';
    VRS.$$.ListOperatorFlag =                           'Flagge';
    VRS.$$.ListOperatorIcao =                           'Betr. "Code"';
    VRS.$$.ListOwnershipStatus =                        'Eigentumsstatus';
    VRS.$$.ListPicture =                                'Bild';
    VRS.$$.ListPopularName =                            'Bekannter Name';
    VRS.$$.ListPreviousId =                             'Vorherige ID';
    VRS.$$.ListReceiver =                               'Empfänger';
    VRS.$$.ListRegistration =                           'Reg.';
    VRS.$$.ListRowNumber =                              'Zeile';
    VRS.$$.ListRoute =                                  'Route';
    VRS.$$.ListSerialNumber =                           'Seriennummer';
    VRS.$$.ListSignalLevel =                            'Sig';
    VRS.$$.ListSpecies =                                'Art/Typ';
    VRS.$$.ListSpeed =                                  'Geschwindigkeit';
    VRS.$$.ListSpeedType =                              'Geschwindigkeitstyp';
    VRS.$$.ListSquawk =                                 '"Squawk"';
    VRS.$$.ListStartTime =                              'Gesehen';
    VRS.$$.ListStatus =                                 'Status';
    VRS.$$.ListTargetAltitude =                         'A/P Alt.';
    VRS.$$.ListTargetHeading =                          'A/P Hdg.';
    VRS.$$.ListTotalHours =                             'Stunden Total';
    VRS.$$.ListTransponderType =                        'Transponder';
    VRS.$$.ListTransponderTypeFlag =                    '';
    VRS.$$.ListUserTag =                                'Etikett';
    VRS.$$.ListVerticalSpeed =                          'Steig-/Sinkgeschw.';
    VRS.$$.ListVerticalSpeedType =                      'Steig-/Sinkgeschw.typ';
    VRS.$$.ListWtc =                                    'WTC';
    VRS.$$.ListYearBuilt =                              'Erbaut';
    VRS.$$.Longitude =                                  'Längenangabe';
    VRS.$$.Manufacturer =                               'Hersteller';
    VRS.$$.Map =                                        'Karte';
    VRS.$$.MaxTakeoffWeight =                           '"MaxTakeoffWeight"';
    VRS.$$.Menu =                                       'Menü';
    VRS.$$.MenuBack =                                   'zurück';
    VRS.$$.MessageCount =                               'Nachrichten Zähler';
    VRS.$$.MetreAbbreviation =                          '{0} m';
    VRS.$$.MetrePerSecondAbbreviation =                 '{0} m/sec';
    VRS.$$.MetrePerMinuteAbbreviation =                 '{0} m/min';
    VRS.$$.Metres =                                     'Meter';
    VRS.$$.MilesPerHour =                               'Meilen/Stunde';
    VRS.$$.MilesPerHourAbbreviation =                   '{0} mph';
    VRS.$$.Military =                                   'Militär';
    VRS.$$.Mlat =                                       'MLAT';                 //Eine Abkürzung für Multilateration
    VRS.$$.MobilePage =                                 'Bewegliche Seite';
    VRS.$$.MobileReportPage =                           'Bewegliche Berichtsseite';
    VRS.$$.Model =                                      'Modell';
    VRS.$$.ModelIcao =                                  'Modell Code';
    VRS.$$.ModeS =                                      'Mode-S';
    VRS.$$.ModeSCountry =                               'Mode-S Land';
    VRS.$$.MovingMap =                                  'Karte verschieben';
    VRS.$$.MuteOff =                                    'Stummschalten aus';
    VRS.$$.MuteOn =                                     'Stummschalten';
    VRS.$$.NauticalMileAbbreviation =                   '{0} nmi';
    VRS.$$.NauticalMiles =                              'Seemeilen';
    VRS.$$.No =                                         'Nein';
    VRS.$$.NoLocalStorage =                             'Dieser Browser unterstützt keinen lokalen Speicher. Ihre Konfigurationseinstellungen werden nicht gespeichert.\n\nWenn Sie im privaten Modus browsen, versuchen Sie diesen abzuschalten. Der private Modus deaktiviert den lokalen Speicher in manchen Browsern.';
    VRS.$$.None =                                       'Leer';
    VRS.$$.Notes =                                      'Notizen';
    VRS.$$.NoSettingsFound =                            'Keine Einstellungen gefunden';
    VRS.$$.NotBetween =                                 'Ist nicht zwischen';
    VRS.$$.NotContains =                                'Enthält nicht';
    VRS.$$.NotEndsWith =                                'Endet nicht mit';
    VRS.$$.NotEquals =                                  'Ist nicht';
    VRS.$$.NotStartsWith =                              'Fängt nicht an mit';
    VRS.$$.OffRadarAction =                             'Wenn Flugzeug außer Reichweite geht:';
    VRS.$$.OffRadarActionWait =                         'De-selektieren des Flugzeugs';
    VRS.$$.OffRadarActionEnableAutoSelect =             'Aktiviere Auto-Selektieren';
    VRS.$$.OffRadarActionNothing =                      'Tue nichts';
    VRS.$$.OfPages =                                    'von {0:N0}';                            // wie in "1 von 10" Seiten
    VRS.$$.OnlyAircraftOnMap =                          'Liste nur sichtbare';
    VRS.$$.OnlyAutoSelected =                           'Nur Details von auto-selektierten Flugzeugen anzeigen';
    VRS.$$.OnlyUsePre22Icons =                          'Zeige Flugzeugmarker nur im alten Stil';
    VRS.$$.Operator =                                   'Betreiber';
    VRS.$$.OperatorCode =                               'Betreibercode';
    VRS.$$.OperatorFlag =                               'Betreiberflagge';
    VRS.$$.Options =                                    'Optionen';
    VRS.$$.OverwriteExistingSettings =                  'Existierene Einstellungen überschreiben';
    VRS.$$.OwnershipStatus =                            'Eigentumsstatus';
    VRS.$$.PageAircraft =                               'Flugzeug';
    VRS.$$.AircraftDetailShort =                        'Detail';
    VRS.$$.PageFirst =                                  'Erste';
    VRS.$$.PageGeneral =                                'Allgemein';
    VRS.$$.PageLast =                                   'Zuletzt';
    VRS.$$.PageList =                                   'Liste';
    VRS.$$.PageListShort =                              'Liste';
    VRS.$$.PageMapShort =                               'Karte';
    VRS.$$.PageNext =                                   'Weiter';
    VRS.$$.PagePrevious =                               'Vorherige';
    VRS.$$.PaneAircraftDisplay =                        'Flugzeuganzeige';
    VRS.$$.PaneAircraftTrails =                         'Flugzeugspur';
    VRS.$$.PaneAudio =                                  'Ton';
    VRS.$$.PaneAutoSelect =                             'Auto-Selektion';
    VRS.$$.PaneCurrentLocation =                        'Aktueller Standort';
    VRS.$$.PaneDataFeed =                               'Datenstrom';
    VRS.$$.PaneDetailSettings =                         'Flugzeug Einzelheiten';
    VRS.$$.PaneInfoWindow =                             'Flugzeuginfofenster';
    VRS.$$.PaneListSettings =                           'Einstellungen auflisten';
    VRS.$$.PaneManyAircraft =                           'Mehrfachbericht Flugzeug';
    VRS.$$.PanePermanentLink =                          'Permanenter Link';
    VRS.$$.PaneRangeCircles =                           'Reichweitenringe';
    VRS.$$.PaneReceiverRange =                          'Empfänger Reichweite';
    VRS.$$.PaneSingleAircraft =                         'Einzelbericht Flugzeug';
    VRS.$$.PaneSortAircraftList =                       'Flugzeugliste sortieren';
    VRS.$$.PaneSortReport =                             'Bericht sortieren';
    VRS.$$.PaneUnits =                                  'Einheiten';
    VRS.$$.Pause =                                      'Pause';
    VRS.$$.PinTextNumber =                              'Flugzeug Beschriftungszeile {0}';
    VRS.$$.PopularName =                                'Bekannter Name';
    VRS.$$.PositionAndAltitude =                        'Position und Höhe';
    VRS.$$.PositionAndSpeed =                           'Position und Geschwindigkeit';
    VRS.$$.Picture =                                    'Bild';
    VRS.$$.PictureOrThumbnails =                        'Bild oder Skizze';
    VRS.$$.PinTextLines =                               'Anzahl Beschriftungszeilen';
    VRS.$$.Piston =                                     'Kolben';
    VRS.$$.Pixels =                                     'Bildpunkte';
    VRS.$$.PoweredByVRS =                               'Powered by Virtual Radar Server';
    VRS.$$.PreviousId =                                 'Vorherige ID';
    VRS.$$.Quantity =                                   'Quantität';
    VRS.$$.RadioMast =                                  'Funkmast';
    VRS.$$.RangeCircleEvenColour =                      'Gerade Kreisfarbe';
    VRS.$$.RangeCircleOddColour =                       'Ungerade Kreisfarbe';
    VRS.$$.RangeCircles =                               'Reichweitenringe';
    VRS.$$.Receiver =                                   'Empfänger';
    VRS.$$.ReceiverRange =                              'Empfänger Reichweite';
    VRS.$$.Refresh =                                    'Aktualisieren';
    VRS.$$.Registration =                               'Registrierung';
    VRS.$$.RegistrationAndIcao =                        'Reg. & Icao';
    VRS.$$.Remove =                                     'Entfernen';
    VRS.$$.RemoveAll =                                  'Alles Entfernen';
    VRS.$$.ReportCallsignInvalid =                      'Callsign Bericht';
    VRS.$$.ReportCallsignValid =                        'Callsign Bericht für {0}';
    VRS.$$.ReportEmpty =                                'Es wurden keine Einträge für die gesetzten Kriterien gefunden';
    VRS.$$.ReportFreeForm =                             'Freiform Bericht';
    VRS.$$.ReportIcaoInvalid =                          'ICAO Bericht';
    VRS.$$.ReportIcaoValid =                            'ICAO Bericht für {0}';
    VRS.$$.ReportRegistrationInvalid =                  'Registrierungsbericht';
    VRS.$$.ReportRegistrationValid =                    'Registrierung Bericht für {0}';
    VRS.$$.ReportTodaysFlights =                        'Flüge Heute';
    VRS.$$.ReportYesterdaysFlights =                    'Flüge Gestern';
    VRS.$$.Reports =                                    'Berichte';
    VRS.$$.ReportsAreDisabled =                         'Eingeschränkte Berechtigungen verhindern die Berichtserstellung';
    VRS.$$.Resume =                                     'Wiederaufnehmen';
    VRS.$$.Reversing =                                  'Umkehren';
    VRS.$$.ReversingShort =                             'REV';
    VRS.$$.Route =                                      'Route';
    VRS.$$.RouteShort =                                 'Route (kurz)';
    VRS.$$.RouteFull =                                  'Route (vollst.)';
    VRS.$$.RouteNotKnown =                              'Route unbekannt';
    VRS.$$.RowNumber =                                  'Zeilennummer';
    VRS.$$.Rows =                                       'Zeilen';
    VRS.$$.RunReport =                                  'Bericht erstellen';
    VRS.$$.SeaPlane =                                   'Seaplane';
    VRS.$$.Select =                                     'Auswählen';
    VRS.$$.SeparateTwoValues =                          ' und ';
    VRS.$$.SerialNumber =                               'Seriennummer';
    VRS.$$.ServerFetchFailedTitle =                     'Abrufen fehlgeschlagen';
    VRS.$$.ServerFetchFailedBody =                      'Konnte vom Server nicht abfragen. Der Fehler ist "{0}" und der Status ist "{1}"';
    VRS.$$.ServerFetchTimedOut =                        'Zeitüberschreitung der Anforderung.';
    VRS.$$.ServerReportExceptionBody =                  'Ausnahmefehler (Server) bei der Erzeugung des Berichts. Ausnahmefehler ist "{0}";';
    VRS.$$.ServerReportExceptionTitle =                 '"Server Exception"';
    VRS.$$.SetCurrentLocation =                         'Setze aktuellen Standort';
    VRS.$$.Settings =                                   'Einstellungen';
    VRS.$$.SettingsPage =                               'Einstellungen Seite';
    VRS.$$.Shortcuts =                                  'Abkürzungen';
    VRS.$$.ShowAltitudeStalk =                          'Anzeige vertikale Höhenmarkierung';
    VRS.$$.ShowAltitudeType =                           'Anzeige Höhe';
    VRS.$$.ShowCurrentLocation =                        'Anzeige aktueller Standort';
    VRS.$$.ShowDetail =                                 'Anzeige Detail';
    VRS.$$.ShowForAllAircraft =                         'Anzeige für alle Flugzeuge';
    VRS.$$.ShowEmptyValues =                            'Anzeige leere Werte';
    VRS.$$.ShowForSelectedOnly =                        'Nur für das ausgewählte Flugzeug anzeigen';
    VRS.$$.ShowRangeCircles =                           'Anzeige Reichweitenkreise';
    VRS.$$.ShowShortTrails =                            'Anzeige kurze Spur';
    VRS.$$.ShowSpeedType =                              'Anzeige Geschwindigkeit';
    VRS.$$.ShowTrackType =                              'Anzeige Kurs';
    VRS.$$.ShowUnits =                                  'Anzeige Einheiten';
    VRS.$$.ShowVerticalSpeedType =                      'Anzeige Steig-/Sinkraten';
    VRS.$$.ShowVsiInSeconds =                           'Anzeige Steig-/Sinkrate pro Sekunde';
    VRS.$$.SignalLevel =                                'Signalpegel';
    VRS.$$.Silhouette =                                 'Silhouette';
    VRS.$$.SilhouetteAndOpFlag =                        'Sil. & Betr. Flagge';
    VRS.$$.SiteTimedOut =                               'Wegen Inaktivität wurde die Seite angehalten. Schließen Sie diese Nachricht um Aktualisierungen wieder aufzunehmen.';
    VRS.$$.SortBy =                                     'Sortieren nach';
    VRS.$$.Species =                                    'Art/Typ';
    VRS.$$.Speed =                                      'Geschwindigkeit';
    VRS.$$.SpeedGraph =                                 'Geschwindigkeitsgraph';
    VRS.$$.Speeds =                                     'Geschwindigkeiten';
    VRS.$$.SpeedType =                                  'Geschwindigkeitstyp';
    VRS.$$.Squawk =                                     '"Squawk"';
    VRS.$$.Squawk7000 =                                 'Kein "Squawk" zugewiesen';
    VRS.$$.Squawk7500 =                                 'Flugzeugentführung';
    VRS.$$.Squawk7600 =                                 'Funkausfall durchgeben';
    VRS.$$.Squawk7700 =                                 'Allgemeiner Notfall';
    VRS.$$.Start =                                      'Start';
    VRS.$$.StartsWith =                                 'Startet mit';
    VRS.$$.StartTime =                                  'Startzeit';
    VRS.$$.Status =                                     'Status';
    VRS.$$.StatuteMileAbbreviation =                    '{0} mi';
    VRS.$$.StatuteMiles =                               'Britische Meilen (statute miles)';
    VRS.$$.StorageEngine =                              'Speichermaschine';
    VRS.$$.StorageSize =                                'Speichergröße';
    VRS.$$.StrokeOpacity =                              'Strich-Opazität';
    VRS.$$.SubmitRoute =                                'Route übermitteln';
    VRS.$$.SubmitRouteCorrection =                      'Routenkorrektur übermitteln';
    VRS.$$.SuppressAltitudeStalkWhenZoomedOut =         'Ausblenden der vertikalen Höhenmarkierung beim Herauszoomen';
    VRS.$$.TargetAltitude =                             'Zielhöhe';
    VRS.$$.TargetHeading =                              'Zielkurs';
    VRS.$$.ThenBy =                                     'dann nach';
    VRS.$$.Tiltwing =                                   'Kippflügel';
    VRS.$$.TimeTracked =                                'Zeitdauer der Rückverfolgung';
    VRS.$$.TitleAircraftDetail =                        'Flugzeugdetail';
    VRS.$$.TitleAircraftList =                          'Flugzeugliste';
    VRS.$$.TitleFlightDetail =                          'Detail';
    VRS.$$.TitleFlightsList =                           'Flüge';
    VRS.$$.ToAltitude =                                 'bis {0}';
    VRS.$$.TitleSiteTimedOut =                          'Abgelaufen';
    VRS.$$.TotalHours =                                 'Stunden Total';
    VRS.$$.TrackingCountAircraft =                      'Verfolge {0:N0} Flugzeuge';
    VRS.$$.TrackingCountAircraftOutOf =                 'Verfolge {0:N0} Flugzeuge (aus {1:N0})';
    VRS.$$.TrackingOneAircraft =                        'Verfolge 1 Flugzeug';
    VRS.$$.TrackingOneAircraftOutOf =                   'Verfolge 1 Flugzeug (aus {0:N0})';
    VRS.$$.TransponderType =                            'Transponder';
    VRS.$$.TransponderTypeFlag =                        'Transponderflagge';
    VRS.$$.TrueAirSpeed =                               'Wahr';
    VRS.$$.TrueAirSpeedShort =                          'TAS';                          // halten Sie dies kurz, eine Abkürzung wenn möglich.
    VRS.$$.TrueHeading =                                'Tatsächlicher Kurs';
    VRS.$$.TrueHeadingShort =                           'Wahr';
    VRS.$$.Turbo =                                      'Turbo';
    VRS.$$.Unknown =                                    'Unbekannt';
    VRS.$$.UseBrowserLocation =                         'GPS Standort nutzen';
    VRS.$$.UseRelativeDates =                           'Relative Daten nutzen';
    VRS.$$.UserTag =                                    'Benutzer "Tag"';
    VRS.$$.VerticalSpeed =                              'Steig-/Sinkrate';
    VRS.$$.VerticalSpeedType =                          'Steig-/Sinkratenart';
    VRS.$$.VirtualRadar =                               'Virtual Radar';
    VRS.$$.Volume25 =                                   'Lautstärke 25 %';
    VRS.$$.Volume50 =                                   'Lautstärke 50%';
    VRS.$$.Volume75 =                                   'Lautstärke 75%';
    VRS.$$.Volume100 =                                  'Lautstärke 100%';
    VRS.$$.VrsVersion =                                 'Version {0}';
    VRS.$$.WakeTurbulenceCategory =                     'Wirbelschleppen';
    VRS.$$.Warning =                                    'Warnung';
    VRS.$$.WorkingInOfflineMode =                       'Arbeitet im Offline Modus';
    VRS.$$.WtcLight =                                   'Licht';
    VRS.$$.WtcMedium =                                  'Medium';
    VRS.$$.WtcHeavy =                                   'Schwer';
    VRS.$$.YearBuilt =                                  'Baujahr';
    VRS.$$.Yes =                                        'Ja';

    // Date picker text
    VRS.$$.DateClose =                                  'Erledigt';                         // kurz halten
    VRS.$$.DateCurrent =                                'Heute';                        // kurz halten
    VRS.$$.DateNext =                                   'Nächste';                         // kurz halten
    VRS.$$.DatePrevious =                               'Vorherige';                         // kurz halten
    VRS.$$.DateWeekAbbr =                               'Wo.';                           // sehr kurz halten
    VRS.$$.DateYearSuffix =                             '';                             // Dies ist nach dem Jahr dargestellt
    // If your language has a different month format when days preceed months, and the date picker
    // should be using that month format, then set this to true. Otherwise leave at false.
    VRS.$$.DateUseGenetiveMonths =                      false;

    // Text-to-speech formatting
    VRS.$$.SayCallsign =                                'Funkrufzeichen (Call sign) {0}.';
    VRS.$$.SayHyphen =                                  'Bindestrich';
    VRS.$$.SayIcao =                                    'eye co {0}.';
    VRS.$$.SayModelIcao =                               'Typ {0}.';
    VRS.$$.SayOperator =                                'Betrieben durch {0}.';
    VRS.$$.SayRegistration =                            'Registrierung {0}.';
    VRS.$$.SayRouteNotKnown =                           'Route unbekannt.';
    VRS.$$.SayFromTo =                                  'Fliegt von {0} nach {1}.';
    VRS.$$.SayFromToVia =                               'Fliegt von {0} über {1} nach {2}.';

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
        See the notes below against 'Komplizierte Zeichenketten'. Diese Funktion nimmt eine Matrix von Zwischenstationen in eine Route auf und
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

            if(isLastStopover)        result += ' und ';
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
            case 'C':       result = 'Gekoppelt'; break;
            case '1':       result = 'Einzel'; break;
            case '2':       result = 'Twin'; break;
            case '3':       result = 'Drei'; break;
            case '4':       result = 'Vier'; break;
            case '5':       result = 'Fünf'; break;
            case '6':       result = 'Sechs'; break;
            case '7':       result = 'Sieben'; break;
            case '8':       result = 'Acht'; break;
            default:        result = countEngines; break;
        }

        switch(engineType) {
            case VRS.EngineType.Electric:   result += ' Elektrisch'; break;
            case VRS.EngineType.Jet:        result += ' jet'; break;
            case VRS.EngineType.Piston:     result += ' Kolben'; break;
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
                case VRS.WakeTurbulenceCategory.None:   if(!ignoreNone) result = 'keine'; break;
                case VRS.WakeTurbulenceCategory.Light:  result = 'Leicht'; break;
                case VRS.WakeTurbulenceCategory.Medium: result = 'Mittel'; break;
                case VRS.WakeTurbulenceCategory.Heavy:  result = 'Schwer'; break;
                default: throw 'Unknown wake turbulence category ' + category;  // Do not translate this line
            }

            if(expandedDescription && result) {
                switch(category) {
                    case VRS.WakeTurbulenceCategory.Light:  result += ' (bis 7 Tonnen)'; break;
                    case VRS.WakeTurbulenceCategory.Medium: result += ' (bis 135 Tonnen)'; break;
                    case VRS.WakeTurbulenceCategory.Heavy:  result += ' (über 135 Tonnen)'; break;
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
        if(from) result = 'Von ' + from;
        if(to) {
            if(result.length) result += ' nach ';
            else              result = 'Nach ';
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
