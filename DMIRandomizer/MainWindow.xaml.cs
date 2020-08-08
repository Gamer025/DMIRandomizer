using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        string exiftool = System.IO.Directory.GetCurrentDirectory()+"\\exiftool.exe";
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
            if (!File.Exists(exiftool))
            {
                MessageBox.Show("Please place exiftool.exe (at least version 11.63) in the same directory as DMIRandomizer.exe");
                Environment.Exit(2);
            }
        }


        private void Randomize_Button_Click(object sender, RoutedEventArgs e)
        {
            if (singleFile.IsChecked == true)
            {
                //Check if path is set
                if (DMIPath_Box.Text == "DMIFile")
                    MessageBox.Show("Please select a DMI file first");
                else
                {
                    if (ParseOptions())
                    {
                        try
                        {
                            RandomizeDMI(DMIPath_Box.Text);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.ToString());
                        }
                    }

                }
            }
            else if (combineDMI.IsChecked == true)
            {
                // Check if both paths are set
                if (sourceDMIPath_Box.Text == "SourceFile" || targetDMIPath_Box.Text == "SourcePath")
                    MessageBox.Show("Please select both DMI files first");
                else
                {
                    if (ParseOptions())
                    {
                        try
                        {
                            RandomizeDMI(sourceDMIPath_Box.Text, targetDMIPath_Box.Text);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.ToString());
                        }
                    }
                }
            }
            else if (multipleFiles.IsChecked == true)
            {
                //Check if path is set
                if (FolderPath_Box.Text == "Folder")
                    MessageBox.Show("Please select a folder first");
                else
                {
                    if (ParseOptions())
                    {
                        try
                        {
                            RandomizeFolder(FolderPath_Box.Text);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.ToString());
                        }
                    }
                }

            }
            else if (multiple_folders.IsChecked == true)
            {
                if (FolderAPath_Box.Text == "Source Folder" || FolderBPath_Box.Text == "Target Folder")
                    MessageBox.Show("Please select both folders first");
                else
                {
                    if (ParseOptions())
                    {
                        try
                        {
                            RandomizeFolder(FolderAPath_Box.Text, FolderBPath_Box.Text);
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show(exc.ToString());
                        }
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

            sourceRect.Width = targetRect.Width = width;
            sourceRect.Height = targetRect.Height =height;


            for (int i = 0; i < fileSize * multiplier; i++)
            {
                yRect = y(yRows, height);
                sourceRect.Y = yRect;
                if (yRect / height == yRows -1) //If x is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                    sourceRect.X = x(xRows, lastIcons, true, width);
                else
                    sourceRect.X = x(xRows, lastIcons, false, width);


                yRect = y(yRows, height);
                targetRect.Y = yRect;
                if (yRect / width == yRows -1) //If x is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                    targetRect.X = x(xRows, lastIcons, true, width);
                else
                    targetRect.X = x(xRows, lastIcons, false, width);

                try
                {
                    using (Graphics grD = Graphics.FromImage(myBitmap))
                    {
                        grD.DrawImage(myBitmap, targetRect, sourceRect, GraphicsUnit.Pixel);
                    }
                }
                catch
                {
                    //Sometimes this just breaks
                }

            }
            myBitmap.Save(DMIPath + ".new");
            myBitmap.Dispose();
            File.Move(DMIPath, DMIPath + ".old");
            process.StartInfo.Arguments = "/S /c \"\""+exiftool + "\" -overwrite_original -z -TagsFromFile \"" + DMIPath + ".old\"" + " -Description \"" + DMIPath + ".new\"\"";
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

            sourceRect.Width = sourceWidth;
            sourceRect.Height = sourceHeight;
            if (stretch_Checkbox.IsChecked == true)
            {
                targetRect.Width = targetWidth;
                targetRect.Height = targetHeight;
            }
            else
            {
                targetRect.Width = sourceWidth;
                targetRect.Height = sourceHeight;
            }

            for (int i = 0; i < counter * multiplier; i++)
            {


                sourceYRect = y(sourceYRows, sourceHeight);
                sourceRect.Y = sourceYRect;
                if (sourceYRect / sourceHeight == sourceYRows - 1) //If y is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                    sourceRect.X = x(sourceXRows, sourceLastIcons, true, sourceWidth);
                else
                    sourceRect.X = x(sourceXRows, sourceLastIcons, false, sourceWidth);


                targetYRect = y(targetYRows, sourceHeight);
                targetRect.Y = targetYRect;
                if (targetYRect / targetHeight == targetYRows - 1) //If y is the amount of rows aka the last row we need to limit y because the last row probably doesn't have the max amount of items
                    targetRect.X = x(targetXRows, targetLastIcons, true, sourceWidth);
                else
                    targetRect.X = x(targetXRows, targetLastIcons, false, sourceWidth);

                try
                {
                    using (Graphics grD = Graphics.FromImage(targetBitmap))
                    {
                        grD.DrawImage(sourceBitmap, targetRect, sourceRect, GraphicsUnit.Pixel);
                    }
                }
                catch
                {
                    //Sometimes this just breaks
                }
            }
            targetBitmap.Save(targetDMI + ".new");
            sourceBitmap.Dispose();
            targetBitmap.Dispose();
            File.Move(targetDMI, targetDMI + ".old");
            process.StartInfo.Arguments = "/S /c \"\""+exiftool +"\" -overwrite_original -z -TagsFromFile \"" + targetDMI+ ".old\"" + " -Description \"" + targetDMI + ".new\"\"";
            process.Start();
            process.WaitForExit();
            File.Delete(targetDMI + ".old");
            File.Move(targetDMI + ".new", targetDMI);
        }

        void RandomizeFolder (string folderPath)
        {
            List<string> allTextures = System.IO.Directory.GetFiles(folderPath, "*.dmi", SearchOption.AllDirectories).ToList<string>();
            if (use_PNGs.IsChecked == true)
            {
                allTextures.AddRange(System.IO.Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories).ToList<string>());
            }


            for (int i = 0; i < allTextures.Count();)
            {
                string FileA = allTextures[rnd.Next(0, allTextures.Count())]; //File to which we want to take the sprites from
                int FileBIndex = rnd.Next(0, allTextures.Count()); //Store the index number of file B because this is the file we are gonna randomize, so we gotta remove it from the array afterwards
                string FileB = allTextures[FileBIndex]; // File in which we want to inject the sprites into

                if(rnd.Next(0, 100) <= ratio_Slider.Value) //Decide if we are either mixing one file into another one or if we a mixing the DMI itself
                    RandomizeDMI(FileB);
                else
                    RandomizeDMI(FileA, FileB);

                allTextures.RemoveAt(FileBIndex);
            }

        }

        void RandomizeFolder(string folderAPath, string folderBPath)
        {
            List<string> allTexturesA = System.IO.Directory.GetFiles(folderAPath, "*.dmi", SearchOption.AllDirectories).ToList<string>();
            if (use_PNGs.IsChecked == true)
            {
                allTexturesA.AddRange(System.IO.Directory.GetFiles(folderAPath, "*.png", SearchOption.AllDirectories).ToList<string>());
            }
            List<string> allTexturesB = System.IO.Directory.GetFiles(folderBPath, "*.dmi", SearchOption.AllDirectories).ToList<string>();
            if (use_PNGs.IsChecked == true)
            {
                allTexturesB.AddRange(System.IO.Directory.GetFiles(folderBPath, "*.png", SearchOption.AllDirectories).ToList<string>());
            }


            for (int i = 0; i < allTexturesA.Count();)
            {
                string FileA = allTexturesA[rnd.Next(0, allTexturesA.Count())]; //File to which we want to take the sprites from
                int FileBIndex = rnd.Next(0, allTexturesB.Count()); //Store the index number of file B because this is the file we are gonna randomize, so we gotta remove it from the array afterwards
                string FileB = allTexturesB[FileBIndex]; // File in which we want to inject the sprites into

                RandomizeDMI(FileA, FileB);

                allTexturesB.RemoveAt(FileBIndex);
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

            if (DMIPath.EndsWith(".png")) //If file is a png (aka has no Byond metadata) calculate the sprite count from the image pixels amount (height * widht) divided by the entered size in the dialog
            {
                var img = Image.FromFile(DMIPath);
                SpriteCount = (img.Width * img.Height) / (Convert.ToInt32(PNG_size.Text)* Convert.ToInt32(PNG_size.Text));
                img.Dispose();
                width = height = Convert.ToInt32(PNG_size.Text);
            }

            return new DMIMetadata(width,height,SpriteCount);
        }

        int x (int rows, int lastIcons, bool lastrow, int width)
        {
            int x = 0;
            if (lastrow)
                x = rnd.Next(0, lastIcons);
            else
                x = rnd.Next(0, rows);

            x = x * width;
            return x;
        }
        int y(int rows, int height)
        {
            
            int x = rnd.Next(0, rows);

            x = x * height;
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

        #region OpenDialogeButtons

        private void SelectFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "What does DMI even stand for|*.dmi|PNG (experimental)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                DMIPath_Box.Text = openFileDialog.FileName;
            }
        }

        private void SelectSourceFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "What does DMI even stand for|*.dmi|PNG (experimental)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                sourceDMIPath_Box.Text = openFileDialog.FileName;
            }
        }

        private void SelectTargetFile_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "What does DMI even stand for|*.dmi|PNG (experimental)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                targetDMIPath_Box.Text = openFileDialog.FileName;
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

        private void SelectFolderA_Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderAPath_Box.Text = dialog.SelectedPath;
                }
            }
        }

        private void SelectFolderB_Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderBPath_Box.Text = dialog.SelectedPath;
                }
            }
        }

        #endregion
    }
}
