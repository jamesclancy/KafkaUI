module ClientBuilder

open Confluent.Kafka
open Confluent.Kafka.Admin
open Shared.Dtos
open System.Linq
open FSharp.Control
open System.Collections.Generic

let extractConfiguartionProperty (properyList: ConfigEntryResult seq) key =
    properyList
        .Where(fun p -> p.Name = key)
        .First()
        .Value


type ConsumerGroupTopicPartitionMetadataHolder =
    { ConsumerGroup: GroupInfo
      TopicMetadata: TopicMetadata
      TopicPartition: TopicPartition
      TopicPartitionOffset: TopicPartitionOffset
      WatermarkOffsets: WatermarkOffsets }

type ConsumerGroupTopicMetadataHolder =
    { ConsumerGroup: GroupInfo
      TopicMetadata: TopicMetadata
      ConsumerGroupTopicPartitionMetadataHolders: ConsumerGroupTopicPartitionMetadataHolder list }

type TopicMetadataHolder =
    { TopicMetadata: TopicMetadata
      TopicPartitionOffsets: WatermarkOffsets list
      ConsumerGroups: ConsumerGroupTopicMetadataHolder list
      TopicConfiguration: ConfigEntryResult list }
    member this.EventCount =
        this.TopicPartitionOffsets
        |> Seq.filter (fun x -> x.High.IsSpecial = false)
        |> Seq.map (fun x -> x.High.Value - x.Low.Value)
        |> Seq.sum

    member this.ConsumerCount =
        this.ConsumerGroups
        |> Seq.map (fun x -> x.ConsumerGroup.Members.Count)
        |> Seq.sum

    member this.Name = this.TopicMetadata.Topic
    member this.Partitions = this.TopicMetadata.Partitions
    member this.PartitionCount = this.Partitions.Count

    member this.UnderReplicatedPartitions =
        (this.TopicMetadata.Partitions
         |> Seq.filter (fun x -> x.Replicas.Length < x.InSyncReplicas.Length)
         |> List.ofSeq)

    member this.UnderReplicatedPartitionCount = (this.UnderReplicatedPartitions.Count())

    member this.PartitionsWithoutLeader =
        this.TopicMetadata.Partitions
        |> Seq.filter (fun x -> x.Leader < 0)
        |> List.ofSeq

    member this.PartitionWithoutLeaderCount = this.PartitionsWithoutLeader.Count()

    member this.IsCompacted =
        (extractConfiguartionProperty this.TopicConfiguration "cleanup.policy") = "compact"

    member this.MinInSyncReplicas =
        System.Convert.ToInt32(extractConfiguartionProperty this.TopicConfiguration "min.insync.replicas")

    member this.PartitionsUnderMinInSyncReplicaValue =
        this.TopicMetadata.Partitions
        |> Seq.filter (fun x -> x.InSyncReplicas.Length < this.MinInSyncReplicas)
        |> List.ofSeq

    member this.PartitionUnderMinInSyncReplicaValueCount =
        this.PartitionsUnderMinInSyncReplicaValue.Count()

    member this.TopicConfigurationProperties =
        this.TopicConfiguration.Select
            (fun x ->
                { Name = x.Name
                  Value = x.Value
                  Description = None
                  KafkaDefaultValue = None
                  BrokerDefaultValue = None
                  Locked = x.IsReadOnly })

    member this.TopicConsumerGroupSummaries =
        this.ConsumerGroups.Select
            (fun x ->
                { GroupId = x.ConsumerGroup.Group
                  State = x.ConsumerGroup.State
                  Lag = 0 // ToDo
                })

    member this.TopicPartitionSummaries =
        this.Partitions.Select
            (fun x ->
                { Partition = x.PartitionId
                  Leader = x.Leader
                  OffsetMin = 0 // ToDo
                  OffsetMax = 0 // ToDo
                  SizeInBytes = 0 // ToDo
                  InSyncReplicas = x.InSyncReplicas.Count()
                  Replicas = x.Replicas.Count() })

    member this.TopicBrokerSummaries =
        this.ConsumerGroups.Select
            (fun x ->
                { GroupId = x.ConsumerGroup.Group
                  State = x.ConsumerGroup.State
                  Lag = 0 // ToDo
                })


let loadConfiguration =
    let conf = (new AdminClientConfig())
    conf.BootstrapServers <- "localhost:29092"
    conf


let buildAdminClient (config: ClientConfig) =
    let admin = (new AdminClientBuilder(config)).Build()
    admin

let getAdminClient = buildAdminClient loadConfiguration

let defaultTimeout = System.TimeSpan.FromSeconds(15.0)


