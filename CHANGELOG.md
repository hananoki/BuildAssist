# BuildAssist

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