using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChaseAucoin.Aurora
{
    class Program
    {
        static readonly string _auroraAddress = "---.cluster-----.us-east-2.rds.amazonaws.com";
        static readonly string _auroraPassword = "--------------";
        static readonly string _sshTunnelAddress = "--------------.us-east-2.compute.amazonaws.com";        
        static readonly string _sshUserName = "ubuntu";
        static readonly string _keyFileName = "mykey.pem";


        static void Main(string[] args)
        {
            var path = @"E:\wocka.json";
            var jsonJokes = File.ReadAllText(path);
            var jokes = JsonConvert.DeserializeObject<IEnumerable<Joke>>(jsonJokes);

            JokeContext context = GetContext();

            context.Database.EnsureCreated();

            var jokeCount = context.Jokes.Count();
            if (jokeCount == 0)
                foreach(var batch in jokes.Batch(10000))
                {
                    context.Set<Joke>().AddRange(batch);                
                    context.ChangeTracker.DetectChanges();
                    context.SaveChanges();
                    context?.Dispose();
                    context = GetContext();

                    Console.WriteLine("Writing to serverless database");
                }

            var oneLiners = context.Jokes.Where(joke => joke.Category == "One Liners")
                .ToList();
            
            var randomOneLiner = oneLiners.OrderBy(x => Guid.NewGuid()).First();

            Console.WriteLine($"There are {oneLiners.Count} one liners!");

            Console.WriteLine(randomOneLiner.Body);
        }

        public static JokeContext GetContext()
        {
            return new JokeContext(_auroraAddress, _auroraPassword, _sshTunnelAddress, _sshUserName, _keyFileName, true);
        }
    }
}
