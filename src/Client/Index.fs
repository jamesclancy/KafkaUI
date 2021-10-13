module Index

open Elmish
open Fable.Remoting.Client
open Shared

open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Fable.MaterialUI.MaterialDesignIcons

open Client.Models
open Client.Pages.Brokers.Models
open Client.Pages.Topics.Models
open Client.Pages.Consumers.Models

open Shared.Contracts


let consumersApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IConsumersApi>

let topicsApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITopicsApi>

let brokersApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IBrokersApi>



let init () : Model * Cmd<Msg> =
    let model =
        { Page = Home
          SystemThemeMode = Light
          CustomThemeMode = None
          Connection = None
          PageModel = LoadingViewModel }

    model, Cmd.none

let processNavigateRequest p m =
    let cmd =
        match p with
        | Brokers -> Cmd.ofMsg (LoadBrokerSummaryViewModel |> BrokerMsg)
        | Topics -> Cmd.ofMsg (LoadTopicSummaryViewModel |> TopicMsg)
        | Consumers -> Cmd.ofMsg (LoadConsumerGroupSummaryViewModel |> ConsumerMsg)
        | _ -> Cmd.none

    { m with Page = p }, cmd


let update (msg: Msg) (m: Model) : Model * Cmd<Msg> =

    let executionContext =
        { ConsumerApi = consumersApi
          TopicApi = topicsApi
          BrokerApi = brokersApi }

    match msg with
    | Navigate p -> processNavigateRequest p m
    | SetSystemThemeMode mode -> { m with SystemThemeMode = mode }, Cmd.none
    | ToggleCustomThemeMode ->
        { m with
              CustomThemeMode =
                  match m.CustomThemeMode with
                  | None -> Some Dark
                  | Some Dark -> Some Light
                  | Some Light -> None },
        Cmd.none
    | BrokerMsg bm ->
        if m.Page = Brokers then
            Client.Pages.Brokers.BrokerSummary.update executionContext.BrokerApi bm m
        else
            m, Cmd.none
    | TopicMsg bm ->
        if m.Page = Topics then
            Client.Pages.Topics.TopicSummary.update executionContext.TopicApi bm m
        else
            m, Cmd.none
    | ConsumerMsg bm ->
        if m.Page = Consumers then
            Client.Pages.Consumers.ConsumerGroupSummary.update executionContext.ConsumerApi bm m
        else
            m, Cmd.none

let view model dispatch = Client.Layout.RootView(model, dispatch)
