module Client.Pages.Brokers.BrokerRollingRestartConfigurationDialog

open Client.Pages.Brokers.Models
open Client.Models
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Feliz.MaterialUI.MaterialTable
open Elmish
open Browser.Types
open Client.Pages.Brokers.Data
open Client.Pages.CommonLayoutItems
open Shared.Contracts

let update (api: IBrokersApi) (msg: BrokerRollingRestartMsg) (m: Model) : Model * Cmd<Msg> =
    match m.PageModel with
    | BrokerSummaryViewModel b ->
        let currentRestartConfig =
            b.BrokerRollingRestartConfigurationDialog
            |> Option.fold (fun x y -> y) BrokerRollingRestartConfiguration.Default

        let badIntegerErrorHandler =
            (fun badValue ->
                { m with
                      PageModel =
                          { b with
                                BrokerRollingRestartConfigurationDialog =
                                    Some
                                        { currentRestartConfig with
                                              ErrorMessage = Some(sprintf "Unable to parse %s as an integer" badValue) } }
                          |> BrokerSummaryViewModel },
                Cmd.none)

        match msg with
        | ShowBrokerRollingRestartDialog ->
            { m with
                  PageModel =
                      { b with
                            BrokerRollingRestartConfigurationDialog = Some currentRestartConfig }
                      |> BrokerSummaryViewModel },
            Cmd.none
        | CloseBrokerRollingRestartDialog ->
            { m with
                  PageModel =
                      { b with
                            BrokerRollingRestartConfigurationDialog = None }
                      |> BrokerSummaryViewModel },
            Cmd.none
        | UseSudoToggled ->
            { m with
                  PageModel =
                      { b with
                            BrokerRollingRestartConfigurationDialog =
                                Some
                                    { currentRestartConfig with
                                          UseSudo = not currentRestartConfig.UseSudo } }
                      |> BrokerSummaryViewModel },
            Cmd.none
        | StopCommandEdited s ->
            { m with
                  PageModel =
                      { b with
                            BrokerRollingRestartConfigurationDialog =
                                Some
                                    { currentRestartConfig with
                                          StopCommand = s } }
                      |> BrokerSummaryViewModel },
            Cmd.none
        | StartCommandEdited s ->
            { m with
                  PageModel =
                      { b with
                            BrokerRollingRestartConfigurationDialog =
                                Some
                                    { currentRestartConfig with
                                          StartCommand = s } }
                      |> BrokerSummaryViewModel },
            Cmd.none
        | StartRollingRestartFailed s ->
            { m with
                  PageModel =
                      { b with
                            BrokerRollingRestartConfigurationDialog =
                                Some
                                    { currentRestartConfig with
                                          ErrorMessage = Some s } }
                      |> BrokerSummaryViewModel },
            Cmd.none
        | HealthCheckIntervalInSecondsEdited s ->
            tryParseInteger
                s
                (fun parsedValue ->
                    { m with
                          PageModel =
                              { b with
                                    BrokerRollingRestartConfigurationDialog =
                                        Some
                                            { currentRestartConfig with
                                                  HealthCheckIntervalInSeconds = parsedValue } }
                              |> BrokerSummaryViewModel },
                    Cmd.none)
                badIntegerErrorHandler
        | StableIterationsCountEdited s ->
            tryParseInteger
                s
                (fun parsedValue ->
                    { m with
                          PageModel =
                              { b with
                                    BrokerRollingRestartConfigurationDialog =
                                        Some
                                            { currentRestartConfig with
                                                  StableIterationsCount = parsedValue } }
                              |> BrokerSummaryViewModel },
                    Cmd.none)
                badIntegerErrorHandler
        | StartRollingRestart p ->
            m,
            Cmd.OfAsync.either
                (startRollingRestartforCluster api)
                p.ToBrokerRollingRestartRequest
                (fun r -> r)
                (fun x ->
                    x.ToString()
                    |> StartRollingRestartFailed
                    |> BrokerRollingRestartMsg
                    |> BrokerMsg)
    | _ -> m, Cmd.none

let RollingRestartDialog (model: BrokerSummaryViewModel) (dispatch: BrokerMsg -> unit) =

    match model.BrokerRollingRestartConfigurationDialog with
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
                             CloseBrokerRollingRestartDialog
                             |> BrokerRollingRestartMsg
                             |> dispatch)
                     dialog.children [ Mui.dialogTitle "Rolling Restart Cluster"
                                       Mui.dialogContent [ Mui.dialogContentText
                                                               "Start a rolling restart of the cluster. This will one by one try to shut down and restart each broker."
                                                           Mui.dialogContent [ errorMessage ]
                                                           Mui.textField [ textField.autoFocus true
                                                                           textField.margin.dense
                                                                           textField.label "Start Command"
                                                                           textField.value edit.StartCommand
                                                                           textField.fullWidth true
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> StartCommandEdited
                                                                                   |> BrokerRollingRestartMsg
                                                                                   |> dispatch) ]
                                                           Mui.textField [ textField.autoFocus false
                                                                           textField.margin.dense
                                                                           textField.label "Stop Command"
                                                                           textField.value edit.StopCommand
                                                                           textField.fullWidth true
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> StopCommandEdited
                                                                                   |> BrokerRollingRestartMsg
                                                                                   |> dispatch) ]
                                                           Mui.textField [ textField.autoFocus false
                                                                           textField.margin.dense
                                                                           textField.type' "number"
                                                                           textField.label
                                                                               "Health Check Interval (Seconds)"
                                                                           textField.fullWidth true
                                                                           textField.disabled false
                                                                           textField.value
                                                                               edit.HealthCheckIntervalInSeconds
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> HealthCheckIntervalInSecondsEdited
                                                                                   |> BrokerRollingRestartMsg
                                                                                   |> dispatch) ]
                                                           Mui.textField [ textField.autoFocus false
                                                                           textField.margin.dense
                                                                           textField.type' "number"
                                                                           textField.label "Stable Iterations Count"
                                                                           textField.fullWidth true
                                                                           textField.disabled false
                                                                           textField.value edit.StableIterationsCount
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> StableIterationsCountEdited
                                                                                   |> BrokerRollingRestartMsg
                                                                                   |> dispatch) ]
                                                           Mui.formControlLabel [ formControlLabel.label "Use Sudo"
                                                                                  formControlLabel.control (
                                                                                      Mui.checkbox [ checkbox.checked'
                                                                                                         edit.UseSudo
                                                                                                     checkbox.onChange
                                                                                                         (fun (_: Event) ->
                                                                                                             UseSudoToggled
                                                                                                             |> BrokerRollingRestartMsg
                                                                                                             |> dispatch) ]
                                                                                  ) ] ]
                                       Mui.dialogActions [ Mui.button [ button.color.primary
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                CloseBrokerRollingRestartDialog
                                                                                |> BrokerRollingRestartMsg
                                                                                |> dispatch)
                                                                        button.children "Cancel" ]
                                                           Mui.button [ button.color.primary
                                                                        button.children "Start"
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                edit
                                                                                |> StartRollingRestart
                                                                                |> BrokerRollingRestartMsg
                                                                                |> dispatch) ] ] ] ]
    | _ -> Mui.typography ""
