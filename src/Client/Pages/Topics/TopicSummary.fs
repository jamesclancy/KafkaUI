module Client.Pages.Topics.TopicSummary

open Client.Models
open Client.Pages.Topics.Models
open Client.Pages.CommonLayoutItems
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Feliz.MaterialUI.MaterialTable
open Elmish
open Browser.Types
open Shared.Contracts


let summaryCards model dispatch =
    Html.div [ Mui.grid [ grid.container true
                          grid.spacing._3
                          grid.justify.flexStart
                          grid.alignItems.flexStart
                          grid.alignContent.center
                          grid.children [ summaryCard grid.xs._4 "Topics" (sprintf "%i" model.TopicCount)
                                          summaryCard grid.xs._4 "Partitions" (sprintf "%i" model.Partitions)
                                          summaryCard
                                              grid.xs._4
                                              "Under Replicated Partitions"
                                              (sprintf "%i" model.UnderReplicatedPartitions)
                                          summaryCard
                                              grid.xs._4
                                              "Topics Without Leader"
                                              (sprintf "%i" model.TopicsWithoutLeader)
                                          summaryCard
                                              grid.xs._4
                                              "In Sync Replica Issues"
                                              (sprintf "%i" model.InSyncReplicaIssues) ] ] ]

let topicTable model dispatch =
    genericFormatedTable
        ""
        true
        [ columns.column [ column.title "Name"
                           column.field<TopicSummaryViewModel> (fun rd -> nameof rd.Name) ]
          columns.column [ column.title "Partitions"
                           column.field<TopicSummaryViewModel> (fun rd -> nameof rd.Partitions) ]
          columns.column [ column.title "Under Replicated Partitions"
                           column.field<TopicSummaryViewModel> (fun rd -> nameof rd.UnderReplicatedPartitions)
                           column.type'.numeric ]
          columns.column [ column.title "Count"
                           column.field<TopicSummaryViewModel> (fun rd -> nameof rd.CountAsInt32)
                           column.type'.numeric ]
          columns.column [ column.title "Size (Bytes)"
                           column.field<TopicSummaryViewModel> (fun rd -> nameof rd.SizeInBytes)
                           column.type'.numeric ]
          columns.column [ column.title "Consumers"
                           column.field<TopicSummaryViewModel> (fun rd -> nameof rd.Consumers)
                           column.type'.numeric ] ]
        model.AvailableTopics
        [ actions.action<TopicSummaryViewModel>
              (fun rowData ->
                  [ action.icon (Mui.icon [ settingsIcon [] ])
                    action.tooltip (sprintf "Configure %s" rowData.Name)
                    action.onClick
                        (fun _ _ ->
                            rowData.Name
                            |> LoadManageTopicViewModel
                            |> ManageTopicMsg
                            |> dispatch) ]) ]

let topicSummaryActionMenu (model: TopicsSummaryViewModel) dispatch =
    Mui.grid [ grid.item true
               grid.alignContent.flexEnd
               grid.xs._12
               grid.children [ Mui.buttonGroup [ buttonGroup.variant.text
                                                 buttonGroup.orientation.horizontal
                                                 buttonGroup.fullWidth true
                                                 prop.children [ Mui.button [ prop.text "Create new topic"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      ShowCreateTopicDialog
                                                                                      |> CreateTopicMsg
                                                                                      |> dispatch) ] ] ] ] ]

let viewFromGenericPage (model: TopicsSummaryViewModel) (dispatch: TopicMsg -> unit) =
    genericPageLayout
        "Topics"
        "A Kakfa is a classification for incoming events. Consumers listen for events from a topic."
        [ Client.Pages.Topics.CreateTopicDialog.CreateTopicDialog model dispatch ]
        (topicSummaryActionMenu model dispatch)
        (summaryCards model dispatch)
        [ Mui.grid [ grid.item true
                     grid.alignContent.flexEnd
                     grid.xs._12
                     grid.children [ (topicTable model dispatch) ] ] ]


let topicsSummaryView =
    FunctionComponent.Of(
        (fun (model: TopicsSummaryViewModel, dispatch: TopicMsg -> unit) -> viewFromGenericPage model dispatch),
        "TopicsSummaryPage",
        memoEqualsButFunctions
    )



let update (api: ITopicsApi) (msg: TopicMsg) (m: Model) : Model * Cmd<Msg> =
    if m.Page <> Topics then
        m, Cmd.none
    else
        match msg with
        | LoadTopicSummaryViewModel ->
            m,
            Cmd.OfAsync.perform
                (Client.Pages.Topics.Data.fetchSummary api)
                ()
                (fun x -> x |> LoadedTopicSummaryViewModel |> TopicMsg)
        | LoadedTopicSummaryViewModel vm ->
            { m with
                  PageModel = vm |> TopicSummaryViewModel },
            Cmd.none
        | CreateTopicMsg msg -> Client.Pages.Topics.CreateTopicDialog.update api msg m
        | ManageTopicMsg msg -> Client.Pages.Topics.ManageTopic.update api msg m
        | _ -> m, Cmd.none

let view (model: PageModel) (dispatch: Msg -> unit) =
    let topicDispatch = fun x -> (x |> TopicMsg |> dispatch)

    let manageTopicDispatch =
        fun x -> (x |> ManageTopicMsg |> TopicMsg |> dispatch)

    match model with
    | TopicSummaryViewModel b -> topicsSummaryView (b, topicDispatch)
    | ManageTopicViewModel b -> Client.Pages.Topics.ManageTopic.manageTopicView (b, manageTopicDispatch)
    | _ -> Mui.typography "Loading"
