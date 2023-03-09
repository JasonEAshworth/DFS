using System;

public class SQLisBAD
{
    string connectionString = "Server=.\PDATA_SQLEXPRESS;Database=;User Id=sa;Password=2BeChanged!;";

    public SQLisBAD()
    {
    }

    public void InjectSQL(string author)
    {
        var queryString = "SELECT Title, Body, Excerpt FROM Post WHERE author = '" + author + "' ORDER BY Published DESC";
	using (SqlConnection connection = new SqlConnection(connectionString))
	{
            SqlCommand command = new SqlCommand(queryString, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    Console.WriteLine(String.Format("{0}, {1}",
                    reader["tPatCulIntPatIDPk"], reader["tPatSFirstname"]));// etc
                }
            }
            finally
            {
                // Always call Close when done reading.
                reader.Close();
            }
        }
    }
}
