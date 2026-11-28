# Roadmap

Sliding Block Escape is a puzzle game, and a portfolio project for a **Unity mobile game engineer** role.
This plan grows it from an offline game into a small **live game**: tested and built by CI, backed by a
server that trusts nothing the client says, saved in the cloud, and tuned with remote config and A/B tests.

New levels are paused. The phases below are the game work now.

| Phase | Goal | Status | Target |
| --- | --- | --- | --- |
| 0 | Offline game: Core rules, BFS solver, level editor, 10 levels, saved progress, 116 tests | Done | - |
| 1 | CI/CD with GitHub Actions | Done | Week 1 |
| 2 | C# backend: verified solutions, cloud save, leaderboard, on Google Cloud | Planned | Weeks 2-4 |
| 3 | Profiling pass on a real Android phone | Planned | During interviews |
| 4 | LiveOps: remote config, A/B test, analytics, daily level | Planned | During interviews |
| 5 | Stretch: 2-player race over WebSockets + Protobuf, Addressables | Stretch | If time is left |

**Apply gate:** apply after Phase 2c (the server verifies solutions and is deployed). The rest of Phase 2 and
the later phases are good things to talk about in interviews as "what I am working on now".

## What changed from the first plan, and why

1. **Profiling (now Phase 3) comes before LiveOps (now Phase 4).** Mobile optimization and profiling are a
   *requirement* in the job post; LiveOps and A/B testing are an *advantage*. Profiling is also small (a few
   days) and does not need the backend, so it gives real numbers for the README sooner.
2. **Phase 2 starts with client-side data work (2a).** Reading the code showed three things the server
   needs that do not exist yet:
   - Levels are `LevelData` ScriptableObjects. A server cannot load Unity assets, so levels must also exist
     as **JSON**.
   - Levels are found by **index** in the `LevelPack`. If a level is moved, index 3 means a different level,
     and saved best scores point to the wrong level. Levels need a **stable id** (`"level-03"`).
   - `ILevelProgress` saves only the current level index. Cloud save and a leaderboard need more: the highest
     level reached and the best move count per level.
3. **Remote config is our own `/config` endpoint**, not Unity Remote Config (UGS). The backend already
   exists after Phase 2, and building it ourselves shows the client-server skill the job asks for.
4. **Addressables move to the stretch phase.** After 2a, levels are JSON, and the server can send new levels
   without Addressables. Addressables are still useful for art and bigger content, so they stay in Phase 5.

## Facts checked before planning (2026-11-25)

| Question | Answer | How it was checked |
| --- | --- | --- |
| Does `Core` build outside Unity? | Yes, as a `netstandard2.1` library with C# 9 | Built a csproj that links `Assets/_Project/Core/**/*.cs` |
| Do the `Core` tests run with `dotnet test`? | Yes, 116 of 116 pass (NuGet NUnit 3.14, .NET 10) | Ran `dotnet test` on a csproj that links Core and Core.Tests |
| Can CI use NUnit 4? | No, not without changes. The tests use `Assert.AreEqual` (76 times), which NUnit 4 moved to `ClassicAssert` | grep in `Core.Tests` |
| Can the repo hold a csproj today? | No. `.gitignore` ignores `*.csproj` and `*.sln` (Unity makes its own). A hand-written project needs an exception | Read `.gitignore` |
| Is there a GameCI Docker image for our Unity version? | Yes: `unityci/editor:ubuntu-6000.6.2f1-android-4` | Docker Hub tags API |
| What does GameCI need for a Personal license? | Secrets `UNITY_LICENSE` (the `.ulf` file text), `UNITY_EMAIL`, `UNITY_PASSWORD` | game.ci activation docs |

## Phase 1: CI/CD with GitHub Actions

**Goal:** every push runs the tests and builds an Android APK, with no manual step.

**Design decisions**

- **Two jobs, fast first.** Job 1 runs `dotnet test` on Core: no Unity, no license, about 1 minute. Job 2 runs
  Unity (GameCI) Edit Mode tests and builds the APK: slow (10-20 minutes) and needs the license secrets. If
  the rules are broken, job 1 fails in a minute and you don't wait for Unity.
- **A hand-written .NET project for Core, outside `Assets/`.** It *links* the Unity source files
  (`<Compile Include="../../Assets/_Project/Core/**/*.cs" />`) instead of copying them. One copy of the rules
  code, compiled by Unity for the game and by `dotnet` for CI and (in Phase 2) the server.
- **`LangVersion 9.0`** in that project, the C# version Unity uses. Otherwise CI could accept C# 12 code that
  Unity then refuses to compile.
- **The APK is a build artifact,** downloadable from the Actions run. No store upload yet (no signing key in CI).
- **CD part comes in Phase 2:** a deploy job that ships the server to Cloud Run after the tests pass on `main`.

