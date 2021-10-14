module Client.Pages.Consumers.Models

open Shared.Dtos

type CreateConsumerDialogViewModel =
    {

      AvailableTopics: string list
      SelectedTopic: string option }

type ConsumerGroupSummaryConsumerGroupSummaryViewModel =
    { ConsumerGroupName: string
      State: string
      Lag: int
      Members: int
      AssignedPartitions: int
      AssignedTopics: int
      PartitionAssignmentStrategy: string } //https://medium.com/streamthoughts/understanding-kafka-partition-assignment-strategies-and-how-to-write-your-own-custom-assignor-ebeda1fc06f3
    static member FromConsumerGroupSummaryConsumerGroupSummary(dto: ConsumerGroupSummaryConsumerGroupSummary) =
        { ConsumerGroupName = dto.ConsumerGroupName
          State = dto.State
          Lag = dto.Lag
          Members = dto.Members
          AssignedPartitions = dto.AssignedPartitions
          AssignedTopics = dto.AssignedTopics
          PartitionAssignmentStrategy = dto.PartitionAssignmentStrategy }

type ConsumerGroupSummaryViewModel =
    { ConsumerCount: int
      ConsumerStates: Map<string, int>
      ConsumerGroups: ConsumerGroupSummaryConsumerGroupSummaryViewModel list }
    static member FromConsumerGroupSummary(customerGroupSummary: ConsumerGroupSummary) =
        { ConsumerCount = customerGroupSummary.ConsumerCount
          ConsumerStates = customerGroupSummary.ConsumerStates
          ConsumerGroups =
              customerGroupSummary.ConsumerGroups
              |> List.map ConsumerGroupSummaryConsumerGroupSummaryViewModel.FromConsumerGroupSummaryConsumerGroupSummary }

type ManageConsumerGroupMemberViewModel =
    { ConsumerGroupName: string
      MemberType: string
      MemberId: string
      ClientId: string
      HostName: string
      Lag: int
      AssignedPartitions: int
      AssignedTopic: string
      CursorLocation: int
      CusorEnd: int }
    static member FromConsumerGroupDetailMember(m: ConsumerGroupDetailMember) =
        { ConsumerGroupName = m.ConsumerGroupName
          MemberType = m.MemberType
          MemberId = m.MemberId
          ClientId = m.ClientId
          HostName = m.HostName
          Lag = m.Lag
          AssignedPartitions = m.AssignedPartitions
          AssignedTopic = m.AssignedTopic
          CursorLocation = m.CursorLocation
          CusorEnd = m.CusorEnd }

type AlterConsumerGroupOffsetDialogMode =
    | DoNothing
    | Begining
    | Latest
    | TakeLastNumberOfEvents
    | SpecificDate

type AlterConsumerGroupOffsetDialogViewModel =
    { ConsumerGroupName: string
      AlterConsumerGroupOffsetDialogMode: AlterConsumerGroupOffsetDialogMode

      CanSpecifyDate: bool
      SpecifiedDate: System.DateTime option

      CanSpecifyLastNumberOfEvents: bool
      SpecifiedLastNumnerOfEvents: int option }
    static member DefaultForConsumerGroupName consumerGroupName =
        { ConsumerGroupName = consumerGroupName
          AlterConsumerGroupOffsetDialogMode = DoNothing
          CanSpecifyDate = false
          SpecifiedDate = None
          CanSpecifyLastNumberOfEvents = false
          SpecifiedLastNumnerOfEvents = None }

type ManageConsumerGroupViewModel =
    { ConsumerGroupName: string
      State: string
      Lag: int
      MemberCount: int
      AssignedPartitions: int
      AssignedTopics: int
      PartitionAssignmentStrategy: string //https://medium.com/streamthoughts/understanding-kafka-partition-assignment-strategies-and-how-to-write-your-own-custom-assignor-ebeda1fc06f3
      Members: ManageConsumerGroupMemberViewModel list

      ShowDeleteConsumerGroupConfirmationDialog: bool
      DeleteConsumerGroupConfirmationDialogErrorMessage: string option
      ShowRebalanceConsumerGroupConfirmationDialog: bool
      RebalanceConsumerGroupConfirmationDialogErrorMessage: string option }
    static member FromConsumerGroupDetail(dto: ConsumerGroupDetail) =
        { ConsumerGroupName = dto.ConsumerGroupName
          State = dto.State
          Lag = dto.Lag
          MemberCount = dto.MemberCount
          AssignedPartitions = dto.AssignedPartitions
          AssignedTopics = dto.AssignedTopics
          PartitionAssignmentStrategy = dto.PartitionAssignmentStrategy
          Members =
              dto.Members
              |> List.map ManageConsumerGroupMemberViewModel.FromConsumerGroupDetailMember

          ShowDeleteConsumerGroupConfirmationDialog = false
          DeleteConsumerGroupConfirmationDialogErrorMessage = None
          ShowRebalanceConsumerGroupConfirmationDialog = false
          RebalanceConsumerGroupConfirmationDialogErrorMessage = None }

type DeleteConsumerGroupConfirmationDialogMsg =
    | ShowDeleteConsumerGroupConfirmationDialog of string
    | CloseDeleteConsumerGroupConfirmationDialog
    | SaveDeleteConsumerGroupConfirmationDialog of string
    | SaveDeleteConsumerGroupConfirmationDialogFailed of string

type RebalanceConsumerGroupConfirmationDialogMsg =
    | ShowRebalanceConsumerGroupConfirmationDialog of string
    | CloseRebalanceConsumerGroupConfirmationDialog
    | SaveRebalanceConsumerGroupConfirmationDialog of string
    | SaveRebalanceConsumerGroupConfirmationDialogFailed of string

type AlterConsumerGroupOffsetDialogMsg =
    | ShowAlterConsumerGroupOffsetDialog of AlterConsumerGroupOffsetDialogViewModel
    | CloseAlterConsumerGroupOffsetDialog
    | SaveAlterConsumerGroupOffsetDialog of AlterConsumerGroupOffsetDialogViewModel
    | SaveAlterConsumerGroupOffsetDialogFailed of string

type ManageConsumerGroupMsg =
    | LoadManageConsumerGroupViewModel of string
    | LoadedManageConsumerGroupViewModel of ManageConsumerGroupViewModel

    | DeleteConsumerGroupConfirmationDialogMsg of DeleteConsumerGroupConfirmationDialogMsg
    | RebalanceConsumerGroupConfirmationDialogMsg of RebalanceConsumerGroupConfirmationDialogMsg
    | AlterConsumerGroupOffsetDialogMsg of AlterConsumerGroupOffsetDialogMsg

type ConsumerMsg =
    | LoadConsumerGroupSummaryViewModel
    | LoadedConsumerGroupSummaryViewModel of ConsumerGroupSummaryViewModel

    | ManageConsumerGroupMsg of ManageConsumerGroupMsg
