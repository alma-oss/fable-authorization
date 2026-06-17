# Preferred Patterns

## Core Principles

- Set the storage key exactly once, at application startup, before any `load`/`save`. The default key is `"user"`; override it with `User.setUserKey` so the key is app-specific and will not collide with other apps on the same origin.
- Let `Secure.api` own the full lifecycle of a secured call: loading the user, attaching the token, renewing the token on success, and logging out on a token error. Do not reimplement these steps by hand.
- Keep the session as the single source of truth in `localStorage`. Read and write it only through the `User` module so the `UserDto` (camelCase) on-disk format stays consistent.

## Recommended API Usage

- After login, persist the returned `User` with `User.save`. To end a session explicitly, call `User.delete`.
- Expose each secured client function by partially applying `Secure.api liftError serverApiCall`, leaving `data` as the only remaining argument. The result is `Async<Result<'Success, SecureError<'Error>>>`. See `examples.md` → Realistic Example.
- `User.load` returns `User option`. It returns `None` when nothing is stored or when the stored value cannot be deserialized; in the deserialization-failure case it also clears the corrupt entry.

## Error Handling

- Match on `SecureError<'Error>` at the call site: `TokenError` means the session is no longer valid and the user has already been logged out (the stored user was deleted); `OtherError` carries every other failure, including authorization and generic server errors.
- Provide `liftError` to convert the library's internal `string` messages (e.g. when no user is logged in) into your API's own error type, so the whole call returns a single, uniform error type.

## Composition

- The server endpoint must be typed as `SecuredApiCall<'Data, 'Success, 'Error>` in the Shared project. `Secure.api` consumes exactly that shape, so client and server stay in sync through one type.
- Build a Fable.Remoting proxy of the Shared API, then wrap only the secured endpoints with `Secure.api`; leave public endpoints (login, etc.) called directly. See `examples.md` → Integration Example.

## Integration with Other Libraries

- Persistence is delegated to `Alma.Fable.Storage` (`LocalStorage`); you never touch the browser API directly.
- Shared types (`User`, `Username`, `JWT`, `SecureRequest`, `SecuredApiCall`, `SecuredRequestError`, `RenewedToken`, `SecurityToken`) come from `Alma.Authorization.Common` and are shared with the server companion library — use those types rather than redefining them.

## Naming Conventions

- Both modules are `[<RequireQualifiedAccess>]`; always call them qualified (`User.load`, `Secure.api`).
- Use a descriptive, namespaced storage key (e.g. `"my-domain.user"`) to avoid clashes.
- Name wrapped client functions after the action they expose, with an explicit signature ending in `Async<Result<'Success, SecureError<'Error>>>` for clarity.

## Testing Recommendations

- The library's behavior is driven by `localStorage` state, so tests should arrange that state through the `User` module (`save`/`delete`) and assert on the `SecureError` branch returned. See `examples.md` → Test Example.
- Cover the three outcomes of a secured call: success (token renewed), token error (user logged out), and other error (state untouched).
