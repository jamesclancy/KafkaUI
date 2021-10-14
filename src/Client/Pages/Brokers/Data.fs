module Client.Pages.Brokers.Data

open Client.Models
open Client.Pages.Brokers.Models
open Microsoft.FSharp.Control
open Shared.Contracts

let fetchSummary (api: IBrokersApi) unit =
    async {

        let! metaData = api.fetchSummary ()
        return BrokerSummaryViewModel.FromBrokersSummary metaData
    }

let fetchConfigurationForBroker (api: IBrokersApi) brokerId =
    async {
        let! configuration = api.fetchConfigurationForBroker brokerId
        return BrokerConfigurationDetailViewModel.FromBrokerDetails configuration
    }

let saveConfigurationPropertyForBrokerAndReturnRefresh
    (api: IBrokersApi)
    (newValue: BrokerConfigurationEditPropertyViewModel)
    =
    async {
        let! brokerId =
            api.saveConfigurationPropertyForBrokerAndReturnRefresh (
                newValue.BrokerId,
                newValue.ToBrokerConfigurationProperty
            )

        return ({ BrokerId = brokerId })
    }

let startRollingRestartforCluster (api: IBrokersApi) config =
    async {
        do! api.tryStartRollingRestartforCluster config
        return LoadBrokerSummaryViewModel |> BrokerMsg
    }