let getConsumerClient groupId =
    (new ConsumerBuilder<Ignore, string>(new ConsumerConfig(BootstrapServers = "localhost:29092", GroupId = groupId)))
        .Build()

let getProducerClient =
    (new ProducerBuilder<Null, string>(new ProducerConfig(BootstrapServers = "localhost:29092")))
        .Build()



let adminClient = getAdminClient

let consumerClient = getConsumerClient
let producerClient = getProducerClient



let private updateKafkaConfigurationProperty configResource propertyName newValue =
    async {
        let! describe =
            adminClient.DescribeConfigsAsync([ configResource ])
            |> Async.AwaitTask

        let newConfig =
            describe
            |> Seq.collect (fun (x: DescribeConfigsResult) -> x.Entries)
            |> Seq.map
                (fun (x: KeyValuePair<string, ConfigEntryResult>) ->
                    if (propertyName = x.Value.Name) then
                        new ConfigEntry(Name = propertyName, Value = newValue)
                    else
                        new ConfigEntry(Name = x.Value.Name, Value = x.Value.Value))

        // ToDo:This will loose all sensiotve properites...

        let configurationRequest =
            [ newConfig.ToList() ]
                .ToDictionary(fun x -> configResource)

        let! results =
            adminClient.AlterConfigsAsync(configurationRequest, new AlterConfigsOptions())
            |> Async.AwaitTask

        results |> ignore

        ()
    }



let configResourceForTopicName topicName =
    (new ConfigResource(Name = topicName, Type = ResourceType.Topic))

let configResourceForBrokerId brokerId =
    (new ConfigResource(Name = brokerId, Type = ResourceType.Broker))

let configResourceForConsumerGroupName consumerGroupName =
    (new ConfigResource(Name = consumerGroupName, Type = ResourceType.Group))

(* Topics *)

let topicSummariesFromMetadata (topicMetadata: TopicMetadataHolder) : TopicSummary =

    { Name = topicMetadata.Name
      Partitions = topicMetadata.PartitionCount
      UnderReplicatedPartitions = topicMetadata.UnderReplicatedPartitionCount
      Count = topicMetadata.EventCount
      SizeInBytes = 0
      Consumers = topicMetadata.ConsumerCount }


let getConsumersGroupsInfoForTopic (topicMetadata: TopicMetadata) (groupMetadata: GroupInfo) =
    seq {
        let consumer = (getConsumerClient groupMetadata.Group)

        let topicPartitions =
            topicMetadata.Partitions
            |> Seq.map (fun x -> new TopicPartition(topicMetadata.Topic, new Partition(x.PartitionId)))

        topicPartitions |> consumer.Assign

        let topicParisionTimestamps =
            topicPartitions
            |> Seq.map (fun x -> new TopicPartitionTimestamp(x, new Timestamp(System.DateTime.MinValue)))

        let offsets =
            consumer.OffsetsForTimes(topicParisionTimestamps, defaultTimeout)

        for lag in offsets do
            if lag.Offset.Value > -1L then
                let partitionInfo =
                    consumer.GetWatermarkOffsets(lag.TopicPartition) //, defaultTimeout) // ToDo:    This doesn't work (it only looks at caches and is a massive n+1 problem.
                //          I am not certain hwo to implement this over the existing apis
                //          maybe thsio could use OffsetsForTimes ?

                yield
                    { ConsumerGroup = groupMetadata
                      TopicMetadata = topicMetadata
                      TopicPartition = lag.TopicPartition
                      TopicPartitionOffset = lag
                      WatermarkOffsets = partitionInfo }
    }

    |> Seq.groupBy (fun x -> (x.ConsumerGroup, x.TopicMetadata))
    |> Seq.map
        (fun ((cg, tm), y) ->
            { ConsumerGroup = cg
              TopicMetadata = tm
              ConsumerGroupTopicPartitionMetadataHolders = y |> List.ofSeq })

let private getTopicInformationForTopic (groups: GroupInfo seq) (topicMetadata: TopicMetadata) =
    async {
        let describeConfigsOptions =
            configResourceForTopicName topicMetadata.Topic

        let! topicDetails =
            adminClient.DescribeConfigsAsync([ describeConfigsOptions ])
            |> Async.AwaitTask

        let consumer =
            (getConsumerClient "getTopicInformationForTopic")

        let topicPartitions =
            topicMetadata.Partitions
            |> Seq.map (fun x -> new TopicPartition(topicMetadata.Topic, new Partition(x.PartitionId)))

        topicPartitions |> consumer.Assign

        let res =
            consumer.Consume(System.TimeSpan.FromMilliseconds(10.0))

        let offsets =
            topicPartitions
            |> Seq.map consumer.GetWatermarkOffsets
            |> Seq.toList

        return
            { TopicMetadata = topicMetadata
              TopicPartitionOffsets = offsets
              ConsumerGroups =
                  groups.SelectMany(fun x -> getConsumersGroupsInfoForTopic topicMetadata x)
                  |> List.ofSeq
              TopicConfiguration =
                  topicDetails
                      .Select(fun x -> x.Entries)
                      .SelectMany(fun x -> x.Select(fun y -> y.Value))
                  |> List.ofSeq }
    }

