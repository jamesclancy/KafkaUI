module Client.Pages.Brokers.BrokerEditConfiguration

open Client.Models
open Client.Pages.Brokers.Models
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Feliz.MaterialUI.MaterialTable
open Elmish
open Browser.Types
open Client.Pages.Brokers.Data
open Shared.Contracts


let EditDialog (model: BrokerConfigurationDetailViewModel) (dispatch: BrokerMsg -> unit) =
    match model.ConfigurationSettingToEdit with
    | Some edit ->
        let description =
            match edit.Description with
            | Some x -> x
            | None -> "Description Unavailable"

        let errorMessage =
            match edit.ErrorMessage with
            | Some x ->
                Mui.alert [ alert.severity.error
                            prop.children [ Mui.alertTitle [ prop.text x ] ] ]
            | None -> Mui.typography ""

        Mui.dialog [ dialog.open' true
                     dialog.onClose
                         (fun _ ->
                             CancelEditOfConfigurationProperty
                             |> BrokerEditMsg
                             |> dispatch)
                     dialog.children [ Mui.dialogTitle (sprintf "Editing: %s" edit.Name)
                                       Mui.dialogContent [ Mui.dialogContentText description
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
                                                                                   |> EditOfConfigurationPropertyValueChanged
                                                                                   |> BrokerEditMsg
                                                                                   |> dispatch) ]
                                                           Mui.formControlLabel [ formControlLabel.label
                                                                                      "Apply To All Brokers In Cluster"
                                                                                  formControlLabel.control (
                                                                                      Mui.checkbox [ checkbox.checked'
                                                                                                         edit.ApplyToAllBrokersInCluster
                                                                                                     checkbox.onChange
                                                                                                         (fun (_: Event) ->
                                                                                                             EditOfConfigurationPropertyToggleApplyToAllBrokersInCluster
                                                                                                             |> BrokerEditMsg
                                                                                                             |> dispatch) ]
                                                                                  ) ] ]
                                       Mui.dialogActions [ Mui.button [ prop.onClick
                                                                            (fun _ ->
                                                                                CancelEditOfConfigurationProperty
                                                                                |> BrokerEditMsg
                                                                                |> dispatch)
                                                                        button.color.primary
                                                                        button.children "Cancel" ]
                                                           Mui.button [ prop.onClick
                                                                            (fun _ ->
                                                                                edit
                                                                                |> SaveEditOfConfigurationProperty
                                                                                |> BrokerEditMsg
                                                                                |> dispatch)
                                                                        button.color.primary
                                                                        button.children "Submit" ] ] ] ]
    | _ -> Mui.typography ""


let startEditorDialog (api: IBrokersApi) b pg m =
    match pg with
    | StartEditOfConfigurationProperty p ->
        let newModel =
            { b with
                  ConfigurationSettingToEdit =
                      Some(BrokerConfigurationEditPropertyViewModel.FromBrokerConfigurationProperty b.BrokerId p) }
            |> BrokerConfigurationDetailViewModel

        { m with PageModel = newModel }, Cmd.none
    | CancelEditOfConfigurationProperty ->
        let newModel =
            { b with
                  ConfigurationSettingToEdit = None }
            |> BrokerConfigurationDetailViewModel

        { m with PageModel = newModel }, Cmd.none
    | EditOfConfigurationPropertyValueChanged p ->
        match b.ConfigurationSettingToEdit with
        | None -> m, Cmd.none
        | Some s ->
            let newModel =
                { b with
                      ConfigurationSettingToEdit = Some { s with NewValue = p } }
                |> BrokerConfigurationDetailViewModel

            { m with PageModel = newModel }, Cmd.none
    | EditOfConfigurationPropertyToggleApplyToAllBrokersInCluster ->
        match b.ConfigurationSettingToEdit with
        | None -> m, Cmd.none
        | Some s ->
            let newModel =
                { b with
                      ConfigurationSettingToEdit =
                          Some
                              { s with
                                    ApplyToAllBrokersInCluster = not s.ApplyToAllBrokersInCluster } }
                |> BrokerConfigurationDetailViewModel

            { m with PageModel = newModel }, Cmd.none
    | SaveEditOfConfigurationPropertyFailed error ->
        match b.ConfigurationSettingToEdit with
        | None -> m, Cmd.none
        | Some s ->
            let newModel =
                { b with
                      ConfigurationSettingToEdit =
                          Some
                              { s with
                                    ErrorMessage = Some(error.ToString()) } }
                |> BrokerConfigurationDetailViewModel

            { m with PageModel = newModel }, Cmd.none
    | SaveEditOfConfigurationProperty p ->
        m,
        Cmd.OfAsync.either
            (saveConfigurationPropertyForBrokerAndReturnRefresh api)
            p
            (fun r ->
                { BrokerId = r.BrokerId }
                |> BrokerLoadConfiguration
                |> BrokerMsg)
            (fun x ->
                x
                |> SaveEditOfConfigurationPropertyFailed
                |> BrokerEditMsg
                |> BrokerMsg)
