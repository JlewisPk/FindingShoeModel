using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Xamarin.Forms;

namespace Tabs
{
    public partial class SearchTable : ContentPage
    {
		
		public SearchTable()
        {
            InitializeComponent();
            RetrieveInformation();
        }
        // https://store.nike.com/nz/en_gb/pw/n/1j7?sl=

        void OnMore (object sender, EventArgs e)
        {
            var mi = (shoemodel)((MenuItem)sender).CommandParameter;
			string xx = "https://store.nike.com/nz/en_gb/pw/n/1j7?sl=" + mi.Id;
            Device.OpenUri(new System.Uri(xx));
        }
		public async Task OnDelete(object sender, EventArgs e)
		{
            var mi = (shoemodel)((MenuItem)sender).CommandParameter;

			await AzureManager.AzureManagerInstance.DeleteShoeInformation(mi);
            RetrieveInformation();
		}
		void Handle_ClickedAsync(object sender, System.EventArgs e)
		{
            RetrieveInformation();
		}
        private async void RetrieveInformation()
        {
			List<shoemodel> shoeInformation = await AzureManager.AzureManagerInstance.GetShoeInformation();

			ShoeList.ItemsSource = shoeInformation;
        }
    }
}
