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
    public class CocReader
    {
        private IConfiguration configuration;
        private ILogger<CocReader>? logger;

        public CocReader(IConfiguration configuration, ILogger<CocReader> logger = null)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<Coc> GetAsync(string epc)
        {
            try
            {
                using var sql = new SqlConnection(configuration.GetValue<string>("connectionString"));
                return await sql.QueryFirstOrDefaultAsync<Coc>(@"
SELECT [Id]
      ,[EPC]
      ,[DataSet]
      ,[PRODUKTIONSNR]
      ,[InsertedTimestamp]
  FROM [Prosign_ItemControl].[dbo].[TagCoc]
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
