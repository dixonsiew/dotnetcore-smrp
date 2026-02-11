using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using smrp.Controllers.Report.MasterPD101;
using smrp.Dtos;
using smrp.Models;
using smrp.Services;
using smrp.sql;
using smrp.Utils;
using System.Security.Claims;

namespace smrp.Controllers.Report
{
    [Route("api/master-pd101")]
    [ApiController]
    public class MasterPD101Controller : ControllerBase
    {
        private readonly RsConnection rscon;
        private readonly IConfiguration config;
        private readonly IMongoClient client;
        private readonly UserService userService;

        public MasterPD101Controller(DefaultConnection conn, RsConnection rsconn, IConfiguration cfg, IMongoClient cli)
        {
            rscon = rsconn;
            config = cfg;
            client = cli;
            userService = new UserService(conn);
        }

        [HttpGet("rpt1")]
        [Authorize]
        public async Task<IResult> List(
            [FromQuery(Name = "_page")] string page = "1",
            [FromQuery(Name = "_limit")] string limit = "20",
            [FromQuery(Name = "vt")] string vt = "0",
            [FromQuery(Name = "datefrom")] string datefrom = "",
            [FromQuery(Name = "dateto")] string dateto = "")
        {
            IResult res = Results.Json(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "User not found",
            }, statusCode: StatusCodes.Status404NotFound);

            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return res;
            }

            var filter = Builders<BsonDocument>.Filter.Empty;
            var db = GetDb(client, vt);
            var col = db.GetCollection<BsonDocument>($"__{username}__");
            var col2 = db.GetCollection<BsonDocument>($"__{username}-q__");
            long total = await col.CountDocumentsAsync(new BsonDocument());

            string dateFrom = datefrom;
            string dateTo = dateto;
            long t2 = await col2.CountDocumentsAsync(filter);
            if (t2 > 0)
            {
                var ld = await col2.Find(filter).ToListAsync();
                dateFrom = ld[0]["datefrom"].AsString;
                dateTo = ld[0]["dateto"].AsString;
            }

