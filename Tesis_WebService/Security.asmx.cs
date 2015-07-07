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
            Dictionary<int, string> listaNotas = new Dictionary<int, string>();

            string Assessment = "", 
                   Score = "", 
                   Period = "", 
                   DefinitivaString = "";

            int Grade = 0, 
                Percentage = 0,
                contadorI = 0,
                contadorII = 0,
                contadorIII = 0,
                AssessmentId = 0;

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
                #region Definiendo el query I - Evaluaciones con notas del estudiante
                string queryI =
                    "SELECT A.Name AssessmentName, " +
                           "A.AssessmentId, " +
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
                #region Definiendo el query II - La lista de todas las evaluaciones respectivas
                string queryII = 
                    "SELECT A.Name AssessmentName, " +
                        "A.AssessmentId, " +
                        "A.Percentage AssessmentPercentage, " +
                        "C.Grade, " +
                        "P.Name Period " +
                    "FROM Assessments A, " +
                         "Courses C, " +
                         "Periods P " +
                    "WHERE A.CASU_CourseId =  @CourseId AND " +
                          "A.CASU_SubjectId = @SubjectId AND " +
                          "A.CASU_CourseId = C.CourseId AND " + 
                          "A.CASU_PeriodId = P.PeriodId";
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
                    AssessmentId = Convert.ToInt32(reader["AssessmentId"].ToString());
                    Score = (Grade > 6 ? reader["NumberScore"].ToString() : reader["LetterScore"].ToString());                    

                    listaNotas.Add(AssessmentId, Score);                                                            
                }
                reader.Close();
                sqlConnection.Close();
                #endregion
                #region Operaciones para query II
                sqlConnection.Open();
                sqlCommand = new SqlCommand(queryII, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                sqlCommand.Parameters.AddWithValue("@SubjectId", SubjectId);
                reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    Grade = Convert.ToInt32(reader["Grade"].ToString());
                    Percentage = Convert.ToInt32(reader["AssessmentPercentage"].ToString());
                    Assessment = (Grade > 6 ? reader["AssessmentName"].ToString() + " (" + Percentage.ToString() + "%)" : reader["AssessmentName"].ToString());
                    AssessmentId = Convert.ToInt32(reader["AssessmentId"].ToString());
                    Period = reader["Period"].ToString();
                    Score = (listaNotas.ContainsKey(AssessmentId) ? Score = listaNotas[AssessmentId] : "N/A");

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
                        {
                            if (Score.Equals("N/A"))
                                calculoI += (double)(1 * Percentage) / 100;
                            else
                                calculoI += (double)(Convert.ToInt32(Score) * Percentage) / 100;
                        }
                        #endregion
                        #region Primaria
                        else //Primaria
                        {
                            if (Score.Equals("N/A")) calculoI += 1;
                            else if (Score.Equals("A")) calculoI += 5;
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
                        {
                            if (Score.Equals("N/A"))
                                calculoII += (double)(1 * Percentage) / 100;
                            else
                                calculoII += (double)(Convert.ToInt32(Score) * Percentage) / 100;
                        }
                        #endregion
                        #region Primaria
                        else //Primaria
                        {
                            if (Score.Equals("N/A")) calculoII += 1;
                            else if (Score.Equals("A")) calculoII += 5;
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
                        {
                            if (Score.Equals("N/A"))
                                calculoIII += (double)(1 * Percentage) / 100;
                            else
                                calculoIII += (double)(Convert.ToInt32(Score) * Percentage) / 100;
                        }
                        #endregion
                        #region Primaria
                        else //Primaria
                        {
                            if (Score.Equals("N/A")) calculoIII += 1;
                            else if (Score.Equals("A")) calculoIII += 5;
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

                #region Definitiva
                if (Grade > 6) //Bachillerato
                {
                    definitiva = (double)(calculoI + calculoII + calculoIII) / 3;
                    DefinitivaString = Math.Round(definitiva, 2).ToString();
                }
                else //Primaria
                {
                    definitiva = (double)((double)(calculoI / contadorI) +
                                 (double)(calculoII / contadorII) +
                                 (double)(calculoIII / contadorIII)) / 3;

                    if (Math.Round(definitiva) == 5) DefinitivaString = "A";
                    else if (Math.Round(definitiva) == 4) DefinitivaString = "B";
                    else if (Math.Round(definitiva) == 3) DefinitivaString = "C";
                    else if (Math.Round(definitiva) == 2) DefinitivaString = "D";
                    else if (Math.Round(definitiva) == 1) DefinitivaString = "E";
                }

                result.Add(new { Definitiva = DefinitivaString });
                #endregion

                reader.Close();
                sqlConnection.Close();
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
        public string AssessmentsToday(string SchoolId, string Grade, string CourseId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            SqlConnection sqlConnection = null;
            string MonthNow = DateTime.Now.Month.ToString();
            Dictionary<int, string> diccionarioMaterias = new Dictionary<int, string>();
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el queryI - Materias del grado & año escolar
                string queryI = 
                    "SELECT S.SubjectId, " + 
                           "S.[Name] SubjectName " + 
                    "FROM SUBJECTS S " + 
                    "WHERE S.School_SchoolId = @SchoolId AND " + 
                          "S.Grade = @Grade";
                #endregion
                #region Definiendo el queryII - Evaluaciones de este mes por materia
                string queryII =
                    "SELECT A.AssessmentId, " + 
                           "A.[Name] AssessmentName, " + 
                           "A.Percentage, " + 
                           "CONVERT(CHAR(2), A.StartDate, 103) StartDate_Day, " + 
                           "CONVERT(CHAR(2), A.StartDate, 101) StartDate_Month, " + 
                           "CONVERT(CHAR(4), A.StartDate, 121) StartDate_Year, " + 
                           "CONVERT(CHAR(2), A.FinishDate, 103) FinishDate_Day, " + 
                           "CONVERT(CHAR(2), A.FinishDate, 101) FinishDate_Month, " + 
                           "CONVERT(CHAR(4), A.FinishDate, 121) FinishDate_Year, " + 
                           "A.CASU_CourseId, " + 
                           "A.CASU_PeriodId, " + 
                           "A.CASU_SubjectId " + 
                    "FROM ASSESSMENTS A " + 
                    "WHERE A.CASU_CourseId = @CourseId AND " + 
                          "A.CASU_SubjectId = @SubjectId AND " + 
                          "(CONVERT(CHAR(4), SYSDATETIME(), 100) = CONVERT(CHAR(4), A.StartDate, 100) OR " + 
                          "CONVERT(CHAR(4), SYSDATETIME(), 100) = CONVERT(CHAR(4), A.FinishDate, 100))";
                #endregion

                #region Operaciones para query I
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@SchoolId", SchoolId);
                sqlCommand.Parameters.AddWithValue("@Grade", Grade);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    int SubjectId = Convert.ToInt32(reader["SubjectId"].ToString());
                    string SubjectName = reader["SubjectName"].ToString();

                    diccionarioMaterias.Add(SubjectId, SubjectName);
                }

                reader.Close();
                sqlConnection.Close();
                #endregion

                #region Ciclo por cada materia respectiva
                foreach (KeyValuePair<int, string> PairValue in diccionarioMaterias)
                {
                    int SubjectId = PairValue.Key;
                    string SubjectName = PairValue.Value;

                    #region Operaciones para query II
                    sqlConnection.Open();
                    sqlCommand = new SqlCommand(queryII, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@CourseId", CourseId);
                    sqlCommand.Parameters.AddWithValue("@SubjectId", SubjectId);
                    reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        int AssessmentId = Convert.ToInt32(reader["AssessmentId"].ToString());
                        
                        string StartDate_Month = reader["StartDate_Month"].ToString();
                        string StartDate_Day = reader["StartDate_Day"].ToString();
                        string StartDate_Year = reader["StartDate_Year"].ToString();

                        string FinishDate_Month = reader["FinishDate_Month"].ToString();
                        string FinishDate_Day = reader["FinishDate_Day"].ToString();
                        string FinishDate_Year = reader["FinishDate_Year"].ToString();

                        #region Definiendo el nombre de la evaluación
                        string Name = reader["AssessmentName"].ToString() + " (" + 
                            reader["Percentage"].ToString() + "%)";
                        
                        if (!StartDate_Month.Equals(FinishDate_Month) || !StartDate_Day.Equals(FinishDate_Day))
                        {
                            if (StartDate_Month.Equals(MonthNow))
                                Name = "Inicia - " + Name;
                            else
                                Name = "Finaliza - " + Name;
                        }

                        Name += " [" + SubjectName + "]";
                        #endregion

                        result.Add(new
                        {
                            Name = Name,
                            StartDate = StartDate_Day + "/" + StartDate_Month + "/" + StartDate_Year,
                            FinishDate = FinishDate_Day + "/" + FinishDate_Month + "/" + FinishDate_Year
                        });
                    }

                    reader.Close();
                    sqlConnection.Close();
                    #endregion
                }
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
        public string Careers(string StudentId, string SchoolId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            SqlConnection sqlConnection = null;
            /* Tipos de razonamiento: 
             *      Razonamiento verbal: #1
             *      Razonamiento numérico: #2
             */
            Dictionary<int, float> porcentajesRazonamiento = new Dictionary<int, float>();
            Dictionary<int, int> listaMaterias = new Dictionary<int, int>();
            Dictionary<KeyValuePair<int, string>, double> acumuladosPorMateriaGrado =
                new Dictionary<KeyValuePair<int, string>, double>();

            string argumento = "";
            bool Success = false;

            #region Definiendo lista de materias de razonamientos
            /*Dictionary<string, float> listaMateriasRazonamiento = new Dictionary<string, float>();
            //Razonamiento numérico
            listaMateriasRazonamiento.Add("Matemática", 0);
            listaMateriasRazonamiento.Add("Física", 0);

            //Razonamiento verbal
            listaMateriasRazonamiento.Add("Castellano", 0);
            //Se suman estas dos
            listaMateriasRazonamiento.Add("Historia de Venezuela", 0); 
            listaMateriasRazonamiento.Add("Historia Universal", 0);
            listaMateriasRazonamiento.Add("Geografía", 0);
            listaMateriasRazonamiento.Add("Psicología", 0);
            listaMateriasRazonamiento.Add("Inglés", 0);*/
            double razonamientoVerbal = 0;
            double razonamientoNumerico = 0;
            int contadorVerbal = 0;
            int contadorNumerico = 0;
            #endregion

            string SubjectName = "";
            #endregion
            #region Variables auxiliares de cálculo
            double acumulativoLapsoI = 0;
            double acumulativoLapsoII = 0;
            double acumulativoLapsoIII = 0;
            double acumulativoGrado = 0;
            bool pase = false;
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el query I - Los scores del Test psicológico
                string queryI = "SELECT PTS.ReasoningType, " + 
                                       "PTS.PsychologicalTestId, " + 
                                       "PTS.StudentId, " + 
                                       "PTS.Score " + 
                                "FROM Students S, " + 
                                     "PsychologicalTest_Score PTS " + 
                                "WHERE S.StudentId = @StudentId AND " + 
                                      "S.StudentId = PTS.StudentId";
                #endregion
                #region Definiendo el query II - Las materias respectivas
                string queryII = 
                    "SELECT DISTINCT " + 
                        "S.SubjectId, " + 
                        "S.[Name], " + 
                        "S.Grade " + 
                    "FROM Subjects S, " + 
                         "Courses C, " + 
                         "CASUs CASU " + 
                    "WHERE CASU.CourseId = C.CourseId AND " + 
                          "CASU.SubjectId = S.SubjectId AND " + 
                          "C.School_SchoolId = @SchoolId AND " + 
                          "C.Grade = @CourseGrade AND " + 
                          "C.Grade = S.Grade AND " + 
                          "C.School_SchoolId = S.School_SchoolId";
                #endregion
                #region Definiendo el query III - Los assessments asociados al estudiante
                string queryIII =
                    "SELECT Sco.NumberScore, " + 
                           "A.Percentage, " + 
                           "A.[Name] AssessmentName, " + 
                           "Su.[Name] SubjectName, " + 
                           "Su.Grade, " + 
                           "P.[Name] Period " +
                    "FROM Assessments A, " + 
                         "Courses C, " + 
                         "CASUs CASU, " + 
                         "StudentCourses SC, " + 
                         "Students St, " + 
                         "Scores Sco, " + 
                         "Subjects Su, " + 
                         "Periods P " +
                    "WHERE " + 
                          //Validación con Student
                          "St.StudentId = @StudentId AND " +
                          "St.StudentId = SC.Student_StudentId AND " +
                          //Validación con Course
                          "SC.Course_CourseId = C.CourseId AND " +
                          "C.Grade = @CourseGrade AND " +
                          "C.School_SchoolId = @SchoolId AND " +
                          //Validación con CASU
                          "CASU.CourseId = C.CourseId AND " +
                          "CASU.CourseId = A.CASU_CourseId AND " +
                          "CASU.SubjectId = A.CASU_SubjectId AND " +
                          "CASU.SubjectId = Su.SubjectId AND " +
                          "CASU.PeriodId = A.CASU_PeriodId AND " + 
                          "CASU.PeriodId = P.PeriodId AND " +
                          //Validación con Subject
                          "Su.SubjectId = @SubjectId AND " +
                          "Su.School_SchoolId = C.School_SchoolId AND " +
                          //Validación con Score
                          "Sco.StudentId = St.StudentId AND " +
                          "Sco.AssessmentId = A.AssessmentId";
                #endregion
                #region Definiendo el query IV - Carreras (razonamiento numérico)
                string queryIV = 
                    "SELECT C.CareerId, " + 
                           "C.Title, " + 
                           "C.[Type], " + 
                           "C.[Description], " + 
                           "C.OccupationalArea, " + 
                           "C.KnowledgeSubArea_KnowledgeSubAreaId " + 
                    "FROM Careers C " + 
                    "WHERE CareerId = 148 OR CareerId = 160 OR " + 
                          "CareerId = 70 OR CareerId = 23 OR " + 
                          "CareerId = 149 OR CareerId = 3 OR " + 
                          "CareerId = 27 OR CareerId = 2 OR " + 
                          "CareerId = 127 OR CareerId = 502 OR " + 
                          "CareerId = 163 OR CareerId = 446 OR " + 
                          "CareerId = 74 OR CareerId = 445 OR " + 
                          "CareerId = 436 " + 
                    "ORDER BY Title";
                #endregion
                #region Definiendo el query V - Carreras (razonamiento verbal)
                string queryV =
                    "SELECT C.CareerId, " +
                           "C.Title, " +
                           "C.[Type], " +
                           "C.[Description], " +
                           "C.OccupationalArea, " +
                           "C.KnowledgeSubArea_KnowledgeSubAreaId " +
                    "FROM Careers C " +
                    "WHERE CareerId = 528 OR CareerId = 342 OR " +
                          "CareerId = 532 OR CareerId = 527 OR " +
                          "CareerId = 531 OR CareerId = 529 OR " +
                          "CareerId = 358 OR CareerId = 586 OR " +
                          "CareerId = 340 OR CareerId = 596 OR " +
                          "CareerId = 334 OR CareerId = 598 OR " +
                          "CareerId = 353 OR CareerId = 599 OR " +
                          "CareerId = 600 OR CareerId = 518 OR " +
                          "CareerId = 481 OR CareerId = 207 OR " +
                          "CareerId = 488 OR CareerId = 208 OR " +
                          "CareerId = 214 OR CareerId = 595 OR " +
                          "CareerId = 589 OR CareerId = 322 " +
                     "ORDER BY Title";
                #endregion

                #region Operaciones para queryI
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    float PorcentajeRazonamiento = Convert.ToInt32(reader["Score"].ToString());
                    int TipoRazonamiento = Convert.ToInt32(reader["ReasoningType"].ToString());
                    porcentajesRazonamiento.Add(TipoRazonamiento, PorcentajeRazonamiento);
                }
                reader.Close();
                #endregion
                #region Operaciones para queryII
                for (int Grade = 7; Grade <= 11; Grade++)
                {
                    sqlCommand = new SqlCommand(queryII, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@SchoolId", SchoolId);
                    sqlCommand.Parameters.AddWithValue("@CourseGrade", Grade);
                    reader = sqlCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        int SubjectId = Convert.ToInt32(reader["SubjectId"].ToString());
                        int SubjectGrade = Convert.ToInt32(reader["Grade"].ToString());
                        listaMaterias.Add(SubjectId, SubjectGrade);
                    }
                    reader.Close();
                }
                #endregion
                #region Operaciones para queryIII
                #region Ciclo por grado
                for (int Grade = 7; Grade <= 11; Grade++)
                {
                    #region Ciclo por materia
                    foreach (KeyValuePair<int, int> value in listaMaterias.Where(m => m.Value == Grade))
                    {
                        #region Conectando el query
                        sqlCommand = new SqlCommand(queryIII, sqlConnection);
                        sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                        sqlCommand.Parameters.AddWithValue("@CourseGrade", Grade);
                        sqlCommand.Parameters.AddWithValue("@SchoolId", SchoolId);
                        sqlCommand.Parameters.AddWithValue("@SubjectId", value.Key);
                        reader = sqlCommand.ExecuteReader();
                        #endregion
                        #region Definiendo valor de variable pase
                        pase = (reader.Read() ? true : false);
                        #endregion
                        #region Ciclo de resultados del query
                        while (reader.Read())
                        {
                            #region Obtención de los datos
                            double NumberScore = Convert.ToInt32(reader["NumberScore"].ToString());
                            int Percentage = Convert.ToInt32(reader["Percentage"].ToString());
                            string AssessmentName = reader["AssessmentName"].ToString();
                            SubjectName = reader["SubjectName"].ToString();
                            string Period = reader["Period"].ToString();
                            #endregion

                            #region 1er Lapso
                            if(Period.Equals("1er Lapso"))
                                acumulativoLapsoI += NumberScore * ((double)(Percentage) / 100);
                            #endregion
                            #region 2do Lapso
                            else if (Period.Equals("2do Lapso"))
                                acumulativoLapsoII += NumberScore * ((double)(Percentage) / 100);
                            #endregion
                            #region 3er Lapso
                            else if (Period.Equals("3er Lapso"))
                                acumulativoLapsoIII += NumberScore * ((double)(Percentage) / 100);
                            #endregion
                        }
                        #endregion
                        #region Cálculo acumulativos
                        if (pase)
                        {
                            acumulativoGrado = Math.Round((double)(acumulativoLapsoI + acumulativoLapsoII + 
                                acumulativoLapsoIII) / 3, 2);

                            acumuladosPorMateriaGrado.Add(new KeyValuePair<int, string>(Grade, SubjectName),
                                acumulativoGrado);
                        }
                        #endregion
                        #region Reiniciando valores
                        acumulativoLapsoI = 0;
                        acumulativoLapsoII = 0;
                        acumulativoLapsoIII = 0;
                        acumulativoGrado = 0;
                        reader.Close();
                        #endregion
                    }
                    #endregion                
                }
                #endregion
                #endregion

                #region Agrupando las materias respectivas
                foreach (KeyValuePair<KeyValuePair<int, string>, double> pairValue in acumuladosPorMateriaGrado)
                {
                    if(pairValue.Key.Value.Equals("Matemática") || pairValue.Key.Value.Equals("Física"))
                    {
                        razonamientoNumerico += (double)pairValue.Value;
                        contadorNumerico++;
                    }
                    else if (pairValue.Key.Value.Equals("Castellano") || 
                             pairValue.Key.Value.Equals("Historia de Venezuela") ||
                             pairValue.Key.Value.Equals("Historia Universal") ||
                             pairValue.Key.Value.Equals("Geografía") ||
                             pairValue.Key.Value.Equals("Psicología") ||
                             pairValue.Key.Value.Equals("Inglés"))
                    {
                        razonamientoVerbal += (double)pairValue.Value;
                        contadorVerbal++;
                    }
                }
                #endregion
                #region Resultados de razonamientos verbal y numérico por materias
                razonamientoNumerico = (double)razonamientoNumerico / contadorNumerico;
                razonamientoVerbal = (double)razonamientoVerbal / contadorVerbal;
                #endregion
                #region Cálculo matriz valores referenciales de carreras a sugerir
                float razonamientoVerbalTest = porcentajesRazonamiento.Where(m => m.Key == 1).FirstOrDefault().Value;
                float razonamientoNumericoTest = porcentajesRazonamiento.Where(m => m.Key == 2).FirstOrDefault().Value;

                #region Caso #1 - Razonamiento <= 40%
                if (razonamientoVerbalTest <= 40 && razonamientoNumericoTest <= 40)
                {
                    argumento = "No se muestran carreras universitarias, debido a que los resultados " +
                    "obtenidos son de interpretación ambigua; requieren de otras técnicas de abordo " +
                    "psicológico que permitan involucrar elementos para la toma de decisión vocacional, como" +
                    " lo son: la evaluación de intereses personales, actitudes, valores, y entorno familiar.";
                    Success = false;

                    result.Add(new
                    {
                        Success = (Success ? "True" : "False"),
                        Argumento = argumento,
                        CareerId = "",
                        Title = ""
                    });
                }
                #endregion
                #region Caso #2 - <=13pts
                else if (razonamientoVerbal <= 13 && razonamientoNumerico <= 13) 
                {
                    argumento = "No se muestran carreras universitarias, debido a que los resultados " +
                    "obtenidos son de interpretación ambigua; requieren de otras técnicas de abordo " +
                    "psicológico que permitan involucrar elementos para la toma de decisión vocacional, como" +
                    " lo son: la evaluación de intereses personales, actitudes, valores, y entorno familiar.";
                    Success = false;

                    result.Add(new
                    {
                        Success = (Success ? "True" : "False"),
                        Argumento = argumento,
                        CareerId = "",
                        Title = ""
                    });
                }
                #endregion
                else
                {                    
                    #region Caso #3 - Razonamiento Verbal > 13 && > 14pts
                    if (razonamientoVerbal > 13 && razonamientoVerbalTest > 41)
                    {
                        Success = true;
                        #region Operaciones para query IV
                        sqlCommand = new SqlCommand(queryIV, sqlConnection);
                        reader = sqlCommand.ExecuteReader();

                        while (reader.Read())
                        {
                            string CareerId = reader["CareerId"].ToString();
                            string Title = reader["Title"].ToString();
                            string Type = reader["Type"].ToString();
                            string Description = reader["Description"].ToString();
                            string OccupationalArea = reader["OccupationalArea"].ToString();
                            string KnowledgeSubAreaId = reader["KnowledgeSubArea_KnowledgeSubAreaId"].ToString();

                            result.Add(new
                            {
                                Success = (Success ? "True" : "False"),
                                Argumento = argumento,
                                CareerId = CareerId,
                                Title = Title,                                
                            });
                        }
                        reader.Close();
                        #endregion
                    }
                    #endregion
                    #region Caso #4 - Razonamiento Numérico > 13 && > 14pts
                    if (razonamientoNumerico > 13 && razonamientoNumericoTest > 41)
                    {
                        Success = true;
                        #region Operaciones para query V
                        sqlCommand = new SqlCommand(queryV, sqlConnection);
                        reader = sqlCommand.ExecuteReader();

                        while (reader.Read())
                        {
                            string CareerId = reader["CareerId"].ToString();
                            string Title = reader["Title"].ToString();
                            string Type = reader["Type"].ToString();
                            string Description = reader["Description"].ToString();
                            string OccupationalArea = reader["OccupationalArea"].ToString();
                            string KnowledgeSubAreaId = reader["KnowledgeSubArea_KnowledgeSubAreaId"].ToString();

                            result.Add(new
                            {
                                Success = (Success ? "True" : "False"),
                                Argumento = argumento,
                                CareerId = CareerId,
                                Title = Title,
                            });
                        }
                        reader.Close();
                        #endregion
                    }
                    #endregion
                }
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
        public string CareersInfo(string CareerId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            SqlConnection sqlConnection = null;
            bool pase = true; /* Variable utilizada para controlar los datos a insertar en la lista de 
                               * resultados */
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el queryI - Info de la carrera
                string queryI = 
                    "SELECT C.[Type] CareerType, " + 
                           "C.[Description] CareerDescription, " + 
                           "C.OccupationalArea, " + 
                           "I.InstituteId, " + 
                           "I.[Name] InstituteName, " + 
                           "I.Profile InstituteProfile " + 
                    "FROM Careers C, " + 
                         "Opportunities O, " + 
                         "Cores Co, " + 
                         "Institutes I " + 
                    "WHERE C.CareerId = @CareerId AND " + 
                          "C.CareerId = O.CareerId AND " + 
                          "O.CoreId = Co.CoreId AND " + 
                          "O.InstituteId = Co.InstituteId AND " + 
                          "Co.InstituteId = I.InstituteId " + 
                    "ORDER BY I.[Name]";
                #endregion

                #region Operaciones para queryI
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@CareerId", CareerId);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    string CareerDescription = reader["CareerDescription"].ToString();
                    string OccupationalArea = reader["OccupationalArea"].ToString();
                    string InstituteId = reader["InstituteId"].ToString();
                    string InstituteName = reader["InstituteName"].ToString();

                    #region Acción por booleano "pase"
                    if (pase)
                    {
                        result.Add(new {
                            CareerDescription = CareerDescription,
                            OccupationalArea = OccupationalArea
                        });
                        pase = false; //Controlando la data a insertar
                    }
                    #endregion

                    result.Add(new {
                        InstituteId = InstituteId,
                        InstituteName = InstituteName,
                    });
                }
                reader.Close();

                #region Eliminando duplicados
                result = result.Distinct().ToList<object>();
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
        public string CoreInfo(string CoreId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            SqlConnection sqlConnection = null;
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el queryI - Info del núcleo
                string queryI =
                    "SELECT C.CoreName, " + 
                           "C.Address " + 
                    "FROM Cores C " +
                    "WHERE C.CoreId = @CoreId";
                #endregion

                #region Operaciones para queryI
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@CoreId", CoreId);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {                    
                    string Address = reader["Address"].ToString();

                    result.Add(new
                    {                 
                        Address = Address,
                    });
                }
                reader.Close();
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
                #region Definiendo el query 0
                string query0 = 
                    "SELECT MAX(C.Grade) Grade " + 
                    "FROM Students ST, " + 
                         "StudentCourses SC, " + 
                         "Courses C, " + 
                         "CASUs CASU, " + 
                         "Periods P, " + 
                         "SchoolYears SY " + 
                    "WHERE ST.StudentId = @StudentId AND " + 
                          "ST.StudentId = SC.Student_StudentId AND " + 
                          "SC.Course_CourseId = C.CourseId AND " + 
                          "CASU.CourseId = C.CourseId AND " + 
                          "CASU.PeriodId = P.PeriodId AND " + 
                          "P.SchoolYear_SchoolYearId = SY.SchoolYearId AND " + 
                          "SY.Status = 1";
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

                #region Operaciones para query 0
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(query0, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                if(reader.Read())
                    CourseGrade = Convert.ToInt32(reader["Grade"].ToString());

                reader.Close();
                #endregion
                #region Operaciones para query I
                sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@StudentId", StudentId);
                sqlCommand.Parameters.AddWithValue("@PeriodId", PeriodId);
                reader = sqlCommand.ExecuteReader();
                
                while (reader.Read())
                {
                    SiHayData = true;
                    CourseId = reader["CourseId"].ToString();
                    CourseName = reader["CourseName"].ToString();
                    CourseSection = reader["CourseSection"].ToString();
                    int auxGrade = Convert.ToInt32(reader["CourseGrade"].ToString());

                    if(auxGrade == CourseGrade)
                    {
                        Materias += ":" +
                                reader["SubjectId"].ToString() + ":" +
                                reader["SubjectName"].ToString() + " (" +
                                reader["UserName"].ToString() + " " +
                                reader["UserLastName"].ToString() + ")_";
                    }
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
                          "CAST(P.FinishDate AS DATE) >= CAST(GETDATE() AS DATE) " +
                    "ORDER BY C.Grade DESC, " + 
                             "S.FirstName, " + 
                             "S.SecondName";
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
                    CourseId = CourseId,
                    CourseGrade = Grade,
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
        public string InstituteInfo(string InstituteId, string CareerId)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            SqlConnection sqlConnection = null;
            #endregion

            #region Try
            try
            {
                #region Estableciendo la conexión a BD
                sqlConnection = Conexion();
                #endregion
                #region Definiendo el queryI - Info de la carrera
                string queryI =
                    "SELECT I.InstituteId, " +
                           "I.[Name] InstituteName, " +
                           "I.Profile InstituteProfile, " +
                           "C.CoreId, " +
                           "C.CoreName, " +
                           "C.Address " +
                    "FROM Institutes I, " +
                         "Cores C, " +
                         "Opportunities O " +
                    "WHERE I.InstituteId = @InstituteId AND " +
                          "I.InstituteId = C.InstituteId AND " +
                          "O.CareerId = @CareerId AND " +
                          "O.CoreId = C.CoreId AND " + 
                          "O.InstituteId = I.InstituteId";
                #endregion

                #region Operaciones para queryI
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(queryI, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@InstituteId", InstituteId);
                sqlCommand.Parameters.AddWithValue("@CareerId", CareerId);
                SqlDataReader reader = sqlCommand.ExecuteReader();

                while (reader.Read())
                {
                    string InstituteProfile = reader["InstituteProfile"].ToString();
                    string CoreName = reader["CoreName"].ToString();
                    string CoreId = reader["CoreId"].ToString();

                    result.Add(new
                    {
                        InstituteProfile = InstituteProfile,
                        CoreName = CoreName,
                        CoreId = CoreId,
                    });
                }
                reader.Close();
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
                           "CONVERT(CHAR(2), N.DateOfCreation, 103) DateOfCreation_Day, " +
                           "CONVERT(CHAR(2), N.DateOfCreation, 101) DateOfCreation_Month, " +
                           "CONVERT(CHAR(4), N.DateOfCreation, 121) DateOfCreation_Year, " +
                           "CONVERT(CHAR(2), N.SendDate, 103) SendDate_Day, " +
                           "CONVERT(CHAR(2), N.SendDate, 101) SendDate_Month, " +
                           "CONVERT(CHAR(4), N.SendDate, 121) SendDate_Year, " +
                           "N.Message, " +
                           "N.Automatic, " +
                           "N.User_Id UserId, " +
                           "SN.SentNotificationId, " +
                           "SN.[Read], " +
                           "SN.Sent, " +
                           "SN.New " +
                    "FROM NOTIFICATIONS N, " +
                         "SENTNOTIFICATIONS SN " +
                    "WHERE N.NotificationId = SN.NotificationId AND  " +
                         "(SN.Student_StudentId = @StudentId OR " +
                         "SN.Course_CourseId = @CourseId) " +
                    "ORDER BY N.DateOfCreation DESC";
                #endregion
                #region QueryIII - Usuario que crea la notificación (solo para los casos que aplican)
                string query3 =
                    "SELECT Name User_Name, " +
                           "LastName User_LastName " +
                    "FROM AspNetUsers " +
                    "WHERE Id = @UserId";
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
                        string DateOfCreation = reader["DateOfCreation_Day"].ToString() +
                            "/" + reader["DateOfCreation_Month"].ToString() + 
                            "/" + reader["DateOfCreation_Year"].ToString();
                        string SendDate = reader["SendDate_Day"].ToString() + 
                            "/" + reader["SendDate_Month"].ToString() + 
                            "/" + reader["SendDate_Year"].ToString();
                        string Message = reader["Message"].ToString();
                        string Automatic = reader["Automatic"].ToString();
                        string From = "";
                        string NotificationId = reader["SentNotificationId"].ToString();
                        string Read = reader["Read"].ToString();
                        string New = reader["New"].ToString();

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
                        #region Resultado final
                        result.Add(new
                        {
                            Attribution = Attribution,
                            AlertType = AlertType,
                            DateOfCreation = DateOfCreation,
                            SendDate = SendDate,
                            Message = Message,
                            Automatic = Automatic,
                            From = From,
                            NotificationId = NotificationId,
                            Read = Read,
                            New = New
                        });
                        #endregion
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

                result.Add(new
                {
                    Attribution = "",
                    AlertType = "",
                    DateOfCreation = "",
                    SendDate = "",
                    Message = "",
                    Automatic = "",
                    From = "",
                    NotificationId = "",
                    Read = "",
                    New = ""
                });
            }
            #endregion

            return new JavaScriptSerializer().Serialize(result);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string NewNotifications(string UserId)
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
                           "CONVERT(CHAR(2), N.DateOfCreation, 103) DateOfCreation_Day, " +
                           "CONVERT(CHAR(2), N.DateOfCreation, 101) DateOfCreation_Month, " +
                           "CONVERT(CHAR(4), N.DateOfCreation, 121) DateOfCreation_Year, " +
                           "CONVERT(CHAR(2), N.SendDate, 103) SendDate_Day, " +
                           "CONVERT(CHAR(2), N.SendDate, 101) SendDate_Month, " +
                           "CONVERT(CHAR(4), N.SendDate, 121) SendDate_Year, " +
                           "N.Message, " +
                           "N.Automatic, " +
                           "N.User_Id UserId, " +
                           "SN.SentNotificationId, " +
                           "SN.[Read], " +
                           "SN.Sent, " +
                           "SN.New " +
                    "FROM NOTIFICATIONS N, " +
                         "SENTNOTIFICATIONS SN " +
                    "WHERE N.NotificationId = SN.NotificationId AND  " +
                         "(SN.Student_StudentId = @StudentId OR " +
                         "SN.Course_CourseId = @CourseId) AND " +
                         "SN.New = 1 " +
                    "ORDER BY N.DateOfCreation DESC";
                #endregion
                #region QueryIII - Usuario que crea la notificación (solo para los casos que aplican)
                string query3 =
                    "SELECT Name User_Name, " +
                           "LastName User_LastName " +
                    "FROM AspNetUsers " +
                    "WHERE Id = @UserId";
                #endregion
                #region Query IV - Actualizador de 'News' para notificaciones
                string query4 =
                    "UPDATE SentNotifications " +
                    "SET New = 0 " +
                    "WHERE SentNotificationId = @SentNotificationId";
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
                for (int i = 0; i <= listaEstudiantes.Count() - 1; i++)
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
                        string DateOfCreation = reader["DateOfCreation_Day"].ToString() +
                            "/" + reader["DateOfCreation_Month"].ToString() +
                            "/" + reader["DateOfCreation_Year"].ToString();
                        string SendDate = reader["SendDate_Day"].ToString() +
                            "/" + reader["SendDate_Month"].ToString() +
                            "/" + reader["SendDate_Year"].ToString();
                        string Message = reader["Message"].ToString();
                        string Automatic = reader["Automatic"].ToString();
                        string From = "";
                        string NotificationId = reader["SentNotificationId"].ToString();
                        string Read = reader["Read"].ToString();
                        string New = reader["New"].ToString();

                        #region Identificando el emisor
                        if (Automatic.Equals("True")) //Notificación automática
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
                        #region Resultado final
                        result.Add(new
                        {
                            Attribution = Attribution,
                            AlertType = AlertType,
                            DateOfCreation = DateOfCreation,
                            SendDate = SendDate,
                            Message = Message,
                            Automatic = Automatic,
                            From = From,
                            NotificationId = NotificationId,
                            Read = Read,
                            New = New
                        });
                        #endregion

                        #region Proceso para actualizar el new de las notificaciones
                        if (New.Equals("True"))
                        {
                            SqlConnection sqlConnection2 = Conexion();
                            sqlConnection2.Open();
                            SqlCommand sqlCommand3 = sqlConnection2.CreateCommand();
                            sqlCommand3.CommandText = query4;
                            sqlCommand3.Parameters.AddWithValue("@SentNotificationId", NotificationId);
                            sqlCommand3.ExecuteNonQuery();
                            sqlCommand3.Dispose();
                        }
                        #endregion
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
            string imgPath3 = "S" + SchoolId + "Y" + SchoolYearId + "C" + CourseId + "_" +
                ConstantsRepository.STATISTICS_IMG_3;

            imgPath1 = Path.Combine(Server.MapPath(path), imgPath1);
            imgPath2 = Path.Combine(Server.MapPath(path), imgPath2);
            imgPath3 = Path.Combine(Server.MapPath(path), imgPath3);
            #endregion
            #endregion
            
            try
            {
                #region Obteniendo imágenes desde rutas
                Image img1 = Image.FromFile(imgPath1);
                Image img2 = Image.FromFile(imgPath2);
                Image img3 = Image.FromFile(imgPath3);
                #endregion
                #region Operaciones de conversión a byte[]
                MemoryStream stream1 = new MemoryStream();
                MemoryStream stream2 = new MemoryStream();
                MemoryStream stream3 = new MemoryStream();

                img1.Save(stream1, System.Drawing.Imaging.ImageFormat.Bmp);
                img2.Save(stream2, System.Drawing.Imaging.ImageFormat.Bmp);
                img3.Save(stream3, System.Drawing.Imaging.ImageFormat.Bmp);

                byte[] imageByte1 = stream1.ToArray();
                byte[] imageByte2 = stream2.ToArray();
                byte[] imageByte3 = stream3.ToArray();

                string imageBase64_1 = Convert.ToBase64String(imageByte1);
                string imageBase64_2 = Convert.ToBase64String(imageByte2);
                string imageBase64_3 = Convert.ToBase64String(imageByte3);

                stream1.Dispose(); stream2.Dispose(); stream3.Dispose();

                img1.Dispose(); img2.Dispose(); img3.Dispose();
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

                result.Add(new
                {
                    Title = "Top 10 resultados deficientes",
                    Image = imageBase64_3
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
        public void StatisticsImageGenerator(int type, int CourseId, int SchoolYearId, int SchoolId,
            string imageBase64)
        {
            #region Declaracíón de variables globales
            string path = ConstantsRepository.STATISTICS_IMAGES_PATH_APP_UPLOADS;
            path += @"\School_" + SchoolId + @"\SchoolYear_" + SchoolYearId + @"\";

            string imgPath = "";
            #endregion
            #region Definiendo el nombre de la estadística
            switch (type)
            {
                case ConstantsRepository.MOBILE_STATISTICS_CODE_AprobadosVsReprobados:
                    imgPath = "S" + SchoolId + "Y" + SchoolYearId + "C" + CourseId + "_" +
                        ConstantsRepository.STATISTICS_IMG_1;                                        
                    break;
                case ConstantsRepository.MOBILE_STATISTICS_CODE_Top10ResultadosDestacados:
                    imgPath = "S" + SchoolId + "Y" + SchoolYearId + "C" + CourseId + "_" +
                        ConstantsRepository.STATISTICS_IMG_2;
                    break;
                case ConstantsRepository.MOBILE_STATISTICS_CODE_Top10ResultadosDeficientes:
                    imgPath = "S" + SchoolId + "Y" + SchoolYearId + "C" + CourseId + "_" +
                        ConstantsRepository.STATISTICS_IMG_3;
                    break;
            }
            #endregion
            #region Obteniendo la ruta de la imagen
            imgPath = Path.Combine(Server.MapPath(path), imgPath);
            #endregion
            #region Descifrando la imagen
            byte[] imageBytes = Convert.FromBase64String(imageBase64);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image img = Image.FromStream(ms, true);
            #endregion

            img.Save(imgPath);
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
                    "SELECT DISTINCT S.* " +
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
                    "ORDER BY S.FirstName, " + 
                             "S.SecondName";
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

        /// <summary>
        /// Método definido para actualizar aquellas notificaciones que ya están leídas. Utilizando el atributo
        /// 'Read'.
        /// </summary>
        /// <param name="ArrayIds">El string de la lista de Ids de las notificaciones leídas, separadas por una
        /// coma (,)</param>
        [WebMethod]
        public void UpdateNotifications(string ArrayIds)
        {
            #region Declarando variables
            List<object> result = new List<object>();
            List<string> listaEstudiantes = new List<string>();
            List<string> listaCursos = new List<string>();
            SqlConnection sqlConnection = null;
            #endregion
            #region Query I - Update notifications
            string query1 = "UPDATE SentNotifications " +
                            "SET [Read] = 1 " +
                            "WHERE SentNotificationId = @SentNotificationId";
            #endregion

            #region Try
            try
            {
                #region Separando el string
                string[] ArrayIds_Array = ArrayIds.Split(',');
                #endregion
                

                #region Ciclo de actualizaciones
                for (int i = 0; i < ArrayIds_Array.Length; i++)
                {
                    #region Estableciendo la conexión a BD
                    sqlConnection = Conexion();
                    #endregion
                    #region Conexión - QueryI (Parte I)
                    sqlConnection.Open();
                    SqlCommand sqlCommand = sqlConnection.CreateCommand();
                    #endregion
                    #region Obteniendo id del SentNotification
                    int SentNotificationId = Convert.ToInt32(ArrayIds_Array[i]);
                    #endregion
                    #region Conexión - QueryI (Parte II)
                    sqlCommand.CommandText = query1;
                    sqlCommand.Parameters.AddWithValue("@SentNotificationId", SentNotificationId);
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Dispose();
                    sqlConnection.Close();
                    #endregion
                }
                #endregion
            }
            #endregion
            #region Catch
            catch (SqlException)
            {
            }
            catch (Exception)
            {
            }
            #endregion
        }
    }
}