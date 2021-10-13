module Client.Pages.Topics.PurgeTopicConfirmationDialog

open Client.Pages.Topics.Models
open Client.Models
open Feliz.MaterialUI
open Feliz
open Client.Pages.CommonLayoutItems
open Elmish
open Browser.Types
open Shared.Contracts


let update (api: ITopicsApi) (msg: PurgeTopicConfirmationDialogMsg) (m: Model) : Model * Cmd<Msg> =
    let addErrorIfExists (optionalValue: AddPartitionDialogViewModel option) (errorMsg: string) =
        match optionalValue with
        | None -> None
        | Some s -> Some { s with ErrorMessage = Some errorMsg }

    let addValueIfExists (optionalValue: AddPartitionDialogViewModel option) (newValue: int) =
        match optionalValue with
        | None -> None
        | Some s -> Some { s with NewValue = newValue }

    match m.PageModel with
    | ManageTopicViewModel vm ->

        let badIntegerErrorHandler =
            (fun badValue ->
                { m with
                      PageModel =
                          { vm with
                                AddPartitionDialogViewModel =
                                    addErrorIfExists
                                        vm.AddPartitionDialogViewModel
                                        (sprintf "Unable to parse %s as an integer" badValue) }
                          |> ManageTopicViewModel },
                Cmd.none)

        match msg with
        | ShowPurgeTopicConfirmationDialog ->
            { m with
                  PageModel =
                      { vm with
                            ShowPurgeTopicConfirmationDialog = true }
                      |> ManageTopicViewModel },
            Cmd.none
        | ClosePurgeTopicConfirmationDialog ->
            { m with
                  PageModel =
                      { vm with
                            ShowPurgeTopicConfirmationDialog = false }
                      |> ManageTopicViewModel },
            Cmd.none
        | SavePurgeTopicConfirmationDialogFailed s ->
            { m with
                  PageModel =
                      { vm with
                            PurgeTopicConfirmationDialogErrorMessage = Some s }
                      |> ManageTopicViewModel },
            Cmd.none
        | SavePurgeTopicConfirmationDialog tn ->
            m,
            Cmd.OfAsync.either
                (Client.Pages.Topics.Data.tryToPurgeTopic api)
                tn
                (fun r -> r)
                (fun x ->
                    x.ToString()
                    |> SavePurgeTopicConfirmationDialogFailed
                    |> PurgeTopicConfirmationDialogMsg
                    |> ManageTopicMsg
                    |> TopicMsg)
        | _ -> m, Cmd.none
    | _ -> m, Cmd.none

let purgeTopicConfirmationDialogView (model: ManageTopicViewModel) (dispatch: ManageTopicMsg -> unit) =

    match model.ShowPurgeTopicConfirmationDialog with
    | true ->
        let errorMessage =
            match model.PurgeTopicConfirmationDialogErrorMessage with
            | Some x ->
                Mui.alert [ alert.severity.error
                            prop.children [ Mui.alertTitle [ prop.text x ] ] ]
            | None -> Mui.typography ""

        Mui.dialog [ dialog.open' true
                     dialog.onClose
                         (fun _ ->
                             ClosePurgeTopicConfirmationDialog
                             |> PurgeTopicConfirmationDialogMsg
                             |> dispatch)
                     dialog.children [ Mui.dialogTitle "Are you sure you want to purge the topic?"
                                       Mui.dialogContent [ Mui.dialogContentText
                                                               "This is dangerous and cannot be reversed."
                                                           Mui.dialogContent [ errorMessage ] ]
                                       Mui.dialogActions [ Mui.button [ button.color.primary
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                ClosePurgeTopicConfirmationDialog
                                                                                |> PurgeTopicConfirmationDialogMsg
                                                                                |> dispatch)
                                                                        button.children "Cancel" ]
                                                           Mui.button [ button.color.primary
                                                                        button.children "Start"
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                model.TopicName
                                                                                |> SavePurgeTopicConfirmationDialog
                                                                                |> PurgeTopicConfirmationDialogMsg
                                                                                |> dispatch) ] ] ] ]
    | _ -> Mui.typography ""
