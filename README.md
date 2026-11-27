# 911, What's Your Emergency?
A short 2D top-down psychological survival-horror game about a kidnapped girl, a damaged phone, and the voice of a 911 operator. Built with Unity 2022.3 LTS for WebGL by Slafurry Studios.

<p align="center">
  <img src="https://github.com/user-attachments/assets/258e6ae0-0b67-4e54-ac6a-bde65b53e741" width="100%" alt="911-whats-your-emergency banner" />
</p>

<br>
<p align="left">
  <img src="https://img.shields.io/badge/UNITY-ffffff?style=for-the-badge&logo=unity&logoColor=000000" />
  <img src="https://img.shields.io/badge/STATUS • IN DEVELOPMENT-000000?style=for-the-badge&logo=git&logoColor=ffffff" />
  <img src="https://img.shields.io/badge/MADE BY SLAFURRY STUDIOS-ffffff?style=for-the-badge&logo=gamepad&logoColor=000000" />
</p>

---

## ABOUT
You play as a young girl kidnapped and trapped inside a dangerous man's house in a remote forest. She cannot fight back, but she has found an old phone with a faulty charger, and a 911 operator on the line may be her only chance of survival. Experienced entirely through a frightened child's eyes, the game explores fear, vulnerability, and the desperate hope of being rescued.

---

## PLAY IT ON
<p align="left">
  <a href="https://slafurrystudios.itch.io/911-whats-your-emergency"><img src="https://img.shields.io/badge/ITCH.IO-ffffff?style=for-the-badge&logo=itch.io&logoColor=000000" /></a>
</p>

---

## FEATURES
- Stealth-based survival: hide in cabinets, wardrobes, and under furniture instead of fighting
- Phone resource management: keep an old phone charged by adjusting a faulty charger while you talk
- 911 communication: answer the operator's questions about the kidnapper and your surroundings
- Throwable items: toss small objects to stun the kidnapper and buy yourself time
- Dynamic enemy AI: the kidnapper reacts to noise, grows more aggressive, and destroys hiding places
- Gut instinct system: visual warnings that hint at nearby danger
- Environmental storytelling: previous victims' belongings and documents hint at what happened here

---

## ARCHITECTURE
The project follows the Sourdough-Starter layout: persistent cross-scene Systems (Audio, Save, Scene, Localization) built on shared `GameSystem` base classes, per-session Managers that coordinate gameplay, per-instance controllers and entities, and reactive UI observers.

```
Assets/_Game/
├── 00_Scripts/
│   ├── Core/       — base classes & interfaces (Singleton, GameSystem, Manager)
│   ├── Systems/    — persistent cross-scene services (Audio, Save, Scene, Localization)
│   ├── Manager/    — per-session gameplay coordinators
│   ├── Game/       — per-instance controllers & entities (+ Triggers/)
│   ├── UI/         — reactive observers & screen coordination
│   └── Utils/      — generic reusable helpers (GameFeel, extensions)
├── 01_Objects/     — prefabs & data, grouped by domain
├── 02_Art/         — Drive-synced sprites
├── 03_Audio/       — GameAudioMixer + Music/SFX (Drive-synced clips)
├── 04_Scenes/      — 00_Boot.unity + 8 more (all in Build Settings)
└── 05_Settings/    — URP, Input
```

See [`ARCHITECTURE.md`](./ARCHITECTURE.md) for the full breakdown of base classes, naming conventions, and design decisions.

---

## GETTING STARTED

### Prerequisites
- Unity `2022.3.62f3` (see `ProjectSettings/ProjectVersion.txt`)
- Git (no Git LFS — binaries are stored as plain blobs)

### Setup
```bash
git clone https://github.com/Slafurry-Studios/911-Whats-Your-Emergency-.git
cd 911-Whats-Your-Emergency-
```
Open the project in Unity Hub, let it import, then open `Assets/_Game/04_Scenes/00_Boot.unity`.

---

## GITHUB WORKFLOWS

CI/CD covers Google Drive asset sync (`track`/`retrieve`), a **PR compile gate** (`unity-compile`), and Unity builds deployed to itch.io. Setup (secrets/variables) and usage are documented in [`GITHUB_WORKFLOWS.md`](./GITHUB_WORKFLOWS.md).

---

## TECH STACK
![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=ffffff)
![C#](https://img.shields.io/badge/C%23-000000?style=flat-square&logo=csharp&logoColor=ffffff)
![Cinemachine](https://img.shields.io/badge/Cinemachine-000000?style=flat-square&logo=unity&logoColor=ffffff)
![Newtonsoft.Json](https://img.shields.io/badge/Newtonsoft.Json-000000?style=flat-square&logo=json&logoColor=ffffff)

---

## TEAM
| Role | Name |
|---|---|
| Project Manager, Game Designer, Unity Programmer | Lord Zaini |
| Level Designer, Unity Programmer | TiramissuCake |
| 2D Artist, UI Designer | Fekanme |
| Audio Engineer | Ifanitas |
| Narrative Designer | americano |

---

## CONTRIBUTING

Contributions are welcome — see [`CONTRIBUTING.md`](./CONTRIBUTING.md) for setup, architecture rules, and PR guidelines.

To report a vulnerability, please email **slafurrystudios@gmail.com** instead of opening a public issue — see [`SECURITY.md`](./SECURITY.md).

---

## CONNECT
<p align="left">
  <a href="https://github.com/Slafurry-Studios"><img src="https://img.shields.io/badge/GITHUB-ffffff?style=for-the-badge&logo=github&logoColor=000000" /></a>
  <a href="https://www.linkedin.com/company/slafurry-studios/"><img src="https://img.shields.io/badge/LINKEDIN-000000?style=for-the-badge&logo=linkedin&logoColor=ffffff" /></a>
  <a href="https://slafurrystudios.itch.io/"><img src="https://img.shields.io/badge/ITCH.IO-ffffff?style=for-the-badge&logo=itch.io&logoColor=000000" /></a>
</p>
