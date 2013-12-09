// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

using Shaolinq.Persistence.Sql;

namespace Shaolinq.Postgres
{
	public static class PostgresConfiguration
	{
		public static DataAccessModelConfiguration CreateConfiguration(string databaseName, string serverName, string userId, string password)
		{
			return CreateConfiguration(databaseName, serverName, userId, password, true);
		}

		public static DataAccessModelConfiguration CreateConfiguration(string databaseName, string serverName, string userId, string password, bool poolConnections)
		{
			return CreateConfiguration(databaseName, serverName, userId, password, poolConnections, DatabaseReadMode.ReadWrite);
		}

		public static DataAccessModelConfiguration CreateConfiguration(string databaseName, string serverName, string userId, string password, bool poolConnections, DatabaseReadMode databaseReadMode)
		{
			return new DataAccessModelConfiguration()
			{
				DatabaseConnectionInfos = new DatabaseConnectionInfo[]
				{
					new PostgresDatabaseConnectionInfo()
					{
						DatabaseName = databaseName,
						DatabaseReadMode = databaseReadMode,
						ServerName = serverName,
						Pooling = true,
						UserId = userId,
						Password = password
					},
				}
			};
		}
	}
}
