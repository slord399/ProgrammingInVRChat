/**
 * MIT License
 *
 * Copyright (c) 2019 Naqtn
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
    
/**
 * VRChat Client Starter
 *
 *
 * This Unity extension starts VRChat client automatically after world publishing is completed.
 * You can go directly into published world.
 *
 * After installation, open window via Unity menu Window > VRC_Iwsd > Client Starter
 *
 * When "Auto Start" option is enabled, this closes VRChat SDK "Manage World in Browser" dialog.
 * Disenable it if you need to use that dialog.
 *
 * Other features
 * - Start client manually
 * - Open manage page at vrchat.com
 * - "Auto Start" feature works even if setting window is not opened
 *
 *
 * Written by naqtn (https://twitter.com/naqtn)
 * Hosted at https://github.com/naqtn/ProgrammingInVRChat
 * If you have defect reports or feature requests, please post to GitHub issue (https://github.com/naqtn/ProgrammingInVRChat/issues)
 *
 *
 * TODO hide "not logged in" warning when IsLoggedInWithCredentials becomes true (need?)
 * TODO show URL as copyable string (need?)
 * TODO preserve nonce and instance number and reasonably refresh, to meet players by multiple invoke
 * TODO Add non-VR selection feature (need directly starting client instead of Application.OpenURL())
 * TODO validate inputed ID string (cut from tail to be good if public URL, or use regex)
 * BUG no need to be logged for public access instance
 */
namespace Iwsd
{

    public class ClientStarterWindow : EditorWindow
    {

        [MenuItem("Window/VRC_Iwsd/Client Starter")]
        static void OpenClientStarterWindow()
        {
            EditorWindow.GetWindow<ClientStarterWindow>("VRC Client");
        }

        bool moreOptions = false;
        // "wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        string manualInputId = "(input world ID: wrld_xxx...)"; // I choiced not to be saved
        ClientStarter.Result result2 = new ClientStarter.Result(null, true, "");
            
        void OnGUI ()
        {
            var settings = ClientStarter.settings;
            var result1 = ClientStarter.lastResult;

            /// Label
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("VRChat Client Starter", new GUIStyle(){fontStyle = FontStyle.Bold});
            moreOptions = EditorGUILayout.ToggleLeft("more options", moreOptions);
            EditorGUILayout.EndHorizontal();
                           
            /// Settings
            EditorGUI.BeginChangeCheck();
            settings.startAfterPublished
                = EditorGUILayout.Toggle(new GUIContent("Auto Start", "Start client after publish completed"),
                                         settings.startAfterPublished);

            settings.worldAccessLevel1 = (ClientStarter.WorldAccessLevel)
                EditorGUILayout.EnumPopup("Access", settings.worldAccessLevel1);

            if (EditorGUI.EndChangeCheck()) {
                settings.Store();
            }
            if (moreOptions)
            {
                var rect = EditorGUILayout.GetControlRect(true);
                EditorGUI.PrefixLabel(rect, new GUIContent("World ID (read only)"));
                rect.x += EditorGUIUtility.labelWidth;
                rect.width -= EditorGUIUtility.labelWidth;
                GUI.enabled = false;
                EditorGUI.TextField(rect, "");
                GUI.enabled = true;
                EditorGUI.SelectableLabel(rect, result1.blueprintId);
                // EditorGUILayout.TextField("World ID (read only)", result.blueprintId);
            }
            
            /// Operation buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Operations");
            if (GUILayout.Button("Start Client",  GUILayout.ExpandWidth(false)))
            {
                var r = ClientStarter.TryToOpenLaunchURL(null, settings.worldAccessLevel1);
                ClientStarter.lastResult = r;
            }
            if (GUILayout.Button("Open Manage Page",  GUILayout.ExpandWidth(false)))
            {
                var r = ClientStarter.TryToOpenManageURL(null);
                ClientStarter.lastResult = r;
            }
            EditorGUILayout.EndHorizontal();

            //// Info
            EditorGUILayout.Space();
            if (!result1.IsSucceeded)
            {
                EditorGUILayout.HelpBox(result1.Value, MessageType.Warning);
            }

            /// Manual input ID
            if (moreOptions)
            {
                /// Section label
                GUILayout.Space(15);
                EditorGUILayout.LabelField(new GUIContent(" Start another world:",
                                                          "To start a world that doesn't relate to editing scene"),
                                           new GUIStyle(){fontStyle = FontStyle.Bold});
                EditorGUI.indentLevel++;

                /// Settings 2
                settings.worldAccessLevel2 = (ClientStarter.WorldAccessLevel)
                    EditorGUILayout.EnumPopup("Access", settings.worldAccessLevel2);

                manualInputId = EditorGUILayout.TextField("World ID", manualInputId);

                /// Operation buttons 2
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Operations");
                if (GUILayout.Button("Start Client",  GUILayout.ExpandWidth(false)))
                {
                    result2 = ClientStarter.TryToOpenLaunchURL(manualInputId, settings.worldAccessLevel2);
                }
                if (GUILayout.Button("Open Manage Page",  GUILayout.ExpandWidth(false)))
                {
                    result2 = ClientStarter.TryToOpenManageURL(manualInputId);
                }
                EditorGUILayout.EndHorizontal();

                /// Info 2
                EditorGUILayout.Space();
                if (!result2.IsSucceeded)
                {
                    EditorGUILayout.HelpBox(result2.Value, MessageType.Warning);
                }

                EditorGUI.indentLevel--;
            }

        }

    }
    ////////////////////////////////////////////////////////////////////////////////

