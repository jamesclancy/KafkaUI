module Client.Pages.Topics.ManageTopic

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
open Shared.Dtos


let summaryCards (model: ManageTopicViewModel) dispatch =
    Html.div [ Mui.grid [ grid.item true
                          grid.alignContent.flexEnd
                          grid.xs._12
                          grid.children [ Mui.typography [ typography.component' "h4"
                                                           typography.variant.h6
                                                           typography.children "Summary" ]
                                          Mui.typography [ typography.paragraph true
                                                           typography.children "" ] ] ]
               Mui.grid [ grid.container true
                          grid.spacing._5
                          grid.justify.flexStart
                          grid.alignItems.flexStart
                          grid.alignContent.center
                          grid.children [ summaryCard grid.xs._4 "Events" (sprintf "%i" model.EventCount)
                                          summaryCard grid.xs._4 "Partitions" (sprintf "%i" model.PartitionCount)
                                          summaryCard
                                              grid.xs._4
                                              "Partitions Without Leader"
                                              (sprintf "%i" model.PartitionsWithoutLeaderCount)
                                          summaryCard
                                              grid.xs._4
                                              "Replication Factor"
                                              (sprintf "%i" model.ReplicationFactor)
                                          summaryCard
                                              grid.xs._4
                                              "In Sync Replica Issue Count"
                                              (sprintf "%i" model.InSyncReplicaIssueCount)
                                          summaryCard grid.xs._4 "Is Compacted" (sprintf "%b" model.IsCompacted) ] ] ]

