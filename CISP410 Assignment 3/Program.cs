using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Messaging;


namespace CISP410_Assignment_3
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            string xmlLogDocName = "xmlAssignment3LogFile.xml";
            string xmlDataDocName = "XmlDataFile.xml";
            string messageQ = @"ltditsnklevalle\private$\klevalley2";
            var i = 0;
            var loop = true;

            XmlDocument LogDoc = new XmlDocument();
            XmlDocument DataDoc = new XmlDocument();
            XmlAttribute attribute;
            XmlNode resultsNode = LogDoc.CreateElement("LogFile");
            XmlNode messageNode = LogDoc.CreateElement("Message");

            // Code example from https://msdn.microsoft.com/en-us/library/system.xml.xmldocument(v=vs.110).aspx

            LogDoc.PreserveWhitespace = true;
            // if log file doesn't exist create it
            try
            {
                LogDoc.Load(xmlLogDocName);
            }
            catch (System.IO.FileNotFoundException)
            {
                LogDoc.LoadXml("<?xml version=\"1.0\"?> \n" +
                "<LogFiles> \n" +
                "  <LogFile type=\"Information\" DataFlow=\"NA\" Date=\"" + DateTime.Now.ToString() + "\"> \n" +
                "    <Message>Log File Created</Message> \n" +
                "  </LogFile> \n" +
                "</LogFiles>");

                LogDoc.Save(xmlLogDocName);
            }

            // If data doc already exists, use that data for transaction
            // Else download data from website
            DataDoc.PreserveWhitespace = true;

            try
            {
                DataDoc.Load(xmlDataDocName);
                
                LogDoc.LastChild.AppendChild(newLogEntry(LogDoc, "From DataFile", "Information", "Data File used for Transaction"));
            }
            catch (System.IO.FileNotFoundException)
            {
                string libraryGuide = "";
                libraryGuide = client.DownloadString("http://libguides.davenport.edu/hours");
                libraryGuide = trimHTML(libraryGuide, "Gr. Rapids (Margaret D. Sneden Library)", "<!--Davenport Side Panel-->");

                // splits data into array and trims remaining white space
                string[] splitData = libraryGuide.Split('\n');

                for (int j = 0; j < splitData.Length; j++)
                {
                    splitData[j] = splitData[j].Trim();
                }

                // used for testing
                File.WriteAllText("libraryGuide.txt", libraryGuide);

                string[] locations = new string[4] { "Gr. Rapids", "Lansing", "Midland", "Warren" };

                DataDoc.LoadXml("<?xml version=\"1.0\"?> \n" +
               "<Record Date=\"" + DateTime.Now.ToString() + "\"> \n</Record>");

                XmlNode rootNode = DataDoc.DocumentElement;
                XmlNode LibraryNode;
                XmlNode dayNode;

                foreach (var location in locations)
                {
                    i = 0;
                    loop = true;

                    // find where data starts for library
                    while (loop)
                    {
                        if (splitData[i].Contains(location))
                        {
                            LibraryNode = DataDoc.CreateElement("Library");
                            attribute = DataDoc.CreateAttribute("name");
                            attribute.Value = location;
                            LibraryNode.Attributes.Append(attribute);

                            i += 2;

                            for (int j = 0; j < 7; j++)
                            {
                                dayNode = DataDoc.CreateElement(splitData[i]);
                                i++;
                                dayNode.InnerText = splitData[i];
                                i++;
                                LibraryNode.AppendChild(dayNode);
                            }

                            rootNode.AppendChild(LibraryNode);

                            DataDoc.Save(xmlDataDocName);
                            loop = false;
                        }
                        else
                        {
                            i++;
                        }
                    }

                } // end Foreach

                LogDoc.LastChild.AppendChild(newLogEntry(LogDoc, "From Website", "Information", "Data pulled from website"));

            } // end catch

            // Start Message queue
            if (!MessageQueue.Exists(messageQ))
            {
                MessageQueue.Create(messageQ);
                LogDoc.LastChild.AppendChild(newLogEntry(LogDoc, "NA", "Information", "Message queue created"));
            }
            try
            {
                MessageQueue myQueue = new MessageQueue(messageQ);

                myQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(XmlDocument) });

                Message message = new Message();

                myQueue.Formatter.Write(message, DataDoc);
                myQueue.Send(message);
                myQueue.Close();

                LogDoc.LastChild.AppendChild(newLogEntry(LogDoc, "To Message queue", "Information", "Data sent to Message queue"));
                File.Delete(xmlDataDocName);
            }
            catch (Exception)
            {
                LogDoc.LastChild.AppendChild(newLogEntry(LogDoc, "NA", "Warning", "Data not sent over message queue, DataFile saved"));
            }
            

            LogDoc.Save(xmlLogDocName);

        } // end Main



        //************************************************************************************//
        //                                 METHODS                                            //
        //************************************************************************************//


        // takes in a string containing an HTML Page and returns a simpliefied string with no tags.  Requires a string to identify where to start and end the page
        public static string trimHTML(string htmlPage, string startPoint, string endPoint)
        {
            int index = htmlPage.IndexOf(startPoint);

            // removes all text before index point
            htmlPage = htmlPage.Substring(index);

            index = htmlPage.IndexOf(endPoint);

            // removes all text after index point
            htmlPage = htmlPage.Substring(0, index);

            int firstIndex = 0;
            int secondIndex = 0;

            // removes all HTML tags
            while (htmlPage.Contains("<") && htmlPage.Contains(">"))
            {
                firstIndex = htmlPage.IndexOf("<");
                secondIndex = htmlPage.IndexOf(">") + 1;

                if (secondIndex > firstIndex)
                {
                    htmlPage = htmlPage.Remove(firstIndex, (secondIndex - firstIndex));
                }
                else break;
            }

            // trims white space
            htmlPage = htmlPage.Replace("\t", "");
            htmlPage = htmlPage.Trim();

            // trims white space and extra line breaks
            while (htmlPage.Contains("\n\n") || htmlPage.Contains("  ") || htmlPage.Contains("\n "))
            {
                htmlPage = htmlPage.Replace("\n\n", "\n");
                htmlPage = htmlPage.Replace("  ", " ");
                htmlPage = htmlPage.Replace("\n ", "\n");
            }

            return htmlPage;

        } // end method

        public static XmlNode newLogEntry (XmlDocument LogDoc, string dataFlow, string type, string message)
        {
            XmlAttribute attribute;
            XmlNode resultsNode = LogDoc.CreateElement("LogFile");
            XmlNode messageNode = LogDoc.CreateElement("Message");

            attribute = LogDoc.CreateAttribute("Date");
            attribute.Value = DateTime.Now.ToString();
            resultsNode.Attributes.Append(attribute);

            attribute = LogDoc.CreateAttribute("DataFlow");
            attribute.Value = dataFlow;
            resultsNode.Attributes.Append(attribute);

            attribute = LogDoc.CreateAttribute("Type");
            attribute.Value = type;
            resultsNode.Attributes.Append(attribute);

            messageNode.InnerText = message;

            resultsNode.AppendChild(messageNode);
            return resultsNode;
        }


    }
}
