using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.IntegrationTests;
using SQLiteNetExtensionsAsync.Extensions;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace SQLiteNetExtensionsIntegrationTests.Tests.Async
{
    [TestFixture]
    public class RecursiveWriteAsyncTests
    {
        public class PersonInt
        {
            [PrimaryKey] [AutoIncrement] public int Identifier { get; set; }

            public string Name { get; set; }
            public string Surname { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public PassportInt PassportInt { get; set; }
        }

        public class PassportInt
        {
            [PrimaryKey] [AutoIncrement] public int Id { get; set; }

            public string PassportNumber { get; set; }

            [ForeignKey(typeof(PersonInt))] public int OwnerId { get; set; }

            [OneToOne(ReadOnly = true)] public PersonInt Owner { get; set; }
        }

        [Test]
        public async Task TestOneToOneRecursiveInsertAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportInt>();
            await conn.DropTableAsync<PersonInt>();
            await conn.CreateTableAsync<PassportInt>();
            await conn.CreateTableAsync<PersonInt>();

            var person = new PersonInt
            {
                Name = "John",
                Surname = "Smith",
                PassportInt = new PassportInt
                {
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertWithChildrenAsync(person, true);

            var obtainedPerson = await conn.FindAsync<PersonInt>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<PassportInt>(person.PassportInt.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.PassportInt.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }

        [Test]
        public async Task TestOneToOneRecursiveInsertOrReplaceAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportInt>();
            await conn.DropTableAsync<PersonInt>();
            await conn.CreateTableAsync<PassportInt>();
            await conn.CreateTableAsync<PersonInt>();

            var person = new PersonInt
            {
                Name = "John",
                Surname = "Smith",
                PassportInt = new PassportInt
                {
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, true);

            var obtainedPerson = await conn.FindAsync<PersonInt>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<PassportInt>(person.PassportInt.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.PassportInt.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));


            var newPerson = new PersonInt
            {
                Identifier = person.Identifier,
                Name = "John",
                Surname = "Smith",
                PassportInt = new PassportInt
                {
                    Id = person.PassportInt.Id,
                    PassportNumber = "JS123456"
                }
            };
            person = newPerson;

            // Replace the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, true);

            obtainedPerson = await conn.FindAsync<PersonInt>(person.Identifier);
            obtainedPassport = await conn.FindAsync<PassportInt>(person.PassportInt.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.PassportInt.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }

        public class PersonGuid
        {
            [PrimaryKey] public Guid Identifier { get; set; }

            public string Name { get; set; }
            public string Surname { get; set; }

            [OneToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public PassportGuid Passport { get; set; }
        }

        public class PassportGuid
        {
            [PrimaryKey] public Guid Id { get; set; }

            public string PassportNumber { get; set; }

            [ForeignKey(typeof(PersonGuid))] public Guid OwnerId { get; set; }

            [OneToOne(ReadOnly = true)] public PersonGuid Owner { get; set; }
        }

        [Test]
        public async Task TestOneToOneRecursiveInsertGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportGuid>();
            await conn.DropTableAsync<PersonGuid>();
            await conn.CreateTableAsync<PassportGuid>();
            await conn.CreateTableAsync<PersonGuid>();

            var person = new PersonGuid
            {
                Identifier = Guid.NewGuid(),
                Name = "John",
                Surname = "Smith",
                Passport = new PassportGuid
                {
                    Id = Guid.NewGuid(),
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertWithChildrenAsync(person, true);

            var obtainedPerson = await conn.FindAsync<PersonGuid>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }

        [Test]
        public async Task TestOneToOneRecursiveInsertOrReplaceGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<PassportGuid>();
            await conn.DropTableAsync<PersonGuid>();
            await conn.CreateTableAsync<PassportGuid>();
            await conn.CreateTableAsync<PersonGuid>();

            var person = new PersonGuid
            {
                Identifier = Guid.NewGuid(),
                Name = "John",
                Surname = "Smith",
                Passport = new PassportGuid
                {
                    Id = Guid.NewGuid(),
                    PassportNumber = "JS123456"
                }
            };

            // Insert the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, true);

            var obtainedPerson = await conn.FindAsync<PersonGuid>(person.Identifier);
            var obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));


            var newPerson = new PersonGuid
            {
                Identifier = person.Identifier,
                Name = "John",
                Surname = "Smith",
                Passport = new PassportGuid
                {
                    Id = person.Passport.Id,
                    PassportNumber = "JS123456"
                }
            };
            person = newPerson;

            // Replace the elements in the database recursively
            await conn.InsertOrReplaceWithChildrenAsync(person, true);

            obtainedPerson = await conn.FindAsync<PersonGuid>(person.Identifier);
            obtainedPassport = await conn.FindAsync<PassportGuid>(person.Passport.Id);

            Assert.NotNull(obtainedPerson);
            Assert.NotNull(obtainedPassport);
            Assert.That(obtainedPerson.Name, Is.EqualTo(person.Name));
            Assert.That(obtainedPerson.Surname, Is.EqualTo(person.Surname));
            Assert.That(obtainedPassport.PassportNumber, Is.EqualTo(person.Passport.PassportNumber));
            Assert.That(obtainedPassport.OwnerId, Is.EqualTo(person.Identifier));
        }

        public class CustomerInt
        {
            [PrimaryKey] [AutoIncrement] public int Id { get; set; }

            public string Name { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
            public OrderInt[] Orders { get; set; }
        }

        [Table("Orders")] // 'Order' is a reserved keyword
        public class OrderInt
        {
            [PrimaryKey] [AutoIncrement] public int Id { get; set; }

            public float Amount { get; set; }
            public DateTime Date { get; set; }

            [ForeignKey(typeof(CustomerInt))] public int CustomerId { get; set; }

            [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public CustomerInt CustomerInt { get; set; }
        }

        [Test]
        public async Task TestOneToManyRecursiveInsertAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerInt>();
            await conn.DropTableAsync<OrderInt>();
            await conn.CreateTableAsync<CustomerInt>();
            await conn.CreateTableAsync<OrderInt>();

            var customer = new CustomerInt
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderInt { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderInt { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderInt { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderInt { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderInt { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertWithChildrenAsync(customer, true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerInt>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.CustomerInt);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.CustomerInt.Id);
                Assert.AreEqual(customer.Name, order.CustomerInt.Name);
                Assert.NotNull(order.CustomerInt.Orders);
                Assert.AreEqual(expectedOrders.Count, order.CustomerInt.Orders.Length);
            }
        }

        [Test]
        public async Task TestOneToManyRecursiveInsertOrReplaceAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerInt>();
            await conn.DropTableAsync<OrderInt>();
            await conn.CreateTableAsync<CustomerInt>();
            await conn.CreateTableAsync<OrderInt>();

            var customer = new CustomerInt
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderInt { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderInt { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderInt { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderInt { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderInt { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertOrReplaceWithChildrenAsync(customer);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerInt>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.CustomerInt);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.CustomerInt.Id);
                Assert.AreEqual(customer.Name, order.CustomerInt.Name);
                Assert.NotNull(order.CustomerInt.Orders);
                Assert.AreEqual(expectedOrders.Count, order.CustomerInt.Orders.Length);
            }

            var newCustomer = new CustomerInt
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderInt
                    {
                        Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15)
                    },
                    new OrderInt
                    {
                        Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1)
                    },
                    new OrderInt { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new OrderInt { Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new OrderInt { Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            await conn.InsertOrReplaceWithChildrenAsync(customer, true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<CustomerInt>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.CustomerInt);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.CustomerInt.Id);
                Assert.AreEqual(customer.Name, order.CustomerInt.Name);
                Assert.NotNull(order.CustomerInt.Orders);
                Assert.AreEqual(expectedOrders.Count, order.CustomerInt.Orders.Length);
            }
        }

        public class CustomerGuid
        {
            [PrimaryKey] public Guid Id { get; set; }

            public string Name { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
            public OrderGuid[] Orders { get; set; }
        }

        [Table("Orders")] // 'Order' is a reserved keyword
        public class OrderGuid
        {
            [PrimaryKey] public Guid Id { get; set; }

            public float Amount { get; set; }
            public DateTime Date { get; set; }

            [ForeignKey(typeof(CustomerGuid))] public Guid CustomerId { get; set; }

            [ManyToOne(CascadeOperations = CascadeOperation.CascadeInsert)]
            public CustomerGuid Customer { get; set; }
        }

        [Test]
        public async Task TestOneToManyRecursiveInsertGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid
                        { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertWithChildrenAsync(customer, true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        [Test]
        public async Task TestOneToManyRecursiveInsertOrReplaceGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid
                        { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            await conn.InsertOrReplaceWithChildrenAsync(customer, true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }

            var newCustomer = new CustomerGuid
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid
                        { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                    new OrderGuid
                        { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                    new OrderGuid
                        { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new OrderGuid
                        { Id = Guid.NewGuid(), Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            await conn.InsertOrReplaceWithChildrenAsync(customer, true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        /// <summary>
        ///     This test will validate the same scenario than TestOneToManyRecursiveInsert but inserting
        ///     one of the orders instead of the customer
        /// </summary>
        [Test]
        public async Task TestManyToOneRecursiveInsertAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerInt>();
            await conn.DropTableAsync<OrderInt>();
            await conn.CreateTableAsync<CustomerInt>();
            await conn.CreateTableAsync<OrderInt>();

            var customer = new CustomerInt
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderInt { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderInt { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderInt { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderInt { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderInt { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].CustomerInt = customer;
            await conn.InsertWithChildrenAsync(customer.Orders[0], true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerInt>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.CustomerInt);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.CustomerInt.Id);
                Assert.AreEqual(customer.Name, order.CustomerInt.Name);
                Assert.NotNull(order.CustomerInt.Orders);
                Assert.AreEqual(expectedOrders.Count, order.CustomerInt.Orders.Length);
            }
        }

        /// <summary>
        ///     This test will validate the same scenario than TestOneToManyRecursiveInsertOrReplace but inserting
        ///     one of the orders instead of the customer
        /// </summary>
        [Test]
        public async Task TestManyToOneRecursiveInsertOrReplaceAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerInt>();
            await conn.DropTableAsync<OrderInt>();
            await conn.CreateTableAsync<CustomerInt>();
            await conn.CreateTableAsync<OrderInt>();

            var customer = new CustomerInt
            {
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderInt { Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderInt { Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderInt { Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderInt { Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderInt { Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].CustomerInt = customer;
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerInt>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.CustomerInt);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.CustomerInt.Id);
                Assert.AreEqual(customer.Name, order.CustomerInt.Name);
                Assert.NotNull(order.CustomerInt.Orders);
                Assert.AreEqual(expectedOrders.Count, order.CustomerInt.Orders.Length);
            }

            var newCustomer = new CustomerInt
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderInt
                    {
                        Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15)
                    },
                    new OrderInt
                    {
                        Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1)
                    },
                    new OrderInt { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new OrderInt { Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new OrderInt { Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            // Insert any of the orders instead of the customer
            customer.Orders[0].CustomerInt = customer; // Required to complete the entity tree
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<CustomerInt>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.CustomerInt);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.CustomerInt.Id);
                Assert.AreEqual(customer.Name, order.CustomerInt.Name);
                Assert.NotNull(order.CustomerInt.Orders);
                Assert.AreEqual(expectedOrders.Count, order.CustomerInt.Orders.Length);
            }
        }

        /// <summary>
        ///     This test will validate the same scenario than TestOneToManyRecursiveInsertGuid but inserting
        ///     one of the orders instead of the customer
        /// </summary>
        [Test]
        public async Task TestManyToOneRecursiveInsertGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid
                        { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer; // Required to complete the entity tree
            await conn.InsertWithChildrenAsync(customer.Orders[0], true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        /// <summary>
        ///     This test will validate the same scenario than TestOneToManyRecursiveInsertOrReplaceGuid but inserting
        ///     one of the orders instead of the customer
        /// </summary>
        [Test]
        public async Task TestManyToOneRecursiveInsertOrReplaceGuidAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<CustomerGuid>();
            await conn.DropTableAsync<OrderGuid>();
            await conn.CreateTableAsync<CustomerGuid>();
            await conn.CreateTableAsync<OrderGuid>();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 25.7f, Date = new DateTime(2014, 5, 15, 11, 30, 15) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 15.2f, Date = new DateTime(2014, 3, 7, 13, 59, 1) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 0.5f, Date = new DateTime(2014, 4, 5, 7, 3, 0) },
                    new OrderGuid
                        { Id = Guid.NewGuid(), Amount = 106.6f, Date = new DateTime(2014, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 98f, Date = new DateTime(2014, 02, 1, 22, 31, 7) }
                }
            };

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer; // Required to complete the entity tree
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], true);

            var expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            var obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }

            var newCustomer = new CustomerGuid
            {
                Id = customer.Id,
                Name = "John Smith",
                Orders = new[]
                {
                    new OrderGuid
                        { Id = customer.Orders[0].Id, Amount = 15.7f, Date = new DateTime(2012, 5, 15, 11, 30, 15) },
                    new OrderGuid
                        { Id = customer.Orders[2].Id, Amount = 55.2f, Date = new DateTime(2012, 3, 7, 13, 59, 1) },
                    new OrderGuid
                        { Id = customer.Orders[4].Id, Amount = 4.5f, Date = new DateTime(2012, 4, 5, 7, 3, 0) },
                    new OrderGuid
                        { Id = Guid.NewGuid(), Amount = 206.6f, Date = new DateTime(2012, 7, 20, 21, 20, 24) },
                    new OrderGuid { Id = Guid.NewGuid(), Amount = 78f, Date = new DateTime(2012, 02, 1, 22, 31, 7) }
                }
            };

            customer = newCustomer;

            // Insert any of the orders instead of the customer
            customer.Orders[0].Customer = customer; // Required to complete the entity tree
            await conn.InsertOrReplaceWithChildrenAsync(customer.Orders[0], true);

            expectedOrders = customer.Orders.OrderBy(o => o.Date).ToDictionary(o => o.Id);

            obtainedCustomer = await conn.GetWithChildrenAsync<CustomerGuid>(customer.Id, true);
            Assert.NotNull(obtainedCustomer);
            Assert.NotNull(obtainedCustomer.Orders);
            Assert.AreEqual(expectedOrders.Count, obtainedCustomer.Orders.Length);

            foreach (var order in obtainedCustomer.Orders)
            {
                var expectedOrder = expectedOrders[order.Id];
                Assert.AreEqual(expectedOrder.Amount, order.Amount, 0.0001);
                Assert.AreEqual(expectedOrder.Date, order.Date);
                Assert.NotNull(order.Customer);
                Assert.AreEqual(customer.Id, order.CustomerId);
                Assert.AreEqual(customer.Id, order.Customer.Id);
                Assert.AreEqual(customer.Name, order.Customer.Name);
                Assert.NotNull(order.Customer.Orders);
                Assert.AreEqual(expectedOrders.Count, order.Customer.Orders.Length);
            }
        }

        public class TwitterUser
        {
            [PrimaryKey] [AutoIncrement] public int Id { get; set; }

            public string Name { get; set; }

            [ManyToMany(typeof(FollowerLeaderRelationshipTable), "LeaderId", "Followers",
                CascadeOperations = CascadeOperation.All)]
            public List<TwitterUser> FollowingUsers { get; set; }

            // ReadOnly is required because we're not specifying the followers manually, but want to obtain them from database
            [ManyToMany(typeof(FollowerLeaderRelationshipTable), "FollowerId", "FollowingUsers",
                CascadeOperations = CascadeOperation.CascadeRead, ReadOnly = true)]
            public List<TwitterUser> Followers { get; set; }

            public override bool Equals(object obj)
            {
                return obj is TwitterUser other && Name.Equals(other.Name);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public override string ToString()
            {
                return $"[TwitterUser: Id={Id}, Name={Name}]";
            }
        }

        // Intermediate class, not used directly anywhere in the code, only in ManyToMany attributes and table creation
        private class FollowerLeaderRelationshipTable
        {
            public int LeaderId { get; set; }
            public int FollowerId { get; set; }
        }

        [Test]
        public async Task TestManyToManyRecursiveInsertWithSameClassRelationshipAsync()
        {
            // We will configure the following scenario
            // 'John' follows 'Peter' and 'Thomas'
            // 'Thomas' follows 'John'
            // 'Will' follows 'Claire'
            // 'Claire' follows 'Will'
            // 'Jaime' follows 'Peter', 'Thomas' and 'Mark'
            // 'Mark' doesn't follow anyone
            // 'Martha' follows 'Anthony'
            // 'Anthony' follows 'Peter'
            // 'Peter' follows 'Martha'
            //
            // Then, we will insert 'Thomas' and we the other users will be inserted using cascade operations
            //
            // 'Followed by' branches will be ignored in the insert method because the property doesn't have the
            // 'CascadeInsert' operation and it's marked as ReadOnly
            //
            // We'll insert 'Jaime', 'Mark', 'Claire' and 'Will' manually because they're outside the 'Thomas' tree
            //
            // Cascade operations should stop once the user has been inserted once
            // So, more or less, the cascade operation tree will be the following (order may not match)
            // 'Thomas' |-(follows)>  'John' |-(follows)> 'Peter' |-(follows)> 'Martha' |-(follows)> 'Anthony' |-(follows)-> 'Peter'*
            //                               |-(follows)> 'Thomas'*
            //
            //
            // (*) -> Entity already inserted in a previous operation. Stop cascade insert

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<TwitterUser>();
            await conn.DropTableAsync<FollowerLeaderRelationshipTable>();
            await conn.CreateTableAsync<TwitterUser>();
            await conn.CreateTableAsync<FollowerLeaderRelationshipTable>();

            var john = new TwitterUser { Name = "John" };
            var thomas = new TwitterUser { Name = "Thomas" };
            var will = new TwitterUser { Name = "Will" };
            var claire = new TwitterUser { Name = "Claire" };
            var jaime = new TwitterUser { Name = "Jaime" };
            var mark = new TwitterUser { Name = "Mark" };
            var martha = new TwitterUser { Name = "Martha" };
            var anthony = new TwitterUser { Name = "anthony" };
            var peter = new TwitterUser { Name = "Peter" };

            john.FollowingUsers = new List<TwitterUser> { peter, thomas };
            thomas.FollowingUsers = new List<TwitterUser> { john };
            will.FollowingUsers = new List<TwitterUser> { claire };
            claire.FollowingUsers = new List<TwitterUser> { will };
            jaime.FollowingUsers = new List<TwitterUser> { peter, thomas, mark };
            mark.FollowingUsers = new List<TwitterUser>();
            martha.FollowingUsers = new List<TwitterUser> { anthony };
            anthony.FollowingUsers = new List<TwitterUser> { peter };
            peter.FollowingUsers = new List<TwitterUser> { martha };

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };

            // Only need to insert Jaime and Claire, the other users are contained in these trees
            await conn.InsertAllWithChildrenAsync(new[] { jaime, claire }, true);

            void CheckUser(TwitterUser expected, TwitterUser obtained)
            {
                Assert.NotNull(obtained, "User is null: {0}", expected.Name);
                Assert.AreEqual(expected.Name, obtained.Name);
                Assert.That(obtained.FollowingUsers, Is.EquivalentTo(expected.FollowingUsers));
                var followers = allUsers.Where(u => u.FollowingUsers.Contains(expected));
                Assert.That(obtained.Followers, Is.EquivalentTo(followers));
            }

            var obtainedThomas = await conn.GetWithChildrenAsync<TwitterUser>(thomas.Id, true);
            CheckUser(thomas, obtainedThomas);

            var obtainedJohn = obtainedThomas.FollowingUsers.FirstOrDefault(u => u.Id == john.Id);
            CheckUser(john, obtainedJohn);

            var obtainedPeter = obtainedJohn.FollowingUsers.FirstOrDefault(u => u.Id == peter.Id);
            CheckUser(peter, obtainedPeter);

            var obtainedMartha = obtainedPeter.FollowingUsers.FirstOrDefault(u => u.Id == martha.Id);
            CheckUser(martha, obtainedMartha);

            var obtainedAnthony = obtainedMartha.FollowingUsers.FirstOrDefault(u => u.Id == anthony.Id);
            CheckUser(anthony, obtainedAnthony);

            var obtainedJaime = obtainedThomas.Followers.FirstOrDefault(u => u.Id == jaime.Id);
            CheckUser(jaime, obtainedJaime);

            var obtainedMark = obtainedJaime.FollowingUsers.FirstOrDefault(u => u.Id == mark.Id);
            CheckUser(mark, obtainedMark);
        }

        [Test]
        public async Task TestManyToManyRecursiveDeleteWithSameClassRelationshipAsync()
        {
            // We will configure the following scenario
            // 'John' follows 'Peter' and 'Thomas'
            // 'Thomas' follows 'John'
            // 'Will' follows 'Claire'
            // 'Claire' follows 'Will'
            // 'Jaime' follows 'Peter', 'Thomas' and 'Mark'
            // 'Mark' doesn't follow anyone
            // 'Martha' follows 'Anthony'
            // 'Anthony' follows 'Peter'
            // 'Peter' follows 'Martha'
            //
            // Then, we will delete 'Thomas' and the other users will be deleted using cascade operations
            //
            // 'Followed by' branches will be ignored in the delete method because the property doesn't have the
            // 'CascadeDelete' operation and it's marked as ReadOnly
            //
            // 'Jaime', 'Mark', 'Claire' and 'Will' won't be deleted because they're outside the 'Thomas' tree
            //
            // Cascade operations should stop once the user has been marked for deletion once
            // So, more or less, the cascade operation tree will be the following (order may not match)
            // 'Thomas' |-(follows)>  'John' |-(follows)> 'Peter' |-(follows)> 'Martha' |-(follows)> 'Anthony' |-(follows)-> 'Peter'*
            //                               |-(follows)> 'Thomas'*
            //
            //
            // (*) -> Entity already marked for deletion in a previous operation. Stop cascade delete

            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<TwitterUser>();
            await conn.DropTableAsync<FollowerLeaderRelationshipTable>();
            await conn.CreateTableAsync<TwitterUser>();
            await conn.CreateTableAsync<FollowerLeaderRelationshipTable>();

            var john = new TwitterUser { Name = "John" };
            var thomas = new TwitterUser { Name = "Thomas" };
            var will = new TwitterUser { Name = "Will" };
            var claire = new TwitterUser { Name = "Claire" };
            var jaime = new TwitterUser { Name = "Jaime" };
            var mark = new TwitterUser { Name = "Mark" };
            var martha = new TwitterUser { Name = "Martha" };
            var anthony = new TwitterUser { Name = "anthony" };
            var peter = new TwitterUser { Name = "Peter" };

            john.FollowingUsers = new List<TwitterUser> { peter, thomas };
            thomas.FollowingUsers = new List<TwitterUser> { john };
            will.FollowingUsers = new List<TwitterUser> { claire };
            claire.FollowingUsers = new List<TwitterUser> { will };
            jaime.FollowingUsers = new List<TwitterUser> { peter, thomas, mark };
            mark.FollowingUsers = new List<TwitterUser>();
            martha.FollowingUsers = new List<TwitterUser> { anthony };
            anthony.FollowingUsers = new List<TwitterUser> { peter };
            peter.FollowingUsers = new List<TwitterUser> { martha };

            var allUsers = new[] { john, thomas, will, claire, jaime, mark, martha, anthony, peter };

            // Inserts all the objects in the database recursively
            await conn.InsertAllWithChildrenAsync(allUsers, true);

            // Deletes the entity tree starting at 'Thomas' recursively
            await conn.DeleteAsync(thomas, true);

            var expectedUsers = new[] { jaime, mark, claire, will };
            var existingUsers = await conn.Table<TwitterUser>().ToListAsync();

            // Check that the users have been deleted and only the users outside the 'Thomas' tree still exist
            Assert.That(existingUsers, Is.EquivalentTo(expectedUsers));
        }

        private class Teacher
        {
            [PrimaryKey] [AutoIncrement] public int Id { get; set; }

            public string Name { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.CascadeInsert)]
            public List<Student> Students { get; set; }
        }

        private class Student
        {
            [PrimaryKey] [AutoIncrement] public int Id { get; set; }

            public string Name { get; set; }

            [ManyToOne] public Teacher Teacher { get; set; }

            [TextBlob("AddressBlob")] public Address Address { get; set; }

            [ForeignKey(typeof(Teacher))] public int TeacherId { get; set; }

            public string AddressBlob { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
            public string Town { get; set; }
        }

        [Test]
        public async Task TestInsertTextBlobPropertiesRecursiveAsync()
        {
            var conn = Utils.CreateAsyncConnection();
            await conn.DropTableAsync<Student>();
            await conn.DropTableAsync<Teacher>();
            await conn.CreateTableAsync<Student>();
            await conn.CreateTableAsync<Teacher>();

            var teacher = new Teacher
            {
                Name = "John Smith",
                Students = new List<Student>
                {
                    new()
                    {
                        Name = "Bruce Banner",
                        Address = new Address
                        {
                            Street = "Sesame Street 5",
                            Town = "Gotham City"
                        }
                    },
                    new()
                    {
                        Name = "Peter Parker",
                        Address = new Address
                        {
                            Street = "Arlington Road 69",
                            Town = "Arkham City"
                        }
                    },
                    new()
                    {
                        Name = "Steve Rogers",
                        Address = new Address
                        {
                            Street = "28th Street 19",
                            Town = "New York"
                        }
                    }
                }
            };

            await conn.InsertWithChildrenAsync(teacher, true);

            foreach (var student in teacher.Students)
            {
                var dbStudent = await conn.GetWithChildrenAsync<Student>(student.Id);
                Assert.NotNull(dbStudent);
                Assert.NotNull(dbStudent.Address);
                Assert.AreEqual(student.Address.Street, dbStudent.Address.Street);
                Assert.AreEqual(student.Address.Town, dbStudent.Address.Town);
            }
        }
    }
}