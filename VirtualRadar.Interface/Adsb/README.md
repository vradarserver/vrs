# VirtualRadar.Interface.Adsb

## Changes from V2

This is a straight port from VRS V2 except some floats have been converted to doubles
(see notes elsewhere re. converting some floats to doubles).

There were four places that were using floats:

| Class                              | Property |
| ---                                | --- |
| `AirborneVelocityMessage`          | `HorizontalVelocityError` |
| `AircraftOperationalStatusMessage` | `MaximumLength` |
| `AircraftOperationalStatusMessage` | `MaximumWidth` |
| `TargetStateAndStatusVersion2`     | `BarometricPressureSetting` |
| `TcasResolutionAdvisory`           | `ThreatRange` |
