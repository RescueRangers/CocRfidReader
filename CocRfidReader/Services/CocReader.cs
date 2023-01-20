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
        private ConfigurationService configuration;
        private ILogger<CocReader>? logger;

        public CocReader(ConfigurationService configuration, ILogger<CocReader> logger = null)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<Coc> GetAsync(string epc, CancellationToken cancellationToken)
        {
            try
            {
                var query = @"
with production as (
SELECT TOP (1) 
       A.[PRODUKTIONSNR]
	  ,B.ItemNumber
	  ,B.Name
	  ,B.ItemText
  FROM [Prosign_ItemControl].[dbo].[TagCoc] A
  INNER JOIN [Prosign_ItemControl].[dbo].[CocPrintQueueLog] B on A.PRODUKTIONSNR = B.PRODUKTIONSNR
  WHERE EPC = @EPC
  ),
  account as (
  select top (1)
  Account as AccountNumber FROM [c5sql].[dbo].PRODORDRE where KUNDEORDRE = (SELECT p.KUNDEORDRE
              FROM [c5sql].[dbo].[PRODKART] p
              WHERE p.[DATASET] = 'DAT'
                     AND p.PRODUKTIONSNR = (select PRODUKTIONSNR from production)) 
  )
  SELECT * from production, account
";
                using var sql = new SqlConnection(configuration.GetSettings().ConnectionString);
                return await sql.QueryFirstOrDefaultAsync<Coc>(new CommandDefinition(query, parameters: new { EPC = epc }, cancellationToken: cancellationToken));

            }
            catch (SqlException ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }
            
        }
    }
}
