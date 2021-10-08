using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensionsAsync.Extensions;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace SQLiteNetExtensionsIntegrationTests.Tests.Async
{
    [TestFixture]
    public class OneToManyAsyncTests : BaseAsyncTest
    {
        [Table("ClassA")]
        private class O2MClassA
        {
            [PrimaryKey, AutoIncrement, Column("PrimaryKey")]
            public int Id { get; set; }

            [OneToMany]
            public List<O2MClassB> BObjects { get; set; }

            public string Bar { get; set; }
        }

        [Table("ClassB")]
        private class O2MClassB
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [ForeignKey(typeof(O2MClassA)), Column("class_a_id")]
            public int ClassAKey { get; set; }

            public string Foo { get; set; }
        }

        private class O2MClassC
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [OneToMany]
            public ObservableCollection<O2MClassD> DObjects { get; set; }

            public string Bar { get; set; }
        }

        private class O2MClassD
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [ForeignKey(typeof(O2MClassC))]
            public int ClassCKey { get; set; }

            [ManyToOne]     // OneToMany Inverse relationship
            public O2MClassC ObjectC { get; set; }

            public string Foo { get; set; }
        }

        private class O2MClassE
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [OneToMany("ClassEKey")]   // Explicit foreign key declaration
            public O2MClassF[] FObjects { get; set; } // Array of objects instead of List

            public string Bar { get; set; }
        }

        private class O2MClassF
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public int ClassEKey { get; set; }  // Foreign key declared in relationship

            public string Foo { get; set; }
        }

        private class O2MClassG
        {
            [PrimaryKey]
            public Guid Guid { get; set; }

            [OneToMany]
            public ObservableCollection<O2MClassH> HObjects { get; set; }

            public string Bar { get; set; }
        }

        private class O2MClassH
        {
            [PrimaryKey]
            public Guid Guid { get; set; }

            [ForeignKey(typeof(O2MClassG))]
            public Guid ClassGKey { get; set; }

            [ManyToOne]     // OneToMany Inverse relationship
            public O2MClassG ObjectG { get; set; }

            public string Foo { get; set; }
        }

        [SetUp]
        public async Task SetUp()
        {
            await Connection.CreateTableAsync<O2MClassA>();
            await Connection.CreateTableAsync<O2MClassB>();
            await Connection.CreateTableAsync<O2MClassC>();
            await Connection.CreateTableAsync<O2MClassD>();
            await Connection.CreateTableAsync<O2MClassE>();
            await Connection.CreateTableAsync<O2MClassF>();
            await Connection.CreateTableAsync<O2MClassG>();
            await Connection.CreateTableAsync<O2MClassH>();
            await Connection.CreateTableAsync<Employee>();
        }

        [Test]
        public async Task TestGetOneToManyList()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
            {
                new O2MClassB {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await Connection.InsertAsync(objectA);

            Assert.Null(objectA.BObjects);

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectA);
            Assert.NotNull(objectA.BObjects);
            Assert.AreEqual(0, objectA.BObjects.Count);

            // Set the relationship using IDs
            foreach (var objectB in objectsB)
            {
                objectB.ClassAKey = objectA.Id;
                await Connection.UpdateAsync(objectB);
            }

            Assert.NotNull(objectA.BObjects);
            Assert.AreEqual(0, objectA.BObjects.Count);

            // Fetch the relationship
            await Connection.GetChildrenAsync(objectA);

            Assert.NotNull(objectA.BObjects);
            Assert.AreEqual(objectsB.Count, objectA.BObjects.Count);
            var foos = objectsB.Select(objectB => objectB.Foo).ToList();
            foreach (var objectB in objectA.BObjects)
            {
                Assert.IsTrue(foos.Contains(objectB.Foo));
            }
        }

        [Test]
        public async Task TestGetOneToManyListWithInverse()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassD>
            {
                new O2MClassD {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassD {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassD {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassD {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsD);

            var objectC = new O2MClassC();
            await Connection.InsertAsync(objectC);

            Assert.Null(objectC.DObjects);

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectC);
            Assert.NotNull(objectC.DObjects);
            Assert.AreEqual(0, objectC.DObjects.Count);

            // Set the relationship using IDs
            foreach (var objectD in objectsD)
            {
                objectD.ClassCKey = objectC.Id;
                await Connection.UpdateAsync(objectD);
            }

            Assert.NotNull(objectC.DObjects);
            Assert.AreEqual(0, objectC.DObjects.Count);

            // Fetch the relationship
            await Connection.GetChildrenAsync(objectC);

            Assert.NotNull(objectC.DObjects);
            Assert.AreEqual(objectsD.Count, objectC.DObjects.Count);
            var foos = objectsD.Select(objectB => objectB.Foo).ToList();
            foreach (var objectD in objectC.DObjects)
            {
                Assert.IsTrue(foos.Contains(objectD.Foo));
                Assert.AreEqual(objectC.Id, objectD.ObjectC.Id);
                Assert.AreEqual(objectC.Bar, objectD.ObjectC.Bar);
                Assert.AreSame(objectC, objectD.ObjectC); // Not only equal, they are the same!
            }
        }

        [Test]
        public async Task TestGetOneToManyArray()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsF = new[]
            {
                new O2MClassF {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassF {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassF {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassF {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsF);

            var objectE = new O2MClassE();
            await Connection.InsertAsync(objectE);

            Assert.Null(objectE.FObjects);

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectE);
            Assert.NotNull(objectE.FObjects);
            Assert.AreEqual(0, objectE.FObjects.Length);

            // Set the relationship using IDs
            foreach (var objectB in objectsF)
            {
                objectB.ClassEKey = objectE.Id;
                await Connection.UpdateAsync(objectB);
            }

            Assert.NotNull(objectE.FObjects);
            Assert.AreEqual(0, objectE.FObjects.Length);

            // Fetch the relationship
            await Connection.GetChildrenAsync(objectE);

            Assert.NotNull(objectE.FObjects);
            Assert.AreEqual(objectsF.Length, objectE.FObjects.Length);
            var foos = objectsF.Select(objectF => objectF.Foo).ToList();
            foreach (var objectF in objectE.FObjects)
            {
                Assert.IsTrue(foos.Contains(objectF.Foo));
            }
        }

        [Test]
        public async Task TestUpdateSetOneToManyList()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
            {
                new O2MClassB {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await Connection.InsertAsync(objectA);

            Assert.Null(objectA.BObjects);

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                Assert.AreEqual(0, objectB.ClassAKey, "Foreign keys shouldn't have been updated yet");
            }


            await Connection.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                Assert.AreEqual(objectA.Id, objectB.ClassAKey, "Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = await Connection.GetAsync<O2MClassB>(objectB.Id);
                Assert.AreEqual(objectA.Id, newObjectB.ClassAKey, "Database stored value is not correct");
            }

        }

        [Test]
        public async Task TestUpdateUnsetOneToManyEmptyList()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
            {
                new O2MClassB {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await Connection.InsertAsync(objectA);

            Assert.Null(objectA.BObjects);

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                Assert.AreEqual(0, objectB.ClassAKey, "Foreign keys shouldn't have been updated yet");
            }

            await Connection.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                Assert.AreEqual(objectA.Id, objectB.ClassAKey, "Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = await Connection.GetAsync<O2MClassB>(objectB.Id);
                Assert.AreEqual(objectA.Id, newObjectB.ClassAKey, "Database stored value is not correct");
            }

            // At this point the test is exactly the same as TestUpdateSetOneToManyList
            objectA.BObjects = new List<O2MClassB>(); // Reset the relationship

            await Connection.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectsB)
            {
                // Check database values
                var newObjectB = await Connection.GetAsync<O2MClassB>(objectB.Id);
                Assert.AreEqual(0, newObjectB.ClassAKey, "Database stored value is not correct");
            }

        }

        [Test]
        public async Task TestUpdateUnsetOneToManyNullList()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<O2MClassB>
            {
                new O2MClassB {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassB {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsB);

            var objectA = new O2MClassA();
            await Connection.InsertAsync(objectA);

            Assert.Null(objectA.BObjects);

            objectA.BObjects = objectsB;

            foreach (var objectB in objectsB)
            {
                Assert.AreEqual(0, objectB.ClassAKey, "Foreign keys shouldn't have been updated yet");
            }

            await Connection.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectA.BObjects)
            {
                Assert.AreEqual(objectA.Id, objectB.ClassAKey, "Foreign keys haven't been updated yet");

                // Check database values
                var newObjectB = await Connection.GetAsync<O2MClassB>(objectB.Id);
                Assert.AreEqual(objectA.Id, newObjectB.ClassAKey, "Database stored value is not correct");
            }

            // At this point the test is exactly the same as TestUpdateSetOneToManyList
            objectA.BObjects = null; // Reset the relationship

            await Connection.UpdateWithChildrenAsync(objectA);

            foreach (var objectB in objectsB)
            {
                // Check database values
                var newObjectB = await Connection.GetAsync<O2MClassB>(objectB.Id);
                Assert.AreEqual(0, newObjectB.ClassAKey, "Database stored value is not correct");
            }

        }

        [Test]
        public async Task TestUpdateSetOneToManyArray()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsF = new[]
            {
                new O2MClassF {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassF {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassF {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassF {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsF);

            var objectE = new O2MClassE();
            await Connection.InsertAsync(objectE);

            Assert.Null(objectE.FObjects);

            objectE.FObjects = objectsF;

            foreach (var objectF in objectsF)
            {
                Assert.AreEqual(0, objectF.ClassEKey, "Foreign keys shouldn't have been updated yet");
            }


            await Connection.UpdateWithChildrenAsync(objectE);

            foreach (var objectF in objectE.FObjects)
            {
                Assert.AreEqual(objectE.Id, objectF.ClassEKey, "Foreign keys haven't been updated yet");

                // Check database values
                var newObjectF = await Connection.GetAsync<O2MClassF>(objectF.Id);
                Assert.AreEqual(objectE.Id, newObjectF.ClassEKey, "Database stored value is not correct");
            }

        }


        [Test]
        public async Task TestUpdateSetOneToManyListWithInverse()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassD>
            {
                new O2MClassD {
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassD {
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassD {
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassD {
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsD);

            var objectC = new O2MClassC();
            await Connection.InsertAsync(objectC);

            Assert.Null(objectC.DObjects);

            objectC.DObjects = new ObservableCollection<O2MClassD>(objectsD);

            foreach (var objectD in objectsD)
            {
                Assert.AreEqual(0, objectD.ClassCKey, "Foreign keys shouldn't have been updated yet");
            }


            await Connection.UpdateWithChildrenAsync(objectC);

            foreach (var objectD in objectC.DObjects)
            {
                Assert.AreEqual(objectC.Id, objectD.ClassCKey, "Foreign keys haven't been updated yet");
                Assert.AreSame(objectC, objectD.ObjectC, "Inverse relationship hasn't been set");

                // Check database values
                var newObjectD = await Connection.GetAsync<O2MClassD>(objectD.Id);
                Assert.AreEqual(objectC.Id, newObjectD.ClassCKey, "Database stored value is not correct");
            }

        }

        [Test]
        public async Task TestGetOneToManyListWithInverseGuidId()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<O2MClassH>
            {
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsD);

            var objectC = new O2MClassG { Guid = Guid.NewGuid() };
            await Connection.InsertAsync(objectC);

            Assert.Null(objectC.HObjects);

            // Fetch (yet empty) the relationship
            await Connection.GetChildrenAsync(objectC);
            Assert.NotNull(objectC.HObjects);
            Assert.AreEqual(0, objectC.HObjects.Count);

            // Set the relationship using IDs
            foreach (var objectD in objectsD)
            {
                objectD.ClassGKey = objectC.Guid;
                await Connection.UpdateAsync(objectD);
            }

            Assert.NotNull(objectC.HObjects);
            Assert.AreEqual(0, objectC.HObjects.Count);

            // Fetch the relationship
            await Connection.GetChildrenAsync(objectC);

            Assert.NotNull(objectC.HObjects);
            Assert.AreEqual(objectsD.Count, objectC.HObjects.Count);
            var foos = objectsD.Select(objectB => objectB.Foo).ToList();
            foreach (var objectD in objectC.HObjects)
            {
                Assert.IsTrue(foos.Contains(objectD.Foo));
                Assert.AreEqual(objectC.Guid, objectD.ObjectG.Guid);
                Assert.AreEqual(objectC.Bar, objectD.ObjectG.Bar);
                Assert.AreSame(objectC, objectD.ObjectG); // Not only equal, they are the same!
            }
        }

        [Test]
        public async Task TestUpdateSetOneToManyListWithInverseGuidId()
        {
            // Use standard SQLite-Net API to create the objects
            var objectsH = new List<O2MClassH>
            {
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("1- Foo String {0}", new Random().Next(100))
                },
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("2- Foo String {0}", new Random().Next(100))
                },
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("3- Foo String {0}", new Random().Next(100))
                },
                new O2MClassH {
                    Guid = Guid.NewGuid(),
                    Foo = string.Format("4- Foo String {0}", new Random().Next(100))
                }
            };
            await Connection.InsertAllAsync(objectsH);

            var objectG = new O2MClassG { Guid = Guid.NewGuid() };
            await Connection.InsertAsync(objectG);

            Assert.Null(objectG.HObjects);

            objectG.HObjects = new ObservableCollection<O2MClassH>(objectsH);

            foreach (var objectD in objectsH)
            {
                Assert.AreEqual(Guid.Empty, objectD.ClassGKey, "Foreign keys shouldn't have been updated yet");
            }


            await Connection.UpdateWithChildrenAsync(objectG);

            foreach (var objectH in objectG.HObjects)
            {
                Assert.AreEqual(objectG.Guid, objectH.ClassGKey, "Foreign keys haven't been updated yet");
                Assert.AreSame(objectG, objectH.ObjectG, "Inverse relationship hasn't been set");

                // Check database values
                var newObjectH = await Connection.GetAsync<O2MClassH>(objectH.Guid);
                Assert.AreEqual(objectG.Guid, newObjectH.ClassGKey, "Database stored value is not correct");
            }

        }

        private class Employee
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }

            [OneToMany]
            public List<Employee> Subordinates { get; set; }

            [ManyToOne]
            public Employee Supervisor { get; set; }

            [ForeignKey(typeof(Employee))]
            public int SupervisorId { get; set; }
        }

        /// <summary>
        /// Tests the recursive inverse relationship automatic discovery
        /// Issue #17: https://bitbucket.org/twincoders/sqlite-net-extensions/issue/17
        /// </summary>
        [Test]
        [NUnit.Framework.Ignore("There's a bug here")]
        public async Task TestRecursiveInverseRelationship()
        {
            var employee1 = new Employee
            {
                Name = "Albert"
            };
            await Connection.InsertAsync(employee1);

            var employee2 = new Employee
            {
                Name = "Leonardo",
                SupervisorId = employee1.Id
            };
            await Connection.InsertAsync(employee2);

            var result = await Connection.GetWithChildrenAsync<Employee>(employee1.Id);
            Assert.AreEqual(employee1, result);
            Assert.That(employee1.Subordinates.Select(e => e.Name), Contains.Item(employee2.Name));
        }

    }
}
