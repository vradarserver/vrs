# VATSIM Plugin
VATSIM (https://www.vatsim.net/) is a virtual ATC network that users
of flight simulators can connect to.

This plugin adds a feed to the program that shows the location of all
of the flight simulator pilots currently connected to the VATSIM network.

## Installation
The plugin only works with version 3 preview 9 (or higher) of Virtual
Radar Server. It will not work with version 2.

It will work with either the 32-bit or 64-bit version of the program.

You do not need to have anything else installed. Specifically:

* You do not need to have any VATSIM client software installed.
* You do not need to register with VATSIM.
* You do not need to have a flight simulator installed or running.

## Configuration
You can configure the plugin either via
 `Tools`|`Plugins`|`VATSIM`|`Options` or via the `WebAdmin` plugin.

Most of the configuration options are self-explanatory, but there are
a few things to note.

**Enabled**: When you first install the plugin it is switched off. You
have to edit the plugin's configuration and enable it before you will
see any VATSIM feeds.

**Refresh Interval**: Regardless of the value you set here VATSIM will
only send updated values every 15 seconds (as of time of writing).

**Assume slow aircraft are on ground**: VATSIM does not send a flag to
indicate whether aircraft are airborne. The plugin can only infer the
on-ground state from the aircraft's ground speed.

**Infer model from model type**: VATSIM does not send the specific
manufacturer and model, it only sends the ICAO 8643 code for the aircraft
type (but only once a flight plan has been submitted by the pilot).
This flag controls whether the program will guess the manufacturer
and model from the type code.

**Show invalid registrations**: VATSIM pilots often omit the aircraft's
registration from their flight plan (in which case the plugin
will use the callsign, if the callsign appears to be a registration), or
they enter registrations that do not comply with ICAO regulations.

This option controls whether registrations that do not comply with ICAO
regulations are hidden from view.

## Feeds
VATSIM feeds are shown on the normal desktop and mobile web pages.

For the avoidance of doubt - they are not shown on the flight simulator
page. Use the normal desktop or mobile page.

To access a VATSIM feed click on `Menu`|`Receiver` and choose one of
the receivers that starts with **VATSIM**.

VATSIM feeds are only shown if the plugin has been enabled. If you cannot
see any VATSIM feeds then make sure you have enabled the plugin.

The plugin always adds a single feed called `VATSIM: Everything`. You
cannot remove this feed.

You can also configure geofenced feeds. These only show aircraft that
are within a pair of latitudes and longitudes on the surface of the Earth.

A geofenced feed can be centred on a coordinate, a pilot or an airport.

The plugin comes with two example geofenced feeds - one for the UK
and Ireland and another for Heathrow airport.

## Aircraft Details

There are a couple of fields that hold information specific to VATSIM
aircraft. These can be added to the aircraft detail panel and aircraft
list display via `Menu`|`Options` in the browser.

**User Tag**: Shows the pilot's CID and name.

**Notes**: Shows the route and remarks from the filed flight plan.

### Altitude
VATSIM sends the true altitude above mean sea level. VRS calls this the
geometric or AMSL altitude.

Mode-C/-S (and typically ADSB) report the pressure altitude at a standard
pressure of 1013mb, rounded to the nearest 25 feet. The plugin calculates
the pressure altitude from the geometric altitude and local pressure
setting that VATSIM reports for each pilot.

There are a variety of fields that can show either or both altitudes:

| Field                   | Meaning |
| ---                     | --- |
| `Altitude (AMSL)`     | The geometric altitude, i.e. the altitude reported by VATSIM. |
| `Altitude (Pressure)` | The pressure altitude calculated from the geometric altitude. |
| `Altitude`             | Either the pressure or geometric altitude depending on whether "Use pressure altitude" is ticked in the `General` tab. |
| `Flight Level`         | The `Altitude` (either pressure or geometric, see above) up to the transition altitude set in the `General` tab, and then the flight level calculated from the pressure altitude (never from the geometric altitude) when above the transition. |

*Note: The pressure altitude calculation was added in preview 10.*

## Caveats
VATSIM feeds cannot be recorded by the BaseStation Database writer, so
you cannot generate reports on VATSIM feeds.

VATSIM feeds cannot be rebroadcast or filtered.

The program does not track receiver ranges for VATSIM feeds.


## Troubleshooting

| Problem                                             | Solution |
| ---                                                 | --- |
| I cannot see any VATSIM feeds in the Receivers menu | Enable the plugin. |
| The `VATSIM: Everything` feed kills my browser    | Zoom in a bit, try only showing trails for the selected aircraft instead of all aircraft, or configure a geofenced feed and view that instead. |

