﻿using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace SessionDownloader2019
{
    class Program
    {
        private static char[] invalidFilenameChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\n', '.' };

        private static ConsoleColor defaultForegroundConsoleColor = Console.ForegroundColor;
        private static ConsoleColor defaultBackgroundConsoleColor = Console.BackgroundColor;

        enum MessageLevel
        {
            Normal,
            Highlight,
            Warning,
            Error
        }

        public class Arguments
        {
            public string DestinationPath { get; set; }
        }

        static Arguments arguments;
        static void Main(string[] args)
        {

            arguments = ParseArgs(args);

            if (arguments == null)
            {
                return;
            }

            string allThatJson = DownloadSessionMetaData();

            dynamic sessions = JArray.Parse(allThatJson);

            Console.WriteLine($"{sessions.Count} talks found.");

            DownloadSessions(sessions);

            Console.WriteLine($"Finished at {DateTime.Now}");

            Console.ReadLine();

        }

        private static string DownloadSessionMetaData()
        {
            WebClient webClient = new WebClient();
            string sourceData = "https://api.mybuild.techcommunity.microsoft.com/api/session/all";

            WriteHighlight("Starting Metadata Download");
            string allThatJson = webClient.DownloadString(sourceData);
            WriteHighlight("Metadata Download Complete");
            return allThatJson;
        }

        private static Arguments ParseArgs(string[] args)
        {
            if (args.Length < 1)
            {
                WriteError("Please enter a destination path!");
                return null;
            }
            var destinationPath = args[0];

            return new Arguments()
            {
                DestinationPath = destinationPath
            };
        }

        private static void DownloadSessions(dynamic sessions)
        {
            foreach (var session in sessions)
            {

                Console.WriteLine("*****************************");
                Console.WriteLine($"Session: {session.title}");

                if (session.slideDeck != string.Empty)
                {
                    Console.WriteLine("Slide deck available.");

                    string remoteUri = session.slideDeck.ToString();

                    string scrubbedSessionTitle = ScrubSessionTitle(session.title.ToString());
                    string destinationFilename = $"{arguments.DestinationPath}{scrubbedSessionTitle}.pptx";

                    if (File.Exists(destinationFilename) == true)
                    {
                        WriteWarning("File exists. Skipping");
                    }
                    else
                    {
                        try
                        {
                            DownloadFile(remoteUri, destinationFilename);
                        }
                        catch (Exception exception)
                        {
                            WriteError($"Error downloading {remoteUri} to {destinationFilename}");
                        }

                    }

                    Console.WriteLine($"{destinationFilename}");
                }

                if (session.downloadVideoLink != string.Empty)
                {
                    Console.WriteLine("Video available.");

                    string remoteUri = session.downloadVideoLink.ToString();

                    string scrubbedSessionTitle = ScrubSessionTitle(session.title.ToString());
                    string destinationFilename = $"{arguments.DestinationPath}{scrubbedSessionTitle}.mp4";

                    if (File.Exists(destinationFilename) == true)
                    {
                        Console.WriteLine("File exists. Skipping");
                    }
                    else
                    {
                        DownloadFile(remoteUri, destinationFilename);
                    }
                }
            }
        }

        private static void DownloadFile(string remoteUri, string destinationFilename)
        {
            Console.WriteLine($"\t Download started at {DateTime.Now}");

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(remoteUri, destinationFilename);

                Console.WriteLine($"\t Download completed at {DateTime.Now}");
            }
        }

        private static string ScrubSessionTitle(string sessionTitle)
        {
            var scrubbedString = sessionTitle;

            invalidFilenameChars.ToList().ForEach(x =>
            {
                scrubbedString = scrubbedString.Replace(x, ' ');
            });

            return scrubbedString;
        }

        private static void ResetConsoleColors()
        {
            Console.ForegroundColor = defaultForegroundConsoleColor;
            Console.BackgroundColor = defaultBackgroundConsoleColor;
        }

        private static void WriteError(string message)
        {
            WriteMessage(message, MessageLevel.Error);
        }
        private static void WriteWarning(string message)
        {
            WriteMessage(message, MessageLevel.Warning);
        }
        private static void WriteHighlight(string message)
        {
            WriteMessage(message, MessageLevel.Highlight);
        }


        private static void WriteMessage(string message, MessageLevel messageLevel)
        {
            switch (messageLevel)
            {
                case MessageLevel.Normal:
                    break;
                case MessageLevel.Highlight:
                    InvertConsoleColors();
                    break;
                case MessageLevel.Warning:
                    WarningConsoleColors();
                    break;
                case MessageLevel.Error:
                    ErrorConsoleColors();
                    break;
                default:
                    break;
            }

            Console.WriteLine(message);

            ResetConsoleColors();


        }

        private static void InvertConsoleColors()
        {
            Console.ForegroundColor = defaultBackgroundConsoleColor;
            Console.BackgroundColor = defaultForegroundConsoleColor;
        }

        private static void ErrorConsoleColors()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = defaultBackgroundConsoleColor;
        }

        private static void WarningConsoleColors()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = defaultBackgroundConsoleColor;
        }

    }
}
