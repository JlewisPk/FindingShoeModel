using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Tabs.Model;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tabs
{
    public partial class SearchModel : ContentPage
    {
        public SearchModel()
        {
            InitializeComponent();
        }


		private async void accessGallery(object sender, EventArgs e)
        {
			if (PredictionLabel.Text != null)
			{
				PredictionLabel.Text = null;
				PredictionLabel2.Text = null;
			}
			if (TagLabel.Text != null)
			{
				TagLabel.Text = null;
				TagLabel2.Text = null;
			}
			if (!CrossMedia.Current.IsPickPhotoSupported)
			{
				await DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
				return;
			}
            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
				PhotoSize = PhotoSize.Medium
			});


			if (file == null)
				return;

			image.Source = ImageSource.FromStream(() =>
			{
				return file.GetStream();
			});
            await MakePredictionRequest(file);
        }

		private async void loadCamera(object sender, EventArgs e)
		{
			if (PredictionLabel.Text != null)
			{
				PredictionLabel.Text = null;
				PredictionLabel2.Text = null;
			}
			if (TagLabel.Text != null)
			{
				TagLabel.Text = null;
				TagLabel2.Text = null;
			}
			await CrossMedia.Current.Initialize();

			if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
			{
				await DisplayAlert("No Camera", ":( No camera available.", "OK");
				return;
			}

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                SaveToAlbum = true,
				PhotoSize = PhotoSize.Medium,
				Directory = "Sample",
				Name = $"{DateTime.UtcNow}.jpg"
			});

			if (file == null)
				return;

			image.Source = ImageSource.FromStream(() =>
			{
				return file.GetStream();
			});
			
			

			await MakePredictionRequest(file);
		}

		static byte[] GetImageAsByteArray(MediaFile file)
		{
			var stream = file.GetStream();
			BinaryReader binaryReader = new BinaryReader(stream);
			return binaryReader.ReadBytes((int)stream.Length);
		}

		async Task MakePredictionRequest(MediaFile file)
		{
            Processing.IsRunning = true;
            Processing.IsVisible = true;
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add("Prediction-Key", "2c8600cda0a845ee834da798650406f7");

			string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/1ddda278-879a-4c38-a935-ea3c153545a2/image?iterationId=45bf0bca-d790-4dd6-b00f-ec17f96abc20";

			HttpResponseMessage response;

			byte[] byteData = GetImageAsByteArray(file);

			using (var content = new ByteArrayContent(byteData))
			{

				content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				response = await client.PostAsync(url, content);
                int value;
				 if (response.IsSuccessStatusCode)
                {
                    
					var responseString = await response.Content.ReadAsStringAsync();
                    JObject rss = JObject.Parse(responseString);
                    //Truncate values to labels in XAML
                    string tagId, tagName;


                    float probability = (float)rss["Predictions"][0]["Probability"];

                    //var Tag = from p in rss["Predictions"] select (string)p["Tag"];
                    if (int.TryParse((string)rss["Predictions"][0]["Tag"], out value)) {
						tagId = (string)rss["Predictions"][0]["Tag"];
						tagName = (string)rss["Predictions"][1]["Tag"];
                    } else {
						tagName = (string)rss["Predictions"][0]["Tag"];
						tagId = (string)rss["Predictions"][1]["Tag"];
                    }


					shoemodel instance = new shoemodel();
                    instance.Id = tagId;
                    instance.Name = tagName;


					///// option 1   //////////////////////////////////////////////////////////////////////////////////////////////////////////////
					if (probability >0.5) {
						

						bool contained = true;
						List<shoemodel> shoeInformation = await AzureManager.AzureManagerInstance.GetShoeInformation();
						foreach (shoemodel x in shoeInformation)
						{
							if (((string)x.Id).Equals((string)instance.Id))
							{
								contained = true;
                                break;
							}
							else
							{
								contained = false;
							}
						}
						if (contained.Equals(false))
						{
							PredictionLabel.Text = "Model ID: ";
							PredictionLabel2.Text += tagId;
							TagLabel.Text = "Model Name: ";
							TagLabel2.Text = tagName;
							await AzureManager.AzureManagerInstance.PostShoeInformation(instance);
                        } else {
							PredictionLabel.Text = "Model ID: ";
							PredictionLabel2.Text += tagId;
							TagLabel.Text = "Model Name: ";
							TagLabel2.Text = tagName;
                        }

                    } else {
                        PredictionLabel.Text = "The model is not found. Try other model.";
                    }

					////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
					// 
					// 
					// option 2 : Do probability check right after the line no.136 then do this.
					// 
					//PredictionLabel.Text = "Model ID: ";
					//PredictionLabel2.Text += tagId;
					//TagLabel.Text = "Model Name: ";
					//TagLabel2.Text = tagName;
					//bool contained = true;
					//List<shoemodel> shoeInformation = await AzureManager.AzureManagerInstance.GetShoeInformation();
					//foreach (shoemodel x in shoeInformation)
					//{
					//	if (((string)x.Id).Equals((string)instance.Id))
					//	{
					//		contained = true;
					//	}
					//	else
					//	{
					//		contained = false;
					//	}
					//}
					//if (contained.Equals(false))
					//{
					//	await AzureManager.AzureManagerInstance.PostShoeInformation(instance);
					//}




				} else {
                    PredictionLabel.Text = "Something went wrong. Try again.";
                }
                Processing.IsRunning = false;
                Processing.IsVisible = false;
				//Get rid of file once we have finished using it
				file.Dispose();
			}
		}
    }
}
