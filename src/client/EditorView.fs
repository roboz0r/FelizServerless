module FelizServerless.EditorView

open System
open Feliz
open Fable.MaterialUI.Icons
open Feliz.MaterialUI

type private Msg<'Msg,'TError> = EditorMsg<'Msg,'TError>

// [<ReactComponent>]
// let Render (renderState:'T -> ('Msg -> unit) -> ReactElement) (state: Editor<'T,'TError>) dispatch =

//     match state with
//     | Working state -> 
//             Mui.container [
//                 renderState state.Current (Msg.Changed >> dispatch)
//             ]
//     | Pending state ->
//         Mui.container [
//             Mui.dialog [
//                 dialog.disableBackdropClick true
//                 dialog.open' true
//                 dialog.children [
//                     Mui.dialogTitle "Loading..."
//                 ]
//             ]

//             renderState state.Current (Msg.Changed >> dispatch)
//         ]
//     | EditorError(state, error) ->
//         Mui.container [
//             Mui.dialog [
//                 dialog.disableBackdropClick true
//                 dialog.open' true
//                 dialog.children [
//                     Mui.dialogTitle "Error"
//                     Mui.dialogContentText error
//                     Mui.button [
//                         button.variant.contained
//                         button.color.secondary
//                         button.children "Revert"
//                         button.endIcon (undoIcon [])
//                         prop.onClick (fun _ -> Msg.Clean |> dispatch)
//                     ]
//                 ]
//             ]
//             renderState state.Current (Msg.Changed >> dispatch)
//         ]
