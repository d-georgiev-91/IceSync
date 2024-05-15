namespace IceSync.ApiClient.Configs
{
    public class ApiClientConfig
    {
        public required string ApiBaseUrl { get; set; }

        public required string ApiCompanyId { get; set; }
        
        public required string ApiUserId { get; set; }
        
        public required string ApiUserSecret { get; set; }
    }
}
