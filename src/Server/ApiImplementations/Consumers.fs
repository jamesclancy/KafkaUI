module ApiImplementation.Consumers

open Shared.Contracts
open Shared.Dtos
open Confluent.Kafka
open System.Linq
open System.Collections.Generic
open Confluent.Kafka.Admin


let adminClient = ClientBuilder.getAdminClient
let consumerClient = ClientBuilder.getConsumerClient
let producerClient = ClientBuilder.getProducerClient

let consumerApiImplementation: IConsumersApi =
    { fetchSummary = fun () -> ClientBuilder.getInformationForAllConsumerGroups
      fetchManageConsumerGroupDetails = fun consumerGroupName -> ClientBuilder.getConsumerGroupDetails consumerGroupName
      tryToDeleteCustomerGroup = fun consumerGroupName -> ClientBuilder.deleteCustomerGroup consumerGroupName }
