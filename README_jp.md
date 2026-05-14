<p align="center">
<img width="640" height="240" alt="Rain1" src="https://github.com/user-attachments/assets/28bbad28-d577-408b-bf8a-1c10ee0f1a8b" />
</p>

日本語版ドキュメントです。英語版は [README.md](./README.md) を参照してください。

`RainbowTerminal` は、Unity 6 のメインツールバーに `terminal` ボタンを追加する Editor 拡張です。

## Features

- Unity Editor のメインツールバーにボタンを追加
- クリックで現在開いている Unity プロジェクト root に terminal を開く
- `wt.exe` が使えない場合は PowerShell にフォールバック
- クリックごとにアイコン色をランダム変更
- Unity Editor の表示言語に合わせて文言を切り替え
- Unity の正式な `MainToolbar` API を使用

-# 「れいんぼー」な要素はアイコン色がランダム変更するのみです

## Requirements

- Unity 6
- Windows
- Windows Terminal (`wt.exe`) または PowerShell

## Installation

Git submodule として追加する場合:

```bash
git submodule add https://github.com/Dotoe-chan/RainbowTerminal Assets/Submodules/RainbowTerminal
```

追加後に Unity で `Assets > Refresh` を実行してください。
表示されない場合は Unity Editor を再起動してください。

Unity Package Manager から追加する場合:

1. `Window > Package Manager` を開く
2. `+` ボタンを押す
3. `Add package from git URL...` を選ぶ
4. 下の URL をそのまま貼り付ける

```text
https://github.com/Dotoe-chan/RainbowTerminal.git
```

`Packages/manifest.json` を直接編集して追加することもできます。

```json
{
  "dependencies": {
    "com.dotoe.rainbow-terminal": "https://github.com/Dotoe-chan/RainbowTerminal.git"
  }
}
```

## Behavior

- 英語環境では `terminal`、日本語環境では `ターミナル` と表示
- クリックで現在開いている Unity プロジェクト root に terminal を開く
- まず `wt.exe -d "<project path>"` を試す
- Windows Terminal を起動できない場合は PowerShell にフォールバック
- 右クリックメニューから `Open Terminal` も実行可能

## Localization

表示文言は Unity Editor の表示言語に追従します。

- ローカライズ呼び出し: `UnityEditor.L10n.Tr(...)`
- 日本語翻訳ファイル: `Editor/ja.po`
- アセンブリ属性: `Editor/AssemblyInfo.cs`

## Files

- `package.json`
- `Editor/RainbowCommitHeaderButton.cs`
- `Editor/RainbowTerminal.Editor.asmdef`
- `Editor/AssemblyInfo.cs`
- `Editor/ja.po`

## Notes

- Editor 専用です
- Windows 専用です
- Unity の正式なメインツールバー拡張 API を使っています
- ボタン内部の自由な背景スタイル変更や複数画像の自由配置はしていません

## Troubleshooting

- ボタンが表示されない場合は `Assets > Refresh` を実行してください
- それでも表示されない場合は Unity を再起動してください
- asmdef 名が他の拡張と衝突していないか確認してください

## License

MIT
