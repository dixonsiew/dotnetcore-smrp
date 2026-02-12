using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using MongoDB.Driver;
using smrp.Controllers.Report.MasterPD101;
using smrp.Dtos;
using smrp.Models;
using smrp.Services;
using smrp.sql;
using smrp.Utils;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;

namespace smrp.Controllers.Report
{
    [Authorize]
    [Route("api/master-pd101")]
    [ApiController]
    public class MasterPD101Controller : ControllerBase
    {
        private readonly RsConnection rscon;
        private readonly IConfiguration config;
        private readonly IMongoClient client;
        private readonly CommonSetupService commonSetupService;
        private readonly ReportService reportService;
        private readonly JsonSerializerOptions jsonOptions;

        public MasterPD101Controller(DefaultConnection conn, RsConnection rsconn, IConfiguration cfg, IMongoClient cli)
        {
            rscon = rsconn;
            config = cfg;
            client = cli;
            commonSetupService = new CommonSetupService(conn);
            reportService = new ReportService(conn, commonSetupService);
            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                IndentSize = 4,
            };
        }

        [HttpGet("export/rpt2")]
        public async Task<IResult> JsonRH101(
            [FromQuery(Name = "datefrom")] string datefrom = "",
            [FromQuery(Name = "dateto")] string dateto = "")
        {
            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return ApiResult.UserNotFound;
            }

            var filter = Builders<BsonDocument>.Filter.Empty;
            var col = GetCollection(client, username, "1");
            var lx = await col.Find(filter).ToListAsync();
            var ls = Helper.ProcessDoc(lx);

            var dt1 = datefrom.Split("-");
            var dt2 = datefrom.Split("-");
            var ds1 = $"{dt1[2]}{dt1[1]}{dt1[0]}";
            var ds2 = $"{dt2[2]}{dt2[1]}{dt2[0]}";

            var forms = new List<Dictionary<string, object>>();
            foreach ( var d in ls)
            {
                var person = new Dictionary<string, object>()
                {
                    { "refPersonTitleCode",        await reportService.RefPersonTitleCode(d) },
                    { "fullName",                  Helper.GetStr(d["PATIENT_NAME"]) },
                    { "refIdentificationTypeCode", await reportService.RefIdentificationTypeCode(d) },
                    { "identificationNo",          Helper.GetStr(d["DOCUMENT_NUMBER"]) },
                    { "refAddressTypeCode",        "C" },
                    { "street1",                   Helper.GetStr(d["STREET1"]) },
                    { "street2",                   Helper.GetStr(d["STREET2"]) },
                    { "refCityCode",               await reportService.RefCityCode(d) },
                    { "refPostCode",               Helper.GetStr(d["POSTCODE"]) },
                    { "refStateCode",              await reportService.RefStateCode(d) },
                    { "refCountryCode",            await reportService.RefCitizenshipCode(d) },
                    { "refContactTypeCode",        "02" },
                    { "contactInfo",               Helper.GetStr(d["HOME_PHONE"]) },
                };

                var nok = new Dictionary<string, object>()
                {
                    { "refPersonTitleCode",        await reportService.RefPersonTitleCodeNOK(d) },
                    { "fullName",                  Helper.GetStr(d["PATIENT_NOK_NAME"]) },
                    { "refIdentificationTypeCode", await reportService.RefIdentificationTypeCodeNOK(d) },
                    { "identificationNo",          Helper.GetStr(d["NOK_ID"]) },
                    { "refAddressTypeCode",        "C" },
                    { "street1",                   Helper.GetStr(d["NOK_STREET1"]) },
                    { "street2",                   Helper.GetStr(d["NOK_STREET2"]) },
                    { "refCityCode",               await reportService.RefCityCodeNOK(d) },
                    { "refPostCode",               Helper.GetStr(d["NOK_POSTCODE"]) },
                    { "refStateCode",              await reportService.RefStateCodeNOK(d) },
                    { "refCountryCode",            await reportService.RefCitizenshipCodeNOK(d) },
                    { "refContactTypeCode",        "02" },
                    { "contactInfo",               Helper.GetStr(d["NOK_MOBILE_PHONE"]) },
                };

                var m = new Dictionary<string, object>()
                {
                    { "rn",                               Helper.GetStr(d["ACCOUNT_NO"]) },
                    { "mrn",                              Helper.GetStr(d["PRN"]) },
                    { "eventDate",                        $"{d["REGISTRATION_DATE"]} {d["REGISTRATION_TIME"]}:00" },
                    { "isPoliceCase",                     "00" },
                    { "internalReferral",                 "false" },
                    { "refReferralSourceCode",            await reportService.RefReferralSourceCode(d) },
                    { "refGenderCode",                    await reportService.RefGenderCode(d) },
                    { "dob",                              Helper.GetStr(d["DOB"]) },
                    { "refMaritalStatusCode",             await reportService.RefMaritalStatusCode(d) },
                    { "refReligionCode",                  await reportService.RefReligionCode(d) },
                    { "refCitizenshipCode",               await reportService.RefCitizenshipCode(d) },
                    { "refEthnicCode",                    await reportService.RefEthnicCode(d) },
                    { "height",                           Helper.GetNum(Helper.GetStr(d["HEIGHT"])) },
                    { "weight",                           Helper.GetNum(Helper.GetStr(d["WEIGHT"])) },
                    { "refForeignerOriginCountryCode",    await reportService.RefForeignerOriginCountryCode(d) },
                    { "refForeignerResidenceCountryCode", await reportService.RefForeignerResidenceCountryCode(d) },
                    { "refPersonCategoryCode",            await reportService.RefPersonCategoryCode(d) },
                    { "refRelationshipCode",              await reportService.RefRelationshipCode(d) },
                    { "totalDurationDay",                 "0" },
                    { "admissionDate",                    $"{d["ADMISSION_DATE"]} {d["ADMISSION_TIME"]}:00" },
                    { "person",                           person },
                    { "nextOfKins",                       nok },
                };

                forms.Add(m);
            }

