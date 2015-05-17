using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tesis_WebService
{
    public static class ConstantsRepository
    {
        public const string DATABASE_CONNECTION = @"Data Source=.\SQLEXPRESS;Initial Catalog=Context;Integrated Security=True";
        public const string STATISTICS_IMAGES_PATH = @"~\App_Uploads\MobileApp_Statistics\School_";
        public const string STATISTICS_IMG_1 = "AprobadosVsReprobados.PNG";
        public const string STATISTICS_IMG_2 = "Top10ResultadosDestacados.PNG";
    }
}