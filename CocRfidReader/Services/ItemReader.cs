using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocRfidReader.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CocRfidReader.Services
{
    public class ItemReader
    {
        private IConfiguration configuration;
        private ILogger<ItemReader>? logger;

        public ItemReader(IConfiguration configuration, ILogger<ItemReader> logger = null)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<TagItem> GetAsync(string epc)
        {
            try
            {
                using var sql = new SqlConnection(configuration.GetValue<string>("connectionString"));

                return await sql.QueryFirstOrDefaultAsync<TagItem>(@"
SELECT [Id]
      ,[EPC]
      ,[DataSet]
      ,[ItemNumber]
      ,[ItemName1]
      ,[InsertedTimestamp]
  FROM [Prosign_ItemControl].[dbo].[TagItem]
  WHERE EPC = @EPC", new { EPC = epc });
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }
        }
    }
}
