# Anti-Patterns

Each entry is **mistake → why it is wrong → fix**.

## Common Mistakes

- **Skipping `User.setUserKey` at startup.** → The session falls back to the default key `"user"`, which can collide with other apps on the same origin and is not app-specific. → Call `User.setUserKey "my-domain.user"` once during app initialization, before any `load`/`save`.

- **Reading or writing `localStorage` directly (or with a hand-rolled DTO).** → The on-disk format is the private camelCase `UserDto`; bypassing the `User` module produces an incompatible record that `User.load` cannot deserialize. → Always go through `User.save`, `User.load`, and `User.delete`.

- **Manually attaching the token and calling the proxy endpoint for secured actions.** → You lose automatic token renewal and automatic logout, and you duplicate the request-building logic. → Wrap the `SecuredApiCall` with `Secure.api liftError serverApiCall`.

- **Renewing the token by hand after a successful call.** → `Secure.api` already calls `User.renewToken` with the `RenewedToken` returned by the server; doing it again risks overwriting the session with stale data. → Trust `Secure.api`; never re-save the token yourself on success.

- **Treating `TokenError` as a retryable error.** → On `TokenError` the stored user has already been deleted (the session is gone), so retrying with the same state will keep failing. → Handle `TokenError` as "logged out" — redirect to login rather than retry.

- **Forgetting `liftError`, or expecting the library's raw `string` messages.** → `Secure.api` lifts internal messages (such as "User not logged in") through `liftError` into your error type; without a correct `liftError` the error type will not unify. → Pass a `liftError` that maps `string` into your API's own error type.

- **Assuming `User.load` always returns the stored user.** → It returns `User option`; on a deserialization failure it returns `None` *and* clears the corrupt entry as a side effect. → Pattern-match the `option` and treat `None` as "no valid session".

## Do Not Use / Avoid

- Do not use this library on the server or in non-browser contexts — it depends on browser `localStorage` via `Alma.Fable.Storage`.
- Do not redefine the shared types (`User`, `SecuredApiCall`, `SecureRequest`, `RenewedToken`, etc.); import them from `Alma.Authorization.Common` so client and server agree.

## Legacy Usage

- The old `Api` module no longer exists — it was renamed to `Secure`. Code or docs referencing `Api.call` / `Api.callSecured` are outdated; use `Secure.api`.
- The project's own README release step names `Alma.Fable.Profiler.fsproj`; that is a copy-paste error. The real project file is `Alma.Fable.Authorization.fsproj`.
