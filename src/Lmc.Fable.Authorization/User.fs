namespace Lmc.Fable.Authorization

[<RequireQualifiedAccess>]
module User =
    open Lmc.Fable.Storage
    open Lmc.Authorization.Common

    // fsharplint:disable
    type private UserDto = {
        userName: string
        token: string
    }
    // fsharplint:enable

    [<RequireQualifiedAccess>]
    module private UserDto =
        let fromUser { Username = Username username; Token = JWTToken token } = { userName = username; token = token }
        let toUser { userName = username; token = token } = { Username = Username username; Token = JWTToken token }

    let mutable private userKey = "user"

    let setUserKey customUserKey =
        userKey <- customUserKey

    let save (user: User) =
        user
        |> UserDto.fromUser
        |> LocalStorage.save userKey

    let load (): User option =
        match userKey |> LocalStorage.load<UserDto> with
        | Ok user -> Some (user |> UserDto.toUser)
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
