# VantanGP26_10 — 後期サンプル「3D探索アドベンチャー」

> **このブランチ（`lesson11-bugs`）は第11回「デバッグの方法」用のバグ入り版です。**
> 完成形にバグが 7 か所仕込んであります（コンパイルエラー 1、実行時エラー 2、エラーは出ないが挙動がおかしいもの 4）。
> 症状から原因を探して直してください。完成形に戻るには `git checkout main`。

バンタン ゲームプログラマ26 1年生 **後期（第14〜25回）** の授業で分解して学ぶための完成サンプルです。
テーマは「C# の設計を深掘りする＝**きれいに作る**」。敵・アイテム・ギミック・状態・セーブが絡む探索ゲームを題材に、
クラス／継承／ScriptableObject／PlayerPrefs などを学びます。

> **このリポジトリには Asset Store の素材は入っていません。** 素材なしでも「カプセル人間＋プリミティブ」でそのまま遊べます。
> 本物のモデル・演出・街並みで動かしたい人は、次の 4 つ（すべて無料）を Asset Store から各自インポートしてください。
> 手順は [docs/アセット導入手順.md](docs/アセット導入手順.md)。
>
> | 用途 | 素材 |
> |------|------|
> | キャラクター（プレイヤー・敵） | [First Person + Third Person \| Character Controllers](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-package-196526)（Unity 公式 Starter Assets。ロボットのモデルとモーションだけ使う） |
> | 演出 | [Particle Pack](https://assetstore.unity.com/packages/vfx/particles/particle-pack-starter-assets-127325)（Unity Technologies） |
> | アイテムの見た目 | [Low Poly Basic Items Pack - Household Items](https://assetstore.unity.com/packages/3d/props/low-poly-basic-items-pack-household-items-249507)（Kabungus） |
> | ステージ装飾 | [SimplePoly City - Low Poly Assets](https://assetstore.unity.com/packages/3d/environments/simplepoly-city-low-poly-assets-58899)（VenCreations） |
>
> 効果音・BGM は **各自で用意**して `Assets/Resources/Sounds/` に置きます（Asset Store 不要）。音ファイルはリポジトリに入っていません
> （フリー素材は再配布禁止のため）。サンプルで使っている音のダウンロード先と、必要な音のファイル名一覧は
> [docs/アセット導入手順.md](docs/アセット導入手順.md) の「効果音・BGM の一覧と差し替え方」。

---

## 1. 動作環境

| 項目 | 内容 |
|------|------|
| Unity | **6000.3.x（Unity 6.3）**。6000.0 では開けません |
| レンダーパイプライン | **URP（Universal Render Pipeline 17.x）**。前期プロジェクトと同じ。URP アセットは `Assets/Settings/` に同梱 |
| 入力 | 旧 Input Manager（`Input.GetAxis` / `Input.GetKeyDown`） |
| UI 文字 | TextMeshPro（UI の文字は英数字のみ。日本語フォントは同梱していません） |
| 追加パッケージ | URP のみ（Input System / Cinemachine などは使いません） |

## 2. クローンしてから遊べるまで（5分）

```bash
git clone https://github.com/tanufuku/GP26_Oct.git
```

コマンドは、保存したい場所（例 `C:\dev\Unity`）をエクスプローラーで開き、アドレスバーに `powershell` と打って Enter した画面に貼り付けます
（GitHub Desktop の Clone でも可）。**`GP26_Oct` というフォルダ**ができます。これがこのプロジェクト本体です。

1. **Unity Hub で開く**（`Add` → `Add project from disk` → さっきできた `GP26_Oct` フォルダ）。Unity 6000.3 系を選ぶ。
   初回は `Library/` を作るので数分かかります。URP の設定・TextMeshPro の必須リソース・タグはリポジトリに入っているので、追加の設定はありません。
2. `Assets/Scenes/Main.unity` を開いて **Play**。カプセル人間で遊べます。
3. （任意）本物のモデル・演出・街並み・音で遊ぶ → [docs/アセット導入手順.md](docs/アセット導入手順.md) の手順で
   Asset Store 素材をインポートし、**自分の手で `Assets/Resources/` に Prefab を作って組み込む**（シーンは触らない）。

セットアップを自動で行うメニューはありません（Unity の操作を自分で覚えるため）。困ったときは下の「よくあるトラブル」を見てください。
シーンや Prefab を壊してしまったときは、Git で元に戻せます（例：`git checkout -- Assets/Scenes/Main.unity`、全部戻すなら `git checkout -- Assets`）。

## 3. 操作方法

| キー | 操作 |
|------|------|
| W A S D / 矢印 | 移動 |
| Space | ジャンプ（タイトルではゲーム開始） |
| J / 左クリック | 攻撃（正面に弾を撃つ） |
| Esc | ポーズ／再開 |
| C（タイトル） | チェックポイントから続きを始める（セーブがあるとき） |
| R（ゲームオーバー） | チェックポイントからやり直す |

**ゲームの流れ**：エリアA（看板・スイッチで扉1を開ける）→ エリアB（巡回する敵・鍵・宝箱×2・鍵付きの扉2）
→ エリアC（追跡する敵・遠距離の敵・ボス）。ボスを倒すとクリア。エリア境界のチェックポイントで自動セーブ。

**授業用の最小シーン**（`Assets/Scenes/Lessons/`）：各回で扱う要素だけを置いた小さなシーン。開いて Play するだけで、素材も音も無しでその回の挙動を確認できます。

| シーン | 回 | 置いてあるもの |
|---|---|---|
| `Lesson02_Method` | 後期2回目 メソッド | 動かない的 1 体（頭上に HP と受けたダメージが出る） |
| `Lesson03_ClassData` | 後期3回目 クラス① | EnemyData だけが違う的 3 体 |
| `Lesson04_ClassMethod` | 後期4回目 クラス② | 追いかけて攻撃してくる敵 1 体 |
| `Lesson05_Inheritance` | 後期5回目 継承 | Patrol / Chaser / Ranged を横並び（広い床） |
| `Lesson06_Practice` | 後期6回目 復習 | Lesson05 の複製。自作した敵をここに置く |
| `Lesson07_GetComponent` | 後期7回目 GetComponent | スイッチ→扉、鍵→鍵付き宝箱。敵なし |
| `Lesson08_Coroutine` | 後期8回目 コルーチン | 敵 1 体。被弾後の無敵点滅を見る |
| `Lesson09_ScriptableObject` | 後期9回目 ScriptableObject | 剣・盾・回復薬。HP を減らした状態で開始 |
| `Lesson10_SaveState` | 後期10回目 セーブ・状態 | チェックポイント 2 つと敵 1 体。この回だけタイトルから開始 |

## 4. フォルダ構成

```
Assets/
├── Scripts/           ← 授業で読む対象（全29本）
│   ├── Player/        PlayerController, PlayerHealth, PlayerAttack, PlayerBullet, PlayerInventory
│   ├── Enemy/         EnemyBase, PatrolEnemy, ChaserEnemy, RangedEnemy, EnemyData(SO), EnemyBullet
│   ├── Items/         ItemData(SO), ItemType(enum), ItemPickup, KeyPickup
│   ├── Gimmicks/      Switch, Door, Chest, Checkpoint
│   └── System/        GameManager, GameState(enum), UIManager, ScoreManager, SaveManager,
│                      CameraFollow, GameConstants, ModelLoader, EffectLibrary, SoundLibrary
├── Data/              ← ScriptableObject の実データ（Items/Sword など、Enemies/Boss など）※②で生成
├── Prefabs/           ← Player / Enemy_* / Item / Key / Door / Chest / Switch / Checkpoint
├── Materials/         ← 色マテリアルと UI 用の白スプライト
├── Scenes/
│   ├── Main.unity     ← 完成形（1シーン構成）
│   └── Lessons/       ← 回ごとの最小シーン（その回で扱う要素だけ。カプセル・箱のまま、素材なしで動く）
├── Resources/         ← 名前で読まれる「本物」の置き場
│   ├── Sounds/        効果音・BGM（各自でダウンロードして置く。音ファイルは Git 対象外）
│   └── Characters/ Effects/ Items/ Stage/  ← 各自が手作業で作る、素材を参照する Prefab。Git 対象外
├── Settings/          ← URP アセットと Renderer（同梱）
├── TextMesh Pro/      ← TMP 必須リソース（同梱）
├── Starter Assets/    ← Asset Store からインポート（Character Controllers：ロボットのモデル・モーション）。Git 対象外
├── UnityTechnologies/ ← Asset Store からインポート（Particle Pack）。Git 対象外
├── Kabungus/          ← Asset Store からインポート（Household Items）。Git 対象外
└── SimplePoly City - Low Poly Assets/ ← Asset Store からインポート。Git 対象外
docs/                  ← Asset Store 素材の導入手順
```

## 5. GitHub に「入っているもの／入っていないもの」

| 入っている（Git 管理） | 入っていない（各自で用意） |
|---|---|
| `Assets/Scripts/` の全 C#（29 本） | `Assets/Starter Assets/`（Character Controllers）, `Assets/UnityTechnologies/`（Particle Pack） |
| `Assets/Data/`, `Prefabs/`, `Materials/`, `Scenes/`（Git 同梱） | `Assets/Kabungus/`, `Assets/SimplePoly City - Low Poly Assets/` |
| `Assets/TextMesh Pro/`（TMP 必須リソース）, `Assets/Settings/`（URP 設定） | `Assets/Resources/Characters|Effects|Items|Stage/`（各自が作る、素材を参照する Prefab）, `Assets/Resources/Sounds/*`（各自が置く音） |
| `ProjectSettings/`, `Packages/manifest.json` | `Library/`, `Temp/`, `Logs/`, `*.csproj` などの自動生成物 |

シーンや Prefab は Asset Store 素材を直接参照していません。仮の見た目（カプセル・ひし形）を置き、
各自が `Assets/Resources/` に作った本物を `ModelLoader` / `EffectLibrary` / `SoundLibrary` が起動時に `Resources.Load` で差し替えます。
だから素材が無くても壊れず、素材を入れて手順どおりに Prefab を置けば本物になります。

## 6. よくあるトラブル

| 症状 | 対処 |
|------|------|
| Unity Hub でバージョン違いの警告 | 6000.3.x を Hub でインストールして開く。6000.0 では開けません |
| 文字が表示されない／`TMP_Settings` のエラー | `Window > TextMeshPro > Import TMP Essential Resources` を実行 |
| 全部ピンク（マテリアルが壊れて見える） | `Edit > Project Settings > Graphics` の Default Render Pipeline に `Assets/Settings/PC_RPAsset` が入っているか確認（通常は入っている） |
| Starter Assets や Particle Pack を入れたら Cinemachine / Input System / Post Processing まで入った | 素材側の依存指定のため。そのままでも動く。片付け方は手順書の「Starter Assets の不要物を片付ける」 |
| Starter Assets を入れたら「Input System を有効にして再起動しますか」と聞かれた | **No** を選ぶ。Yes を押すと移動も Space も効かなくなる。押してしまったら `Edit > Project Settings > Player > Active Input Handling` を **Input Manager (Old)** に戻して Unity を再起動 |
| Starter Assets を入れたら `Library/PackageCache/com.unity.splines` のエラーが大量に出た | まず Unity を再起動。直らなければ手順書の「Starter Assets の不要物を片付ける」（Starter Assets のスクリプトを消してから Cinemachine を Remove） |
| Particle Pack のインポート中に止まった | 古いスクリプトの更新確認ダイアログ。自分のファイルは変わらないので「I Made a Backup. Go Ahead!」で進めてよい |
| モデル・演出・装飾が出ない | Prefab の名前と置き場所が手順書どおりか見直す（`Assets/Resources/Characters/Player.prefab` など。大文字小文字も） |
| SimplePoly City の建物がピンク | マテリアルが Built-in 用のまま。手順書 3-5 の Render Pipeline Converter で URP 用に変換する |

## 7. ライセンス

- このリポジトリのコード（`Assets/Scripts`, `Assets/Editor`, `docs`）: 授業用サンプル。授業内での利用・改変は自由です。
- `Assets/TextMesh Pro/`（TMP 必須リソース）と `Assets/Settings/`（URP テンプレート設定）は Unity Technologies 提供。Unity Companion License（同梱フォント LiberationSans は SIL Open Font License 1.1）。
- Asset Store の 4 素材は、それぞれの Asset Store ページに書かれたライセンスに従います（ページの「License type」欄を確認。
  Particle Pack / Household Items / SimplePoly City は Extension Asset、Character Controllers は Non standard EULA）。
  いずれも **使う人が自分で My Assets に追加する**必要があり、ファイルを他人に渡すことはできないため、このリポジトリには含めていません。
- 効果音・BGM のフリー素材も再配布禁止のため含めていません。入手先と利用条件は [docs/アセット導入手順.md](docs/アセット導入手順.md) を参照。
- **自分のリポジトリに push するときも、素材と音は入れないでください**（`.gitignore` で除外されています。外さないこと）。
