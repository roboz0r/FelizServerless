[<RequireQualifiedAccess>]
module FelizServerless.HazopAddressView

open Feliz
open Feliz.MaterialUI
open FelizServerless.Hazop

type private Msg = HazopAddress.Msg

[<ReactComponent>]
let View (address: HazopAddress.State) dispatch =

    Mui.container [
        Mui.typography "Address Details"
        Mui.textField [
            textField.label "Line 1"
            textField.defaultValue address.Line1
            textField.onChange (Msg.Line1Changed >> dispatch)
        ]
        Mui.textField [
            textField.label "Line 2"
            textField.defaultValue address.Line2
            textField.onChange (Msg.Line2Changed >> dispatch)
        ]
        Mui.textField [
            textField.label "City"
            textField.defaultValue address.City
            textField.onChange (Msg.CityChanged >> dispatch)
        ]
        Mui.textField [
            textField.label "State"
            textField.defaultValue address.State
            textField.onChange (Msg.StateChanged >> dispatch)
        ]
        Mui.textField [
            textField.label "Postcode / Zip"
            textField.defaultValue address.Postcode
            textField.onChange (Msg.PostcodeChanged >> dispatch)
        ]
        Mui.textField [
            textField.label "Country"
            textField.defaultValue address.Country
            textField.onChange (Msg.CountryChanged >> dispatch)
        ]
    ]
