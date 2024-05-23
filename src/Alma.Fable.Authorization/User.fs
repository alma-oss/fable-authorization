namespace Alma.Fable.Authorization

[<RequireQualifiedAccess>]
module User =
    open Alma.Fable.Storage
    open Alma.Authorization.Common

    // fsharplint:disable
    type private UserDto = {
        userName: string
        token: string
    }
    // fsharplint:enable

    [<RequireQualifiedAccess>]
    module private UserDto =
        let fromUser { Username = Username username; Token = JWT token } = { userName = username; token = token }
        let toUser { userName = username; token = token } = { Username = Username username; Token = JWT token }

    let mutable private userKey = "user"

    let setUserKey customUserKey =
        userKey <- customUserKey

    let save (user: User) =
        user
        |> UserDto.fromUser
        |> LocalStorage.save userKey

    let delete () =
        LocalStorage.delete userKey

    let load (): User option =
        match userKey |> LocalStorage.load<UserDto> with
        | Ok user -> Some (user |> UserDto.toUser)
        | Error _ ->
            delete ()
            None

    let renewToken (RenewedToken token) =
        match load() with
        | Some user -> { user with Token = token } |> save
        | _ -> ()
