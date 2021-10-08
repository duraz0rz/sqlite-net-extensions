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
// ReSharper disable UnusedMember.Local

namespace SQLiteNetExtensionsIntegrationTests.Tests.Async
{
    [TestFixture]
    public class ManyToManyAsyncTests : BaseAsyncTest
    {
        private class M2MClassA
        {
            [PrimaryKey, AutoIncrement, Column("_id")]
            public int Id { get; set; }

            [ManyToMany(typeof(ClassAClassB))]
            public List<M2MClassB> BObjects { get; set; }

            public string Bar { get; set; }
        }

        private class M2MClassB
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Foo { get; set; }
        }

        private class ClassAClassB
        {
            [ForeignKey(typeof(M2MClassA)), Column("class_a_id")]
            public int ClassAId { get; set; }

            [ForeignKey(typeof(M2MClassB))]
            public int ClassBId { get; set; }
        }

        private class M2MClassC
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [ManyToMany(typeof(ClassCClassD), inverseForeignKey: "ClassCId")]   // Foreign key specified in ManyToMany attribute
            public M2MClassD[] DObjects { get; set; } // Array instead of List

            public string Bar { get; set; }
        }

        private class M2MClassD
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Foo { get; set; }
        }

        private class ClassCClassD
        {
            public int ClassCId { get; set; }   // ForeignKey attribute not needed, already specified in the ManyToMany relationship
            [ForeignKey(typeof(M2MClassD))]
            public int ClassDId { get; set; }
        }

        [Table("class_e")]
        private class M2MClassE
        {
            [PrimaryKey]
            public Guid Id { get; set; } // Guid identifier instead of int

            [ManyToMany(typeof(ClassEClassF), inverseForeignKey: "ClassEId")]   // Foreign key specified in ManyToMany attribute
            public M2MClassF[] FObjects { get; set; } // Array instead of List

            public string Bar { get; set; }
        }

        private class M2MClassF
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Foo { get; set; }
        }

        [Table("class_e_class_f")]
        private class ClassEClassF
        {
            public Guid ClassEId { get; set; }   // ForeignKey attribute not needed, already specified in the ManyToMany relationship
            [ForeignKey(typeof(M2MClassF))]
            public int ClassFId { get; set; }
        }

        private class M2MClassG
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }

            [ManyToMany(typeof(ClassGClassG), "ChildId", "Children")]
            public ObservableCollection<M2MClassG> Parents { get; set; }

            [ManyToMany(typeof(ClassGClassG), "ParentId", "Parents")]
            public List<M2MClassG> Children { get; set; }
        }

        [Table("M2MClassG_ClassG")]
        private class ClassGClassG
        {
            [Column("Identifier")]
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Column("parent_id")]
            public int ParentId { get; set; }
            public int ChildId { get; set; }
        }

        [Table("ClassH")]
        private class M2MClassH
        {
            [Column("_id")]
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }

            [Column("parent_elements")]
            [ManyToMany(typeof(ClassHClassH), "ChildId", "Children", ReadOnly = true)] // Parents relationship is read only
            public List<M2MClassH> Parents { get; set; }

            [ManyToMany(typeof(ClassHClassH), "ParentId", "Parents")]
            public ObservableCollection<M2MClassH> Children { get; set; }
        }

        private class ClassHClassH
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public int ParentId { get; set; }
            public int ChildId { get; set; }
        }

        [SetUp]
        public async Task CreateTables()
        {
            await Connection.CreateTableAsync<M2MClassA>();
            await Connection.CreateTableAsync<M2MClassB>();
            await Connection.CreateTableAsync<ClassAClassB>();
            await Connection.CreateTableAsync<M2MClassC>();
            await Connection.CreateTableAsync<M2MClassD>();
            await Connection.CreateTableAsync<ClassCClassD>();
            await Connection.CreateTableAsync<M2MClassE>();
            await Connection.CreateTableAsync<M2MClassF>();
            await Connection.CreateTableAsync<ClassEClassF>();
            await Connection.CreateTableAsync<M2MClassG>();
            await Connection.CreateTableAsync<ClassGClassG>();
            await Connection.CreateTableAsync<M2MClassH>();
            await Connection.CreateTableAsync<ClassHClassH>();
        }

        [Test]
        public async Task TestGetManyToManyList()
        {
            // In this test we will create a N:M relationship between objects of ClassA and ClassB
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<M2MClassB>
            {
                new() { Foo = $"1- Foo String {new Random().Next(100)}" },
                new() { Foo = $"2- Foo String {new Random().Next(100)}" },
                new() { Foo = $"3- Foo String {new Random().Next(100)}" },
                new() { Foo = $"4- Foo String {new Random().Next(100)}" }
            };
            await Connection.InsertAllAsync(objectsB);

            var objectsA = new List<M2MClassA>
            {
                new() { Bar = $"1- Bar String {new Random().Next(100)}" },
                new() { Bar = $"2- Bar String {new Random().Next(100)}" },
                new() { Bar = $"3- Bar String {new Random().Next(100)}" },
                new() { Bar = $"4- Bar String {new Random().Next(100)}" }
            };

            await Connection.InsertAllAsync(objectsA);

            foreach (var objectA in objectsA)
            {
                var copyA = objectA;
                Assert.Null(objectA.BObjects);

                // Fetch (yet empty) the relationship
                await Connection.GetChildrenAsync(copyA);

                Assert.NotNull(copyA.BObjects);
                Assert.AreEqual(0, copyA.BObjects.Count);
            }


            // Create the relationships in the intermediate table
            for (var aIndex = 0; aIndex < objectsA.Count; aIndex++)
            {
                for (var bIndex = 0; bIndex <= aIndex; bIndex++)
                {
                    await Connection.InsertAsync(new ClassAClassB
                    {
                        ClassAId = objectsA[aIndex].Id,
                        ClassBId = objectsB[bIndex].Id
                    });
                }
            }


            for (var i = 0; i < objectsA.Count; i++)
            {
                var objectA = objectsA[i];

                // Relationship still empty because hasn't been refreshed
                Assert.NotNull(objectA.BObjects);
                Assert.AreEqual(0, objectA.BObjects.Count);

                // Fetch the relationship
                await Connection.GetChildrenAsync(objectA);

                var childrenCount = i + 1;

                Assert.NotNull(objectA.BObjects);
                Assert.AreEqual(childrenCount, objectA.BObjects.Count);
                var foos = objectsB.GetRange(0, childrenCount).Select(objectB => objectB.Foo).ToList();
                foreach (var objectB in objectA.BObjects)
                {
                    Assert.IsTrue(foos.Contains(objectB.Foo));
                }
            }
        }

        [Test]
        public async Task TestGetManyToManyArray()
        {
            // In this test we will create a N:M relationship between objects of ClassC and ClassD
            //      Class C     -       Class D
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // Use standard SQLite-Net API to create the objects
            var objectsD = new List<M2MClassD>
            {
                new() { Foo = $"1- Foo String {new Random().Next(100)}" },
                new() { Foo = $"2- Foo String {new Random().Next(100)}" },
                new() { Foo = $"3- Foo String {new Random().Next(100)}" },
                new() { Foo = $"4- Foo String {new Random().Next(100)}" }
            };
            await Connection.InsertAllAsync(objectsD);

            var objectsC = new List<M2MClassC>
            {
                new() { Bar = $"1- Bar String {new Random().Next(100)}" },
                new() { Bar = $"2- Bar String {new Random().Next(100)}" },
                new() { Bar = $"3- Bar String {new Random().Next(100)}" },
                new() { Bar = $"4- Bar String {new Random().Next(100)}" }
            };

            await Connection.InsertAllAsync(objectsC);

            foreach (var objectC in objectsC)
            {
                var copyC = objectC;
                Assert.Null(objectC.DObjects);

                // Fetch (yet empty) the relationship
                await Connection.GetChildrenAsync(copyC);

                Assert.NotNull(copyC.DObjects);
                Assert.AreEqual(0, copyC.DObjects.Length);
            }


            // Create the relationships in the intermediate table
            for (var cIndex = 0; cIndex < objectsC.Count; cIndex++)
            {
                for (var dIndex = 0; dIndex <= cIndex; dIndex++)
                {
                    await Connection.InsertAsync(new ClassCClassD
                    {
                        ClassCId = objectsC[cIndex].Id,
                        ClassDId = objectsD[dIndex].Id
                    });
                }
            }


            for (var i = 0; i < objectsC.Count; i++)
            {
                var objectC = objectsC[i];

                // Relationship still empty because hasn't been refreshed
                Assert.NotNull(objectC.DObjects);
                Assert.AreEqual(0, objectC.DObjects.Length);

                // Fetch the relationship
                await Connection.GetChildrenAsync(objectC);

                var childrenCount = i + 1;

                Assert.NotNull(objectC.DObjects);
                Assert.AreEqual(childrenCount, objectC.DObjects.Length);
                var foos = objectsD.GetRange(0, childrenCount).Select(objectB => objectB.Foo).ToList();
                foreach (var objectD in objectC.DObjects)
                {
                    Assert.IsTrue(foos.Contains(objectD.Foo));
                }
            }
        }

        [Test]
        public async Task TestUpdateSetManyToManyList()
        {
            // In this test we will create a N:M relationship between objects of ClassA and ClassB
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<M2MClassB>
            {
                new() { Foo = $"1- Foo String {new Random().Next(100)}" },
                new() { Foo = $"2- Foo String {new Random().Next(100)}" },
                new() { Foo = $"3- Foo String {new Random().Next(100)}" },
                new() { Foo = $"4- Foo String {new Random().Next(100)}" }
            };
            await Connection.InsertAllAsync(objectsB);

            var objectsA = new List<M2MClassA>
            {
                new()
                {
                    Bar = $"1- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                },
                new()
                {
                    Bar = $"2- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                },
                new()
                {
                    Bar = $"3- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                },
                new()
                {
                    Bar = $"4- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                }
            };

            await Connection.InsertAllAsync(objectsA);

            // Create the relationships
            for (var aIndex = 0; aIndex < objectsA.Count; aIndex++)
            {
                var objectA = objectsA[aIndex];

                for (var bIndex = 0; bIndex <= aIndex; bIndex++)
                {
                    var objectB = objectsB[bIndex];
                    objectA.BObjects.Add(objectB);
                }

                await Connection.UpdateWithChildrenAsync(objectA);
            }

            for (var i = 0; i < objectsA.Count; i++)
            {
                var objectA = objectsA[i];
                var childrenCount = i + 1;
                var storedChildKeyList =
                    (await Connection.Table<ClassAClassB>()
                        .Where(ab => ab.ClassAId == objectA.Id)
                        .ToListAsync())
                    .Select(ab => ab.ClassBId).ToList();

                Assert.AreEqual(childrenCount, storedChildKeyList.Count(), "Relationship count is not correct");
                var expectedChildIds = objectsB.GetRange(0, childrenCount).Select(objectB => objectB.Id).ToList();
                foreach (var objectBKey in storedChildKeyList)
                {
                    Assert.IsTrue(expectedChildIds.Contains(objectBKey), "Relationship ID is not correct");
                }
            }
        }

        [Test]
        public async Task TestUpdateUnsetManyToManyList()
        {
            // In this test we will create a N:M relationship between objects of ClassA and ClassB
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // After that, we will remove objects 1 and 2 from relationships
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       <empty>
            //          2       -       <empty>
            //          3       -       3
            //          4       -       3, 4

            // Use standard SQLite-Net API to create the objects
            var objectsB = new List<M2MClassB>
            {
                new() { Foo = $"1- Foo String {new Random().Next(100)}" },
                new() { Foo = $"2- Foo String {new Random().Next(100)}" },
                new() { Foo = $"3- Foo String {new Random().Next(100)}" },
                new() { Foo = $"4- Foo String {new Random().Next(100)}" }
            };
            await Connection.InsertAllAsync(objectsB);

            var objectsA = new List<M2MClassA>
            {
                new()
                {
                    Bar = $"1- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                },
                new()
                {
                    Bar = $"2- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                },
                new()
                {
                    Bar = $"3- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                },
                new()
                {
                    Bar = $"4- Bar String {new Random().Next(100)}",
                    BObjects = new List<M2MClassB>()
                }
            };

            await Connection.InsertAllAsync(objectsA);

            // Create the relationships
            for (var aIndex = 0; aIndex < objectsA.Count; aIndex++)
            {
                var objectA = objectsA[aIndex];

                for (var bIndex = 0; bIndex <= aIndex; bIndex++)
                {
                    var objectB = objectsB[bIndex];
                    objectA.BObjects.Add(objectB);
                }

                await Connection.UpdateWithChildrenAsync(objectA);
            }

            // At these points all the relationships are set
            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // Now we will remove ClassB objects 1 and 2 from the relationships
            var objectsBToRemove = objectsB.GetRange(0, 2);

            foreach (var objectA in objectsA)
            {
                objectA.BObjects.RemoveAll(objectsBToRemove.Contains);
                await Connection.UpdateWithChildrenAsync(objectA);
            }

            // This should now be the current status of all relationships

            //      Class A     -       Class B
            // --------------------------------------
            //          1       -       <empty>
            //          2       -       <empty>
            //          3       -       3
            //          4       -       3, 4

            for (var i = 0; i < objectsA.Count; i++)
            {
                var objectA = objectsA[i];

                var storedChildKeyList =
                    (await Connection.Table<ClassAClassB>().Where(ab => ab.ClassAId == objectA.Id).ToListAsync())
                    .Select(ab => ab.ClassBId).ToList();

                var expectedChildIds = objectsB.GetRange(0, i + 1).Where(b => !objectsBToRemove.Contains(b)).Select(objectB => objectB.Id).ToList();
                Assert.AreEqual(expectedChildIds.Count, storedChildKeyList.Count,
                    $"Relationship count is not correct for Object with Id {objectA.Id}");
                foreach (var objectBKey in storedChildKeyList)
                {
                    Assert.IsTrue(expectedChildIds.Contains(objectBKey), "Relationship ID is not correct");
                }
            }
        }

        [Test]
        public async Task TestGetManyToManyGuidIdentifier()
        {
            // In this test we will create a N:M relationship between objects of ClassE and ClassF
            //      Class E     -       Class F
            // --------------------------------------
            //          1       -       1
            //          2       -       1, 2
            //          3       -       1, 2, 3
            //          4       -       1, 2, 3, 4

            // Use standard SQLite-Net API to create the objects
            var objectsF = new List<M2MClassF>
            {
                new() { Foo = $"1- Foo String {new Random().Next(100)}" },
                new() { Foo = $"2- Foo String {new Random().Next(100)}" },
                new() { Foo = $"3- Foo String {new Random().Next(100)}" },
                new() { Foo = $"4- Foo String {new Random().Next(100)}" }
            };
            await Connection.InsertAllAsync(objectsF);

            var objectsE = new List<M2MClassE>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Bar = $"1- Bar String {new Random().Next(100)}"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Bar = $"2- Bar String {new Random().Next(100)}"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Bar = $"3- Bar String {new Random().Next(100)}"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Bar = $"4- Bar String {new Random().Next(100)}"
                }
            };

            await Connection.InsertAllAsync(objectsE);

            foreach (var objectE in objectsE)
            {
                var copyE = objectE;
                Assert.Null(objectE.FObjects);

                // Fetch (yet empty) the relationship
                await Connection.GetChildrenAsync(copyE);

                Assert.NotNull(copyE.FObjects);
                Assert.AreEqual(0, copyE.FObjects.Length);
            }


            // Create the relationships in the intermediate table
            for (var eIndex = 0; eIndex < objectsE.Count; eIndex++)
            {
                for (var fIndex = 0; fIndex <= eIndex; fIndex++)
                {
                    await Connection.InsertAsync(new ClassEClassF
                    {
                        ClassEId = objectsE[eIndex].Id,
                        ClassFId = objectsF[fIndex].Id
                    });
                }
            }


            for (var i = 0; i < objectsE.Count; i++)
            {
                var objectE = objectsE[i];

                // Relationship still empty because hasn't been refreshed
                Assert.NotNull(objectE.FObjects);
                Assert.AreEqual(0, objectE.FObjects.Length);

                // Fetch the relationship
                await Connection.GetChildrenAsync(objectE);

                var childrenCount = i + 1;

                Assert.NotNull(objectE.FObjects);
                Assert.AreEqual(childrenCount, objectE.FObjects.Length);
                var foos = objectsF.GetRange(0, childrenCount).Select(objectB => objectB.Foo).ToList();
                foreach (var objectD in objectE.FObjects)
                {
                    Assert.IsTrue(foos.Contains(objectD.Foo));
                }
            }
        }

        [Test]
        public async Task TestManyToManyCircular()
        {
            // In this test we will create a many to many relationship between instances of the same class
            // including inverse relationship

            // This is the hierarchy that we're going to implement
            //                      1
            //                     / \
            //                   [2] [3]
            //                  /  \ /  \
            //                 4    5    6
            //
            // To implement it, only relationships of objects [2] and [3] are going to be persisted,
            // the inverse relationships will be discovered automatically

            var object1 = new M2MClassG { Name = "Object 1" };
            var object2 = new M2MClassG { Name = "Object 2" };
            var object3 = new M2MClassG { Name = "Object 3" };
            var object4 = new M2MClassG { Name = "Object 4" };
            var object5 = new M2MClassG { Name = "Object 5" };
            var object6 = new M2MClassG { Name = "Object 6" };

            var objects = new List<M2MClassG> { object1, object2, object3, object4, object5, object6 };
            await Connection.InsertAllAsync(objects);

            object2.Parents = new ObservableCollection<M2MClassG> { object1 };
            object2.Children = new List<M2MClassG> { object4, object5 };
            await Connection.UpdateWithChildrenAsync(object2);

            object3.Parents = new ObservableCollection<M2MClassG> { object1 };
            object3.Children = new List<M2MClassG> { object5, object6 };
            await Connection.UpdateWithChildrenAsync(object3);

            // These relationships are discovered on runtime, assign them to check for correctness below
            object1.Children = new List<M2MClassG> { object2, object3 };
            object4.Parents = new ObservableCollection<M2MClassG> { object2 };
            object5.Parents = new ObservableCollection<M2MClassG> { object2, object3 };
            object6.Parents = new ObservableCollection<M2MClassG> { object3 };

            foreach (var expected in objects)
            {
                var obtained = await Connection.GetWithChildrenAsync<M2MClassG>(expected.Id);

                if (obtained?.Children == null || obtained.Parents == null) Assert.Fail("obtained, its parents or its children were null");

                Assert.AreEqual(expected.Name, obtained.Name);
                Assert.AreEqual((expected.Children ?? new List<M2MClassG>()).Count, (obtained.Children ?? new List<M2MClassG>()).Count, obtained.Name);
                Assert.AreEqual((expected.Parents ?? new ObservableCollection<M2MClassG>()).Count, (obtained.Parents ?? new ObservableCollection<M2MClassG>()).Count, obtained.Name);

                foreach (var child in expected.Children ?? Enumerable.Empty<M2MClassG>())
                    Assert.IsTrue(obtained.Children.Any(c => c.Id == child.Id && c.Name == child.Name), obtained.Name);

                foreach (var parent in expected.Parents ?? Enumerable.Empty<M2MClassG>())
                    Assert.IsTrue(obtained.Parents.Any(p => p.Id == parent.Id && p.Name == parent.Name), obtained.Name);
            }
        }

        [Test]
        public async Task TestManyToManyCircularReadOnly()
        {
            // In this test we will create a many to many relationship between instances of the same class
            // including inverse relationship

            // This is the hierarchy that we're going to implement
            //                     [1]
            //                     / \
            //                   [2] [3]
            //                  /  \ /  \
            //                 4    5    6
            //
            // To implement it, only children relationships of objects [1], [2] and [3] are going to be persisted,
            // the inverse relationships will be discovered automatically

            var object1 = new M2MClassH { Name = "Object 1" };
            var object2 = new M2MClassH { Name = "Object 2" };
            var object3 = new M2MClassH { Name = "Object 3" };
            var object4 = new M2MClassH { Name = "Object 4" };
            var object5 = new M2MClassH { Name = "Object 5" };
            var object6 = new M2MClassH { Name = "Object 6" };

            var objects = new List<M2MClassH> { object1, object2, object3, object4, object5, object6 };
            await Connection.InsertAllAsync(objects);

            object1.Children = new ObservableCollection<M2MClassH> { object2, object3 };
            await Connection.UpdateWithChildrenAsync(object1);

            object2.Children = new ObservableCollection<M2MClassH> { object4, object5 };
            await Connection.UpdateWithChildrenAsync(object2);

            object3.Children = new ObservableCollection<M2MClassH> { object5, object6 };
            await Connection.UpdateWithChildrenAsync(object3);

            // These relationships are discovered on runtime, assign them to check for correctness below
            object2.Parents = new List<M2MClassH> { object1 };
            object3.Parents = new List<M2MClassH> { object1 };
            object4.Parents = new List<M2MClassH> { object2 };
            object5.Parents = new List<M2MClassH> { object2, object3 };
            object6.Parents = new List<M2MClassH> { object3 };

            foreach (var expected in objects)
            {
                var obtained = await Connection.GetWithChildrenAsync<M2MClassH>(expected.Id);

                if (obtained?.Children == null || obtained.Parents == null) Assert.Fail("obtained, its parents or its children were null");

                Assert.AreEqual(expected.Name, obtained.Name);
                Assert.AreEqual((expected.Children ?? new ObservableCollection<M2MClassH>()).Count, (obtained.Children ?? new ObservableCollection<M2MClassH>()).Count, obtained.Name);
                Assert.AreEqual((expected.Parents ?? new List<M2MClassH>()).Count, (obtained.Parents ?? new List<M2MClassH>()).Count, obtained.Name);

                foreach (var child in expected.Children ?? Enumerable.Empty<M2MClassH>())
                    Assert.IsTrue(obtained.Children.Any(c => c.Id == child.Id && c.Name == child.Name), obtained.Name);

                foreach (var parent in expected.Parents ?? Enumerable.Empty<M2MClassH>())
                    Assert.IsTrue(obtained.Parents.Any(p => p.Id == parent.Id && p.Name == parent.Name), obtained.Name);
            }
        }
    }
}
