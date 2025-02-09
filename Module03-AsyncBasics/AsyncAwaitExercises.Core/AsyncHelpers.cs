﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAwaitExercises.Core
{
    public static class AsyncHelpers
    {
        private const int MS_TO_SECONDS_FACTOR = 1000;
        
        public static Task<string> GetStringWithRetries(HttpClient client, string url, int maxTries = 3, CancellationToken token = default)
        {
            // Create a method that will try to get a response from a given `url`, retrying `maxTries` number of times.
            // It should wait one second before the second try, and double the wait time before every successive retry
            // (so pauses before retries will be 1, 2, 4, 8, ... seconds).
            // * `maxTries` must be at least 2
            // * we retry if:
            //    * we get non-successful status code (outside of 200-299 range), or
            //    * HTTP call thrown an exception (like network connectivity or DNS issue)
            // * token should be able to cancel both HTTP call and the retry delay
            // * if all retries fails, the method should throw the exception of the last try
            // HINTS:
            // * `HttpClient.GetStringAsync` does not accept cancellation token (use `GetAsync` instead)
            // * you may use `EnsureSuccessStatusCode()` method
            if (maxTries < 2)
                throw new ArgumentException();

            return GetStringWithRetriesInternal(client, url, maxTries, token);
        }

        private static async Task<string> GetStringWithRetriesInternal(HttpClient client, string url, int maxTries, CancellationToken token)
        {
            int noOfTries = 0;
            
            while (noOfTries <= maxTries)
            {
                if (token.IsCancellationRequested)
                    throw new TaskCanceledException();
                try
                {
                    HttpResponseMessage responseMessage = await client.GetAsync(url, token);
                    noOfTries++;
                    responseMessage.EnsureSuccessStatusCode();
                    return await responseMessage.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    if (noOfTries == maxTries) throw;
                    await Task.Delay((int)Math.Pow(2, noOfTries - 1) * MS_TO_SECONDS_FACTOR, token);
                }
                
            }
            return string.Empty;
        }
    }
}
