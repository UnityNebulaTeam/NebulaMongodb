namespace NebulaTool
{
    public static class NebulaURL
    {
        public static class MongoDB
        {
            public static readonly string itemURL = "http://localhost:5135/api/Mongo/item";
            public static readonly string databaseURL = "http://localhost:5135/api/Mongo/db";
            public static readonly string tableURL = "http://localhost:5135/api/Mongo/table";


            public static readonly string RegisterURL = "http://localhost:5135/api/auth/register";
            public static readonly string LoginURL = "http://localhost:5135/api/auth/login";
            public static readonly string ApiDatabaseURL = "http://localhost:5135/api/user/db";
        }
    }
}