using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;

//Statistics for counting lines in all of the scripts folder.
public class LineCounter : EditorWindow {
    struct File {
        public string name;
        public string displayName;
        public int numLines;

        public File(string name, string displayName, int numLines) {
            this.name = name;
            this.displayName = displayName;
            this.numLines = numLines;
        }
    }

    private StringBuilder stringStats;
    private Vector2 scrollPos = Vector2.zero;
    private string directoryToCheck = "/Assets/MAIN - Blackraze";

    private enum SortMethod {LineCount, Alphabetical};
    private static SortMethod sortMethod = SortMethod.LineCount;

    private enum DisplayMethod {Default, Graph, TextOnly};
    private static DisplayMethod displayMethod = DisplayMethod.Default;

    private static List<string> blacklist;
    private static bool ignoreEmptyLines = false;
    private static bool displayMatchesOnly = true;
    private static bool comparisonMode = false;
    
    private static string search = "";

    private List<File> fileStats = new List<File>();
    private File largestLineCountRef;
    private int totalLines;
    private float averageLines;
    private string directory;

    [MenuItem("Tools/Statistics/Line Counter %&C", false, 1)]
    public static void Init() {
        blacklist = new List<string>();
        blacklist.Add("Editor");

        sortMethod = (SortMethod)EditorPrefs.GetInt("LC_SortMethod", 0);
        displayMethod = (DisplayMethod)EditorPrefs.GetInt("LC_DisplayMode", 0);
        ignoreEmptyLines = EditorPrefs.GetBool("LC_IgnoreEmptyLines", false);
        displayMatchesOnly = EditorPrefs.GetBool("LC_DisplayMatchesOnly", true);
        comparisonMode = EditorPrefs.GetBool("LC_ComparisonMode", false);

        if(displayMethod != DisplayMethod.Graph) {
            search = EditorPrefs.GetString("LC_SavedSearch", "");
        }

        LineCounter window = GetWindow<LineCounter>("Line Counter");
        window.Show();
        window.Focus();
        window.CountLines();
    }
    
