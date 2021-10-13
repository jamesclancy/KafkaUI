module Client.Models

open Client.Pages.Brokers.Models
open Client.Pages.Topics.Models
open Client.Pages.Consumers.Models
open Shared.Contracts


type ExecutionContext =
    { ConsumerApi: IConsumersApi
      TopicApi: ITopicsApi
      BrokerApi: IBrokersApi }

type Page =
    | Home
    | Brokers
    | Topics
    | Consumers
    | Settings
    static member ParentMenuPages = [ Home; Brokers; Topics; Consumers ]

type ThemeMode =
    | Light
    | Dark


let pageTitle =
    function
    | Home -> "Home"
    | Brokers -> "Brokers"
    | Topics -> "Topics"
    | Consumers -> "Consumers"
    | Settings -> "Settings"

type HomeViewModel = { BrokerCount: int }


type SettingsViewModel =
    { CurrentlyConnected: bool
      ConnectionString: string option }


type KafkaConnection =
    { IsConnected: bool
      Url: string
      Port: int }

type PageModel =
    | HomeViewModel of HomeViewModel
    | LoadingViewModel
    | BrokerSummaryViewModel of BrokerSummaryViewModel
    | BrokerConfigurationDetailViewModel of BrokerConfigurationDetailViewModel
    | TopicSummaryViewModel of TopicsSummaryViewModel
    | ManageTopicViewModel of ManageTopicViewModel
    | ConsumerGroupSummaryViewModel of ConsumerGroupSummaryViewModel
    | ManageConsumerGroupViewModel of ManageConsumerGroupViewModel
    | SettingsViewModel of SettingsViewModel

type Model =
    { Page: Page
      SystemThemeMode: ThemeMode
      CustomThemeMode: ThemeMode option
      Connection: KafkaConnection option
      PageModel: PageModel }

type Msg =
    | Navigate of Page
    | SetSystemThemeMode of ThemeMode
    | ToggleCustomThemeMode
    | BrokerMsg of BrokerMsg
    | TopicMsg of TopicMsg
    | ConsumerMsg of ConsumerMsg

(*
let preventDefault (e: Microsoft.FSharp.Control.Event) =
  e.preventDefault ()
*)
