namespace smrp.Services
{
    public class ConfigService
    {
        private readonly IConfiguration config;

        public string FacilityCode { get; set; }
        public string MongoDbPrefix { get; set; }

        public ConfigService(IConfiguration cfg)
        {
            config = cfg;
            FacilityCode = config.GetValue<string>("AppSettings:facilityCode") ?? string.Empty;
            MongoDbPrefix = config.GetValue<string>("AppSettings:mongodb.prefix") ?? "";
        }
    }
}
