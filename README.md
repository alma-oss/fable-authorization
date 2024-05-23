Fable.Authorization
===================

> Fable library for secured api request, work with the User and Tokens.

---

## Install

Add following into `paket.dependencies`
```
source https://nuget.pkg.github.com/almacareer/index.json username: "%PRIVATE_FEED_USER%" password: "%PRIVATE_FEED_PASS%"
# LMC Nuget dependencies:
nuget Alma.Fable.Authorization
```

NOTE: For local development, you have to create ENV variables with your github personal access token.
```sh
export PRIVATE_FEED_USER='{GITHUB USERNANME}'
export PRIVATE_FEED_PASS='{TOKEN}'	# with permissions: read:packages
```

Add following into `paket.references`
```
Alma.Fable.Authorization
```

## Use

### User
User is set to a `local storage` with default key `user`. You should change it to a app specific key.

#### Set up a User key
```fs
open Alma.Fable.Authorization

User.setUserKey "my-domain.user"
```

### Securer api calls

First of all you need to have an Api (defined in Shared project of your SAFE app).
```fs
// Shared
open Alma.Authorization.Common

type IMyApi = {
    // Public actions
    Login: Username * Password -> AsyncResult<User, string>

    // Secured actions
    LoadData: SecuredApiCall<unit, Data list, string> // Where unit is a request data, Data list is the response and string is an error
}
```

Define in the server.
```fs
open Alma.ErrorHandling
open Alma.ErrorHandling.Result.Operators
open Alma.Authorization
open Alma.Authorization.Common

// Server
let inline private (>?>) authorize action =
    Authorize.authorizeAction
        currentApplication.Authorization
        id
        logAuthorizationError
        authorize
        action

let (api: IMyApi) = {
    Login = fun (username, password) -> asyncResult {
        let! credentials =
            (username, password)
            |> Credentials.deserialize <@> CredentialsError.format

        return!
            credentials
            |> User.login currentApplication.Authorize
    }

    LoadData = Authorize.withLogin >?> fun () -> asyncResult {
        return [ (* list of Data *) ]
    }
}
```

Then define a proxy to your Api.
```fs
// Client
open Alma.Authorization.Common
open Alma.Fable.Authorization
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

let login = Server.api.Login

//
// Secured actions
//

let loadData: unit -> AsyncResult<Data list, Secure.SecureError<MyErrorType>> = Secure.api MyErrorType Server.api.LoadData
```

## Release
1. Increment version in `Alma.Fable.Profiler.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)

### Build
```bash
./build.sh build
```

### Tests
```bash
./build.sh -t tests
```
