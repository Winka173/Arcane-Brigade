# Vietnamese text layout fix — 2026-11-07

The Settings title could reach the Back button because its plaque expanded to 900 design pixels, and fixed labels did not consistently constrain translated height. Menu page headers now reserve 20 design pixels before Back, draw inside the plaque's carved caps, and stay on one line. Vietnamese Settings uses the shorter title “Cài đặt”.

Vietnamese labels and buttons fit their translated content against both width and height without changing the shared source style. Short rows stay on one line, reading panels wrap, and an exceptionally long name receives an ellipsis at the minimum size with its full text preserved in the GUI tooltip. Clipping provides the final boundary against neighbouring controls.

Validation: 35 localization checks passed, including long titles, long portrait names, paragraph height, preserving full shortened captions and shared styles. Windows build succeeded with zero errors; existing compiler/shader warnings are recorded in `LocalizationBuild.txt`. Native verification runs at 960×540 using the isolated diagnostic profile; local captures are under `PreviewExports/LocalizationUIFix`.
