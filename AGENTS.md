# AGENTS.md

Unity **2022.3.62f3** (URP) C# starter. Verify C# with Unity batchmode (see **Verify changes**). Style: root `.editorconfig` (hints only). CI: `.github/workflows/` (unity-compile PR gate, itch.io deploy, Drive retrieve/track).

## Sources of truth

- **`ARCHITECTURE.md`** — folder rules, base classes, lifecycle, naming (System vs Manager vs Controller). Prefer over README.
- **`README.md`** — pitch + setup; tree uses `_Game/` + `Systems/`.
- **`GITHUB_WORKFLOWS.md`** — secrets/vars, deploy inputs, changelog.
- Executable wins over docs: `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `.gitattributes`, `.github/workflows/`.

## Layout

```
Assets/_Game/          # all game content
  00_Scripts/          # Core, Systems, Manager, Game, UI, Utils
  01_Objects/          # Prefabs/ + Data/ (SO variants)
  02_Art/Sprite/       # Drive-synced sprites
  03_Audio/            # GameAudioMixer.mixer (+ Music/, SFX/)
  04_Scenes/           # 00_Boot.unity + 8 more (all enabled in Build Settings)
  05_Settings/         # URP, Input
Assets/Editor/         # BuildScript.cs only (Editor-only code)
Assets/_Vendor/        # third-party — do not edit, do not commit churn
.github/workflows/     # unity-compile (PR), unity-itchio-deploy, retrieve, track
scripts/test_state.py  # Drive state regressions
retrieve.py, track.py, core/, state/, requirements.txt  # Drive → assets tooling
```

- Namespaces mirror folders: `Slafurry.Systems.*`, `Slafurry.Core.*`, `Slafurry.Utils.*`. Folder is **`Systems/`** (plural) — not `System/` (BCL conflict).
- **No `.asmdef`** — every non-`Editor` script compiles into `Assembly-CSharp`. Editor-only code must live under an `Editor/` folder.
- Internal rule: max 6 subfolders per folder (ARCHITECTURE.md).

## Boot / lifecycle (easy to get wrong)

- `LoadingSystem` must exist first — in `Assets/_Game/04_Scenes/00_Boot.unity` on root GameObject `====== LOADING ======`. New systems attach to root `====== SYSTEM ======` (alongside `SceneLoader`, `SaveSystem`, `InputHub`, `PauseSystem`, `LocalizationSystem`) or their own root.
- `Awake`/`Start` on base classes are private and sealed. `Singleton` registers with `LoadingSystem` in `Start()`; `Manager` registers in `Awake()`. Override `OnSingletonAwake()` / `OnManagerAwake()` for subclass setup.
- `Initialize()` = internal setup only (no other-object refs). It is an `IEnumerator` coroutine — `LoadingSystem` yields it with a per-object timeout (default 10s). Cross-object wiring only in `PostInitialize()`. Avoid `Start()` (guard with `_isReady` if unavoidable).
- `IInitializable.Priority` orders boot (lower first). Late registrants after boot get a one-frame delayed batch.
- Base class: cross-scene singleton → `GameSystem<T>`; scene-bound singleton → `LocalSingleton<T>`; session coordinator → `Manager` (registers with `GameManager` in `PostInitialize` via abstract `RegisterToGameManager`/`UnregisterFromGameManager`/`OnPostInitialize`).
- **`GameManager` does not exist** — `Manager.cs` and ARCHITECTURE still describe it. **Any `Manager` subclass will not compile** until you add one (or drop the abstract hooks). No subclasses exist today.
- `BootstrapLoader` (Systems/Scene) is optional: wire `systemsToWaitFor` + `SceneReference targetScene` (no default — must assign in Inspector or it errors).

## Static facades (prefer over `.Instance` chains)

`Audio` · `Save` · `Pause` · `Controls` (input) · `Localize` · `SceneSystem`

- Pause is a **key stack** (`Pause.On("UI")` / `Pause.Off("UI")`), not a bool — multiple holders can keep pause.
- Save: Newtonsoft.Json → `Application.persistentDataPath/{fileName}.json` (caller omits `.json`).
- Linear volume → dB via `Mathf.Log10(x) * 20` before `AudioMixer.SetFloat`.
- `GameFeel` is a **MonoBehaviour**, not a static facade.

## GitHub Actions — config lives in workflow-level `env:`

All four workflows declare config in `env:`; `run:` steps read `"$NAME"` only.

**Secrets** (credentials): `GEMINI_API_KEY`, `GOOGLE_SERVICE_ACCOUNT_JSON_B64`, `DISCORD_WEBHOOK_URL`, `DISCORD_WEBHOOK_URL_DEPLOY`, `UNITY_EMAIL`, `UNITY_PASSWORD`, `BUTLER_API_KEY`, `PR_PAT` (**retrieve only** — compile/deploy use default `GITHUB_TOKEN`).

**Variables** (non-credential): `DRIVE_SPRITE_FOLDER_ID`, `DRIVE_AUDIO_FOLDER_ID`, `GEMINI_MODEL`, `GEMINI_PERSONA` (deploy only), `GEMINI_PERSONA_ASSET` (track/retrieve), `GEMINI_LANGUAGE`, `BOT_GIT_USERNAME`, `BOT_GIT_EMAIL`, `ITCH_*`, `GAME_DISPLAY_NAME`, `UNITY_PROJECT_PATH`.

- **Retrieve** runs a Unity batchmode import after download so new Drive files get generated `.meta` — requires `UNITY_EMAIL`/`UNITY_PASSWORD`.
- **Deploy inputs**: `release_type` (release/beta/alpha) · `release_tag` (`1.2.3` → `v1.2.3-release`/`-beta`/`-alpha`; empty = unversioned, no GitHub Release). WebGL only — no `platforms` input.
- **Deploy changelogs**: `git log` changelog → Gemini writes Discord flavor text + GitHub Release body. No `generate_release_notes`.
- `secrets.*` still appears inline in action `with:` inputs and failure-notify `env:` blocks — intentional, not drift.

## Unity / git gotchas

- **No Git LFS** — binaries are plain blobs; `.gitattributes` marks them `binary`. Do not reintroduce `filter=lfs`. Do not commit `_Vendor` churn.
- **Commit `.meta`** with every new/renamed asset; never hand-edit GUIDs.
- **Input System only** (`activeInputHandler: 2`). Actions: `Assets/_Game/05_Settings/Input/Main Input.inputactions` → generated `Main Input.cs` (regenerate in Unity after editing actions).
- **SO creation:** all live SO types use `[CreateAssetMenu]`. `GameAssetCreatorAttribute` exists in `Utils/Attributes` but the `Slafurry → Game Data` window was never implemented — do not document or rely on it.
- `EditorBuildSettings` lists **9 enabled scenes** (`00_Boot` + 8 more). Do not assume only Boot is loadable.
- `*.csproj` / `*.sln` are Editor-generated — never hand-edit or commit them.
- **No git hooks** (`.githooks/pre-push` and `scripts/check-compile.sh` removed). Verify locally via batchmode.
- **Drive bots re-sync from `origin/main`** every run: `retrieve.yml` `rm -rf`s `state/`, `02_Art/Sprite`, `03_Audio` then restores via `git archive origin/main`. Commit `GameAudioMixer.mixer` + local state to `main` before relying on those workflows.
- **`track` / `retrieve` never push `main`** (ruleset GH013 — PR required). Both force-push one disposable **`retrieve-assets`** branch (state + assets) and open/update a PR → `main`; merge promptly or the next run re-syncs stale state and re-notifies Discord. Do not commit to `retrieve-assets` manually. `core/state.py` must keep retrieve fields when track merges a fresh listing (`scripts/test_state.py`).

## Verify changes

- **Style only:** `.editorconfig`.
- **Compile:** run Unity batchmode directly. Pass = exit 0, log contains `Exiting batchmode successfully`, no `error CS` lines. Editor path from `ProjectSettings/ProjectVersion.txt` / Unity Hub install.
- **CI:** `unity-compile.yml` runs the same batchmode + `error CS` grep on **PRs only** (paths: `Assets/`, `Packages/`, `ProjectSettings/`, the workflow file). Require in branch protection. CI installs the editor from `ProjectVersion.txt`.
- **Drive tooling:** `pip install -r requirements.txt`, then `python3 scripts/test_state.py` after touching `core/state.py` / `track.py` / `retrieve.py`.

```bash
~/Unity/Hub/Editor/2022.3.62f3/Editor/Unity \
  -batchmode -nographics -quit \
  -projectPath "$PWD" -logFile /tmp/unity-compile.log
grep -n "error CS" /tmp/unity-compile.log
grep -q "Exiting batchmode successfully" /tmp/unity-compile.log && echo OK
```

Prefer this (or opening the Editor) after any C# change. Play-mode behavior needs a manual Editor run (`00_Boot.unity`).
