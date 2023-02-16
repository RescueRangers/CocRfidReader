using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocRfidReader.Services;
using CocRfidReader.WPF.ViewModels;
using Impinj.OctaneSdk;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace CocRfidReader.WPF.Services
{
    public class TagsService
    {
        private ILogger<TagsService> logger;
        private ICocReader cocReader;

        public TagsService(ILogger<TagsService> logger, ICocReader cocReader)
        {
            this.logger = logger;
            this.cocReader = cocReader;
            cancellationTokenSource = new CancellationTokenSource();
        }

        private CancellationTokenSource cancellationTokenSource;

        public async Task<CocViewModel> GetCocFromTag(Tag tag, string accountNumber)
        {
            var token = cancellationTokenSource.Token;
            try
            {
                var epcValue = tag.Epc.ToHexString();
                logger?.LogInformation("Getting COC from the database");
                var coc = await cocReader.GetAsync(epcValue, token);

                if (coc == null)
                {
                    logger?.LogWarning($"Could not get coc number from EPC: {epcValue}");
                    throw new ArgumentException("Could not get the COC from supplied EPC");
                }

                logger?.LogInformation($"Database returned: {coc}");

                var cocVM = new CocViewModel(coc);
                if (GetHammingDistance(coc.AccountNumber, accountNumber) < 2)
                {
                    cocVM.IsAccountCorrect = true;
                }
                
                return cocVM;
            }
            catch (SqlException ex)
            {
                logger?.LogError(ex.Message);
                throw;
            }
        }

        private int GetHammingDistance(string s, string t)
        {
            if (s.Length != t.Length)
            {
                return 99;
            }

            int distance =
                s.ToCharArray()
                .Zip(t.ToCharArray(), (c1, c2) => new { c1, c2 })
                .Count(m => m.c1 != m.c2);

            return distance;
        }

        public void CancelOperations()
        {
            cancellationTokenSource.Cancel();
        }

    }
}
