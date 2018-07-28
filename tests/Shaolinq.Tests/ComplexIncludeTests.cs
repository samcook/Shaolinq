﻿// Copyright (c) 2007-2017 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform;
using Shouldly;
using Shaolinq.Persistence.Linq;
using Shaolinq.Tests.ComplexPrimaryKeyModel;

namespace Shaolinq.Tests
{
	[TestFixture("MySql")]
	[TestFixture("Sqlite")]
	[TestFixture("SqliteInMemory")]
	[TestFixture("SqliteClassicInMemory")]
	[TestFixture("Sqlite:DataAccessScope")]
	[TestFixture("SqlServer:DataAccessScope")]
	[TestFixture("Postgres")]
	[TestFixture("Postgres.DotConnect")]
	public class ComplexIncludeTests
		: BaseTests<ComplexPrimaryKeyDataAccessModel>
	{
		private const int seed = 327328;
		private readonly Random random = new Random(seed);
		private readonly Dictionary<string, int> numberOfShopsPerMall = new Dictionary<string, int>();

		public ComplexIncludeTests(string providerName)
			: base(providerName)
		{
			Console.WriteLine($"Starting random seed: {seed}");
		}

		public void Shuffle<T>(IList<T> list)  
		{  
			var n = list.Count;  

			for(var i = list.Count - 1; i > 1; i--)
			{
				var rnd = random.Next(i + 1);  

				var value = list[rnd];  

				list[rnd] = list[i];  
				list[i] = value;
			}
		}

		private IEnumerable<int> CreateLinearRandomSequence(int minimumValue, int maximumValue)
		{
			var used = new HashSet<int>();

			for (var i = minimumValue; i <= maximumValue; )
			{
				var x = this.random.Next(minimumValue, maximumValue + 1);

				if (used.Contains(x))
				{
					continue;
				}

				used.Add(x);

				yield return x;

				i++;
			}
		}

		private string GetMallName(int i)
		{
			return $"Mall {i:00}";
		}

		[OneTimeSetUp]
		public void SetUpFixture()
		{
			using (var scope = NewTransactionScope())
			{
				var numberOfMalls = 20;
				
				foreach (var i in CreateLinearRandomSequence(1, numberOfMalls))
				{
					var mall = this.model.Malls.Create();
					var building = this.model.Buildings.Create();

					building.Name = $"Building";

					mall.Building = building;
					mall.Name =  GetMallName(i);
					
					var numberOfShops = random.Next(1, 10);

					this.numberOfShopsPerMall[mall.Name] = numberOfShops;
				}

				scope.Flush();

				// Create shops in random order

				var shopsToCreate = new List<Tuple<int, int>>();

				for (var i = 1; i <= numberOfMalls; i++)
				{
					for (var j = 1; j <= numberOfShopsPerMall[GetMallName(i)]; j++)
					{
						shopsToCreate.Add(new Tuple<int, int>(i, j));
					}
				}

				Shuffle(shopsToCreate);

				foreach (var shopToCreate in shopsToCreate)
				{
					var mallName = GetMallName(shopToCreate.Item1);
					var mall = this.model.Malls.Single(c => c.Name == mallName);

					var shop = mall.Shops.Create();

					shop.Address = this.model.Addresses.Create();
					shop.Address.Region = this.model.Regions.Create();
					shop.Address.Region.Name = "Region for shop {j:00}";

					shop.Name = $"Shop {shopToCreate.Item2:00} for {mall.Name}";
				}
				
				scope.Complete();
			}

			foreach (var i in CreateLinearRandomSequence(1, 20))
			{
				var mall = this.model.Malls.Single(c => c.Name == $"Mall {i:00}");
			}
		}

		[TestCase(true, 10)]
		[TestCase(false, 10)]
		[TestCase(true, 7)]
		[TestCase(false, 7)]
		public void Test(bool useInclude, int take)
		{
			IQueryable<Mall> queryable = this.model.Malls;

			if (useInclude)
			{
				queryable = queryable.Include(c => c.Shops);
			}

			queryable  = queryable
				.OrderBy(c => c.Building.Name)
				.Where(c => c.Building.Name != "")
				.Skip(0)
				.Take(take); // Take is essential for causing a failure

			var malls = queryable.ToList();

			foreach (var mall in malls.OrderBy(c => c.Name))
			{
				Console.WriteLine($"{mall.Name} [expected shop count: {numberOfShopsPerMall[mall.Name]}]");

				var shops = useInclude ? mall.Shops.Items() : mall.Shops.ToList();

				shops.Count.ShouldBe(numberOfShopsPerMall[mall.Name]);
				shops.Count.ShouldBe(mall.Shops.Count());

				shops.OrderBy(c => c.Name).Select((c,i) => new { name = c.Name, i = i + 1 }).ShouldAllBe(c => c.name.StartsWith($"Shop {c.i:00}"));

				foreach (var shop in shops)
				{
					Console.WriteLine($"{shop.Name}");
				}
			}
		}

		[TestCase(true, 10)]
		[TestCase(false, 10)]
		[TestCase(true, 7)]
		[TestCase(false, 7)]
		public void Test2(bool useInclude, int take)
		{
			IQueryable<Mall> queryable = this.model.Malls;

			if (useInclude)
			{
				queryable = queryable.Include(c => c.Shops).Include(c => c.Building);
			}

			var queryable2  = queryable
				.OrderBy(c => c.Building.Name)
				.Select(c => c)
				.Where(c => c.Building.Name != "")
				.Skip(0)
				.Take(take); // Take is essential for causing a failure

			var malls = queryable2.ToList();

			foreach (var mall in malls.OrderBy(c => c.Name))
			{
				Console.WriteLine($"{mall.Name} [expected shop count: {numberOfShopsPerMall[mall.Name]}]");

				var shops = useInclude ? mall.Shops.Items() : mall.Shops.ToList();

				shops.Count.ShouldBe(numberOfShopsPerMall[mall.Name]);
				shops.Count.ShouldBe(mall.Shops.Count());

				shops.OrderBy(c => c.Name).Select((c,i) => new { name = c.Name, i = i + 1 }).ShouldAllBe(c => c.name.StartsWith($"Shop {c.i:00}"));

				foreach (var shop in shops)
				{
					Console.WriteLine($"{shop.Name}");
				}
			}
		}

		[TestCase(true, 10)]
		[TestCase(false, 10)]
		[TestCase(true, 7)]
		[TestCase(false, 7)]
		public void Test3(bool useInclude, int take)
		{
			IQueryable<Mall> queryable = this.model.Malls;

			if (useInclude)
			{
				queryable = queryable.Include(c => c.Shops);
			}

			var queryable2  = queryable
				.OrderBy(c => c.Building.Name)
				.Select(c => new { a1 = new { a2 = c } })
				.Select(c => new { a3 = c.a1 })
				.Where(c => c.a3.a2.Building.Name != "")
				.Skip(0)
				.Take(take); // Take is essential for causing a failure

			var malls = queryable2.ToList().Select(c => c.a3.a2);

			foreach (var mall in malls.OrderBy(c => c.Name))
			{
				Console.WriteLine($"{mall.Name} [expected shop count: {numberOfShopsPerMall[mall.Name]}]");

				var shops = useInclude ? mall.Shops.Items() : mall.Shops.ToList();

				shops.Count.ShouldBe(numberOfShopsPerMall[mall.Name]);
				shops.Count.ShouldBe(mall.Shops.Count());

				shops.OrderBy(c => c.Name).Select((c,i) => new { name = c.Name, i = i + 1 }).ShouldAllBe(c => c.name.StartsWith($"Shop {c.i:00}"));

				foreach (var shop in shops)
				{
					Console.WriteLine($"{shop.Name}");
				}
			}
		}

		[TestCase(true, 10)]
		[TestCase(false, 10)]
		[TestCase(true, 7)]
		[TestCase(false, 7)]
		public void Test4(bool useInclude, int take)
		{
			IQueryable<Mall> queryable = this.model.Malls;

			if (useInclude)
			{
				queryable = queryable.Include(c => c.Shops);
			}

			var queryable2  = queryable
				.OrderBy(c => c.Building.Name)
				.Select(c => new { a1 = new { a2 = c } })
				.Select(c => new { a3 = c})
				.Select(c => c.a3.a1.a2.Name)
				.Skip(0)
				.Take(take);

			var malls = queryable2.ToList();
		}
	}
}
