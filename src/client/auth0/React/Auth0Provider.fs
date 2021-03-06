module rec Fable.Auth0.Provider
// fsharplint:disable

open System
open Fable.Core
open Fable.Core.JS

module JSX =
    type Element() =
        interface Fable.React.ReactElement

type CacheLocation = Fable.Auth0.Global.CacheLocation

[<AllowNullLiteral>]
type IAppState =
    abstract returnTo: string option with get, set

    [<Emit "$0[$1]{{=$2}}">]
    abstract Item: key:string -> obj option with get, set

/// The main configuration to instantiate the `Auth0Provider`.
[<AllowNullLiteral>]
type IAuth0ProviderOptions =
    /// The child nodes your Provider has wrapped
    abstract children: Fable.React.ReactElement option with get, set
    /// By default this removes the code and state parameters from the url when you are redirected from the authorize page.
    /// It uses `window.history` but you might want to overwrite this if you are using a custom router, like `react-router-dom`
    /// See the EXAMPLES.md for more info.
    abstract onRedirectCallback: (IAppState -> unit) option with get, set
    /// By default, if the page url has code/state params, the SDK will treat them as Auth0's and attempt to exchange the
    /// code for a token. In some cases the code might be for something else (another OAuth SDK perhaps). In these
    /// instances you can instruct the client to ignore them eg
    ///
    /// ```jsx
    /// <Auth0Provider
    ///    clientId={clientId}
    ///    domain={domain}
    ///    skipRedirectCallback={window.location.pathname === '/stripe-oauth-callback'}
    /// >
    /// ```
    abstract skipRedirectCallback: bool option with get, set
    /// Your Auth0 account domain such as `'example.auth0.com'`,
    /// `'example.eu.auth0.com'` or , `'example.mycompany.com'`
    /// (when using [custom domains](https://auth0.com/docs/custom-domains))
    abstract domain: string with get, set
    /// The issuer to be used for validation of JWTs, optionally defaults to the domain above
    abstract issuer: string option with get, set
    /// The Client ID found on your Application settings page
    abstract clientId: string with get, set
    /// The default URL where Auth0 will redirect your browser to with
    /// the authentication result. It must be whitelisted in
    /// the "Allowed Callback URLs" field in your Auth0 Application's
    /// settings. If not provided here, it should be provided in the other
    /// methods that provide authentication.
    abstract redirectUri: string option with get, set
    /// The value in seconds used to account for clock skew in JWT expirations.
    /// Typically, this value is no more than a minute or two at maximum.
    /// Defaults to 60s.
    abstract leeway: float option with get, set
    /// The location to use when storing cache data. Valid values are `memory` or `localstorage`.
    /// The default setting is `memory`.
    abstract cacheLocation: CacheLocation option with get, set
    /// If true, refresh tokens are used to fetch new access tokens from the Auth0 server. If false, the legacy technique of using a hidden iframe and the `authorization_code` grant with `prompt=none` is used.
    /// The default setting is `false`.
    ///
    /// **Note**: Use of refresh tokens must be enabled by an administrator on your Auth0 client application.
    abstract useRefreshTokens: bool option with get, set
    /// A maximum number of seconds to wait before declaring background calls to /authorize as failed for timeout
    /// Defaults to 60s.
    abstract authorizeTimeoutInSeconds: float option with get, set
    /// Changes to recommended defaults, like defaultScope
    abstract advancedOptions: Auth0ProviderOptionsAdvancedOptions option with get, set
    /// Maximum allowable elapsed time (in seconds) since authentication.
    /// If the last time the user authenticated is greater than this value,
    /// the user must be reauthenticated.
    abstract maxAge: U2<string, float> option with get, set
    /// The default scope to be used on authentication requests.
    /// The defaultScope defined in the Auth0Client is included
    /// along with this scope
    abstract scope: string option with get, set
    /// The default audience to be used for requesting API access.
    abstract audience: string option with get, set
    /// If you need to send custom parameters to the Authorization Server,
    /// make sure to use the original parameter name.
    [<Emit "$0[$1]{{=$2}}">]
    abstract Item: key:string -> obj option with get, set

[<AllowNullLiteral>]
type Auth0ProviderOptionsAdvancedOptions =
    /// The default scope to be included with all requests.
    /// If not provided, 'openid profile email' is used. This can be set to `null` in order to effectively remove the default scopes.
    ///
    /// Note: The `openid` scope is **always applied** regardless of this setting.
    abstract defaultScope: string option with get, set
