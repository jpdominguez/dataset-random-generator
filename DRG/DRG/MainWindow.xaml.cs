﻿using DRG.src;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DRG
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        OpenFileDialog file_dialog_labelsPath;
        CommonOpenFileDialog folder_dialog_imagesPath;

        public MainWindow()
        {
            InitializeComponent();

            file_dialog_labelsPath = new OpenFileDialog();
            folder_dialog_imagesPath = new CommonOpenFileDialog();
            file_dialog_labelsPath.Title = "Select labels file";
            folder_dialog_imagesPath.Title = "Select the folder that contains the images";
            file_dialog_labelsPath.Filter = "Text files (*.csv, *.txt) | *.csv; *.txt";

            textblock_imageFolder.Text = "No file selected";
            textblock_labelsFile.Text = "No file selected";
        }

        #region EventHandlers
        private void slider_Train_MouseMove(object sender, MouseEventArgs e)
        {
            if (checkbox_Validation.IsChecked == true)
            {
                slider_Validation.Value = 100 - slider_Train.Value - slider_Test.Value;
            }
            slider_Test.Value = 100 - slider_Train.Value - slider_Validation.Value;
        }

        private void slider_Test_MouseMove(object sender, MouseEventArgs e)
        {
            if (checkbox_Validation.IsChecked == true)
            {
                slider_Validation.Value = 100 - slider_Train.Value - slider_Test.Value;
            }
            slider_Train.Value = 100 - slider_Test.Value - slider_Validation.Value;
        }

        private void slider_Validation_MouseMove(object sender, MouseEventArgs e)
        {
            slider_Test.Value = 100 - slider_Train.Value - slider_Validation.Value;
            slider_Train.Value = 100 - slider_Test.Value - slider_Validation.Value;
        }

        private void checkbox_Validation_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                slider_Validation.IsEnabled = true;
            }
        }

        private void checkbox_Validation_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
            {
                slider_Validation.Value = 0;
                slider_Test.Value = 100 - slider_Train.Value - slider_Validation.Value;
                slider_Train.Value = 100 - slider_Test.Value - slider_Validation.Value;
                slider_Validation.IsEnabled = false;
            }
        }

        private void btn_imageFolder_Click(object sender, RoutedEventArgs e)
        {
            folder_dialog_imagesPath.IsFolderPicker = true;
            if (folder_dialog_imagesPath.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textblock_imageFolder.Text = folder_dialog_imagesPath.FileName;
            }
        }

        private void btn_labels_Click(object sender, RoutedEventArgs e)
        {
            if (file_dialog_labelsPath.ShowDialog() == true)
            {
                textblock_labelsFile.Text = file_dialog_labelsPath.FileName;
            }
        }
        #endregion

        private void button_Launch_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog folder_dialog_savePath = new CommonOpenFileDialog();
            folder_dialog_savePath.IsFolderPicker = true;
            folder_dialog_savePath.Title = "Select a folder to save the output dataset";

            StringBuilder stringbuilder_saveLabels = new StringBuilder();

            if (folder_dialog_savePath.ShowDialog() == CommonFileDialogResult.Ok)
            {
                System.IO.Directory.CreateDirectory(folder_dialog_savePath.FileName + "\\generated_DB");
                Utilities.grant_access(folder_dialog_savePath.FileName + "\\generated_DB");
                System.IO.Directory.CreateDirectory(folder_dialog_savePath.FileName + "\\generated_DB\\train");
                Utilities.grant_access(folder_dialog_savePath.FileName + "\\generated_DB\\train");
                System.IO.Directory.CreateDirectory(folder_dialog_savePath.FileName + "\\generated_DB\\test");
                Utilities.grant_access(folder_dialog_savePath.FileName + "\\generated_DB\\test");
                if (checkbox_Validation.IsChecked == true)
                {
                    System.IO.Directory.CreateDirectory(folder_dialog_savePath.FileName + "\\generated_DB\\validation");
                    Utilities.grant_access(folder_dialog_savePath.FileName + "\\generated_DB\\validation");
                }

                File.Copy(file_dialog_labelsPath.FileName, file_dialog_labelsPath.FileName + "_bkp");


                var files_in_path = Directory.GetFiles(folder_dialog_imagesPath.FileName, "*.*", SearchOption.AllDirectories).Where(s => s.Contains(System.IO.Path.GetExtension(".jpg")) || s.Contains(System.IO.Path.GetExtension(".png")));

                List<string> all_files = new List<string>(files_in_path);

                int number_trainImages = files_in_path.Count() * (int)slider_Train.Value / 100;
                int number_validationImages = files_in_path.Count() * (int)slider_Validation.Value / 100;
                int number_testImages = files_in_path.Count() * (int)slider_Test.Value / 100;

                List<string> list_train_images = new List<string>();
                List<string> list_validation_images = new List<string>();
                List<string> list_test_images = new List<string>();

                Random rnd = new Random();

                int random_number;

                for (int i = 0; i < number_trainImages; i++)
                {
                    random_number = rnd.Next(all_files.Count());

                    list_train_images.Add(all_files[random_number]);
                    System.IO.File.Copy(all_files[random_number], folder_dialog_savePath.FileName + "\\generated_DB\\train\\" + all_files[random_number].Split('\\')[all_files[random_number].Split('\\').Count()-1]);


                    stringbuilder_saveLabels.Append(i).Append(Utilities.getLabel(file_dialog_labelsPath.FileName, all_files[random_number]));
                    

                    all_files.RemoveAt(random_number);
                }

                File.AppendAllText(folder_dialog_savePath.FileName + "\\generated_DB\\train.txt", stringbuilder_saveLabels.ToString());
                stringbuilder_saveLabels.Clear();

                for (int i = 0; i < number_testImages; i++)
                {
                    random_number = rnd.Next(all_files.Count());

                    list_test_images.Add(all_files[random_number]);
                    System.IO.File.Copy(all_files[random_number], folder_dialog_savePath.FileName + "\\generated_DB\\test\\" + all_files[random_number].Split('\\')[all_files[random_number].Split('\\').Count() - 1]);

                    stringbuilder_saveLabels.Append(i).Append(Utilities.getLabel(file_dialog_labelsPath.FileName, all_files[random_number]));

                    all_files.RemoveAt(random_number);
                }

                File.AppendAllText(folder_dialog_savePath.FileName + "\\generated_DB\\test.txt", stringbuilder_saveLabels.ToString());
                stringbuilder_saveLabels.Clear();

                if (checkbox_Validation.IsChecked == true)
                {
                    for (int i = 0; i < number_validationImages; i++)
                    {
                        random_number = rnd.Next(all_files.Count());

                        list_validation_images.Add(all_files[random_number]);
                        System.IO.File.Copy(all_files[random_number], folder_dialog_savePath.FileName + "\\generated_DB\\validation\\" + all_files[random_number].Split('\\')[all_files[random_number].Split('\\').Count() - 1]);
                        all_files.RemoveAt(random_number);
                    }
                }
            }
        }
    }
}