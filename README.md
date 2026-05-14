# Rainbow Commit Nag

Unity 6 editor joke module that injects a button into the top header and makes it flash rainbow every 10 minutes to nag you into saving and committing.

## Intended layout

Drop this repository into a Unity project as a Git submodule:

```bash
git submodule add <your-repo-url> Assets/Submodules/RainbowCommitNag
```

## What it does

- Adds a button to the Unity header area.
- Flashes rainbow for 8 seconds every 10 minutes.
- Saves open scenes and assets on click.
- Copies suggested `git add .` and `git commit -m "..."` commands to the clipboard.
- Exposes `Tools/Rainbow Commit Nag/Flash Now` for quick testing.

## Notes

- The header insertion uses the Unity editor toolbar visual tree, so this is editor-only.
- The module looks for `.git` from the project root upward before showing Git status details.
