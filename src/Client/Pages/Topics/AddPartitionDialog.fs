module Client.Pages.Topics.AddPartitionDialog

open Client.Pages.Topics.Models
open Client.Models
open Feliz.MaterialUI
open Feliz
open Client.Pages.CommonLayoutItems
open Elmish
open Browser.Types
open Shared.Contracts


let update (api: ITopicsApi) (msg: AddPartitionDialogMsg) (m: Model) : Model * Cmd<Msg> =

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
        | ShowAddPartitionDialog c ->
            { m with
                  PageModel =
                      { vm with
                            AddPartitionDialogViewModel = Some c }
                      |> ManageTopicViewModel },
            Cmd.none
        | CloseAddPartitionDialog ->
            { m with
                  PageModel =
                      { vm with
                            AddPartitionDialogViewModel = None }
                      |> ManageTopicViewModel },
            Cmd.none
        | PartitionValueEdited s ->
            tryParseInteger
                s
                (fun parsedValue ->
                    { m with
                          PageModel =
                              { vm with
                                    AddPartitionDialogViewModel =
                                        addValueIfExists vm.AddPartitionDialogViewModel parsedValue }
                              |> ManageTopicViewModel },
                    Cmd.none)
                badIntegerErrorHandler
        | SaveAddPartitionDialogFailed s -> badIntegerErrorHandler s
        | SaveAddPartitionDialog c ->
            m,
            Cmd.OfAsync.either
                (Client.Pages.Topics.Data.tryToUpdatePartitions api)
                c
                (fun r -> r)
                (fun x ->
                    x.ToString()
                    |> SaveAddPartitionDialogFailed
                    |> AddPartitionDialogMsg
                    |> ManageTopicMsg
                    |> TopicMsg)
        | _ -> m, Cmd.none
    | _ -> m, Cmd.none

let addPartitionDialogView (model: ManageTopicViewModel) (dispatch: ManageTopicMsg -> unit) =

    match model.AddPartitionDialogViewModel with
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
                             CloseAddPartitionDialog
                             |> AddPartitionDialogMsg
                             |> dispatch)
                     dialog.children [ Mui.dialogTitle "Edit Topic Configuratin Property"
                                       Mui.dialogContent [ Mui.dialogContentText
                                                               "Consumers can then listen to events send on these topics"
                                                           Mui.dialogContent [ errorMessage ]
                                                           Mui.textField [ textField.autoFocus false
                                                                           textField.margin.dense
                                                                           textField.label "Previous Value"
                                                                           textField.fullWidth true
                                                                           textField.disabled true
                                                                           textField.type' "number"
                                                                           textField.value edit.Value ]
                                                           Mui.textField [ textField.autoFocus true
                                                                           textField.margin.dense
                                                                           textField.label "New Value"
                                                                           textField.fullWidth true
                                                                           textField.type' "number"
                                                                           textField.value edit.NewValue
                                                                           textField.onChange
                                                                               (fun x ->
                                                                                   x
                                                                                   |> PartitionValueEdited
                                                                                   |> AddPartitionDialogMsg
                                                                                   |> dispatch) ] ]
                                       Mui.dialogActions [ Mui.button [ button.color.primary
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                CloseAddPartitionDialog
                                                                                |> AddPartitionDialogMsg
                                                                                |> dispatch)
                                                                        button.children "Cancel" ]
                                                           Mui.button [ button.color.primary
                                                                        button.children "Start"
                                                                        prop.onClick
                                                                            (fun _ ->
                                                                                edit
                                                                                |> SaveAddPartitionDialog
                                                                                |> AddPartitionDialogMsg
                                                                                |> dispatch) ] ] ] ]
    | _ -> Mui.typography ""
