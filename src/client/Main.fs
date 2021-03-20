module FelizServerless.Main

// open Feliz
// open Browser.Dom
open Fable.Core.JsInterop

// Use Elmish.HMR Rather than Feliz.useElmish where dispatch stops working after a HMR. 
open Elmish
open Elmish.React
open Elmish.HMR

importAll "./styles/global.scss"

// The Feliz way of starting the App
// ReactDOM.render (AppView.FelizRouter(), document.getElementById "feliz-app")

//The Elmish way of starting the App
Program.mkProgram App.init App.update AppView.Router
|> Program.withReactBatched "feliz-app"
|> Program.run