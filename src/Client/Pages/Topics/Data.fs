module Client.Pages.Topics.Data

open Client.Pages.Topics.Models
open Client.Models
open Shared.Contracts
open Shared.Dtos

let fetchSummary (api: ITopicsApi) unit =
    async {
        let! results = api.fetchSummary ()
        return TopicsSummaryViewModel.FromTopicsSummary results
    }

let fetchDefaultCreateTopicModel (api: ITopicsApi) unit =
    async {
        let! results = api.fetchDefaultsForCreatingTopic ()
        return CreateTopicViewModel.FromCreateTopicRequest results
    }

let fetchDetailsForTopic (api: ITopicsApi) topicName =
    async {

        let! results = api.fetchDetailsForTopic topicName
        return ManageTopicViewModel.FromTopicDetails results
    }

let tryToCreateTopic (api: ITopicsApi) (config: CreateTopicViewModel) =
    async {
        let! topicName = api.tryToCreateTopic config.AsCreateTopicRequest

        return
            topicName
            |> LoadManageTopicViewModel
            |> ManageTopicMsg
            |> TopicMsg
    }

let tryToUpdatePartitions (api: ITopicsApi) (vm: AddPartitionDialogViewModel) =
    async {
        let! topicName = api.tryToUpdatePartitions (vm.TopicName, vm.NewValue)

        return
            topicName
            |> LoadManageTopicViewModel
            |> ManageTopicMsg
            |> TopicMsg
    }

let tryToPurgeTopic (api: ITopicsApi) topicName =
    async {
        let! topicName = api.tryToPurgeTopic topicName

        return
            topicName
            |> LoadManageTopicViewModel
            |> ManageTopicMsg
            |> TopicMsg
    }

let tryToUpdateConfiguration (api: ITopicsApi) topicName (newConfigurationPropertyValue: TopicConfigurationProperty) =
    async {
        let! topicName =
            api.tryToUpdateTopicConfigurationProperty (
                topicName,
                newConfigurationPropertyValue.Name,
                newConfigurationPropertyValue.Value
            )

        return
            topicName
            |> LoadManageTopicViewModel
            |> ManageTopicMsg
            |> TopicMsg
    }
