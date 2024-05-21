namespace Alma.Fable.Authorization

[<RequireQualifiedAccess>]
module Secure =
    open Alma.Authorization.Common

    type SecureError<'Error> =
        | TokenError of 'Error
        | OtherError of 'Error

    type private SecureData<'Data, 'Error> = 'Data -> Result<SecureRequest<'Data>, SecuredRequestError<'Error>>

    let private secure liftError: SecureData<'Data, 'Error> = fun data ->
        match User.load() with
        | Some user ->
            Ok {
                Token = SecurityToken (user.Token)
                RequestData = data
            }
        | _ ->
            Error (SecuredRequestError.TokenError (liftError "User not logged in"))

    /// Compose api call with secure request creation
    let inline private (>?>) (secure: SecureData<'Data, 'Error>) (apiCall: SecuredApiCall<'Data, 'Success, 'Error>) =
        secure >> function
        | Ok secureRequest -> secureRequest |> apiCall
        | Error error -> async { return Error error }

    let private handleSecuredResult: Result<RenewedToken * 'Success, SecuredRequestError<'Error>> -> Result<'Success, SecureError<'Error>> = function
        | Ok (token, success) ->
            token |> User.renewToken
            Ok success

        | Error (SecuredRequestError.TokenError message) ->
            User.delete ()
            Error (TokenError message)
        | Error (SecuredRequestError.AuthorizationError error)
        | Error (SecuredRequestError.OtherError error) ->
            Error (OtherError error)

    let api liftError (apiCall: SecuredApiCall<'Data, 'Success, 'Error>) data: Async<Result<'Success, SecureError<'Error>>> =
        async {
            let! result = data |> (secure liftError >?> apiCall)

            return result |> handleSecuredResult
        }
