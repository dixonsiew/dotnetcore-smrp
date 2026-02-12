namespace smrp.Utils
{
    public class ApiResult
    {
        public static IResult UserNotFound
        {
            get
            {
                return Results.Json(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = "User not found",
                }, statusCode: StatusCodes.Status404NotFound);
            }
        }
    }
}
