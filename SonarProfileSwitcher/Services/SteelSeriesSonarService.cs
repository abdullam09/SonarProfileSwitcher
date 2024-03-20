using Microsoft.Data.Sqlite;
using SonarProfileSwitcher.Interfaces;
using SonarProfileSwitcher.Models;
using SonarProfileSwitcher.Services.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SonarProfileSwitcher.Services
{
    internal class SteelSeriesSonarService : ISteelSeriesSonarService
    {
        private readonly string _connectionString;
        private int? _lastWorkingPort;

        public SteelSeriesSonarService()
        {
            _connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    @"SteelSeries\GG\apps\sonar\db\database.db")
            }.ToString();
        }

        public static SteelSeriesSonarService Instance { get; } = new();

        public IEnumerable<SonarGamingConfiguration> AvailableGamingConfigurations =>
            GetGamingConfigurations().OrderBy(s => s.Name);

        public IEnumerable<SonarGamingConfiguration> GetGamingConfigurations()
        {
            // Get all the available profiles from SQLite
            using var sqliteConnection = new SqliteConnection(_connectionString);
            sqliteConnection.Open();

            using SqliteCommand sqliteCommand = sqliteConnection.CreateCommand();
            sqliteCommand.CommandText = "select id, name, vad from configs where vad == 1";
            using SqliteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
            while (sqliteDataReader.Read())
            {
                string id = sqliteDataReader.GetString(0);
                string name = sqliteDataReader.GetString(1);
                yield return new SonarGamingConfiguration(id, name);
            }
        }

        public async Task ChangeSelectedGamingConfiguration(SonarGamingConfiguration sonarGamingConfiguration,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(sonarGamingConfiguration.Id))
                return;

            Process[] processesByName = Process.GetProcessesByName("SteelSeriesSonar");
            if (processesByName.Length <= 0 || cancellationToken.IsCancellationRequested)
                return;

            IEnumerable<int> potentialPorts = processesByName.SelectMany(p => NetworkHelper.GetPortById(p.Id, false));

            potentialPorts = _lastWorkingPort != null ? potentialPorts.Prepend(_lastWorkingPort.Value) : potentialPorts;
            _lastWorkingPort = null;

            using var httpClient = new HttpClient();
            foreach (int potentialPort in potentialPorts.Distinct())
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                HttpResponseMessage? httpResponseMessage = await httpClient.PutAsync(
                    $"http://localhost:{potentialPort}/configs/{sonarGamingConfiguration.Id}/select",//50773
                    new StringContent(""),
                    cancellationToken).ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);
                if (httpResponseMessage?.StatusCode == HttpStatusCode.OK)
                {
                    _lastWorkingPort = potentialPort;
                    break;
                }
            }

            //StateManager.Instance.GetOrLoadState<HomeViewModel>().ActiveProfile = sonarGamingConfiguration;
        }

        public string GetSelectedGamingConfiguration()
        {
            // Get all the available profiles from SQLite
            using var sqliteConnection = new SqliteConnection(_connectionString);
            sqliteConnection.Open();

            using SqliteCommand sqliteCommand = sqliteConnection.CreateCommand();
            sqliteCommand.CommandText = "select config_id, vad from selected_config where vad == 1";
            using SqliteDataReader sqliteDataReader = sqliteCommand.ExecuteReader();
            if (!sqliteDataReader.Read())
                throw new InvalidOperationException("Unable to check for selected gaming profile");
            return sqliteDataReader.GetString(0);
        }
    }
}
