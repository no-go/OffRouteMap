# OffRouteMap

A simple but modern Windows Application in C# and WPF to get and cache free
map tiles (e.g. OSM or OpenBusMap) and use them offline to draw and measure routes
in kilometers, save and load them.

## Creation roadmap and ideas

- make this readme in inital comit and make everything public on github (good idea?!)
- cool and easy gui size and dark/light modes via MahApps.Metro
- store gui size and theme in setting
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

## Features

- map
  - [x] given area from window size / region
  - [ ] load tiles from given folder (settings)
  - [x] load tiles from network if missing 
  - [x] select different maps
  - [ ] customize differnt map folder and sources
  - [x] move area (right click)
  - [x] in/out zoom (mouse wheel)
  - [x] display position of cursor (status line)
- route
  - [ ] display (or hide parts) on map in all zoom states
  - [ ] start, end, remove and set a point/line (left click / context menu)
  - [ ] display length (status line)
- settings
  - [ ] tiles folder
  - [x] GUI size and theme (part of window frame)
  - [x] initial/last position
  - [x] initial/last zoom
- menu
  - [ ] load route (file window)
  - [ ] save route (file window)
  - [ ] set tiles folder ("file" window)
  - [ ] clean map

## Not clear and to late design questions

- localisation
- unit tests
- something similar to doxygen
- ci/cd
- any idea to port it to linux? maybe with glide or something strange like fltk? a hard break between code and gui?
