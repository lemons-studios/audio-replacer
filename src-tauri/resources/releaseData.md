# Audio Replacer 5.0.1
This project is always going to be cursed with an immediate hotfix release after a major update, I swear

## New Features
- Added an option to change the intensity of the noise suppression filter, if enabled

## Removed Features
- Remove an unused total time with app open statistic because I couldn't figure out how to get it to work. Maybe in the future when i revisit the project....

## Fixes
- Added type checking on any number input settings to prevent situations where invalid values would be in the settings
- Fix FFMpeg builds not building with the arnndn model, which is required for noise suppression
