using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;

public class Folders : MonoBehaviour
{

    private static GameObject textBoxPrefab;
    private static GameObject gridObjectCollectionPrefab;
    private static GameObject codeShellPrefab;

    private static List<GameObject> gridObjectCollections = new List<GameObject>();


    public void CreateSubdirectories(GameObject button)
    {
        string parentDirectory = button.transform.Find("ScriptFunctionality").GetComponent<TextBoxData>().path;

        // if a folder of a higher hierarchy is selected, everything below it is deleted
        for(int i = gridObjectCollections.Count - 1; i > -1; i--){
            if((gridObjectCollections[i].name.StartsWith(button.transform.parent.transform.name) && 
            !button.transform.parent.transform.name.Equals(gridObjectCollections[i].name)) ||
            button.transform.parent.transform.name.Equals("StartCollection")){
                Destroy(gridObjectCollections[i]);
                gridObjectCollections.RemoveAt(i);
            }
        }

        Hashtable priorities = GameObject.Find("MoveableObject").GetComponent<PMD>().priorities;
        string pmdOut = GameObject.Find("MoveableObject").GetComponent<PMD>().pmdOut;
        string[] pmdLines = pmdOut.Split(new string[] {Environment.NewLine}, StringSplitOptions.None);
        
        // if pressed artifact is java file
        if(parentDirectory.EndsWith(".java")){
            string javaCode = System.IO.File.ReadAllText(parentDirectory);

            // check if pmd finds something for this file
            foreach (string pmdLine in pmdLines){
                string message = string.Empty;
                int lineNumber = 0;
                if(pmdLine.Contains(parentDirectory)){
                    string[] pmdLineTabs = pmdLine.Split('\t');
                    string[] lineArray = pmdLineTabs[0].Split(new string[] {".java"}, StringSplitOptions.None);
                    lineNumber = Int32.Parse(lineArray[lineArray.Length - 1].Replace(":", ""));
                    message = pmdLineTabs[1].Replace(":", "");
                    string color = string.Empty;

                    if((int) priorities[message] == 1){
                        color = "red";
                    } else if((int) priorities[message] == 2){
                        color = "orange";
                    } else if((int) priorities[message] == 3){
                        color = "yellow";
                    }

                    // Highlight PMD finding in code
                    StringBuilder sbText = new StringBuilder();
                    using (StringReader reader = new StringReader(javaCode)){
                        int counter = 1;
                        string line = string.Empty;
                        do {
                            line = reader.ReadLine();
                            if (counter == lineNumber){
                                line = String.Concat(line + " <color=" + color + ">" + message + "</color>");
                            } 
                            sbText.AppendLine(line);
                            counter++;
                        } while (line != null);
                    }
                    javaCode = sbText.ToString();
                }
            }

            // Create Gameobject with code
            codeShellPrefab = Resources.Load("Prefabs/CodeShellPrefab") as GameObject;
            GameObject codeShell = Instantiate(codeShellPrefab);
            gridObjectCollections.Add(codeShell);
            codeShell.name = parentDirectory;
            codeShell.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = javaCode;
            codeShell.transform.position = new Vector3(
                button.transform.parent.transform.position.x + 0.18f, 
                button.transform.parent.transform.position.y, 
                button.transform.parent.transform.position.z);


        } else {
            // Create Gameobject with folder name
            gridObjectCollectionPrefab = Resources.Load("Prefabs/GridObjectCollectionPrefab") as GameObject;
            GameObject gridObjectCollection = Instantiate(gridObjectCollectionPrefab);
            gridObjectCollections.Add(gridObjectCollection);
            gridObjectCollection.name = parentDirectory + "\\";
            gridObjectCollection.transform.position = new Vector3(
                button.transform.parent.transform.position.x + 0.18f, 
                button.transform.parent.transform.position.y, 
                button.transform.parent.transform.position.z);
            gridObjectCollection.transform.parent = GameObject.Find("MoveableObject").transform;

            DirectoryInfo di = new DirectoryInfo(parentDirectory);
            DirectoryInfo[] diArr = di.GetDirectories();
            textBoxPrefab = Resources.Load("Prefabs/TextBoxPrefab") as GameObject;


            // Check for pmd findings in child folders and initiate them
            foreach (DirectoryInfo dri in diArr){
                string color = string.Empty;
                string currentColor = string.Empty;

                foreach (string pmdLine in pmdLines){
                    string message = string.Empty;

                    if(pmdLine.Contains(dri.FullName)){
                        string[] pmdLineTabs = pmdLine.Split('\t');
                        string[] lineArray = pmdLineTabs[0].Split(new string[] {".java"}, StringSplitOptions.None);
                        message = pmdLineTabs[1].Replace(":", "");
        
                        if((int) priorities[message] == 1){
                                color = "red";
                                currentColor = "red";
                            } else if((int) priorities[message] == 2 && !currentColor.Equals("red")){
                                color = "orange";
                                currentColor = "orange";
                            } else if((int) priorities[message] == 3 && !currentColor.Equals("red") && !currentColor.Equals("orange")){
                                color = "yellow";
                                currentColor = "yellow";
                            }
                    
                    
                    }
                }

                GameObject textBox = Instantiate(textBoxPrefab);
                textBox.transform.Find("ScriptFunctionality").GetComponent<TextBoxData>().path = dri.FullName;
                if(!color.Equals(string.Empty)){
                    textBox.transform.Find("IconAndText").GetComponentInChildren<TMPro.TextMeshPro>().text = "<color=" + color + ">" + dri.Name + "</color>";
                } else {
                    textBox.transform.Find("IconAndText").GetComponentInChildren<TMPro.TextMeshPro>().text = dri.Name;
                }

                textBox.name = dri.Name;
                textBox.transform.parent = gridObjectCollection.transform;
            }


            // Check for pmd findings in child files and initiate them
            foreach (var fi in di.GetFiles()){

                if(fi.Name.EndsWith(".java")){

                    string color = string.Empty;
                    string currentColor = string.Empty;

                    foreach (string pmdLine in pmdLines){
                        string message = string.Empty;

                        if(pmdLine.Contains(fi.FullName)){
                            string[] pmdLineTabs = pmdLine.Split('\t');

                            string[] lineArray = pmdLineTabs[0].Split(new string[] {".java"}, StringSplitOptions.None);
                            message = pmdLineTabs[1].Replace(":", "");
        
                            if((int) priorities[message] == 1){
                                color = "red";
                                currentColor = "red";
                            } else if((int) priorities[message] == 2 && !currentColor.Equals("red")){
                                color = "orange";
                                currentColor = "orange";
                            } else if((int) priorities[message] == 3 && !currentColor.Equals("red") && !currentColor.Equals("orange")){
                                color = "yellow";
                                currentColor = "yellow";
                            }
                        }
                    }


                    GameObject textBox = Instantiate(textBoxPrefab);
                    textBox.transform.Find("ScriptFunctionality").GetComponent<TextBoxData>().path = fi.FullName;
                    if(!color.Equals(string.Empty)){
                        textBox.transform.Find("IconAndText").GetComponentInChildren<TMPro.TextMeshPro>().text = "<color=" + color + ">" + fi.Name + "</color>";
                    } else {
                        textBox.transform.Find("IconAndText").GetComponentInChildren<TMPro.TextMeshPro>().text = fi.Name;
                    }
                
                    while(textBox.transform.Find("IconAndText").childCount > 1){
                        DestroyImmediate(textBox.transform.Find("IconAndText").GetChild(1).gameObject);
                    }
                
                    textBox.name = fi.Name;
                    textBox.transform.parent = gridObjectCollection.transform;
                }
            }


            GridObjectCollection goc = gridObjectCollection.GetComponent<GridObjectCollection>();
            goc.UpdateCollection();
        }
    }
}

