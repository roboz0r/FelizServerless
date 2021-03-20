[<RequireQualifiedAccess>]
module FelizServerless.HazopCompanyView

open Feliz
open Feliz.MaterialUI
open FelizServerless.Hazop
open Fable.Core
open Browser.Types

let private console = JS.console
type private Msg = HazopCompany.Msg

[<ReactComponent>]
let View (state: HazopCompany.State) dispatch =

    Mui.container [
        Mui.typography "Company Details"
        Mui.textField [
            textField.label "Name"
            textField.defaultValue state.Name
            textField.onChange (Msg.NameChanged >> dispatch)
        ]
        match state.ImgString with
        | Some imgStr ->
            Mui.cardMedia [
                cardMedia.image imgStr
                prop.alt (
                    state.Logo
                    |> Option.map (fun x -> x.Filename)
                    |> Option.defaultValue ""
                )
            ]
        | None -> Mui.typography "Select a logo"
        Html.input [
            prop.type'.file
            prop.onInput
                (fun e ->
                    let file =
                        (e.target :?> HTMLInputElement).files.[0]

                    let fileName = file.name

                    let mimeType =
                        Mime.getType fileName
                        |> function
                        | Some mimeType when mimeType.StartsWith("image") -> Some mimeType
                        | _ -> None

                    match mimeType with
                    | Some mimeType ->
                        let reader = Browser.Dom.FileReader.Create()

                        reader.onload <-
                            fun evt ->
                                {
                                    Data = (evt.target :?> FileReader).result :?> _
                                    Filename = fileName
                                    MIMEType = mimeType
                                }
                                |> Some
                                |> Msg.LogoChanged
                                |> dispatch

                        reader.onerror <- fun evt -> console.log "Error reading file"

                        reader.readAsArrayBuffer (file)
                    | None ->
                        // TODO Display an error to the user
                        console.log $"Image file not selected")
        ]
        Mui.textField [
            textField.label "Phone"
            textField.defaultValue state.Phone
            textField.onChange (Msg.PhoneChanged >> dispatch)
        ]
        match state.Address with
        | Some address ->
            Mui.formControlLabel [
                formControlLabel.label "Has Address"
                formControlLabel.control (
                    Mui.checkbox [
                        checkbox.checked' true
                        checkbox.onChange (fun (_: Event) -> None |> Msg.AddressChanged |> dispatch)
                    ]
                )
            ]

            HazopAddressView.View address (Msg.AddressView >> dispatch)
        | None ->
            Mui.formControlLabel [
                formControlLabel.label "Has Address"
                formControlLabel.control (
                    Mui.checkbox [
                        checkbox.checked' false
                        checkbox.onChange
                            (fun (_: Event) ->
                                Address.Empty
                                |> Some
                                |> Msg.AddressChanged
                                |> dispatch)
                    ]
                )
            ]
    ]
