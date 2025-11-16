# RouteEditor C#

[![Windows x64](docs/badge-windows.svg)](https://github.com/no-go/RouteEditorCS/releases/latest/download/RouteEditorCS.exe)
![Dot Net](https://github.com/no-go/RouteEditorCS/actions/workflows/build.yml/badge.svg)
[![Tests](https://img.shields.io/badge/tests%20and%20coverage-exists-green)](https://no-go.github.io/RouteEditorCS/index.html)
[![License](https://img.shields.io/badge/license-BSD%202-yellowgreen)](https://github.com/no-go/RouteEditorCS/blob/main/LICENSE.txt)
[![Dev docs](https://github.com/no-go/RouteEditorCS/actions/workflows/deploy.yml/badge.svg)](https://no-go.github.io/RouteEditorCS/api/RouteEditorCS.html)
[![Source Code](https://img.shields.io/badge/source%20code-github-orange)](https://github.com/no-go/RouteEditorCS/)

RouteEditor C# is a simple but modern Windows Application in C# and WPF to get to draw and measure routes
in kilometers, save and load them.

A GTK4 version of RouteEditor exists here: [gitlab.com/deadlockz/routeeditor](https://gitlab.com/deadlockz/routeeditor)

Because Offline Maps does not work well, I remove it from *OffRouteMap* and call
the Application *RouteEditor C#* now.

![Screenshot](docs/screenshot.png)

## UI Features

- map
  - select different maps
  - move area via left click
  - in/out zoom via mouse wheel
  - display position of cursor in status line
- route
  - display on map in all zoom states
  - start, end and set a point via double click
  - remove route from map (click on trash icon)
  - remove last route point via right click
  - display distance in status line
- settings
  - GUI size
  - initial/last position
  - initial/last zoom
- menu (or similar)
  - load route (file window)
  - save route (file window)
  - set tiles folder ("file" window, suitcase icon)
- ci/cd
  - some unit tests via xUnit
    - mock example
    - code coverage report
  - build a release executeable
  - in code documentation and something similar to doxygen/breath


## Dev Docs

See [here](https://no-go.github.io/RouteEditorCS/api/RouteEditorCS.html)!
