using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CallWebApi
{
    public class DataObject
    {
        public Int32 UserId { get; set; }
        public Int32 Id { get; set; }
        public String Title { get; set; }
        public String Body { get; set; }
    }

    public class CallWebApi
    {
        private const string URL = "https://jsonplaceholder.typicode.com/posts";

        static void Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var dt = PrepareDataTableForDataObjects();
            Console.WriteLine("Data table created and ready...");

            // List data response.
            HttpResponseMessage response = client.GetAsync(URL).Result;  
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body.
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<DataObject>>().Result;

                foreach (var d in dataObjects)
                {
                    dt.Rows.Add(d.UserId,d.Id,d.Body,d.Title);
                }
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            SaveToDatabase(dt);
            client.Dispose();

        }

        private static void SaveToDatabase(object dt)
        {
            try
            {
                using (SqlConnection conn =
                    new SqlConnection("Server=DESKTOP-DR8AVUI;Database=CallWebApiDev;Trusted_Connection=True;"))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("usp_call_web_api", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@dt", dt));

                    cmd.ExecuteNonQuery();

                    Console.WriteLine("Data saved...");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Data was not saved..."+e);
                throw;
            }
        }

        private static DataTable PrepareDataTableForDataObjects()
        {
            var table = new DataTable("PostData");
            DataColumn column = new DataColumn("id");
            column.DataType = Type.GetType("System.Int32");
            column.AutoIncrement = true;

            //DataTable Columns 
            table.Columns.Add("UserID", typeof(string));
            table.Columns.Add("PostId", typeof(string));
            table.Columns.Add("Title", typeof(string));
            table.Columns.Add("Body", typeof(string));

            return table;

        }
    }
}