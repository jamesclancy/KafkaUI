module Client.Pages.Topics.Models

open Shared.Dtos

type EditTopicConfigurationPropertyViewModel =
    { TopicName: string
      NewValue: string
      Name: string
      Value: string
      Description: string option
      KafkaDefaultValue: string option
      BrokerDefaultValue: string option
      ErrorMessage: string option }
    static member FromTopicConfigurationProperty topicName (model: TopicConfigurationProperty) =
        { TopicName = topicName
          NewValue = model.Value
          BrokerDefaultValue = model.BrokerDefaultValue
          Description = model.Description
          KafkaDefaultValue = model.KafkaDefaultValue
          ErrorMessage = None
          Name = model.Name
          Value = model.Value }

    member this.AsTopicConfigurationProperty =
        { Locked = false
          Value = this.NewValue
          BrokerDefaultValue = this.BrokerDefaultValue
          Description = this.Description
          KafkaDefaultValue = this.KafkaDefaultValue
          Name = this.Name }

type CreateTopicViewModel =
    { TopicName: string
      Partitions: int
      ReplicationFactor: int
      CleanupPolicyUseRetentionPolicy: bool
      CleanUpPolicyUseCompaction: bool
      Properties: EditTopicConfigurationPropertyViewModel list
      ErrorMessage: string option }
    static member Default =
        { TopicName = (System.Guid.NewGuid().ToString())
          Partitions = 3
          ReplicationFactor = 1
          CleanupPolicyUseRetentionPolicy = false
          CleanUpPolicyUseCompaction = false
          Properties = List.Empty
          ErrorMessage = None }

    static member FromCreateTopicRequest(request: CreateTopicRequest) =
        { TopicName = request.TopicName
          Partitions = request.Partitions
          ReplicationFactor = request.ReplicationFactor
          CleanupPolicyUseRetentionPolicy = request.CleanupPolicyUseRetentionPolicy
          CleanUpPolicyUseCompaction = request.CleanUpPolicyUseCompaction
          Properties =
              request.ConfigurationProperties
              |> List.map (EditTopicConfigurationPropertyViewModel.FromTopicConfigurationProperty request.TopicName)
          ErrorMessage = None }

    member this.AsCreateTopicRequest =
        { TopicName = this.TopicName
          Partitions = this.Partitions
          ReplicationFactor = this.ReplicationFactor
          CleanupPolicyUseRetentionPolicy = this.CleanupPolicyUseRetentionPolicy
          CleanUpPolicyUseCompaction = this.CleanUpPolicyUseCompaction
          ConfigurationProperties =
              this.Properties
              |> List.map (fun x -> x.AsTopicConfigurationProperty) }

type TopicSummaryViewModel =
    { Name: string
      Partitions: int
      UnderReplicatedPartitions: int
      Count: int64
      SizeInBytes: int
      Consumers: int }
    static member FromTopicSummary(summary: TopicSummary) =
        { Name = summary.Name
          Partitions = summary.Partitions
          UnderReplicatedPartitions = summary.UnderReplicatedPartitions
          Count = summary.Count
          SizeInBytes = summary.SizeInBytes
          Consumers = summary.Consumers }

    member this.CountAsInt32 = System.Convert.ToInt32(this.Count)

type TopicsSummaryViewModel =
    { TopicCount: int
      Partitions: int
      UnderReplicatedPartitions: int
      TopicsWithoutLeader: int
      InSyncReplicaIssues: int

      CreateDialog: CreateTopicViewModel option
      AvailableTopics: TopicSummaryViewModel list }
    static member FromTopicsSummary(summary: TopicsSummary) =
        { TopicCount = summary.TopicCount
          Partitions = summary.Partitions
          UnderReplicatedPartitions = summary.UnderReplicatedPartitions
          TopicsWithoutLeader = summary.PartitionsWithoutLeader
          InSyncReplicaIssues = summary.InSyncReplicaIssues
          CreateDialog = None
          AvailableTopics =
              summary.AvailableTopics
              |> List.map TopicSummaryViewModel.FromTopicSummary }


type ManageTopicConfigurationPropertyEditViewModel =
    { Locked: bool
      Name: string
      Value: string
      NewValue: string
      Description: string option
      KafkaDefaultValue: string option
      ErrorMessage: string option }
    member this.AsTopicConfigurationProperty =
        { Locked = this.Locked
          Description = this.Description
          KafkaDefaultValue = this.KafkaDefaultValue
          BrokerDefaultValue = None
          Name = this.Name
          Value = this.NewValue }

    static member FromTopicConfigurationProperty(model: TopicConfigurationProperty) =
        { NewValue = model.Value
          Locked = false
          Description = model.Description
          KafkaDefaultValue = model.KafkaDefaultValue
          ErrorMessage = None
          Name = model.Name
          Value = model.Value }

