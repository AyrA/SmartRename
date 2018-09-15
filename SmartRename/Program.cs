using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartRename
{
    /// <summary>
    /// Main Application Class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Extensions that get the fixed name
        /// </summary>
        public const string FIX_EXT = "mkv,avi,mp4";
        /// <summary>
        /// Extensions of files to delete
        /// </summary>
        public const string DEL_EXT = "nfo,lnk,url,sfv,txt,diz";
        /// <summary>
        /// Directories to delete
        /// </summary>
        public const string DEL_DIR = "sample,proof";
        /// <summary>
        /// Subtitles to fix
        /// </summary>
        public const string SUB_FIX = "idx,sub,srt";

        /// <summary>
        /// Represents a possible Action
        /// </summary>
        public struct SmartName
        {
            /// <summary>
            /// Current Name
            /// </summary>
            public string OldName;
            /// <summary>
            /// New Name
            /// </summary>
            /// <remarks>This is null for actions that don't rename</remarks>
            public string NewName;
            /// <summary>
            /// Action to perform
            /// </summary>
            public SmartAction Action;

            /// <summary>
            /// Perform the specified Action
            /// </summary>
            /// <returns><see cref="true"/>, if action successfully performed</returns>
            /// <remarks><see cref="SmartAction.None"/> will always return <see cref="false"/> </remarks>
            public bool ExecuteAction()
            {
                if (File.Exists(OldName))
                {
                    switch (Action)
                    {
                        case SmartAction.Delete:
                            try
                            {
                                File.Delete(OldName);
                            }
                            catch
                            {
                                return false;
                            }
                            return true;
                        case SmartAction.Rename:
                            try
                            {
                                File.Move(OldName, NewName);
                            }
                            catch
                            {
                                return false;
                            }
                            return true;
                        default:
                            return false;
                    }
                }
                if (Directory.Exists(OldName))
                {
                    switch (Action)
                    {
                        case SmartAction.Delete:
                            try
                            {
                                Directory.Delete(OldName, true);
                            }
                            catch
                            {
                                return false;
                            }
                            return true;
                        case SmartAction.Rename:
                            try
                            {
                                Directory.Move(OldName, NewName);
                            }
                            catch
                            {
                                return false;
                            }
                            return true;
                        default:
                            return false;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Action Type
        /// </summary>
        public enum SmartAction
        {
            /// <summary>
            /// Don't perform an action
            /// </summary>
            None,
            /// <summary>
            /// Rename File/Directory
            /// </summary>
            Rename,
            /// <summary>
            /// Delete File/Directory
            /// </summary>
            Delete
        }

        /// <summary>
        /// Main Entry Point
        /// </summary>
        /// <param name="args">Arguments</param>
        public static void Main(string[] args)
        {
#if DEBUG
            args = @"T:\Downloaded\*".Split(' ');
#endif
            //Check for help
            if (args.Length == 0 || args.Any(m => m == "/?" || m == "--help" || m == "-?"))
            {
                Console.Error.WriteLine(@"SmartRename <dir> [...]
Renames Files and directories of downloaded media files to something usable.
Also deletes unusable files.

dir   - Directory to rename
        multiple directories can be specified.
        You are prompted before each batch of actions.
        The specified directory must be the one with the unnecessarily stupid
        name

WARNING!
========
Only use if the download was for a video file.
Don't use for games or audio albums.
Carefully review the pending changes because they can't be undone.");
            }
            //Allow processing of multiple directories
            foreach (var arg in args.SelectMany(m => AyrA.IO.MaskMatch.Match(m, AyrA.IO.MatchType.Directory)))
            {
                Console.Error.WriteLine(arg);
                continue;
                var Actions = GetSmartNames(arg);
                if (Actions.Length > 0)
                {
                    //List all actions for the user to review them
                    foreach (var Action in Actions)
                    {
                        switch (Action.Action)
                        {
                            case SmartAction.None:
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Error.WriteLine("Ignore: {0}", ToShort(Action.OldName));
                                break;
                            case SmartAction.Rename:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Error.WriteLine("Rename: {0} --> {1}", ToShort(Action.OldName), ToShort(Action.NewName));
                                break;
                            case SmartAction.Delete:
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Error.WriteLine("Delete: {0}", ToShort(Action.OldName));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    Console.ResetColor();
                    if (YN("Execute these actions?"))
                    {
                        foreach (var Act in Actions.Where(m => m.Action != SmartAction.None))
                        {
                            if (Act.ExecuteAction())
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Error.WriteLine("{0} OK", Act.Action);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Error.WriteLine("{0} FAIL", Act.Action);
                                Console.Error.WriteLine("OldName: {0}", Act.OldName);
                                Console.Error.WriteLine("NewName: {0}", Act.NewName);
                            }
                        }
                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.Error.WriteLine($"No actions for {arg}");
                }
            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
        }

        /// <summary>
        /// Ask a Y/N question and wait for proper key
        /// </summary>
        /// <param name="Text">Text to print. It is appended by "[Y/N]: "</param>
        /// <param name="BeepOnError">Beep on wrong key press</param>
        /// <returns>Y=<see cref="true"/>,N=<see cref="false"/></returns>
        public static bool YN(string Text, bool BeepOnError = true)
        {
            Console.Error.Write("{0} [Y/N]: ", Text);
            while (true)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.Y:
                        Console.Error.WriteLine();
                        return true;
                    case ConsoleKey.N:
                        Console.Error.WriteLine();
                        return false;
                    default:
                        if (BeepOnError)
                        {
                            Console.Beep();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Gets a shorter name
        /// </summary>
        /// <param name="Dir">File/Directory name</param>
        /// <returns>File name plus parent directory name</returns>
        /// <remarks>This is probably rendering the name unusable</remarks>
        public static string ToShort(string Dir)
        {
            var Segments = Dir.Split(Path.DirectorySeparatorChar);
            return Segments[Segments.Length - 2] + Path.DirectorySeparatorChar + Segments.Last();
        }

        /// <summary>
        /// Gets all naming actions for a directory
        /// </summary>
        /// <param name="dirname">Main Directory</param>
        /// <returns>Pending Action List</returns>
        public static SmartName[] GetSmartNames(string dirname)
        {
            var Renames = new List<SmartName>();
            dirname = Path.GetFullPath(dirname);

            //List of all files
            var AllFiles = Directory.GetFiles(dirname, "*", SearchOption.AllDirectories);
            //List of all directories
            var AllDirs = Directory.GetDirectories(dirname, "*", SearchOption.AllDirectories);

            //This matches everything up to the first 4 digit group with a dot in front of them
            var R = new Regex(@"^(.+?)\.\d{4}");

            //Last segment of directory
            var segment = Path.GetFileName(dirname);

            //Arrays for constants
            var DeleteDirectories = DEL_DIR.Split(',');
            var DeleteExtensions = DEL_EXT.Split(',');
            var SubtitleExtensions = SUB_FIX.Split(',');
            var FixExtensions = FIX_EXT.Split(',');

            //Make sure the specified directory has a stupid name
            if (R.IsMatch(segment))
            {
                //List for subtitles, they are processed seperately
                var Subtitles = new List<string>();

                //Convert Stupid Name to Normal Name
                segment = R.Match(segment).Groups[1].Value.Replace('.', ' ').Trim();

                //Process files first
                foreach (var F in AllFiles)
                {
                    //Get extension without the dot
                    var ext = Path.GetExtension(F).Substring(1).ToLower();
                    //Check if file to delete
                    if (DeleteExtensions.Contains(ext))
                    {
                        //Delete this file
                        Renames.Add(new SmartName()
                        {
                            OldName = F,
                            Action = SmartAction.Delete
                        });
                    }
                    //Check if file to rename
                    else if (FixExtensions.Contains(ext))
                    {
                        //Rename this file to Normal Name
                        Renames.Add(new SmartName()
                        {
                            OldName = F,
                            NewName = Path.Combine(Path.GetDirectoryName(F), $"{segment}.{ext}"),
                            Action = SmartAction.Rename
                        });
                    }
                    //Special case for subtitles
                    else if (SubtitleExtensions.Contains(ext))
                    {
                        Subtitles.Add(F);
                    }
                    else
                    {
                        //This file is ignored
                        Renames.Add(new SmartName()
                        {
                            OldName = F,
                            Action = SmartAction.None
                        });
                    }
                }

                //Process subtitles. We process them seperately because of the suffix.
                //Because they are files, do them before directories
                if (Subtitles.Count > 0)
                {
                    //Get the base name (the part of the name all of them have in common)
                    //Subtitles usually end in a language code or "forced"
                    //This gets everything up to that point.
                    //If this breaks we can replace it with an extension stripper and then get the shortest name.
                    //This works because there is usually at least one subtitle file without any suffix.
                    //Ideally we would combine these two methods
                    var BaseLength = Subtitles.Max(m => Path.GetFileNameWithoutExtension(m).LastIndexOfAny(".-_".ToCharArray())) + 1;

                    foreach (var Sub in Subtitles)
                    {
                        //Rename action for subtitle
                        Renames.Add(new SmartName()
                        {
                            OldName = Sub,
                            NewName = Path.Combine(Path.GetDirectoryName(Sub), segment + Sub.Substring(Sub.LastIndexOf(Path.DirectorySeparatorChar) + BaseLength)),
                            Action = SmartAction.Rename
                        });
                    }
                }

                //Process directories
                foreach (var D in AllDirs)
                {
                    //We only have "Delete" actions for now
                    if (DeleteDirectories.Contains(Path.GetFileName(D).ToLower()))
                    {
                        Renames.Add(new SmartName()
                        {
                            OldName = D,
                            Action = SmartAction.Delete
                        });
                    }
                }

                //Add the main directory as last entry
                Renames.Add(new SmartName()
                {
                    OldName = dirname,
                    NewName = Path.Combine(Path.GetDirectoryName(dirname), segment),
                    Action = SmartAction.Rename
                });
            }
            return Renames.ToArray();
        }
    }
}