            var pg = new Pager(Convert.ToInt32(total), Convert.ToInt32(page), Convert.ToInt32(limit)); 
            var lx = await col.Find(filter).Skip(pg.LowerBound).Limit(pg.PageSize).ToListAsync();
            var ls = Helper.ProcessDoc(lx);
            return Results.Ok(new
            {
                columnmaps = RptColMap.COLUMN_MAP,
                total_count = total,
                total_page = pg.TotalPages,
                page = pg.PageNum,
                data = ls.Select(k => k.ToDictionary()).ToList(),
                datefrom = dateFrom,
                dateto = dateTo,
            });
        }

        [HttpPost("rpt1")]
        [Authorize]
        public async Task<IResult> SearchList(ReportQueryDto data)
        {
            IResult res = Results.Json(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "User not found",
            }, statusCode: StatusCodes.Status404NotFound);

            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return res;
            }

            var md = await QueryAndSaveAsync(data, username);
            return Results.Ok(md);
        }

        [HttpGet("rpt1/{id}")]
        [Authorize]
        public async Task<IResult> Edit(string id, [FromQuery(Name = "vt")] string vt = "0")
        {
            IResult res = Results.Json(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "User not found",
            }, statusCode: StatusCodes.Status404NotFound);

            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return res;
            }

            var col = GetCollection(client, username, vt);
            var objectId = new ObjectId(id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            var lx = await col.Find(filter).ToListAsync();
            var ls = Helper.ProcessDoc(lx);

            if (ls.Count > 0)
            {
                return Results.Ok(ls[0].ToDictionary());
            }

            return Results.Json(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "Record not found",
            }, statusCode: StatusCodes.Status404NotFound);
        }

        [HttpPost("rpt1/{id}")]
        [Authorize]
        public async Task<IResult> Update(Dictionary<string, object> data, string id, [FromQuery(Name = "vt")] string vt = "0")
        {
            IResult res = Results.Json(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "User not found",
            }, statusCode: StatusCodes.Status404NotFound);

            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return res;
            }

            var col = GetCollection(client, username, vt);
            var objectId = new ObjectId(id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            var update = Builders<BsonDocument>.Update;
            var updateDefinitions = new List<UpdateDefinition<BsonDocument>>();
            foreach (var kvp in data)
            {
                updateDefinitions.Add(update.Set(kvp.Key, kvp.Value));
            }

            var combinedUpdate = Builders<BsonDocument>.Update.Combine(updateDefinitions);
            await col.FindOneAndUpdateAsync(filter, combinedUpdate);
            return Results.Ok(new
            {
                success = 1,
            });
        }

        private IMongoCollection<BsonDocument> GetCollection(IMongoClient cli, string username, string vt)
        {
            var db = GetDb(cli, vt);
            var s = $"__{username}__";
            return db.GetCollection<BsonDocument>(s);
        }

        private IMongoDatabase GetDb(IMongoClient cli, string vt)
        {
            string suffix = "";
            IMongoDatabase db;
            if (config["mongodb.prefix"] == "prod")
            {
                suffix = "_prod";
            }

            if (vt == "0")
            {
                var s = $"master_pd101{suffix}";
                db = cli.GetDatabase(s);
            }

            else
            {
                var s = $"master_rh101{suffix}";
                db = cli.GetDatabase(s);
            }

            return db;
        }

        private async Task<Dictionary<string, object>> QueryAndSaveAsync(ReportQueryDto data, string username)
        {
            var page = data.Page;
            var limit = data.Limit;
            var vt = $"{data.Vt}";
            var datefrom = data.DateFrom;
            var dateto = data.DateTo;
            var vs = "('INPATIENT')";
            if (vt == "1")
            {
                vs = "('DAY-SURGERY')";
            }

            var qs = Sql.GetMasterPD101(vs);
            using var conn = rscon.CreateConnection();
            conn.Open();
            var q = await conn.QueryAsync<dynamic>(qs, new { datefrom, dateto });
            List<string> colnames = new List<string>();
            List<BsonDocument> lx = new List<BsonDocument>();
            var filter = Builders<BsonDocument>.Filter.Empty;
            int i = 0;
            foreach (var r in q)
            {
                var rowDictionary = (IDictionary<string, object>)r;
                var mx = new Dictionary<string, object>();
                foreach (var property in rowDictionary)
                {
                    string columnName = property.Key;
                    object columnValue = property.Value;

                    if (i == 0)
                    {
                        colnames.Add(columnName);
                    }

                    Helper.GetDataMap(mx, columnName, columnValue);
                }
                ++i;
                lx.Add(new BsonDocument(mx));
            }

            List<BsonDocument> ld = new List<BsonDocument>();
            var total = lx.Count;
            var pg = new Pager(total, page, limit);

            if (total > 0)
            {
                var dm = GetDb(client, vt);
                await dm.DropCollectionAsync($"__{username}__");
                var col = dm.GetCollection<BsonDocument>($"__{username}__");
                await col.InsertManyAsync(lx);

                await dm.DropCollectionAsync($"__{username}-c__");
                var col1 = dm.GetCollection<BsonDocument>($"__{username}-c__");
                var doc1 = new BsonDocument(new Dictionary<string, object> { { "columns", colnames } });
                await col1.InsertOneAsync(doc1);

                await dm.DropCollectionAsync($"__{username}-q__");
                var col2 = dm.GetCollection<BsonDocument>($"__{username}-q__");
                var doc2 = new BsonDocument(new Dictionary<string, object> { { "datefrom", datefrom }, { "dateto", dateto } });
                await col2.InsertOneAsync(doc2);

                var lv = await col.Find(filter).Skip(pg.LowerBound).Limit(pg.PageSize).ToListAsync();
                ld = Helper.ProcessDoc(lv);
            }

            return new Dictionary<string, object>()
            {
                { "columnmaps", RptColMap.COLUMN_MAP },
                { "total_count", total },
                { "total_page", pg.TotalPages },
                { "page", pg.PageNum },
                { "data", ld.Select(k => k.ToDictionary()).ToList() },
            };
        }
    }
}
