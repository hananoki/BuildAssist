# Build Assist

## [2.0.0] - 2021-04-03
- SharedModule v1.8.0 or later

### Added
- UnityHub cooperation was added

### Changed
- Changed to TreeView based UI
- I removed the asset bundle build
- Enables WebGL playback

## [1.3.9] - 2021-02-07
- SharedModule v1.7.5 or later

### Fixed
- Fixed exclusionAssets error

## [1.3.8] - 2020-12-20
- SharedModule v1.7.4 or later

## [1.3.7] - 2020-12-13
- SharedModule v1.7.3 or later

### Changed
- Changed namespace

## [1.3.6] - 2020-12-02
- SharedModule v1.7.0 or later

### Fixed
- Fix the error when BuildTargetGroup
- Fix Android localization

## [1.3.5] - 2020-11-15
- SharedModule v1.6.0 or later

## [1.3.4] - 2020-08-02

### Changed
- Changed dependencies in package.json

## [1.3.3] - 2020-07-25

### Changed
- removed as wasmStreaming is no longer supported in 2020.1

## [1.3.2] - 2020-07-16
- SharedModule v1.5.0 is supported

### Changed
- Support for the new setting
- The Localize folder has been moved
- Display was disabled at build time

## [1.3.1] - 2020-05-31
SharedModule v1.3.0 is required

### Changed
- Changed to be compatible with SharedModule v1.3.0
- Set "Auto Referenced" of asmdef to false

## [1.3.0] - 2020-04-18

### Added
- Added asset exclusion feature at build time.
- Build event added.

### Changed
- Project settings can now be selected to display.

### Fixed
- Added a product name to the WebGL output.
- Window scrolling is now supported.

## [1.2.0] - 2020-04-04

### Added
- Added the ability to define the scene you want to build.

## [1.1.0] - 2020-04-02

### Added
- Added items that can be set in WebGL.
- Allow optional asset bundle builds to be selected
- Add BuildAssistEvent and its sample
- Allows selection of StreamingAssets copy after asset bundle build

### Changed
- Moved the platform selection to the project settings
- Styles has been refactored.

### Fixed
- SymbolSettings linkage was not working, fixed.

## [1.0.1] - 2020-03-23

### Added
- Added "PlayerSettings.WebGL.compressionFormat" to diff

### Fixed
- Fixed error when opening preferences

## [1.0.0] - 2020-03-21
- First release

### Changed
- Changed repository from BuildManager to BuildAssist
- Refactored file names and namespaces