            var facilityCode = config["facilityCode"];
            var filename = $"{ds1}_{ds2}_RH101.json";

            Response.Headers.Append(HeaderNames.ContentDisposition, $"attachment; filename={filename}");
            Response.Headers.Append(HeaderNames.CacheControl, "no-cache, no-store, must-revalidate");
            Response.Headers.Append(HeaderNames.Pragma, "no-cache");
            Response.Headers.Append(HeaderNames.Expires, "0");
            Response.Headers.Append("filename", filename);
            Response.Headers.Append(HeaderNames.ContentType, MediaTypeNames.Application.Json);

            var data = new
            {
                filename,
                admissionFrom = datefrom,
                admissionTo = dateto,
                refServiceTypeCode = "02",
                facilityCode,
                forms,
            };
            return Results.Json(data, jsonOptions, MediaTypeNames.Application.Json);
        }

        [HttpGet("export/rpt1")]
        public async Task<IResult> JsonPD101(
            [FromQuery(Name = "datefrom")] string datefrom = "",
            [FromQuery(Name = "dateto")] string dateto = "")
        {
            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return ApiResult.UserNotFound;
            }

            var filter = Builders<BsonDocument>.Filter.Empty;
            var col = GetCollection(client, username, "0");
            var lx = await col.Find(filter).ToListAsync();
            var ls = Helper.ProcessDoc(lx);

            var dt1 = datefrom.Split("-");
            var dt2 = datefrom.Split("-");
            var ds1 = $"{dt1[2]}{dt1[1]}{dt1[0]}";
            var ds2 = $"{dt2[2]}{dt2[1]}{dt2[0]}";

            var forms = new List<Dictionary<string, object>>();
            foreach (var d in ls)
            {
                var person = new Dictionary<string, object>()
                {
                    { "refPersonTitleCode",        await reportService.RefPersonTitleCode(d) },
                    { "fullName",                  Helper.GetStr(d["PATIENT_NAME"]) },
                    { "refIdentificationTypeCode", await reportService.RefIdentificationTypeCode(d) },
                    { "identificationNo",          Helper.GetStr(d["DOCUMENT_NUMBER"]) },
                    { "refAddressTypeCode",        "C" },
                    { "street1",                   Helper.GetStr(d["STREET1"]) },
                    { "street2",                   Helper.GetStr(d["STREET2"]) },
                    { "refCityCode",               await reportService.RefCityCode(d) },
                    { "refPostCode",               Helper.GetStr(d["POSTCODE"]) },
                    { "refStateCode",              await reportService.RefStateCode(d) },
                    { "refCountryCode",            await reportService.RefCitizenshipCode(d) },
                    { "refContactTypeCode",        "02" },
                    { "contactInfo",               Helper.GetStr(d["HOME_PHONE"]) },
                };

                var nok = new Dictionary<string, object>()
                {
                    { "refPersonTitleCode",        await reportService.RefPersonTitleCodeNOK(d) },
                    { "fullName",                  Helper.GetStr(d["PATIENT_NOK_NAME"]) },
                    { "refIdentificationTypeCode", await reportService.RefIdentificationTypeCodeNOK(d) },
                    { "identificationNo",          Helper.GetStr(d["NOK_ID"]) },
                    { "refAddressTypeCode",        "C" },
                    { "street1",                   $"{d["NOK_STREET1"]}" },
                    { "street2",                   Helper.GetStr(d["NOK_STREET2"]) },
                    { "refCityCode",               await reportService.RefCityCodeNOK(d) },
                    { "refPostCode",               Helper.GetStr(d["NOK_POSTCODE"]) },
                    { "refStateCode",              await reportService.RefStateCodeNOK(d) },
                    { "refCountryCode",            await reportService.RefCitizenshipCodeNOK(d) },
                    { "refContactTypeCode",        "02" },
                    { "contactInfo",               Helper.GetStr(d["NOK_MOBILE_PHONE"]) },
                };

                var m = new Dictionary<string, object>()
                {
                    { "rn",                               Helper.GetStr(d["ACCOUNT_NO"]) },
                    { "mrn",                              Helper.GetStr(d["PRN"]) },
                    { "eventDate",                        $"{d["REGISTRATION_DATE"]} {d["REGISTRATION_TIME"]}:00" },
                    { "isPoliceCase",                     "02" },
                    { "internalReferral",                 "false" },
                    { "refReferralSourceCode",            await reportService.RefReferralSourceCode(d) },
                    { "refGenderCode",                    await reportService.RefGenderCode(d) },
                    { "dob",                              $"{d["DOB"]}" },
                    { "refMaritalStatusCode",             await reportService.RefMaritalStatusCode(d) },
                    { "refReligionCode",                  await reportService.RefReligionCode(d) },
                    { "refCitizenshipCode",               await reportService.RefCitizenshipCode(d) },
                    { "refEthnicCode",                    await reportService.RefEthnicCode(d) },
                    { "height",                           Helper.GetNum($"{d["HEIGHT"]}") },
                    { "weight",                           Helper.GetNum($"{d["WEIGHT"]}") },
                    { "refForeignerOriginCountryCode",    await reportService.RefForeignerOriginCountryCode(d) },
                    { "refForeignerResidenceCountryCode", await reportService.RefForeignerResidenceCountryCode(d) },
                    { "refPersonCategoryCode",            await reportService.RefPersonCategoryCode(d) },
                    { "refRelationshipCode",              await reportService.RefRelationshipCode(d) },
                    { "totalDurationDay",                 "0" },
                    { "refWardTransitionTypeCode",        "A" },
                    { "wardDateTime",                     $"{d["ADMISSION_DATE"]} {d["ADMISSION_TIME"]}:00" },
                    { "wardCode",                         Helper.GetStr(d["WARD_NO"]) },
                    { "refDisciplineCode",                await reportService.RefDisciplineCode(d) },
                    { "refSpecialityCode",                await reportService.RefDisciplineCode(d) },
                    { "refSubSpecialityCode",             await reportService.RefDisciplineCode(d) },
                    { "refWardClassCode",                 await reportService.RefWardClassCode(d) },
                    { "refWardCategoryCode",              "00" },
                    { "person",                           person },
                    { "nextOfKins",                       nok },
                };

                forms.Add(m);
            }

            var facilityCode = config["facilityCode"];
            var filename = $"{ds1}_{ds2}_PD101.json";

            Response.Headers.Append(HeaderNames.ContentDisposition, $"attachment; filename={filename}");
            Response.Headers.Append(HeaderNames.CacheControl, "no-cache, no-store, must-revalidate");
            Response.Headers.Append(HeaderNames.Pragma, "no-cache");
            Response.Headers.Append(HeaderNames.Expires, "0");
            Response.Headers.Append("filename", filename);
            Response.Headers.Append(HeaderNames.ContentType, MediaTypeNames.Application.Json);

            var data = new
            {
                filename,
                admissionFrom = datefrom,
                admissionTo = dateto,
                refServiceTypeCode = "01",
                facilityCode,
                forms,
            };
            return Results.Json(data, jsonOptions, MediaTypeNames.Application.Json);
        }

        [HttpGet("rpt1")]
        public async Task<IResult> List(
            [FromQuery(Name = "_page")] string page = "1",
            [FromQuery(Name = "_limit")] string limit = "20",
            [FromQuery(Name = "vt")] string vt = "0",
            [FromQuery(Name = "datefrom")] string datefrom = "",
            [FromQuery(Name = "dateto")] string dateto = "")
        {
            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return ApiResult.UserNotFound;
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
            return Results.Json(new
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
        public async Task<IResult> SearchList(ReportQueryDto data)
        {
            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return ApiResult.UserNotFound;
            }

            var md = await QueryAndSaveAsync(data, username);
            return Results.Json(md);
        }

        [HttpGet("rpt1/{id}")]
        public async Task<IResult> Edit(string id, [FromQuery(Name = "vt")] string vt = "0")
        {
            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return ApiResult.UserNotFound;
            }

            var col = GetCollection(client, username, vt);
            var objectId = new ObjectId(id);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", objectId);
            var lx = await col.Find(filter).ToListAsync();
            var ls = Helper.ProcessDoc(lx);

            if (ls.Count > 0)
            {
                return Results.Json(ls[0].ToDictionary());
            }

            return Results.Json(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "Record not found",
            }, statusCode: StatusCodes.Status404NotFound);
        }

        [HttpPut("rpt1/{id}")]
        public async Task<IResult> Update(Dictionary<string, object> data, string id, [FromQuery(Name = "vt")] string vt = "0")
        {
            var userClaimsPrincipal = User;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            if (username == null)
            {
                return ApiResult.UserNotFound;
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
            return Results.Json(new
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
