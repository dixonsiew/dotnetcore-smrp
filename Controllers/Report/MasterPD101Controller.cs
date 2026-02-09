using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Authentication;
using smrp.Models;
using smrp.Services;
using smrp.Utils;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace smrp.Controllers.Report
{
    [Route("api/master-pd101")]
    [ApiController]
    public class MasterPD101Controller : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly IMongoClient client;
        private readonly UserService userService;

        public MasterPD101Controller(DefaultConnection conn, IConfiguration cfg, IMongoClient cli)
        {
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
            var db = getDb(client, vt);
            var col = db.GetCollection<BsonDocument>($"__{username}__");
            var col2 = db.GetCollection<BsonDocument>($"__{username}-q__");
            var total = await col.CountDocumentsAsync(new BsonDocument());

            string dateFrom = datefrom;
            string dateTo = dateto;
            var t2 = await col2.CountDocumentsAsync(new BsonDocument());
            if (t2 > 0)
            {
                List<BsonDocument>? ld;
                using (var cur = await col2.FindAsync(new BsonDocument()))
                {
                    ld = await cur.ToListAsync();
                    dateFrom = ld[0]["datefrom"].AsString;
                    dateTo = ld[0]["dateto"].AsString;
                }
            }

            var pg = new Pager(Convert.ToInt32(total), Convert.ToInt32(page), Convert.ToInt32(limit));
            List<BsonDocument> ls;
            using (var cur = await col.FindAsync(new BsonDocument()))
            {
                var lx = await cur.ToListAsync();
                lx = lx.Skip(pg.LowerBound).Take(pg.PageSize).ToList();
                ls = Helper.processDoc(lx);
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

        private IMongoCollection<dynamic> getCollection(IMongoClient cli, string username, string vt)
        {
            var db = getDb(cli, vt);
            var s = $"__{username}__";
            return db.GetCollection<dynamic>(s);
        }

        private IMongoDatabase getDb(IMongoClient cli, string vt)
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
    }
}
