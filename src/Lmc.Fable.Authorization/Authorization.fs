namespace Lmc.Fable.Authorization

[<RequireQualifiedAccess>]
module Authorization =
    open Lmc.Authorization.Common

    //
    // Secured actions
    //

    let secure onError data =
        match User.load() with
        | Some user ->
            Ok {
                Token = SecurityToken (user.Token)
                RequestData = data
            }
        | _ ->
            onError "User is not logged in."
            |> Error

    module Operators =
        /// Compose api call with secure request creation
        let inline (>?>) secure apiCall =
            secure >> function
            | Ok secureRequest -> secureRequest |> apiCall
            | Error error -> async { return error }
