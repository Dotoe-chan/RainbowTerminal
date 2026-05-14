<p align="center">
<img width="640" height="240" alt="Rain1" src="https://github.com/user-attachments/assets/2dab77b1-453a-42f6-bbcc-6680ec5a20ba" />
</p>

You want English? see [README.md](./README.md) 

`RainbowTerminal` は、Unity 6 のメインツールバーに `terminal` ボタンを追加する Editor 拡張です。

<img width="546" height="50" alt="image" src="https://github.com/user-attachments/assets/70b00e66-48e6-4509-bbfe-5925be4ae84a" />

## 機能

- Unity Editor のメインツールバーにボタンを追加
- クリックで現在開いている Unity プロジェクト root に terminal を開く
- `wt.exe` が使えない場合は PowerShell にフォールバック
- クリックごとにアイコン色をランダム変更
- Unity Editor の表示言語に合わせて文言を切り替え
- Unity の正式な `MainToolbar` API を使用

※ 「れいんぼー」な要素はアイコン色がランダム変更するのみです

## 環境

- Unity 6
- Windows
- Windows Terminal (`wt.exe`) または PowerShell

## 導入

### Unity Package Manager から追加する場合:

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

### Git submodule として追加する場合:

```bash
git submodule add https://github.com/Dotoe-chan/RainbowTerminal Assets/Submodules/RainbowTerminal
```

追加後に Unity で `Assets > Refresh` を実行してください。
表示されない場合は Unity Editor を再起動してください。


## 仕様

- 英語環境では `terminal`、日本語環境では `ターミナル` と表示
- クリックで現在開いている Unity プロジェクト root に terminal を開く
- まず `wt.exe -d "<project path>"` を試す
- Windows Terminal を起動できない場合は PowerShell にフォールバック
- 右クリックメニューから `Open Terminal` も実行可能

## 言語

表示文言は Unity Editor の表示言語に追従します。

- ローカライズ呼び出し: `UnityEditor.L10n.Tr(...)`
- 日本語翻訳ファイル: `Editor/ja.po`
- アセンブリ属性: `Editor/AssemblyInfo.cs`

## ファイル

- `package.json`
- `Editor/RainbowCommitHeaderButton.cs`
- `Editor/RainbowTerminal.Editor.asmdef`
- `Editor/AssemblyInfo.cs`
- `Editor/ja.po`

## メモ

- Editor 専用です
- Unity の正式なメインツールバー拡張 API を使っています

## トラブルシューティング

- ボタンが表示されない場合は `Assets > Refresh` を実行してください
- それでも表示されない場合は Unity を再起動してください
- asmdef 名が他の拡張と衝突していないか確認してください

## ライセンス

MIT
