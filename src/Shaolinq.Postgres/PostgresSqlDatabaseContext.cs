﻿// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Data.Common;
using System.Transactions;
using Npgsql;
﻿using Shaolinq.Persistence;
using Shaolinq.Postgres.Shared;

﻿namespace Shaolinq.Postgres
{
	public class PostgresSqlDatabaseContext
		: SqlDatabaseContext
	{
		public int Port { get; set; }
		public string Host { get; set; }
		public string Userid { get; set; }
		public string Password { get; set; }
		
		public static PostgresSqlDatabaseContext Create(PostgresDatabaseContextInfo contextInfo, DataAccessModel model)
		{
			var constraintDefaults = model.Configuration.ConstraintDefaults;
			var sqlDialect = PostgresSharedSqlDialect.Default;
			var sqlDataTypeProvider = new PostgresSharedSqlDataTypeProvider(constraintDefaults, contextInfo.NativeUuids, contextInfo.DateTimeKindIfUnspecified);
			var sqlQueryFormatterManager = new DefaultSqlQueryFormatterManager(sqlDialect, sqlDataTypeProvider, (options, sqlDataTypeProviderArg, sqlDialectArg) => new PostgresSharedSqlQueryFormatter(options, sqlDataTypeProviderArg, sqlDialectArg, contextInfo.SchemaName));

			return new PostgresSqlDatabaseContext(model, sqlDialect, sqlDataTypeProvider, sqlQueryFormatterManager, contextInfo);
		}

		protected PostgresSqlDatabaseContext(DataAccessModel model, SqlDialect sqlDialect, SqlDataTypeProvider sqlDataTypeProvider, SqlQueryFormatterManager sqlQueryFormatterManager, PostgresDatabaseContextInfo contextInfo)
			: base(model, sqlDialect, sqlDataTypeProvider, sqlQueryFormatterManager, contextInfo.DatabaseName, contextInfo)
		{
			this.Host = contextInfo.ServerName;
			this.Userid = contextInfo.UserId;
			this.Password = contextInfo.Password;
			this.Port = contextInfo.Port;
			this.CommandTimeout = TimeSpan.FromSeconds(contextInfo.CommandTimeout);

			this.ConnectionString = String.Format("Host={0};User Id={1};Password={2};Database={3};Port={4};Pooling={5};MinPoolSize={6};MaxPoolSize={7};Enlist=false;Timeout={8};CommandTimeout={9}", contextInfo.ServerName, contextInfo.UserId, contextInfo.Password, contextInfo.DatabaseName, contextInfo.Port, contextInfo.Pooling, contextInfo.MinPoolSize, contextInfo.MaxPoolSize, contextInfo.ConnectionTimeout, contextInfo.CommandTimeout);
			this.ServerConnectionString = String.Format("Host={0};User Id={1};Password={2};Port={4};Pooling={5};MinPoolSize={6};MaxPoolSize={7};Enlist=false;Timeout={8};CommandTimeout={9}", contextInfo.ServerName, contextInfo.UserId, contextInfo.Password, contextInfo.DatabaseName, contextInfo.Port, contextInfo.Pooling, contextInfo.MinPoolSize, contextInfo.MaxPoolSize, contextInfo.ConnectionTimeout, contextInfo.CommandTimeout);

			this.SchemaManager = new PostgresSharedSqlDatabaseSchemaManager(this);
		}

		public override SqlDatabaseTransactionContext CreateDatabaseTransactionContext(Transaction transaction)
		{
			return new PostgresSqlDatabaseTransactionContext(this, transaction);
		}

		public override DbProviderFactory CreateDbProviderFactory()
		{
			return NpgsqlFactory.Instance;
		}

		public override IDisabledForeignKeyCheckContext AcquireDisabledForeignKeyCheckContext(SqlDatabaseTransactionContext sqlDatabaseTransactionContext)
		{
			return new DisabledForeignKeyCheckContext(sqlDatabaseTransactionContext);	
		}

		public override void DropAllConnections()
		{
			NpgsqlConnection.ClearAllPools();
		}

		public override string GetRelatedSql(Exception e)
		{
			var postgresException = e as NpgsqlException;

			if (postgresException == null)
			{
				return base.GetRelatedSql(e);
			}

			return postgresException.ErrorSql;
		}

		public override Exception DecorateException(Exception exception, string relatedQuery)
		{
			var typedException = exception as NpgsqlException;

			if (typedException == null)
			{
				return base.DecorateException(exception, relatedQuery);
			}

			if (typedException.Code == "23505")
			{
				return new UniqueKeyConstraintException(typedException, relatedQuery);
			}
			
			return new DataAccessException(typedException, relatedQuery);
		}
	}
}
