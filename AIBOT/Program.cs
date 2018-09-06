using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;

namespace AIBOT
{
    class Program
    {
        //using microsot cognitive api to detect faces
        FaceServiceClient faceServiceClient = new FaceServiceClient("3cfd478015af47dba813c7fa4f1b8f78", "https://westus.api.cognitive.microsoft.com/face/v1.0");


        //Creating a Group to identify 
        public async void CreatePersonGroup(string personGroupId, string personGroupName)
        {
            try
            {
                await faceServiceClient.CreatePersonGroupAsync
                    (personGroupId, personGroupName);
                //CreatePersonResult person = await faceServiceClient.CreatePersonAsync(personGroupId, personGroupName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error please check your code"+ex.Message);
            }
        }



        public async void AddPersonToGroup(string personGroupId, string name, string pathImage )
        {
            try
            {
                await faceServiceClient.GetPersonGroupAsync(personGroupId);
                CreatePersonResult person = await faceServiceClient.CreatePersonAsync(personGroupId, name); // this will fetch different detected persons


                DetectFaceAndRegister(personGroupId, person, pathImage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error please check your code" + ex.Message);
            }





        }

        private async void DetectFaceAndRegister(string personGroupId, CreatePersonResult person, string pathImage)
        {
            try
            {
               foreach(var imgPath in Directory.GetFiles(pathImage, "*.jpg")){
                    using (Stream s = File.OpenRead(imgPath)) {
                        await faceServiceClient.AddPersonFaceAsync(personGroupId, person.PersonId, s);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error please check your code" + ex.Message);
            }





        }

        public async void TrainingAI(string personGroupId) {
            await faceServiceClient.TrainPersonGroupAsync(personGroupId);
            TrainingStatus trainingStatus = null;
            while (true) {

                trainingStatus = await faceServiceClient.GetPersonGroupTrainingStatusAsync(personGroupId);

                if (trainingStatus.Status != Status.Running)
                    break;
                await Task.Delay(1000);
            }
            Console.WriteLine("Training AI completed");
            
        }

        public async void RecognitionFace(string PersongroupId , string imgPath ) {
           
            using (Stream s = File.OpenRead(imgPath)) {

                var faces = await faceServiceClient.DetectAsync(s);
                var faceIds = faces.Select(face => face.FaceId).ToArray();

                try
                {
                    var results = await faceServiceClient.IdentifyAsync(PersongroupId, faceIds);
                    foreach (var identifyResult in results) {
                        Console.WriteLine($"The face is: {identifyResult.FaceId} ");
                        if (identifyResult.Candidates.Length == 0)
                            Console.WriteLine("no no no");
                        else {
                            //fetches all the results
                            var canidateId = identifyResult.Candidates[0].PersonId;
                            var person = await faceServiceClient.GetPersonAsync(PersongroupId, canidateId);
                            Console.WriteLine($"Identified as {person.Name}");
                            
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }
        }

        static void Main(string[] args)
        {

            // new Program().CreatePersonGroup("s", "hollywood123");
           // new Program().AddPersonToGroup("s", "Tom Cruise", @"C:\Users\suleman\Desktop\Usman_App\Training AI\TomCruise\");
           // new Program().AddPersonToGroup("s", "Hems Worth", @"C:\Users\suleman\Desktop\Usman_App\Training AI\hemsworth\");

             //new Program().TrainingAI("s");
           new Program().RecognitionFace("hollywood123", @"C:\Users\suleman\Desktop\Usman_App\Training AI\hemsworth\3.jpg");
            Console.ReadLine();
        }
    }
}
