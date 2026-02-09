using MongoDB.Bson;
using Swashbuckle.Swagger;

namespace smrp.Utils
{
    public class Helper
    {
        public static string getDateStr(BsonValue? o)
        {
            if (o == null)
            {
                return "";
            }

            if (o.IsBsonDateTime)
            {
                var dt = o.AsBsonDateTime;
                return dt.ToUniversalTime().ToString("yyyy-mm-dd");
            }

            string s = o.AsString;
            string a = s.Length > 10 ? s.Substring(0, 10) : s;
            return a;
        }

        public static List<BsonDocument> processDoc(List<BsonDocument> lx)
        {
            var ls = new List<BsonDocument>();
            const string na = "N/A";
            foreach (var x in lx)
            {
                if (x.Contains("ADMISSION_DATE"))
                {
                    var o = x["ADMISSION_DATE"];
                    x["ADMISSION_DATE"] = getDateStr(o);
                }

                if (x.Contains("DISCHARGE_DATE"))
                {
                    var o = x["DISCHARGE_DATE"];
                    x["DISCHARGE_DATE"] = getDateStr(o);
                }

                if (x.Contains("DEATH_DATE"))
                {
                    var o = x["DEATH_DATE"];
                    x["DEATH_DATE"] = getDateStr(o);
                }

                if (x.Contains("DELIVERY_DATE"))
                {
                    var o = x["DELIVERY_DATE"];
                    x["DELIVERY_DATE"] = getDateStr(o);
                }

                if (x.Contains("PATIENT_NOK_NAME"))
                {
                    string s = x["PATIENT_NOK_NAME"].AsString;
                    if (na == s)
                    {
                        x["NOK_STREET1"] = na;
                        x["NOK_STREET2"] = na;
                        x["NOK_CITYCODE"] = na;
                        x["NOK_POSTCODE"] = na;
                        x["NOK_OCITY"] = na;
                        x["NOK_NATIONALITY"] = na;
                    }

                    else
                    {
                        setValue(x, "NOK_STREET1", "STREET1");
                        setValue(x, "NOK_STREET2", "STREET2");
                        setValue(x, "NOK_CITYCODE", "CITYCODE");
                        setValue(x, "NOK_POSTCODE", "POSTCODE");
                        setValue(x, "NOK_OCITY", "OCITY");
                        setValue(x, "NOK_NATIONALITY", "NATIONALITY");
                    }
                }

                else
                {
                    x["NOK_STREET1"] = na;
                    x["NOK_STREET2"] = na;
                    x["NOK_CITYCODE"] = na;
                    x["NOK_POSTCODE"] = na;
                    x["NOK_OCITY"] = na;
                    x["NOK_NATIONALITY"] = na;
                }

                ls.Add(x);
            }

            return ls;
        }

        public static void setValue(BsonDocument x, string ofield, string srcField)
        {
            var v = x[srcField].AsString;
            if (x.Contains(ofield))
            {
                var s = x[ofield].AsString;
                if ("N/A" == s)
                {
                    x[ofield] = v;
                }
            }

            else
            {
                x[ofield] = v;
            }

            if (x[ofield].AsString == "undefined")
            {
                x[ofield] = "N/A";
            }
        }
    }
}
