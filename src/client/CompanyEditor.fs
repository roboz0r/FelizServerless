[<RequireQualifiedAccess>]
module FelizServerless.HazopCompany

open FelizServerless.Hazop

/// A string to be used on the src property of an img tag
/// https://stackoverflow.com/a/9464137/14134059
let getImgString (image: Image) =
    $"data:{image.MIMEType};base64,{toBase64String image.Data}"

type State =
    {
        Name: string
        Logo: Image option
        ImgString: string option
        Phone: string
        Address: Address option
    }
    member this.ToCompany() : Company =
        {
            Name = this.Name
            Logo = this.Logo
            Phone = this.Phone
            Address = this.Address
        }

    static member OfCompany(company: Company) =
        {
            Name = company.Name
            Logo = company.Logo
            Phone = company.Phone
            Address = company.Address
            ImgString = company.Logo |> Option.map getImgString
        }

let init company = State.OfCompany company

type Msg =
    | NameChanged of string
    | LogoChanged of Image option
    | PhoneChanged of string
    | AddressChanged of Address option
    | AddressView of HazopAddress.Msg

let update msg (state: State) =
    match msg with
    | NameChanged name -> { state with Name = name }
    | LogoChanged logo ->
        { state with
            Logo = logo
            ImgString = logo |> Option.map getImgString
        }
    | PhoneChanged phone -> { state with Phone = phone }
    | AddressChanged address -> { state with Address = address }
    | AddressView msg ->
        match state.Address with
        | Some address ->
            let address = HazopAddress.update msg address
            { state with Address = Some address }
        | None -> state
