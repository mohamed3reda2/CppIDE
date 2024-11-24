using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace CppIDE
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            
            MessageTextBlock.Text = "Please enter your name:";
            UserNameTextBox.Visibility = Visibility.Visible;
            SubmitNameButton.Visibility = Visibility.Visible;
            MessageTextBlock.Visibility = Visibility.Visible;

            
            RunButton.IsEnabled = false;
        }

        private void SubmitNameButton_Click(object sender, RoutedEventArgs e)
        {
            string name = UserNameTextBox.Text;
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a valid name.");
                return;
            }

     
            UserNameTextBox.Visibility = Visibility.Collapsed;
            SubmitNameButton.Visibility = Visibility.Collapsed;
            MessageTextBlock.Visibility = Visibility.Collapsed;

        
            RunButton.IsEnabled = true;

          
            OutputBox.Text = $"You entered: {name}\nNow running C++ code...\n";

            string code = CodeEditor.Text;
            string output = CompileAndRunCppCode(code, name);
            OutputBox.Text += output;
        }

        private string CompileAndRunCppCode(string code, string name)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), "temp.cpp");
            string exeFilePath = Path.Combine(Path.GetTempPath(), "temp.exe");
            File.WriteAllText(tempFilePath, code);

            try
            {
                
                Process compileProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "g++", 
                        Arguments = $"\"{tempFilePath}\" -o \"{exeFilePath}\"",
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                compileProcess.Start();
                string compileErrors = compileProcess.StandardError.ReadToEnd();
                compileProcess.WaitForExit();

                if (compileProcess.ExitCode != 0)
                {
                    return $"Compilation Error:\n{compileErrors}";
                }


                Process runProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exeFilePath,
                        RedirectStandardInput = true,  
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                runProcess.Start();

 
                StreamWriter writer = runProcess.StandardInput;
                writer.WriteLine(name); 

                string output = runProcess.StandardOutput.ReadToEnd();
                string runtimeErrors = runProcess.StandardError.ReadToEnd();
                runProcess.WaitForExit();

                if (!string.IsNullOrEmpty(runtimeErrors))
                {
                    return $"Runtime Error:\n{runtimeErrors}";
                }

                return $"Output:\n{output}";
            }
            catch (Exception ex)
            {
                return $"Error:\n{ex.Message}";
            }
            finally
            {
               
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                if (File.Exists(exeFilePath)) File.Delete(exeFilePath);
            }
        }
    }
}
