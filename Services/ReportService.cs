using MongoDB.Bson;
using smrp.Utils;

namespace smrp.Services
{
    public class ReportService
    {
        private readonly DefaultConnection ctx;
        private readonly CommonSetupService commonSetupService;

        public ReportService(DefaultConnection c, CommonSetupService cs)
        {
            ctx = c;
            commonSetupService = cs;
        }

        public async Task<string> RefReferralSourceCode(BsonDocument doc)
        {
            return await GetCode("REFERRAL", doc, "referral");
        }

        public async Task<string> RefPersonTitleCode(BsonDocument doc)
        {
            return await GetCode("TITLE", doc, "title");
        }

        public async Task<string> RefGenderCode(BsonDocument doc)
        {
            return await GetCode("GENDER", doc, "gender");
        }

        public async Task<string> RefGenderCode1(BsonDocument doc)
        {
            return await GetCode("CHILD_SEX", doc, "gender");
        }

        public async Task<string> RefMaritalStatusCode(BsonDocument doc)
        {
            return await GetCode("MARITAL_STATUS", doc, "marital_status");
        }

        public async Task<string> RefReligionCode(BsonDocument doc)
        {
            return await GetCode("RELIGION", doc, "religion");
        }

        public async Task<string> RefCitizenshipCode(BsonDocument doc)
        {
            return await GetCode("NATIONALITY", doc, "country");
        }

        public async Task<string> RefCitizenshipCodeNOK(BsonDocument doc)
        {
            return await GetCode("NOK_NATIONALITY", doc, "country");
        }

        public async Task<string> RefEthnicCode(BsonDocument doc)
        {
            return await GetCode("ETHNIC_GROUP", doc, "ethnic_group");
        }

        public async Task<string> RefForeignerResidenceCountryCode(BsonDocument doc)
        {
            return await GetCode("REFFOREIGNRCOUNTRYCODE", doc, "country");
        }

        public async Task<string> RefPersonCategoryCode(BsonDocument doc)
        {
            return await GetCode("REFPERSONCATEGORYCODE", doc, "person_category_code");
        }

        public async Task<string> RefIdentificationTypeCode(BsonDocument doc)
        {
            return await GetCode("DOCUMENT_TYPE", doc, "id_type");
        }

        public async Task<string> RefCityCode(BsonDocument doc)
        {
            return await GetCode("CITYCODE", doc, "city");
        }

        public async Task<string> RefCityCodeNOK(BsonDocument doc)
        {
            return await GetCode("NOK_CITYCODE", doc, "city");
        }

        public async Task<string> RefStateCode(BsonDocument doc)
        {
            return await GetCode("OCITY", doc, "state");
        }

        public async Task<string> RefStateCodeNOK(BsonDocument doc)
        {
            return await GetCode("NOK_OCITY", doc, "state");
        }

        public async Task<string> RefPersonTitleCodeNOK(BsonDocument doc)
        {
            return await GetCode("NOK_TITLE", doc, "title");
        }

        public async Task<string> RefRelationshipCode(BsonDocument doc)
        {
            return await GetCode("RELATION_DESCRIPTION", doc, "relationship");
        }

        public async Task<string> RefIdentificationTypeCodeNOK(BsonDocument doc)
        {
            return await GetCode("NOK_ID_TYPE", doc, "id_type");
        }

        public async Task<string> RefDisciplineCode(BsonDocument doc)
        {
            return await GetCode("PRIMARY_SPECIALITY", doc, "speciality");
        }

        public async Task<string> RefDisciplineCode1(BsonDocument doc)
        {
            return await GetCode("PRIMARY_SPECIALTY", doc, "speciality");
        }

        public async Task<string> RefWardClassCode(BsonDocument doc)
        {
            return await GetCode("PAYMENT_CLASS_CODE", doc, "ward_class");
        }

        public async Task<string> RefDischargeTypeCode(BsonDocument doc)
        {
            return await GetCode("DISCHARGE_REASON", doc, "discharge_type");
        }

        public async Task<string> RefDiagnosisItemTypeCode(BsonDocument doc)
        {
            return await GetCode("DIAGNOSIS_DESC", doc, "diag_item_type");
        }

        public async Task<string> RefLabourModeCode(BsonDocument doc)
        {
            return await GetCode("DELIVERY_TYPE", doc, "delivery_type");
        }

        private async Task<string> GetCode(string key, BsonDocument doc, string table)
        {
            var x = Utils.Constants.NO_INFO;
            var s = Get(key, doc);
            if (s != "")
            {
                var o = await commonSetupService.FindByDescAsync(s, table);
                if (o != null)
                {
                    x = o.Code;
                }
            }

            return x;
        }

        private string Get(string key, BsonDocument doc)
        {
            string s = "";
            if (doc.Contains(key))
            {
                s = $"{doc[key]}";
                s = s.Trim();
            }

            return s;
        }
    }
}
