# RainbowTerminal

Unity 6 のメインツールバーに `terminal` ボタンを追加する Editor 拡張です。

## Features

- Unity Editor のメインツールバーにボタンを追加
- クリックで現在の Unity プロジェクト root に terminal を開く
- `wt.exe` が使えない場合は PowerShell にフォールバック
- クリックごとにアイコン色をランダム変更
- Unity Editor の表示言語に応じて文言を切り替え
- Unity 公式の `MainToolbar` API を使用

## Requirements

- Unity 6
- Windows
- Windows Terminal (`wt.exe`) または PowerShell

## Installation

Git submodule として追加します。

```bash
git submodule add https://github.com/Dotoe-chan/RainbowTerminal Assets/Submodules/RainbowTerminal
```

追加後に Unity で `Assets > Refresh` を実行してください。表示されない場合は Unity を再起動してください。

## Behavior

- ツールバーに `terminal` または `ターミナル` ボタンを表示します
- クリックで現在開いている Unity プロジェクトの root に terminal を開きます
- まず `wt.exe -d "<project path>"` を試し、失敗したら PowerShell にフォールバックします
- 右クリックメニューから `Open Terminal` も実行できます

## Localization

表示文言は Unity Editor の表示言語に合わせて切り替わります。

- ローカライズ呼び出し: `UnityEditor.L10n.Tr(...)`
- 日本語翻訳ファイル: `Editor/ja.po`
- アセンブリ属性: `Editor/AssemblyInfo.cs`

## Files

- `Editor/RainbowCommitHeaderButton.cs`
- `Editor/RainbowTerminal.Editor.asmdef`
- `Editor/AssemblyInfo.cs`
- `Editor/ja.po`

## Notes

- Editor 専用です
- Windows 向け実装です
- Unity の正式なメインツールバー拡張 API を使っています
- ボタン背景の自由なスタイル変更や複数画像の自由レイアウトはしていません

## Troubleshooting

- ボタンが出ない場合は `Assets > Refresh` を実行してください
- それでも出ない場合は Unity を再起動してください
- asmdef 名が他の拡張と重複していないか確認してください

## License

MIT