let customerGroupsTable (model: ManageTopicViewModel) dispatch =
    genericFormatedTable
        "Customer Groups"
        true
        [ columns.column [ column.title "GroupId"
                           column.field<TopicConsumerGroupSummary> (fun rd -> nameof rd.GroupId) ]
          columns.column [ column.title "State"
                           column.field<TopicConsumerGroupSummary> (fun rd -> nameof rd.State) ]
          columns.column [ column.title "Lag"
                           column.field<TopicConsumerGroupSummary> (fun rd -> nameof rd.Lag)
                           column.type'.numeric ] ]
        model.ConsumerGroups
        []

let partitionsTable (model: ManageTopicViewModel) dispatch =
    genericFormatedTable
        "Partitions"
        true
        [ columns.column [ column.title "Partition"
                           column.field<TopicPartitionSummary> (fun rd -> nameof rd.Partition) ]
          columns.column [ column.title "Leader"
                           column.field<TopicPartitionSummary> (fun rd -> nameof rd.Leader)
                           column.type'.numeric ]
          columns.column [ column.title "Offset Min"
                           column.field<TopicPartitionSummary> (fun rd -> nameof rd.OffsetMin)
                           column.type'.numeric ]
          columns.column [ column.title "Offset Max"
                           column.field<TopicPartitionSummary> (fun rd -> nameof rd.OffsetMax)
                           column.type'.numeric ]
          columns.column [ column.title "Size In Bytes"
                           column.field<TopicPartitionSummary> (fun rd -> nameof rd.SizeInBytes)
                           column.type'.numeric ]
          columns.column [ column.title "In Sync Replicas"
                           column.field<TopicPartitionSummary> (fun rd -> nameof rd.InSyncReplicas)
                           column.type'.numeric ]
          columns.column [ column.title "Replicas"
                           column.field<TopicPartitionSummary> (fun rd -> nameof rd.Replicas)
                           column.type'.numeric ] ]
        model.Partitions
        []

let brokersTable (model: ManageTopicViewModel) dispatch =
    genericFormatedTable
        "Brokers"
        true
        [ columns.column [ column.title "BrokerId"
                           column.field<TopicBrokerSummary> (fun rd -> nameof rd.BrokerId) ]
          columns.column [ column.title "Partitions As Leader"
                           column.field<TopicBrokerSummary> (fun rd -> nameof rd.PartitionsAsLeader)
                           column.type'.numeric ]
          columns.column [ column.title "Partitions"
                           column.field<TopicBrokerSummary> (fun rd -> nameof rd.Partitions)
                           column.type'.numeric ] ]
        model.Brokers
        []

let configurationTable (model: ManageTopicViewModel) dispatch =
    genericFormatedTable
        "Configuration"
        true
        [ columns.column [ column.title "Name"
                           column.field<TopicConfigurationProperty> (fun rd -> nameof rd.Name) ]
          columns.column [ column.title "Value"
                           column.field<TopicConfigurationProperty> (fun rd -> nameof rd.Value) ] ]
        model.Configuration
        [ actions.action<TopicConfigurationProperty>
              (fun rowData ->
                  [ if rowData.Locked then
                        action.icon (Mui.icon [ lockIcon [] ])
                        action.disabled true

                        action.tooltip (sprintf "%s is locked unable to edit." rowData.Name)
                    else
                        action.icon (Mui.icon [ settingsIcon [] ])
                        action.tooltip (sprintf "Edit %s" rowData.Name)

                        action.onClick
                            (fun _ _ ->
                                rowData
                                |> ShowEditTopicConfigurationPropertyDialog
                                |> EditTopicConfigurationPropertyMsg
                                |> dispatch) ]) ]

let manageTopicTabs (model: ManageTopicViewModel) (dispatch: ManageTopicMsg -> unit) =
    let appropiateTable =
        match model.ManageTopicTabSelection with
        | CustomerGroupsTab -> customerGroupsTable model dispatch
        | PartitionsTab -> partitionsTable model dispatch
        | BrokersTab -> brokersTable model dispatch
        | ConfigurationTab -> configurationTable model dispatch

    let styleForActiveTabOrNot boolValue =
        if boolValue then
            [ button.variant.contained ]
        else
            []

    [ Mui.grid [ grid.item true
                 grid.alignContent.flexEnd
                 grid.xs._12
                 grid.children [ Mui.typography [ typography.component' "h4"
                                                  typography.variant.h6
                                                  typography.children "Details" ] ] ]
      Mui.grid [ grid.item true
                 grid.alignContent.flexEnd
                 grid.xs._12
                 grid.children [ Mui.buttonGroup [ buttonGroup.variant.text
                                                   buttonGroup.orientation.horizontal
                                                   buttonGroup.fullWidth true
                                                   prop.children [ Mui.button [ prop.text "Customer Group"
                                                                                yield!
                                                                                    styleForActiveTabOrNot (
                                                                                        model.ManageTopicTabSelection =
                                                                                            ManageTopicTabSelection.CustomerGroupsTab
                                                                                    )
                                                                                prop.onClick
                                                                                    (fun _ ->
                                                                                        CustomerGroupsTab
                                                                                        |> ChangeManageTopicTabSelection
                                                                                        |> dispatch) ]
                                                                   Mui.button [ prop.text "Partitions"
                                                                                yield!
                                                                                    styleForActiveTabOrNot (
                                                                                        model.ManageTopicTabSelection =
                                                                                            ManageTopicTabSelection.PartitionsTab
                                                                                    )
                                                                                prop.onClick
                                                                                    (fun _ ->
                                                                                        PartitionsTab
                                                                                        |> ChangeManageTopicTabSelection
                                                                                        |> dispatch) ]
                                                                   Mui.button [ prop.text "Brokers"
                                                                                yield!
                                                                                    styleForActiveTabOrNot (
                                                                                        model.ManageTopicTabSelection =
                                                                                            ManageTopicTabSelection.BrokersTab
                                                                                    )
                                                                                prop.onClick
                                                                                    (fun _ ->
                                                                                        BrokersTab
                                                                                        |> ChangeManageTopicTabSelection
                                                                                        |> dispatch) ]
                                                                   Mui.button [ prop.text "Configuration"
                                                                                yield!
                                                                                    styleForActiveTabOrNot (
                                                                                        model.ManageTopicTabSelection =
                                                                                            ManageTopicTabSelection.ConfigurationTab
                                                                                    )
                                                                                prop.onClick
                                                                                    (fun _ ->
                                                                                        ConfigurationTab
                                                                                        |> ChangeManageTopicTabSelection
                                                                                        |> dispatch) ] ] ] ] ]

      Mui.grid [ grid.item true
                 grid.alignContent.flexEnd
                 grid.xs._12
                 grid.children [ appropiateTable ] ] ]

let manageTopicActionMenu (model: ManageTopicViewModel) dispatch =
    Mui.grid [ grid.item true
               grid.alignContent.flexEnd
               grid.xs._12
               grid.children [ Mui.buttonGroup [ buttonGroup.variant.text
                                                 buttonGroup.orientation.horizontal
                                                 buttonGroup.fullWidth true
                                                 prop.children [ Mui.button [ prop.text "Add Partitions"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      { TopicName = model.TopicName
                                                                                        Value = model.PartitionCount
                                                                                        NewValue = model.PartitionCount
                                                                                        ErrorMessage = None }
                                                                                      |> ShowAddPartitionDialog
                                                                                      |> AddPartitionDialogMsg
                                                                                      |> dispatch) ]
                                                                 Mui.button [ prop.text "Purge Topic"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      ShowPurgeTopicConfirmationDialog
                                                                                      |> PurgeTopicConfirmationDialogMsg
                                                                                      |> dispatch) ] ] ] ] ]



let viewFromGenericPage (model: ManageTopicViewModel) (dispatch: ManageTopicMsg -> unit) =
    genericPageLayout
        (sprintf "Manage Topic %s" model.TopicName)
        ""
        [ Client.Pages.Topics.EditTopicConfigurationPropertyDialog.EditTopicConfigurationPropertyDialog model dispatch
          Client.Pages.Topics.AddPartitionDialog.addPartitionDialogView model dispatch
          Client.Pages.Topics.PurgeTopicConfirmationDialog.purgeTopicConfirmationDialogView model dispatch ]
        (manageTopicActionMenu model dispatch)
        (summaryCards model dispatch)
        (manageTopicTabs model dispatch)



let manageTopicView =
    FunctionComponent.Of(
        (fun (model: ManageTopicViewModel, dispatch: ManageTopicMsg -> unit) -> viewFromGenericPage model dispatch),
        "ManageTopicPage",
        memoEqualsButFunctions
    )


let view (model: PageModel) (dispatch: Msg -> unit) =
    let topicDispatch =
        fun x -> (x |> ManageTopicMsg |> TopicMsg |> dispatch)

    match model with
    | ManageTopicViewModel b -> manageTopicView (b, topicDispatch)
    | _ -> Mui.typography "Loading"

let update (api: ITopicsApi) (msg: ManageTopicMsg) (m: Model) : Model * Cmd<Msg> =

    match msg with
    | LoadManageTopicViewModel topicName ->
        m,
        Cmd.OfAsync.perform
            (Client.Pages.Topics.Data.fetchDetailsForTopic api)
            topicName
            (fun x ->
                x
                |> LoadedManageTopicViewModel
                |> ManageTopicMsg
                |> TopicMsg)
    | LoadedManageTopicViewModel vm ->
        { m with
              PageModel = vm |> ManageTopicViewModel },
        Cmd.none
    | _ ->
        match m.PageModel with
        | ManageTopicViewModel b ->
            match msg with
            | ChangeManageTopicTabSelection tab ->
                { m with
                      PageModel =
                          { b with ManageTopicTabSelection = tab }
                          |> ManageTopicViewModel },
                Cmd.none
            | AddPartitionDialogMsg dm -> Client.Pages.Topics.AddPartitionDialog.update api dm m
            | PurgeTopicConfirmationDialogMsg msg -> Client.Pages.Topics.PurgeTopicConfirmationDialog.update api msg m
            | EditTopicConfigurationPropertyMsg msg ->
                Client.Pages.Topics.EditTopicConfigurationPropertyDialog.update api msg m
            | _ -> m, Cmd.none
        | _ -> m, Cmd.none
    | _ -> m, Cmd.none
