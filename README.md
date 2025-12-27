# Tarkov Quest Scanner

This overlay tool helps Escape from Tarkov players track their active quests in real-time by scanning the in-game task list.

## Features
- **Quest Scanning:** Opens your "Tasks" tab and press **F9** to instantly scan and list your active quests.
- **Smart Filtering:** Integrates with **TarkovTracker.io** to hide quests you have already completed.
- **Map Grouping:** Automatically groups quests by Map (Customs, Woods, etc.) so you know exactly what to do in your next raid.
- **Privacy Secure:** Your API Key is masked and stored locally.

## Setup & Installation

1.  **Download:** Get the latest release from the Releases page (if available) or build from source.
2.  **Prerequisites:**
    -   .NET Framework 4.7.2
    -   Visual C++ Redistributable 2015-2022
3.  **Run:** Launch `TarkovQuestScanner.exe`.
4.  **Configuration:**
    -   (Optional) Enter your [TarkovTracker API Key](https://tarkovtracker.io/settings) in the settings.
    -   The input box will turn **Green** if the key is valid, or **Red** if invalid.
    -   Your progress will automatically sync when quests are scanned.

## How to Build from Source

**Requirements:**
-   Visual Studio 2022 (or 2019) with ".NET desktop development" workload.

**Steps:**
1.  Clone the repository.
2.  Open `TarkovQuestScanner.sln` in Visual Studio.
3.  Right-click the solution in Solution Explorer and select **Restore NuGet Packages**.
4.  Set the build configuration to **Release** / **x64**.
5.  Build the solution (`Ctrl+Shift+B`).
6.  The executable will be in `bin/x64/Release/TarkovQuestScanner.exe`.

## Usage
1.  Launch the app. It will minimize to tray or show the settings window.
2.  In Escape from Tarkov, open your **Tasks** tab.
3.  Press **F9**.
4.  A popup will show your active quests grouped by map.

## Credits
-   Original OCR logic by TarkovPriceViewer.
-   Data provided by [Tarkov.dev](https://tarkov.dev/) and [TarkovTracker.io](https://tarkovtracker.io/).