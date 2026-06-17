---
name: fable-authorization
description: Use whenever generating or reviewing F# (Fable) client code for SAFE-stack apps that manages a logged-in user session in browser localStorage or composes secured API calls. Trigger on Secure.api, SecureError, TokenError/OtherError, User.setUserKey, User.load, User.save, User.delete, User.renewToken, SecuredApiCall, SecureRequest, RenewedToken, liftError, automatic token renewal, or automatic logout on token errors. Also trigger on mentions of Alma.Fable.Authorization, the User module, or the Secure module.
---

# Fable-Authorization

Library: [alma-oss/fable-authorization](https://github.com/alma-oss/fable-authorization)
NuGet: `Alma.Fable.Authorization`

## Purpose

`Alma.Fable.Authorization` is a Fable (F#-to-JavaScript) library for client-side authorization in SAFE-stack web apps. It persists the logged-in user (username + JWT) in browser `localStorage`, composes secured API calls by attaching the stored token, renews the token after a successful call, and logs the user out automatically when the server reports a token error.

## When to Use

- Persisting or reading a logged-in user session on the client (`User` module).
- Calling a server endpoint typed as `SecuredApiCall` from a Fable client (`Secure.api`).
- Wiring automatic token renewal and automatic logout into client API calls.

## When NOT to Use

- Server-side authorization or token verification (that belongs to the server companion library, not this one).
- Non-Fable / non-browser code — this library depends on browser `localStorage`.
- Plain unauthenticated/public API calls — call the proxy endpoint directly instead of through `Secure.api`.

## Main Concepts

- **`User` module** — `[<RequireQualifiedAccess>]` CRUD for the session in `localStorage` (`save`, `load`, `delete`, `renewToken`, `setUserKey`).
- **`userKey`** — mutable storage key (default `"user"`); set once at startup with `User.setUserKey`.
- **`UserDto`** — private camelCase record (`userName`, `token`) used only for JSON serialization to `localStorage`.
- **`Secure` module** — `[<RequireQualifiedAccess>]` composer that turns a `SecuredApiCall` into a callable client function.
- **`Secure.api`** — `liftError -> apiCall -> data -> Async<Result<'Success, SecureError<'Error>>>`; loads the user, builds the request, handles the result.
- **`SecureError<'Error>`** — result wrapper: `TokenError` (session invalid → logged out) or `OtherError` (any other failure).
- **`liftError`** — caller-supplied function lifting a `string` into the API's own error type.

## Related Libraries

- `Alma.Authorization.Common` — shared types reused here: `User`, `Username`, `JWT`, `SecureRequest`, `SecuredApiCall`, `SecuredRequestError`, `RenewedToken`, `SecurityToken`.
- `Alma.Fable.Storage` — the `LocalStorage` abstraction used for browser persistence.

## Keywords for Search

fable-authorization, Alma.Fable.Authorization, Secure.api, SecureError, TokenError, OtherError, User.setUserKey, User.load, User.save, User.delete, User.renewToken, userKey, UserDto, SecuredApiCall, SecureRequest, SecurityToken, RenewedToken, liftError, localStorage, SAFE stack, token renewal, automatic logout, JWT, Fable client authorization

## Reference Files

- For composition principles and recommended API usage, read `references/preferred-patterns.md`.
- For known pitfalls and incorrect assumptions, read `references/anti-patterns.md`.
- For worked code examples, read `references/examples.md`.
