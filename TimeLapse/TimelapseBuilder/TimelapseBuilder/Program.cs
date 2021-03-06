﻿/*
 *  Author: Brian Hungerman
 *  
 *  This application generates a GIF out of any set of images in a defined subfolder to the executable's current directory
 *  
 *  The width (in px) and frame rate (in ms) of the resulting GIF can be controlled
 *  
 *  Runtime note: At 600px Width, 900 images, takes 35~ minutes to process GIF, 35~ seconds to convert to MP4
 * 
 */ 

//Not sure if the work from Anna will be 12~ hours, one of two new projects? 
//  lineslide volume or fallowed land time series... need extents for those...


using ImageMagick;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimelapseBuilder
{
    class Program
    {

        static string filler = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";
        static List<String> filesFound = new List<String>();
        static int water_year = 2008;
        static int time_bounds_low = 6;
        static int time_bounds_high = 18;
        static bool using_time_bounds = false;
        static bool using_one_a_day = false;
        static bool using_three_a_day = false;
        static bool using_range = false;
        static bool using_month = false;
        static int start_month;
        static int start_day;
        static int end_month;
        static int end_day;

        static bool if_stamped = false;

        static void Main(string[] args)
        {
            /* Init Variables */
            string folder = "PlanetData";
            int speed = 0; 
            int width = 600;

            /* Get Introduction */
            Console.WriteLine(filler + "\n~ Welcome to the Timelapse Builder\n" + filler);

            Console.WriteLine("Enter desired speed: \n(\"2\" ~ game camera, \"20\" ~ satellite imagery)");
            speed = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter target folder: \n(\"C:\\Users\\Example\\Pictures\\Avocet\"");
            folder = Console.ReadLine();

            /* Add timestamps to each image?*/
            Console.WriteLine("Do you want timestamps? (y/N):");
            if (Console.ReadLine().ToUpper() == "Y")
            {
                Console.WriteLine("Stamping by file name: ...");
                if_stamped = true;
                timeStamp(folder);
                folder += "\\stamp";
            }
            Console.WriteLine("Do you want to limit the frames per day? (y/N):");
            if (Console.ReadLine().ToUpper() == "Y")
            {
                Console.WriteLine("Options:\n(\"range\",\"month\",\"daytime\", \"one-a-day\", \"three-a-day\"):");
                
                string input = Console.ReadLine();
                switch (input)
                {
                    case "month":
                        Console.WriteLine("Checking for a range of dates...\n Month (1-12):");
                        start_month = int.Parse(Console.ReadLine());
                        using_month = true;
                        break;
                    case "range":
                        Console.WriteLine("Checking for a range of dates...\n Starting Month (1-12):");
                        start_month = int.Parse(Console.ReadLine());
                        Console.WriteLine("Starting Day (1-30):");
                        start_day = int.Parse(Console.ReadLine());
                        Console.WriteLine("Ending Month (1-12):");
                        end_month = int.Parse(Console.ReadLine());
                        Console.WriteLine("Ending Day (1-30):");
                        end_day = int.Parse(Console.ReadLine());
                        using_range = true;
                        
                        break;
                    case "daytime":
                        Console.WriteLine("Checking for daytime images...");
                        using_time_bounds = true;
                        break;
                    case "one-a-day":
                        Console.WriteLine("Checking for daily images...");
                        using_one_a_day = true;
                        break;
                    case "three-a-day":
                        Console.WriteLine("Checking for images three times a day...");
                        using_three_a_day = true;
                        break;
                }
                //    timeStamp(folder);
            }
            Console.WriteLine("~ Press ENTER to begin");
            Console.Read();

            /* Run */
            generateGIF(folder, speed, width);

            
        }

        static void convertTIFtoPNG(string folder)
        {
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + folder);
            foreach (var file in d.GetFiles("*.tif"))
            {
                string newName = file.FullName.Substring(0, file.FullName.Length - 3) + "png";
                Console.WriteLine(newName);
                System.Drawing.Bitmap.
                FromFile(file.FullName).
                Save(newName,
                System.Drawing.Imaging.ImageFormat.Png);
            }            
        }

        /* Give range of values of a given set of data */
        static void generateGIF(string folder, int speed, int width)
        {

            string searchFolder = folder; //Directory.GetCurrentDirectory() + "\\" + folder;
            var filters = new string[] { "jpg", "jpeg", "png", "gif", "tiff", "tif", "bmp" };
            var files = getImages(searchFolder, filters, false);

            using (MagickImageCollection collection = new MagickImageCollection())
            {
                int number_images = files.Length;
                int image_index = 0;
                for (int i = 0; i < number_images; i++)
                {
                    MagickImageInfo info = new MagickImageInfo(files[i]);
                    DateTime written_time = new FileInfo(files[i]).LastWriteTime;
                    if (using_month)
                    {
                        if (written_time.Month != start_month)
                        {
                            continue;
                        }
                        if (written_time.Hour <= time_bounds_low || written_time.Hour >= time_bounds_high) //remove 8pm to 5am
                        {
                            continue;
                        }
                    }
                    if (using_range && written_time.Month >= start_month && written_time.Month <= end_month)
                    {
                        if (written_time.Month == start_month && written_time.Day < start_day)
                        {
                            continue;
                        }
                        if (written_time.Month == end_month && written_time.Day > end_day)
                        {
                            break;
                        }
                        if (written_time.Hour <= time_bounds_low || written_time.Hour >= time_bounds_high) //remove 8pm to 5am
                        {
                            continue;
                        }
                    }
                    else if (using_range) continue;
                    if (using_time_bounds && (written_time.Hour <= time_bounds_low || written_time.Hour >= time_bounds_high)) //remove 8pm to 5am
                    {
                        continue;
                    }
                    if (using_one_a_day && (written_time.Hour != 12 || written_time.Minute > 10))
                    {
                        continue;
                    }
                    if (using_three_a_day)
                    {
                        if (written_time.Minute > 10) continue;
                        if (written_time.Hour < 9) continue;
                        if (written_time.Hour > 15) continue;
                        if (written_time.Hour == 10 || written_time.Hour == 11 || written_time.Hour == 13 || written_time.Hour == 14) continue;
                    }
                    Console.WriteLine(((i * 1.0) / number_images).ToString("p") + ": Add  " + written_time);
                   
                    collection.Add(files[i]);
                    collection[image_index].Resize(width, 0);
                    collection[image_index].AnimationDelay = speed;
                    image_index++;
                }

                Console.Clear();
                Console.WriteLine("Added Images, Optimizing... \n (this make take some time)");
                
                //collection.Optimize();
                
               
                Console.Clear();
                Console.WriteLine("Optimized, Outputting... \n (this may take some time)");
                string outputName = "";

                Random rnd = new Random();
                for (int i = 0; i < 10; i++)
                {
                    outputName += (char)(rnd.Next(97, 122));
                }
                outputName += ".gif";
                collection.Write(outputName);

                Console.WriteLine("Outputted, Closing");
            }
        }

        static string[] getImages(string searchFolder, string[] filters, bool isRecursive)
        {           
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }
        static void timeStamp(string folder)
        {
            string searchFolder = folder;//Directory.GetCurrentDirectory() + "\\" + folder;
            var filters = new string[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
            var files = getImages(searchFolder, filters, false);

            using (MagickImageCollection collection = new MagickImageCollection())
            {
                int number_images = files.Length;
                for (int i = 0; i < number_images; i++)
                {
                    MagickImageInfo info = new MagickImageInfo(files[i]);
                    Console.WriteLine(((i * 1.0) / number_images).ToString("p") + ": Added");

                    using (MagickImageCollection images = new MagickImageCollection())
                    {
                        MagickImage first = new MagickImage(files[i]);
                        first.Resize(new MagickGeometry(0, 0, 600, 600));
                        images.Add(first);

                        using (MagickImage image = new MagickImage(new MagickColor(255, 255, 255, 0), 480, 580))
                        {

                            int index = filesFound[i].Split('\\').Length - 1;

                            Console.WriteLine(filesFound[i].Split('_')[1]);
                            Console.WriteLine(filesFound[i].Split('_')[1].Substring(5, 4));
                            int month_int = int.Parse(filesFound[i].Split('_')[1].Substring(5, 2));//filesFound[i].Split('\\')[index].Substring(4, 2)); //filesFound[i].Split('\\')[index].Split('-')[1]);//
                            string year = filesFound[i].Split('_')[1].Substring(0, 4);//.Split('\\')[index].Substring(0, 4);//.Split('_')[1].Substring(0, 4); filesFound[i].Split('\\')[index].Split('_')[1].Substring(0, 4);//
                                                                                      //filesFound[i].Split('\\')[index].Split('-')[1];
                                                                                      //  Console.WriteLine("y:" + year + ", m:" + month);
                                                                                      //   Console.Read();
                            string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month_int);
                            string text = month + "\n" + year + "\n";
                            if (month_int >= 10)
                            {
                                water_year = int.Parse(year);
                            }
                            MagickColor color = new MagickColor();
                            switch (water_year)
                            {
                                case 2008:
                                    color = MagickColors.Black;
                                    break;
                                case 2009:
                                    color = MagickColors.White;
                                    break;
                                case 2010:
                                    color = MagickColors.Black;
                                    break;
                                case 2011:
                                    color = MagickColors.White;
                                    break;
                                case 2012:
                                    color = MagickColors.Black;
                                    break;
                                case 2013:
                                    color = MagickColors.White;
                                    break;
                                case 2014:
                                    color = MagickColors.Black;
                                    break;
                                case 2015:
                                    color = MagickColors.White;
                                    break;
                                case 2016:
                                    color = MagickColors.Black;
                                    break;
                                case 2017:
                                    color = MagickColors.White;
                                    break;
                                case 2018:
                                    color = MagickColors.Black;
                                    break;
                            }
                            // {-----}
                            new Drawables()
                              // Draw text on the image
                              .FontPointSize(50)
                              .Font("Comic Sans")
                              .StrokeColor(new MagickColor("black"))
                              .FillColor(color)
                              .TextAlignment(TextAlignment.Left)
                              .Text(20, 460, text)
                              .Draw(image);

                            images.Add(image);

                            // Create a mosaic from both images
                            using (IMagickImage result = images.Mosaic())
                            {
                                // Save the result
                                result.Write(folder + "\\stamp\\" + filesFound[i].Split('\\')[index]);
                            }
                        }

                    }
                }

            }

        }

        static void combineImages()
        {
            using (MagickImageCollection images = new MagickImageCollection())
            {
                // Add the first image
                MagickImage first = new MagickImage("test.jpg");

                images.Add(first);
       
                using (MagickImage image = new MagickImage(new MagickColor("#ff00ff"), 512, 128))
                {
                    new Drawables()
                      // Draw text on the image
                      .FontPointSize(72)
                      .Font("Comic Sans")
                      .StrokeColor(new MagickColor("yellow"))
                      .FillColor(MagickColors.Orange)
                      .TextAlignment(TextAlignment.Center)
                      .Text(256, 64, "Magick.NET")
                      // Add an ellipse
                      .StrokeColor(new MagickColor(0, Quantum.Max, 0))
                      .FillColor(MagickColors.SaddleBrown)
                      .Ellipse(256, 96, 192, 8, 0, 360)
                      .Draw(image);

                    images.Add(image);

                    // Create a mosaic from both images
                    using (IMagickImage result = images.Mosaic())
                    {
                        // Save the result
                        result.Write("Mosaic.png");
                    }
                }

               
            }
        }
    }
}