let getTopicInformationForClusterMetadata (clusterMetadata: Metadata) =
    asyncSeq {
        let topics = clusterMetadata.Topics
        let groups = adminClient.ListGroups(defaultTimeout)


        let results =
            topics
            |> AsyncSeq.ofSeq
            |> AsyncSeq.mapAsyncParallel (getTopicInformationForTopic groups)

        yield! results
    }

let getInformationForAllTopics =
    asyncSeq {
        let clusterMetadata = adminClient.GetMetadata(defaultTimeout)
        yield! getTopicInformationForClusterMetadata clusterMetadata
    }

let getInformationForATopics topicName =
    async {
        let clusterMetadata =
            adminClient.GetMetadata(topicName, defaultTimeout)

        let! listOfResults =
            (getTopicInformationForClusterMetadata clusterMetadata
             |> AsyncSeq.toListAsync)

        return listOfResults.Head
    }

let purgeTopic topicName =
    async {
        let topicInformation =
            adminClient
                .GetMetadata(
                    topicName,
                    defaultTimeout
                )
                .Topics
            |> Seq.collect (fun x -> x.Partitions)
            |> Seq.map (fun x -> new TopicPartitionOffset(topicName, new Partition(x.PartitionId), Offset.End))

        let! results =
            adminClient.DeleteRecordsAsync(topicInformation)
            |> Async.AwaitTask

        results |> ignore
        ()
    }

let updateTopicConfigurationProperty topicName propertyName newValue =
    async {
        let configResource = configResourceForTopicName topicName

        return! updateKafkaConfigurationProperty configResource propertyName newValue
    }


let getTopicDetails topicName =
    async {

        let! topicMetadata = getInformationForATopics topicName

        return
            { TopicName = topicName
              EventCount = topicMetadata.EventCount
              PartitionCount = topicMetadata.PartitionCount
              PartitionsWithoutLeaderCount = topicMetadata.PartitionWithoutLeaderCount
              ReplicationFactor = topicMetadata.MinInSyncReplicas
              InSyncReplicaIssueCount = topicMetadata.PartitionUnderMinInSyncReplicaValueCount
              IsCompacted = topicMetadata.IsCompacted

              ConsumerGroups =
                  topicMetadata.TopicConsumerGroupSummaries
                  |> List.ofSeq
              Partitions =
                  topicMetadata.TopicPartitionSummaries
                  |> List.ofSeq
              Brokers = List.empty // topicMetadata.TopicBrokerSummaries |> List.ofSeq ToDo
              Configuration =
                  topicMetadata.TopicConfigurationProperties
                  |> List.ofSeq }
    }

let getTopicSummaries: Async<TopicsSummary> =
    async {
        let! topicMetadata = getInformationForAllTopics |> AsyncSeq.toListAsync

        return
            { TopicCount = topicMetadata.Count()
              Partitions =
                  topicMetadata
                  |> Seq.fold (fun y x -> y + x.PartitionCount) 0
              UnderReplicatedPartitions =
                  topicMetadata
                  |> Seq.fold (fun y x -> y + x.PartitionUnderMinInSyncReplicaValueCount) 0
              PartitionsWithoutLeader =
                  topicMetadata
                  |> Seq.fold (fun y x -> y + x.PartitionWithoutLeaderCount) 0
              InSyncReplicaIssues = topicMetadata.Sum(fun x -> x.PartitionUnderMinInSyncReplicaValueCount)
              AvailableTopics =
                  topicMetadata
                  |> Seq.map topicSummariesFromMetadata
                  |> List.ofSeq }
    }


(* Brokers *)

let brokerSummaryFromBrokerMetadata (brokerMetadata: BrokerMetadata) : BrokerSummary =
    { Id = brokerMetadata.BrokerId.ToString()
      Rack = "Need from zookeeper I think"
      Listener = (sprintf "%s:%i" brokerMetadata.Host brokerMetadata.Port)
      Paritions = 0 // ToDo
      PartitionSkew = 0 // ToDo
      Leaders = 0 // ToDo
      LeaderSkew = 0 // ToDo
      Replicas = 0 // ToDo
      UnderReplicatedPartitions = 0 // ToDo
      SizeInBytes = 0 }


