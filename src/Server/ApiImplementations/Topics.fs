module ApiImplementation.Topics

open Shared.Contracts
open Shared.Dtos
open Confluent.Kafka
open System.Linq
open System.Collections.Generic
open Confluent.Kafka.Admin


let adminClient = ClientBuilder.getAdminClient
let consumerClient = ClientBuilder.getConsumerClient
let producerClient = ClientBuilder.getProducerClient

let topicsApiImplementation: ITopicsApi =
    { fetchSummary = fun () -> ClientBuilder.getTopicSummaries
      fetchDetailsForTopic = fun topicName -> ClientBuilder.getTopicDetails topicName
      fetchDefaultsForCreatingTopic =
          fun () ->
              async {
                  return
                      { TopicName = (System.Guid.NewGuid().ToString())
                        Partitions = 3
                        ReplicationFactor = 1
                        CleanupPolicyUseRetentionPolicy = false
                        CleanUpPolicyUseCompaction = false
                        ConfigurationProperties = List.Empty }
              }
      tryToCreateTopic =
          fun (req: CreateTopicRequest) ->
              async {
                  let configDict =
                      req.ConfigurationProperties.ToDictionary((fun x -> x.Name), (fun x -> x.Value))

                  let topicSpec =
                      (new Admin.TopicSpecification(
                          Name = req.TopicName,
                          NumPartitions = req.Partitions,
                          ReplicationFactor = System.Convert.ToInt16(req.ReplicationFactor),
                          Configs = configDict
                      ))

                  do!
                      adminClient.CreateTopicsAsync([ topicSpec ])
                      |> Async.AwaitTask

                  return req.TopicName
              }
      tryToUpdatePartitions =
          fun ((x: string), (y: int)) ->
              async {
                  let partitionSpecification =
                      new PartitionsSpecification(Topic = x, IncreaseTo = y)

                  do!
                      adminClient.CreatePartitionsAsync([ partitionSpecification ])
                      |> Async.AwaitTask

                  return x
              }
      tryToPurgeTopic =
          function
          | (s: string) ->
              async {
                  do! ClientBuilder.purgeTopic s
                  return s
              }
      tryToUpdateTopicConfigurationProperty =
          fun (topicName, propertyName, newValue) ->
              async {
                  do! ClientBuilder.updateTopicConfigurationProperty topicName propertyName newValue
                  return topicName
              } }
