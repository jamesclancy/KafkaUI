module Client.Pages.Topics.EditTopicConfigurationPropertyDialog

open Client.Pages.Topics.Models
open Client.Models
open Feliz.MaterialUI
open Feliz
open Client.Pages.CommonLayoutItems
open Elmish
open Browser.Types
open Shared.Contracts


let update (api: ITopicsApi) (msg: EditTopicConfigurationPropertyMsg) (m: Model) : Model * Cmd<Msg> =

    let addErrorIfExists (optionalValue: ManageTopicConfigurationPropertyEditViewModel option) (errorMsg: string) =
        match optionalValue with
        | None -> None
        | Some s -> Some { s with ErrorMessage = Some errorMsg }

    let addValueIfExists (optionalValue: ManageTopicConfigurationPropertyEditViewModel option) (newValue: string) =
        match optionalValue with
        | None -> None
        | Some s -> Some { s with NewValue = newValue }

    match m.PageModel with
    | ManageTopicViewModel vm ->
        match msg with
        | ShowEditTopicConfigurationPropertyDialog v ->
            { m with
                  PageModel =
                      { vm with
                            EditTopicConfigurationPropertyDialog =
                                Some(ManageTopicConfigurationPropertyEditViewModel.FromTopicConfigurationProperty v) }
                      |> ManageTopicViewModel },
            Cmd.none
        | CloseEditTopicConfigurationPropertyDialog ->
            { m with
                  PageModel =
                      { vm with
                            EditTopicConfigurationPropertyDialog = None }
                      |> ManageTopicViewModel },
            Cmd.none
        | NewValueEdited v ->
            { m with
                  PageModel =
                      { vm with
                            EditTopicConfigurationPropertyDialog =
                                addValueIfExists vm.EditTopicConfigurationPropertyDialog v }
                      |> ManageTopicViewModel },
            Cmd.none
        | EditTopicConfigurationPropertyFailed em ->
            { m with
                  PageModel =
                      { vm with
                            EditTopicConfigurationPropertyDialog =
                                addErrorIfExists vm.EditTopicConfigurationPropertyDialog em }
                      |> ManageTopicViewModel },
            Cmd.none
        | SaveEditTopicConfigurationPropertyDialog edit ->
            m,
            Cmd.OfAsync.either
                (Client.Pages.Topics.Data.tryToUpdateConfiguration api vm.TopicName)
                edit.AsTopicConfigurationProperty
                (fun r -> r)
                (fun x ->
                    x.ToString()
                    |> EditTopicConfigurationPropertyFailed
                    |> EditTopicConfigurationPropertyMsg
                    |> ManageTopicMsg
                    |> TopicMsg)
    | _ -> m, Cmd.none

let EditTopicConfigurationPropertyDialog (model: ManageTopicViewModel) (dispatch: ManageTopicMsg -> unit) =

    match model.EditTopicConfigurationPropertyDialog with
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
                             CloseEditTopicConfigurationPropertyDialog
                             |> EditTopicConfigurationPropertyMsg
                             |> dispatch)
                     dialog.children [ Mui.dialogTitle "Edit Topic Configuration Property"
                                       Mui.dialogContent [ Mui.dialogContentText
                                                               "Consumers can then listen to events send on these topics"
                                                           Mui.dialogContent [ errorMessage ]
                                                           Mui.textField [ textField.autoFocus false
                                                                           textField.margin.dense
                                                                           textField.label "Kafka Default Value"
                                                                           textField.fullWidth true
                                                                           textField.value edit.KafkaDefaultValue
                                                                           textField.disabled true ]
                                                           Mui.textField [ textField.autoFocus false
                                                                           textField.margin.dense
                                                                           textField.label "Previous Value"
                                                                           textField.fullWidth true
                                                                           textField.disabled true
                                                                           textField.value edit.Value ]
                                                           Mui.textField [ textField.autoFocus true
                                                                           textField.margin.dense
                                                                           textField.label "New Value"
                                                                           textField.fullWidth true
                                                                           textField.value edit.NewValue
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> NewValueEdited
                                                                                   |> EditTopicConfigurationPropertyMsg
                                                                                   |> dispatch) ] ]
                                       Mui.dialogActions [ Mui.button [ button.color.primary
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                CloseEditTopicConfigurationPropertyDialog
                                                                                |> EditTopicConfigurationPropertyMsg
                                                                                |> dispatch)
                                                                        button.children "Cancel" ]
                                                           Mui.button [ button.color.primary
                                                                        button.children "Start"
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                edit
                                                                                |> SaveEditTopicConfigurationPropertyDialog
                                                                                |> EditTopicConfigurationPropertyMsg
                                                                                |> dispatch) ] ] ] ]
    | _ -> Mui.typography ""
