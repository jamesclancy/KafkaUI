module Client.Pages.Consumers.DeleteConsumerGroupConfirmationDialog

open Client.Pages.Consumers.Models
open Client.Models
open Feliz.MaterialUI
open Feliz
open Client.Pages.CommonLayoutItems
open Elmish
open Browser.Types
open Shared.Contracts


let update (api: IConsumersApi) (msg: DeleteConsumerGroupConfirmationDialogMsg) (m: Model) : Model * Cmd<Msg> =

    match m.PageModel with
    | ManageConsumerGroupViewModel vm ->

        match msg with
        | ShowDeleteConsumerGroupConfirmationDialog c ->
            { m with
                  PageModel =
                      { vm with
                            ShowDeleteConsumerGroupConfirmationDialog = true }
                      |> ManageConsumerGroupViewModel },
            Cmd.none
        | CloseDeleteConsumerGroupConfirmationDialog ->
            { m with
                  PageModel =
                      { vm with
                            ShowDeleteConsumerGroupConfirmationDialog = false }
                      |> ManageConsumerGroupViewModel },
            Cmd.none
        | SaveDeleteConsumerGroupConfirmationDialogFailed s ->
            { m with
                  PageModel =
                      { vm with
                            DeleteConsumerGroupConfirmationDialogErrorMessage = Some s }
                      |> ManageConsumerGroupViewModel },
            Cmd.none
        | SaveDeleteConsumerGroupConfirmationDialog tn ->
            m,
            Cmd.OfAsync.either
                (Client.Pages.Consumers.Data.tryToDeleteCustomerGroup api)
                tn
                (fun r -> r)
                (fun x ->
                    x.ToString()
                    |> SaveDeleteConsumerGroupConfirmationDialogFailed
                    |> DeleteConsumerGroupConfirmationDialogMsg
                    |> ManageConsumerGroupMsg
                    |> ConsumerMsg)
        | _ -> m, Cmd.none
    | _ -> m, Cmd.none

let DeleteConsumerGroupConfirmationDialogView
    (model: ManageConsumerGroupViewModel)
    (dispatch: ManageConsumerGroupMsg -> unit)
    =

    match model.ShowDeleteConsumerGroupConfirmationDialog with
    | true ->
        let errorMessage =
            match model.DeleteConsumerGroupConfirmationDialogErrorMessage with
            | Some x ->
                Mui.alert [ alert.severity.error
                            prop.children [ Mui.alertTitle [ prop.text x ] ] ]
            | None -> Mui.typography ""

        Mui.dialog [ dialog.open' true
                     dialog.onClose
                         (fun _ ->
                             CloseDeleteConsumerGroupConfirmationDialog
                             |> DeleteConsumerGroupConfirmationDialogMsg
                             |> dispatch)
                     dialog.children [ Mui.dialogTitle "Are you sure you want to purge the topic?"
                                       Mui.dialogContent [ Mui.dialogContentText
                                                               "This is dangerous and cannot be reversed."
                                                           Mui.dialogContent [ errorMessage ] ]
                                       Mui.dialogActions [ Mui.button [ button.color.primary
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                CloseDeleteConsumerGroupConfirmationDialog
                                                                                |> DeleteConsumerGroupConfirmationDialogMsg
                                                                                |> dispatch)
                                                                        button.children "Cancel" ]
                                                           Mui.button [ button.color.primary
                                                                        button.children "Start"
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                model.ConsumerGroupName
                                                                                |> SaveDeleteConsumerGroupConfirmationDialog
                                                                                |> DeleteConsumerGroupConfirmationDialogMsg
                                                                                |> dispatch) ] ] ] ]
    | _ -> Mui.typography ""
