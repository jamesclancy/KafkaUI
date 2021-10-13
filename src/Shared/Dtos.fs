module Shared.Dtos

(* Broker Related *)


type BrokerSummary =
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


type BrokerRollingRestartRequest =
    { UseSudo: bool
      StopCommand: string
      StartCommand: string
      HealthCheckIntervalInSeconds: int
      StableIterationsCount: int }

type BrokersSummary =
    { BrokerCount: int
      ClusterId: string
      Controller: bool
      Version: string
      SimilarConfiguration: bool

      AvailableBrokers: BrokerSummary list }

type BrokerConfigurationProperty =
    { Name: string
      Value: string
      Locked: bool
      Description: string option
      KafkaDefaultValue: string option }

type BrokerDetails =
    { BrokerId: string
      Hostname: string
      Port: string
      Partitions: int
      Relicas: int
      UnderReplicatedPartitions: int
      HideReadOnly: bool
      ShowOverridesOnly: bool
      Properties: BrokerConfigurationProperty list }


(* Topics Related *)

type TopicConfigurationProperty =
    { Name: string
      Value: string
      Description: string option
      KafkaDefaultValue: string option
      BrokerDefaultValue: string option
      Locked: bool }

type TopicSummary =
    { Name: string
      Partitions: int
      UnderReplicatedPartitions: int
      Count: int64
      SizeInBytes: int
      Consumers: int }

type TopicsSummary =
    { TopicCount: int
      Partitions: int
      UnderReplicatedPartitions: int
      PartitionsWithoutLeader: int
      InSyncReplicaIssues: int

      AvailableTopics: TopicSummary list }

type TopicConsumerGroupSummary =
    { GroupId: string
      State: string //https://jaceklaskowski.gitbooks.io/apache-kafka/content/kafka-coordinator-group-GroupMetadata.html
      Lag: int } // Consumer group lag is the difference between the last produced message (the latest message available) and the last committed message (the last processed or read message) of a partition

type TopicPartitionSummary =
    { Partition: int
      Leader: int
      OffsetMin: int
      OffsetMax: int
      SizeInBytes: int
      InSyncReplicas: int
      Replicas: int }

type TopicBrokerSummary =
    { BrokerId: string
      PartitionsAsLeader: int list
      Partitions: int list }


type TopicDetails =
    { TopicName: string
      EventCount: int64
      PartitionCount: int
      PartitionsWithoutLeaderCount: int
      ReplicationFactor: int
      InSyncReplicaIssueCount: int

      IsCompacted: bool

      ConsumerGroups: TopicConsumerGroupSummary list
      Partitions: TopicPartitionSummary list
      Brokers: TopicBrokerSummary list
      Configuration: TopicConfigurationProperty list }

type CreateTopicRequest =
    { TopicName: string
      Partitions: int
      ReplicationFactor: int
      CleanupPolicyUseRetentionPolicy: bool
      CleanUpPolicyUseCompaction: bool
      ConfigurationProperties: TopicConfigurationProperty list }


(* Consumer Group Related *)

type ConsumerGroupSummaryConsumerGroupSummary =
    { ConsumerGroupName: string
      State: string
      Lag: int
      Members: int
      AssignedPartitions: int
      AssignedTopics: int
      PartitionAssignmentStrategy: string } //https://medium.com/streamthoughts/understanding-kafka-partition-assignment-strategies-and-how-to-write-your-own-custom-assignor-ebeda1fc06f3

type ConsumerGroupSummary =
    { ConsumerCount: int
      ConsumerStates: Map<string, int>

      ConsumerGroups: ConsumerGroupSummaryConsumerGroupSummary list }

type ConsumerGroupDetailMember =
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

type ConsumerGroupDetail =
    { ConsumerGroupName: string
      State: string
      Lag: int
      MemberCount: int
      AssignedPartitions: int
      AssignedTopics: int
      PartitionAssignmentStrategy: string //https://medium.com/streamthoughts/understanding-kafka-partition-assignment-strategies-and-how-to-write-your-own-custom-assignor-ebeda1fc06f3
      Members: ConsumerGroupDetailMember list }
