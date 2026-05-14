<p align="center">
<img width="640" height="240" alt="Rain2" src="https://github.com/user-attachments/assets/e9221e69-fdc0-4242-97cc-29a26f53a96f" />
</p>

日本語のREADMEはこちら、[README_jp.md](./README_jp.md).

`RainbowTerminal` is a Unity 6 editor extension that adds a `terminal` button to the main toolbar.

<img width="546" height="50" alt="image" src="https://github.com/user-attachments/assets/16242909-82ce-46ef-a2c3-034a5853eaf5" />

## Features

- Adds a button to the Unity Editor main toolbar
- Opens a terminal at the current Unity project root when clicked
- Falls back to PowerShell if `wt.exe` is not available
- Randomizes the icon color on each click
- Uses the Unity Editor display language for labels
- Uses Unity's supported `MainToolbar` API

Note: the only "rainbow" behavior is the icon color randomization.
 
## Requirements

- Unity 6
- Windows
- Windows Terminal (`wt.exe`) or PowerShell

## Installation

Add this repository as a Git submodule:

```bash
git submodule add https://github.com/Dotoe-chan/RainbowTerminal Assets/Submodules/RainbowTerminal
```

After adding it, run `Assets > Refresh` in Unity.
If the button does not appear, restart the Unity Editor.

You can also add it directly from Unity Package Manager:

1. Open `Window > Package Manager`
2. Click the `+` button
3. Choose `Add package from git URL...`
4. Copy and paste this URL:

```text
https://github.com/Dotoe-chan/RainbowTerminal.git
```

You can also add it by editing `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dotoe.rainbow-terminal": "https://github.com/Dotoe-chan/RainbowTerminal.git"
  }
}
```

## Behavior

- Shows `terminal` in English or `ターミナル` in Japanese
- Opens a terminal at the currently open Unity project root
- Tries `wt.exe -d "<project path>"` first
- Falls back to PowerShell if Windows Terminal cannot be launched
- Also exposes `Open Terminal` in the context menu

## Localization

Displayed text follows the Unity Editor language.

- Localization calls: `UnityEditor.L10n.Tr(...)`
- Japanese translation file: `Editor/ja.po`
- Assembly attribute: `Editor/AssemblyInfo.cs`

## Files

- `package.json`
- `Editor/RainbowCommitHeaderButton.cs`
- `Editor/RainbowTerminal.Editor.asmdef`
- `Editor/AssemblyInfo.cs`
- `Editor/ja.po`

## Notes

- Editor only
- Uses Unity's supported main toolbar extension API
- Does not implement arbitrary background styling or freeform multi-image layout inside the button

## Troubleshooting

- If the button does not appear, run `Assets > Refresh`
- If it still does not appear, restart Unity
- Make sure the asmdef name does not conflict with another extension

## License

MIT
