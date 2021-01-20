Fable.Authorization
===================

> Fable library for secured api request, work with the User and Tokens.

---

## Install

Add following into `paket.dependencies`
```
git ssh://git@bitbucket.lmc.cz:7999/archi/nuget-server.git master Packages: /nuget/
# LMC Nuget dependencies:
nuget Lmc.Fable.Authorization
```

Add following into `paket.references`
```
Lmc.Fable.Authorization
```

## Use

### User
User is set to a `local storage` with default key `user`. You should change it to a app specific key.

#### Set up a User key
```fs
open Lmc.Fable.Authorization

User.setUserKey "my-domain.user"
```

### Securer api calls

First of all you need to have an Api (defined in Shared project of your SAFE app).
```fs
// Shared
open Lmc.Authorization.Common

type IMyApi = {
    // Public actions
    Login: Username * Password -> AsyncResult<User, string>

    // Secured actions
    LoadData: SecureRequest<unit> -> SecuredAsyncResult<Data list, string>
}
```

Then define a proxy to your Api.
```fs
open Lmc.Authorization.Common
open Lmc.Fable.Authorization
open Shared

type MyErrorType = MyErrorType of string

module private Server =
    open Fable.Remoting.Client

    /// A proxy you can use to talk to server directly
    let api : IMyApi =
        Remoting.createApi()
        |> Remoting.withRouteBuilder Route.builderForClient
        |> Remoting.buildProxy<IMyApi>

//
// Public actions
//

let login onSuccess onError credentials =
    Server.api.Login credentials |> Api.call onSuccess onError

//
// Secured actions
//

let secure onError: Secure<'RequestData, 'Action> = Authorization.secure MyErrorType onError

open Authorization.Operators

let loadData onSuccess onError onAuthorizationError =
    secure onError >?> (Server.api.LoadData >> Api.callSecured onSuccess onError onAuthorizationError)
```

## Release
1. Increment version in `Fable.Authorization.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it
4. Run `$ fake build target release`
5. Go to `nuget-server` repo, run `faket build target copyAll` and push new versions

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)
- [FAKE](https://fake.build/fake-gettingstarted.html)

### Build
```bash
fake build
```

### Watch
```bash
fake build target watch
```
