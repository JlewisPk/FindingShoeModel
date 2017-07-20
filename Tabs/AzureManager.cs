using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace Tabs
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<shoemodel> shoeTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("https://lewispkjang.azurewebsites.net");
            this.shoeTable = this.client.GetTable<shoemodel>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

		public async Task<List<shoemodel>> GetShoeInformation()
		{
			return await this.shoeTable.ToListAsync();
		}
		public async Task PostShoeInformation(shoemodel shoe)
		{
            await this.shoeTable.InsertAsync(shoe);
		}
        public async Task DeleteShoeInformation(shoemodel shoe)
        {
            await this.shoeTable.DeleteAsync(shoe);
        }
    }
}