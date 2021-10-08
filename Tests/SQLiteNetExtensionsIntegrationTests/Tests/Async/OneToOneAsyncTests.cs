using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensionsAsync.Extensions;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace SQLiteNetExtensionsIntegrationTests.Tests.Async
{
    [TestFixture]
    public class OneToOneAsyncTests : BaseAsyncTest
    {
        private class O2OClassA
        {
            [PrimaryKey, AutoIncrement] public int Id { get; set; }

            [ForeignKey(typeof(O2OClassB))] // Explicit foreign key attribute
            public int OneClassBKey { get; set; }

            [OneToOne] public O2OClassB OneClassB { get; set; }
        }

        private class O2OClassB
        {
            [PrimaryKey, AutoIncrement] public int Id { get; set; }

            public string Foo { get; set; }
        }

        private class O2OClassC
        {
            [PrimaryKey, AutoIncrement] public int ClassId { get; set; }

            [OneToOne] // OneToOne Foreign key can be declared in the referenced class
            public O2OClassD ElementD { get; set; }

            public string Bar { get; set; }
        }

        private class O2OClassD
        {
            [PrimaryKey, AutoIncrement] public int Id { get; set; }

            [ForeignKey(typeof(O2OClassC))] // Explicit foreign key attribute for a inverse relationship
            public int ObjectCKey { get; set; }

            public string Foo { get; set; }
        }

        private class O2OClassE
        {
            [PrimaryKey, AutoIncrement] public int Id { get; set; }

            public int ObjectFKey { get; set; }

            [OneToOne("ObjectFKey")] // Explicit foreign key declaration
            public O2OClassF ObjectF { get; set; }

            public string Foo { get; set; }
        }

        private class O2OClassF
        {
            [PrimaryKey, AutoIncrement] public int Id { get; set; }

            [OneToOne] // Inverse relationship, doesn't need foreign key
            public O2OClassE ObjectE { get; set; }

            public string Bar { get; set; }
        }

        [SetUp]
        public async Task SetUp()
        {
            await Connection.CreateTableAsync<O2OClassA>();
            await Connection.CreateTableAsync<O2OClassB>();
            await Connection.CreateTableAsync<O2OClassC>();
            await Connection.CreateTableAsync<O2OClassD>();
            await Connection.CreateTableAsync<O2OClassE>();
            await Connection.CreateTableAsync<O2OClassF>();
        }

        [Test]
        public async Task TestGetOneToOneDirect()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectB = new O2OClassB { Foo = $"Foo String {new Random().Next(100)}" };
            var objectA = new O2OClassA();

            await Connection.InsertAllAsync(new object[] { objectB , objectA });

            objectA.OneClassB.Should().BeNull();

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectA);
            objectA.OneClassB.Should().BeNull();

            // Set the relationship using IDs
            objectA.OneClassBKey = objectB.Id;
            await Connection.UpdateAsync(objectA);

            objectA.OneClassB.Should().BeNull();

            // Fetch the relationship
            await Connection.GetChildrenAsync(objectA);

            objectA.OneClassB.Should().NotBeNull();
            objectA.OneClassB.Id.Should().Be(objectB.Id);
            objectA.OneClassB.Foo.Should().Be(objectB.Foo);
        }

        [Test]
        public async Task TestGetOneToOneInverseForeignKey()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectC = new O2OClassC { Bar = $"Bar String {new Random().Next(100)}" };
            await Connection.InsertAsync(objectC);

            objectC.ElementD.Should().BeNull();

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectC);

            objectC.ElementD.Should().BeNull();

            var objectD = new O2OClassD
            {
                ObjectCKey = objectC.ClassId,
                Foo = $"Foo String {new Random().Next(100)}"
            };
            await Connection.InsertAsync(objectD);

            objectC.ElementD.Should().BeNull();

            await Connection.GetChildrenAsync(objectC);

            objectC.ElementD.Should().NotBeNull();
            objectC.ClassId.Should().Be(objectC.ElementD.ObjectCKey);
            objectD.Foo.Should().Be(objectC.ElementD.Foo);
        }

        [Test]
        public async Task TestGetOneToOneWithInverseRelationship()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectF = new O2OClassF { Bar = $"Bar String {new Random().Next(100)}" };
            var objectE = new O2OClassE { Foo = $"Foo String {new Random().Next(100)}" };
            await Connection.InsertAllAsync(new object[] { objectF, objectE });

            objectE.ObjectF.Should().BeNull();

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectE);
            objectE.ObjectF.Should().BeNull();

            // Set the relationship using IDs
            objectE.ObjectFKey = objectF.Id;
            await Connection.UpdateAsync(objectE);

            objectE.ObjectF.Should().BeNull();

            // Fetch the relationship
            await Connection.GetChildrenAsync(objectE);

            Assert.NotNull(objectE.ObjectF);
            Assert.AreEqual(objectF.Id, objectE.ObjectF.Id);
            Assert.AreEqual(objectF.Bar, objectE.ObjectF.Bar);

            // Check the inverse relationship
            Assert.NotNull(objectE.ObjectF.ObjectE);
            Assert.AreEqual(objectE.Foo, objectE.ObjectF.ObjectE.Foo);
            Assert.AreSame(objectE, objectE.ObjectF.ObjectE);
        }

        [Test]
        public async Task TestGetInverseOneToOneRelationshipWithExplicitKey()
        {
            var objectF = new O2OClassF { Bar = $"Bar String {new Random().Next(100)}" };
            await Connection.InsertAsync(objectF);

            var objectE = new O2OClassE { Foo = $"Foo String {new Random().Next(100)}" };
            await Connection.InsertAsync(objectE);

            Assert.Null(objectF.ObjectE);

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectF);
            Assert.Null(objectF.ObjectE);

            // Set the relationship using IDs
            objectE.ObjectFKey = objectF.Id;
            await Connection.UpdateAsync(objectE);

            Assert.Null(objectF.ObjectE);

            // Fetch the relationship
            await Connection.GetChildrenAsync(objectF);

            Assert.NotNull(objectF.ObjectE);
            Assert.AreEqual(objectE.Foo, objectF.ObjectE.Foo);

            // Check the inverse relationship
            Assert.NotNull(objectF.ObjectE.ObjectF);
            Assert.AreEqual(objectF.Id, objectF.ObjectE.ObjectF.Id);
            Assert.AreEqual(objectF.Bar, objectF.ObjectE.ObjectF.Bar);
            Assert.AreSame(objectF, objectF.ObjectE.ObjectF);
        }

        [Test]
        public async Task TestUpdateSetOneToOneRelationship()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectB = new O2OClassB
            {
                Foo = $"Foo String {new Random().Next(100)}"
            };
            await Connection.InsertAsync(objectB);

            var objectA = new O2OClassA();
            await Connection.InsertAsync(objectA);

            // Set the relationship using objects
            objectA.OneClassB = objectB;
            Assert.AreEqual(0, objectA.OneClassBKey);

            await Connection.UpdateWithChildrenAsync(objectA);

            Assert.AreEqual(objectB.Id, objectA.OneClassBKey, "Foreign key should have been refreshed");

            // Fetch the relationship
            var newObjectA = await Connection.GetAsync<O2OClassA>(objectA.Id);
            Assert.AreEqual(objectB.Id, newObjectA.OneClassBKey, "Foreign key should have been refreshed in database");

        }

        [Test]
        public async Task TestUpdateUnsetOneToOneRelationship()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectB = new O2OClassB
            {
                Foo = $"Foo String {new Random().Next(100)}"
            };
            await Connection.InsertAsync(objectB);

            var objectA = new O2OClassA();
            await Connection.InsertAsync(objectA);

            // Set the relationship using objects
            objectA.OneClassB = objectB;
            Assert.AreEqual(0, objectA.OneClassBKey);

            await Connection.UpdateWithChildrenAsync(objectA);

            Assert.AreEqual(objectB.Id, objectA.OneClassBKey, "Foreign key should have been refreshed");

            // Until here, test is same that TestUpdateSetOneToOneRelationship
            objectA.OneClassB = null; // Unset relationship

            Assert.AreEqual(objectB.Id, objectA.OneClassBKey, "Foreign key shouldn't have been refreshed yet");

            await Connection.UpdateWithChildrenAsync(objectA);

            Assert.AreEqual(0, objectA.OneClassBKey, "Foreign key hasn't been unset");
        }

        [Test]
        public async Task TestUpdateSetOneToOneRelationshipWithInverse()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectF = new O2OClassF
            {
                Bar = $"Bar String {new Random().Next(100)}"
            };
            await Connection.InsertAsync(objectF);

            var objectE = new O2OClassE();
            await Connection.InsertAsync(objectE);

            // Set the relationship using objects
            objectE.ObjectF = objectF;
            Assert.AreEqual(0, objectE.ObjectFKey);

            await Connection.UpdateWithChildrenAsync(objectE);

            Assert.AreEqual(objectF.Id, objectE.ObjectFKey, "Foreign key should have been refreshed");
            Assert.AreSame(objectF, objectE.ObjectF, "Inverse relationship hasn't been set");

            // Fetch the relationship
            var newObjectA = await Connection.GetAsync<O2OClassE>(objectE.Id);
            Assert.AreEqual(objectF.Id, newObjectA.ObjectFKey, "Foreign key should have been refreshed in database");
        }

        [Test]
        public async Task TestUpdateSetOneToOneRelationshipWithInverseForeignKey()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectF = new O2OClassF
            {
                Bar = $"Bar String {new Random().Next(100)}"
            };
            await Connection.InsertAsync(objectF);

            var objectE = new O2OClassE();
            await Connection.InsertAsync(objectE);

            // Set the relationship using objects
            objectF.ObjectE = objectE;
            Assert.AreEqual(0, objectE.ObjectFKey);

            await Connection.UpdateWithChildrenAsync(objectF);

            Assert.AreEqual(objectF.Id, objectE.ObjectFKey, "Foreign key should have been refreshed");
            Assert.AreSame(objectF, objectE.ObjectF, "Inverse relationship hasn't been set");

            // Fetch the relationship
            var newObjectA = await Connection.GetAsync<O2OClassE>(objectE.Id);
            Assert.AreEqual(objectF.Id, newObjectA.ObjectFKey, "Foreign key should have been refreshed in database");
        }

        [Test]
        public async Task TestUpdateUnsetOneToOneRelationshipWithInverseForeignKey()
        {
            // Use standard SQLite-Net API to create a new relationship
            var objectF = new O2OClassF
            {
                Bar = $"Bar String {new Random().Next(100)}"
            };
            await Connection.InsertAsync(objectF);

            var objectE = new O2OClassE();
            await Connection.InsertAsync(objectE);

            // Set the relationship using objects
            objectF.ObjectE = objectE;
            Assert.AreEqual(0, objectE.ObjectFKey);

            await Connection.UpdateWithChildrenAsync(objectF);

            Assert.AreEqual(objectF.Id, objectE.ObjectFKey, "Foreign key should have been refreshed");
            Assert.AreSame(objectF, objectE.ObjectF, "Inverse relationship hasn't been set");

            // At this point the test is the same as TestUpdateSetOneToOneRelationshipWithInverseForeignKey
            objectF.ObjectE = null;     // Unset the relationship

            await Connection.UpdateWithChildrenAsync(objectF);

            // Fetch the relationship
            var newObjectA = await Connection.GetAsync<O2OClassE>(objectE.Id);
            Assert.AreEqual(0, newObjectA.ObjectFKey, "Foreign key should have been refreshed in database");
        }

        [Test]
        public async Task TestGetAllNoFilter()
        {
            var a1 = new O2OClassA();
            var a2 = new O2OClassA();
            var a3 = new O2OClassA();
            var aObjects = new []{ a1, a2, a3 };
            await Connection.InsertAllAsync(aObjects);

            var b1 = new O2OClassB{ Foo = "Foo 1" };
            var b2 = new O2OClassB{ Foo = "Foo 2" };
            var b3 = new O2OClassB{ Foo = "Foo 3" };
            var bObjects = new []{ b1, b2, b3 };
            await Connection.InsertAllAsync(bObjects);

            a1.OneClassB = b1;
            a2.OneClassB = b2;
            a3.OneClassB = b3;
            await Connection.UpdateWithChildrenAsync(a1);
            await Connection.UpdateWithChildrenAsync(a2);
            await Connection.UpdateWithChildrenAsync(a3);

            var aElements = (await Connection.GetAllWithChildrenAsync<O2OClassA>()).OrderBy(a => a.Id).ToArray();
            Assert.AreEqual(aObjects.Length, aElements.Length);
            for (int i = 0; i < aObjects.Length; i++) {
                Assert.AreEqual(aObjects[i].Id, aElements[i].Id);
                Assert.AreEqual(aObjects[i].OneClassB.Id, aElements[i].OneClassB.Id);
                Assert.AreEqual(aObjects[i].OneClassB.Foo, aElements[i].OneClassB.Foo);
            }

        }

        [Test]
        public async Task TestGetAllFilter()
        {
            var c1 = new O2OClassC { Bar = "Bar 1" };
            var c2 = new O2OClassC { Bar = "Foo 2" };
            var c3 = new O2OClassC { Bar = "Bar 3" };
            var cObjects = new []{ c1, c2, c3 };
            await Connection.InsertAllAsync(cObjects);

            var d1 = new O2OClassD{ Foo = "Foo 1" };
            var d2 = new O2OClassD{ Foo = "Foo 2" };
            var d3 = new O2OClassD{ Foo = "Foo 3" };
            var bObjects = new []{ d1, d2, d3 };
            await Connection.InsertAllAsync(bObjects);

            c1.ElementD = d1;
            c2.ElementD = d2;
            c3.ElementD = d3;
            await Connection.UpdateWithChildrenAsync(c1);
            await Connection.UpdateWithChildrenAsync(c2);
            await Connection.UpdateWithChildrenAsync(c3);

            var expectedCObjects = cObjects.Where(c => c.Bar.Contains("Bar")).ToArray();
            var cElements = (await Connection.GetAllWithChildrenAsync<O2OClassC>(c => c.Bar.Contains("Bar")))
                .OrderBy(a => a.ClassId).ToArray();

            Assert.AreEqual(expectedCObjects.Length, cElements.Length);
            for (int i = 0; i < expectedCObjects.Length; i++) {
                Assert.AreEqual(expectedCObjects[i].ClassId, cElements[i].ClassId);
                Assert.AreEqual(expectedCObjects[i].ElementD.Id, cElements[i].ElementD.Id);
                Assert.AreEqual(expectedCObjects[i].ElementD.Foo, cElements[i].ElementD.Foo);
            }
        }
    }
}
