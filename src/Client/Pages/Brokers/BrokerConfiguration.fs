module Client.Pages.Brokers.BrokerConfiguration

open Client.Pages.Brokers.Models
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Feliz.MaterialUI.MaterialTable
open Elmish
open Browser.Types
open Client.Pages.Brokers.BrokerEditConfiguration
open Shared.Dtos

let brokerConfigurationTable (model: BrokerConfigurationDetailViewModel) dispatch =
    Client.Pages.CommonLayoutItems.genericFormatedTable
        "Configuration"
        true
        [ columns.column [ column.title "Name"
                           column.field<BrokerConfigurationProperty> (fun rd -> nameof rd.Name) ]
          columns.column [ column.title "Value"
                           column.field<BrokerConfigurationProperty> (fun rd -> nameof rd.Value) ] ]
        model.Properties
        [ actions.action<BrokerConfigurationProperty>
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
                                |> StartEditOfConfigurationProperty
                                |> BrokerEditMsg
                                |> dispatch) ]) ]


let brokerConfigurationHeader (model: BrokerConfigurationDetailViewModel) dispatch =
    [ Mui.grid [ grid.item true
                 grid.alignContent.flexEnd
                 grid.xs._10
                 grid.children [ Mui.typography [ typography.component' "h1"
                                                  typography.variant.h3
                                                  typography.children (
                                                      sprintf "Configuration for BrokerId:%s" model.BrokerId
                                                  ) ]
                                 Mui.typography [ typography.paragraph true
                                                  typography.children
                                                      "A Kakfa broker is an actual server/node which recieves events from a consumer and eventually delivers them to a consumer." ] ] ]
      Mui.grid [ grid.item true
                 grid.alignContent.flexEnd
                 grid.xs._2
                 grid.children [ Mui.fab [ fab.color.primary
                                           fab.size.medium
                                           fab.children [ Mui.tooltip [ tooltip.title "Back to Broker List"
                                                                        tooltip.children (arrowBackIcon []) ] ]
                                           prop.onClick (fun _ -> dispatch LoadBrokerSummaryViewModel) ]

                                  ] ]
      Mui.grid [ grid.item true
                 grid.alignContent.flexEnd
                 grid.xs._12
                 grid.children [ Mui.divider [] ] ]
      Mui.grid [ grid.item true
                 grid.alignContent.flexEnd
                 grid.xs._12
                 grid.children [ Mui.table [ table.size.medium
                                             table.children [ Mui.tableRow [ tableRow.children [ Mui.tableCell
                                                                                                     "Host Name"
                                                                                                 Mui.tableCell
                                                                                                     model.Hostname ] ]
                                                              Mui.tableRow [ tableRow.children [ Mui.tableCell "Port"
                                                                                                 Mui.tableCell
                                                                                                     model.Port ] ]
                                                              Mui.tableRow [ tableRow.children [ Mui.tableCell
                                                                                                     "Partitions"
                                                                                                 Mui.tableCell (
                                                                                                     model.Partitions.ToString
                                                                                                         ()
                                                                                                 ) ] ]
                                                              Mui.tableRow [ tableRow.children [ Mui.tableCell "Relicas"
                                                                                                 Mui.tableCell (
                                                                                                     model.Relicas.ToString
                                                                                                         ()
                                                                                                 ) ] ]
                                                              Mui.tableRow [ tableRow.children [ Mui.tableCell
                                                                                                     "Under Replicated Partitions"
                                                                                                 Mui.tableCell (
                                                                                                     model.UnderReplicatedPartitions.ToString
                                                                                                         ()
                                                                                                 ) ] ] ] ] ] ] ]



let brokerConfigurationView =
    FunctionComponent.Of(
        (fun (model: BrokerConfigurationDetailViewModel, dispatch: BrokerMsg -> unit) ->
            Html.div [ prop.children [ Mui.grid [ grid.container true
                                                  grid.spacing._3
                                                  grid.justify.flexStart
                                                  grid.alignItems.flexStart
                                                  grid.alignContent.center
                                                  grid.children [ yield! brokerConfigurationHeader model dispatch
                                                                  EditDialog model dispatch
                                                                  Mui.grid [ grid.item true
                                                                             grid.alignContent.flexEnd
                                                                             grid.xs._12
                                                                             grid.children [ Mui.divider [] ] ]
                                                                  Mui.grid [ grid.item true
                                                                             grid.alignContent.flexEnd
                                                                             grid.xs._12
                                                                             grid.children [ brokerConfigurationTable
                                                                                                 model
                                                                                                 dispatch ] ] ] ] ] ]),
        "BrokerConfigurationPage",
        memoEqualsButFunctions
    )