**Done when:** a push to `main` shows two green jobs, an APK artifact, and a CI badge in the README.

**Proves for the job post:** CI/CD, unit testing, clean OOP split (rules that run without the engine).

## Phase 2: C# backend (server-authoritative), cloud save, leaderboard

**Goal:** a real backend the game talks to. The main story for interviews:

> The client sends only its list of moves. The server rebuilds the board from the level, replays every move
> with the **same `Core` rules code** the game uses, and only then accepts the score. A hacked client can't
> send "I solved it in 1 move".

Example: Level 03 has par 3. A client sends `moves: [{blockId:2, distance:-1}, {blockId:0, distance:4}]`.
The server replays move 1 (legal), move 2 (`Board.CanMove` is false because block 1 is in the way) and
answers `400 { "error": "illegal_move", "index": 1 }`. No score is saved.

### 2a. Data contracts in the client (no server yet)

- A stable **`Id`** on `LevelData` (`"level-01"`), checked by the level editor (not empty, unique in the pack).
- An editor menu **Sliding Block → Export Levels to JSON** that writes `levels.json` (id, width, height, par,
  blocks). The server reads this file. A Core test (or CI step) checks it matches the assets.
- A **`SolutionReplay`** (pure C#, in `Core`): board + move list → result (solved / illegal move at index N /
  not solved). Tested in `Core.Tests`. The client and the server both use it.
- **Record the moves** in the game (a `MoveHistory` the presenter adds to after each applied move).
- **Richer progress:** `PlayerProgress { MaxLevelReached, BestMoves[levelId], Version }`, saved as JSON,
  with a one-time migration from the old `CurrentLevelIndex` PlayerPrefs key so no player loses progress.

### 2b. The server (ASP.NET Core, C#)

- `dotnet/SlidingBlock.Server` references the Core project from Phase 1.
- Endpoints (all JSON, all under `/v1` so a later `/v2` can live next to old clients):

| Method and path | Body / answer | Notes |
| --- | --- | --- |
| `POST /v1/players` | → `{ playerId, token }` | Anonymous account. The client stores both. |
| `POST /v1/levels/{levelId}/solutions` | `{ moves: [...] }` → `{ valid, moveCount, par, rank }` | Replays with Core. Max 500 moves per request. |
| `GET /v1/save` | → `{ version, maxLevelReached, bestMoves }` | Cloud save of this player |
| `PUT /v1/save` | `{ version, ... }` → `200` or `409 + server copy` | Version check (see below) |
| `GET /v1/levels/{levelId}/leaderboard` | → top 10 + my rank | Redis sorted set |

- **Anonymous auth:** the server makes a random `playerId` and a secret `token`, stores only a hash of the
  token, and the client sends `Authorization: Bearer <token>`. Not `SystemInfo.deviceUniqueIdentifier`: it
  changes on reinstall on some devices, and a device id is personal data.
- **Interfaces in front of storage** (`ISaveRepository`, `ILeaderboard`), so tests use in-memory fakes and
  production uses Firestore and Redis.
- **Integration tests** with `WebApplicationFactory`: start the real app in memory, send real HTTP requests,
  check the answers (good solution, illegal move, wrong token, version conflict).

### 2c. Deploy on Google Cloud (apply gate)

- **First: a $5 budget alert** in Google Cloud Billing, before any resource exists.
- Docker image → Artifact Registry → **Cloud Run** (scales to zero, so no cost when nobody plays).
- **Firestore** for players and saves (free tier).
- The Phase 1 workflow gets a **deploy job** on `main`, after the tests pass. It logs in to Google with
  Workload Identity Federation (no long-lived JSON key in GitHub secrets).
- Cold start: with zero instances, the first request can take a few seconds. The client timeout and retry
  (2d) must handle that.

### 2d. The client: `IBackendClient`

- In `Services`, behind an interface, registered in VContainer like `ILevelProgress`.
- `UnityWebRequest` + UniTask (`await request.SendWebRequest().WithCancellation(token)`).
- JSON with Newtonsoft (`com.unity.nuget.newtonsoft-json`). `JsonUtility` can't read dictionaries or
  properties, and our data is properties by the project's style rules.
- **Timeout** per request (10 s), **retry with exponential backoff and jitter** (0.5 s, 1 s, 2 s, plus a random
  part) only for network errors, `5xx` and `429`. Never retry a `4xx`: the same request fails again.
- The game **never waits** for the network to let the player play.

### 2e. Cloud save: offline-first

- The **local save is the source of truth.** A win saves locally first, marks the save as "dirty", then tries
  to upload. No network → it stays dirty and uploads on the next start or win.
- **Version number for conflicts.** Phone A and phone B both have version 4. A uploads → server is version 5.
  B uploads with version 4 → `409 Conflict` + the server copy. B merges and uploads again with version 5.
