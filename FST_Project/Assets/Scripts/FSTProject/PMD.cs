using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class PMD : MonoBehaviour
{


    public string pmdOut;

    public Hashtable priorities = new Hashtable();


    void Start()
    {
        // Reads projectPath and priorities from conf-file
        string[] confLines = System.IO.File.ReadAllLines(@"..\conf.txt");
        string projectPath = confLines[0].Split(' ')[1];
        priorities.Add("UnnecessaryImport", Int32.Parse(confLines[1].Split(' ')[1].Trim()));
        priorities.Add("AddEmptyString", Int32.Parse(confLines[2].Split(' ')[1].Trim()));
        priorities.Add("EmptyCatchBlock", Int32.Parse(confLines[3].Split(' ')[1].Trim()));

        GameObject.Find("MoveableObject/StartCollection/StartFolder").transform.Find("ScriptFunctionality").GetComponent<TextBoxData>().path = projectPath;
    
        // Executes PMD command via CLI
        var process = new System.Diagnostics.Process();
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            WorkingDirectory = @"..\pmd-bin-6.54.0\bin",
            CreateNoWindow = true,
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        }; 

        process.StartInfo = startInfo;
        process.Start();

        // Command for Windows
        process.StandardInput.WriteLine($".\\pmd.bat -d {projectPath} -f text -R category/java/codestyle.xml/UnnecessaryImport -R category/java/performance.xml/AddEmptyString -R category/java/errorprone.xml/EmptyCatchBlock");
        process.StandardInput.Flush();
        process.StandardInput.Close();

        pmdOut = process.StandardOutput.ReadToEnd();
        pmdOut = pmdOut.Split(new string[] {Environment.NewLine + Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)[1];
        
        File.AppendAllText(@"..\pmd.txt", pmdOut);

        process.WaitForExit();

    }

}

