using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCombinatorsExercises.Core
{
    public static class HttpClientExtensions
    {
        /*
         Write cancellable async method with timeout handling, that concurrently tries to get data from
         provided urls (first wins and its response is returned, rest is __cancelled__).
         
         Tips:
         * consider using HttpClient.GetAsync (as it is cancellable)
         * consider using Task.WhenAny
         * you may use urls like for testing https://postman-echo.com/delay/3
         * you should have problem with tasks cancellation -
            - how to merge tokens of operations (timeouts) with the provided token? 
            - Tip: you can link tokens with the help of CancellationTokenSource.CreateLinkedTokenSource(token)
         */
        public static async Task<string> ConcurrentDownloadAsync(this HttpClient httpClient,
            string[] urls, int millisecondsTimeout, CancellationToken token)
        {
            CancellationTokenSource localSource = new CancellationTokenSource(millisecondsTimeout);
            if(token != default)
            {
                localSource = CancellationTokenSource.CreateLinkedTokenSource(localSource.Token, token);
            }

            using (localSource)
            {
                List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

                foreach (string url in urls)
                {
                    tasks.Add(httpClient.GetAsync(url, localSource.Token));
                }

                Task<HttpResponseMessage> resultedTask = await Task.WhenAny(tasks);

                if(resultedTask.IsCanceled)
                    throw new TaskCanceledException();
                localSource.Cancel();
                return await (await resultedTask).Content.ReadAsStringAsync();
            }
        }
    }
}
