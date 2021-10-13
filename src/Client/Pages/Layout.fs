module Client.Layout

open Client.Models
open Fable.React
open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons
open Fable.MaterialUI.MaterialDesignIcons



let lightTheme =
    Styles.createMuiTheme (
        [ theme.palette.type'.light
          theme.palette.primary Colors.indigo
          theme.palette.secondary Colors.pink ]
    )

let darkTheme =
    Styles.createMuiTheme (
        [ theme.palette.type'.dark
          theme.palette.primary Colors.lightBlue
          theme.palette.secondary Colors.pink
          theme.props.muiAppBar [ appBar.color.default' ] ]
    )

let pageIcon page =
    match page with
    | Home -> homeIcon []
    | Brokers -> storageIcon []
    | Topics -> callSplitIcon []
    | Consumers -> earHearingIcon []
    | _ -> brightness1Icon []


let private pageListItem model dispatch page =
    Mui.listItem [ prop.key (pageTitle page)
                   prop.onClick (fun _ -> Navigate page |> dispatch)
                   listItem.button true
                   listItem.divider ((page = Home))
                   listItem.selected (model.Page = page)
                   listItem.children [ Mui.listItemIcon [ pageIcon page ]
                                       Mui.listItemText (pageTitle page) ] ]

let private pageView model dispatch =
    match model.Page with
    | Home -> Mui.typography "Your Dashboard For Exciting Kafka Management"
    | Brokers -> Client.Pages.Brokers.BrokerSummary.view model.PageModel dispatch
    | Topics -> Client.Pages.Topics.TopicSummary.view model.PageModel dispatch
    | Consumers -> Client.Pages.Consumers.ConsumerGroupSummary.view model.PageModel dispatch
    | Settings -> Mui.typography "Settings"

let private useToolbarTyles =
    Styles.makeStyles (fun styles theme -> {| appBarTitle = styles.create [ style.flexGrow 1 ] |})

let Toolbar =
    FunctionComponent.Of(
        (fun (page, customThemeMode, connection, dispatch) ->
            let c = useToolbarTyles ()

            Mui.toolbar [ Mui.typography [ typography.variant.h6
                                           typography.color.inherit'
                                           typography.children (pageTitle page)
                                           typography.classes.root c.appBarTitle ]
                          Mui.tooltip [ tooltip.title (
                                            match customThemeMode with
                                            | None -> "Using system light/dark theme"
                                            | Some Light -> "Using light theme"
                                            | Some Dark -> "Using dark theme"
                                        )
                                        tooltip.children (
                                            Mui.iconButton [ prop.onClick (fun _ -> dispatch ToggleCustomThemeMode)
                                                             iconButton.color.inherit'
                                                             iconButton.children [ match customThemeMode with
                                                                                   | None -> brightnessAutoIcon []
                                                                                   | Some Light -> brightness7Icon []
                                                                                   | Some Dark -> brightness4Icon [] ] ]

                                        ) ]

                          Mui.tooltip [ tooltip.title ("Connection Settings")
                                        tooltip.title (
                                            match connection with
                                            | None -> "Add a Connection"
                                            | Some x when x.IsConnected -> "Currently Connected"
                                            | Some x when not x.IsConnected -> "Currently Unconnected"
                                        )
                                        tooltip.children (
                                            Mui.iconButton [ prop.onClick (fun _ -> dispatch (Settings |> Navigate))
                                                             iconButton.color.inherit'
                                                             iconButton.children [ match connection with
                                                                                   | None -> pipeDisconnectedIcon []
                                                                                   | Some x when x.IsConnected ->
                                                                                       pipeIcon []
                                                                                   | Some x when not x.IsConnected ->
                                                                                       pipeDisconnectedIcon [] ] ]

                                        ) ] ]),
        "Toolbar",
        memoEqualsButFunctions
    )


let private useRootViewStyles =
    Styles.makeStyles
        (fun styles theme ->
            let drawerWidth = 240

            {| root =
                   styles.create
                       (fun model ->
                           [ style.display.flex
                             style.userSelect.none
                             if model.Page = Home then
                                 style.color Colors.green.``300`` ])
               appBar = styles.create [ style.zIndex (theme.zIndex.drawer + 1) ]
               appBarTitle = styles.create [ style.flexGrow 1 ]
               drawer =
                   styles.create [ style.width (length.px drawerWidth)
                                   style.flexShrink 0 ]
               drawerPaper =
                   styles.create [ style.width (length.px drawerWidth)
                                   // Example of breakpoint media queries
                                   style.inner theme.breakpoints.downXs [ style.backgroundColor.red ] ]
               content =
                   styles.create [ style.flexGrow 1
                                   style.padding (theme.spacing 5) ]
               toolbar = styles.create [ yield! theme.mixins.toolbar ] |})

let RootView =
    FunctionComponent.Of(
        (fun (model, dispatch) ->
            let c = useRootViewStyles model

            let currentTheme =
                match model.CustomThemeMode
                      |> Option.defaultValue model.SystemThemeMode with
                | Dark -> darkTheme
                | Light -> lightTheme

            Mui.themeProvider [ themeProvider.theme currentTheme
                                themeProvider.children [ Html.div [ prop.className c.root
                                                                    prop.children [ Mui.cssBaseline []
                                                                                    Mui.appBar [ appBar.classes.root
                                                                                                     c.appBar
                                                                                                 appBar.position.fixed'
                                                                                                 appBar.children [ Toolbar(
                                                                                                                       model.Page,
                                                                                                                       model.CustomThemeMode,
                                                                                                                       model.Connection,
                                                                                                                       dispatch
                                                                                                                   ) ] ]
                                                                                    Mui.drawer [ drawer.variant.permanent
                                                                                                 drawer.classes.root
                                                                                                     c.drawer
                                                                                                 drawer.classes.paper
                                                                                                     c.drawerPaper
                                                                                                 drawer.children [ Html.div [ prop.className
                                                                                                                                  c.toolbar ]
                                                                                                                   Mui.list [ list.component'
                                                                                                                                  "nav"
                                                                                                                              list.children (
                                                                                                                                  Page.ParentMenuPages
                                                                                                                                  |> List.map (
                                                                                                                                      pageListItem
                                                                                                                                          model
                                                                                                                                          dispatch
                                                                                                                                  )
                                                                                                                                  |> ofList
                                                                                                                              ) ] ] ]
                                                                                    Html.main [ prop.className c.content
                                                                                                prop.children [ Html.div [ prop.className
                                                                                                                               c.toolbar ]
                                                                                                                pageView
                                                                                                                    model
                                                                                                                    dispatch ] ] ] ] ] ]),
        "RootView",
        memoEqualsButFunctions
    )
