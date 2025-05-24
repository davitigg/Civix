# Civil3D Custom Commands

This AutoCAD Civil 3D plugin provides a set of custom commands to support and speed up drafting workflows for our civil engineering project work.

Each command corresponds to its own class in the source code.

> **Note:** This plugin is designed specifically for our internal use at our civil engineering design team.

---

## Commands

| Command        | Description                                                                 |
|----------------|-----------------------------------------------------------------------------|
| `AccioSymbols` | Inserts predefined block symbols at COGO point locations based on their `RawDescription` values. |
| `ConnectoCogo` | Connects selected COGO points into categorized 3D polylines by description. |
| `PortaSeparo`  | Splits a selected polyline segment and adds a gate segment on a specific layer. |
| `AccioPipe`    | Connects COGO points in pairs to draw pipe polylines and optionally adds design or existing labels. |

---

## Requirements

- Required **block definitions** (e.g., ჭა, ელ. ბოძი) must exist in the drawing or be loaded in advance.
- Certain **layers** (e.g., `_ჭიშკარი`) must be present in the drawing for commands like `PortaSeparo`.
- Commands are intended for use in Civil 3D drawings that include COGO points with meaningful `RawDescription` codes.

---

## Usage

1. Load the compiled `.dll` into AutoCAD using the `NETLOAD` command.
2. Use any of the commands listed above by typing them into the command line.