type ManageTopicTabSelection =
    | CustomerGroupsTab
    | PartitionsTab
    | BrokersTab
    | ConfigurationTab

type AddPartitionDialogViewModel =
    { ErrorMessage: string option
      Value: int
      NewValue: int
      TopicName: string }

type ManageTopicViewModel =
    { TopicName: string
      EventCount: int64
      PartitionCount: int
      PartitionsWithoutLeaderCount: int
      ReplicationFactor: int
      InSyncReplicaIssueCount: int

      IsCompacted: bool

      EditTopicConfigurationPropertyDialog: ManageTopicConfigurationPropertyEditViewModel option
      ManageTopicTabSelection: ManageTopicTabSelection

      AddPartitionDialogViewModel: AddPartitionDialogViewModel option
      ShowPurgeTopicConfirmationDialog: bool
      PurgeTopicConfirmationDialogErrorMessage: string option

      ConsumerGroups: TopicConsumerGroupSummary list
      Partitions: TopicPartitionSummary list
      Brokers: TopicBrokerSummary list
      Configuration: TopicConfigurationProperty list }
    static member FromTopicDetails(topicDetails: TopicDetails) =
        { TopicName = topicDetails.TopicName
          EventCount = topicDetails.EventCount
          PartitionCount = topicDetails.PartitionCount
          PartitionsWithoutLeaderCount = topicDetails.PartitionsWithoutLeaderCount
          ReplicationFactor = topicDetails.ReplicationFactor
          InSyncReplicaIssueCount = topicDetails.InSyncReplicaIssueCount
          IsCompacted = topicDetails.IsCompacted

          ConsumerGroups = topicDetails.ConsumerGroups
          Partitions = topicDetails.Partitions
          Brokers = topicDetails.Brokers
          Configuration = topicDetails.Configuration

          ShowPurgeTopicConfirmationDialog = false
          PurgeTopicConfirmationDialogErrorMessage = None
          ManageTopicTabSelection = CustomerGroupsTab
          EditTopicConfigurationPropertyDialog = None
          AddPartitionDialogViewModel = None }

type EditTopicConfigurationPropertyMsg =
    | ShowEditTopicConfigurationPropertyDialog of TopicConfigurationProperty
    | SaveEditTopicConfigurationPropertyDialog of ManageTopicConfigurationPropertyEditViewModel
    | NewValueEdited of string
    | CloseEditTopicConfigurationPropertyDialog
    | EditTopicConfigurationPropertyFailed of string

type AddPartitionDialogMsg =
    | ShowAddPartitionDialog of AddPartitionDialogViewModel
    | CloseAddPartitionDialog
    | SaveAddPartitionDialog of AddPartitionDialogViewModel
    | SaveAddPartitionDialogFailed of string
    | PartitionValueEdited of string

type PurgeTopicConfirmationDialogMsg =
    | ShowPurgeTopicConfirmationDialog
    | ClosePurgeTopicConfirmationDialog
    | SavePurgeTopicConfirmationDialog of string
    | SavePurgeTopicConfirmationDialogFailed of string


type ManageTopicMsg =
    | LoadManageTopicViewModel of string
    | LoadedManageTopicViewModel of ManageTopicViewModel
    | ChangeManageTopicTabSelection of ManageTopicTabSelection

    | AddPartitionDialogMsg of AddPartitionDialogMsg
    | PurgeTopicConfirmationDialogMsg of PurgeTopicConfirmationDialogMsg
    | EditTopicConfigurationPropertyMsg of EditTopicConfigurationPropertyMsg

type CreateTopicMsg =
    | ShowCreateTopicDialog
    | CloseCreateTopicDialog
    | TopicNameEdited of string
    | PartitionsEdited of string
    | ReplicationFactorEdited of string
    | CleanupPolicyUseRetentionPolicyToggled
    | CleanUpPolicyUseCompactionToggled

    | CreateTopic of CreateTopicViewModel
    | CreateTopicFailed of string

type TopicMsg =
    | LoadTopicSummaryViewModel
    | LoadedTopicSummaryViewModel of TopicsSummaryViewModel

    | ManageTopicMsg of ManageTopicMsg
    | CreateTopicMsg of CreateTopicMsg
