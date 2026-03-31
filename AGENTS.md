# AGENTS.md — Alma.Fable.Authorization

## Project Purpose

`Alma.Fable.Authorization` is a Fable (F#-to-JavaScript) NuGet library for client-side authorization in SAFE stack web applications. It handles user session management via browser `localStorage`, secured API call composition, automatic token renewal on successful responses, and automatic user logout on token errors.

## Tech Stack

- **Language:** F# (.NET 10) compiled to JavaScript via Fable
- **Package manager:** Paket
- **Build system:** FAKE (F# Make) via `build.sh`
- **NuGet package:** `Alma.Fable.Authorization`
- **Repository:** <https://github.com/alma-oss/fable-authorization>

## Key Dependencies

- `FSharp.Core ~> 10.0`
- `Fable.Core ~> 4` — Fable compiler core
- `Alma.Authorization.Common ~> 7.0` — shared types (`User`, `Username`, `JWT`, `SecureRequest`, `SecuredApiCall`, `RenewedToken`)
- `Alma.Fable.Storage ~> 9.0` — `LocalStorage` abstraction for browser storage

## Commands

```bash
# Install dependencies
dotnet paket install

# Build
./build.sh build

# Run tests
./build.sh -t tests
```

## Project Structure

```
├── src/
│   └── Alma.Fable.Authorization/
│       ├── Alma.Fable.Authorization.fsproj  # Project file (includes fable/ content for Fable consumers)
│       ├── AssemblyInfo.fs                  # Auto-generated assembly info
│       ├── User.fs                          # User session management (localStorage)
│       ├── Secure.fs                        # Secured API call composition
│       └── paket.references                 # Package references
├── build/                                   # FAKE build scripts
├── paket.dependencies                       # Dependency definitions
└── fsharplint.json                          # Lint config
```

## Architecture

### Modules

1. **`User`** — manages user sessions in browser `localStorage`:
   - `User.save` / `User.load` / `User.delete` — CRUD for user data
   - `User.setUserKey` — customize storage key (default: `"user"`)
   - `User.renewToken` — updates stored token after successful secured call
   - Uses `UserDto` (camelCase) for JSON serialization to localStorage

2. **`Secure`** — composes secured API calls:
   - `Secure.api liftError apiCall data` — loads user from storage → creates `SecureRequest` → calls API → handles result:
     - **Success:** renews token in storage, returns `Ok 'Success`
     - **Token error:** deletes user from storage (logout), returns `Error (TokenError ...)`
     - **Other error:** returns `Error (OtherError ...)`
   - `SecureError<'Error>` — `TokenError | OtherError` wrapper

### Data Flow

```
Client code → Secure.api liftError Server.api.SecuredEndpoint data
    → User.load() from localStorage
    → create SecureRequest (token + data)
    → call server API (returns Result<RenewedToken * 'Success, SecuredRequestError>)
    → on success: User.renewToken, return Ok
    → on token error: User.delete (logout), return Error TokenError
```

## Conventions

- **Fable library** — source files are included in the NuGet package under `fable/` for Fable compilation
- **`UserDto`** uses camelCase properties (annotated with `// fsharplint:disable`) for JavaScript interop
- **`[<RequireQualifiedAccess>]`** on all modules
- **Private `>?>` operator** in `Secure` module — composes secure data creation with API call
- Depends on `Alma.Authorization.Common` shared types — both server (`fauthorization`) and client (`fable-authorization`) use the same type definitions
- **Mutable `userKey`** — set once at app startup via `User.setUserKey`

## CI/CD

| Workflow | Trigger | What it does |
|---|---|---|
| `tests.yaml` | PR, daily at 03:00 UTC | `./build.sh -t tests` on ubuntu-latest with .NET 10 |
| `publish.yaml` | Tag push (`X.Y.Z`) | `./build.sh -t publish` → NuGet.org |
| `pr-check.yaml` | PR | Blocks fixup commits, runs ShellCheck |

## Release Process

1. Increment `<Version>` in `src/Alma.Fable.Authorization/Alma.Fable.Authorization.fsproj`
2. Update `CHANGELOG.md`
3. Commit and push a git tag matching the version (e.g., `9.0.1`)

## Pitfalls

- **No docker-compose / no local environment** — this is a pure Fable library, no runtime services
- **No tests directory visible** — tests may not exist or may be in a different location
- **Fable content packaging** — the `.fsproj` includes `*.fsproj; *.fs;` as `Content` with `PackagePath="fable\"`. Do not change this without understanding Fable's NuGet consumption model
- **Paket.Restore.targets path** — uses `..\..\` relative path since the project is nested under `src/Alma.Fable.Authorization/`
- **Mutable state** — `User.userKey` is mutable; ensure `setUserKey` is called before any `load`/`save` operations
- **README mentions `Alma.Fable.Profiler.fsproj`** in release steps — this is a copy-paste error in the README; the actual file is `Alma.Fable.Authorization.fsproj`
