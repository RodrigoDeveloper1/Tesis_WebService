using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tesis_WebService
{
    public static class ConstantsRepository
    { 
        #region SQL Server Express (local)
        public const string SQLSERVER_EXPRESS_EDITION_DATABASE_CONNECTION = 
            @"Data Source=.\SQLEXPRESS;Initial Catalog=Context;Integrated Security=True";
        #endregion

        #region AppHarbor
        public const string APPHARBOR_DATABASE_CONNECTION =
                "Server=45e2a27d-28d0-4335-8ae2-a4c3014db818.sqlserver.sequelizer.com;" +
                "Database=db45e2a27d28d043358ae2a4c3014db818;" +
                "User ID=ydzlamzucsjhurji;" +
                "Password=qmo2ZexLRdKuaM3p4CpZvseyq4k3LHJFU34YgM84RrR72JWzYhwszD87gWK62C8h;" +
                "MultipleActiveResultSets=True;";
        public const string APPHARBOR_HOSTNAME = "45e2a27d-28d0-4335-8ae2-a4c3014db818.sqlserver.sequelizer.com";
        public const string APPHARBOR_USERNAME = "ydzlamzucsjhurji";
        public const string APPHARBOR_PASSWORD = "qmo2ZexLRdKuaM3p4CpZvseyq4k3LHJFU34YgM84RrR72JWzYhwszD87gWK62C8h";
        public const string APPHARBOR_DATABASE = "db45e2a27d28d043358ae2a4c3014db818";
        #endregion

        public const string STATISTICS_IMAGES_PATH_APP_UPLOADS = @"~\App_Uploads\MobileApp_Statistics\";
        public const string STATISTICS_IMG_1 = "AprobadosVsReprobados.PNG";
        public const string STATISTICS_IMG_2 = "Top10ResultadosDestacados.PNG";
        public const string STATISTICS_IMG_3 = "Top10ResultadosDeficientes.PNG";

        public const int MOBILE_STATISTICS_CODE_AprobadosVsReprobados = 1;
        public const int MOBILE_STATISTICS_CODE_Top10ResultadosDestacados = 2;
        public const int MOBILE_STATISTICS_CODE_Top10ResultadosDeficientes = 3;
    }
}