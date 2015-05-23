﻿using System;
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
        /// <summary>
        /// Método que establece la conexión con la base de datos.
        /// Rodrigo Uzcátegui - 22-05-15
        /// </summary>
        /// <returns>El objeto de la conexión</returns>
        private SqlConnection Conexion()
        {
            ///AppHarbor Server
            string conexion = ConstantsRepository.APPHARBOR_DATABASE_CONNECTION;

            ///Localhost
            //string conexion = ConstantsRepository.SQLSERVER_EXPRESS_EDITION_DATABASE_CONNECTION;

            return new SqlConnection(conexion);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CourseInfo(string StudentId, string PeriodId)
        {
            #region Declarando variables
            object result;
            string CourseId = "";
            string CourseName = "";
            string CourseGrade = "";
            string CourseSection = "";
            string Materias = "";
            string NroAlumnos = "";
            bool SiHayData = false;
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = Conexion();
            #endregion
            #region Definiendo el query I
            string queryI =
                "SELECT C.CourseId CourseId, " +
                       "C.Name CourseName, " +
                       "C.Grade CourseGrade, " +
                       "C.Section CourseSection, " +
                       "SU.SubjectId SubjectId, " +
                       "SU.Name SubjectName, " +
                       "SU.SubjectCode SubjectCode, " +
                       "U.Name UserName, " +
                       "U.LastName UserLastName " +
                "FROM STUDENTS S, " +
                     "StudentCourses SC, " +
                     "COURSES C, " +
                     "CASUS CASU, " +
                     "SUBJECTS SU, " +
                     "ASPNETUSERS U " +
                "WHERE S.StudentId = @StudentId AND " +
                      "SC.Student_StudentId = S.StudentId AND " +
                      "SC.Course_CourseId = C.CourseId AND " +
                      "CASU.CourseId = C.CourseId AND " +
                      "CASU.PeriodId = @PeriodId AND " +
                      "CASU.SubjectId = SU.SubjectId AND " +
                      "CASU.UserId = U.Id";
            #endregion
            #region Definiendo el query II
            string queryII = "SELECT COUNT(SC.Student_StudentId) NroAlumnos " +
                             "FROM COURSES C, " +
                                  "StudentCourses SC " +
                             "WHERE C.CourseId = @CourseId AND " +
                                   "C.CourseId = SC.Course_CourseId";
            #endregion

            try
            {
                #region Abriendo la conexión y ejecutando la consulta I
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                sqlCommand.Parameters.AddWithValue("@PeriodId", PeriodId);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                #endregion
                #region Obteniendo info del curso
                while (reader.Read())
                {
                    SiHayData = true;
                    CourseId = reader["CourseId"].ToString();
                    CourseName = reader["CourseName"].ToString();
                    CourseGrade = reader["CourseGrade"].ToString();
                    CourseSection = reader["CourseSection"].ToString();
                    Materias += reader["SubjectName"].ToString() + " (" +
                                reader["UserName"].ToString() + " " +
                                reader["UserLastName"].ToString() + ")_";
                }
                reader.Close();
                #endregion
                #region Si hay data
                if (SiHayData)
                {
                    Materias = Materias.Remove(Materias.Length - 1); //Eliminando el último caracter

                    sqlCommand = new SqlCommand(queryII, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                    reader = sqlCommand.ExecuteReader();

                    if (reader.Read())
                        NroAlumnos = reader["NroAlumnos"].ToString();

                    result = new
                    {
                        Success = true,
                        CourseId = CourseId,
                        CourseName = CourseName,
                        CourseGrade = CourseGrade,
                        CourseSection = CourseSection,
                        NroAlumnos = NroAlumnos,
                        Materias = Materias
                    };
                }
                #endregion
                #region No hay data
                else
                    result = new { Success = false, Exception = "No hay data asociada" };
                #endregion

                return new JavaScriptSerializer().Serialize(result);
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
        public string Home(string UserId)
        {
            #region Declarando la variable de resultado
            object result;
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = Conexion();
            #endregion
            #region Definiendo el query
            string query =
                "SELECT TOP(1) " +
                        "SCH.SchoolId SchoolId, " +
                        "SCH.Name School_Name, " +
                        "SY.SchoolYearId SchoolYearId, " +
                        "CONVERT(DATE, SY.StartDate, 103) SchoolYear_StartDate, " +
                        "CONVERT(DATE, SY.EndDate, 103) SchoolYear_EndDate, " +
                        "P.PeriodId PeriodId, " +
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
            /*Este query no incluye obtener información después de la fecha de finalización del último lapso*/
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
                    string PeriodId = reader["PeriodId"].ToString();
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
                        PeriodId = PeriodId,
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
        public string Login(string Username, string Password)
        {
            #region Declarando la variable de resultado
            object result;
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = Conexion();
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
        public string Notifications(string UserId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            List<string> listaEstudiantes = new List<string>();
            List<string> listaCursos = new List<string>();
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = Conexion();
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

            try
            {
                Image img1 = Image.FromFile(imgPath1);
                Image img2 = Image.FromFile(imgPath2);

                MemoryStream stream1 = new MemoryStream();
                MemoryStream stream2 = new MemoryStream();

                img1.Save(stream1, System.Drawing.Imaging.ImageFormat.Bmp);
                img2.Save(stream2, System.Drawing.Imaging.ImageFormat.Bmp);

                byte[] imageByte1 = stream1.ToArray();
                byte[] imageByte2 = stream2.ToArray();

                string imageBase64_1 = Convert.ToBase64String(imageByte1);
                string imageBase64_2 = Convert.ToBase64String(imageByte2);

                stream1.Dispose(); stream2.Dispose();
                img1.Dispose(); img2.Dispose();

                result.Add(new 
                { 
                    Title = "Aprobados vs Reprobados",
                    Image = imageBase64_1 
                });

                result.Add(new
                {
                    Title = "Top 10 resultados destacados",
                    Image = imageBase64_2
                });

                return new JavaScriptSerializer().Serialize(result);
            }
            catch (Exception e)
            {
                result.Add(new { Success = false, Exception = e.Message });
                return new JavaScriptSerializer().Serialize(result);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string StudentsInfo(string UserId)
        {
            #region Declarando la variable de resultado
            List<object> result = new List<object>();
            #endregion
            #region Estableciendo la conexión a BD
            SqlConnection sqlConnection = Conexion();
            #endregion
            #region Definiendo el query
            string query =
                "SELECT S.* " +
                "FROM STUDENTS S, " +
                     "RepresentativeStudents RS, " +
                     "Representatives R " +
                "WHERE S.StudentId = RS.Student_StudentId AND " +
                      "RS.Representative_RepresentativeId = R.RepresentativeId AND " +
                      "R.RepresentativeId = @UserId " +
                "ORDER BY S.RegistrationNumber";
            #endregion

            try
            {
                #region Abriendo la conexión y ejecutando la consulta
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@UserId", UserId);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                #endregion
                #region Obteniendo info de los estudiantes
                while (reader.Read())
                {
                    string StudentId = reader["StudentId"].ToString();
                    string LastName = reader["FirstLastName"].ToString();
                    string SecondLastName = reader["SecondLastName"].ToString();
                    string FirstName = reader["FirstName"].ToString();
                    string SecondName = reader["SecondName"].ToString();
                    string RegistrationNumber = reader["RegistrationNumber"].ToString();
                    string EntireName = LastName + " " + SecondLastName + ", " + FirstName + " " + SecondName;

                    result.Add(new
                    {
                        StudentId = StudentId,
                        EntireName = EntireName,
                        RegistrationNumber = RegistrationNumber,
                    });
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
    }
}