module Client.Pages.CommonLayoutItems

open Feliz.MaterialUI
open Feliz
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Feliz.MaterialUI.MaterialTable


let tryParseInteger (str: string) goodFunc errorFunc =
    let couldParse, parsedValue = System.Int32.TryParse(str)

    if couldParse then
        goodFunc parsedValue
    else
        errorFunc str

let summaryCard (size: IReactProperty) (key: string) (value: string) =
    Mui.grid [ grid.item true
               size
               grid.justify.center
               grid.children [ Mui.paper [ Mui.typography [ typography.variant.h3
                                                            typography.align.center
                                                            typography.children value ]
                                           Mui.typography [ typography.variant.h6
                                                            typography.align.center
                                                            typography.children key ] ] ] ]


let genericPageLayout
    (title: string)
    (description: string)
    dialogs
    (actions: ReactElement)
    (summaryCards: ReactElement)
    content
    : ReactElement =
    Html.div [ prop.children [ Mui.grid [ grid.container true
                                          grid.spacing._3
                                          grid.justify.flexStart
                                          grid.alignItems.flexStart
                                          grid.alignContent.center
                                          grid.children [ Mui.grid [ grid.item true
                                                                     grid.xs._12
                                                                     grid.children [ Mui.typography [ typography.component'
                                                                                                          "h1"
                                                                                                      typography.variant.h3
                                                                                                      typography.children (
                                                                                                          title
                                                                                                      ) ]
                                                                                     Mui.typography [ typography.paragraph
                                                                                                          true
                                                                                                      typography.children
                                                                                                          description ] ] ]
                                                          yield! dialogs
                                                          Mui.grid [ grid.item true
                                                                     grid.alignContent.flexEnd
                                                                     grid.xs._12
                                                                     grid.children [ Mui.divider [] ] ]
                                                          actions
                                                          Mui.grid [ grid.item true
                                                                     grid.alignContent.flexEnd
                                                                     grid.xs._12
                                                                     grid.children [ Mui.divider [] ] ]
                                                          Mui.grid [ grid.item true
                                                                     grid.alignContent.flexEnd
                                                                     grid.xs._12
                                                                     grid.children [ summaryCards ] ]
                                                          Mui.grid [ grid.item true
                                                                     grid.alignContent.flexEnd
                                                                     grid.xs._12
                                                                     grid.children [ Mui.divider [] ] ]
                                                          yield! content


                                                           ] ] ] ]

let genericFormatedTable<'T> (title: string) showTitle columns (data: 'T list) actions =
    Mui.materialTable [ materialTable.title title
                        materialTable.columns columns
                        materialTable.data data
                        materialTable.actions actions
                        materialTable.options [ options.headerStyle [ style.paddingRight (length.em 1)
                                                                      style.fontVariant.smallCaps
                                                                      style.textAlign.left
                                                                      style.textJustify.none ]
                                                options.actionsColumnIndex 80
                                                options.showTitle showTitle
                                                options.exportButton true
                                                options.exportAllData true
                                                options.pageSize 15
                                                options.pageSizeOptions [ 15; 30; 60 ] ] ]
