# Match-Game-Unity-Project


Reha Demircan  
**Unity Version:** 2022.3.47f1 (LTS)  
**Scene:** MainScene  
**Target Resolution:** 1080x1920 (Portrait, mobile-like)  
**IDE:** JetBrains Rider (2025.1.x)
**Date: 26.01.2026


## Overview
This project is a Collapse/Blast tile-matching prototype developed
It includes a performant blast mechanic, dynamic group icons based on thresholds, and deadlock detection with a deterministic shuffle solution.

## How to Play
- Click/tap a connected group of same-colored tiles.
- Groups with **2 or more tiles** are blasted and removed.
- Tiles fall down to fill gaps, and new tiles spawn from the top.

## Implemented Features
- **Collapse/Blast Mechanic** (min group size: 2)
- **Configurable Board** via `GameConfig` (rows/columns, 1–6 colors)
- **Group Detection** (4-directional flood fill)
- **Dynamic Group Icons (Default / A / B / C)** based on thresholds (A/B/C) in `GameConfig`
- **Deadlock Detection + Smart Shuffle**
  - Detects deadlock when no adjacent same-colored pair exists
  - Shuffles colors while keeping positions and guarantees at least one valid move
- **SFX Feedback**
  - Soft pop sound on blast
  - Error sound on invalid click

## Project Structure
- `BoardManager.cs` – grid creation, input, blast/collapse/refill
- `Tile.cs` – tile behavior and movement
- `BoardAnalyzer.cs` – group detection 
- `DeadlockSolver.cs` – deadlock detection + deterministic shuffle
- `GameConfig.cs` – setup, thresholds

## Notes
- Submitted as a Unity project ZIP **excluding the Library folder**.
- No external plugins required.
- Compatible with Windows / macOS / Linux via Unity 2022.3.47f1.
