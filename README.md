# CursorMagnifier
[![Latest Release](https://img.shields.io/github/v/release/thismre/CursorMagnifier)](https://github.com/thismre/CursorMagnifier/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/thismre/CursorMagnifier/latest/total)](https://github.com/thismre/CursorMagnifier/releases/latest)
**Version 2.0.0.0**

## Download CursorMagnifier
Always download the official Release build.
Do NOT use “Code → Download ZIP” — that ZIP contains source code only and will not run.

Download the latest Release:
https://github.com/thismre/CursorMagnifier/releases/download/v2.0.0.0/CursorMagnifier_v2.0.0.0.zip

The Release ZIP contains the pre-built executable, ready to run on Windows.

---

## Important
The green "Code → Download ZIP" button downloads the source tree, not the application.
If you want to run CursorMagnifier, you must download the Release ZIP from the link above.

### What’s the difference?

| Download Type | Contains | Intended For |
|---------------|----------|--------------|
| Release ZIP | Pre-built EXE, versioned, ready to run | Instructors, end-users |
| Code → Download ZIP | Raw source code | Developers, contributors |

---

## Overview
CursorMagnifier is a lightweight instructional magnifier designed for trainers, presenters, and technical educators. It provides a clear, high-visibility zoomed view of the area surrounding the mouse cursor, making demonstrations easier to follow in both classroom and remote training environments.

## Features
- Smooth zoom control using Ctrl + Scroll
- Automatic corner repositioning when the cursor approaches the magnified view
- Adaptive reticle with dynamic contrast based on background brightness
- Updated circular reticle ring with 5px thickness and increased transparency
- Stable capture logic without offset adjustments
- Optional Run-as-Administrator mode for full scroll-through support
- Instructional banner showing zoom level and admin status
- No installation required; runs on .NET Framework 4.7.2

## Controls
- F8 toggles the magnifier overlay
- Ctrl + Scroll zooms in or out
- Ctrl + F8 terminates the application
- The magnified view initially appears in the lower-right corner of the active monitor
  and moves to the opposite corner if the cursor gets too close.

## System Requirements
- Windows 10 or Windows 11
- .NET Framework 4.7.2 or later (included with Windows)

---

## Creating a Run‑as‑Administrator Shortcut
To enable full scroll-through support, create a local shortcut:

1. Right-click `CursorMagnifier.exe`
2. Select **Send to → Desktop (create shortcut)**
3. Right-click the new desktop shortcut
4. Select **Properties**
5. Open the **Shortcut** tab
6. Click **Advanced…**
7. Check **Run as administrator**
8. Click **OK**, then **OK**

---

## Version History

### 2.0.0.0 — Initial Release
- First public version of CursorMagnifier
- Smooth zoom control with Ctrl + Scroll
- Corner-aware magnified view repositioning
- Adaptive reticle with dynamic contrast
- Updated circular reticle ring (5px, increased transparency)
- Stable capture logic without offset adjustments
- Instructional banner showing zoom level and admin status
- F8 overlay toggle
- Ctrl + F8 application termination
- Optional Run-as-Administrator mode for full scroll-through support

---

# Standard README Sections

## Contents of the Release ZIP
The Release ZIP contains the following files:

- CursorMagnifier.exe
- CursorMagnifier.exe.config
- LICENSE.txt
- VERSION.txt

These are the only files required to run CursorMagnifier.

---

## Installation
CursorMagnifier requires no installation.

1. Download the Release ZIP.
2. Extract it to any folder.
3. Run `CursorMagnifier.exe`.

For full scroll-through support, use a Run-as-Administrator shortcut as described above.

---

## Usage
After launching:

- Press F8 to toggle the magnifier overlay.
- Use Ctrl + Scroll to adjust zoom.
- Press Ctrl + F8 to exit.
- The magnified view will reposition automatically to avoid covering the cursor.

CursorMagnifier is designed for instructors who need a clear, high-visibility cursor magnifier during demonstrations.

---

## Support / Issues
If you encounter problems, open an issue here:
https://github.com/thismre/CursorMagnifier/issues

Include:

- Windows version
- .NET Framework version
- Steps to reproduce
- Screenshots if applicable

---

## Contributing
CursorMagnifier is a small instructional tool.
If you want to contribute improvements, you may submit a pull request.

Source code is available via:
Code → Download ZIP

---

## License
CursorMagnifier is distributed under the license included in:
LICENSE.txt

---

If CursorMagnifier makes your teaching easier, you can support development here:
https://buymeacoffee.com/markjacob

I’m not a developer — I’m an instructor who built this tool with the help of AI to make demos clearer for everyone.
If it saves you time or helps your students, your support means a lot.
