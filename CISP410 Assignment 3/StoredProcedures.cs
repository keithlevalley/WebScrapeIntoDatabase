using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.IO;
using System.Xml;

namespace CISP410_Assignment_3
{
    class StoredProcedures
    {
        // used sample code from https://msdn.microsoft.com/en-us/library/5czye81z(v=vs.100).aspx

        // Method will return either the number of rows affected or -1 if the database connection failed
        [SqlProcedure()]
        public static int InsertLibrary(XmlDocument LogDoc, XmlDocument DataDoc)
        {
            // Create new trasaction node to hold records
            XmlNode transactionNode = LogDoc.CreateElement("Transaction");
            XmlNode recordNode = LogDoc.CreateElement("Record");
            XmlAttribute attribute;

            int affectedRows = -1;

                foreach (XmlNode node in DataDoc.DocumentElement.ChildNodes)
                {
                    if (node.HasChildNodes == true)
                    {
                    try // check for connection to database
                    {
                        using (SqlConnection conn = new SqlConnection("data source = (LocalDb)\\MSSQLLocalDB; initial catalog = CISP410Assignment3.LibHoursContext")) //CISP410Assignment3.LibHoursContext
                        {
                            SqlCommand InsertLibraryCommand = new SqlCommand();
                            SqlParameter LibName = new SqlParameter("@LibName", SqlDbType.NVarChar);
                            SqlParameter Monday = new SqlParameter("@Monday", SqlDbType.NVarChar);
                            SqlParameter Tuesday = new SqlParameter("@Tuesday", SqlDbType.NVarChar);
                            SqlParameter Wednesday = new SqlParameter("@Wednesday", SqlDbType.NVarChar);
                            SqlParameter Thursday = new SqlParameter("@Thursday", SqlDbType.NVarChar);
                            SqlParameter Friday = new SqlParameter("@Friday", SqlDbType.NVarChar);
                            SqlParameter Saturday = new SqlParameter("@Saturday", SqlDbType.NVarChar);
                            SqlParameter Sunday = new SqlParameter("@Sunday", SqlDbType.NVarChar);

                            LibName.Value = node.Attributes[0].Value.ToString();
                            Monday.Value = node.ChildNodes[0].InnerText;
                            Tuesday.Value = node.ChildNodes[1].InnerText;
                            Wednesday.Value = node.ChildNodes[2].InnerText;
                            Thursday.Value = node.ChildNodes[3].InnerText;
                            Friday.Value = node.ChildNodes[4].InnerText;
                            Saturday.Value = node.ChildNodes[5].InnerText;
                            Sunday.Value = node.ChildNodes[6].InnerText;

                            InsertLibraryCommand.Parameters.Add(LibName);
                            InsertLibraryCommand.Parameters.Add(Monday);
                            InsertLibraryCommand.Parameters.Add(Tuesday);
                            InsertLibraryCommand.Parameters.Add(Wednesday);
                            InsertLibraryCommand.Parameters.Add(Thursday);
                            InsertLibraryCommand.Parameters.Add(Friday);
                            InsertLibraryCommand.Parameters.Add(Saturday);
                            InsertLibraryCommand.Parameters.Add(Sunday);

                            InsertLibraryCommand.CommandText =
                                "INSERT Libhours (LibName, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday)" +
                                " VALUES(@LibName, @Monday, @Tuesday, @Wednesday, @Thursday, @Friday, @Saturday, @Sunday)";

                            InsertLibraryCommand.Connection = conn;
                            conn.Open();
                            affectedRows += InsertLibraryCommand.ExecuteNonQuery();

                            recordNode = LogDoc.CreateElement("Record");
                            attribute = LogDoc.CreateAttribute("Date");
                            attribute.Value = DateTime.Now.ToString();
                            recordNode.Attributes.Append(attribute);

                            attribute = LogDoc.CreateAttribute("DataFlow");
                            attribute.Value = "To Database";
                            recordNode.Attributes.Append(attribute);

                            attribute = LogDoc.CreateAttribute("Type");
                            attribute.Value = "New Record for " + LibName.Value;
                            recordNode.Attributes.Append(attribute);

                            transactionNode.AppendChild(recordNode);
                            
                            conn.Close();
                        } // end using
                    }
                    catch (Exception)
                    {
                        return affectedRows;
                    }
                    
                } // end if
            } // end foreach

            attribute = LogDoc.CreateAttribute("Date");
            attribute.Value = DateTime.Now.ToString();
            transactionNode.Attributes.Append(attribute);

            attribute = LogDoc.CreateAttribute("TransactionNumber");
            int temp = (LogDoc.GetElementsByTagName("Transaction").Count) + 1;
            attribute.Value = temp.ToString();
            transactionNode.Attributes.Append(attribute);

            LogDoc.LastChild.AppendChild(transactionNode);

            return affectedRows;
        }
    }
}