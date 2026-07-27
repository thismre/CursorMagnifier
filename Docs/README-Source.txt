CursorMagnifier – Source Notes
------------------------------

Project Overview:
CursorMagnifier is a Windows Forms application targeting .NET Framework 4.7.2.
It provides a stable, instructor‑focused magnification window that anchors to
screen corners, supports smooth zooming, and maintains high visibility through
an adaptive reticle and instructional banner. The magnifier does not follow the
cursor; instead, it relocates to the opposite corner when the cursor approaches
too closely.

Target Framework:
    • .NET Framework 4.7.2
    • No external dependencies
    • Compatible with Windows 10 and Windows 11

Core Components:
    • OverlayForm.cs – Main application window, rendering, capture logic,
      corner‑movement logic, reticle drawing, banner drawing, and WH_MOUSE_LL hook.
    • Program.cs – Standard WinForms entry point.

Rendering Pipeline:
    • Capture performed using CopyFromScreen.
    • Timer‑driven update loop at ~16ms for smooth motion.
    • Double‑buffered drawing to reduce flicker.
    • Magnifier content rendered into an off‑screen bitmap before display.

Movement Logic:
    • DangerRadius determines when the magnifier relocates to avoid covering
      the cursor.
    • SlideFactor and EmergencySlideFactor control movement speed.
    • Magnifier position updates smoothly toward the appropriate corner docking zone.
    • On multi‑monitor setups, the magnifier re‑anchors to the corresponding
      corner of the monitor the cursor moves onto.

Reticle System:
    • Updated circular ring with 5px thickness.
    • Increased transparency for reduced visual dominance.
    • Dynamic core color determined by luminance sampling of the capture area.
    • Crosshair lines drawn at the center for precise cursor focus.

Zoom System:
    • Ctrl + Scroll adjusts magnification.
    • Zoom factor clamped between MIN_MAG and MAX_MAG.
    • Capture size recalculated based on current zoom level.

Mouse Hook:
    • WH_MOUSE_LL intercepts scroll events.
    • Scroll‑through behavior forwarded to underlying windows.
    • When running as administrator, scroll‑through works inside elevated
      applications.

Banner System:
    • Displays exit key, zoom instructions, current zoom level, and admin status.
    • Semi‑transparent rounded rectangle for readability.
    • Uses Segoe UI for consistent instructor‑friendly typography.

Build Instructions:
    1. Open the solution in Visual Studio.
    2. Set configuration to Release.
    3. Build the solution.
    4. Distribute the contents of bin\Release\.

Distribution Notes:
    • Include the EXE and any generated DLLs (none in this project).
    • Optional: Provide a shortcut configured to “Run as administrator” for
      full scroll‑through support.
    • Include README.md, Installation Instructions.txt, and changelog.txt.

Version Notes:
This source edition corresponds to Version 2.0.0, which includes the updated
reticle ring, stable capture logic, and verified Run‑as‑Administrator behavior.