    void OnGUI() {
        GUILayout.Space(7f);

        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        if(stringStats != null) {
            EditorGUILayout.HelpBox(stringStats.ToString(), MessageType.None);

            GUILayout.Space(5f);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            sortMethod = (SortMethod)EditorGUILayout.EnumPopup("Sort Method:", sortMethod);
            displayMethod = (DisplayMethod)EditorGUILayout.EnumPopup("Display Mode:", displayMethod);
            ignoreEmptyLines = EditorGUILayout.Toggle("Ignore Empty Lines:", ignoreEmptyLines);

            GUILayout.Space(4f);

            if(displayMethod != DisplayMethod.Graph) {
                search = EditorGUILayout.TextField("Search Script Name:", search);

                if(GUI.changed) {
                    EditorPrefs.SetString("LC_SavedSearch", search);
                }

                if(displayMethod == DisplayMethod.Default) {
                    EditorGUI.indentLevel += 1;
                    displayMatchesOnly = EditorGUILayout.Toggle("Display Matches Only:", displayMatchesOnly);

                    if(displayMatchesOnly) {
                        comparisonMode = EditorGUILayout.Toggle("Comparison Mode:", comparisonMode);
                    }
                    else {
                        comparisonMode = false;
                    }
                    EditorGUI.indentLevel -= 1;
                }
            }
            else {
                search = "";
            }

            if(GUI.changed) {
                EditorPrefs.SetInt("LC_SortMethod", (int)sortMethod);
                EditorPrefs.SetInt("LC_DisplayMode", (int)displayMethod);
                EditorPrefs.SetBool("LC_IgnoreEmptyLines", ignoreEmptyLines);
                EditorPrefs.SetBool("LC_DisplayMatchesOnly", displayMatchesOnly);
                EditorPrefs.SetBool("LC_ComparisonMode", comparisonMode);

                CountLines();
            }

            GUILayout.Space(5f);

            if(fileStats != null) {
                string builtString = "";
                int displayCount = 0;
                for(int i = 0; i < fileStats.Count; i++) {
                    float alphaMod = 1f;
                    float finalHeight = 15f;
                    if(!string.IsNullOrEmpty(search) && !fileStats[i].displayName.ToLower().Contains(search.ToLower())) {
                        if(displayMethod == DisplayMethod.TextOnly) {
                            continue;
                        }

                        if(comparisonMode) {
                            finalHeight = 1f;
                            alphaMod = 0.5f;
                        }
                        else if(displayMatchesOnly) {
                            continue;
                        }
                        else {
                            alphaMod = 0.3f;
                        }
                    }

                    displayCount++;

                    if(displayMethod == DisplayMethod.TextOnly) {
                        builtString += (i + 1).ToString() + " - " + fileStats[i].displayName + " -> [" + fileStats[i].numLines.ToString() + "]";

                        if(i < fileStats.Count - 1) {
                            builtString += "\n";
                        }
                    }
                    else {
                        if(displayMethod == DisplayMethod.Graph) {
                            finalHeight = 1f;
                        }

                        Rect displayRect = EditorGUILayout.GetControlRect(true, GUILayout.MaxHeight(finalHeight));

                        GUI.color = new Color(0f, 0f, 0f, ((displayMethod == DisplayMethod.Default) ? 0.3f : 0.05f) * alphaMod);
                        GUI.Box(displayRect, "");
                        GUI.color = new Color(1f, 1f, 1f, alphaMod);

                        GUI.depth += 1;

                        GUI.color = new Color(0.5f, 0.8f, 0.4f, GUI.color.a);
                        float oldWidth = displayRect.width;
                        displayRect.width *= (fileStats[i].numLines / (float)largestLineCountRef.numLines);
                        displayRect.width = Mathf.Round(displayRect.width);

                        if(Event.current.type == EventType.Repaint && displayRect.width > 1f) {
                            GUI.Box(displayRect, "");
                        }

                        displayRect.width = oldWidth;

                        GUI.depth += 1;

                        GUI.color = new Color(0.8f, 0.8f, 0.4f, GUI.color.a);
                        float oldWidth2 = displayRect.width;
                        displayRect.width *= (fileStats[i].numLines / (float)totalLines);
                        displayRect.width = Mathf.Round(displayRect.width);

                        if(Event.current.type == EventType.Repaint && displayRect.width > 1f) {
                            GUI.Box(displayRect, "");
                        }

                        displayRect.width = oldWidth2;
                        GUI.color = new Color(1f, 1f, 1f, GUI.color.a);

                        GUI.depth += 1;

                        displayRect.y += 1;

                        if(displayMethod != DisplayMethod.Graph) {
                            int oldFontSize = GUI.skin.label.fontSize;
                            GUI.skin.label.fontSize = 9;
                            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                            GUI.Label(displayRect, (i + 1).ToString() + ": " + fileStats[i].displayName);
                            GUI.skin.label.alignment = TextAnchor.MiddleRight;
                            GUI.Label(displayRect, "[" + fileStats[i].numLines.ToString() + " lines]  (" + (fileStats[i].numLines * 100 / (float)totalLines).ToString("F3") + "%)");
                            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                            GUI.skin.label.fontSize = oldFontSize;
                        }

                        GUI.depth -= 3;
                    }
                }

                if(displayCount <= 0) {
                    EditorGUILayout.HelpBox("No results...", MessageType.None);
                }
                else if(!string.IsNullOrEmpty(builtString) && displayMethod == DisplayMethod.TextOnly) {
                    EditorGUILayout.HelpBox(builtString, MessageType.None);
                }
            }

            EditorGUILayout.EndScrollView();
        }
        else {
            EditorGUILayout.HelpBox("REFRESH THE WINDOW BY PRESSING CTRL+ALT+C", MessageType.None);
        }
    }

