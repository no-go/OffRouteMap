# OffRouteMap

![Dot Net](https://github.com/no-go/OffRouteMap/actions/workflows/dotnet-desktop.yml/badge.svg)
![Tests](https://github.com/no-go/OffRouteMap/actions/workflows/tests.yml/badge.svg)

A simple but modern Windows Application in C# and WPF to get and cache free
map tiles (e.g. OSM or OpenBusMap) and use them offline to draw and measure routes
in kilometers, save and load them.

![Screenshot](screenshot.png)

## Initial creation roadmap and ideas 

- make this readme in inital comit and make everything public on github (good idea?!)
- cool and easy gui size and dark/light modes via MahApps.Metro
- store gui size and dark mode in setting
- store last map position in settings
- does a nuget osm package still exist with needful features?
- design a single window application
- hide issues, if map title not downloadable (offline mode)
- cache map tiles in same way like foxtrotgps in a folder
- multithread downloads (?)
  - use a producer/consumer pattern 
  - maybe using LiveCharts2 to visualisze downloads
- maybe allow to set different map tile url sources similar to foxtrotgps
  - code must be open for that feature
  - see https://github.com/ArduPilot/MissionPlanner/blob/master/ExtLibs/Maps/WMSProvider.cs
- in code docu 

## UI Features

- map
  - [x] load tiles from given folder
  - [x] load tiles from network if missing 
  - [x] select different maps
  - [ ] add and customize different mapProvider urls (optional)
  - [x] move area via left click
  - [x] in/out zoom via mouse wheel
  - [x] display position of cursor in status line
- route
  - [x] display on map in all zoom states
  - [x] start, end and set a point via double click
  - [x] remove route from map (click on trash icon)
  - [x] remove last route point via right click
  - [x] display distance in status line
- settings
  - [x] tiles folder
  - [x] GUI size
  - [x] initial/last position
  - [x] initial/last zoom
- menu (or similar)
  - [x] load route (file window)
  - [x] save route (file window)
  - [x] set tiles folder ("file" window, suitcase icon)

## Future tasks

- dark/night mode removed as feature 2025-10-17
- [x] minimal localization
- [x] some unit tests
- [x] make model/interface for `GMapControl` and mock it in unit tests!!!!
- [x] switch to xUnit
- [ ] do lot of code coverage
- [ ] in code documentation and something similar to doxygen/breath
- [ ] re-add nullable !
- [x] ci/cd

## Known bugs

- caching tiles sometimes stored in a mixed way during the change of MapProvider
- inital offline cache not working
- ... see issue tracker !