let getInformationForAllBrokers =
    async {
        let clusterMetadata = adminClient.GetMetadata(defaultTimeout)

        let brokers = clusterMetadata.Brokers

        return
            { BrokerCount = brokers.Count
              ClusterId = "ToDo: need to get from Zookeeper"
              Controller = false // ToDo
              Version = "ToDo: need to get from Zookeeper"
              SimilarConfiguration = false // ToDo

              AvailableBrokers =
                  brokers
                  |> Seq.map brokerSummaryFromBrokerMetadata
                  |> List.ofSeq }
    }

let getDetailForABrokers brokerId =
    async {

        let describeConfigsOptions = configResourceForBrokerId brokerId

        let clusterMetadata = adminClient.GetMetadata(defaultTimeout)

        let brokers = clusterMetadata.Brokers

        let brokerOfInterest =
            brokers
            |> Seq.find (fun x -> x.BrokerId.ToString() = brokerId)

        let! topicDetails =
            adminClient.DescribeConfigsAsync([ describeConfigsOptions ])
            |> Async.AwaitTask

        let properties =
            topicDetails
                .Select(fun x -> x.Entries)
                .SelectMany(fun x -> x.Select(fun y -> y.Value))
                .Select(fun x ->
                    { Name = x.Name
                      Value = x.Value
                      Description = None // ToDo
                      KafkaDefaultValue = None // ToDo
                      Locked = x.IsReadOnly })

        return
            { BrokerId = brokerOfInterest.BrokerId.ToString()
              Hostname = brokerOfInterest.Host
              Port = brokerOfInterest.Port.ToString()
              Partitions = 0 // ToDo
              Relicas = 0 // ToDo
              UnderReplicatedPartitions = 0 // ToDo
              HideReadOnly = false // ToDo
              ShowOverridesOnly = false // ToDo
              Properties = properties |> List.ofSeq }
    }


let updateBrokerConfigurationProperty brokerId propertyName newValue =
    async {
        let configResource = configResourceForBrokerId brokerId

        do! updateKafkaConfigurationProperty configResource propertyName newValue
    }



(* Consumers *)

let groupInfoToConsumerGroupSummaryConsumerGroupSummary (groupInfo: GroupInfo) =
    { ConsumerGroupName = groupInfo.Group
      State = groupInfo.State
      Lag = 1 // ToDo
      Members = groupInfo.Members.Count // ToDo
      AssignedPartitions = 1 // ToDo
      AssignedTopics = 1 // ToDo
      PartitionAssignmentStrategy = "RangeAssignor" // ToDo
    }

let getInformationForAllConsumerGroups =
    async {
        let groups =
            getAdminClient.ListGroups(defaultTimeout)

        let reformatedGroups =
            groups
            |> Seq.map groupInfoToConsumerGroupSummaryConsumerGroupSummary
            |> List.ofSeq

        let consumerStates =
            reformatedGroups
            |> Seq.groupBy (fun x -> x.State)
            |> Seq.map (fun (key, value) -> (key, value.Count()))
            |> Map.ofSeq

        return
            { ConsumerCount = groups.Count
              ConsumerStates = consumerStates
              ConsumerGroups = reformatedGroups }
    }

let getConsumerGroupDetails consumerGroupName =
    async {

        let groupInfo =
            getAdminClient.ListGroup(consumerGroupName, defaultTimeout)

        let groupMembers =
            groupInfo.Members
            |> Seq.map
                (fun x ->
                    System.Console.WriteLine("writing out member metadata...")

                    System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(x.MemberMetadata))
                    |> ignore

                    System.Console.WriteLine(System.Text.Encoding.UTF8.GetString(x.MemberAssignment))
                    |> ignore

                    { ConsumerGroupName = consumerGroupName
                      MemberType = "ToDo"
                      MemberId = x.MemberId
                      ClientId = x.ClientId
                      HostName = x.ClientHost
                      Lag = 1 // ToDo
                      AssignedPartitions = 1 // ToDo
                      AssignedTopic = "ToDo" // ToDo
                      CursorLocation = 1 // ToDo
                      CusorEnd = 1 // ToDo
                    })
            |> List.ofSeq

        return
            { ConsumerGroupName = groupInfo.Group
              State = groupInfo.State // ToDo
              Lag = 1 // ToDo
              MemberCount = groupInfo.Members.Count // ToDo
              AssignedPartitions = 1 // ToDo
              AssignedTopics = 1 // ToDo
              PartitionAssignmentStrategy = "RangeAssignor" // ToDo
              Members = groupMembers }
    }

let deleteCustomerGroup consumerGroupName = async { failwith "ToDo This" }
