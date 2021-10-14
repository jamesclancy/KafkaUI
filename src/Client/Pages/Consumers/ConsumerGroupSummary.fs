module Client.Pages.Consumers.ConsumerGroupSummary

open Client.Models
open Client.Pages.Consumers.Models
open Client.Pages.CommonLayoutItems
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Feliz.MaterialUI.MaterialTable
open Elmish
open Browser.Types
open Shared.Contracts


let summaryCards (model: ConsumerGroupSummaryViewModel) dispatch =
    Html.div [ Mui.grid [ grid.container true
                          grid.spacing._3
                          grid.justify.flexStart
                          grid.alignItems.flexStart
                          grid.alignContent.center
                          grid.children [ yield!
                                              model.ConsumerStates
                                              |> Seq.map (fun x -> summaryCard grid.xs._4 x.Key (sprintf "%i" x.Value)) ] ] ]

let consumerGroupTable model dispatch =
    genericFormatedTable
        ""
        false
        [ columns.column [ column.title "Name"
                           column.field<ConsumerGroupSummaryConsumerGroupSummaryViewModel>
                               (fun rd -> nameof rd.ConsumerGroupName) ]
          columns.column [ column.title "State"
                           column.field<ConsumerGroupSummaryConsumerGroupSummaryViewModel> (fun rd -> nameof rd.State) ]
          columns.column [ column.title "Lag"
                           column.field<ConsumerGroupSummaryConsumerGroupSummaryViewModel> (fun rd -> nameof rd.Lag)
                           column.type'.numeric ]
          columns.column [ column.title "Members"
                           column.field<ConsumerGroupSummaryConsumerGroupSummaryViewModel> (fun rd -> nameof rd.Members)
                           column.type'.numeric ]
          columns.column [ column.title "Asn. Partitions"
                           column.field<ConsumerGroupSummaryConsumerGroupSummaryViewModel>
                               (fun rd -> nameof rd.AssignedPartitions)
                           column.type'.numeric ]
          columns.column [ column.title "Asn. Topics"
                           column.field<ConsumerGroupSummaryConsumerGroupSummaryViewModel>
                               (fun rd -> nameof rd.AssignedTopics)
                           column.type'.numeric ]
          columns.column [ column.title "Partition Asn. Strategy"
                           column.field<ConsumerGroupSummaryConsumerGroupSummaryViewModel>
                               (fun rd -> nameof rd.PartitionAssignmentStrategy) ] ]
        model.ConsumerGroups
        [ actions.action<ConsumerGroupSummaryConsumerGroupSummaryViewModel>
              (fun rowData ->
                  [ action.icon (Mui.icon [ settingsIcon [] ])
                    action.tooltip (sprintf "Configure %s" rowData.ConsumerGroupName)
                    action.onClick
                        (fun _ _ ->
                            (rowData.ConsumerGroupName
                             |> LoadManageConsumerGroupViewModel
                             |> ManageConsumerGroupMsg
                             |> dispatch)) ]) ]

let consumerGroupSummaryView =
    FunctionComponent.Of(
        (fun (model: ConsumerGroupSummaryViewModel, dispatch: ConsumerMsg -> unit) ->
            Html.div [ prop.children [ Mui.grid [ grid.container true
                                                  grid.spacing._3
                                                  grid.justify.flexStart
                                                  grid.alignItems.flexStart
                                                  grid.alignContent.center
                                                  grid.children [ Mui.grid [ grid.item true
                                                                             grid.xs._10
                                                                             grid.children [ Mui.typography [ typography.component'
                                                                                                                  "h1"
                                                                                                              typography.variant.h3
                                                                                                              typography.children
                                                                                                                  "Consumer Groups" ]
                                                                                             Mui.typography [ typography.paragraph
                                                                                                                  true
                                                                                                              typography.children
                                                                                                                  "A consumer group is a pool of consumers listening for events coming from a topic." ] ] ]
                                                                  Mui.grid [ grid.item true
                                                                             grid.alignContent.flexEnd
                                                                             grid.xs._12
                                                                             grid.children [ Mui.divider [] ] ]
                                                                  Mui.grid [ grid.item true
                                                                             grid.alignContent.flexEnd
                                                                             grid.xs._12
                                                                             grid.children [ summaryCards model dispatch ] ]
                                                                  Mui.grid [ grid.item true
                                                                             grid.alignContent.flexEnd
                                                                             grid.xs._12
                                                                             grid.children [ Mui.divider [] ] ]
                                                                  Mui.grid [ grid.item true
                                                                             grid.alignContent.flexEnd
                                                                             grid.xs._12
                                                                             grid.children [ consumerGroupTable
                                                                                                 model
                                                                                                 dispatch ] ]


                                                                   ] ]



                                        ] ]),
        "ConsumerGroupSummaryPage",
        memoEqualsButFunctions
    )



let update (api: IConsumersApi) (msg: ConsumerMsg) (m: Model) : Model * Cmd<Msg> =
    if m.Page <> Consumers then
        m, Cmd.none
    else
        match msg with
        | LoadConsumerGroupSummaryViewModel ->
            m,
            Cmd.OfAsync.perform
                (Client.Pages.Consumers.Data.fetchSummary api)
                ()
                (fun x ->
                    x
                    |> ConsumerGroupSummaryViewModel.FromConsumerGroupSummary
                    |> LoadedConsumerGroupSummaryViewModel
                    |> ConsumerMsg)
        | LoadedConsumerGroupSummaryViewModel vm ->
            { m with
                  PageModel = vm |> ConsumerGroupSummaryViewModel },
            Cmd.none
        | ManageConsumerGroupMsg vm -> ManageConsumerGroup.update api vm m
        | _ -> m, Cmd.none

let view (model: PageModel) (dispatch: Msg -> unit) =
    let consumerGroupDispatch = fun x -> (x |> ConsumerMsg |> dispatch)

    let manageConsumerGroupDispatch =
        fun x ->
            (x
             |> ManageConsumerGroupMsg
             |> ConsumerMsg
             |> dispatch)

    match model with
    | ConsumerGroupSummaryViewModel b -> consumerGroupSummaryView (b, consumerGroupDispatch)
    | ManageConsumerGroupViewModel b -> ManageConsumerGroup.manageConsumerGroupView (b, manageConsumerGroupDispatch)
    | _ -> Mui.typography "Loading"
