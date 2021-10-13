module Client.Pages.Brokers.BrokerSummary

open Client.Models
open Client.Pages.Brokers.Models
open Client.Pages.CommonLayoutItems
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Feliz.MaterialUI.MaterialTable
open Elmish
open Browser.Types
open Client.Pages.Brokers.BrokerEditConfiguration
open Client.Pages.Brokers.BrokerConfiguration
open Client.Pages.Brokers.BrokerRollingRestartConfigurationDialog
open Client.Pages.Brokers.Data
open Shared.Contracts


let summaryCards model dispatch =
    Html.div [ Mui.grid [ grid.container true
                          grid.spacing._3
                          grid.justify.flexStart
                          grid.alignItems.flexStart
                          grid.alignContent.center
                          grid.children [ summaryCard grid.xs._3 "Brokers" (sprintf "%i" model.BrokerCount)
                                          summaryCard grid.xs._3 "Controller" (sprintf "%b" model.Controller)
                                          summaryCard grid.xs._3 "Version" (sprintf "%s" model.Version)
                                          summaryCard
                                              grid.xs._3
                                              "Similar Configuration"
                                              (sprintf "%b" model.SimilarConfiguration) ] ] ]

let brokerTable model dispatch =
    Client.Pages.CommonLayoutItems.genericFormatedTable
        (sprintf "Cluster Id: %s" model.ClusterId)
        true
        [ columns.column [ column.title "Id"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.Id) ]
          columns.column [ column.title "Rack"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.Rack) ]
          columns.column [ column.title "Partition Skew"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.PartitionSkew)
                           column.type'.numeric ]
          columns.column [ column.title "Leaders"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.Leaders)
                           column.type'.numeric ]
          columns.column [ column.title "LeaderSkew"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.LeaderSkew)
                           column.type'.numeric ]
          columns.column [ column.title "Replicas"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.Replicas)
                           column.type'.numeric ]
          columns.column [ column.title "Under Replicated Partitions"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.UnderReplicatedPartitions)
                           column.type'.numeric ]
          columns.column [ column.title "Size In Bytes"
                           column.field<BrokerSummaryDetailViewModel> (fun rd -> nameof rd.SizeInBytes)
                           column.type'.numeric ] ]
        model.AvailableBrokers
        [ actions.action<BrokerSummaryDetailViewModel>
              (fun rowData ->
                  [ action.icon (Mui.icon [ settingsIcon [] ])
                    action.tooltip (sprintf "Configure %s" rowData.Id)
                    action.onClick
                        (fun _ _ ->
                            { BrokerId = rowData.Id }
                            |> BrokerLoadConfiguration
                            |> dispatch) ]) ]

let manageTopicActionMenu (model: BrokerSummaryViewModel) dispatch =
    Mui.grid [ grid.item true
               grid.alignContent.flexEnd
               grid.xs._12
               grid.children [ Mui.buttonGroup [ buttonGroup.variant.text
                                                 buttonGroup.orientation.horizontal
                                                 buttonGroup.fullWidth true
                                                 prop.children [ Mui.button [ prop.text "Rebalance Cluster"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      RebalanceCluster |> dispatch) ]
                                                                 Mui.button [ prop.text "Rolling Restart"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      ShowBrokerRollingRestartDialog
                                                                                      |> BrokerRollingRestartMsg
                                                                                      |> dispatch) ] ] ] ] ]


let viewFromGenericPage (model: BrokerSummaryViewModel) (dispatch: BrokerMsg -> unit) =
    genericPageLayout
        "Brokers"
        "A Kakfa broker is an actual server/node which recieves events from a consumer and eventually delivers them to a consumer."
        [ RollingRestartDialog model dispatch ]
        (manageTopicActionMenu model dispatch)
        (summaryCards model dispatch)
        [ Mui.grid [ grid.item true
                     grid.alignContent.flexEnd
                     grid.xs._12
                     grid.children [ (brokerTable model dispatch) ] ] ]


let brokerSummaryView =
    FunctionComponent.Of(
        (fun (model: BrokerSummaryViewModel, dispatch: BrokerMsg -> unit) -> viewFromGenericPage model dispatch),
        "BrokersSummaryPage",
        memoEqualsButFunctions
    )



let update (api: IBrokersApi) (msg: BrokerMsg) (m: Model) : Model * Cmd<Msg> =
    if m.Page <> Brokers then
        m, Cmd.none
    else
        match msg with
        | BrokerSummaryViewModelLoaded vm ->
            { m with
                  PageModel = vm |> BrokerSummaryViewModel },
            Cmd.none
        | BrokerLoadConfiguration r ->
            m,
            Cmd.OfAsync.perform
                (fetchConfigurationForBroker api)
                r.BrokerId
                (fun x ->
                    x
                    |> BrokerConfigurationDetailViewModelLoaded
                    |> BrokerMsg)
        | BrokerConfigurationDetailViewModelLoaded vm ->
            { m with
                  PageModel = vm |> BrokerConfigurationDetailViewModel },
            Cmd.none
        | BrokerRollingRestartMsg rr -> Client.Pages.Brokers.BrokerRollingRestartConfigurationDialog.update api rr m
        | BrokerEditMsg p ->
            match m.PageModel with
            | BrokerConfigurationDetailViewModel b -> startEditorDialog api b p m
            | _ -> m, Cmd.none
        | LoadBrokerSummaryViewModel ->
            m,
            Cmd.OfAsync.perform
                (Client.Pages.Brokers.Data.fetchSummary api)
                ()
                (fun x -> x |> BrokerSummaryViewModelLoaded |> BrokerMsg)
        | _ -> m, Cmd.none

let view (model: PageModel) (dispatch: Msg -> unit) =
    let brokerDispatch x = x |> BrokerMsg |> dispatch

    match model with
    | BrokerSummaryViewModel b -> brokerSummaryView (b, brokerDispatch)
    | BrokerConfigurationDetailViewModel b -> brokerConfigurationView (b, brokerDispatch)
    | _ -> Mui.typography "Loading"