    private void CountLines() {
        directory = Directory.GetCurrentDirectory();
        directory += directoryToCheck;
        fileStats = new List<File>();
        ProcessDirectory(fileStats, directory);

        totalLines = 0;
        foreach(File f in fileStats) {
            totalLines += f.numLines;
        }

        int scriptCountDelta = (fileStats.Count - EditorPrefs.GetInt("LC_OldScriptCount_" + PlayerSettings.productName, 0));
        int totalLineDelta = (totalLines - EditorPrefs.GetInt("LC_OldTotalLines_" + PlayerSettings.productName, 0));

        stringStats = new StringBuilder();
        stringStats.Append("Total Number of Scripts: " + fileStats.Count + ((scriptCountDelta != 0) ? "   [" + ((scriptCountDelta > 0) ? "+" : "") + scriptCountDelta.ToString() + " script(s)]" : "") + "\n");
        stringStats.Append("Total Number of Lines: " + totalLines + ((totalLineDelta != 0) ? "   [" + ((totalLineDelta > 0) ? "+" : "") + totalLineDelta.ToString() + " line(s)]" : "") + "\n");

        averageLines = Mathf.Round(totalLines * 10f / fileStats.Count) / 10f;
        float avgLineDelta = (averageLines - EditorPrefs.GetFloat("LC_OldAvgLines_" + PlayerSettings.productName, 0f));
        stringStats.Append("Averages Lines per Script: " + averageLines.ToString("F1") + " lines " + ((avgLineDelta != 0f) ? "   [" + ((avgLineDelta > 0f) ? "+" : "") + avgLineDelta.ToString("F1") + " avg line(s)]" : ""));
               
        if(sortMethod == SortMethod.LineCount) {
            fileStats.Sort((f1, f2) => f2.numLines.CompareTo(f1.numLines));
        }
        else if(sortMethod == SortMethod.Alphabetical) {
            fileStats.Sort((f1, f2) => f1.displayName.CompareTo(f2.displayName));
        }

        largestLineCountRef = new File();
        if(fileStats.Count > 0) {
            if(sortMethod == SortMethod.LineCount) {
                largestLineCountRef = fileStats[0];
            }
            else if(sortMethod == SortMethod.Alphabetical) {
                int largestLine = 0;
                for(int i = 0; i < fileStats.Count; i++) {
                    if(fileStats[i].numLines > largestLine) {
                        largestLineCountRef = fileStats[i];
                        largestLine = fileStats[i].numLines;
                    }
                }
            }
        }

        Repaint();

        EditorPrefs.SetInt("LC_OldScriptCount_" + PlayerSettings.productName, fileStats.Count);
        EditorPrefs.SetInt("LC_OldTotalLines_" + PlayerSettings.productName, totalLines);
        EditorPrefs.SetFloat("LC_OldAvgLines_" + PlayerSettings.productName, averageLines);
    }

    private static void ProcessDirectory(List<File> stats, string dir) {
        string[] strArrFiles = Directory.GetFiles(dir, "*.cs");
        foreach(string strFileName in strArrFiles) {
            bool skip = false;
            foreach(string bl in blacklist) {
                if(strFileName.Contains(bl)) {
                    skip = true;
                    break;
                }
            }

            if(skip) {
                continue;
            }

            ProcessFile(stats, strFileName);
        }

        string[] strArrSubDir = Directory.GetDirectories(dir);
        foreach(string strSubDir in strArrSubDir) {
            ProcessDirectory(stats, strSubDir);
        }
    }

    private static void ProcessFile(List<File> stats, string filename) {
        StreamReader reader = System.IO.File.OpenText(filename);

        int lineCount = 0;
        while(reader.Peek() >= 0) {
            string curLine = reader.ReadLine();
            if(RemoveSpaces(curLine).Length > ((ignoreEmptyLines) ? 0 : -1)) {
                lineCount++;
            }
        }

        if(lineCount <= 0) {
            reader.Close();
            return;
        }

        string dispName = filename;
        int lastSlash = dispName.LastIndexOf(@"\");
        if(lastSlash > -1) {
            dispName = dispName.Substring(lastSlash + 1);
        }

        stats.Add(new File(filename, dispName, lineCount));
        reader.Close();
    }
	
	public static string RemoveSpaces(string toRemove) {
        return toRemove.Trim().Replace(" ", "");
    }
}