# Examples

All example code for this skill lives here. Examples are ordered from simplest to most complete; each is self-contained.

## Basic Example — set the storage key at startup

```fsharp
open Alma.Fable.Authorization

// Run once during application initialization, before any load/save.
User.setUserKey "my-domain.user"
```

## Realistic Example — expose a secured client function

```fsharp
open Alma.Authorization.Common
open Alma.Fable.Authorization

// Error type owned by the client.
type WebApiError = WebApiError of string

// Server endpoint typed in the Shared project as:
//   LoadItems: SecuredApiCall<unit, Item list, string>
// `serverApi` is the built proxy (see Integration Example).

let loadItems: unit -> Async<Result<Item list, Secure.SecureError<WebApiError>>> =
    Secure.api WebApiError serverApi.LoadItems

// Call site: the session is loaded, the token attached, renewed on success,
// and the user logged out automatically on a token error.
async {
    match! loadItems () with
    | Ok items -> printfn "loaded %d items" (List.length items)
    | Error (Secure.TokenError _) -> () // session gone — redirect to login
    | Error (Secure.OtherError (WebApiError msg)) -> eprintfn "failed: %s" msg
}
```

## Integration Example — Shared API + Fable.Remoting proxy

```fsharp
// Shared project
open Alma.Authorization.Common

type IWebApi = {
    // Public action
    Login: Username * Password -> Async<Result<User, string>>
    // Secured action
    LoadItems: SecuredApiCall<unit, Item list, string>
}
```

```fsharp
// Client project
open Alma.Authorization.Common
open Alma.Fable.Authorization
open Fable.Remoting.Client
open Shared

let serverApi: IWebApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builderForClient
    |> Remoting.buildProxy<IWebApi>

type WebApiError = WebApiError of string

// Public endpoint: called directly.
let login = serverApi.Login

// Secured endpoint: wrapped with Secure.api.
let loadItems = Secure.api WebApiError serverApi.LoadItems
```

## Test Example — arrange session state, assert on the result branch

```fsharp
open Alma.Authorization.Common
open Alma.Fable.Authorization

// No session stored -> Secure.api short-circuits before calling the server.
let ``secured call without a logged-in user yields a token error`` () =
    async {
        User.delete ()

        let serverApi: SecuredApiCall<unit, int, string> =
            fun _ -> async { return Ok (RenewedToken (JWT "unused"), 42) }

        match! Secure.api id serverApi () with
        | Error (Secure.TokenError _) -> () // expected: not logged in
        | other -> failwithf "unexpected result: %A" other
    }
```

## Full Workflow — login, persist, secured call, logout

```fsharp
open Alma.Authorization.Common
open Alma.Fable.Authorization

type WebApiError = WebApiError of string

let run (username, password) =
    async {
        // 1. Log in via the public endpoint and persist the session.
        match! login (username, password) with
        | Error msg -> return Error (WebApiError msg)
        | Ok user ->
            User.save user

            // 2. Call a secured endpoint; token is attached and renewed on success.
            let loadItems = Secure.api WebApiError serverApi.LoadItems
            match! loadItems () with
            | Ok items ->
                return Ok items
            | Error (Secure.TokenError e) ->
                // 3. Session already cleared by Secure.api — surface as logged out.
                return Error e
            | Error (Secure.OtherError e) ->
                return Error e
    }
```
