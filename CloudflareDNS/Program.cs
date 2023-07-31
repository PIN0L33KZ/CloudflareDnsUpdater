using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace CloudflareDNS
{
    internal class Program
    {
        private static bool _autoUpdate = false;

        [STAThread]
        private static async Task Main(string[] args)
        {
            PrintWelcomeScreen();

            await ArgumentHandler(args);

            while(true)
            {
                PrintUserInput();

                string userInput = Console.ReadLine().ToLower().Trim();

                //If user input is empty, skip loop iteration
                if(userInput == "")
                {
                    continue;
                }

                switch(userInput)
                {
                    case "setup":
                        await Setup();
                        break;

                    case "update":
                        if(Properties.Settings.Default.APIToken == "" || Properties.Settings.Default.ZoneID == "" || Properties.Settings.Default.DefaultRecordID == "")
                        {
                            PrintError("You need to run setup before.");
                            continue;
                        }

                        await UpdateAsync();
                        break;

                    case "reset":
                        Reset();
                        break;

                    case "get records":
                        if(Properties.Settings.Default.APIToken == "" || Properties.Settings.Default.ZoneID == "")
                        {
                            PrintError("You need to run setup before.");
                            continue;
                        }

                        PrintRecords();
                        break;

                    case "get ip":
                        IPAddress pubIP = await GetPublicIPAsync();

                        if(pubIP == null)
                        {
                            PrintError("Unable to get your public ip address.\n");
                            continue;
                        }

                        switch(pubIP.AddressFamily)
                        {
                            case System.Net.Sockets.AddressFamily.InterNetwork:
                                PrintSuccess("Your public ipv4 address is: " + pubIP);
                                break;

                            case System.Net.Sockets.AddressFamily.InterNetworkV6:
                                PrintSuccess("Your public ipv6 address is: " + pubIP);
                                break;

                            default:
                                PrintError("Unknown address format.");
                                break;
                        }
                        break;

                    case "help":
                        PrintInformation("Help menu:\n" + Properties.Resources.HelpMenu);
                        break;

                    case "clear":
                        Console.Clear();
                        PrintWelcomeScreen();
                        break;

                    case "exit":
                        Environment.Exit(0);
                        break;

                    default:
                        PrintError("Command unknown or invalid, use help to get more information.");
                        break;
                }
            }
        }

        private static async Task ArgumentHandler(string[] args)
        {
            //Return if no arguments given
            if(args.Length == 0)
            {
                return;
            }

            if(args[0] == "autoupdate")
            {
                PrintSuccess("Argument autoupdate processed successfully.");
                _autoUpdate = true;

                if(Properties.Settings.Default.APIToken == "" || Properties.Settings.Default.ZoneID == "" || Properties.Settings.Default.DefaultRecordID == "")
                {
                    PrintError("You need to run setup before.");
                    return;
                }

                await UpdateAsync();
            }
        }

        private static async Task Setup()
        {
            PrintInformation("Setup will start now.");

            PrintQuestion("What's your Cloduflare API token? See: https://developers.cloudflare.com/fundamentals/api/get-started/create-token");

            PrintUserInput();
            string userInput = Console.ReadLine().Trim();

            //Validate api token
            PrintInformation("Validating API token...");
            CloudflareAPI cfApi = new CloudflareAPI(userInput);
            if(await cfApi.ValidateTokenAsync())
            {
                PrintSuccess("Token is valid.");
                Properties.Settings.Default.APIToken = userInput;
            }
            else
            {
                PrintError("Token is invalid.");
                PrintInformation("Setup cancelled.");
                return;
            }

            //This code will only be exectued if api token is valid.
            PrintQuestion("What's your Cloudflare Zone ID?");

            PrintUserInput();
            userInput = Console.ReadLine().Trim();

            //Validate zone id
            PrintInformation("Validating zone ID...");
            if(await cfApi.ValidateZoneIDAsync(userInput))
            {
                PrintSuccess("Zone ID is valid.");
                Properties.Settings.Default.ZoneID = userInput;

            }
            else
            {
                PrintError("Zone ID is invalid.");
                PrintInformation("Setup cancelled.");
                return;
            }

            //This code will only be exectued if api token and zone id are valid.
            Properties.Settings.Default.Save();

            PrintRecords();

            //Get default dns record to update automatically.
            PrintQuestion("What's the record ID that you wish to update automatically?");
            PrintUserInput();
            userInput = Console.ReadLine().Trim();

            //Validate record id
            PrintInformation("Validating record ID...");
            if(await cfApi.ValidateRecordIDAsync(Properties.Settings.Default.ZoneID, userInput))
            {
                PrintSuccess("Record ID is valid.");
                Properties.Settings.Default.DefaultRecordID = userInput;
            }
            else
            {
                PrintError("Record ID is invalid.");
                PrintInformation("Setup cancelled.");
                return;
            }

            PrintSuccess("Setup completed.");
            Properties.Settings.Default.Save();
        }

        private static async Task UpdateAsync()
        {
            PrintInformation("Attempting to update default record.");

            CloudflareAPI cfApi = new CloudflareAPI(Properties.Settings.Default.APIToken);

            HttpResponseMessage response = await cfApi.UpdateRecordAsync(Properties.Settings.Default.ZoneID, Properties.Settings.Default.DefaultRecordID, GetPublicIPAsync().Result);

            if(response == null)
            {
                PrintError("Unable to send API request.");
                return;
            }

            if(response.IsSuccessStatusCode == true)
            {
                PrintSuccess("Default record updated successfully.");

                if(_autoUpdate)
                {
                    Environment.Exit(0);
                }
            }
            else if(response.IsSuccessStatusCode == false)
            {
                PrintError("Unable to update default record. Reason: " + response.StatusCode + ".");
            }
        }

        private static void Reset()
        {
            PrintInformation("Attempting reset.");
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            PrintSuccess("Reset successfully.");
        }

        private static async Task<List<CloudflareRecord>> GetRecordsAsync()
        {
            CloudflareAPI cfApi = new CloudflareAPI(Properties.Settings.Default.APIToken);
            List<CloudflareRecord> records = await cfApi.GetDnsRecordsAsync(Properties.Settings.Default.ZoneID);

            return records;
        }

        private static void PrintRecords()
        {
            ConsoleTables.ConsoleTableOptions tableOptions = new ConsoleTables.ConsoleTableOptions() { EnableCount = false, NumberAlignment = ConsoleTables.Alignment.Right, Columns = new string[] { "Name", "Type", "ID" } };
            ConsoleTables.ConsoleTable table = new ConsoleTables.ConsoleTable(tableOptions);

            foreach(CloudflareRecord record in GetRecordsAsync().Result)
            {
                _ = table.AddRow(record.Name, record.Type, record.Id);
            }

            PrintSuccess("Zone records:\n");
            table.Write(ConsoleTables.Format.Minimal);
        }

        private static async Task<IPAddress> GetPublicIPAsync()
        {
            using(HttpClient client = new HttpClient())
            {
                try
                {
                    string apiResponse = await client.GetStringAsync("https://api64.ipify.org");

                    _ = IPAddress.TryParse(apiResponse, out IPAddress pubIP);

                    return pubIP;
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }

        private static void PrintWelcomeScreen()
        {
            Console.Title = "Cloudflare DNS updater";
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Cloudflare DNS updater v." + Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("By ©PIN0L33KZ visit https://pinoleekz.de for more information.");
            Console.WriteLine("--------------------------------------------------------------");
            Console.ResetColor();
        }

        private static void PrintUserInput()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(System.Security.Principal.WindowsIdentity.GetCurrent().Name + "> ");
            Console.ResetColor();
        }

        private static void PrintInformation(string message)
        {
            Console.Write('[');
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write('I');
            Console.ResetColor();
            Console.Write("] ");
            Console.WriteLine(message);
        }

        private static void PrintSuccess(string message)
        {
            Console.Write('[');
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write('S');
            Console.ResetColor();
            Console.Write("] ");
            Console.WriteLine(message);
        }

        private static void PrintError(string message)
        {
            Console.Write('[');
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('E');
            Console.ResetColor();
            Console.Write("] ");
            Console.WriteLine(message);
        }

        private static void PrintQuestion(string message)
        {
            Console.Write('[');
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write('Q');
            Console.ResetColor();
            Console.Write("] ");
            Console.WriteLine(message);
        }
    }
}