    [InitializeOnLoad]
    public class ClientStarter {
        
        ////////////////////////////////////////////////////////////
        // sub structures

        public enum WorldAccessLevel
        {
            Public,      // access "", omit tailing ~ section
            FriendsPlus, // access "hidden"
            Friends,     // access "friends"
            InvitePlus,  // access "private", at last "~canRequestInvite"
            Invite,      // access "private"
        }

        public class Settings
        {
            public bool startAfterPublished;
            public WorldAccessLevel worldAccessLevel1;
            public WorldAccessLevel worldAccessLevel2;

    
            private const string startAfterPublished_key = "Iwsd.ClientStarter.startAfterPublished";
            private const string worldAccessLevel1_key = "Iwsd.ClientStarter.worldAccessLevel1";
            private const string worldAccessLevel2_key = "Iwsd.ClientStarter.worldAccessLevel2";

            internal static Settings Load()
            {
                var o = new Settings();
                o.startAfterPublished = EditorPrefs.GetBool(startAfterPublished_key, true);
                o.worldAccessLevel1 = (WorldAccessLevel)EditorPrefs.GetInt(worldAccessLevel1_key, (int)WorldAccessLevel.Friends);
                o.worldAccessLevel2 = (WorldAccessLevel)EditorPrefs.GetInt(worldAccessLevel2_key, (int)WorldAccessLevel.Friends);
                return o;
            }
            
            internal void Store()
            {
                EditorPrefs.SetBool(startAfterPublished_key, this.startAfterPublished);
                EditorPrefs.GetInt(worldAccessLevel1_key, (int)this.worldAccessLevel1);
                EditorPrefs.GetInt(worldAccessLevel2_key, (int)this.worldAccessLevel2);
            }
        }

        public class Result {
            public bool IsSucceeded;
            public string Value;
            public string blueprintId;
            
            public Result(Result result, bool isSucceeded, string value)
            {
                IsSucceeded = isSucceeded;
                Value = value;
                blueprintId = (result == null)? "---": result.blueprintId;
            }
        }

        ////////////////////////////////////////////////////////////
        // Auto start handling
        
        static public Settings settings;
        static public Result lastResult;
        
        static ClientStarter()
        {
            lastResult = new Result(null, true, "");
            settings = Settings.Load();

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += PublishPolling;
            // EditorApplication.modifierKeysChanged += PublishPolling; // For manual spike
        }

        // How many update call to next dialog polling
        private const int CHECK_CYCLE_ONUPDATE = 50;
        private const int CHECK_TRIAL_LIMIT = 20;

