module FelizServerless.Main

open Feliz
open Browser.Dom
open Fable.Core.JsInterop

importAll "./styles/global.scss"

ReactDOM.render(
    App.Router(),
    document.getElementById "feliz-app"
)