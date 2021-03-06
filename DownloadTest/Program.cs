﻿using s3.amazon.com.docsamples;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DownloadTest
{
    public class A : IXmlSerializable
    {
        public A()
        {
            tableData = new ObservableCollection<B>();
        }

        //public string personName
        //{
        //    get;
        //    set;
        //}

        private ObservableCollection<B> _tableData;
        public ObservableCollection<B> tableData
        {
            get
            {
                return _tableData;
            }
            set
            {
                _tableData = value;
            }
        }

        // Xml Serialization Infrastructure

        public void WriteXml(XmlWriter writer)
        {
            //writer.WriteAttributeString("personName", tableData);
            writer.WriteStartElement("tableData");
            writer.WriteAttributeString("count", tableData.Count.ToString());
            var otherSer = new XmlSerializer(typeof(B));
            foreach (B other in tableData)
            {
                otherSer.Serialize(writer, other);
            }
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            //personName = reader.GetAttribute("personName");
            //string xml = reader.ReadInnerXml();
            int count;
            reader.MoveToContent();
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "tableData")
                    {
                        XElement el = XNode.ReadFrom(reader) as XElement;
                        if(el.FirstAttribute.Name == "count")
                        {
                            count = Int32.Parse(el.FirstAttribute.Value);
                        }
                        foreach(XElement node in el.Nodes())
                        {
                            if(node.NodeType == XmlNodeType.Element)
                            {
                                var otherSer = new XmlSerializer(typeof(B));
                                if (node.Name == "B")
                                {
                                    B other = (B)otherSer.Deserialize(new StringReader(node.ToString()));
                                    //tableData.Add(new B() { b_name = node.Value });
                                    tableData.Add(other);
                                }
                            }
                        }
                    }

                }
                //if(reader.NodeType == XmlNodeType.Attribute)
                //{

                //}
            }

            //reader.ReadStartElement("A");
            //xml = reader.ReadInnerXml();

            //reader.ReadStartElement("tableData");

            //xml = reader.ReadInnerXml();
            //if (reader.MoveToAttribute("count"))
            //if (reader.MoveToElement())
            //{
            //    var v = reader.GetAttribute("count");
            //}
            ////var v = reader.Value;
            //int count = int.Parse(reader.Value);

            //var otherSer = new XmlSerializer(typeof(B));
            //for (int i = 0; i < count; i++)
            //{
            //    var other = (B)otherSer.Deserialize(reader);
            //    tableData.Add(other);
            //}

            //reader.ReadEndElement();
            //reader.ReadEndElement();
        }

        public XmlSchema GetSchema()
        {
            return (null);
        }
    }

    public class B
    {
        public B() { }
        public string b_name
        {
            get;
            set;
        }
    }

    //https://s3-us-west-2.amazonaws.com/udtdownloads/SqlLocalDB.msi
    class Program
    {
        private static long GetURLFileLength(string url, string fileName)
        {
            //A a = new A();
            //a.tableData.Add(new B() { b_name = "first" });
            //a.tableData.Add(new B() { b_name = "second" });

            //XmlSerializer serializer = new XmlSerializer(a.GetType());

            //using (StringWriter writer = new StringWriter())
            //{
            //    serializer.Serialize(writer, a);

            //    string XML = writer.ToString();
            //}

            long length = -1;
            var webRequest = HttpWebRequest.Create(
                string.Format("https://s3-us-west-2.amazonaws.com/udtdownloads/{0}", fileName));
            webRequest.Method = "HEAD";

            using (var webResponse = webRequest.GetResponse())
            {
                length = Int64.Parse(webResponse.Headers.Get("Content-Length"));
            }
            Console.WriteLine("File {0} length = {1}", fileName, length);
            return length;
        }

        //url += string.Format("?fileName={0}", fileName);
        //var webReq = (HttpWebRequest)WebRequest.Create(url);
        //webReq.Method = "GET";
        //try
        //{
        //    WebResponse response = webReq.GetResponse();
        //    long length = 0;

        //    using (Stream stream = response.GetResponseStream())
        //    {
        //        using (StreamReader sr = new StreamReader(stream))
        //        {
        //            length = Int64.Parse(sr.ReadToEnd());
        //        }
        //    }
        //    Console.WriteLine("File {0} length = {1}", fileName, length);

        //}
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("For file '{0}' {1}", fileName, ex.Message);
        //    }
        //}

        private static async Task<bool> GetURLContentsAsync(string url, string fileName)
        {
            // The downloaded resource ends up in the variable named content.
            var content = new MemoryStream();

            byte[] data = new byte[5000000];
            string baseUrl = ConfigurationManager.AppSettings["downloadurl"];
            //url += string.Format("?fileName={0}", fileName);
            string bolbUrl = string.Format("{0}/{1}", baseUrl, fileName);

            // Initialize an HttpWebRequest for the current URL.
            var webReq = (HttpWebRequest)WebRequest.Create(bolbUrl);
            webReq.Method = "GET";
            
            // **Call GetResponseAsync instead of GetResponse, and await the result.
            // GetResponseAsync returns a Task.
            int count = 5000000;
            int total = 0;
            int chunk = 0;
            Console.WriteLine("");
            Console.WriteLine("Loading file {0}", fileName);
            WebResponse response = null;
            try
            {
                response = await webReq.GetResponseAsync();
                // Get the data stream that is associated with the specified URL.
                using (Stream responseStream = response.GetResponseStream())
                {
                    while (count > 0)
                    {
                        count = await responseStream.ReadAsync(data, 0, 5000000);
                        // TBD: write to file stream
                        content.Write(data, 0, count);
                        if (count > 0)
                        {
                            chunk += count;
                            total += count;
                            if (chunk >= 500000)
                            {
                                Console.WriteLine("Chunk size {0} total {1} KB.", chunk, total / 1000);
                                chunk = 0;
                            }
                        }
                    }
                    Console.WriteLine("Chunk size {0} total {1} KB.", chunk, total / 1000);

                }
                return true;
            }
            catch(Exception ex)
            {
                //return response;
                return false;
            }
            // Return the result as a byte array.
            // content.ToArray();
        }

        static string url = "http://localhost:54946/api/Download/FileLength";
        static void Main(string[] args)
        {
            A a = new A();
            //a.personName = "my name";
            a.tableData.Add(new B() { b_name = "first" });
            a.tableData.Add(new B() { b_name = "second" });

            XmlSerializer serializer = new XmlSerializer(a.GetType());

            string XML = "";
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, a);

                XML = writer.ToString();
            }


            using (TextReader reader = new StringReader(XML))
            {
                A result = serializer.Deserialize(reader) as A;
            }


            //UploadObject.AWStest(null);
            //return;

            //long length = GetURLFileLength(url, "UtdAppPublish.zip");
            //length = GetURLFileLength(url, "SqlLocalDB.msi");
            //length = GetURLFileLength(url, "sqlncli32.msi");
            //length = GetURLFileLength(url, "sqlncli64.msi");
            //GetURLFileLength(url, "missing");

            //url = "http://localhost:54946/api/Download/GetFile";
            //Task<bool> t = GetURLContentsAsync(url, "SqlLocalDB.msi");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "UtdAppPublish.zip");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "sqlncli32.msi");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "sqlncli64.msi");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");

            //t = GetURLContentsAsync(url, "missing");
            //t.Wait();
            //if (!t.Result) Console.WriteLine("file load failed");


            //HttpWebResponse r = t.Result as HttpWebResponse;
            //Console.WriteLine("Request status: {0}", r.StatusCode);
            //r.Dispose();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }
    }
}
