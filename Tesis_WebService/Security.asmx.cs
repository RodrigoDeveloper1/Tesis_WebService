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
        public string Assessments(string StudentId, string CourseId, string SubjectId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            SqlConnection sqlConnection = null;

            string Assessment = "", 
                   Score = "", 
                   Period = "", 
                   DefinitivaString = "";

            int Grade = 0, 
                Percentage = 0,
                contadorI = 0,
                contadorII = 0,
                contadorIII = 0;

            double calculoI = 0,
                   calculoII = 0,
                   calculoIII = 0,
                   definitiva = 0;
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el query I
                string queryI =
                    "SELECT A.Name AssessmentName, " +
                           "A.Percentage AssessmentPercentage, " +
                           "S.NumberScore, " +
                           "S.LetterScore, " +
                           "P.Name Period, " +
                           "C.Grade "+
                    "FROM Assessments A, " +
                         "Scores S, " +
                         "Students St, " +
                         "Periods P, " +
                         "Courses C " +
                    "WHERE A.CASU_CourseId = @CourseId AND " +
                          "A.CASU_SubjectId = @SubjectId AND " +
                          "St.StudentId = @StudentId AND " +
                          "A.AssessmentId = S.AssessmentId AND " +
                          "S.StudentId = St.StudentId AND " +
                          "A.CASU_PeriodId = P.PeriodId AND " +
                          "A.CASU_CourseId = C.CourseId";
                #endregion

                #region Operaciones para query I
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                sqlCommand.Parameters.AddWithValue("@SubjectId", SubjectId);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    Grade = Convert.ToInt32(reader["Grade"].ToString());
                    Percentage = Convert.ToInt32(reader["AssessmentPercentage"].ToString());

                    Assessment = reader["AssessmentName"].ToString();
                    Score = (Grade > 6 ? reader["NumberScore"].ToString() :
                        reader["LetterScore"].ToString());
                    Period = reader["Period"].ToString();

                    result.Add(new
                    {
                        Assessment = Assessment,
                        Score = Score,
                        Period = Period
                    });

                    #region Cálculos para definitiva
                    #region Cálculos del 1er Lapso
                    if (Period.Equals("1er Lapso"))
                    {
                        #region Bachillerato
                        if (Grade > 6) //Bachillerato
                            calculoI += (double)(Convert.ToInt32(Score) * Percentage) / 100;
                        #endregion
                        #region Primaria
                        else //Primaria
                        {
                            if (Score.Equals("A")) calculoI += 5;
                            else if (Score.Equals("B")) calculoI += 4;
                            else if (Score.Equals("C")) calculoI += 3;
                            else if (Score.Equals("D")) calculoI += 2;
                            else if (Score.Equals("E")) calculoI += 1;

                            contadorI++;
                        }
                        #endregion
                    }
                    #endregion
                    #region Cálculos del 2do Lapso
                    else if (Period.Equals("2do Lapso"))
                    {
                        #region Bachillerato
                        if (Grade > 6) //Bachillerato
                            calculoII += (double)(Convert.ToInt32(Score) * Percentage) / 100;
                        #endregion
                        #region Primaria
                        else //Primaria
                        {
                            if (Score.Equals("A")) calculoII += 5;
                            else if (Score.Equals("B")) calculoII += 4;
                            else if (Score.Equals("C")) calculoII += 3;
                            else if (Score.Equals("D")) calculoII += 2;
                            else if (Score.Equals("E")) calculoII += 1;

                            contadorII++;
                        }
                        #endregion
                    }
                    #endregion
                    #region Cálculos del 3er Lapso
                    else if (Period.Equals("3er Lapso"))
                    {
                        #region Bachillerato
                        if (Grade > 6) //Bachillerato
                            calculoIII += (double)(Convert.ToInt32(Score) * Percentage) / 100;
                        #endregion
                        #region Primaria
                        else //Primaria
                        {
                            if (Score.Equals("A")) calculoIII += 5;
                            else if (Score.Equals("B")) calculoIII += 4;
                            else if (Score.Equals("C")) calculoIII += 3;
                            else if (Score.Equals("D")) calculoIII += 2;
                            else if (Score.Equals("E")) calculoIII += 1;

                            contadorIII++;
                        }
                        #endregion
                    }
                    #endregion
                    #endregion
                }
                reader.Close();

                #region Cálculo definitiva
                #region Bachillerato
                if (Grade > 6) //Bachillerato
                {
                    definitiva = Math.Round((double)(calculoI + calculoII + calculoIII) / 3, 2);
                    result.Add(new { Definitiva = definitiva });
                }
                #endregion
                #region Primaria
                else //Primaria
                {
                    definitiva = Math.Round((double)(((double)calculoI / contadorI) +
                                           ((double)calculoII / contadorII) +
                                           ((double)calculoIII / contadorIII)) / 3);

                    if (definitiva == 1) DefinitivaString = "A";
                    else if (definitiva == 2) DefinitivaString = "B";
                    else if (definitiva == 3) DefinitivaString = "C";
                    else if (definitiva == 4) DefinitivaString = "D";
                    else if (definitiva == 5) DefinitivaString = "E";

                    result.Add(new { Definitiva = DefinitivaString });
                }
                #endregion
                #endregion
                #endregion
            }
            #endregion
            #region Catch
            catch (SqlException e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            catch (Exception e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            #endregion
            #region Finally
            finally
            {
                sqlConnection.Close();
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CourseInfo(string StudentId, string PeriodId)
        {
            #region Declarando variables
            object result;
            SqlConnection sqlConnection = null;
            string CourseId = "", CourseName = "", CourseSection = "", Materias = "", SubjectName = "", 
                AssessmentName = "", AssessmentId = "", PromedioString = "";
            int CourseGrade = 0, NroAlumnos = 0;
            double Promedio = 0;
            bool SiHayData = false;
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
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
                          "CASU.TeacherId = U.Id " +
                    "ORDER BY SU.Name";
                #endregion
                #region Definiendo el query II - Nro de estudiantes por curso
                string queryII = "SELECT COUNT(SC.Student_StudentId) NroAlumnos " +
                                 "FROM COURSES C, " +
                                      "StudentCourses SC " +
                                 "WHERE C.CourseId = @CourseId AND " +
                                       "C.CourseId = SC.Course_CourseId";
                #endregion
                #region Definiendo el query III - Última evaluación por curso
                string queryIII =
                    "SELECT TOP(1) " +
                        "A.[Name] AssessmentName, " +
                        "A.AssessmentId AssessmentId, " +
                        "SUB.[Name] SubjectName " +
                    "FROM Assessments A, " +
                         "Courses C, " +
                         "CASUs CASU, " +
                         "Subjects SUB " +
                    "WHERE A.CASU_CourseId = CASU.CourseId AND " +
                          "A.CASU_PeriodId = CASU.PeriodId AND " +
                          "A.CASU_SubjectId = CASU.SubjectId AND " +
                          "CASU.CourseId = C.CourseId AND " +
                          "C.CourseId = @CourseId AND " +
                          "CASU.SubjectId = SUB.SubjectId " +
                    "ORDER BY A.FinishDate, A.EndHour";
                #endregion
                #region Definiendo el query IV - Promedio de la última evaluación
                string queryIV =
                    "SELECT S.NumberScore, " +
                           "S.LetterScore " +
                    "FROM Assessments A, " +
                         "Scores S " +
                    "WHERE A.AssessmentId = @AssessmentId AND " +
                          "A.AssessmentId = S.AssessmentId";
                #endregion

                #region Operaciones para query I
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                sqlCommand.Parameters.AddWithValue("@PeriodId", PeriodId);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                
                while (reader.Read())
                {
                    SiHayData = true;
                    CourseId = reader["CourseId"].ToString();
                    CourseName = reader["CourseName"].ToString();
                    CourseGrade = Convert.ToInt32(reader["CourseGrade"].ToString());
                    CourseSection = reader["CourseSection"].ToString();
                    Materias += ":" + 
                                reader["SubjectId"].ToString() + ":" + 
                                reader["SubjectName"].ToString() + " (" +
                                reader["UserName"].ToString() + " " +
                                reader["UserLastName"].ToString() + ")_";
                }
                reader.Close();
                #endregion
                #region Operaciones para query II
                if (SiHayData)
                {
                    Materias = Materias.Remove(Materias.Length - 1); //Eliminando el último caracter

                    sqlCommand = new SqlCommand(queryII, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                    reader = sqlCommand.ExecuteReader();

                    if (reader.Read())
                        NroAlumnos = Convert.ToInt32(reader["NroAlumnos"].ToString());
                }
                #endregion
                #region Operaciones para queryIII
                reader.Close();
                sqlCommand = new SqlCommand(queryIII, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                reader = sqlCommand.ExecuteReader();

                if (reader.Read())
                {
                    SubjectName = reader["SubjectName"].ToString();
                    AssessmentName = reader["AssessmentName"].ToString();
                    AssessmentId = reader["AssessmentId"].ToString();
                }
                #endregion
                #region Operaciones para queryIV
                reader.Close();
                sqlCommand = new SqlCommand(queryIV, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@AssessmentId", AssessmentId);
                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    if (CourseGrade > 6) //Bachillerato
                        Promedio += Convert.ToInt32(reader["NumberScore"].ToString());
                    else //Primaria
                    {
                        if (reader["LetterScore"].ToString().ToUpper().Equals("A")) Promedio += 5;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("B")) Promedio += 4;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("C")) Promedio += 3;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("D")) Promedio += 2;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("E")) Promedio += 1;
                    }
                }

                Promedio = (double)Promedio / NroAlumnos;
                #endregion

                if (CourseGrade > 6) //Bachillerato
                    PromedioString = Math.Round(Promedio, 2).ToString();
                else //Primaria
                {
                    if (Math.Round(Promedio) == 1) PromedioString = "A";
                    else if (Math.Round(Promedio) == 2) PromedioString = "B";
                    else if (Math.Round(Promedio) == 3) PromedioString = "C";
                    else if (Math.Round(Promedio) == 4) PromedioString = "D";
                    else if (Math.Round(Promedio) == 5) PromedioString = "E";
                }

                result = new
                {
                    Success = true,
                    CourseId = CourseId,
                    CourseName = CourseName,
                    CourseGrade = CourseGrade,
                    CourseSection = CourseSection,
                    NroAlumnos = NroAlumnos,
                    Materias = Materias,
                    Assessment_Name = SubjectName + " - " + AssessmentName,
                    Promedio = PromedioString
                };
            }
            #endregion
            #region Catch
            catch (SqlException e)
            {
                result = new { Success = false, Exception = e.Message };
            }
            catch (Exception e)
            {
                result = new { Success = false, Exception = e.Message };
            }
            #endregion
            #region Finally
            finally
            {
                sqlConnection.Close();
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Home(string UserId)
        {
            #region Declaración de variables
            object result = null;
            SqlConnection sqlConnection = null;

            string SchoolId = "", School_Name = "", SchoolYearId = "", SchoolYear_StartDate = "",
                    SchoolYear_EndDate = "", PeriodId = "", Period_Name = "", Representative_Name = "",
                    Representative_LastName = "", Course_Name = "", CourseId = "", SubjectName = "",
                    PromedioString = "", AssessmentName = "", AssessmentId = "", StudentId = "", 
                    StudentName = "", School_Address = "", School_Phones = "";
            int Grade = 0, NroEstudiantes = 0;
            double Promedio = 0;
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el query
                string query =
                    "SELECT TOP(1) " +
                            "SCH.SchoolId SchoolId, " +
                            "SCH.Name School_Name, " +
                            "SCH.Address School_Address, " +
                            "SCH.Phone1 School_Phone1, " +
                            "SCH.Phone2 School_Phone2, " +
                            "SY.SchoolYearId SchoolYearId, " +
                            "CONVERT(DATE, SY.StartDate, 103) SchoolYear_StartDate, " +
                            "CONVERT(DATE, SY.EndDate, 103) SchoolYear_EndDate, " +
                            "P.PeriodId PeriodId, " +
                            "P.Name Period_Name, " +
                            "CONVERT(DATE, P.StartDate, 103) Period_StartDate, " +
                            "CONVERT(DATE, P.FinishDate, 103) Period_FinishDate, " +
                            "R.Name Representative_Name, " +
                            "R.LastName Representative_LastName, " +
                            "C.Name CourseName, " +
                            "C.CourseId CourseId, " +
                            "S.StudentId StudentId, " +
                            "S.FirstLastName Student_FirstLastName, " +
                            "S.SecondLastName Student_SecondLastName, " +
                            "S.FirstName Student_FirstName, " +
                            "S.SecondName Student_SecondName, " +
                            "C.Grade Grade " +
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
                #region Definiendo el query I/2 - Nro de estudiantes por curso
                string queryI2 =
                    "SELECT COUNT(SC.Student_StudentId) NroEstudiantes " +
                                 "FROM COURSES C, " +
                                      "StudentCourses SC " +
                                 "WHERE C.CourseId = @CourseId AND " +
                                       "C.CourseId = SC.Course_CourseId";
                #endregion
                #region Definiendo el query II - Última evaluación por curso
                string queryII =
                    "SELECT TOP(1) " +
                        "A.[Name] AssessmentName, " +
                        "A.AssessmentId AssessmentId, " +
                        "SUB.[Name] SubjectName " +
                    "FROM Assessments A, " +
                         "Courses C, " +
                         "CASUs CASU, " +
                         "Subjects SUB " +
                    "WHERE A.CASU_CourseId = CASU.CourseId AND " +
                          "A.CASU_PeriodId = CASU.PeriodId AND " +
                          "A.CASU_SubjectId = CASU.SubjectId AND " +
                          "CASU.CourseId = C.CourseId AND " +
                          "C.CourseId = @CourseId AND " +
                          "CASU.SubjectId = SUB.SubjectId " +
                    "ORDER BY A.FinishDate, A.EndHour";
                #endregion
                #region Definiendo el query III - Promedio de la última evaluación
                string queryIII =
                    "SELECT S.NumberScore, " +
                           "S.LetterScore " +
                    "FROM Assessments A, " +
                         "Scores S " +
                    "WHERE A.AssessmentId = @AssessmentId AND " +
                          "A.AssessmentId = S.AssessmentId";
                #endregion

                #region Operaciones para queryI
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@UserId", UserId);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                if (reader.Read())
                {
                    SchoolId = reader["SchoolId"].ToString();
                    School_Name = reader["School_Name"].ToString();
                    SchoolYearId = reader["SchoolYearId"].ToString();
                    SchoolYear_StartDate = 
                        Convert.ToDateTime(reader["SchoolYear_StartDate"].ToString()).ToShortDateString();
                    SchoolYear_EndDate = 
                        Convert.ToDateTime(reader["SchoolYear_EndDate"].ToString()).ToShortDateString();
                    PeriodId = reader["PeriodId"].ToString();
                    Period_Name = reader["Period_Name"].ToString();
                    Representative_Name = reader["Representative_Name"].ToString();
                    Representative_LastName = reader["Representative_LastName"].ToString();
                    Course_Name = reader["CourseName"].ToString();
                    CourseId = reader["CourseId"].ToString();
                    Grade = Convert.ToInt32(reader["Grade"].ToString());
                    StudentId = reader["StudentId"].ToString();
                    StudentName = reader["Student_FirstLastName"].ToString() + " " + 
                        reader["Student_SecondLastName"].ToString() + ", " +
                        reader["Student_FirstName"].ToString() + " " +
                        reader["Student_SecondName"].ToString();
                    School_Address = reader["School_Address"].ToString();
                    School_Phones = reader["School_Phone1"].ToString();
                    if (!reader["School_Phone2"].ToString().Equals(""))
                        School_Phones += " / " + reader["School_Phone2"].ToString();
                }
                #endregion
                #region Operaciones para queryI/2
                reader.Close();
                sqlCommand = new SqlCommand(queryI2, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                reader = sqlCommand.ExecuteReader();

                if(reader.Read())
                    NroEstudiantes = Convert.ToInt32(reader["NroEstudiantes"].ToString());
                #endregion
                #region Operaciones para queryII
                reader.Close();
                sqlCommand = new SqlCommand(queryII, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                reader = sqlCommand.ExecuteReader();

                if (reader.Read())
                {
                    SubjectName = reader["SubjectName"].ToString();
                    AssessmentName = reader["AssessmentName"].ToString();
                    AssessmentId = reader["AssessmentId"].ToString();
                }
                #endregion
                #region Operaciones para queryIII
                reader.Close();
                sqlCommand = new SqlCommand(queryIII, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@AssessmentId", AssessmentId);
                reader = sqlCommand.ExecuteReader();
                                
                while(reader.Read())
                {
                    if(Grade > 6) //Bachillerato
                        Promedio += Convert.ToInt32(reader["NumberScore"].ToString());
                    else //Primaria
                    {
                        if (reader["LetterScore"].ToString().ToUpper().Equals("A")) Promedio += 5;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("B")) Promedio += 4;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("C")) Promedio += 3;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("D")) Promedio += 2;
                        else if (reader["LetterScore"].ToString().ToUpper().Equals("E")) Promedio += 1;
                    }
                }
                Promedio = (double)Promedio / NroEstudiantes;

                if (Grade > 6) //Bachillerato
                    PromedioString = Math.Round(Promedio, 2).ToString();
                else //Primaria
                {
                    if (Math.Round(Promedio) == 1) PromedioString = "A";
                    else if (Math.Round(Promedio) == 2) PromedioString = "B";
                    else if (Math.Round(Promedio) == 3) PromedioString = "C";
                    else if (Math.Round(Promedio) == 4) PromedioString = "D";
                    else if (Math.Round(Promedio) == 5) PromedioString = "E";
                }
                #endregion
                #region Resultado final
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
                    Representative_LastName = Representative_LastName,
                    Course_Name = Course_Name,
                    Assessment_Name = SubjectName + " - " + AssessmentName,
                    Promedio = PromedioString,
                    StudentId = StudentId,
                    StudentName = StudentName,
                    School_Address = School_Address,
                    School_Phones = School_Phones
                };
                #endregion

                reader.Close();
            }
            #endregion
            #region Catch
            catch (SqlException e)
            {
                result = new { Success = false, Exception = e.Message };
            }
            catch (Exception e)
            {
                result = new { Success = false, Exception = e.Message };
            }
            #endregion
            #region Finally
            finally
            {
                sqlConnection.Close();
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Login(string Username, string Password)
        {
            #region Declaración de variables
            object result;
            SqlConnection sqlConnection = null;
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el query
                string query =
                    "SELECT PASSWORDHASH, REPRESENTATIVEID " +
                    "FROM REPRESENTATIVES " +
                    "WHERE EMAIL = @username";
                #endregion

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

                    bool valor = Logic.PasswordHash(Password, PasswordHash);

                    if(valor)
                        result = new { Success = valor, UserId = UserId };
                    else
                        result = new { Success = false, Exception = "Contraseña incorrecta." };
                }
                #endregion
                #region No hay data
                else
                    result = new { Success = false, Exception = "Nombre de usuario incorrecto." };
                #endregion
            }
            #endregion
            #region Catch
            catch (SqlException e)
            {
                result = new { Success = false, Exception = e.Message };
            }
            catch(Exception e)
            {
                result = new { Success = false, Exception = e.Message };
            }
            #endregion
            #region Finally
            finally 
            {
                sqlConnection.Close();
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Notifications(string UserId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            List<string> listaEstudiantes = new List<string>();
            List<string> listaCursos = new List<string>();
            SqlConnection sqlConnection = null;
            #endregion
            
            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region QueryI - Estudiantes y cursos respectivos
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
                #region QueryII - Notificaciones por estudiantes/cursos
                string query2 =
                    "SELECT N.Attribution, " +
                           "N.AlertType, " +
                           "CONVERT(DATE, N.DateOfCreation, 110) DateOfCreation, " +
                           "CONVERT(DATE, N.SendDate, 110) SendDate, " +
                           "N.Message, " +
                           "N.Automatic, " +
                           "N.User_Id UserId " +
                    "FROM NOTIFICATIONS N, " +
                         "SENTNOTIFICATIONS SN " +
                    "WHERE N.NotificationId = SN.NotificationId AND  " +
                         "(SN.Student_StudentId = @StudentId OR " +
                         "SN.Course_CourseId = @CourseId) " +
                    "ORDER BY N.SendDate";
                #endregion
                #region QueryIII - Usuario que crea la notificación (solo para los casos que aplican)
                string query3 =
                    "SELECT Name User_Name, " +
                           "LastName User_LastName " +
                    "FROM AspNetUsers " +
                    "WHERE Id = '@UserId'";
                #endregion

                #region Conexión - QueryI
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query1, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@UserId", UserId);
                SqlDataReader reader = sqlCommand.ExecuteReader();
                #endregion
                #region Lista de estudiantes & cursos
                while (reader.Read())
                {
                    listaEstudiantes.Add(reader["StudentId"].ToString());
                    listaCursos.Add(reader["CourseId"].ToString());
                }
                #endregion
                reader.Close();

                /* Nota (28-05-15): El Count() de la lista de estudiantes y la lista de cursos debe ser
                 * exactamente igual, ya que por cada alumno asociado al representante, se trae el curso 
                 * asociado a éste. - Rodrigo Uzcátegui. 
                 */
                for (int i = 0; i <= listaEstudiantes.Count() - 1; i++ )
                {
                    #region Id de estudiante & curso
                    string StudentId = listaEstudiantes[i];
                    string CourseId = listaCursos[i];
                    #endregion
                    #region Conexión - QueryII
                    sqlCommand = new SqlCommand(query2, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                    sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                    reader = sqlCommand.ExecuteReader();
                    #endregion
                    #region Lista de notificaciones
                    while (reader.Read())
                    {
                        string Attribution = reader["Attribution"].ToString();
                        string AlertType = reader["AlertType"].ToString();
                        string DateOfCreation = reader["DateOfCreation"].ToString();
                        string SendDate = reader["SendDate"].ToString();
                        string Message = reader["Message"].ToString();
                        string Automatic = reader["Automatic"].ToString();
                        string From = "";

                        #region Identificando el emisor
                        if(Automatic.Equals("True")) //Notificación automática
                            From = "Notificación Automática";
                        else
                        {
                            string TeacherId = reader["UserId"].ToString();

                            #region Conexión - QueryIII
                            SqlCommand sqlCommand2 = new SqlCommand(query3, sqlConnection);
                            sqlCommand2.Parameters.AddWithValue("@UserId", TeacherId);
                            SqlDataReader reader2 = sqlCommand2.ExecuteReader();
                            #endregion

                            if (reader2.Read())
                                From = "Prof. " + reader2["User_Name"].ToString() + " " + 
                                    reader2["User_LastName"].ToString();

                            reader2.Close();
                        }
                        #endregion

                        result.Add(new
                        {
                            Attribution = Attribution,
                            AlertType = AlertType,
                            DateOfCreation = DateOfCreation,
                            SendDate = SendDate,
                            Message = Message,
                            Automatic = Automatic,
                            From = From
                        });
                    }
                    #endregion
                    reader.Close();
                }
            }
            #endregion
            #region Catch
            catch (SqlException e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            catch (Exception e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            #endregion
            #region Finally
            finally
            {
                sqlConnection.Close();
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Statistics(string SchoolId, string SchoolYearId, string CourseId)
        {
            #region Declaración de variables
            List<object> result = new List<object>();

            #region Configurando la ruta de las imágenes
            string path = ConstantsRepository.STATISTICS_IMAGES_PATH_APP_UPLOADS;
            path += @"\School_" + SchoolId + @"\SchoolYear_" + SchoolYearId + @"\";

            string imgPath1 = "S" + SchoolId + "Y" + SchoolYearId + "C" + CourseId + "_" +
                ConstantsRepository.STATISTICS_IMG_1;
            string imgPath2 = "S" + SchoolId + "Y" + SchoolYearId + "C" + CourseId + "_" +
                ConstantsRepository.STATISTICS_IMG_2;

            imgPath1 = Path.Combine(Server.MapPath(path), imgPath1);
            imgPath2 = Path.Combine(Server.MapPath(path), imgPath2);
            #endregion
            #endregion
            
            try
            {
                #region Obteniendo imágenes desde rutas
                Image img1 = Image.FromFile(imgPath1);
                Image img2 = Image.FromFile(imgPath2);
                #endregion
                #region Operaciones de conversión a byte[]
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
                #endregion
                #region Añadiendo los resultados
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
                #endregion
            }
            #region Catch
            catch (SqlException e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            catch (Exception e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            #endregion
            #region Finally
            finally
            {
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string StudentsInfo(string UserId)
        {
            #region Declarando la variable de resultado
            List<object> result = new List<object>();
            SqlConnection sqlConnection = null;
            #endregion
            
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el query
                string query =
                    "SELECT S.* " +
                    "FROM STUDENTS S, " +
                         "RepresentativeStudents RS, " +
                         "Representatives R, " +
                         "StudentCourses SC, " +
                         "Courses C " +
                    "WHERE S.StudentId = RS.Student_StudentId AND " +
                          "RS.Representative_RepresentativeId = R.RepresentativeId AND " +
                          "R.RepresentativeId = @UserId AND " +
                          "S.StudentId = SC.Student_StudentId AND " +
                          "SC.Course_CourseId = C.CourseId " +
                    "ORDER BY C.Grade DESC";
                #endregion

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
            }
            #region Catch
            catch (SqlException e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            catch (Exception e)
            {
                result.Add(new { Success = false, Exception = e.Message });
            }
            #endregion
            #region Finally
            finally
            {
                sqlConnection.Close();
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }
    }
}