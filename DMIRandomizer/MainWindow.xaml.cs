using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using MetadataExtractor;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;


//x = Widht  ---
//y is height |

namespace DMIRandomizer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Random rnd = new Random();
        double multiplier = 2;

        string exiftool = @"D:\Dateien\Downloads\exiftool.exe";
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

        public MainWindow()
        {
            InitializeComponent();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
        }

        private void SelectFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "What does DMI even stand for|*.dmi";
            if (openFileDialog.ShowDialog() == true)
            {
                DMIPath_Box.Text = openFileDialog.FileName;
            }
        }

        private void SelectFolder_Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderPath_Box.Text = dialog.SelectedPath;
                }
            }
        }

        private void Randomize_Button_Click(object sender, RoutedEventArgs e)
        {
            if (singleFile.IsChecked == true)
            {
                //Check if path is set
                if (DMIPath_Box.Text == "")
                    MessageBox.Show("Please select a DMI file first");
                else
                {
                    if (ParseOptions())
                    {
                        RandomizeDMI(DMIPath_Box.Text);
                    }

                }
            }
            else if (combineDMI.IsChecked == true)
            {
                // Check if both paths are set
                if (sourceDMIPath_Box.Text == "" || targetDMIPath_Box.Text == "")
                    MessageBox.Show("Please select both DMI files first");
                else
                {
                    if (ParseOptions())
                    {
                        RandomizeDMI(sourceDMIPath_Box.Text, targetDMIPath_Box.Text);
                    }
                }
            }
            else if (multipleFiles.IsChecked == true)
            {
                //Check if path is set
                if (FolderPath_Box.Text == "")
                    MessageBox.Show("Please select a folder first");
                else
                {
                    if (ParseOptions())
                    {
                        RandomizeFolder(FolderPath_Box.Text);
                    }
                }

            }
            else
                MessageBox.Show("WTF you broke it somehow");
        }


        void RandomizeDMI (string DMIPath)
        {

            int width = 0;
            int height = 0;
            int yRect = 0;
            int fileSize = 0; //Amount of sprites
            int xRows = 0; //Amount of rows on x (width)
            int yRows = 0;//Amount of rows on y (height)
            int lastIcons = 0; //Amount of sprites in the last row
            Bitmap myBitmap = new Bitmap(DMIPath);
            RectangleF sourceRect = new RectangleF(0, 0, 32, 32);
            RectangleF targetRect = new RectangleF(0, 0, 32, 32);
            System.Drawing.Imaging.PixelFormat format = myBitmap.PixelFormat;

            //Set important data from the DMI metadata
            DMIMetadata localDmiData = dmidata(DMIPath);
            width = localDmiData.Width;
            height = localDmiData.Height;
            fileSize = localDmiData.SpriteCount;

            xRows = myBitmap.Width / width; //Hight of the dmi file in pixel divided by the height of a single sprite, results in amount of sprites horizontally
            yRows = myBitmap.Height / height; //Hight of the dmi file in pixel divided by the height of a single sprite, results in amount of sprites vertically
            lastIcons = fileSize - (xRows  * (yRows - 1));

            for (int i = 0; i < fileSize * multiplier; i++)
            {
                yRect = y(yRows);
                sourceRect.Y = yRect;
                if (yRect / 32 == yRows -1) //If x is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                    sourceRect.X = x(xRows, lastIcons, true);
                else
                    sourceRect.X = x(xRows, lastIcons, false);


                yRect = y(yRows);
                targetRect.Y = yRect;
                if (yRect / 32 == yRows -1) //If x is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                    targetRect.X = x(xRows, lastIcons, true);
                else
                    targetRect.X = x(xRows, lastIcons, false);

                using (Graphics grD = Graphics.FromImage(myBitmap))
                {
                    grD.DrawImage(myBitmap, targetRect, sourceRect, GraphicsUnit.Pixel);
                }

            }
            myBitmap.Save(DMIPath + ".new");
            myBitmap.Dispose();
            File.Move(DMIPath, DMIPath + ".old");
            process.StartInfo.Arguments = "/c "+exiftool + " -overwrite_original -z -TagsFromFile " + DMIPath + ".old" + " -Description " + DMIPath + ".new";
            process.Start();
            process.WaitForExit();
            File.Delete(DMIPath + ".old");
            File.Move(DMIPath + ".new", DMIPath);
        }

        void RandomizeDMI (string sourceDMI, string targetDMI)
        {
            int sourceWidth = 0;
            int sourceHeight = 0;
            int sourceYRect = 0;
            int sourceFileSize = 0; //Amount of sprites
            int sourceXRows = 0; //Amount of rows on x (width)
            int sourceYRows = 0;//Amount of rows on y (height)
            int sourceLastIcons = 0; //Amount of sprites in the last row

            int targetWidth = 0;
            int targetHeight = 0;
            int targetYRect = 0;
            int targetFileSize = 0; //Amount of sprites
            int targetXRows = 0; //Amount of rows on x (width)
            int targetYRows = 0;//Amount of rows on y (height)
            int targetLastIcons = 0; //Amount of sprites in the last row

            Bitmap sourceBitmap = new Bitmap(sourceDMI);
            Bitmap targetBitmap = new Bitmap(targetDMI);
            RectangleF sourceRect = new RectangleF(0, 0, 32, 32);
            RectangleF targetRect = new RectangleF(0, 0, 32, 32);

            //Set important data from the DMI metadata
            DMIMetadata sourceLocalDmiData = dmidata(sourceDMI);
            sourceWidth = sourceLocalDmiData.Width;
            sourceHeight = sourceLocalDmiData.Height;
            sourceFileSize = sourceLocalDmiData.SpriteCount;
            DMIMetadata targetLocalDmiData = dmidata(targetDMI);
            targetWidth = targetLocalDmiData.Width;
            targetHeight = targetLocalDmiData.Height;
            targetFileSize = targetLocalDmiData.SpriteCount;

            sourceXRows = sourceBitmap.Width / sourceWidth; //Hight of the dmi file in pixel divided by the height of a single sprite, results in amount of sprites vertically
            sourceYRows = sourceBitmap.Height / sourceHeight; //Hight of the dmi file in pixel divided by the height of a single sprite, results in amount of sprites vertically
            sourceLastIcons = sourceFileSize - (sourceXRows * (sourceYRows - 1));
            targetXRows = targetBitmap.Width / targetWidth; //Hight of the dmi file in pixel divided by the height of a single sprite, results in amount of sprites vertically
            targetYRows = targetBitmap.Height / targetHeight; //Hight of the dmi file in pixel divided by the height of a single sprite, results in amount of sprites vertically
            targetLastIcons = targetFileSize - (targetXRows * (targetYRows - 1));

            int counter = 0;
            if (targetFileSize < sourceFileSize)
                counter = targetFileSize + Convert.ToInt32(0.1 *sourceFileSize); //Smaller file as basis of the amount of copy operations with 10% of the other files size added
            else
                counter = sourceFileSize + Convert.ToInt32(0.1*targetFileSize); //This should result in a good amount of sprites being injected


                for (int i = 0; i < counter * multiplier; i++)
                {


                    sourceYRect = y(sourceYRows);
                    sourceRect.Y = sourceYRect;
                    if (sourceYRect / 32 == sourceYRows - 1) //If y is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                        sourceRect.X = x(sourceXRows, sourceLastIcons, true);
                    else
                        sourceRect.X = x(sourceXRows, sourceLastIcons, false);


                    targetYRect = y(targetYRows);
                    targetRect.Y = targetYRect;
                    if (targetYRect / 32 == targetYRows - 1) //If y is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                        targetRect.X = x(targetXRows, targetLastIcons, true);
                    else
                        targetRect.X = x(targetXRows, targetLastIcons, false);

                    using (Graphics grD = Graphics.FromImage(targetBitmap))
                    {
                        grD.DrawImage(sourceBitmap, targetRect, sourceRect, GraphicsUnit.Pixel);
                    }
                }
            targetBitmap.Save(targetDMI + ".new");
            sourceBitmap.Dispose();
            targetBitmap.Dispose();
            File.Move(targetDMI, targetDMI + ".old");
            process.StartInfo.Arguments = "/c "+exiftool +" -overwrite_original -z -TagsFromFile " + targetDMI+ ".old" + " -Description "+ targetDMI + ".new";
            process.Start();
            process.WaitForExit();
            File.Delete(targetDMI + ".old");
            File.Move(targetDMI + ".new", targetDMI);
        }

        void RandomizeFolder (string folderPath)
        {
            List<string> allDMIs = System.IO.Directory.GetFiles(folderPath, "*.dmi", SearchOption.AllDirectories).ToList<string>();

            //Some blacklisting probably should load this stuff dynamically from a blacklist.txt
            allDMIs.Remove(folderPath+ "\\default_title.dmi");
            allDMIs.Remove(folderPath + "\\minimap.dmi");
            allDMIs.Remove(folderPath + "\\misc\\largeui.dmi");

            for (int i = 0; i < allDMIs.Count();)
            {
                string FileA = allDMIs[rnd.Next(0, allDMIs.Count())]; //File to which we want to take the sprites from
                int FileBIndex = rnd.Next(0, allDMIs.Count()); //Store the index number of file B because this is the file we are gonna randomize, so we gotta remove it from the array afterwards
                string FileB = allDMIs[FileBIndex]; // File in which we want to inject the sprites into

                if(rnd.Next(0, 100) <= ratio_Slider.Value) //Decide if we are either mixing one file into another one or if we a mixing the DMI itself
                    RandomizeDMI(FileB);
                else
                    RandomizeDMI(FileA, FileB);

                allDMIs.RemoveAt(FileBIndex);
            }

        }

        public DMIMetadata dmidata (string DMIPath)
        {
            int width = 32;
            int height = 32;
            int dirs = 0;
            int frames = 0;
            int SpriteCount = 0;

            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(DMIPath);
            foreach (var directory in directories)
                foreach (var tag in directory.Tags)
                {
                    Debug.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                    if ($"{directory.Name}" == "PNG-zTXt")
                    {
                        using (StringReader reader = new StringReader($"{tag.Description}"))
                        {
                            string line = string.Empty;
                            do
                            {
                                line = reader.ReadLine();
                                if (line != null)
                                {
                                    if (line.StartsWith("	width"))
                                    {
                                        width = Convert.ToInt32(line.Replace("	width = ", ""));

                                    }
                                    if (line.StartsWith("	height"))
                                    {
                                        height = Convert.ToInt32(line.Replace("	height = ", ""));

                                    }
                                    if (line.StartsWith("	dirs"))
                                    {
                                        dirs = Convert.ToInt32(line.Replace("	dirs = ", ""));

                                    }
                                    if (line.StartsWith("	frames"))
                                    {
                                        frames = Convert.ToInt32(line.Replace("	frames = ", ""));
                                        SpriteCount = SpriteCount + frames * dirs;
                                    }
                                }

                            } while (line != null);
                        }
                    }

                }
            return new DMIMetadata(width,height,SpriteCount);
        }

        int x (int rows, int lastIcons, bool lastrow)
        {
            int x = 0;
            if (lastrow)
                x = rnd.Next(0, lastIcons);
            else
                x = rnd.Next(0, rows);

            x = x * 32;
            return x;
        }
        int y(int rows)
        {
            
            int x = rnd.Next(0, rows);

            x = x * 32;
            return x;
        }

        private void Multiplier_textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9.-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private bool ParseOptions ()
        {
            bool optionsOK = true;
            if (!double.TryParse(multiplier_textbox.Text, out multiplier))
            {
                //text is not a valid double;
                MessageBox.Show("Shuffle Multiplier is not a valid number!");
                optionsOK = false;
            }
            return optionsOK;
        }

        private void SelectSourceFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "What does DMI even stand for|*.dmi";
            if (openFileDialog.ShowDialog() == true)
            {
                sourceDMIPath_Box.Text = openFileDialog.FileName;
            }
        }

        private void SelectTargetFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "What does DMI even stand for|*.dmi";
            if (openFileDialog.ShowDialog() == true)
            {
                targetDMIPath_Box.Text = openFileDialog.FileName;
            }
        }
    }
}
