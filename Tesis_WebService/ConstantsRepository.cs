using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tesis_WebService
{
    public static class ConstantsRepository
    { 
        #region SQL Server Express (local)
        public const string SQLSERVER_EXPRESS_EDITION_DATABASE_CONNECTION = @"Data Source=.\SQLEXPRESS;Initial Catalog=Context;Integrated Security=True";
        #endregion

        #region AppHarbor
        public const string APPHARBOR_DATABASE_CONNECTION = "Server=22868d3a-d013-47a8-9930-a49b015d6de0.sqlserver.sequelizer.com;Database=db22868d3ad01347a89930a49b015d6de0;User ID=wqwjhdllbiohkkls;Password=wAm8GrVkec54ZpqQbLfupnJXC3u8DPCf75P72rzJYRsokEHVdYqVsfBiZwDN6WNx;";
        public const string APPHARBOR_HOSTNAME = "22868d3a-d013-47a8-9930-a49b015d6de0.sqlserver.sequelizer.com";
        public const string APPHARBOR_USERNAME = "wqwjhdllbiohkkls";
        public const string APPHARBOR_PASSWORD = "wAm8GrVkec54ZpqQbLfupnJXC3u8DPCf75P72rzJYRsokEHVdYqVsfBiZwDN6WNx";
        public const string APPHARBOR_DATABASE = "db22868d3ad01347a89930a49b015d6de0";
        #endregion

        public const string STATISTICS_IMAGES_PATH = @"~\App_Data\MobileApp_Statistics\";
        public const string STATISTICS_IMAGES_PATH_APP_UPLOADS = @"~\App_Uploads\MobileApp_Statistics\";
        public const string STATISTICS_IMAGES_PATH_RESOURCES = @"~\Resources\images\";
        public const string STATISTICS_IMAGES_PATH_images = @"~\images\";
        public const string STATISTICS_IMG_1 = "AprobadosVsReprobados.PNG";
        public const string STATISTICS_IMG_2 = "Top10ResultadosDestacados.PNG";
    }
}