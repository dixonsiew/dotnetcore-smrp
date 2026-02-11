using MongoDB.Bson;
using Swashbuckle.Swagger;
using System.Text.RegularExpressions;

namespace smrp.Utils
{
    public class Helper
    {
        public static string GetDateStr(BsonValue? o)
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

        public static List<BsonDocument> ProcessDoc(List<BsonDocument> lx)
        {
            var ls = new List<BsonDocument>();
            const string na = "N/A";
            foreach (var x in lx)
            {
                var d = x["_id"].AsObjectId;
                x["_id"] = d.ToString();
                if (x.Contains("ADMISSION_DATE"))
                {
                    var o = x["ADMISSION_DATE"];
                    x["ADMISSION_DATE"] = GetDateStr(o);
                }

                if (x.Contains("DISCHARGE_DATE"))
                {
                    var o = x["DISCHARGE_DATE"];
                    x["DISCHARGE_DATE"] = GetDateStr(o);
                }

                if (x.Contains("DEATH_DATE"))
                {
                    var o = x["DEATH_DATE"];
                    x["DEATH_DATE"] = GetDateStr(o);
                }

                if (x.Contains("DELIVERY_DATE"))
                {
                    var o = x["DELIVERY_DATE"];
                    x["DELIVERY_DATE"] = GetDateStr(o);
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
                        SetValue(x, "NOK_STREET1", "STREET1");
                        SetValue(x, "NOK_STREET2", "STREET2");
                        SetValue(x, "NOK_CITYCODE", "CITYCODE");
                        SetValue(x, "NOK_POSTCODE", "POSTCODE");
                        SetValue(x, "NOK_OCITY", "OCITY");
                        SetValue(x, "NOK_NATIONALITY", "NATIONALITY");
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

        public static void SetValue(BsonDocument x, string ofield, string srcField)
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

        public static string GetStr(BsonValue x)
        {
            return x.AsString;
        }

        public static long GetNumber(string s)
        {
            return long.Parse(s); 
        }

        public static double GetNum(string s)
        {
            string r = Regex.Replace(s, @"[^\d.]*", string.Empty);
            return double.Parse(r);
        }

        public static void GetDataMap(Dictionary<string, object> mx, string columnName, object columnValue)
        {
            if (columnValue == null)
            {
                mx.Add(columnName, "");
                return;
            }

            Type columnType = columnValue.GetType();
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
                mx.Add(columnName, ((long)Convert.ToDecimal(columnValue)));
            }

            else if (columnType.Name == "DateTime")
            {
                mx.Add(columnName, columnValue.ToString() ?? "");
            }

            else
            {
                mx.Add(columnName, columnValue.ToString() ?? "");
            }
        }
    }
}
