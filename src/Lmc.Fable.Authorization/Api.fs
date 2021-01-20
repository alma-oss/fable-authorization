namespace Lmc.Fable.Authorization

[<RequireQualifiedAccess>]
module Api =
    open Lmc.Authorization.Common

    let private handleResult (onSuccess: ('Success -> 'Action)) (onError: ('ErrorMessage -> 'Action)) = function
        | Ok success -> success |> onSuccess
        | Error error -> error |> onError

    let call<'Success, 'Action, 'ErrorMessage>
        (onSuccess: ('Success -> 'Action))
        (onError: ('ErrorMessage -> 'Action))
        (call: Async<Result<'Success, 'ErrorMessage>>): Async<'Action> = async {
            let! callResult = call

            return callResult |> handleResult onSuccess onError
        }

    let private handleSecuredResult (onSuccess: ('Success -> 'Action)) (onError: ('ErrorMessage -> 'Action)) (onAuthorizationError: ('ErrorMessage -> 'Action)) = function
        | Ok (token, success) ->
            token |> User.renewToken
            success |> onSuccess

        | Error (SecuredRequestError.TokenError message) ->
            User.delete ()
            message |> onAuthorizationError

        | Error (SecuredRequestError.AuthorizationError error)
        | Error (SecuredRequestError.OtherError error) ->
            error |> onError

    let callSecured<'Success, 'Action, 'ErrorMessage>
        (onSuccess: ('Success -> 'Action))
        (onError: ('ErrorMessage -> 'Action))
        (onAuthorizationError: ('ErrorMessage -> 'Action))
        (call: SecuredAsyncResult<'Success, 'ErrorMessage>): Async<'Action> = async {
            let! callResult = call

            return callResult |> handleSecuredResult onSuccess onError onAuthorizationError
        }
