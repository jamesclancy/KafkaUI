module Client.Pages.Consumers.Data

open Client.Pages.Consumers.Models
open Client.Models
open Shared.Contracts


let private generateSampleGroups =
    [ 0 .. 1000 ]
    |> List.map
        (fun x ->
            { ConsumerGroupName = System.Guid.NewGuid().ToString()
              State = "Stable"
              Lag = 1000 - x
              Members = x
              AssignedPartitions = x
              AssignedTopics = x
              PartitionAssignmentStrategy = "RangeAssignor" })


let private generateSampleMembers consumerGroupName =
    [ 0 .. 1000 ]
    |> List.map
        (fun x ->
            { ConsumerGroupName = consumerGroupName
              MemberType = "idk what this is"
              MemberId = System.Guid.NewGuid().ToString()
              ClientId = System.Guid.NewGuid().ToString()
              HostName = System.Guid.NewGuid().ToString()
              Lag = x
              AssignedPartitions = x
              AssignedTopic = System.Guid.NewGuid().ToString()
              CursorLocation = x
              CusorEnd = x })


let fetchSummary (api: IConsumersApi) unit =
    async {
        let! apiResult = api.fetchSummary ()
        return apiResult
    }

let fetchManageConsumerGroupDetails (api: IConsumersApi) consumerGroupName =
    async {
        let! apiResult = api.fetchManageConsumerGroupDetails consumerGroupName
        return apiResult
    }

let tryToDeleteCustomerGroup (api: IConsumersApi) consumerGroupName =
    async {
        do! api.tryToDeleteCustomerGroup consumerGroupName

        return
            consumerGroupName
            |> LoadManageConsumerGroupViewModel
            |> ManageConsumerGroupMsg
            |> ConsumerMsg
    }
