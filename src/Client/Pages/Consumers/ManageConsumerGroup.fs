module Client.Pages.Consumers.ManageConsumerGroup

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


let summaryCards (model: ManageConsumerGroupViewModel) dispatch =
    Html.div [ Mui.grid [ grid.container true
                          grid.spacing._3
                          grid.justify.flexStart
                          grid.alignItems.flexStart
                          grid.alignContent.center
                          grid.children [ summaryCard grid.xs._4 "State" model.State
                                          summaryCard grid.xs._4 "Lag" (sprintf "%i" model.Lag)
                                          summaryCard grid.xs._4 "Member Count" (sprintf "%i" model.MemberCount)
                                          summaryCard
                                              grid.xs._4
                                              "Assigned Partitions"
                                              (sprintf "%i" model.AssignedPartitions)
                                          summaryCard grid.xs._4 "Assigned Topics" (sprintf "%i" model.AssignedTopics)
                                          summaryCard
                                              grid.xs._4
                                              "Partition Assignment Strategy"
                                              model.PartitionAssignmentStrategy ] ] ]

let consumerGroupTable model dispatch =
    genericFormatedTable
        ""
        false
        [ columns.column [ column.title "MemberType"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.MemberType) ]
          columns.column [ column.title "MemberId"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.MemberId) ]
          columns.column [ column.title "ClientId"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.ClientId) ]
          columns.column [ column.title "HostName"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.HostName) ]
          columns.column [ column.title "Lag"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.Lag)
                           column.type'.numeric ]
          columns.column [ column.title "AssignedPartitions"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.AssignedPartitions)
                           column.type'.numeric ]
          columns.column [ column.title "Assigned Partitions"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.AssignedPartitions)
                           column.type'.numeric ]
          columns.column [ column.title "Assigned Topic"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.AssignedTopic) ]
          columns.column [ column.title "Cursor Location"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.CursorLocation)
                           column.type'.numeric ]
          columns.column [ column.title "Cusor End"
                           column.field<ManageConsumerGroupMemberViewModel> (fun rd -> nameof rd.CusorEnd)
                           column.type'.numeric ] ]
        model.Members
        []

let manageConsumerGroupActionMenu model dispatch =
    Mui.grid [ grid.item true
               grid.alignContent.flexEnd
               grid.xs._12
               grid.children [ Mui.buttonGroup [ buttonGroup.variant.text
                                                 buttonGroup.orientation.horizontal
                                                 buttonGroup.fullWidth true
                                                 prop.children [ Mui.button [ prop.text "Alter Offset"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      model.ConsumerGroupName
                                                                                      |> ShowRebalanceConsumerGroupConfirmationDialog
                                                                                      |> RebalanceConsumerGroupConfirmationDialogMsg
                                                                                      |> dispatch) ]
                                                                 Mui.button [ prop.text "Rebalance"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      model.ConsumerGroupName
                                                                                      |> ShowRebalanceConsumerGroupConfirmationDialog
                                                                                      |> RebalanceConsumerGroupConfirmationDialogMsg
                                                                                      |> dispatch) ]
                                                                 Mui.button [ prop.text "Delete"
                                                                              prop.onClick
                                                                                  (fun _ ->
                                                                                      model.ConsumerGroupName
                                                                                      |> ShowDeleteConsumerGroupConfirmationDialog
                                                                                      |> DeleteConsumerGroupConfirmationDialogMsg
                                                                                      |> dispatch) ] ] ] ] ]


let manageConsumerGroupView =
    FunctionComponent.Of(
        (fun (model: ManageConsumerGroupViewModel, dispatch: ManageConsumerGroupMsg -> unit) ->
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

                                                                  DeleteConsumerGroupConfirmationDialog.DeleteConsumerGroupConfirmationDialogView
                                                                      model
                                                                      dispatch
                                                                  Mui.grid [ grid.item true
                                                                             grid.alignContent.flexEnd
                                                                             grid.xs._12
                                                                             grid.children [ Mui.divider [] ] ]
                                                                  manageConsumerGroupActionMenu model dispatch
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



let update (api: IConsumersApi) (msg: ManageConsumerGroupMsg) (m: Model) : Model * Cmd<Msg> =
    if m.Page <> Consumers then
        m, Cmd.none
    else
        match msg with
        | LoadManageConsumerGroupViewModel name ->
            m,
            Cmd.OfAsync.perform
                (Client.Pages.Consumers.Data.fetchManageConsumerGroupDetails api)
                name
                (fun x ->
                    x
                    |> ManageConsumerGroupViewModel.FromConsumerGroupDetail
                    |> LoadedManageConsumerGroupViewModel
                    |> ManageConsumerGroupMsg
                    |> ConsumerMsg)
        | LoadedManageConsumerGroupViewModel vm ->
            { m with
                  PageModel = vm |> ManageConsumerGroupViewModel },
            Cmd.none
        | DeleteConsumerGroupConfirmationDialogMsg del -> DeleteConsumerGroupConfirmationDialog.update api del m
        | _ -> m, Cmd.none

let view (model: PageModel) (dispatch: Msg -> unit) =
    let topicDispatch =
        fun x ->
            (x
             |> ManageConsumerGroupMsg
             |> ConsumerMsg
             |> dispatch)

    match model with
    | ManageConsumerGroupViewModel b -> manageConsumerGroupView (b, topicDispatch)
    | _ -> Mui.typography "Loading"
