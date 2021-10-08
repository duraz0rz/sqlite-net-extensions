using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace SQLiteNetExtensionsIntegrationTests.Tests.Async
{
    [TestFixture]
    public class DeleteAsyncTests : BaseAsyncTest
    {
        private class DummyClassGuidPk
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        private class DummyClassIntPk
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        [SetUp]
        public async Task SetUp()
        {
            await Connection.CreateTableAsync<DummyClassGuidPk>();
            await Connection.CreateTableAsync<DummyClassIntPk>();
        }

        [Test]
        public async Task TestDeleteAllGuidPk()
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAll extension method
            var elementA = new DummyClassGuidPk
            {
                Id = Guid.NewGuid(),
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassGuidPk
            {
                Id = Guid.NewGuid(),
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassGuidPk
            {
                Id = Guid.NewGuid(),
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassGuidPk> { elementA, elementB, elementC };
            await Connection.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            Assert.AreEqual(elementsList.Count, await Connection.Table<DummyClassGuidPk>().CountAsync());

            var elementsToDelete = new List<DummyClassGuidPk> { elementA, elementC };

            // Delete elements from the database
            await Connection.DeleteAllAsync(elementsToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, await Connection.Table<DummyClassGuidPk>().CountAsync());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(await Connection.FindAsync<DummyClassGuidPk>(deletedElement.Id));
            }
        }

        [Test]
        public async Task TestDeleteAllIntPk()
        {
            // In this test we will create three elements in the database and delete
            // two of them using DeleteAll extension method
            var elementA = new DummyClassIntPk
            {
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassIntPk
            {
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassIntPk
            {
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassIntPk> { elementA, elementB, elementC };
            await Connection.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            Assert.AreEqual(elementsList.Count, await Connection.Table<DummyClassIntPk>().CountAsync());

            var elementsToDelete = new List<DummyClassIntPk> { elementA, elementC };

            // Delete elements from the database
            await Connection.DeleteAllAsync(elementsToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, await Connection.Table<DummyClassIntPk>().CountAsync());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(await Connection.FindAsync<DummyClassIntPk>(deletedElement.Id));
            }
        }

        [Test]
        public async Task TestDeleteAllThousandObjects()
        {
            // In this test we will create thousands of elements in the database all but one with the DeleteAll method
            var elementsList = Enumerable.Range(0, 10000).Select(i =>
                new DummyClassIntPk
                {
                    Foo = "Foo " + i,
                    Bar = "Bar " + i
                }
            ).ToList();

            await Connection.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            Assert.AreEqual(elementsList.Count, await Connection.Table<DummyClassIntPk>().CountAsync());

            var elementsToDelete = new List<DummyClassIntPk>(elementsList);
            elementsToDelete.RemoveAt(0);

            // Delete elements from the database
            await Connection.DeleteAllAsync(elementsToDelete, true);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, await Connection.Table<DummyClassIntPk>().CountAsync());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(await Connection.FindAsync<DummyClassIntPk>(deletedElement.Id));
            }
        }

        [Test]
        public async Task TestDeleteAllIdsGuidPk()
        {
            // In this test we will create three elements in the database and delete two of them using DeleteAllIds extension method
            var elementA = new DummyClassGuidPk
            {
                Id = Guid.NewGuid(),
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassGuidPk
            {
                Id = Guid.NewGuid(),
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassGuidPk
            {
                Id = Guid.NewGuid(),
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassGuidPk> { elementA, elementB, elementC };
            await Connection.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            Assert.AreEqual(elementsList.Count, await Connection.Table<DummyClassGuidPk>().CountAsync());

            var elementsToDelete = new List<DummyClassGuidPk> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            await Connection.DeleteAllIdsAsync<DummyClassGuidPk>(primaryKeysToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, await Connection.Table<DummyClassGuidPk>().CountAsync());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(await Connection.FindAsync<DummyClassGuidPk>(deletedElement.Id));
            }
        }

        [Test]
        public async Task TestDeleteAllIdsIntPk()
        {
            // In this test we will create three elements in the database and delete two of them using DeleteAllIds extension method
            var elementA = new DummyClassIntPk
            {
                Foo = "Foo A",
                Bar = "Bar A"
            };

            var elementB = new DummyClassIntPk
            {
                Foo = "Foo B",
                Bar = "Bar B"
            };

            var elementC = new DummyClassIntPk
            {
                Foo = "Foo C",
                Bar = "Bar C"
            };

            var elementsList = new List<DummyClassIntPk> { elementA, elementB, elementC };
            await Connection.InsertAllAsync(elementsList);

            // Verify that the elements have been inserted correctly
            Assert.AreEqual(elementsList.Count, await Connection.Table<DummyClassIntPk>().CountAsync());

            var elementsToDelete = new List<DummyClassIntPk> { elementA, elementC };
            var primaryKeysToDelete = elementsToDelete.Select(e => (object)e.Id);

            // Delete elements from the database
            await Connection.DeleteAllIdsAsync<DummyClassIntPk>(primaryKeysToDelete);

            // Verify that the elements have been deleted correctly
            Assert.AreEqual(elementsList.Count - elementsToDelete.Count, await Connection.Table<DummyClassIntPk>().CountAsync());
            foreach (var deletedElement in elementsToDelete)
            {
                Assert.IsNull(await Connection.FindAsync<DummyClassIntPk>(deletedElement.Id));
            }
        }
    }
}

