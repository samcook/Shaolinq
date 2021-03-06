﻿// Copyright (c) 2007-2018 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;
using Shaolinq.Tests.TestModel;

namespace Shaolinq.Tests
{
	[TestFixture("MySql")]
	[TestFixture("Postgres")]
	[TestFixture("Postgres.DotConnect")]
	[TestFixture("Postgres.DotConnect.Unprepared")]
	[TestFixture("SqlServer")]
	[TestFixture("Sqlite")]
	[TestFixture("Sqlite:DataAccessScope")]
	[TestFixture("SqliteInMemory")]
	[TestFixture("SqliteClassicInMemory")]
	public class TransactionTests
		: BaseTests<TestDataAccessModel>
	{
		public TransactionTests(string providerName)
			: base(providerName)
		{
		}

		[Test]
		public void Test_Create_Object()
		{
			using (var scope = new TransactionScope())
			{
				var school = this.model.Schools.Create();
				
				school.Name = "Kung Fu School";

				var student = this.model.Students.Create();

				student.Firstname = "Bruce";
				student.Lastname = "Lee";
				student.School = school;

				scope.Complete();
			}

			using (var scope = new TransactionScope())
			{
				var student = this.model.Students.First(c => c.Firstname == "Bruce");

				Assert.AreEqual("Bruce Lee", student.Fullname);

				scope.Complete();
			}
		}

		[Test]
		public void Test_Create_Object_And_Abort()
		{
			using (var scope = new TransactionScope())
			{
				var school = this.model.Schools.Create();
				var student = school.Students.Create();

				student.Firstname = "StudentThatShouldNotExist";
			}

			using (var scope = new TransactionScope())
			{
				Assert.Catch<InvalidOperationException>(() => this.model.Students.First(c => c.Firstname == "StudentThatShouldNotExist"));
			}
		}

		[Test]
		public void Test_Create_Object_And_Flush_Then_Abort()
		{
			using (var scope = new TransactionScope())
			{
				var school = this.model.Schools.Create();
				var student = school.Students.Create();

				student.Firstname = "StudentThatShouldNotExist";

				scope.Flush();

				Assert.IsNotNull(this.model.Students.FirstOrDefault(c => c.Firstname == "StudentThatShouldNotExist"));
			}

			using (var scope = new TransactionScope())
			{
				Assert.Catch<InvalidOperationException>(() => this.model.Students.First(c => c.Firstname == "StudentThatShouldNotExist"));
			}
		}

		[Test]
		public void Test_Multiple_Updates_In_Single_Transaction()
		{
			var address1Name = Guid.NewGuid().ToString();
			var address2Name = Guid.NewGuid().ToString();

			// Create some objects
			using (var scope = new TransactionScope())
			{
				var address1 = this.model.Address.Create();
				address1.Country = address1Name;

				var address2 = this.model.Address.Create();
				address2.Country = address2Name;

				scope.Flush();

				Console.WriteLine("Address1 Id: {0}", address1.Id);
				Console.WriteLine("Address2 Id: {0}", address2.Id);
				
				scope.Complete();
			}

			// Update them
			using (var scope = new TransactionScope())
			{
				var address1 = this.model.Address.Single(x => x.Country == address1Name);
				var address2 = this.model.Address.Single(x => x.Country == address2Name);

				Console.WriteLine("Address1 Id: {0}", address1.Id);
				Console.WriteLine("Address2 Id: {0}", address2.Id);

				address1.Street = "Street1";
				address2.Street = "Street2";

				Console.WriteLine("Address1 changed: {0}", ((IDataAccessObjectAdvanced)address1).HasObjectChanged);
				Console.WriteLine("Address2 changed: {0}", ((IDataAccessObjectAdvanced)address2).HasObjectChanged);

				scope.Complete();
			}

			// Check they were both updated
			using (var scope = new TransactionScope())
			{
				var address1 = this.model.Address.Single(x => x.Country == address1Name);
				var address2 = this.model.Address.Single(x => x.Country == address2Name);

				Assert.That(address1.Street, Is.EqualTo("Street1"));
				Assert.That(address2.Street, Is.EqualTo("Street2"));
			}
		}

		[Test]
		public void Test_Insert_And_Nplus1_Query_Async()
		{
			Test_Insert_And_Nplus1_Query_Async_Private().Wait();
		}

		private async Task Test_Insert_And_Nplus1_Query_Async_Private()
		{
			long schoolId;

			using (var scope = DataAccessScope.CreateReadCommitted())
			{
				var school = this.model.Schools.Create();

				var s1 = school.Students.Create();
				
				await scope.FlushAsync().ContinueOnAnyContext();

				schoolId = school.Id;

				await scope.CompleteAsync().ContinueOnAnyContext();
			}

			using (var scope = DataAccessScope.CreateReadCommitted())
			{
				var school = await this.model.Schools.SingleAsync(c => c.Id == schoolId);

				var x = await school.Students.ToListAsync().ContinueOnAnyContext();

				var newSchool = this.model.Schools.Create();

				var y = await school.Students.ToListAsync();

				Assert.AreEqual(1, y.Count);

				var s1 = await school.Students.FirstAsync();

				school.Name = "Hello";

				await scope.FlushAsync();

				var y2 = await school.Students.ToListAsync().ContinueOnAnyContext();
				var s2 = await school.Students.SingleAsync().ContinueOnAnyContext();
				var newSchool2 = this.model.Schools.Create();

				await scope.CompleteAsync().ContinueOnAnyContext();
			}
		}

		[Test]
		public void Test_AsyncSelect()
		{
			Test_AsyncSelect_Private().Wait();
		}

		private async Task Test_AsyncSelect_Private()
		{
			using (var scope = new TransactionScope())
			{
				var school = this.model.Schools.Create();

				school.Name = "Kung Fu School";

				var student = this.model.Students.Create();

				student.Firstname = "Bruce";
				student.Lastname = "Lee";
				student.School = school;

				scope.Complete();
			}

			await AsyncMethod();
			await AsyncMethod();
		} 

		private Task<Student> AsyncMethod()
		{
			var student = this.model.Students.First();

			Assert.AreEqual("Bruce Lee", student.Fullname);

			return Task.FromResult(student);
		}

		[Test]
		[Category("IgnoreOnMono")]
		public void Test_Async_TransactionScope()
		{
			Test_Async_TransactionScope_Private().Wait();
		}

		private async Task Test_Async_TransactionScope_Private()
		{
			using (var scope = TransactionScopeFactory.CreateReadCommitted(TransactionScopeOption.Required, null, TransactionScopeAsyncFlowOption.Enabled))
			{ 
				var address = this.model.Address.Create();

				address.Street = "Async Street";

				var task = Task.Delay(100);

				await task;

				scope.Complete();
			}
		}
	}
}
