HOW TO ADD YOUR OWN TILE SERVER SETTINGS
========================================

It is a requirement for most free tile servers that the program does not hard
code their address and that there is some way to remove support for a
particular tile server if their operators request it.

To that end VRS downloads a copy of the tile server settings from the SDM site
once a day. The downloaded settings are stored in a file called:

    TileServerSettings-Downloaded.json.

DO NOT CHANGE THE SETTINGS IN THAT FILE. Your changes will be lost within 24
hours.

If you are running your own tile server, or you want to use a tile server that
is not in the main set of downloaded tile server settings, then you can tell
VRS about the server by creating a file called:

                        TileServerSettings-Custom.json

in the same directory as these instructions. The file is a normal text file. In
the file you will need to paste everything between the //----- lines below:

//------ start of file: copy from below this line
[
  {
    "MapProvider": "Leaflet",
    "DisplayOrder": 1,
    "Name": "-- FILL IN YOUR NAME HERE --",
    "Url": "-- FILL IN THE URL USING LEAFLET SPECIFICATIONS --",
    "Attribution": "-- FILL IN ATTRIBUTIONS HERE --",
    "Subdomains": null,
    "Version": null,
    "MinZoom": null,
    "MaxZoom": null,
    "ZoomOffset": null,
    "MinNativeZoom": null,
    "MaxNativeZoom": null,
    "ZoomReverse": false,
    "DetectRetina": false,
    "ClassName": null,
    "ExpandoOptions": []
  }
]
//----- end of file: copy to the line above this one

Most of the fields are optional and can be left as null. The fields that are
NOT optional are: 
  * MapProvider (leave this as Leaflet)
  * DisplayOrder (if you have multiple entries then the lowest values are
    displayed first, otherwise leave as 1)
  * Name (must be unique across all your custom entries)
  * Url
  * Attribution

If you supply a non-null value for a string field then you need to surround the
value with quotes. String fields are:
  * MapProvider (always leave this as Leaflet)
  * Name
  * Url
  * Subdomains
  * ClassName
  * Attribution
  
If you supply a boolean value then you must use either true or false. Boolean
fields are:
  * ZoomReverse
  * DetectRetina
Examples are:
    "ZoomReverse": true,
    "DetectRetina": false,
  
If you supply a non-null numeric value then you must not surround it in quotes.
Numeric fields are:
  * MinZoom
  * MaxZoom
  * ZoomOffset
  * MinNativeZoom
  * MaxNativeZoom
Examples are:
    "MinZoom": null,
    "MaxZoom": 19,

    
CHOOSING A NAME
===============
If you specify more than one tile server then the names for each tile server
must be unique. Your names will be prefixed with "*" in the VRS user
interface.


URL
===
The tile server URL must follow Leaflet's rules for specifying a tile server
URL. At time of writing their documentation can be found here:

https://leafletjs.com/reference-1.3.0.html#tilelayer


ATTRIBUTION
===========
Any HTML you put into the attribution string will be shown to the user as text.
You cannot enter raw HTML here. However, you can use the following tags - these
will be turned into HTML when the map is created:

[c] becomes a copyright symbol
[a href=YOUR-HYPERLINK-ADDRESS-HERE]some text[/a] is transformed into a link
[attribution NAME] is replaced with the attribution from the tile server
setting called NAME


OTHER SETTINGS
==============
At the moment all of the other settings are a subset of the settings that can
be specified when creating Leaflet tile layers. See Leaflet's documentation,
which at time of writing is:

https://leafletjs.com/reference-1.3.0.html#tilelayer

Note that only the single-string version of subdomains is supported.
    

EXPANDO OPTIONS
===============    
ExpandoOptions are extra options that you want VRS to use when creating maps.
If your tile server needs non-standard options like API keys then this is where
you specify them. The ExpandoOptions are an array of objects where each object
has these fields (all strings):

{
    "Option":  "OptionName",
    "Value":   "Value"
}

So if you wanted to add these two new fields to the tile layer options:

  apiKey = ABC123
  version = 7
  
Then your expando options entry in the file would look like this:

[
  {
    "MapProvider": "Leaflet",
    "DisplayOrder": 1,
    "Name": "-- FILL IN YOUR NAME HERE --",
    "Url": "-- FILL IN THE URL USING LEAFLET SPECIFICATIONS --",
    "Attribution": "-- FILL IN ATTRIBUTIONS HERE --",
    "Subdomains": null,
    "Version": null,
    "MinZoom": null,
    "MaxZoom": null,
    "ZoomOffset": null,
    "MinNativeZoom": null,
    "MaxNativeZoom": null,
    "ZoomReverse": false,
    "DetectRetina": false,
    "ClassName": null,
    "ExpandoOptions":
    [
      {
          "Option": "apiKey",
          "Value": "ABC123"
      },
      {
          "Option": "version",
          "Value": "7"
      }
    ]
  }
]

Only single string expando values are supported - no arrays, objects etc.
