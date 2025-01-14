﻿using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace ChocoAutoInstall
{
    internal class Program
    {
        public static string chocoPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "choco_args.txt");
        
        static void Main(string[] args)
        {
            List<string> checkChoco = RunCMD(new List<string> { "/c where choco.exe" });
            if(checkChoco.First().StartsWith("INFO:"))
                Console.WriteLine("Chocolatey not found. Please install Chocolatey and try again.");
            else
                UserInput();
            Console.ReadLine();
        }
        
        public static void UserInput()
        {
            List<object> answers = new List<object>();

            foreach (Question question in questions)
            {
                Console.Write(question.Text);
                string answer = Console.ReadLine();
                
                if (question.Name == "PackageList")
                    PackageList(answer);
                
                if (question.Name == "Upgrade")
                {
                    if (answer == "y")
                        RunCMD(new List<string> { "/c choco upgrade all -y" });
                    else
                        continue;
                }
            }
        }
        
        public static void PackageList(string answer)
        {
            if (answer.Length > 0)
            {
                if (!File.Exists(answer.ToString()))
                    Console.WriteLine("File not found. Using default choco_args.txt");
                else
                    chocoPath = answer.ToString();
            }
            else
                Console.WriteLine("Using default choco_args.txt");

            List<string> chocoLines = new List<string>();
            string[] lines = File.ReadAllLines(chocoPath);
            Console.WriteLine("\n-- List of Packages --\n");
            
            foreach (var line in lines)
            {
                chocoLines.Add($"/c choco upgrade {line}");
                Console.WriteLine(line);
            }
            
            Console.Write($"\nInstall ({lines.Length}) packages? (y/n): ");
            
            if (Console.ReadLine().ToLower() == "y")
                RunCMD(chocoLines);
            else
                Environment.Exit(0);
        }
        
        public static List<string> RunCMD(List<string> argument)
        {
            List<string> stdOuts = new List<string>();
            foreach (string arg in argument)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = arg,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                };
                process.Start();
                stdOuts.Add(process.StandardError.ReadToEnd());
                process.WaitForExit();
            }
            return stdOuts;
        }

        public static Question[] questions = 
        {
            new Question { Text = "Upgrade installed Choco packages not in list? (y/n): ", Name = "Upgrade", ExpectedType = typeof(string) },
            new Question { Text = "Path to package list file (empty for default): ", Name = "PackageList", ExpectedType = typeof(string) },
        };
    }
    
    public class Question
    {
        public string Text { get; set; }
        public string Name { get; set; }
        public Type ExpectedType { get; set; }
    }
}
