using System;
using System.Threading.Tasks;
using Npgsql;

namespace DbCheck
{
	class DbChecker
	{
		static async Task<int> Main(string[] args)
		{
			var connString = "Host=dpg-d42ra5mr433s73du78n0-a.singapore-postgres.render.com;Port=5432;Database=room_for_lease_api;Username=room_for_lease_api_user;Password=qYcL3VO8XSp19sAAhVQoACscNZt9LMPt;SSL Mode=Require;Trust Server Certificate=true;";

			try
			{
				Console.WriteLine("Attempting to open connection to PostgreSQL...");
				await using var conn = new NpgsqlConnection(connString);
				await conn.OpenAsync();
				Console.WriteLine("Connection succeeded.");
				if (conn.PostgresParameters.TryGetValue("server_version", out var ver))
				{
					Console.WriteLine($"Postgres version: {ver}");
				}
				await conn.CloseAsync();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Connection failed:");
				Console.WriteLine(ex.ToString());
				return 2;
			}
		}
	}
}
