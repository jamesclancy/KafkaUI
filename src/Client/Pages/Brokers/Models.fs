module Client.Pages.Brokers.Models

open Shared.Dtos

type BrokerSummaryDetailViewModel =
    { Id: string
      Rack: string
      Listener: string
      Paritions: int
      PartitionSkew: int
      Leaders: int
      LeaderSkew: int
      Replicas: int
      UnderReplicatedPartitions: int
      SizeInBytes: int }
    static member FromBrokerSummary(brokerSummary: BrokerSummary) =
        { Id = brokerSummary.Id
          Rack = brokerSummary.Rack
          Listener = brokerSummary.Listener
          Paritions = brokerSummary.Paritions
          PartitionSkew = brokerSummary.PartitionSkew
          Leaders = brokerSummary.Leaders
          LeaderSkew = brokerSummary.LeaderSkew
          Replicas = brokerSummary.Replicas
          UnderReplicatedPartitions = brokerSummary.UnderReplicatedPartitions
          SizeInBytes = brokerSummary.SizeInBytes }


type BrokerRollingRestartConfiguration =
    { UseSudo: bool
      StopCommand: string
      StartCommand: string
      HealthCheckIntervalInSeconds: int
      StableIterationsCount: int
      ErrorMessage: string option }
    static member Default =
        { UseSudo = false
          StopCommand = "systemctl stop kafka"
          StartCommand = "systemctl start kafka"
          HealthCheckIntervalInSeconds = 1
          StableIterationsCount = 6
          ErrorMessage = None }

    member this.ToBrokerRollingRestartRequest: BrokerRollingRestartRequest =
        { UseSudo = this.UseSudo
          StopCommand = this.StopCommand
          StartCommand = this.StartCommand
          HealthCheckIntervalInSeconds = this.HealthCheckIntervalInSeconds
          StableIterationsCount = this.StableIterationsCount }

type BrokerSummaryViewModel =
    { BrokerCount: int
      ClusterId: string
      Controller: bool
      Version: string
      SimilarConfiguration: bool

      BrokerRollingRestartConfigurationDialog: BrokerRollingRestartConfiguration option
      AvailableBrokers: BrokerSummaryDetailViewModel list }
    static member FromBrokersSummary(metaData: BrokersSummary) =
        { BrokerCount = metaData.BrokerCount
          ClusterId = metaData.ClusterId
          Controller = metaData.Controller
          Version = metaData.Version
          SimilarConfiguration = metaData.SimilarConfiguration
          AvailableBrokers =
              List.map (fun x -> BrokerSummaryDetailViewModel.FromBrokerSummary x) metaData.AvailableBrokers
          BrokerRollingRestartConfigurationDialog = None }

type BrokerConfigurationEditPropertyViewModel =
    { BrokerId: string
      NewValue: string
      ApplyToAllBrokersInCluster: bool
      Description: string option
      KafkaDefaultValue: string option
      ErrorMessage: string option
      Name: string
      Value: string }
    static member FromBrokerConfigurationProperty brokerId (model: BrokerConfigurationProperty) =
        { BrokerId = brokerId
          NewValue = model.Value
          ApplyToAllBrokersInCluster = false
          Description = model.Description
          KafkaDefaultValue = model.KafkaDefaultValue
          ErrorMessage = None
          Name = model.Name
          Value = model.Value }

    member this.ToBrokerConfigurationProperty =
        { Description = this.Description
          KafkaDefaultValue = this.KafkaDefaultValue
          Name = this.Name
          Locked = false
          Value = this.NewValue }

type BrokerConfigurationDetailViewModel =
    { BrokerId: string
      Hostname: string
      Port: string
      Partitions: int
      Relicas: int
      UnderReplicatedPartitions: int
      HideReadOnly: bool
      ShowOverridesOnly: bool
      Properties: BrokerConfigurationProperty list
      ConfigurationSettingToEdit: BrokerConfigurationEditPropertyViewModel option }
    static member FromBrokerDetails(brokerDetails: BrokerDetails) =
        { BrokerId = brokerDetails.BrokerId
          Hostname = brokerDetails.Hostname
          Port = brokerDetails.Port
          Partitions = brokerDetails.Partitions
          Relicas = brokerDetails.Relicas
          UnderReplicatedPartitions = brokerDetails.UnderReplicatedPartitions
          HideReadOnly = brokerDetails.HideReadOnly
          ShowOverridesOnly = brokerDetails.ShowOverridesOnly
          Properties = brokerDetails.Properties
          ConfigurationSettingToEdit = None }


type BrokerLoadConfiguration = { BrokerId: string }

type BrokerEditMsg =
    | StartEditOfConfigurationProperty of BrokerConfigurationProperty
    | CancelEditOfConfigurationProperty
    | SaveEditOfConfigurationProperty of BrokerConfigurationEditPropertyViewModel
    | EditOfConfigurationPropertyValueChanged of string
    | EditOfConfigurationPropertyToggleApplyToAllBrokersInCluster
    | SaveEditOfConfigurationPropertyFailed of exn

type BrokerRollingRestartMsg =
    | ShowBrokerRollingRestartDialog
    | CloseBrokerRollingRestartDialog
    | StopCommandEdited of string
    | StartCommandEdited of string
    | HealthCheckIntervalInSecondsEdited of string
    | StableIterationsCountEdited of string
    | StartRollingRestartFailed of string
    | UseSudoToggled
    | StartRollingRestart of BrokerRollingRestartConfiguration

type BrokerMsg =
    | RebalanceCluster
    | RollingRestart of BrokerRollingRestartConfiguration
    | BrokerLoadConfiguration of BrokerLoadConfiguration
    | LoadBrokerSummaryViewModel
    | BrokerSummaryViewModelLoaded of BrokerSummaryViewModel
    | BrokerConfigurationDetailViewModelLoaded of BrokerConfigurationDetailViewModel
    | BrokerRollingRestartMsg of BrokerRollingRestartMsg
    | BrokerEditMsg of BrokerEditMsg
