﻿// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

 using System;
﻿using System.Reflection;
﻿using Shaolinq.MySql;
﻿using Shaolinq.Postgres;
﻿using Shaolinq.Postgres.DotConnect;
﻿using Shaolinq.Sqlite;
using Shaolinq.Tests.DataModels.Test;
using log4net.Config;

namespace Shaolinq.Tests
{
	public class BaseTests
	{
		protected TestDataAccessModel model;

		protected DataAccessModelConfiguration CreateMySqlConfiguration(string databaseName)
		{
			return MySqlConfiguration.CreateConfiguration(databaseName, "localhost", "root", "root");
		}

		protected DataAccessModelConfiguration CreateSqliteConfiguration(string databaseName)
		{
			return SqliteConfiguration.CreateConfiguration(databaseName + ".db");
		}

		protected DataAccessModelConfiguration CreatePostgresConfiguration(string databaseName)
		{
			return PostgresConfiguration.CreateConfiguration(databaseName, "localhost", "postgres", "postgres");
		}

		protected DataAccessModelConfiguration CreatePostgresDotConnectConfiguration(string databaseName)
		{
			return PostgresDotConnectConfiguration.CreateConfiguration("DotConnect" + databaseName, "localhost", "postgres", "postgres");
		}

		protected DataAccessModelConfiguration CreateConfiguration(string providerName, string databaseName)
		{
			var methodInfo = this.GetType().GetMethod("Create" + providerName.Replace(".", "") + "Configuration", BindingFlags.Instance | BindingFlags.NonPublic);

			return (DataAccessModelConfiguration)methodInfo.Invoke(this, new object[] { databaseName });
		}

		protected string ProviderName
		{
			get;
			private set;
		}

		private readonly DataAccessModelConfiguration configuration;

		public BaseTests(string providerName)
		{
			this.ProviderName = providerName;

			XmlConfigurator.Configure();

			try
			{
				configuration = this.CreateConfiguration(providerName, this.GetType().Name);
				model = DataAccessModel.BuildDataAccessModel<TestDataAccessModel>(configuration);
				model.CreateDatabase(DatabaseCreationOptions.DeleteExisting);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Console.WriteLine(e.StackTrace);

				throw;
			}
		}
	}
}
