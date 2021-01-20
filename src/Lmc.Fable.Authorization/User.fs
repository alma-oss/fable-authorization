namespace Lmc.Fable.Authorization

[<RequireQualifiedAccess>]
module User =
    open Lmc.Fable.Storage
    open Lmc.Authorization.Common

    let mutable private userKey = "user"

    let setUserKey customUserKey =
        userKey <- customUserKey

    let save (user: User) =
        user |> LocalStorage.save userKey

    let load (): User option =
        match userKey |> LocalStorage.load<User> with
        | Ok user -> Some user
        | Error _ ->
            LocalStorage.delete userKey
            // todo - log error
            None

    let renewToken (RenewedToken token) =
        match load() with
        | Some user -> { user with Token = token } |> save
        | _ -> ()

    let delete () =
        LocalStorage.delete userKey
