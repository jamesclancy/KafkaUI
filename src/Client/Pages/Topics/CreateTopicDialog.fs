module Client.Pages.Topics.CreateTopicDialog

open Client.Pages.Topics.Models
open Client.Models
open Feliz.MaterialUI
open Feliz
open Client.Pages.CommonLayoutItems
open Elmish
open Browser.Types
open Shared.Contracts


let update (api: ITopicsApi) (msg: CreateTopicMsg) (m: Model) : Model * Cmd<Msg> =
    match m.PageModel with
    | TopicSummaryViewModel b ->
        let currentCreateDialog =
            b.CreateDialog
            |> Option.fold (fun x y -> y) CreateTopicViewModel.Default

        let badIntegerErrorHandler =
            (fun badValue ->
                { m with
                      PageModel =
                          { b with
                                CreateDialog =
                                    Some
                                        { currentCreateDialog with
                                              ErrorMessage = Some(sprintf "Unable to parse %s as an integer" badValue) } }
                          |> TopicSummaryViewModel },
                Cmd.none)

        match msg with
        | ShowCreateTopicDialog ->
            { m with
                  PageModel =
                      { b with
                            CreateDialog = Some currentCreateDialog }
                      |> TopicSummaryViewModel },
            Cmd.none
        | CloseCreateTopicDialog ->
            { m with
                  PageModel =
                      { b with CreateDialog = None }
                      |> TopicSummaryViewModel },
            Cmd.none
        | TopicNameEdited x ->
            { m with
                  PageModel =
                      { b with
                            CreateDialog =
                                Some
                                    { currentCreateDialog with
                                          TopicName = x } }
                      |> TopicSummaryViewModel },
            Cmd.none
        | CreateTopicFailed x ->
            { m with
                  PageModel =
                      { b with
                            CreateDialog =
                                Some
                                    { currentCreateDialog with
                                          ErrorMessage = Some x } }
                      |> TopicSummaryViewModel },
            Cmd.none
        | CleanupPolicyUseRetentionPolicyToggled ->
            { m with
                  PageModel =
                      { b with
                            CreateDialog =
                                Some
                                    { currentCreateDialog with
                                          CleanupPolicyUseRetentionPolicy =
                                              not currentCreateDialog.CleanupPolicyUseRetentionPolicy } }
                      |> TopicSummaryViewModel },
            Cmd.none
        | CleanUpPolicyUseCompactionToggled ->
            { m with
                  PageModel =
                      { b with
                            CreateDialog =
                                Some
                                    { currentCreateDialog with
                                          CleanUpPolicyUseCompaction =
                                              not currentCreateDialog.CleanUpPolicyUseCompaction } }
                      |> TopicSummaryViewModel },
            Cmd.none
        | PartitionsEdited s ->
            tryParseInteger
                s
                (fun parsedValue ->
                    { m with
                          PageModel =
                              { b with
                                    CreateDialog =
                                        Some
                                            { currentCreateDialog with
                                                  Partitions = parsedValue } }
                              |> TopicSummaryViewModel },
                    Cmd.none)
                badIntegerErrorHandler
        | ReplicationFactorEdited s ->
            tryParseInteger
                s
                (fun parsedValue ->
                    { m with
                          PageModel =
                              { b with
                                    CreateDialog =
                                        Some
                                            { currentCreateDialog with
                                                  ReplicationFactor = parsedValue } }
                              |> TopicSummaryViewModel },
                    Cmd.none)
                badIntegerErrorHandler
        | CreateTopic p ->
            m,
            Cmd.OfAsync.either
                (Client.Pages.Topics.Data.tryToCreateTopic api)
                p
                (fun r -> r)
                (fun x ->
                    x.ToString()
                    |> CreateTopicFailed
                    |> CreateTopicMsg
                    |> TopicMsg)
    | _ -> m, Cmd.none

let CreateTopicDialog (model: TopicsSummaryViewModel) (dispatch: TopicMsg -> unit) =

    match model.CreateDialog with
    | Some edit ->
        let errorMessage =
            match edit.ErrorMessage with
            | Some x ->
                Mui.alert [ alert.severity.error
                            prop.children [ Mui.alertTitle [ prop.text x ] ] ]
            | None -> Mui.typography ""

        Mui.dialog [ dialog.open' true
                     dialog.onClose
                         (fun _ ->
                             CloseCreateTopicDialog
                             |> CreateTopicMsg
                             |> dispatch)
                     dialog.children [ Mui.dialogTitle "Create a new Topic"
                                       Mui.dialogContent [ Mui.dialogContentText
                                                               "Consumers can then listen to events send on these topics"
                                                           Mui.dialogContent [ errorMessage ]
                                                           Mui.textField [ textField.autoFocus true
                                                                           textField.margin.dense
                                                                           textField.label "Name"
                                                                           textField.value edit.TopicName
                                                                           textField.fullWidth true
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> TopicNameEdited
                                                                                   |> CreateTopicMsg
                                                                                   |> dispatch) ]
                                                           Mui.textField [ textField.autoFocus true
                                                                           textField.margin.dense
                                                                           textField.label "Partitions"
                                                                           textField.value edit.Partitions
                                                                           textField.fullWidth true
                                                                           textField.type' "number"
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> PartitionsEdited
                                                                                   |> CreateTopicMsg
                                                                                   |> dispatch) ]
                                                           Mui.textField [ textField.autoFocus true
                                                                           textField.margin.dense
                                                                           textField.label "Replication Factor"
                                                                           textField.value edit.ReplicationFactor
                                                                           textField.fullWidth true
                                                                           textField.type' "number"
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> ReplicationFactorEdited
                                                                                   |> CreateTopicMsg
                                                                                   |> dispatch) ]
                                                           Mui.formControlLabel [ formControlLabel.label
                                                                                      "Cleanup Policy Use Retention Policy"
                                                                                  formControlLabel.control (
                                                                                      Mui.checkbox [ checkbox.checked'
                                                                                                         edit.CleanupPolicyUseRetentionPolicy
                                                                                                     checkbox.onChange
                                                                                                         (fun (_: Event) ->
                                                                                                             CleanupPolicyUseRetentionPolicyToggled
                                                                                                             |> CreateTopicMsg
                                                                                                             |> dispatch) ]
                                                                                  ) ]
                                                           Mui.formControlLabel [ formControlLabel.label
                                                                                      "Clean Up Policy Use Compaction"
                                                                                  formControlLabel.control (
                                                                                      Mui.checkbox [ checkbox.checked'
                                                                                                         edit.CleanUpPolicyUseCompaction
                                                                                                     checkbox.onChange
                                                                                                         (fun (_: Event) ->
                                                                                                             CleanUpPolicyUseCompactionToggled
                                                                                                             |> CreateTopicMsg
                                                                                                             |> dispatch) ]
                                                                                  ) ] ]
                                       Mui.dialogActions [ Mui.button [ button.color.primary
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                CloseCreateTopicDialog
                                                                                |> CreateTopicMsg
                                                                                |> dispatch)
                                                                        button.children "Cancel" ]
                                                           Mui.button [ button.color.primary
                                                                        button.children "Start"
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                edit
                                                                                |> CreateTopic
                                                                                |> CreateTopicMsg
                                                                                |> dispatch) ] ] ] ]
    | _ -> Mui.typography ""
