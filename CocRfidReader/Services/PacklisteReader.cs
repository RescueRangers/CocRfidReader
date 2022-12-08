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
    public class PacklisteReader
    {
        private IConfiguration configuration;
        private ILogger<PacklisteReader>? logger;

        public PacklisteReader(IConfiguration configuration, ILogger<PacklisteReader> logger = null)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<IEnumerable<Coc>> GetCocs(string packingListNumber)
        {
            try
            {
                using var sql = new SqlConnection(configuration.GetValue<string>("connectionString"));
                return await sql.QueryAsync<Coc>(@"SELECT CocNumber as PRODUKTIONSNR
                    FROM [dbo].[uf_GetICocsOnPackingSlip] ('DAT', @PackingListNumber)", 
                    new { PackingListNumber = packingListNumber });

            }
            catch (Exception ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }
        }
    }
}
