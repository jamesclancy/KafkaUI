module Shared.Contracts

open Dtos

type IConsumersApi =
    { fetchSummary: unit -> Async<ConsumerGroupSummary>
      fetchManageConsumerGroupDetails: string -> Async<ConsumerGroupDetail>
      tryToDeleteCustomerGroup: string -> Async<unit> }

type ITopicsApi =
    { fetchSummary: unit -> Async<TopicsSummary>
      fetchDetailsForTopic: string -> Async<TopicDetails>
      fetchDefaultsForCreatingTopic: unit -> Async<CreateTopicRequest>
      tryToCreateTopic: CreateTopicRequest -> Async<string>
      tryToUpdatePartitions: string * int -> Async<string>
      tryToPurgeTopic: string -> Async<string>
      tryToUpdateTopicConfigurationProperty: string * string * string -> Async<string> }

type IBrokersApi =
    { fetchSummary: unit -> Async<BrokersSummary>
      fetchConfigurationForBroker: string -> Async<BrokerDetails>
      saveConfigurationPropertyForBrokerAndReturnRefresh: string * BrokerConfigurationProperty -> Async<string>
      tryStartRollingRestartforCluster: BrokerRollingRestartRequest -> Async<unit> }
