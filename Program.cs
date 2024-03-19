using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class LoggingClient
{

    //default port 
    private static int portNumber = 30000;

    // delimiter for parsing the data (log level and the message itself)
    private const string CLIENT_STR_DELIM = ";";

    //set of valid log level flags
    private static readonly string[] VALID_LOG_LEVELS = { "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL" };

    //default IP addess if no ther specifies
    private static string IPaddress = "localhost";

    static void Main(string[] args)
    {
        //check if there are at least 3 arguments: ip address, port number, and the command switch
        if (args.Length > 2)
        {
            IPaddress = args[0];

            // parsing second argument as the port number
            if (!int.TryParse(args[1], out portNumber))
            {
                Console.WriteLine("Invalid port number \nThe correct format: 'IPadress' 'PortNumber' 'Command'");
                return;
            }

            // checking command switch argument
            switch (args[2])
            {
                //manual testing
                //Expecting: --manual switch + log level + message itself
                //For example: --manual INFO "Example message"
                case "--manual":
                    if (args.Length >= 5 && Array.IndexOf(VALID_LOG_LEVELS, args[3]) > -1)
                    {
                        sendLogMessage(args[3].ToUpper(), args[4]);
                    }
                    else
                    {
                        Console.WriteLine("Invalid log level or message.");
                    }
                    break;

                //automated testing
                //Expecting: --automated
                case "--automated":
                    automatedTesting();
                    break;

                //abuse testing
                //Expecting: --abuse + number of message sent (or the default value of 50 will be used)
                //Example: --abuse 1000
                case "--abuse":
                    int count = 50; //default
                    if (args.Length > 3 && int.TryParse(args[3], out int parsedCount))
                    {
                        count = parsedCount;
                    }
                    abuseTesting(count);
                    break;

                //none of the above cases. Invald switch
                default:
                    Console.WriteLine("Invalid command. Use --automated, --abuse, or --manual [LOG_LEVEL] [MESSAGE].");
                    break;
            }
        }
        //invalid number of arguments
        else
        {
            Console.WriteLine("Invalid arguments. \nThe correct format: 'IPadress' 'PortNumber' 'Command'");
        }
    }



    /*
    * Function:     sendLogMessage
    * Purpose:      starting communication between client application and a logging server to senf the logging message containig the
    *               logLevel - severity of the message and logMessage - message itself
    *          
    *
    * Inputs:       string logLevel - severity of the log message
    *               string logMessage - contents of the message itself
    *
    * Outputs:      Tells if the message was successfully sent or of the error occured while transferring
    *
    * Returns:      None
    */
    private static void sendLogMessage(string logLevel, string logMessage)
    {
        try
        {
            using (var client = new TcpClient(IPaddress, portNumber))
            using (var stream = client.GetStream())
            {
                //connecting the log level and log message with delimiter
                string messageToSend = $"{logLevel}{CLIENT_STR_DELIM}{logMessage}";
                byte[] data = Encoding.ASCII.GetBytes(messageToSend);

                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Log message successfully sent: {logLevel}; {logMessage}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occured while sending message to {IPaddress}: {e.Message}");
        }
    }



    /*
    * Function:     automatedTesting() 
    * Purpose:      doing automated testing sending the log message 3 times with each of the log levels
    *
    * Inputs:       None
    *
    * Outputs:      None
    *
    * Returns:      NOne
    */
    private static void automatedTesting()
    {
        foreach (var logLevel in VALID_LOG_LEVELS)
        {
            for (int i = 0; i < 3; i++)
            {
                sendLogMessage(logLevel, $"{logLevel} testing message #{i}");

                //introducing delay between each log message for half second to avoid overwhelming server
                Thread.Sleep(500);
            }
        }
    }


    /*
    * Function:     abuseTesting
    * Purpose:      doing abuse testing by sending the log message as many times as you sepcified
    *
    * Inputs:       None
    *
    * Outputs:      None
    *
    * Returns:      None
    */
    private static void abuseTesting(int count)
    {
        try
        {
            // Create a local endpoint to bind to
            //var localEndPoint = new IPEndPoint(IPAddress.Parse(0.0.0.0), portNumber + 1);


            for (int i = 0; i < count; i++)
            {
                // using (var client = new TcpClient(IPaddress, portNumber))
                using (var client = new TcpClient())
                {   // Connect to the server
                    client.Connect(IPAddress.Parse(IPaddress), portNumber);

                    using (var stream = client.GetStream())
                    {
                        // Send log message from a single client
                        string logLevel = "ERROR";
                        string logMessage = "This is an abuse testing";
                        string messageToSend = $"{logLevel}{CLIENT_STR_DELIM}{logMessage}";
                        byte[] data = Encoding.ASCII.GetBytes(messageToSend);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine($"Log message successfully sent: {logLevel}; {logMessage}");

                        // Introduce delay between each log message for half a second to avoid overwhelming the server
                        Thread.Sleep(5);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error occurred while sending message to {IPaddress}: {e.Message}");
        }
    }
}