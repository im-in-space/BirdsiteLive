﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.DAL.Postgres.DataAccessLayers.Base;
using BirdsiteLive.DAL.Postgres.Settings;
using Dapper;
using Newtonsoft.Json;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers
{
    public class FollowersPostgresDal : PostgresBase, IFollowersDal
    {
        #region Ctor
        public FollowersPostgresDal(PostgresSettings settings) : base(settings)
        {
            
        }
        #endregion

        public async Task CreateFollowerAsync(string acct, string host, int[] followings, Dictionary<int, long> followingSyncStatus)
        {
            var serializedDic = JsonConvert.SerializeObject(followingSyncStatus);

            acct = acct.ToLowerInvariant();
            host = host.ToLowerInvariant();

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.ExecuteAsync(
                    $"INSERT INTO {_settings.FollowersTableName} (acct,host,followings,followingsSyncStatus) VALUES(@acct,@host,@followings, CAST(@followingsSyncStatus as json))",
                    new { acct, host, followings, followingsSyncStatus = serializedDic });
            }
        }

        public async Task<Follower> GetFollowerAsync(string acct, string host)
        {
            var query = $"SELECT * FROM {_settings.FollowersTableName} WHERE acct = @acct AND host = @host";

            acct = acct.ToLowerInvariant();
            host = host.ToLowerInvariant();

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<SerializedFollower>(query, new { acct, host })).FirstOrDefault();
                return Convert(result);
            }
        }

        public async Task<Follower[]> GetFollowersAsync(int followedUserId)
        {
            if (followedUserId == default) throw new ArgumentException("followedUserId");

            var query = $"SELECT * FROM {_settings.FollowersTableName} WHERE @id=ANY(followings)";
            
            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<SerializedFollower>(query, new { id = followedUserId});
                return result.Select(Convert).ToArray();
            }
        }

        public async Task UpdateFollowerAsync(int id, int[] followings, Dictionary<int, long> followingsSyncStatus)
        {
            if (id == default) throw new ArgumentException("id");

            var serializedDic = JsonConvert.SerializeObject(followingsSyncStatus);
            var query = $"UPDATE {_settings.FollowersTableName} SET followings = @followings, followingsSyncStatus =  CAST(@followingsSyncStatus as json) WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { id, followings, followingsSyncStatus = serializedDic });
            }
        }

        public async Task DeleteFollowerAsync(int id)
        {
            if (id == default) throw new ArgumentException("id");
            
            var query = $"DELETE FROM {_settings.FollowersTableName} WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { id });
            }
        }

        public async Task DeleteFollowerAsync(string acct, string host)
        {
            if (acct == default) throw new ArgumentException("acct");
            if (host == default) throw new ArgumentException("host");

            acct = acct.ToLowerInvariant();
            host = host.ToLowerInvariant();

            var query = $"DELETE FROM {_settings.FollowersTableName} WHERE acct = @acct AND host = @host";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { acct, host });
            }
        }

        private Follower Convert(SerializedFollower follower)
        {
            if (follower == null) return null;

            return new Follower()
            {
                Id = follower.Id,
                Acct = follower.Acct,
                Host = follower.Host,
                Followings = follower.Followings,
                FollowingsSyncStatus = JsonConvert.DeserializeObject<Dictionary<int,long>>(follower.FollowingsSyncStatus)
            };
        }
    }

    internal class SerializedFollower {
        public int Id { get; set; }

        public int[] Followings { get; set; }
        public string FollowingsSyncStatus { get; set; }

        public string Acct { get; set; }
        public string Host { get; set; }
    }
}