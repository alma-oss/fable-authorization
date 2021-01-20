namespace Lmc.Fable.Authorization

[<RequireQualifiedAccess>]
module Authorization =
    open Lmc.Authorization.Common

    //
    // Secured actions
    //

    type Secure<'RequestData, 'Action> = 'RequestData -> Result<SecureRequest<'RequestData>, 'Action>

    let secure<'RequestData, 'Action, 'ErrorMessage>
        (formatError: string -> 'ErrorMessage)
        (onError: 'ErrorMessage -> 'Action)
        : Secure<'RequestData, 'Action> =

        fun data ->
            match User.load() with
            | Some user ->
                Ok {
                    Token = SecurityToken (user.Token)
                    RequestData = data
                }
            | _ ->
                onError (formatError "User is not logged in.")
                |> Error

    module Operators =
        /// Compose api call with secure request creation
        let inline (>?>) (secure: Secure<'RequestData, 'Action>) (apiCall: SecureRequest<'RequestData> -> Async<'Action>) =
            secure >> function
            | Ok secureRequest -> secureRequest |> apiCall
            | Error error -> async { return error }
