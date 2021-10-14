module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Shared.Contracts
open Shared.Dtos
open ApiImplementation.Topics
open ApiImplementation.Brokers
open ApiImplementation.Consumers
open Giraffe

let buildApi () =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder

let topicApi =
    buildApi ()
    |> Remoting.fromValue topicsApiImplementation
    |> Remoting.buildHttpHandler

let brokerApi =
    buildApi ()
    |> Remoting.fromValue brokersApiImplementation
    |> Remoting.buildHttpHandler

let consumerApi =
    buildApi ()
    |> Remoting.fromValue consumerApiImplementation
    |> Remoting.buildHttpHandler


let completeApi =
    choose [ topicApi
             brokerApi
             consumerApi ]


let app =
    application {
        url "http://0.0.0.0:8085"
        use_router completeApi
        memory_cache
        use_static "public"
        use_gzip
    }

run app
