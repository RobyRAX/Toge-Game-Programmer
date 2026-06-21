# AGENTS.md — Toge Game Programmer

Panduan untuk AI coding agent yang bekerja di project Unity ini.

## Project Overview

Unity game project (URP) dengan gameplay code di `Assets/_Roby/Scripts/`. Framework internal **RAXY** dipakai untuk core systems (state machine, inventory, input, animation, dll.). Script game ditulis C# dengan **UniTask**, **Odin Inspector**, dan **Animancer**.

**Prinsip utama:** Agent hanya menyentuh file **`.cs`**. Semua asset Unity (prefab, scene, ScriptableObject instance, dll.) dikonfigurasi manual di Unity Editor oleh developer.

---

## ⛔ Hard Rules — Jangan Sentuh Asset Unity

**JANGAN** read, write, edit, create, atau delete file di bawah ini kecuali user **secara eksplisit** meminta:

| Kategori | Ekstensi / path |
|----------|-----------------|
| Prefab | `*.prefab` |
| Scene | `*.unity` |
| ScriptableObject instance | `*.asset` |
| Unity meta | `*.meta` |
| Material / Shader | `*.mat`, `*.shader`, `*.shadergraph`, `*.shadersubgraph` |
| Model / Texture / Audio | `*.fbx`, `*.png`, `*.jpg`, `*.wav`, `*.mp3`, dll. |
| Animator / Animation clip | `*.controller`, `*.anim` |
| Input Actions asset | `*.inputactions` |
| Addressables / Localization | file di `AddressableAssetsData/`, `LocalizationSettings/` |
| Generated / cache | `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `UserSettings/` |
| Build content assets | `Assets/_Roby/_Build Contents/**` (kecuali ada `.cs` di sana) |
| Third-party plugins | `Assets/Plugins/**`, `Packages/**` |

### Kenapa?

- File YAML Unity (prefab, scene, asset) **sangat besar** → boros token context window.
- Edit manual via text **rawan corrupt** reference GUID di Unity.
- Konfigurasi inspector (serialized fields, references) **harus** lewat Unity Editor.

### Kalau task butuh perubahan asset

Jangan edit asset. Sebagai gantinya:

1. Ubah hanya script `.cs` yang relevan (class, field, `[SerializeField]`, Odin attributes).
2. Beri instruksi singkat ke developer tentang apa yang perlu di-assign di Inspector, contoh:
   > "Di Inspector `GlobalManager`, assign `Item Database SO` ke field `ItemDatabase`."

---

## ✅ Scope Kerja Agent — Hanya Script C#

### Lokasi utama (prioritas)

```
Assets/_Roby/Scripts/
├── Game Manager/           # GlobalManager, GameplayDependencyManager, party manager
├── Game Implementation/
│   ├── Inventory/          # Item SO classes, factory, inventory extensions
│   └── Unit/               # Controller, brain, state machine, hero/enemy
└── ToGaProTest/            # Shared test utilities
```

### File `.cs` yang boleh disentuh

- **Semua** `.cs` di `Assets/_Roby/Scripts/**`
- **Editor scripts** di `Assets/_Roby/Editor/**` (jika ada)
- **Jangan** sentuh `.cs` di `Assets/Plugins/**` atau `Packages/**` kecuali diminta eksplisit

### ScriptableObject — class vs instance

| Boleh | Jangan |
|-------|--------|
| Edit **class** `.cs` (mis. `HeroDataSO.cs`, `ItemBaseSO.cs`) | Edit **instance** `.asset` (mis. `Hero Database SO.asset`) |
| Tambah field/property baru di SO class | Assign reference antar asset via YAML |

---

## Architecture

```
GlobalManager (bootstrap)
    └── ItemDatabase, HeroDatabase (SO references — di-assign di Editor)

GameplayDependencyManager
    └── GameplayPartyManager, dll.

UnitControllerBase
    ├── UnitMovement
    ├── AnimancerController
    ├── Brain_Exploration (ActiveUnitBrainExploration, dll.)
    └── UnitStateMachine_Exploration
            └── Idle / Run / Sprint states
```

- **State machine:** `UnitStateMachineBase` → `UnitStateMachine_Exploration` dengan state di `State Machine/Exploration/`
- **Brain pattern:** `BrainExplorationBase` + config SO (`*ConfigSO.cs`)
- **Combat units:** `CombatUnitControllerBase` → `HeroController`, `EnemyController`
- **Inventory:** RAXY Inventory + custom `ItemBaseSO` hierarchy, `ItemFactory`, `InventoryManager`

---

## Code Style

Ikuti konvensi yang sudah ada di codebase:

- **Namespace:** kebanyakan class global (tanpa namespace) — jangan introduce namespace baru tanpa alasan.
- **Async:** pakai `Cysharp.Threading.Tasks.UniTask`, bukan `Task` atau coroutine, kecuali pattern existing beda.
- **Inspector:** pakai Odin attributes (`[TitleGroup]`, `[ShowInInspector]`, dll.) sesuai style file sekitarnya.
- **Init pattern:** `ISepObject` + `Init()` / `PreInit()` via RAXY Core bootstrap.
- **Regions:** `#region` dipakai untuk grouping (ISepObject, IBootstrapper, dll.) — ikuti file yang sudah ada.
- **Minimize scope:** diff kecil, fokus ke task. Jangan refactor unrelated code.
- **Comments:** hanya untuk logic non-obvious; code harus self-explanatory.

### Dependencies (via Packages/manifest.json)

- `com.raxy.*` — RAXY Framework (core, statemachine, inventory, input, animation, dll.)
- `com.cysharp.unitask` — UniTask
- `com.kybernetik.animancer` — Animancer
- Odin Inspector — via `Assets/Plugins/Sirenix/`

---

## Commands

Project ini adalah Unity project — **tidak ada CLI build/test standar** di repo.

| Action | Cara |
|--------|------|
| Compile check | Buka project di Unity Editor → tunggu script compile |
| Play test | Unity Editor Play Mode |
| Git | Jangan commit kecuali user minta eksplisit |

Agent **tidak perlu** scan `Library/`, `Temp/`, atau folder generated lainnya.

---

## Search & Exploration Tips

Untuk hemat token saat explore codebase:

1. **Glob/grep hanya** `Assets/_Roby/Scripts/**/*.cs`
2. **Skip** `Assets/_Roby/_Build Contents/`, `Assets/Plugins/`, `Packages/`, `Library/`
3. Jangan `read` file `.prefab`, `.unity`, `.asset` — cari info dari class `.cs` counterpart-nya
4. Kalau butuh tahu field serialized apa di prefab, baca class MonoBehaviour-nya, bukan prefab YAML-nya

---

## Git

- `.gitignore` sudah exclude `Library/`, `Temp/`, `*.csproj`, `*.sln`, dll.
- Jangan stage/commit file di `Library/` atau generated artifacts.
- Hanya commit `.cs` (dan `.meta` hanya jika Unity otomatis generate saat user buat file baru di Editor — agent jangan buat `.meta` manual).