- **Safe merge rule:** `maxLevelReached = max(a, b)`, `bestMoves[level] = min(a, b)` for each level. The
  order doesn't matter and merging twice gives the same result, so no progress is ever lost.

### 2f. Redis leaderboard

- One **sorted set** per level, `lb:level-03`, member = playerId, score = move count (lower is better).
- `ZADD lb:level-03 LT 7 player-abc` keeps only a lower (better) score. `ZRANK` gives my rank, `ZRANGE 0 9`
  the top 10.
- Hosted on **Upstash** (free tier, Redis protocol over TLS), with the `StackExchange.Redis` client.
- Why Redis here: the job's backend stack uses Redis, and a leaderboard is its classic use.

**Done when:** the APK from CI talks to the deployed server, a solved level shows a verified rank, and
progress survives reinstalling the app on a second phone.

**Proves for the job post:** client-server, REST APIs, JSON, server-authoritative state, local + remote
persistence, conflict handling, backend code in C#, cloud and managed services (Cloud Run, Firestore, Redis),
integration tests, CD.

## Phase 3: Profiling on a real Android phone

**Goal:** measured numbers, before and after, from a mid-range Android phone.

- Development build with **Autoconnect Profiler**; profile on the phone, not in the Editor.
- **CPU and frame time.** Check the frame rate first: on Android, Unity runs at 30 FPS unless
  `Application.targetFrameRate` is set.
- **GC allocations per frame**, especially while dragging (`BoardInput.Tick`, `BoardPresenter.PreviewDrag`,
  LitMotion). Goal: 0 B per frame while dragging.
- **Draw calls and batching** (Frame Debugger): one sprite atlas for the blocks.
- **Memory Profiler** snapshot: texture memory, texture compression (ASTC), total app memory.
- APK size and time to first frame.
- A README **Performance** table: metric, before, after, what changed.

**Proves:** mobile optimization, profiling, memory management.

## Phase 4: LiveOps

- **Remote config:** `GET /v1/config` → `{ hintButtonEnabled, dailyLevelId, ... }`. The client ships with
  default values, caches the last config, and uses the cache or defaults when offline.
- **A/B test:** group = stable hash of `playerId + experimentName` modulo 100; below 50 is group A. The server
  does this and returns the group in `/config`. Note: .NET's `string.GetHashCode()` is different in every run
  of the program, so it can't be used here (use FNV-1a or SHA-256).
- **Example test:** hint button vs no hint button. The hint is the first move of `BoardSolver.Solve` on the
  current board: the solver we already have.
- **Analytics events:** `level_start`, `level_complete { levelId, moves, seconds, group }`, sent in batches to
  `POST /v1/events`, stored in Firestore. Question to answer: does group A complete more levels?
- **Daily level:** `/config` names one level per day (UTC date → level from a pool). Every player gets the same
  one, with its own leaderboard.

**Proves:** LiveOps, remote config, A/B testing, data-driven design, product thinking.

## Phase 5 (stretch)

- **2-player race:** both players get the same level; first to solve wins. ASP.NET Core **WebSockets**, messages
  in **Protobuf** (`JoinRace`, `RaceStart { levelId }`, `MoveMade { playerId, move }`, `RaceFinished`). The
  server replays each move with Core (the same idea as 2b) and sends each player the other's progress. This
  is state synchronization with the server as the authority.
- **Addressables** for remote art and level packs.
- **Node.js:** a small service (for example the events endpoint) in TypeScript, if time is left.

**Proves:** WebSockets, Protobuf, real-time state sync, remote content.

## Job post requirements → phase

| Requirement in the job post | Where it is shown |
| --- | --- |
| Unity mobile development | Phase 0 (whole game, Android), Phase 3 |
| OOP, design patterns, data-driven | Phase 0 (MVP, DI, pure Core, ScriptableObjects), 2a (JSON levels), 4 (config) |
| Client-server, APIs, JSON | Phase 2b, 2d |
| WebSockets, Protobuf | Phase 5 |
| State synchronization | Phase 2e (save sync with versions), Phase 5 (race) |
| Mobile optimization, profiling, memory | Phase 3 |
| Local and remote data persistence | Phase 2a (local JSON), 2e (cloud save) |
| Unit testing | Phase 0 (116 tests), Phase 1 (in CI), 2b (server integration tests) |
| CI/CD | Phase 1, 2c (deploy job) |
| Product orientation | Phase 2e (no lost progress), Phase 4 (A/B test, analytics) |
| Advantage: backend code (C#) | Phase 2b |
| Advantage: cloud, managed services | Phase 2c (Cloud Run, Firestore), 2f (Upstash Redis) |
| Advantage: LiveOps, A/B tests, remote config | Phase 4 |