        static private int callCount = 0;
        static private int trialCount = CHECK_TRIAL_LIMIT + 1;

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                callCount = 0;
                trialCount = 0;
            }
        }

        private static void PublishPolling()
        {
            // Avoid calling FindObjectsOfTypeAll so often, to reduce CPU load 
            if ((CHECK_TRIAL_LIMIT < trialCount) || (++callCount % CHECK_CYCLE_ONUPDATE != 0))
            {
                return;
            }

            callCount = 0;
            trialCount++;

            // This option check must be after count up considering or it opens immediately when dialog opend and option turns on
            if (!settings.startAfterPublished)
            {
                return;
            }
                                   
            // Currently (VRCSDK-2019.06.25.21.13_Public) ContentUploadedDialog :
            // * appears after publish
            // * is used for world publishing only
            // * is restricted to one instance
            // so it's fit my purpose.
            var completeDialog = Resources.FindObjectsOfTypeAll(typeof(VRCSDK2.ContentUploadedDialog)) as EditorWindow[];
            if (completeDialog.Length != 0)
            {
                lastResult = TryToOpenLaunchURL(null, settings.worldAccessLevel1);
                if (lastResult.IsSucceeded)
                {
                    completeDialog[0].Close();
                }
            }

        }


        ////////////////////////////////////////////////////////////
        // Do start actions
        
        private static Result TryToOpen(Result url)
        {
            if (url.IsSucceeded)
            {
                var s = url.Value;
                Debug.Log("will OpenURL url='" + s + "'");
                Application.OpenURL(s);
            }
            else
            {
                Debug.LogWarning(url.Value);
            }

            return url;
        }

        public static Result TryToOpenManageURL(string id_opt)
        {
            return TryToOpen(ComposeManageURL(id_opt));
        }

        public static Result TryToOpenLaunchURL(string id_opt, WorldAccessLevel accessLevel)
        {
            return TryToOpen(ComposeLaunchURL(id_opt, accessLevel));
        }

        public static Result ExtractSceneBlueprintId(string id_opt)
        {
            if (id_opt != null)
            {
                var r = new Result(null, true, "");
                // TODO validate id_opt 
                r.blueprintId = id_opt;
                return r;
            }
            
            var vrcPipelineManager = Resources.FindObjectsOfTypeAll(typeof(VRC.Core.VRCPipelineManager)) as VRC.Core.VRCPipelineManager[];
            foreach (var pm in vrcPipelineManager)
            {
                // PrefabUtility.GetPrefabType returns None on Unity 2017.4.15f1, PrefabInstance on Unity 2017.4.28f1
                // And GetPrefabType is obsolete in Unity 2018.3.x. Use PrefabUtility.IsPartOfPrefabAsset instead
                PrefabType ptype = PrefabUtility.GetPrefabType(pm);
                if ((ptype == PrefabType.PrefabInstance) || (ptype == PrefabType.None))
                {
                    var blueprintId = pm.blueprintId;
                    if (blueprintId == null || blueprintId == "")
                    {
                        return new Result(null, false, "Not publishd yet? (blueprintId is empty)");
                    }

                    var r = new Result(null, true, "");
                    r.blueprintId = blueprintId;
                    return r;
                }
            }
            return new Result(null, false, "VRC_SceneDescriptor is missing? (vrcPipelineManager.Length=" + vrcPipelineManager.Length + ")");
        }
        

        public static Result ComposeLaunchURL(string id_opt, WorldAccessLevel accessLevel)
        {
            var bid = ExtractSceneBlueprintId(id_opt);
            if (!bid.IsSucceeded)
            {
                return bid;
            }
            var blueprintId = bid.blueprintId;

            if (!VRC.Core.APIUser.IsLoggedInWithCredentials)
            {
                return new Result(bid, false, "Not logged in. (Open 'VRChat SDK/Settings to check and try again' )");
            }

            var user = VRC.Core.APIUser.CurrentUser;
            if (user == null)
            {
                return new Result(bid, false, "user == null");
            }
            var userid = user.id;
            if (userid == null)
            {
                return new Result(bid, false, "user.id == null");
            }

            var nonce = Guid.NewGuid();
            var instno = new System.Random().Next(1000, 9000);

            var access = accessStringOf(accessLevel);

            // NOTE 'ref' should be other value. But API is not documented.
            //  "vrchat://launch?ref=vrchat.com&id={blueprintId}:{instno}~{access}({userid})~nonce({nonce}){option}";

            var url = "vrchat://launch?ref=vrchat.com&id=" + blueprintId + ":" + instno;
            if (accessLevel != WorldAccessLevel.Public)
            {
                url += "~" + access + "("+ userid + ")~nonce(" + nonce + ")";

                if (accessLevel == WorldAccessLevel.InvitePlus)
                {
                    url += "~canRequestInvite";
                }
            }

            return new Result(bid, true, url);
        }

        public static Result ComposeManageURL(string id_opt)
        {
            var bid = ExtractSceneBlueprintId(id_opt);
            if (bid.IsSucceeded)
            {
                // https://vrchat.com/home/world/{blueprintId}
                return new Result(bid, true, "https://vrchat.com/home/world/" + bid.blueprintId);
            }
            return bid;
        }


        private static string accessStringOf(WorldAccessLevel wal)
        {
            switch (wal)
            {
                case WorldAccessLevel.Public:      return "";
                case WorldAccessLevel.FriendsPlus: return "hidden";
                case WorldAccessLevel.Friends:     return "friends";
                case WorldAccessLevel.InvitePlus:  return "private";
                case WorldAccessLevel.Invite:      return "private";
                default:
                    throw new NotImplementedException("Not implemented for " + wal);
            }
        }
    }
}

#endif // UNITY_EDITOR
