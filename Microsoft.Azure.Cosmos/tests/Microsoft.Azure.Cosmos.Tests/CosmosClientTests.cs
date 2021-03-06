﻿//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CosmosClientTests
    {
        public const string AccountEndpoint = "https://localhost:8081/";
        public const string ConnectionString = "AccountEndpoint=https://localtestcosmos.documents.azure.com:443/;AccountKey=425Mcv8CXQqzRNCgFNjIhT424GK99CKJvASowTnq15Vt8LeahXTcN5wt3342vQ==;";

        [TestMethod]
        public async Task TestDispose()
        {
            CosmosClient cosmosClient = new CosmosClient(ConnectionString);
            Database database = cosmosClient.GetDatabase("asdf");
            Container container = cosmosClient.GetContainer("asdf", "asdf");
            TransactionalBatch batch = container.CreateTransactionalBatch(new PartitionKey("asdf"));
            batch.ReadItem("Test");

            // Dispose should be idempotent 
            cosmosClient.Dispose();
            cosmosClient.Dispose();

            List<Func<Task>> validateAsync = new List<Func<Task>>()
            {
                () => cosmosClient.ReadAccountAsync(),
                () => cosmosClient.CreateDatabaseAsync("asdf"),
                () => database.CreateContainerAsync("asdf", "/pkpathasdf", 200),
                () => container.ReadItemAsync<dynamic>("asdf", new PartitionKey("test")),
                () => container.Scripts.ReadStoredProcedureAsync("asdf"),
                () => container.Scripts.ReadTriggerAsync("asdf"),
                () => container.Scripts.ReadUserDefinedFunctionAsync("asdf"),
                () => batch.ExecuteAsync(),
                () => container.GetItemQueryIterator<dynamic>(queryText: "select * from T").ReadNextAsync(),
                () => container.GetItemQueryIterator<dynamic>().ReadNextAsync(),
            };

            foreach (Func<Task> asyncFunc in validateAsync)
            {
                try
                {
                   await asyncFunc();
                    Assert.Fail("Should throw ObjectDisposedException");
                }
                catch (ObjectDisposedException) { }
            }
        }
    }
}
