using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;
using Newtonsoft.Json;

namespace Vacal
{
    class Program
    {
        static void Main(string[] args)
        {
            //SpeechToText(false).Wait();
            //SpeechToEng().Wait();
            SpeechToText(true).Wait();
        }

        static async Task SpeechToEng()
        {
            Console.WriteLine("Fale Algo");
            var speechConfig = SpeechTranslationConfig.FromSubscription("3196cdd9d39f435099c60741d9ef412e", "eastus");
            speechConfig.SpeechRecognitionLanguage = "pt-BR";

            // speechConfig.AddTargetLanguage("en-US");
            var langs = new List<string> { "en-US", "es-AR" };
            langs.ForEach(speechConfig.AddTargetLanguage);

            var recognizer = new TranslationRecognizer(speechConfig);
            var result = await recognizer.RecognizeOnceAsync();
            if (result.Reason == ResultReason.TranslatedSpeech)
            {
                Console.WriteLine(result.Text);
                foreach (var lang in result.Translations)
                {
                    Console.WriteLine($"{lang.Key} - {lang.Value}");
                }
            }

            Console.ReadLine();
        }

        static async Task SpeechToText(bool isGpt)
        {
            var speechConfig = SpeechConfig.FromSubscription("3196cdd9d39f435099c60741d9ef412e"
                , "eastus");
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            var recognizer = new SpeechRecognizer(speechConfig, "pt-BR", audioConfig);
            string pergunta = isGpt ? "Faça uma pergunta !" : "Falai alguma coisa:";
            Console.WriteLine(pergunta);

            while (true)
            {
                var result = await recognizer.RecognizeOnceAsync();
                var texto = result.Text;
                Console.WriteLine($"{texto} ");
                if (texto.ToLower().Contains("sair."))
                {
                    break;
                }

                if (isGpt)
                {
                    if(texto.Length > 0)
                        await Gpt(texto);
                }
                Task.Delay(5000);
                pergunta = isGpt ? "Faça uma pergunta !" : "Falai alguma coisa:";
                Console.WriteLine(pergunta);
            }
           
        }

        static async Task Gpt(string texto)
        {

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("authorization", "Bearer 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");
            // string texto2 = Console.ReadLine();
            var content = new StringContent("{\"model\": \"text-davinci-002\", \"prompt\": \"" + texto + "\",\"temperature\": 1,\"max_tokens\": 100}",
                Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/completions", content);

            string responseString = await response.Content.ReadAsStringAsync();

            try
            {
                var dyData = JsonConvert.DeserializeObject<GPT>(responseString);

                string responseGPT = dyData!.choices[0].text;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(responseGPT);
                Console.ResetColor();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"---> Não foi possível desserializar o JSON: {ex.Message}");
            }



        }


    }
}
