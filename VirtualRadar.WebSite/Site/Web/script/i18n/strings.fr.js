// Copyright © 2013 onwards, Andrew Whewell and Francois /F5ANN.
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

    VRS.$$.Add =                                        'Ajoute';
    VRS.$$.AddCondition =                               'Condition';
    VRS.$$.AddCriteria =                                'Critère';
    VRS.$$.AddFilter =                                  'Filtre';
    VRS.$$.ADSB =                                       'ADS-B';  /** THIS IS NEW! **/
    VRS.$$.ADSB0 =                                      'ADS-B v0';  /** THIS IS NEW! **/
    VRS.$$.ADSB1 =                                      'ADS-B v1';  /** THIS IS NEW! **/
    VRS.$$.ADSB2 =                                      'ADS-B v2';  /** THIS IS NEW! **/
    VRS.$$.AircraftNotTransmittingCallsign =            'Cet avion ne transmet pas de Callsign';
    VRS.$$.AircraftClass =                              'Classe Avion';
    VRS.$$.Airport =                                    'Aéroport';
    VRS.$$.AirportDataThumbnails =                      'Vignettes (airport-data.com)';
    VRS.$$.AllAltitudes =                               'Toutes les altitudes';
    VRS.$$.AllRows =                                    'Toutes les lignes';
    VRS.$$.Altitude =                                   'Altitude';
    VRS.$$.AltitudeType =                               'Altitude Type';  /** THIS IS NEW! **/
    VRS.$$.AltitudeAndSpeedGraph =                      'Graphe Altitude & vitesse';
    VRS.$$.AltitudeAndVerticalSpeed =                   'Altitude & VSI';
    VRS.$$.AltitudeGraph =                              'Graphe Altitude';
    VRS.$$.AltitudeType =                               'Altitude Type';  /** THIS IS NEW! **/
    VRS.$$.AllAircraft =                                'Liste tout';
    VRS.$$.Amphibian =                                  'Amphibian';
    VRS.$$.AnnounceSelected =                           'Détails avion sélectionné';
    VRS.$$.Ascending =                                  'Ascendant';
    VRS.$$.AutoSelectAircraft =                         'Auto-select Avion';
    VRS.$$.AverageSignalLevel =                         'Niveau moyen Signal';
    VRS.$$.Barometric =                                 'Barometric';  /** THIS IS NEW! **/
    VRS.$$.Bearing =                                    'Cap';
    VRS.$$.Between =                                    'Entre';
    VRS.$$.Callsign =                                   'Callsign';
    VRS.$$.CallsignAndShortRoute =                      'Callsign & Route';
    VRS.$$.CallsignMayNotBeCorrect =                    'Callsign peut être incorrect';
    VRS.$$.CentreOnSelectedAircraft =                   'Montre sur la carte';
    VRS.$$.Civil =                                      'Civil';
    VRS.$$.CivilOrMilitary =                            'Civil / Militaire';
    VRS.$$.ClosestToCurrentLocation =                   'Plus près de';
    VRS.$$.CofACategory =                               'C/A Categorie';  // certificate of airworthiness category
    VRS.$$.CofAExpiry =                                 'C/A Expiré';  // certificate of airworthiness expiry
    VRS.$$.Columns =                                    'Colonnes';
    VRS.$$.Contains =                                   'Contains';
    VRS.$$.CountAdsb =                                  'Nombre de messages ADS-B';
    VRS.$$.Country =                                    'Pays';
    VRS.$$.CountModeS =                                 'Nombre de messages Mode-S';
    VRS.$$.CountPositions =                             'Nombre de messages de Position';
    VRS.$$.Criteria =                                   'Critères';
    VRS.$$.CurrentLocationInstruction =                 'Pour entrer votre position, cliquez "voir position courante" et déplacez le marqueur.';
    VRS.$$.CurrentRegDate =                             'Current Reg. Date';
    VRS.$$.Date =                                       'Date';
    VRS.$$.DateTimeShort =                              '{0} {1}';  // Where "{0}" is a date, e.g. 10/10/2013; and "{1}" is a time, e.g. 17:41:32.
    VRS.$$.DefaultSetting =                             '< Defaut >';
    VRS.$$.DegreesAbbreviation =                        '{0}°';
    VRS.$$.DeRegDate =                                  'De-reg. Date';
    VRS.$$.DesktopPage =                                'Desktop Page';
    VRS.$$.DesktopReportPage =                          'Desktop  Page Rapport';
    VRS.$$.DetailItems =                                'Elements de détails avion';
    VRS.$$.DetailPanel =                                'Panneau détail';
    VRS.$$.DisableAutoSelect =                          'Désactiver auto-select';
    VRS.$$.Distance =                                   'Distance';
    VRS.$$.Distances =                                  'Distances';
    VRS.$$.DoNotShow =                                  'Ne pas montrer';
    VRS.$$.Duration =                                   'Durée';
    VRS.$$.Electric =                                   'Electrique';
    VRS.$$.EnableAutoSelect =                           'Valider auto-select';
    VRS.$$.EnableFilters =                              'Valider filtres';
    VRS.$$.EnableInfoWindow =                           'Fenêtre infos';
    VRS.$$.End =                                        'Fin';
    VRS.$$.EndTime =                                    'Fin durée';
    VRS.$$.EndsWith =                                   'Termine avec';
    VRS.$$.Engines =                                    'Moteurs';
    VRS.$$.EngineType =                                 'Type moteurs';
    VRS.$$.Equals =                                     'Est';
    VRS.$$.Feet =                                       'Pieds';
    VRS.$$.FeetAbbreviation =                           '{0} ft';
    VRS.$$.FeetPerMinuteAbbreviation =                  '{0} ft/m';
    VRS.$$.FeetPerSecondAbbreviation =                  '{0} ft/s';
    VRS.$$.FetchPage =                                  'Rapporter';
    VRS.$$.FillOpacity =                                'Fill opacity';  /** THIS IS NEW! **/
    VRS.$$.Filters =                                    'Filtres';
    VRS.$$.FindAllPermutationsOfCallsign =              'Trouver toutes les permutations de callsign';
    VRS.$$.FirstAltitude =                              'Altitude en premier';
    VRS.$$.FirstHeading =                               'Cap en premier';
    VRS.$$.FirstFlightLevel =                           'Premier FL';
    VRS.$$.FirstLatitude =                              'Latitude en premier';
    VRS.$$.FirstLongitude =                             'Longitude en premier';
    VRS.$$.FirstOnGround =                              'Au sol en premier';
    VRS.$$.FirstRegDate =                               'Reg.Date en premier';
    VRS.$$.FirstSpeed =                                 'Vitesse en premier';
    VRS.$$.FirstSquawk =                                'Squawk en premier';
    VRS.$$.FirstVerticalSpeed =                         'Vitesse verticalen en premier';
    VRS.$$.FlightDetailShort =                          'Detail';
    VRS.$$.FlightLevel =                                'Niveayu de vol';
    VRS.$$.FlightLevelAbbreviation =                    'FL{0}';
    VRS.$$.FlightLevelAndVerticalSpeed =                'FL & VSI';
    VRS.$$.FlightLevelHeightUnit =                      'Unité de hauteur niveau de vol';
    VRS.$$.FlightLevelTransitionAltitude =              'Altitude de transition';
    VRS.$$.FlightsCount =                               'Nombre de Vols';
    VRS.$$.FlightsListShort =                           'Vols';
    VRS.$$.FlightSimPage =                              'Page Flight Sim ';
    VRS.$$.FlightSimTitle =                             'Virtual Radar - FSX';
    VRS.$$.ForcePhoneOff =                              'Non Smartphone';  // As in "force the page to ignore the fact that this is a smart phone"
    VRS.$$.ForcePhoneOn =                               'Smartphone';  // As in "force the page to pretend that this is a smart phone"
    VRS.$$.ForceTabletOff =                             'Non tablette';  // As in "force the page to ignore the fact that this is a tablet PC"
    VRS.$$.ForceTabletOn =                              'Tablette';  // As in "force the page to use the settings for a tablet PC"
    VRS.$$.FromAltitude =                               'De {0}';
    VRS.$$.FromToAltitude =                             '{0} to {1}';
    VRS.$$.FromToDate =                                 '{0} to {1}';
    VRS.$$.FromToFlightLevel =                          '{0} to {1}';
    VRS.$$.FromToSpeed =                                '{0} to {1}';
    VRS.$$.FromToSquawk =                               '{0} to {1}';
    VRS.$$.FurthestFromCurrentLocation =                'La plus éloignée de position actuelle';
    VRS.$$.GenericName =                                'Nom généric';
    VRS.$$.Geometric =                                  'Geometric';  /** THIS IS NEW! **/
    VRS.$$.GeometricAltitudeIndicator =                 'GPS';  /** THIS IS NEW! **/  // A ** SHORT ** indication that the reported altitude is geometric (i.e. usually coming from a GPS unit) as opposed to barometric (i.e. coming off one or more pressure sensors).
    VRS.$$.GoogleMapsCouldNotBeLoaded =                 'Google Maps ne peut être chargé';
    VRS.$$.GotoCurrentLocation =                        'Vers position courante';
    VRS.$$.GotoSelectedAircraft =                       'Vers avion sélectionné';
    VRS.$$.GroundAbbreviation =                         'GND';
    VRS.$$.Ground =                                     'Ground';  /** THIS IS NEW! **/
    VRS.$$.GroundTrack =                                'Ground track';  /** THIS IS NEW! **/
    VRS.$$.GroundVehicle =                              'Vehicule terrestre';
    VRS.$$.Gyrocopter =                                 'Gyrocopter';
    VRS.$$.HadAlert =                                   'A eu Alerte';
    VRS.$$.HadEmergency =                               'A eu urgence';
    VRS.$$.HadSPI =                                     'A eu Ident';  // SPI is the name of a pulse in Mode-S, used when ATC has asked for ident from aircraft.
    VRS.$$.Heading =                                    'Cap';
    VRS.$$.HeadingType =                                'Heading Type';  /** THIS IS NEW! **/
    VRS.$$.Heights =                                    'Hauteur';
    VRS.$$.Helicopter =                                 'Helicoptère';
    VRS.$$.Help =                                       'Aide';
    VRS.$$.HideAircraftNotOnMap =                       'Masquer avion non sur la carte';
    VRS.$$.HideEmptyPinTextLines =                      'Caher les lignes de label vides';
    VRS.$$.HideNoPosition =                             'Position';
    VRS.$$.HighContrastMap =                            'Contraste';  // <-- please try to keep this one short, it appears as a button on the map and there may not be a lot of room
    VRS.$$.Icao =                                       'ICAO';
    VRS.$$.Index =                                      'Index';
    VRS.$$.IsMilitary =                                 'Militaire';
    VRS.$$.IndicatedAirSpeed =                          'Indicated';  /** THIS IS NEW! **/
    VRS.$$.IndicatedAirSpeedShort =                     'IAS';  /** THIS IS NEW! **/  // <-- please try to keep this short, an abbreviation if possible
    VRS.$$.Interesting =                                'Interessant';
    VRS.$$.IntervalSeconds =                            'Interval (secs)';
    VRS.$$.Jet =                                        'Jet';
    VRS.$$.JustPositions =                              'Positions';
    VRS.$$.KilometreAbbreviation =                      '{0} km';
    VRS.$$.Kilometres =                                 'Kilometres';
    VRS.$$.KilometresPerHour =                          'Kilometres/Hour';
    VRS.$$.KilometresPerHourAbbreviation =              '{0} km/h';
    VRS.$$.Knots =                                      'Noeuds';
    VRS.$$.KnotsAbbreviation =                          '{0} kts';
    VRS.$$.LandPlane =                                  'Landplane';
    VRS.$$.LastAltitude =                               'Dernier Altitude';
    VRS.$$.LastFlightLevel =                            'Dernier FL';
    VRS.$$.LastHeading =                                'Dernier Cap';
    VRS.$$.LastOnGround =                               'Dernier Au sol';
    VRS.$$.LastLatitude =                               'Dernier Latitude';
    VRS.$$.LastLongitude =                              'Dernier Longitude';
    VRS.$$.LastSpeed =                                  'Dernier Vitesse';
    VRS.$$.LastSquawk =                                 'Dernier Squawk';
    VRS.$$.LastVerticalSpeed =                          'Dernier Vitesse verticale';
    VRS.$$.Latitude =                                   'Latitude';
    VRS.$$.Layout =                                     'Layout';
    VRS.$$.Layout1 =                                    'Classic';
    VRS.$$.Layout2 =                                    'Detail, Haut de la carte';
    VRS.$$.Layout3 =                                    'Detail, Bas de la carte';
    VRS.$$.Layout4 =                                    'Liste, Haut de la carte';
    VRS.$$.Layout5 =                                    'Liste, Bas de la carte';
    VRS.$$.Layout6 =                                    'Detail and List';
    VRS.$$.ListAircraftClass =                          'Classe avion';
    VRS.$$.ListAirportDataThumbnails =                  'Vignettes (airport-data.com)';
    VRS.$$.ListAltitude =                               'Alt.';
    VRS.$$.ListAltitudeType =                           'Alt. Type';  /** THIS IS NEW! **/
    VRS.$$.ListAltitudeAndVerticalSpeed =               'Alt & VSI';
    VRS.$$.ListAverageSignalLevel =                     'Signal moy.';
    VRS.$$.ListBearing =                                'Cap.';
    VRS.$$.ListCallsign =                               'Callsign';
    VRS.$$.ListCivOrMil =                               'Civ/Mil';
    VRS.$$.ListCofACategory =                           'Cat.avion';  // Certificate of airworthiness category
    VRS.$$.ListCofAExpiry =                             'C/A Expiré';  // Certificate of airworthiness expiry
    VRS.$$.ListCountAdsb =                              'Msgs ADS-B .';
    VRS.$$.ListCountMessages =                          'Msgs.';
    VRS.$$.ListCountModeS =                             'Msgs Mode-S';
    VRS.$$.ListCountPositions =                         'Msgs Pos.';
    VRS.$$.ListCountry =                                'Pays';
    VRS.$$.ListCurrentRegDate =                         'Reg. actuelle';  // Date of current registration
    VRS.$$.ListDeRegDate =                              'Date De-reg';  // as in the date it was taken off the register
    VRS.$$.ListDistance =                               'Distance';
    VRS.$$.ListDuration =                               'Durée';
    VRS.$$.ListEndTime =                                'Dernier Message';  // As in the date and time of the last message.
    VRS.$$.ListEngines =                                'Moteurs';
    VRS.$$.ListFirstAltitude =                          'De Alt.';
    VRS.$$.ListFirstFlightLevel =                       'Du FL';
    VRS.$$.ListFirstHeading =                           'Du Cap.';
    VRS.$$.ListFirstLatitude =                          'De Lat.';
    VRS.$$.ListFirstLongitude =                         'De Lng.';
    VRS.$$.ListFirstOnGround =                          'De au sol.';
    VRS.$$.ListFirstRegDate =                           'Première Reg.';  // Date of first registration
    VRS.$$.ListFirstSpeed =                             'De vitesse';
    VRS.$$.ListFirstSquawk =                            'De Squawk';
    VRS.$$.ListFirstVerticalSpeed =                     'De VSI';
    VRS.$$.ListFlightLevel =                            'FL';
    VRS.$$.ListFlightLevelAndVerticalSpeed =            'FL & VSI';
    VRS.$$.ListFlightsCount =                           'Vu';
    VRS.$$.ListGenericName =                            'Nom Généric';
    VRS.$$.ListHadAlert =                               'Alerte';
    VRS.$$.ListHadEmergency =                           'Urgence';
    VRS.$$.ListHadSPI =                                 'SPI';  // Name of a pulse in Mode-S, may not need translation. Used when ATC has asked for ident from aircraft.
    VRS.$$.ListHeading =                                'Cap.';
    VRS.$$.ListHeadingType =                            'Hdg. Type';  /** THIS IS NEW! **/
    VRS.$$.ListIcao =                                   'ICAO';
    VRS.$$.ListInteresting =                            'Interessant';
    VRS.$$.ListLastAltitude =                           'A Alt.';
    VRS.$$.ListLastFlightLevel =                        'Au FL';
    VRS.$$.ListLastHeading =                            'A Cap.';
    VRS.$$.ListLastLatitude =                           'A Lat.';
    VRS.$$.ListLastLongitude =                          'A Lng.';
    VRS.$$.ListLastOnGround =                           'A au sol';
    VRS.$$.ListLastSpeed =                              'A Vitesse';
    VRS.$$.ListLastSquawk =                             'A Squawk';
    VRS.$$.ListLastVerticalSpeed =                      'A VSI';
    VRS.$$.ListLatitude =                               'Lat.';
    VRS.$$.ListLongitude =                              'Lng.';
    VRS.$$.ListNotes =                                  'Notes';
    VRS.$$.ListManufacturer =                           'Constructeur';
    VRS.$$.ListMaxTakeoffWeight =                       'Poids Max T/O';
    VRS.$$.ListModel =                                  'Model';
    VRS.$$.ListModelIcao =                              'Type';
    VRS.$$.ListModeSCountry =                           'Pays Mode-S';
    VRS.$$.ListModelSilhouette =                        'Silhouette';
    VRS.$$.ListModelSilhouetteAndOpFlag =               'Logos';
    VRS.$$.ListOperator =                               'Opérateur';
    VRS.$$.ListOperatorFlag =                           'Logo';
    VRS.$$.ListOperatorIcao =                           'Code Op.';
    VRS.$$.ListOwnershipStatus =                        'Status propriétaire';
    VRS.$$.ListPicture =                                'Image';
    VRS.$$.ListPopularName =                            'Nom usuel';
    VRS.$$.ListPreviousId =                             'ID précédente';
    VRS.$$.ListReceiver =                               'Récepteur';
    VRS.$$.ListRegistration =                           'Reg.';
    VRS.$$.ListRowNumber =                              '';
    VRS.$$.ListRoute =                                  'Route';
    VRS.$$.ListSerialNumber =                           'Série';
    VRS.$$.ListSignalLevel =                            'Signal';
    VRS.$$.ListSpecies =                                'Specs.';
    VRS.$$.ListSpeed =                                  'Vitesse';
    VRS.$$.ListSpeedType =                              'Speed Type';  /** THIS IS NEW! **/
    VRS.$$.ListSquawk =                                 'Squawk';
    VRS.$$.ListStartTime =                              'Premier Message';
    VRS.$$.ListStatus =                                 'Statuts';
    VRS.$$.ListTargetAltitude =                         'A/P Alt.';  /** THIS IS NEW! **/
    VRS.$$.ListTargetHeading =                          'A/P Hdg.';  /** THIS IS NEW! **/
    VRS.$$.ListTotalHours =                             'Heures totales';
    VRS.$$.ListTransponderType =                        'Transponder';  /** THIS IS NEW! **/
    VRS.$$.ListTransponderTypeFlag =                    '';  /** THIS IS NEW! **/
    VRS.$$.ListUserTag =                                'Tag';
    VRS.$$.ListVerticalSpeed =                          'Vitesse Vert.';
    VRS.$$.ListVerticalSpeedType =                      'V.Speed Type';  /** THIS IS NEW! **/
    VRS.$$.ListWtc =                                    'WTC';
    VRS.$$.ListYearBuilt =                              'Construit';
    VRS.$$.Longitude =                                  'Longitude';
    VRS.$$.Manufacturer =                               'Constructeur';
    VRS.$$.Map =                                        'Carte';
    VRS.$$.MaxTakeoffWeight =                           'Poids Total décolage Max';
    VRS.$$.Menu =                                       'Menu';
    VRS.$$.MenuBack =                                   'retour';
    VRS.$$.MessageCount =                               'Messages';
    VRS.$$.MetreAbbreviation =                          '{0} m';
    VRS.$$.MetrePerSecondAbbreviation =                 '{0} m/sec';
    VRS.$$.MetrePerMinuteAbbreviation =                 '{0} m/min';
    VRS.$$.Metres =                                     'Metres';
    VRS.$$.MilesPerHour =                               'Miles/Hour';
    VRS.$$.MilesPerHourAbbreviation =                   '{0} mph';
    VRS.$$.Military =                                   'Militaire';
    VRS.$$.MobilePage =                                 'Page Mobile ';
    VRS.$$.MobileReportPage =                           'Rapport Page Mobile';
    VRS.$$.Model =                                      'Model';
    VRS.$$.ModelIcao =                                  'Model Code';
    VRS.$$.ModeS =                                      'Mode-S';  /** THIS IS NEW! **/
    VRS.$$.ModeSCountry =                               'Pays Mode-S';
    VRS.$$.MovingMap =                                  'Déplacer la carte';
    VRS.$$.MuteOff =                                    'Mute Off';
    VRS.$$.MuteOn =                                     'Mute';
    VRS.$$.NauticalMileAbbreviation =                   '{0} nmi';
    VRS.$$.NauticalMiles =                              'Miles nautiques';
    VRS.$$.No =                                         'Non';
    VRS.$$.NoLocalStorage =                             'Cet explorateur ne supporte pas la sauvegarde locale.Votre configuration est pas sauvegardée.\n\nIf vous êtes en mode "Private Mode" essayer de le désactiver. Le mode Privé désactive la sauvegarde local sur certains explorateurs.';
    VRS.$$.None =                                       'Aucun';
    VRS.$$.Notes =                                      'Notes';
    VRS.$$.NoSettingsFound =                            'Pas de config.';
    VRS.$$.NotBetween =                                 'Est pas entre';
    VRS.$$.NotContains =                                'Ne contient pas';
    VRS.$$.NotEndsWith =                                'Ne finit pas par';
    VRS.$$.NotEquals =                                  'Est pas';
    VRS.$$.NotStartsWith =                              'Ne commence pas par';
    VRS.$$.OffRadarAction =                             'Quand avion est hors de portée:';
    VRS.$$.OffRadarActionWait =                         'Désélectionner avion';
    VRS.$$.OffRadarActionEnableAutoSelect =             'Sélection automatique';
    VRS.$$.OffRadarActionNothing =                      'Ne rien faire';
    VRS.$$.OfPages =                                    'de {0:N0}';  // As in "1 of 10" pages
    VRS.$$.OnlyAircraftOnMap =                          'Liste si visible';
    VRS.$$.OnlyAutoSelected =                           'Annonce détails avion auto-selectionné';
    VRS.$$.Operator =                                   'Operateur';
    VRS.$$.OperatorCode =                               'Code Operateur';
    VRS.$$.OperatorFlag =                               'Logo Operateur';
    VRS.$$.Options =                                    'Options';
    VRS.$$.OwnershipStatus =                            'Status propriétaire';
    VRS.$$.PageAircraft =                               'Avion';
    VRS.$$.AircraftDetailShort =                        'Detail';
    VRS.$$.PageFirst =                                  'Premier';
    VRS.$$.PageGeneral =                                'Général';
    VRS.$$.PageLast =                                   'Dernier';
    VRS.$$.PageList =                                   'Liste';
    VRS.$$.PageListShort =                              'Liste';
    VRS.$$.PageMapShort =                               'Carte';
    VRS.$$.PageNext =                                   'Suivant';
    VRS.$$.PagePrevious =                               'Précédent';
    VRS.$$.PaneAircraftDisplay =                        'Affichage avion';
    VRS.$$.PaneAircraftTrails =                         'Trace avion';
    VRS.$$.PaneAudio =                                  'Audio';
    VRS.$$.PaneAutoSelect =                             'Auto-selection';
    VRS.$$.PaneCurrentLocation =                        'Position Courrante';
    VRS.$$.PaneDataFeed =                               'Flux de données';
    VRS.$$.PaneDetailSettings =                         'Détails avion';
    VRS.$$.PaneInfoWindow =                             'Infos avion';
    VRS.$$.PaneListSettings =                           'Liste config.';
    VRS.$$.PaneManyAircraft =                           'Rapport de plusieurs avions';
    VRS.$$.PanePermanentLink =                          'Lien Permanent ';
    VRS.$$.PaneRangeCircles =                           'Cercles de distance';
    VRS.$$.PaneReceiverRange =                          'Receiver Range';  /** THIS IS NEW! **/
    VRS.$$.PaneSingleAircraft =                         'Rapport avion unique';
    VRS.$$.PaneSortAircraftList =                       'Ordre liste avions';
    VRS.$$.PaneSortReport =                             'Ordre rapport';
    VRS.$$.PaneUnits =                                  'Unités';
    VRS.$$.Pause =                                      'Pause';
    VRS.$$.PinTextNumber =                              'Ligne label avion {0}';
    VRS.$$.PopularName =                                'Nom usuel';
    VRS.$$.PositionAndAltitude =                        'Position et altitude';
    VRS.$$.PositionAndSpeed =                           'Position et vitesse';
    VRS.$$.Picture =                                    'Image';
    VRS.$$.PictureOrThumbnails =                        'Image ou vignettes';
    VRS.$$.PinTextLines =                               'Nombre de lignes de l étiquette';
    VRS.$$.Piston =                                     'Piston';
    VRS.$$.Pixels =                                     'pixels';
    VRS.$$.PoweredByVRS =                               'Powered by Virtual Radar Server';
    VRS.$$.PreviousId =                                 'ID précédent';
    VRS.$$.Quantity =                                   'Quantité';
    VRS.$$.RadioMast =                                  'Mât radio';
    VRS.$$.RangeCircleEvenColour =                      'Couleur cercle pair';
    VRS.$$.RangeCircleOddColour =                       'Couleur cercle impair';
    VRS.$$.RangeCircles =                               'Cercles de couverure';
    VRS.$$.Receiver =                                   'Récepteur';
    VRS.$$.ReceiverRange =                              'Couverture du récepteur';
    VRS.$$.Refresh =                                    'Raffraichir';
    VRS.$$.Registration =                               'Immatriculation';
    VRS.$$.RegistrationAndIcao =                        'Reg. & Icao';
    VRS.$$.Remove =                                     'Oter';
    VRS.$$.RemoveAll =                                  'Oter tous';
    VRS.$$.ReportCallsignInvalid =                      'Rapport Callsign';
    VRS.$$.ReportCallsignValid =                        'Rapport Callsign pour {0}';
    VRS.$$.ReportEmpty =                                'Auncun enregistrement trouvé sur ce critère';
    VRS.$$.ReportFreeForm =                             'Rapport Free-form';
    VRS.$$.ReportIcaoInvalid =                          'Rapport ICAO';
    VRS.$$.ReportIcaoValid =                            'Rapport ICAO pour {0}';
    VRS.$$.ReportRegistrationInvalid =                  'Rapport Registration invalide';
    VRS.$$.ReportRegistrationValid =                    'Rapport Registration pour {0}';
    VRS.$$.ReportTodaysFlights =                        'Vols pour aujourd\'hui';
    VRS.$$.ReportYesterdaysFlights =                    'Vols pour hier';
    VRS.$$.Reports =                                    'Rapports';
    VRS.$$.ReportsAreDisabled =                         'Le serveur autorise pas les rapports';
    VRS.$$.Resume =                                     'Resume';
    VRS.$$.Reversing =                                  'Reversing';  /** THIS IS NEW! **/
    VRS.$$.ReversingShort =                             'REV';  /** THIS IS NEW! **/
    VRS.$$.Route =                                      'Route';
    VRS.$$.RouteShort =                                 'Route (court)';
    VRS.$$.RouteFull =                                  'Route (complète)';
    VRS.$$.RouteNotKnown =                              'Route inconnue';
    VRS.$$.RowNumber =                                  'N° lignes';
    VRS.$$.Rows =                                       'Lignes';
    VRS.$$.RunReport =                                  'Rapport';
    VRS.$$.SeaPlane =                                   'Seaplane';
    VRS.$$.Select =                                     'Select';
    VRS.$$.SeparateTwoValues =                          ' et ';
    VRS.$$.SerialNumber =                               'Serie';
    VRS.$$.ServerFetchFailedTitle =                     'Fetch Failed';
    VRS.$$.ServerFetchFailedBody =                      'Could not fetch from the server. The error is "{0}" and the status is "{1}".';
    VRS.$$.ServerFetchTimedOut =                        'The request has timed out.';
    VRS.$$.ServerReportExceptionBody =                  'Le serveur a rencontré une exception en générant le rapport "{0}"';
    VRS.$$.ServerReportExceptionTitle =                 'Server Exception';
    VRS.$$.SetCurrentLocation =                         'Fixe position courante';
    VRS.$$.Settings =                                   'Config';
    VRS.$$.SettingsPage =                               'Page config.';
    VRS.$$.Shortcuts =                                  'Raccourcis';
    VRS.$$.ShowAltitudeStalk =                          'Montre ligne altitude';
    VRS.$$.ShowAltitudeType =                           'Show altitude type';  /** THIS IS NEW! **/
    VRS.$$.ShowCurrentLocation =                        'Montre position courrante';
    VRS.$$.ShowDetail =                                 'Montre  détails';
    VRS.$$.ShowForAllAircraft =                         'Pour tous les avions';
    VRS.$$.ShowEmptyValues =                            'Valeurs vides';
    VRS.$$.ShowForSelectedOnly =                        'Uniquement avion sélectionné';
    VRS.$$.ShowRangeCircles =                           'Affiche cercles de distance';
    VRS.$$.ShowShortTrails =                            'Montre traces courtes';
    VRS.$$.ShowSpeedType =                              'Show speed type';  /** THIS IS NEW! **/
    VRS.$$.ShowTrackType =                              'Show heading type';  /** THIS IS NEW! **/
    VRS.$$.ShowUnits =                                  'Montre unités';
    VRS.$$.ShowVerticalSpeedType =                      'Show vertical speed type';  /** THIS IS NEW! **/
    VRS.$$.ShowVsiInSeconds =                           'Montre Vitesse verticale par seconde';
    VRS.$$.SignalLevel =                                'Niveau signal';
    VRS.$$.Silhouette =                                 'Silhouette';
    VRS.$$.SilhouetteAndOpFlag =                        'Sil. & logo Op.';
    VRS.$$.SiteTimedOut =                               'Le site est en veille pour inactivité. Fermez cette fenêtre pour réactiver.';
    VRS.$$.SortBy =                                     'Trier par';
    VRS.$$.Species =                                    'Specs';
    VRS.$$.Speed =                                      'Vitesse';
    VRS.$$.SpeedGraph =                                 'Graphe vitesse';
    VRS.$$.Speeds =                                     'Vitesse';
    VRS.$$.SpeedType =                                  'Speed Type';  /** THIS IS NEW! **/
    VRS.$$.Squawk =                                     'Squawk';
    VRS.$$.Start =                                      'Début';
    VRS.$$.StartsWith =                                 'Début avec';
    VRS.$$.StartTime =                                  'Heure de début';
    VRS.$$.Status =                                     'Statut';
    VRS.$$.StatuteMileAbbreviation =                    '{0} mi';
    VRS.$$.StatuteMiles =                               'Statute Miles';
    VRS.$$.StorageEngine =                              'Storage engine';
    VRS.$$.StorageSize =                                'Storage size';
    VRS.$$.StrokeOpacity =                              'Stroke opacity';  /** THIS IS NEW! **/
    VRS.$$.SubmitRoute =                                'Soumettre route';
    VRS.$$.SubmitRouteCorrection =                      'Soumettre correction route';
    VRS.$$.SuppressAltitudeStalkWhenZoomedOut =         'Supprimer la ligne altitude en quittant le zoom';
    VRS.$$.TargetAltitude =                             'Target Altitude';  /** THIS IS NEW! **/
    VRS.$$.TargetHeading =                              'Target Heading';  /** THIS IS NEW! **/
    VRS.$$.ThenBy =                                     'Puis par';
    VRS.$$.Tiltwing =                                   'Tiltwing';
    VRS.$$.TimeTracked =                                'Time Tracked';
    VRS.$$.TitleAircraftDetail =                        'Details avion';
    VRS.$$.TitleAircraftList =                          'Liste avion';
    VRS.$$.TitleFlightDetail =                          'Details';
    VRS.$$.TitleFlightsList =                           'Vols';
    VRS.$$.ToAltitude =                                 'Vers {0}';
    VRS.$$.TitleSiteTimedOut =                          'Timed Out';
    VRS.$$.TotalHours =                                 'Heures totales';
    VRS.$$.TrackingCountAircraft =                      'Suit {0:N0} avions';
    VRS.$$.TrackingCountAircraftOutOf =                 'Suit {0:N0} avions (out of {1:N0})';
    VRS.$$.TransponderType =                            'Transponder';  /** THIS IS NEW! **/
    VRS.$$.TransponderTypeFlag =                        'Transponder Flag';  /** THIS IS NEW! **/
    VRS.$$.TrueAirSpeed =                               'True';  /** THIS IS NEW! **/
    VRS.$$.TrueAirSpeedShort =                          'TAS';  /** THIS IS NEW! **/  // Keep this one short, an abbreviation if possible.
    VRS.$$.TrueHeading =                                'True heading';  /** THIS IS NEW! **/
    VRS.$$.TrueHeadingShort =                           'True';  /** THIS IS NEW! **/
    VRS.$$.Turbo =                                      'Turbo';
    VRS.$$.Unknown =                                    'Unknown';  /** THIS IS NEW! **/
    VRS.$$.UseBrowserLocation =                         'Utiliser position GPS';
    VRS.$$.UseRelativeDates =                           'Utiliser dates relatives';
    VRS.$$.UserTag =                                    'Tag utilisateur';
    VRS.$$.VerticalSpeed =                              'Vitesse verticale';
    VRS.$$.VerticalSpeedType =                          'Vertical Speed Type';  /** THIS IS NEW! **/
    VRS.$$.VirtualRadar =                               'Radar Virtuel';
    VRS.$$.Volume25 =                                   'Volume 25%';
    VRS.$$.Volume50 =                                   'Volume 50%';
    VRS.$$.Volume75 =                                   'Volume 75%';
    VRS.$$.Volume100 =                                  'Volume 100%';
    VRS.$$.VrsVersion =                                 'Version {0}';
    VRS.$$.WakeTurbulenceCategory =                     'Turbulence de sillage';
    VRS.$$.Warning =                                    'Avertissements';
    VRS.$$.WorkingInOfflineMode =                       'Mode de fonctionnement hors ligne ';
    VRS.$$.WtcLight =                                   'Léger';
    VRS.$$.WtcMedium =                                  'Medium';
    VRS.$$.WtcHeavy =                                   'Lourd';
    VRS.$$.YearBuilt =                                  'Année construction';
    VRS.$$.Yes =                                        'Oui';

    // Date picker text
    VRS.$$.DateClose =                                  'Fait';  // Keep this short
    VRS.$$.DateCurrent =                                'Aujour.';  // Keep this short
    VRS.$$.DateNext =                                   'Suiv.';  // Keep this short
    VRS.$$.DatePrevious =                               'Prec.';  // Keep this short
    VRS.$$.DateWeekAbbr =                               'Se';  // Keep this very short
    VRS.$$.DateYearSuffix =                             '';  // This is displayed after the year
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
            case VRS.EngineType.Electric:   result += ' electric'; break;
            case VRS.EngineType.Jet:        result += ' jet'; break;
            case VRS.EngineType.Piston:     result += ' piston'; break;
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
                case VRS.WakeTurbulenceCategory.Light:  result = 'Light'; break;
                case VRS.WakeTurbulenceCategory.Medium: result = 'Medium'; break;
                case VRS.WakeTurbulenceCategory.Heavy:  result = 'Heavy'; break;
                default: throw 'Unknown wake turbulence category ' + category;  // Do not translate this line
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
        if(from) result = 'From ' + from;
        if(to) {
            if(result.length) result += ' to ';
            else              result = 'To ';
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
