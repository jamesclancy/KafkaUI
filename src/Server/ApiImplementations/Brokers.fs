module ApiImplementation.Brokers

open Shared.Contracts
open Shared.Dtos
open Confluent.Kafka
open System.Linq
open System.Collections.Generic
open Confluent.Kafka.Admin


let adminClient = ClientBuilder.getAdminClient
let consumerClient = ClientBuilder.getConsumerClient
let producerClient = ClientBuilder.getProducerClient


let brokersApiImplementation: IBrokersApi =
    { fetchSummary = fun () -> ClientBuilder.getInformationForAllBrokers
      fetchConfigurationForBroker = fun brokerId -> ClientBuilder.getDetailForABrokers brokerId
      saveConfigurationPropertyForBrokerAndReturnRefresh =
          fun (x, y) ->
              async {
                  do! ClientBuilder.updateBrokerConfigurationProperty x y.Name y.Value
                  return x
              }
      tryStartRollingRestartforCluster =
          fun (x) ->
              async {

                  // ToDo: Not certain how to do this
                  return ()
              }

    }
