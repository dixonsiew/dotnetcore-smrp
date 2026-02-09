using Amazon.Runtime.Internal.Transform;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using smrp.Dtos;
using smrp.Models;
using smrp.Services;
using smrp.sql;
using smrp.Utils;
using System;
using System.Security.Claims;

namespace smrp.Controllers.Report
{
    [Route("api/master-pd101")]
    [ApiController]
    public class MasterPD101Controller : ControllerBase
    {
        private readonly DefaultConnection con;
        private readonly IConfiguration config;
        private readonly IMongoClient client;
        private readonly UserService userService;

        public MasterPD101Controller(DefaultConnection conn, IConfiguration cfg, IMongoClient cli)
        {
            con = conn;
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
            var userId = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return res;
            }

            int id = Convert.ToInt32(userId);
            var user = await userService.FindByIdAsync(id);
            if (user == null)
            {
                return res;
            }

            string username = user.Username;
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
                List<BsonDocument>? ld;
                using (var cur = await col2.FindAsync(filter))
                {
                    ld = await cur.ToListAsync();
                    dateFrom = ld[0]["datefrom"].AsString;
                    dateTo = ld[0]["dateto"].AsString;
                }
            }

            var pg = new Pager(Convert.ToInt32(total), Convert.ToInt32(page), Convert.ToInt32(limit));
            List<BsonDocument> ls;
            var findOptions = new FindOptions<BsonDocument, BsonDocument>
            {
                Limit = pg.PageSize,
                Skip = pg.LowerBound,
            };
            using (var cur = await col.FindAsync(filter, options: findOptions))
            {
                var lx = await cur.ToListAsync();
                ls = Helper.ProcessDoc(lx);
            }
            return Results.Ok(new
            {
                columnmaps = RptColMap.COLUMN_MAP,
                total_count = total,
                total_page = pg.TotalPages,
                page = pg.PageNum,
                data = ls,
                datefrom = dateFrom,
                dateto = dateTo,
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

        public async Task<Dictionary<string, object>> QueryAndSaveAsync(ReportQueryDto data, string username)
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
            using var conn = con.CreateConnection();
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
                    Type columnType = columnValue.GetType();
                    if (i == 0)
                    {
                        colnames.Add(columnName);
                    }

                    if (columnType.Name == "String")
                    {
                        mx.Add(columnName, columnValue.ToString() ?? "");
                    }

                    else if (columnType.Name.Contains("Int"))
                    {
                        mx.Add(columnName, Convert.ToInt64(columnValue));
                    }

                    else if (columnType.Name == "Double")
                    {
                        mx.Add(columnName, Convert.ToDouble(columnValue));
                    }

                    else if (columnType.Name == "Decimal")
                    {
                        mx.Add(columnName, Convert.ToDecimal(columnValue));
                    }

                    lx.Add(new BsonDocument(mx));
                }
                ++i;
            }

            List<BsonDocument> ld = new List<BsonDocument>();
            var total = lx.Count;
            var pg = new Pager(total, page, limit);

            if (total > 0)
            {
                var dm = GetDb(client, vt);
                var col = dm.GetCollection<BsonDocument>($"__{username}__");
                await dm.DropCollectionAsync($"__{username}__");
                await col.InsertManyAsync(lx);

                await dm.DropCollectionAsync($"__{username}-c__");
                var col1 = dm.GetCollection<BsonDocument>($"__{username}-c__");
                var doc1 = new BsonDocument(new Dictionary<string, object> { { "columns", colnames } });
                await col1.InsertOneAsync(doc1);

                await dm.DropCollectionAsync($"__{username}-q__");
                var col2 = dm.GetCollection<BsonDocument>($"__{username}-q__");
                var doc2 = new BsonDocument(new Dictionary<string, object> { { "datefrom", datefrom }, { "dateto", dateto } });
                await col2.InsertOneAsync(doc2);

                var findOptions = new FindOptions<BsonDocument, BsonDocument>
                {
                    Limit = pg.PageSize,
                    Skip = pg.LowerBound,
                };
                using (var cur = await col.FindAsync(filter, options: findOptions))
                {
                    var lv = await cur.ToListAsync();
                    ld = Helper.ProcessDoc(lv);
                }
            }

            return new Dictionary<string, object>()
            {
                { "columnmaps", RptColMap.COLUMN_MAP },
                { "total_count", total },
                { "total_page", pg.TotalPages },
                { "page", pg.PageNum },
                { "data", ld }
            };
        }
    }
}
