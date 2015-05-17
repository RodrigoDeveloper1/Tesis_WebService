using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace Tesis_WebService
{
    /// <summary>
    /// Web Service que se encarga de todos los métodos relacionados con el acceso y la seguridad.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]    
    public class Service1 : System.Web.Services.WebService
    {
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Login(string Username, string Password)
        {
            #region Declarando la variable de resultado
            object result;
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = new SqlConnection(ConstantsRepository.DATABASE_CONNECTION);
            #endregion
            #region Definiendo el query
            string query =
                "SELECT PASSWORDHASH, REPRESENTATIVEID " + 
                "FROM REPRESENTATIVES " +
                "WHERE EMAIL = @username";
            #endregion

            try
            {
                #region Abriendo la conexión y ejecutando la consulta
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@username", Username);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                #endregion

                #region Si hay data
                if (reader.Read())
                {
                    string PasswordHash = reader["PASSWORDHASH"].ToString();                    
                    string UserId = reader["REPRESENTATIVEID"].ToString();
                    sqlConnection.Close();

                    bool valor = Logic.PasswordHash(Password, PasswordHash);

                    if(valor)
                        result = new { Success = valor, UserId = UserId };
                    else
                        result = new { Success = false, Exception = "Contraseña incorrecta." };

                    return new JavaScriptSerializer().Serialize(result);
                }
                #endregion
                #region No hay data
                else
                {
                    sqlConnection.Close();
                    result = new { Success = false, Exception = "Nombre de usuario incorrecto." };

                    return new JavaScriptSerializer().Serialize(result);
                }
                #endregion
            }
            #region Catch de los errores
            catch (SqlException e)
            {
                sqlConnection.Close();
                result = new { Success = false, Exception = e.Message };

                return new JavaScriptSerializer().Serialize(result);
            }
            catch(Exception e)
            {
                sqlConnection.Close();
                result = new { Success = false, Exception = e.Message };

                return new JavaScriptSerializer().Serialize(result);
            }
            #endregion
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Home(string UserId)
        {
            #region Declarando la variable de resultado
            object result;
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = new SqlConnection(ConstantsRepository.DATABASE_CONNECTION);
            #endregion
            #region Definiendo el query
            string query =
                "SELECT TOP(1) " + 
                        "SCH.SchoolId SchoolId, " + 
                        "SCH.Name School_Name, " + 
                        "SY.SchoolYearId SchoolYearId, " +
                        "CONVERT(DATE, SY.StartDate, 103) SchoolYear_StartDate, " +
                        "CONVERT(DATE, SY.EndDate, 103) SchoolYear_EndDate, " + 
                        "P.Name Period_Name, " +
                        "CONVERT(DATE, P.StartDate, 103) Period_StartDate, " +
                        "CONVERT(DATE, P.FinishDate, 103) Period_FinishDate, " + 
                        "R.Name Representative_Name, " + 
                        "R.LastName Representative_LastName " + 
                "FROM REPRESENTATIVES R, " + 
                     "REPRESENTATIVESTUDENTS RS, " + 
                     "STUDENTS S, " + 
                     "STUDENTCOURSES SC, " + 
                     "COURSES C, " +
                     "CASUS CASU, " + 
                     "PERIODS P, " + 
                     "SCHOOLYEARS SY, " + 
                     "SCHOOLS SCH " + 
                "WHERE R.RepresentativeId = @UserId AND " + 
                      "RS.Representative_RepresentativeId = R.RepresentativeId AND " + 
                      "S.StudentId = RS.Student_StudentId AND " + 
                      "S.StudentId = SC.Student_StudentId AND " + 
                      "SC.Course_CourseId = C.CourseId AND " + 
                      "C.CourseId = CASU.CourseId AND " + 
                      "CASU.PeriodId = P.PeriodId AND " + 
                      "P.SchoolYear_SchoolYearId = SY.SchoolYearId AND " + 
                      "SY.School_SchoolId = SCH.SchoolId AND " + 
                      "CAST(P.StartDate AS DATE) <= CAST(GETDATE() AS DATE) AND " + 
                      "CAST(P.FinishDate AS DATE) >= CAST(GETDATE() AS DATE)";
            #endregion

            try
            {
                #region Abriendo la conexión y ejecutando la consulta
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@UserId", UserId);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                #endregion

                #region Si hay data
                if (reader.Read())
                {
                    string SchoolId = reader["SchoolId"].ToString();
                    string School_Name = reader["School_Name"].ToString();
                    string SchoolYearId = reader["SchoolYearId"].ToString();
                    string SchoolYear_StartDate = Convert.ToDateTime(reader["SchoolYear_StartDate"].ToString()).ToShortDateString();
                    string SchoolYear_EndDate = Convert.ToDateTime(reader["SchoolYear_EndDate"].ToString()).ToShortDateString();
                    string Period_Name = reader["Period_Name"].ToString();
                    string Representative_Name = reader["Representative_Name"].ToString();
                    string Representative_LastName = reader["Representative_LastName"].ToString();
                    
                    sqlConnection.Close();
                    result = new 
                    { 
                        Success = true,
                        SchoolId = SchoolId,
                        School_Name = School_Name,
                        SchoolYearId = SchoolYearId,
                        SchoolYear_StartDate = SchoolYear_StartDate,
                        SchoolYear_EndDate = SchoolYear_EndDate,
                        Period_Name = Period_Name,
                        Representative_Name = Representative_Name,
                        Representative_LastName = Representative_LastName
                    };

                    return new JavaScriptSerializer().Serialize(result);
                }
                #endregion
                #region No hay data
                else
                {
                    sqlConnection.Close();
                    result = new { Success = false, Exception = "Error. Id de usuario incorrecto." };

                    return new JavaScriptSerializer().Serialize(result);
                }
                #endregion
            }
            #region Catch de los errores
            catch (SqlException e)
            {
                sqlConnection.Close();
                result = new { Success = false, Exception = e.Message };

                return new JavaScriptSerializer().Serialize(result);
            }
            catch (Exception e)
            {
                sqlConnection.Close();
                result = new { Success = false, Exception = e.Message };

                return new JavaScriptSerializer().Serialize(result);
            }
            #endregion
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Notifications(string UserId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            List<string> listaEstudiantes = new List<string>();
            List<string> listaCursos = new List<string>();
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = new SqlConnection(ConstantsRepository.DATABASE_CONNECTION);
            #endregion
            #region Definiendo query de estudiantes asociados
            string query1 = "SELECT S.STUDENTID StudentId, " +
                                   "C.CourseId CourseId " +
                            "FROM REPRESENTATIVES R, " +
                                 "REPRESENTATIVESTUDENTS RS, " +
                                 "STUDENTS S, " +
                                 "STUDENTCOURSES SC, " +
                                 "COURSES C " +
                           "WHERE R.RepresentativeId = @UserId AND " +
                                 "R.RepresentativeId = RS.Representative_RepresentativeId AND " +
                                 "RS.Student_StudentId = S.StudentId AND " + 
                                 "S.StudentId = SC.Student_StudentId AND " + 
                                 "SC.Course_CourseId = C.CourseId";
            #endregion
            #region Definiendo query de notificaciones, desde estudiantes
            string query2 = "SELECT N.Attribution, " + 
                                   "N.AlertType, " + 
                                   "N.DateOfCreation, " + 
                                   "N.SendDate, " + 
                                   "N.Message, " + 
                                   "N.Automatic " + 
                            "FROM NOTIFICATIONS N, " + 
                                 "SENTNOTIFICATIONS SN " + 
                            "WHERE N.NotificationId = SN.NotificationId AND " + 
                                  "SN.Student_StudentId = @StudentId";
            #endregion
            #region Definiendo query de notificaciones, desde cursos
            string query3 = "SELECT N.Attribution, " +
                                   "N.AlertType, " +
                                   "N.DateOfCreation, " +
                                   "N.SendDate, " +
                                   "N.Message, " +
                                   "N.Automatic " +
                            "FROM NOTIFICATIONS N, " +
                                 "SENTNOTIFICATIONS SN " +
                            "WHERE N.NotificationId = SN.NotificationId AND " +
                                  "SN.Course_CourseId = @CourseId";
            #endregion

            try
            {
                #region Abriendo la conexión y ejecutando la consulta - QueryI
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query1, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@UserId", UserId);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                #endregion
                #region Obteniendo la lista de estudiantes y cursos asociados
                while (reader.Read())
                {
                    listaEstudiantes.Add(reader["StudentId"].ToString());
                    listaCursos.Add(reader["CourseId"].ToString());
                }
                reader.Close();
                #endregion
                #region Ciclo por estudiantes
                foreach (string StudentId in listaEstudiantes)
                {
                    #region Abriendo la conexión y ejecutando la consulta - QueryII
                    sqlCommand = new SqlCommand(query2, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                    reader = sqlCommand.ExecuteReader();
                    #endregion
                    #region Obteniendo la lista de notificaciones asociadas
                    while (reader.Read())
                    {
                        string Attribution = reader["Attribution"].ToString();
                        string AlertType = reader["AlertType"].ToString();
                        string DateOfCreation = reader["DateOfCreation"].ToString();
                        string SendDate = reader["SendDate"].ToString();
                        string Message = reader["Message"].ToString();
                        string Automatic = reader["Automatic"].ToString();

                        result.Add(new
                        {
                            Attribution = Attribution,
                            AlertType = AlertType,
                            DateOfCreation = DateOfCreation,
                            SendDate = SendDate,
                            Message = Message,
                            Automatic = Automatic
                        });
                    }
                    reader.Close();
                    #endregion
                }
                #endregion
                #region Ciclo por cursos
                foreach (string CourseId in listaCursos)
                {
                    #region Abriendo la conexión y ejecutando la consulta - QueryIII
                    sqlCommand = new SqlCommand(query3, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                    reader = sqlCommand.ExecuteReader();
                    #endregion
                    #region Obteniendo la lista de notificaciones asociadas
                    while (reader.Read())
                    {
                        string Attribution = reader["Attribution"].ToString();
                        string AlertType = reader["AlertType"].ToString();
                        string DateOfCreation = reader["DateOfCreation"].ToString();
                        string SendDate = reader["SendDate"].ToString();
                        string Message = reader["Message"].ToString();
                        string Automatic = reader["Automatic"].ToString();

                        result.Add(new
                        {
                            Attribution = Attribution,
                            AlertType = AlertType,
                            DateOfCreation = DateOfCreation,
                            SendDate = SendDate,
                            Message = Message,
                            Automatic = Automatic
                        });
                    }
                    reader.Close();
                    #endregion
                }
                #endregion

                return new JavaScriptSerializer().Serialize(result);
            }
            #region Catch de los errores
            catch (SqlException e)
            {
                sqlConnection.Close();
                result.Add(new { Success = false, Exception = e.Message });

                return new JavaScriptSerializer().Serialize(result);
            }
            catch (Exception e)
            {
                sqlConnection.Close();
                result.Add(new { Success = false, Exception = e.Message });

                return new JavaScriptSerializer().Serialize(result);
            }
            #endregion
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Statistics(string SchoolId)
        {
            List<object> result = new List<object>();
            
            string path = ConstantsRepository.STATISTICS_IMAGES_PATH + SchoolId;
            string imgPath1 = Path.Combine(Server.MapPath(path), ConstantsRepository.STATISTICS_IMG_1);
            string imgPath2 = Path.Combine(Server.MapPath(path), ConstantsRepository.STATISTICS_IMG_2);

            /* Chequear por si acaso:
             * http://stackoverflow.com/questions/11273206/send-image-to-a-soap-1-0-webservice-from-a-net-client
             */

            try
            {
                Bitmap img1 = new Bitmap(imgPath1);
                Bitmap img2 = new Bitmap(imgPath2);

                result.Add(img1);
                result.Add(img2);

                return new JavaScriptSerializer().Serialize(result);
            }
            catch (Exception e)
            {
                result.Add(new { Success = false, Exception = e.Message });
                return new JavaScriptSerializer().Serialize(result);
            }
        }
    }
}